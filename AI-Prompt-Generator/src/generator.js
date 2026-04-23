/**
 * Prompt生成器核心逻辑
 * 负责加载配置、生成prompt、保存文件
 */

const fs = require('fs');
const path = require('path');
const { generateItemPrompt, generateItemPromptSimple, generateUIPrompt } = require('./templates');

// 配置路径
const CONFIG_DIR = path.join(__dirname, '..', 'config');
const ITEMS_CONFIG_PATH = path.join(CONFIG_DIR, 'items.json');
const UI_CONFIG_PATH = path.join(CONFIG_DIR, 'ui.json');
const OUTPUT_DIR = path.join(__dirname, '..', 'prompts');
const ITEMS_OUTPUT_DIR = path.join(OUTPUT_DIR, 'items');
const UI_OUTPUT_DIR = path.join(OUTPUT_DIR, 'ui');

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
 * 加载物品配置
 */
function loadItemsConfig() {
  try {
    const data = fs.readFileSync(ITEMS_CONFIG_PATH, 'utf8');
    return JSON.parse(data);
  } catch (error) {
    console.error(`❌ 加载物品配置失败: ${error.message}`);
    process.exit(1);
  }
}

/**
 * 加载UI配置
 */
function loadUIConfig() {
  try {
    if (fs.existsSync(UI_CONFIG_PATH)) {
      const data = fs.readFileSync(UI_CONFIG_PATH, 'utf8');
      return JSON.parse(data);
    }
    return getDefaultUIConfig();
  } catch (error) {
    console.error(`❌ 加载UI配置失败: ${error.message}`);
    return getDefaultUIConfig();
  }
}

/**
 * 获取默认UI配置
 */
function getDefaultUIConfig() {
  return {
    elements: [
      { type: "button", name: "主按钮", description: "游戏主界面按钮" },
      { type: "button", name: "次按钮", description: "次要操作按钮" },
      { type: "panel", name: "面板", description: "游戏面板背景" },
      { type: "progressBar", name: "进度条", description: "体力/经验进度条" },
      { type: "background", name: "背景", description: "游戏背景纹理" },
      { type: "icon", name: "图标", description: "通用UI图标" },
      { type: "border", name: "边框", description: "装饰边框" }
    ]
  };
}

/**
 * 保存prompt到文件
 */
function savePrompt(filePath, content) {
  ensureDirectory(path.dirname(filePath));
  fs.writeFileSync(filePath, content, 'utf8');
  console.log(`✅ 已保存: ${filePath}`);
}

/**
 * 生成单个物品的prompt
 */
function generateSingleItem(itemName) {
  const config = loadItemsConfig();
  
  // 搜索所有合成线
  for (const [lineKey, lineData] of Object.entries(config.lines)) {
    const item = lineData.items.find(i => i.name === itemName);
    if (item) {
      const prompt = generateItemPrompt(item, lineData.theme);
      const fileName = `${item.id}.txt`;
      const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
      savePrompt(filePath, prompt);
      return { success: true, item, line: lineData };
    }
  }
  
  // 搜索特殊物品
  const specialItem = config.specialItems.find(i => i.name === itemName);
  if (specialItem) {
    const prompt = generateItemPrompt(specialItem, "特殊物品");
    const fileName = `${specialItem.id}.txt`;
    const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
    savePrompt(filePath, prompt);
    return { success: true, item: specialItem, line: null };
  }
  
  // 搜索跨线物品
  const crossItem = config.crossLineItems.find(i => i.name === itemName);
  if (crossItem) {
    const prompt = generateItemPrompt(crossItem, "跨线合成");
    const fileName = `${crossItem.id}.txt`;
    const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
    savePrompt(filePath, prompt);
    return { success: true, item: crossItem, line: null };
  }
  
  return { success: false, message: `未找到物品: ${itemName}` };
}

/**
 * 按等级生成物品prompt
 */
function generateByLevel(level) {
  const config = loadItemsConfig();
  let count = 0;
  
  // 遍历所有合成线
  for (const [lineKey, lineData] of Object.entries(config.lines)) {
    for (const item of lineData.items) {
      if (item.level === level) {
        const prompt = generateItemPrompt(item, lineData.theme);
        const fileName = `${item.id}.txt`;
        const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
        savePrompt(filePath, prompt);
        count++;
      }
    }
  }
  
  // 遍历特殊物品
  for (const item of config.specialItems) {
    if (item.level === level) {
      const prompt = generateItemPrompt(item, "特殊物品");
      const fileName = `${item.id}.txt`;
      const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
      savePrompt(filePath, prompt);
      count++;
    }
  }
  
  // 遍历跨线物品
  for (const item of config.crossLineItems) {
    if (item.level === level) {
      const prompt = generateItemPrompt(item, "跨线合成");
      const fileName = `${item.id}.txt`;
      const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
      savePrompt(filePath, prompt);
      count++;
    }
  }
  
  return { success: true, count };
}

/**
 * 按合成线生成物品prompt
 */
function generateByLine(lineName) {
  const config = loadItemsConfig();
  
  // 查找合成线
  const lineData = config.lines[lineName];
  if (!lineData) {
    return { success: false, message: `未找到合成线: ${lineName}` };
  }
  
  let count = 0;
  for (const item of lineData.items) {
    const prompt = generateItemPrompt(item, lineData.theme);
    const fileName = `${item.id}.txt`;
    const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
    savePrompt(filePath, prompt);
    count++;
  }
  
  return { success: true, count, lineName };
}

/**
 * 生成所有物品的prompt
 */
function generateAllItems() {
  const config = loadItemsConfig();
  let count = 0;
  
  console.log('📦 开始生成所有物品Prompt...\n');
  
  // 遍历所有合成线
  for (const [lineKey, lineData] of Object.entries(config.lines)) {
    console.log(`\n📂 合成线: ${lineData.name} (${lineData.theme})`);
    for (const item of lineData.items) {
      const prompt = generateItemPrompt(item, lineData.theme);
      const fileName = `${item.id}.txt`;
      const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
      savePrompt(filePath, prompt);
      count++;
    }
  }
  
  // 特殊物品
  console.log(`\n📂 特殊物品`);
  for (const item of config.specialItems) {
    const prompt = generateItemPrompt(item, "特殊物品");
    const fileName = `${item.id}.txt`;
    const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
    savePrompt(filePath, prompt);
    count++;
  }
  
  // 跨线物品
  console.log(`\n📂 跨线合成物品`);
  for (const item of config.crossLineItems) {
    const prompt = generateItemPrompt(item, "跨线合成");
    const fileName = `${item.id}.txt`;
    const filePath = path.join(ITEMS_OUTPUT_DIR, fileName);
    savePrompt(filePath, prompt);
    count++;
  }
  
  console.log(`\n🎉 共生成 ${count} 个物品Prompt`);
  return { success: true, count };
}

/**
 * 生成UI资源的prompt
 */
function generateUI() {
  const config = loadUIConfig();
  let count = 0;
  
  console.log('🎨 开始生成UI资源Prompt...\n');
  
  for (const element of config.elements) {
    const prompt = generateUIPrompt(element.type, {
      color: element.color,
      size: element.size,
      custom: element.description
    });
    
    const fileName = `${element.type}_${element.name}.txt`;
    const filePath = path.join(UI_OUTPUT_DIR, fileName);
    savePrompt(filePath, prompt);
    count++;
  }
  
  console.log(`\n🎉 共生成 ${count} 个UI Prompt`);
  return { success: true, count };
}

/**
 * 生成所有资源 (物品 + UI)
 */
function generateAll() {
  console.log('🚀 开始生成所有资源...\n');
  
  const itemResult = generateAllItems();
  const uiResult = generateUI();
  
  console.log('\n========================================');
  console.log('📊 生成统计:');
  console.log(`   物品Prompt: ${itemResult.count} 个`);
  console.log(`   UI Prompt: ${uiResult.count} 个`);
  console.log(`   总计: ${itemResult.count + uiResult.count} 个`);
  console.log('========================================\n');
  
  return { 
    success: true, 
    itemCount: itemResult.count, 
    uiCount: uiResult.count 
  };
}

// 初始化输出目录
function init() {
  ensureDirectory(OUTPUT_DIR);
  ensureDirectory(ITEMS_OUTPUT_DIR);
  ensureDirectory(UI_OUTPUT_DIR);
}

// 导出模块
module.exports = {
  init,
  generateSingleItem,
  generateByLevel,
  generateByLine,
  generateAllItems,
  generateUI,
  generateAll
};