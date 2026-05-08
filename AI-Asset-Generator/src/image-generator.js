/**
 * 图片生成器
 * 通过可配置的API接口调用文生图服务，直接生成游戏资源图片
 * 
 * 设计原则:
 *  - API完全由 config/api.json 驱动，不硬编码任何API假设
 *  - 支持自定义端点、请求模板、响应路径解析
 *  - 支持URL下载和Base64解码两种响应格式
 *  - 支持并发控制和重试机制
 */

const fs = require('fs');
const path = require('path');

let https, http, dotenvConfig;
try {
  https = require('https');
  http = require('http');
} catch (e) { /* Node built-ins */ }

// 加载 .env (可选依赖)
function loadEnv() {
  try {
    if (!dotenvConfig) {
      require('dotenv').config({ path: path.join(__dirname, '..', '.env') });
      dotenvConfig = true;
    }
  } catch (e) {
    console.warn('⚠️  未找到 .env 文件或 dotenv 未安装，请确保环境变量已手动设置');
    console.warn('   运行 npm install 安装依赖');
  }
}

// 路径常量
const CONFIG_DIR = path.join(__dirname, '..', 'config');
const API_CONFIG_PATH = path.join(CONFIG_DIR, 'api.json');
const IMAGES_OUTPUT_DIR = path.join(__dirname, '..', 'images');
const IMAGES_ITEMS_DIR = path.join(IMAGES_OUTPUT_DIR, 'items');
const IMAGES_UI_DIR = path.join(IMAGES_OUTPUT_DIR, 'ui');

/**
 * 确保目录存在
 */
function ensureDirectory(dirPath) {
  if (!fs.existsSync(dirPath)) {
    fs.mkdirSync(dirPath, { recursive: true });
    console.log(`📁 创建目录: ${dirPath}`);
  }
}

/**
 * 加载API配置
 */
function loadApiConfig() {
  try {
    const data = fs.readFileSync(API_CONFIG_PATH, 'utf8');
    return JSON.parse(data);
  } catch (error) {
    console.error(`❌ 加载API配置失败: ${error.message}`);
    console.error(`   请确保 ${API_CONFIG_PATH} 存在且格式正确`);
    process.exit(1);
  }
}

/**
 * 获取API密钥
 */
function getApiKey(config) {
  const envVar = config.apiKeyEnv || 'IMAGE_API_KEY';
  const key = process.env[envVar];
  if (!key || key === 'sk-your-api-key-here') {
    console.error(`❌ 未设置API密钥，请在 .env 文件中设置 ${envVar}`);
    console.error('   cp .env.example .env 然后编辑 .env 文件');
    process.exit(1);
  }
  return key;
}

/**
 * 模板替换 - 替换 {{VAR}} 占位符
 * @param {*} template - 可以是字符串或任意嵌套对象
 * @param {Object} variables - 变量映射
 * @param {boolean} removeUnfilled - 是否删除未填充的占位符key
 * @returns {*} 替换后的值
 */
function applyTemplate(template, variables, removeUnfilled = true) {
  if (typeof template === 'string') {
    let result = template;
    let hasUnfilled = false;
    for (const [key, value] of Object.entries(variables)) {
      const placeholder = `{{${key}}}`;
      if (result.includes(placeholder)) {
        result = result.replaceAll(placeholder, String(value));
      } else if (removeUnfilled && result.includes(`{{`)) {
        hasUnfilled = true;
      }
    }
    return result;
  }
  if (Array.isArray(template)) {
    return template.map(item => applyTemplate(item, variables, removeUnfilled));
  }
  if (template !== null && typeof template === 'object') {
    const result = {};
    for (const [key, value] of Object.entries(template)) {
      const newValue = applyTemplate(value, variables, removeUnfilled);
      if (removeUnfilled && typeof key === 'string' && key.startsWith('{{') && key.endsWith('}}')) {
        const varName = key.slice(2, -2);
        if (variables[varName] === undefined) continue;
        result[variables[varName]] = newValue;
      } else {
        result[key] = newValue;
      }
    }
    return result;
  }
  return template;
}

/**
 * 从对象中按路径提取值
 * 例如 "data.0.url" -> obj.data[0].url
 */
function getFieldByPath(obj, pathStr) {
  if (!obj || !pathStr) return undefined;
  const keys = pathStr.split('.');
  let current = obj;
  for (const key of keys) {
    if (current === null || current === undefined) return undefined;
    if (/^\d+$/.test(key)) {
      current = current[parseInt(key, 10)];
    } else {
      current = current[key];
    }
  }
  return current;
}

/**
 * 发起HTTP请求
 * @param {Object} config - API配置
 * @param {Object} requestData - 请求体
 * @returns {Promise<Object>}
 */
function makeRequest(config, requestData) {
  return new Promise((resolve, reject) => {
    const url = new URL(config.endpoint);
    const body = JSON.stringify(requestData);
    const isHttps = url.protocol === 'https:';
    const transport = isHttps ? https : http;

    const headers = { ...config.headers };
    
    // 添加认证
    const apiKey = getApiKey(config);
    if (config.authType === 'bearer' && config.authHeaderName) {
      headers[config.authHeaderName] = `Bearer ${apiKey}`;
    } else if (config.authType === 'header' && config.authHeaderName) {
      headers[config.authHeaderName] = apiKey;
    } else if (config.authType === 'query') {
      url.searchParams.set(config.authHeaderName || 'api_key', apiKey);
    }

    const options = {
      hostname: url.hostname,
      port: url.port || (isHttps ? 443 : 80),
      path: url.pathname + url.search,
      method: config.method || 'POST',
      headers: headers,
      timeout: config.timeout || 120000
    };

    const req = transport.request(options, (res) => {
      let data = '';
      res.on('data', (chunk) => { data += chunk; });
      res.on('end', () => {
        try {
          const json = JSON.parse(data);
          if (res.statusCode >= 200 && res.statusCode < 300) {
            resolve(json);
          } else {
            const errorMsg = json.error?.message || json.message || `HTTP ${res.statusCode}`;
            reject(new Error(`API请求失败 (${res.statusCode}): ${errorMsg}`));
          }
        } catch (e) {
          if (res.statusCode >= 200 && res.statusCode < 300 && data.length > 0) {
            resolve(data);
          } else {
            reject(new Error(`API请求失败 (${res.statusCode}): ${data.slice(0, 200)}`));
          }
        }
      });
    });

    req.on('timeout', () => {
      req.destroy();
      reject(new Error(`API请求超时 (${config.timeout}ms)`));
    });

    req.on('error', (err) => {
      reject(new Error(`网络请求失败: ${err.message}`));
    });

    req.write(body);
    req.end();
  });
}

/**
 * 发起HTTP GET请求 (用于异步轮询)
 */
function makeGetRequest(urlStr, config) {
  return new Promise((resolve, reject) => {
    const u = new URL(urlStr);
    const transport = u.protocol === 'https:' ? https : http;
    const headers = {};
    const apiKey = getApiKey(config);
    if (config.authType === 'bearer' && config.authHeaderName) {
      headers[config.authHeaderName] = `Bearer ${apiKey}`;
    }
    const options = {
      hostname: u.hostname,
      port: u.port || (u.protocol === 'https:' ? 443 : 80),
      path: u.pathname + u.search,
      method: 'GET',
      headers,
      timeout: config.timeout || 60000
    };
    transport.get(options, (res) => {
      let data = '';
      res.on('data', (c) => { data += c; });
      res.on('end', () => {
        try {
          const json = JSON.parse(data);
          if (res.statusCode >= 200 && res.statusCode < 300) {
            resolve(json);
          } else {
            reject(new Error(`轮询失败 (${res.statusCode}): ${json.message || data.slice(0, 200)}`));
          }
        } catch (e) {
          reject(new Error(`轮询失败 (${res.statusCode}): ${data.slice(0, 200)}`));
        }
      });
    }).on('error', (err) => reject(new Error(`轮询网络错误: ${err.message}`)));
  });
}

/**
 * 从URL下载图片
 */
function downloadImage(url, filePath) {
  return new Promise((resolve, reject) => {
    const transport = url.startsWith('https') ? https : http;
    
    transport.get(url, (res) => {
      if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
        downloadImage(res.headers.location, filePath).then(resolve).catch(reject);
        return;
      }
      if (res.statusCode !== 200) {
        reject(new Error(`下载图片失败: HTTP ${res.statusCode}`));
        return;
      }
      const fileStream = fs.createWriteStream(filePath);
      res.pipe(fileStream);
      fileStream.on('finish', () => {
        fileStream.close();
        resolve();
      });
      fileStream.on('error', reject);
    }).on('error', reject);
  });
}

/**
 * 深合并对象
 */
function deepMerge(target, source) {
  const result = { ...target };
  for (const [key, value] of Object.entries(source)) {
    if (value !== null && typeof value === 'object' && !Array.isArray(value) &&
        target[key] !== null && typeof target[key] === 'object' && !Array.isArray(target[key])) {
      result[key] = deepMerge(target[key], value);
    } else {
      result[key] = value;
    }
  }
  return result;
}

/**
 * 轮询异步任务直到完成
 * @param {Object} config
 * @param {string} taskId
 * @returns {Promise<Object>} 最终响应
 */
async function pollAsyncTask(config, taskId) {
  const pollUrl = (config.pollEndpoint || '').replace('{{TASK_ID}}', taskId);
  const deadline = Date.now() + (config.maxPollTime || 120000);
  const interval = config.pollInterval || 5000;

  while (Date.now() < deadline) {
    await new Promise(r => setTimeout(r, interval));
    const response = await makeGetRequest(pollUrl, config);
    const status = getFieldByPath(response, config.taskStatusField || 'output.task_status');

    if (status === 'SUCCEEDED') {
      return response;
    }
    if (status === 'FAILED' || status === 'CANCELED') {
      const msg = getFieldByPath(response, 'output.message') || getFieldByPath(response, 'message') || '';
      throw new Error(`异步任务${status}: ${msg}`);
    }
  }
  throw new Error(`异步任务轮询超时 (${config.maxPollTime}ms)`);
}

/**
 * 生成单张图片
 * @param {string} prompt - AI绘画提示词
 * @param {Object} options - 选项 { size, negativePrompt, fileName, outputDir, requestOverrides }
 * @returns {Promise<Object>} { success, filePath?, error? }
 */
async function generateImage(prompt, options = {}) {
  const config = loadApiConfig();
  let size = options.size || config.defaultSize || '1024x1024';
  const negativePrompt = options.negativePrompt || config.defaultNegativePrompt || '';
  const ext = options.imageExtension || config.imageExtension || 'png';
  const outputDir = options.outputDir || IMAGES_ITEMS_DIR;

  // 规格化尺寸分隔符: 统一将 WxH 转为 W*H (百炼API格式), W*H 保持不变
  size = size.replace(/(\d+)x(\d+)/i, '$1*$2');

  ensureDirectory(IMAGES_OUTPUT_DIR);
  ensureDirectory(outputDir);

  // 构建请求变量
  const variables = {
    PROMPT: prompt,
    NEGATIVE_PROMPT: negativePrompt,
    SIZE: size
  };

  // 构建请求体
  let requestBody = applyTemplate(config.requestBody, variables);
  
  // 合并请求覆盖
  if (options.requestOverrides) {
    requestBody = deepMerge(requestBody, options.requestOverrides);
  }

  // 重试逻辑
  const maxRetries = config.retryCount || 2;
  const retryDelay = config.retryDelay || 3000;
  let lastError;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    if (attempt > 0) {
      console.log(`   🔄 重试 ${attempt}/${maxRetries}... (${retryDelay}ms后)`);
      await new Promise(r => setTimeout(r, retryDelay * attempt));
    }

    try {
      let response;
      
      if (config.protocol === 'async') {
        // 异步流程: 创建任务 → 轮询 → 获取结果
        response = await makeRequest(config, requestBody);
        const taskId = getFieldByPath(response, config.taskIdField || 'output.task_id');
        if (!taskId) {
          throw new Error(`未能获取 task_id，响应: ${JSON.stringify(response).slice(0, 200)}`);
        }
        response = await pollAsyncTask(config, taskId);
      } else {
        // 同步流程
        response = await makeRequest(config, requestBody);
      }
      
      // 从响应中提取图片数据
      const imageField = config.responseImageField || 'data.0.url';
      const imageData = getFieldByPath(response, imageField);

      if (!imageData) {
        throw new Error(`无法从响应中提取图片数据，检查 config/api.json 中的 responseImageField 配置 (当前: ${imageField})`);
      }

      const format = config.responseImageFormat || 'url';
      const fileName = options.fileName || `image_${Date.now()}.${ext}`;
      const filePath = path.join(outputDir, fileName);

      if (format === 'url') {
        // 从URL下载图片
        await downloadImage(imageData, filePath);
      } else if (format === 'base64' || format === 'b64_json') {
        // Base64解码
        const buffer = Buffer.from(imageData, 'base64');
        fs.writeFileSync(filePath, buffer);
      } else {
        throw new Error(`不支持的响应图片格式: ${format}`);
      }

      return { success: true, filePath, size: fs.statSync(filePath).size };
      
    } catch (error) {
      lastError = error;
      if (attempt === maxRetries) break;
    }
  }

  return { success: false, error: lastError?.message || '未知错误' };
}

/**
 * 批量生成图片（带并发控制）
 * @param {Array} tasks - 任务数组 [{ prompt, options }]
 * @param {number} concurrency - 并发数
 * @param {Function} onProgress - 进度回调 (completed, total, result)
 * @returns {Promise<Object>} { successCount, failCount, results }
 */
async function generateImagesBatch(tasks, concurrency, onProgress) {
  const config = loadApiConfig();
  const maxConcurrency = concurrency || config.concurrency || 2;
  
  const results = [];
  let completed = 0;
  let index = 0;

  async function worker() {
    while (index < tasks.length) {
      const i = index++;
      const task = tasks[i];
      const result = await generateImage(task.prompt, task.options);
      results[i] = result;
      completed++;
      if (onProgress) {
        onProgress(completed, tasks.length, result);
      }
    }
  }

  const workers = Array(Math.min(maxConcurrency, tasks.length))
    .fill(null)
    .map(() => worker());

  await Promise.all(workers);

  const successCount = results.filter(r => r && r.success).length;
  const failCount = results.filter(r => r && !r.success).length;

  return { successCount, failCount, results };
}

/**
 * 从已有的prompt文件读取并生成图片
 * @param {string} promptFilePath - prompt .txt 文件路径
 * @param {Object} options - 生成选项
 * @returns {Promise<Object>}
 */
async function generateImageFromPromptFile(promptFilePath, options = {}) {
  try {
    const prompt = fs.readFileSync(promptFilePath, 'utf8').trim();
    if (!prompt) {
      return { success: false, error: `Prompt文件为空: ${promptFilePath}` };
    }
    
    // 根据路径推断输出目录和文件名
    const parsedPath = path.parse(promptFilePath);
    const isUI = promptFilePath.includes('ui');
    const outputDir = options.outputDir || (isUI ? IMAGES_UI_DIR : IMAGES_ITEMS_DIR);
    const fileName = options.fileName || `${parsedPath.name}.png`;
    
    return await generateImage(prompt, { ...options, fileName, outputDir });
  } catch (error) {
    return { success: false, error: `读取Prompt文件失败: ${error.message}` };
  }
}

// 初始化
function init() {
  loadEnv();
  ensureDirectory(IMAGES_OUTPUT_DIR);
  ensureDirectory(IMAGES_ITEMS_DIR);
  ensureDirectory(IMAGES_UI_DIR);
}

module.exports = {
  init,
  generateImage,
  generateImagesBatch,
  generateImageFromPromptFile,
  loadApiConfig,
  IMAGES_OUTPUT_DIR,
  IMAGES_ITEMS_DIR,
  IMAGES_UI_DIR
};
