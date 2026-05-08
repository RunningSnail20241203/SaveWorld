/**
 * 批量图片生成脚本
 * 一键生成全部游戏资源图片，按元素类型自动选择最佳尺寸
 *
 * 用法:
 *   npm run batch                           # 生成全部 (物品 + UI)
 *   npm run batch -- --type items           # 只生成物品
 *   npm run batch -- --type ui              # 只生成UI
 *   npm run batch -- --type all -y          # 全部, 跳过确认
 *   npm run batch -- --concurrency 4 --yes  # 4并发, 跳过确认
 *   npm run batch -- --dry-run              # 预览, 不实际生成
 *   npm run batch -- --limit 5              # 只生成前5个 (测试用)
 *   npm run batch -- --size 1280*1280       # 强制覆盖所有尺寸
 *   npm run batch -- --retry -y             # 只重试上次失败的任务
 */

const fs = require('fs');
const path = require('path');
const readline = require('readline');
const generator = require('./generator');
const { loadApiConfig, generateImagesBatch } = require('./image-generator');

const FAILED_PATH = path.join(__dirname, '..', 'images', '.failed.json');

function parseArgs() {
  const args = process.argv.slice(2);
  const opts = { type: 'all', yes: false, dryRun: false, concurrency: null, limit: null, size: null, retry: false };
  for (let i = 0; i < args.length; i++) {
    const a = args[i];
    const n = args[i + 1];
    switch (a) {
      case '--type': if (n && !n.startsWith('--')) { opts.type = n; i++; } break;
      case '--yes': case '-y': opts.yes = true; break;
      case '--dry-run': opts.dryRun = true; break;
      case '--concurrency': case '-c': if (n && !n.startsWith('--')) { opts.concurrency = parseInt(n, 10); i++; } break;
      case '--limit': if (n && !n.startsWith('--')) { opts.limit = parseInt(n, 10); i++; } break;
      case '--size': case '-s': if (n && !n.startsWith('--')) { opts.size = n; i++; } break;
      case '--retry': case '-r': opts.retry = true; break;
      case '--help': case '-h': printHelp(); process.exit(0); break;
    }
  }
  return opts;
}

function printHelp() {
  console.log(`
🚀 批量图片生成器 - 一键生成全部游戏资源

用法:
  npm run batch -- [选项]

选项:
  --type <items|ui|all>    生成类型 (默认: all)
  --yes, -y                跳过确认直接执行
  --dry-run                预览模式, 不实际生成
  --concurrency, -c <N>    并发请求数 (默认: 3)
  --limit <N>              限制生成数量 (测试用)
  --size, -s <尺寸>        强制覆盖所有尺寸
  --retry, -r              只重试上次失败的任务
  --help, -h               显示帮助

尺寸自动分配 (config/api.json -> sizeMapping):
  物品/图标/按钮/进度条/边框 -> 1024*1024  (最小)
  面板                      -> 1280*1280  (中等)
  背景                      -> 1440*1440  (最大)

示例:
  npm run batch                           # 生成全部
  npm run batch -- --type items -y        # 只生成物品, 跳过确认
  npm run batch -- --dry-run              # 只预览
  npm run batch -- --limit 3 -y           # 测试生成前3个
  npm run batch -- --size 1280*1280 -y    # 全部用指定尺寸
  npm run batch -- --retry -y             # 只重试上次失败
`);
}

function getTypeLabel(type) {
  const map = { item: '物品图标', icon: 'UI图标', button: '按钮', progressBar: '进度条', panel: '面板', background: '背景', border: '边框' };
  return map[type] || type;
}

function askConfirmation(question) {
  const rl = readline.createInterface({ input: process.stdin, output: process.stdout });
  return new Promise(resolve => {
    rl.question(question, (answer) => {
      rl.close();
      resolve(answer.trim().toLowerCase() === 'y' || answer.trim().toLowerCase() === 'yes');
    });
  });
}

/**
 * 保存失败任务到 images/.failed.json
 */
function saveFailed(results, tasks) {
  const failed = [];
  for (let i = 0; i < tasks.length; i++) {
    if (!results[i] || !results[i].success) {
      failed.push({ prompt: tasks[i].prompt, options: tasks[i].options });
    }
  }
  if (failed.length > 0) {
    fs.mkdirSync(path.dirname(FAILED_PATH), { recursive: true });
    fs.writeFileSync(FAILED_PATH, JSON.stringify(failed, null, 2), 'utf8');
    console.log(`\n💾 失败已保存: images/.failed.json (${failed.length}个)`);
    console.log(`   重试: npm run batch -- --retry -y`);
  }
}

/**
 * 从 images/.failed.json 加载失败任务
 */
function loadFailed() {
  if (!fs.existsSync(FAILED_PATH)) {
    console.log('⚠️  没有失败记录 images/.failed.json');
    process.exit(0);
  }
  const tasks = JSON.parse(fs.readFileSync(FAILED_PATH, 'utf8'));
  if (!tasks || tasks.length === 0) {
    console.log('✅ 失败记录为空');
    fs.unlinkSync(FAILED_PATH);
    process.exit(0);
  }
  return tasks;
}

async function main() {
  console.log('========================================');
  console.log('🚀 批量图片生成 - 末世生存合成');
  console.log('========================================\n');

  const opts = parseArgs();
  generator.init();

  // 加载API配置
  let apiConfig;
  try {
    apiConfig = loadApiConfig();
  } catch (e) {
    console.error('❌ 加载API配置失败, 请检查 config/api.json');
    process.exit(1);
  }

  // 收集任务
  const imageOptions = {};
  if (opts.concurrency) imageOptions.concurrency = opts.concurrency;
  if (opts.size) imageOptions.size = opts.size;

  let allTasks;

  if (opts.retry) {
    // 重试模式: 只加载失败任务
    allTasks = loadFailed();
    console.log(`📋 重试模式: ${allTasks.length} 个失败任务\n`);
    if (opts.size) {
      allTasks = allTasks.map(t => ({ prompt: t.prompt, options: { ...t.options, size: opts.size } }));
      console.log(`   尺寸已覆盖为: ${opts.size}\n`);
    }
  } else {
    // 正常收集任务
    let itemTasks = [];
    let uiTasks = [];

    if (opts.type === 'items' || opts.type === 'all') {
      itemTasks = generator.collectAllItemTasks(imageOptions);
    }
    if (opts.type === 'ui' || opts.type === 'all') {
      uiTasks = generator.collectUITasks(imageOptions);
    }

    allTasks = [...itemTasks, ...uiTasks];
    if (opts.limit && opts.limit > 0) {
      allTasks = allTasks.slice(0, opts.limit);
    }

    if (allTasks.length === 0) {
      console.log('⚠️  没有可生成的任务');
      process.exit(0);
    }

    // 按尺寸分组统计
    const sizeGroups = {};
    for (const t of allTasks) {
      const sz = t.options.size || 'unknown';
      if (!sizeGroups[sz]) sizeGroups[sz] = [];
      sizeGroups[sz].push(t);
    }

    // 打印预览
    console.log('📋 生成预览:');
    console.log(`   总任务数: ${allTasks.length}`);
    console.log(`   并发数:   ${opts.concurrency || apiConfig.concurrency || 3}`);
    console.log(`   模型:     ${apiConfig.requestBody?.model || 'unknown'}`);
    console.log(`   协议:     ${apiConfig.protocol || 'sync'}`);
    console.log('   按尺寸分布:');
    for (const [sz, tasks] of Object.entries(sizeGroups)) {
      const typeCount = {};
      for (const t of tasks) {
        const fn = t.options.fileName || '';
        const tp = fn.startsWith('special_') || fn.startsWith('cross_') ? 'item' : fn.split('_')[0];
        typeCount[tp] = (typeCount[tp] || 0) + 1;
      }
      const labels = Object.entries(typeCount).map(([k, v]) => `${getTypeLabel(k)}x${v}`).join(', ');
      console.log(`     ${sz}: ${tasks.length} 个 (${labels})`);
    }

    if (itemTasks.length > 0) {
      console.log(`\n   物品: ${itemTasks.length} 个 -> ${imageOptions.size || apiConfig.sizeMapping?.item || '1024*1024'}`);
    }

    if (opts.dryRun) {
      console.log('\n🔍 预览模式完成。添加 -y 参数实际生成。');
      return;
    }

    // 确认
    if (!opts.yes) {
      const confirmed = await askConfirmation('\n⚠️  确认开始生成? (y/n): ');
      if (!confirmed) {
        console.log('已取消');
        return;
      }
    }
  }

  // 执行
  console.log('\n🎨 开始生成...\n');
  const startTime = Date.now();

  const result = await generateImagesBatch(allTasks, opts.concurrency, (completed, total, r) => {
    const pct = ((completed / total) * 100).toFixed(1);
    const status = r.success ? '✅' : '❌';
    const name = path.basename(r.filePath || '').padEnd(35);
    const elapsed = ((Date.now() - startTime) / 1000).toFixed(0);
    process.stdout.write(`\r   ${status} [${String(completed).padStart(3)}/${total}] ${pct}% | ${name} | ${elapsed}s`);
  });

  const elapsed = ((Date.now() - startTime) / 1000).toFixed(1);

  console.log('\n\n========================================');
  console.log('📊 生成完成:');
  console.log(`   成功: ${result.successCount} / 失败: ${result.failCount}`);
  console.log(`   耗时: ${elapsed}s`);
  console.log(`   保存: images/items/ + images/ui/`);
  console.log('========================================\n');

  if (result.failCount > 0) {
    saveFailed(result.results, allTasks);
  } else if (opts.retry && fs.existsSync(FAILED_PATH)) {
    fs.unlinkSync(FAILED_PATH);
    console.log('🎉 全部成功，失败记录已清除');
  }
}

main().catch(err => {
  console.error(`\n❌ 致命错误: ${err.message}`);
  process.exit(1);
});
