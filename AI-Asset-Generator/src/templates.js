/**
 * Prompt模板定义
 * 定义物品图标和UI贴图的Prompt模板
 * 所有模板均强调透明背景，适配游戏引擎
 */

const ITEM_PROMPT_TEMPLATES = {
  // 物品图标基础模板
  base: `[游戏图标风格], [末世生存主题], [具体物品名称], [物品等级], 
[简洁描述], [像素风/扁平化], [暖色调/冷色调], 
[透明背景], [80x80], [Unity UI使用]`,

  // 等级对应的风格修饰
  levelModifiers: {
    1: { style: "粗糙, 简约, 基础", color: "冷色调, 灰暗", detail: "简单线条" },
    2: { style: "简洁, 朴素, 实用", color: "冷色调, 暗淡", detail: "基础细节" },
    3: { style: "实用, 改良, 精致", color: "中性色调", detail: "适度细节" },
    4: { style: "完善, 精细, 优质", color: "中性色调, 偏暖", detail: "丰富细节" },
    5: { style: "专业, 高品质, 完善", color: "暖色调, 明亮", detail: "精细纹理" },
    6: { style: "先进, 高科技, 精致", color: "暖色调, 明亮", detail: "科技感" },
    7: { style: "尖端, 未来感, 豪华", color: "明亮, 发光", detail: "高级特效" },
    8: { style: "超现代, 全息, 科幻", color: "发光, 蓝色", detail: "全息效果" },
    9: { style: "终极, 超级, 未来", color: "金色, 发光", detail: "光环特效" },
    10: { style: "传奇, 永恒, 神圣", color: "金色, 彩虹光", detail: "神圣光辉" }
  }
};

// 透明背景提示词前缀 - 放在最前面让模型最重视
const TRANSPARENT_PREFIX = "transparent background, alpha channel, cutout object on transparent canvas, no background, isolated, ";

/**
 * 生成物品Icon的Prompt
 * @param {Object} item - 物品对象
 * @param {string} lineTheme - 合成线主题
 * @returns {string} 生成的Prompt
 */
function generateItemPrompt(item, lineTheme) {
  const levelMod = ITEM_PROMPT_TEMPLATES.levelModifiers[item.level] || ITEM_PROMPT_TEMPLATES.levelModifiers[1];
  
  const prompt = `${TRANSPARENT_PREFIX}游戏物品图标, 末世生存主题, ${item.name}, 等级${item.level}, 
${item.description}, 扁平化图标风格, ${levelMod.color}, 
透明背景, 无底色, 纯透明PNG格式, 80x80像素, Unity UI Sprite使用, 
${levelMod.style}, ${levelMod.detail}, 简约UI风格, 
清晰的物品轮廓, 适合游戏内使用, 高对比度, 
主题元素: ${lineTheme}`;
  
  return prompt;
}

/**
 * 生成物品Icon的Prompt (简洁版)
 * @param {Object} item - 物品对象
 * @param {string} lineTheme - 合成线主题
 * @returns {string} 生成的Prompt
 */
function generateItemPromptSimple(item, lineTheme) {
  const levelMod = ITEM_PROMPT_TEMPLATES.levelModifiers[item.level] || ITEM_PROMPT_TEMPLATES.levelModifiers[1];
  
  const prompt = `透明背景, 游戏图标, 末世生存, ${item.name}, Lv.${item.level}, ${item.description}, ${levelMod.color}, transparent background, 80x80, Unity UI`;
  
  return prompt;
}

// UI贴图模板 - 全部以透明背景开头
const UI_PROMPT_TEMPLATES = {
  // 按钮
  button: `transparent background, alpha channel, cutout, 游戏UI按钮, 末世生存风格, 简洁设计, 
透明背景, 无底色, 
暖色调/冷色调, 圆角矩形, 适合点击, 
Unity UI Button使用, 128x48像素, 
PNG格式透明通道, 悬停效果, 按下效果`,

  // 面板
  panel: `transparent background, alpha channel, cutout, 游戏UI面板, 末世生存风格, 
透明背景, 无底色, 
深色主题, 半透明背景, 边框装饰, 
Unity UI Panel使用, 圆角矩形, 
PNG格式透明通道, 适合叠加层`,

  // 进度条
  progressBar: `transparent background, alpha channel, cutout, 游戏UI进度条, 末世生存风格, 
透明背景, 无底色, 
简洁设计, 填充样式, 边框装饰, 
Unity UI Slider使用, 横向/纵向, 
PNG格式透明通道`,

  // 背景 (注: 背景类不需要透明，保持原样但添加说明)
  background: `游戏UI背景, 末世生存风格, 
深色调, 纹理装饰, 氛围感, 
Unity UI Image使用, 平铺支持`,

  // 图标
  icon: `transparent background, alpha channel, cutout, 游戏UI图标, 末世生存风格, 
透明背景, 无底色, 
简洁易懂, 高辨识度, 扁平化设计, 
Unity UI Image使用, 32x32像素, 
PNG格式透明通道`,

  // 边框
  border: `transparent background, alpha channel, cutout, 游戏UI边框, 末世生存风格, 
透明背景, 无底色, 
装饰性边框, 金属质感/科技感, 
Unity UI Image使用, 九宫格拉伸, 
PNG格式透明通道`
};

/**
 * 生成UI元素的Prompt
 * @param {string} uiType - UI元素类型 (button/panel/progressBar/background/icon/border)
 * @param {Object} options - 额外选项
 * @returns {string} 生成的Prompt
 */
function generateUIPrompt(uiType, options = {}) {
  const template = UI_PROMPT_TEMPLATES[uiType] || UI_PROMPT_TEMPLATES.icon;
  
  let prompt = template;
  
  // 添加自定义选项
  if (options.color) {
    prompt += `, ${options.color}`;
  }
  if (options.size) {
    prompt += `, ${options.size}像素`;
  }
  if (options.custom) {
    prompt += `, ${options.custom}`;
  }
  
  return prompt;
}

// 导出模块
module.exports = {
  generateItemPrompt,
  generateItemPromptSimple,
  generateUIPrompt,
  ITEM_PROMPT_TEMPLATES,
  UI_PROMPT_TEMPLATES
};
