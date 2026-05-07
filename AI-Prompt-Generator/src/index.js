/**
 * AI Prompt Generator - 入口文件
 * 末世生存合成游戏 - AI绘画Prompt生成 + 图片生成工具
 * 
 * 使用方法:
 *   npm run generate -- --item 净水                  # 生成单个物品prompt
 *   npm run generate -- --item 净水 -g               # 生成单个物品prompt + 图片
 *   npm run generate -- --level 1                    # 按等级生成prompt
 *   npm run generate -- --line 水源                  # 按合成线生成prompt
 *   npm run generate -- --all-items                  # 生成所有物品prompt
 *   npm run generate -- --ui                         # 生成UI资源prompt
 *   npm run generate -- --all                        # 生成全部资源prompt
 *   npm run generate -- --all-items -g               # 生成所有物品prompt + 图片
 *   npm run generate -- --all -g --size 512x512      # 生成全部 + 图片(512尺寸)
 *   npm run generate -- --image-from-prompt <file>   # 从已有prompt文件生成图片
 */

const path = require('path');
const generator = require('./generator');

// 解析命令行参数
function parseArgs() {
  const args = process.argv.slice(2);
  const options = {
    item: null,
    level: null,
    line: null,
    allItems: false,
    ui: false,
    all: false,
    generateImage: false,
    size: null,
    concurrency: null,
    imageFromPrompt: null
  };
  
  for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    const nextArg = args[i + 1];
    
    switch (arg) {
      case '--item':
      case '-i':
        if (nextArg && !nextArg.startsWith('--')) {
          options.item = nextArg;
          i++;
        }
        break;
        
      case '--level':
      case '-l':
        if (nextArg && !nextArg.startsWith('--')) {
          options.level = parseInt(nextArg, 10);
          i++;
        }
        break;
        
      case '--line':
      case '-L':
        if (nextArg && !nextArg.startsWith('--')) {
          options.line = nextArg;
          i++;
        }
        break;
        
      case '--all-items':
      case '-a':
        options.allItems = true;
        break;
        
      case '--ui':
      case '-u':
        options.ui = true;
        break;
        
      case '--all':
        options.all = true;
        break;
        
      case '--generate-image':
      case '-g':
        options.generateImage = true;
        break;
        
      case '--size':
      case '-s':
        if (nextArg && !nextArg.startsWith('--')) {
          options.size = nextArg;
          i++;
        }
        break;
        
      case '--concurrency':
      case '-c':
        if (nextArg && !nextArg.startsWith('--')) {
          options.concurrency = parseInt(nextArg, 10);
          i++;
        }
        break;
        
      case '--image-from-prompt':
        if (nextArg && !nextArg.startsWith('--')) {
          options.imageFromPrompt = nextArg;
          i++;
        }
        break;
        
      case '--help':
      case '-h':
        printHelp();
        process.exit(0);
        
      case '--version':
      case '-v':
        console.log('AI Prompt Generator v2.0.0');
        process.exit(0);
    }
  }
  
  return options;
}

// 打印帮助信息
function printHelp() {
  console.log(`
🎮 AI Prompt Generator - 末世生存合成 (v2.0)
================================================

使用方法:
  npm run generate -- [选项]

Prompt生成选项:
  --item <名称>       生成单个物品的prompt
  --level <等级>      按等级生成prompt (1-10)
  --line <名称>       按合成线生成prompt
  --all-items         生成所有物品的prompt
  --ui                生成UI资源的prompt
  --all               生成所有资源 (物品 + UI)

图片生成选项:
  --generate-image, -g     同时生成图片 (需配置API)
  --size, -s <尺寸>        指定图片尺寸 (默认: 1024x1024)
                            可用: 512x512, 768x768, 1024x1024, 1024x576, 576x1024
  --concurrency, -c <N>    并发请求数 (默认: 2)
  --image-from-prompt <file> 从已有prompt文件生成图片

其他:
  --help, -h          显示帮助信息
  --version, -v       显示版本信息

示例:
  npm run generate -- --item 净水              # 只生成prompt
  npm run generate -- --item 净水 -g           # 生成prompt + 图片
  npm run generate -- --all-items -g           # 批量生成所有物品图片
  npm run generate -- --all -g --size 512x512  # 全部资源 + 512尺寸
  npm run generate -- --image-from-prompt prompts/items/water_1.txt

配置: 编辑 config/api.json 设置API端点，编辑 .env 设置API Key

合成线列表:
  水源, 食物, 工具, 住所, 医疗, 能源, 知识, 希望, 探索
`);
}

/**
 * 构建图片生成选项
 */
function buildImageOptions(options) {
  const imgOpts = {};
  if (options.size) imgOpts.size = options.size;
  if (options.concurrency) imgOpts.concurrency = options.concurrency;
  return imgOpts;
}

/**
 * 主函数
 */
async function main() {
  console.log('========================================');
  console.log('🎮 AI Prompt Generator - 末世生存合成 v2.0');
  console.log('========================================\n');
  
  generator.init();
  
  const options = parseArgs();
  
  // --image-from-prompt: 从已有prompt文件生成图片
  if (options.imageFromPrompt) {
    const imgOpts = buildImageOptions(options);
    const result = await generator.generateImageFromPromptFile(options.imageFromPrompt, imgOpts);
    if (result.success) {
      console.log(`\n✅ 图片已生成: ${result.filePath}`);
      console.log(`   大小: ${(result.size / 1024).toFixed(1)} KB`);
    } else {
      console.log(`\n❌ 生成失败: ${result.error}`);
    }
    return;
  }
  
  // 图片生成模式
  if (options.generateImage) {
    const imgOptions = buildImageOptions(options);
    let result;
    
    if (options.all) {
      result = await generator.generateAllWithImages(imgOptions);
    } else if (options.allItems) {
      result = await generator.generateAllItemsWithImages(imgOptions);
    } else if (options.ui) {
      result = await generator.generateUIWithImages(imgOptions);
    } else if (options.item) {
      result = await generator.generateSingleItemWithImage(options.item, imgOptions);
      if (result.success) {
        console.log(`\n✅ 已生成物品 "${result.item.name}" 的Prompt + 图片`);
        console.log(`   等级: ${result.item.level}`);
        if (result.image.success) {
          console.log(`   图片: ${result.image.filePath}`);
          console.log(`   大小: ${(result.image.size / 1024).toFixed(1)} KB`);
        } else {
          console.log(`   ❌ 图片生成失败: ${result.image.error}`);
        }
      } else {
        console.log(`\n❌ ${result.message}`);
      }
    } else if (options.level !== null) {
      // 按等级 + 图片生成未单独实现，走全量收集过滤
      console.log(`⚠️  按等级生成图片暂不直接支持，请使用 --all-items -g 生成全部`);
      console.log(`   或者先生成prompt: npm run generate -- --level ${options.level}`);
      console.log(`   再从prompt文件生成图片: npm run generate -- --image-from-prompt <file>`);
    } else if (options.line) {
      // 按合成线 + 图片生成：收集该合成线所有物品
      const config = require(path.join(__dirname, '..', 'config', 'items.json'));
      const lineData = config.lines[options.line];
      if (!lineData) {
        console.log(`\n❌ 未找到合成线: ${options.line}`);
        return;
      }
      console.log(`📂 合成线: ${lineData.name}\n`);
      let successCount = 0;
      let failCount = 0;
      for (const item of lineData.items) {
        const r = await generator.generateSingleItemWithImage(item.name, imgOptions);
        if (r.success && r.image.success) {
          console.log(`   ✅ ${item.name} -> ${r.image.filePath}`);
          successCount++;
        } else {
          console.log(`   ❌ ${item.name}: ${r.image?.error || r.message}`);
          failCount++;
        }
      }
      console.log(`\n📊 完成: ${successCount} 成功 / ${failCount} 失败`);
    } else {
      printHelp();
    }
    return;
  }
  
  // 纯Prompt生成模式 (原逻辑保持不变)
  let result;
  
  if (options.all) {
    result = generator.generateAll();
  } else if (options.allItems) {
    result = generator.generateAllItems();
  } else if (options.ui) {
    result = generator.generateUI();
  } else if (options.item) {
    result = generator.generateSingleItem(options.item);
    if (result.success) {
      console.log(`\n✅ 已生成物品 "${result.item.name}" 的Prompt`);
      console.log(`   等级: ${result.item.level}`);
      console.log(`   描述: ${result.item.description}`);
    } else {
      console.log(`\n❌ ${result.message}`);
      console.log('   使用 --help 查看帮助');
    }
  } else if (options.level !== null) {
    if (isNaN(options.level) || options.level < 1 || options.level > 10) {
      console.log('❌ 等级必须在 1-10 之间');
      process.exit(1);
    }
    result = generator.generateByLevel(options.level);
    console.log(`\n✅ 已生成 ${result.count} 个等级${options.level}的物品Prompt`);
  } else if (options.line) {
    result = generator.generateByLine(options.line);
    if (result.success) {
      console.log(`\n✅ 已生成合成线 "${result.lineName}" 的 ${result.count} 个物品Prompt`);
    } else {
      console.log(`\n❌ ${result.message}`);
      console.log('   可用合成线: 水源, 食物, 工具, 住所, 医疗, 能源, 知识, 希望, 探索');
    }
  } else {
    printHelp();
    process.exit(0);
  }
}

// 运行主函数
main().catch(err => {
  console.error(`\n❌ 运行错误: ${err.message}`);
  process.exit(1);
});
