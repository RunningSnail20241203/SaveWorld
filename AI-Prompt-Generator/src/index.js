/**
 * AI Prompt Generator - 入口文件
 * 末世生存合成游戏 - AI绘画Prompt生成工具
 * 
 * 使用方法:
 *   npm run generate -- --item 净水           # 生成单个物品prompt
 *   npm run generate -- --level 1             # 按等级生成
 *   npm run generate -- --line 水源           # 按合成线生成
 *   npm run generate -- --all-items           # 生成所有物品
 *   npm run generate -- --ui                   # 生成UI资源
 *   npm run generate -- --all                  # 生成全部资源
 */

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
    all: false
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
        
      case '--help':
      case '-h':
        printHelp();
        process.exit(0);
        
      case '--version':
      case '-v':
        console.log('AI Prompt Generator v1.0.0');
        process.exit(0);
    }
  }
  
  return options;
}

// 打印帮助信息
function printHelp() {
  console.log(`
🎮 AI Prompt Generator - 末世生存合成
========================================

使用方法:
  npm run generate -- [选项]

选项:
  --item <名称>     生成单个物品的prompt
  --level <等级>    按等级生成prompt (1-10)
  --line <名称>     按合成线生成prompt
  --all-items       生成所有物品的prompt
  --ui              生成UI资源的prompt
  --all             生成所有资源 (物品 + UI)
  --help, -h        显示帮助信息
  --version, -v     显示版本信息

示例:
  npm run generate -- --item 净水
  npm run generate -- --level 1
  npm run generate -- --line 水源
  npm run generate -- --all-items
  npm run generate -- --ui
  npm run generate -- --all

合成线列表:
  水源, 食物, 工具, 住所, 医疗, 能源, 知识, 希望, 探索
`);
}

// 主函数
function main() {
  console.log('========================================');
  console.log('🎮 AI Prompt Generator - 末世生存合成');
  console.log('========================================\n');
  
  // 初始化目录
  generator.init();
  
  // 解析参数
  const options = parseArgs();
  
  // 执行生成
  let result;
  
  if (options.all) {
    // 生成所有资源
    result = generator.generateAll();
  } else if (options.allItems) {
    // 生成所有物品
    result = generator.generateAllItems();
  } else if (options.ui) {
    // 生成UI资源
    result = generator.generateUI();
  } else if (options.item) {
    // 生成单个物品
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
    // 按等级生成
    if (isNaN(options.level) || options.level < 1 || options.level > 10) {
      console.log('❌ 等级必须在 1-10 之间');
      process.exit(1);
    }
    result = generator.generateByLevel(options.level);
    console.log(`\n✅ 已生成 ${result.count} 个等级${options.level}的物品Prompt`);
  } else if (options.line) {
    // 按合成线生成
    result = generator.generateByLine(options.line);
    if (result.success) {
      console.log(`\n✅ 已生成合成线 "${result.lineName}" 的 ${result.count} 个物品Prompt`);
    } else {
      console.log(`\n❌ ${result.message}`);
      console.log('   可用合成线: 水源, 食物, 工具, 住所, 医疗, 能源, 知识, 希望, 探索');
    }
  } else {
    // 没有参数，显示帮助
    printHelp();
    process.exit(0);
  }
}

// 运行主函数
main();