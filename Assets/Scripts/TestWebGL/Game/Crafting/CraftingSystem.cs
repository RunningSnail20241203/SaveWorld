using System;
using System.Collections.Generic;
using TestWebGL.Game.Items;

namespace TestWebGL.Game.Crafting
{
    /// <summary>
    /// 合成规则 - 表示一个合成配方
    /// 所有物品都遵循 "2合1" 的规则
    /// </summary>
    [System.Serializable]
    public class CraftRecipe
    {
        // 输入物品（需要2个）
        public ItemType inputItem;

        // 输出物品（获得1个）
        public ItemType outputItem;

        // 是否需要额外材料（跨线合成时使用）
        public ItemType secondaryInput;  // 如果不需要则为None
        public int secondaryCount;       // 第二个输入的数量

        /// <summary>
        /// 简单2合1合成
        /// </summary>
        public CraftRecipe(ItemType input, ItemType output)
        {
            inputItem = input;
            outputItem = output;
            secondaryInput = ItemType.None;
            secondaryCount = 0;
        }

        /// <summary>
        /// 跨线合成（需要2种不同物品）
        /// 例如：净水站×1 + 供水中心×2 = 水电站
        /// </summary>
        public CraftRecipe(ItemType input1, ItemType input2, int count2, ItemType output)
        {
            inputItem = input1;
            outputItem = output;
            secondaryInput = input2;
            secondaryCount = count2;
        }
    }

    /// <summary>
    /// 合成系统管理器
    /// 维护所有合成规则，处理合成逻辑
    /// </summary>
    public class CraftingSystem
    {
        private Dictionary<ItemType, CraftRecipe> _recipeMap;

        public delegate void RecipeAddedHandler(ItemType inputItem, ItemType outputItem);
        public event RecipeAddedHandler OnRecipeAdded;

        public CraftingSystem()
        {
            _recipeMap = new Dictionary<ItemType, CraftRecipe>();
            InitializeAllRecipes();
        }

        /// <summary>
        /// 初始化所有合成规则
        /// 根据设计规范 2.2 和 2.3 节
        /// </summary>
        private void InitializeAllRecipes()
        {
            // ==================== 生存线 - 水源（L1-L10）====================
            AddRecipe(ItemType.Water_L1, ItemType.Water_L2);   // 净水×2 → 净水片
            AddRecipe(ItemType.Water_L2, ItemType.Water_L3);   // 净水片×2 → 简易净水器
            AddRecipe(ItemType.Water_L3, ItemType.Water_L4);   // 简易净水器×2 → 净水器
            AddRecipe(ItemType.Water_L4, ItemType.Water_L5);   // 净水器×2 → 净水站
            AddRecipe(ItemType.Water_L5, ItemType.Water_L6);   // 净水站×2 → 供水中心
            AddRecipe(ItemType.Water_L6, ItemType.Water_L7);   // 供水中心×2 → 净水网络
            AddRecipe(ItemType.Water_L7, ItemType.Water_L8);   // 净水网络×2 → 净水系统
            AddRecipe(ItemType.Water_L8, ItemType.Water_L9);   // 净水系统×2 → 净水工厂

            // ==================== 生存线 - 食物（L1-L10）====================
            AddRecipe(ItemType.Food_L1, ItemType.Food_L2);
            AddRecipe(ItemType.Food_L2, ItemType.Food_L3);
            AddRecipe(ItemType.Food_L3, ItemType.Food_L4);
            AddRecipe(ItemType.Food_L4, ItemType.Food_L5);
            AddRecipe(ItemType.Food_L5, ItemType.Food_L6);
            AddRecipe(ItemType.Food_L6, ItemType.Food_L7);
            AddRecipe(ItemType.Food_L7, ItemType.Food_L8);
            AddRecipe(ItemType.Food_L8, ItemType.Food_L9);

            // ==================== 建造线 - 工具（L1-L10）====================
            AddRecipe(ItemType.Tool_L1, ItemType.Tool_L2);
            AddRecipe(ItemType.Tool_L2, ItemType.Tool_L3);
            AddRecipe(ItemType.Tool_L3, ItemType.Tool_L4);
            AddRecipe(ItemType.Tool_L4, ItemType.Tool_L5);
            AddRecipe(ItemType.Tool_L5, ItemType.Tool_L6);
            AddRecipe(ItemType.Tool_L6, ItemType.Tool_L7);
            AddRecipe(ItemType.Tool_L7, ItemType.Tool_L8);
            AddRecipe(ItemType.Tool_L8, ItemType.Tool_L9);

            // ==================== 建造线 - 住所（L1-L10）====================
            AddRecipe(ItemType.Home_L1, ItemType.Home_L2);
            AddRecipe(ItemType.Home_L2, ItemType.Home_L3);
            AddRecipe(ItemType.Home_L3, ItemType.Home_L4);
            AddRecipe(ItemType.Home_L4, ItemType.Home_L5);   // 特殊：帐篷×2 + 工具箱
            AddRecipe(ItemType.Home_L5, ItemType.Home_L6);
            AddRecipe(ItemType.Home_L6, ItemType.Home_L7);
            AddRecipe(ItemType.Home_L7, ItemType.Home_L8);
            AddRecipe(ItemType.Home_L8, ItemType.Home_L9);   // 特殊：堡垒×2 + 智能工厂

            // ==================== 医疗线（L1-L10）====================
            AddRecipe(ItemType.Medical_L1, ItemType.Medical_L2);
            AddRecipe(ItemType.Medical_L2, ItemType.Medical_L3);
            AddRecipe(ItemType.Medical_L3, ItemType.Medical_L4);
            AddRecipe(ItemType.Medical_L4, ItemType.Medical_L5);
            AddRecipe(ItemType.Medical_L5, ItemType.Medical_L6);
            AddRecipe(ItemType.Medical_L6, ItemType.Medical_L7);
            AddRecipe(ItemType.Medical_L7, ItemType.Medical_L8);   // 特殊：诊所×2 + 能源中心
            AddRecipe(ItemType.Medical_L8, ItemType.Medical_L9);
            AddRecipe(ItemType.Medical_L9, ItemType.Medical_L10);  // 特殊：医疗中心 + 智能工厂

            // ==================== 能源线（L1-L10）====================
            AddRecipe(ItemType.Energy_L1, ItemType.Energy_L2);
            AddRecipe(ItemType.Energy_L2, ItemType.Energy_L3);
            AddRecipe(ItemType.Energy_L3, ItemType.Energy_L4);
            AddRecipe(ItemType.Energy_L4, ItemType.Energy_L5);
            AddRecipe(ItemType.Energy_L5, ItemType.Energy_L6);
            AddRecipe(ItemType.Energy_L6, ItemType.Energy_L7);
            AddRecipe(ItemType.Energy_L7, ItemType.Energy_L8);   // 特殊：电网×2 + 净水站
            AddRecipe(ItemType.Energy_L8, ItemType.Energy_L9);
            AddRecipe(ItemType.Energy_L9, ItemType.Energy_L10);  // 特殊：核聚变核心 + 智能工厂

            // ==================== 文明线 - 知识（L1-L10）====================
            AddRecipe(ItemType.Knowledge_L1, ItemType.Knowledge_L2);
            AddRecipe(ItemType.Knowledge_L2, ItemType.Knowledge_L3);
            AddRecipe(ItemType.Knowledge_L3, ItemType.Knowledge_L4);
            AddRecipe(ItemType.Knowledge_L4, ItemType.Knowledge_L5);
            AddRecipe(ItemType.Knowledge_L5, ItemType.Knowledge_L6);
            AddRecipe(ItemType.Knowledge_L6, ItemType.Knowledge_L7);
            AddRecipe(ItemType.Knowledge_L7, ItemType.Knowledge_L8);
            AddRecipe(ItemType.Knowledge_L8, ItemType.Knowledge_L9);
            AddRecipe(ItemType.Knowledge_L9, ItemType.Knowledge_L10);

            // ==================== 文明线 - 希望（L1-L10）====================
            AddRecipe(ItemType.Hope_L1, ItemType.Hope_L2);
            AddRecipe(ItemType.Hope_L2, ItemType.Hope_L3);
            AddRecipe(ItemType.Hope_L3, ItemType.Hope_L4);
            AddRecipe(ItemType.Hope_L4, ItemType.Hope_L5);
            AddRecipe(ItemType.Hope_L5, ItemType.Hope_L6);
            AddRecipe(ItemType.Hope_L6, ItemType.Hope_L7);
            AddRecipe(ItemType.Hope_L7, ItemType.Hope_L8);
            AddRecipe(ItemType.Hope_L8, ItemType.Hope_L9);
            AddRecipe(ItemType.Hope_L9, ItemType.Hope_L10);

            // ==================== 探索线（L1-L10）====================
            AddRecipe(ItemType.Explore_L1, ItemType.Explore_L2);
            AddRecipe(ItemType.Explore_L2, ItemType.Explore_L3);
            AddRecipe(ItemType.Explore_L3, ItemType.Explore_L4);
            AddRecipe(ItemType.Explore_L4, ItemType.Explore_L5);
            AddRecipe(ItemType.Explore_L5, ItemType.Explore_L6);
            AddRecipe(ItemType.Explore_L6, ItemType.Explore_L7);
            AddRecipe(ItemType.Explore_L7, ItemType.Explore_L8);
            AddRecipe(ItemType.Explore_L8, ItemType.Explore_L9);
            AddRecipe(ItemType.Explore_L9, ItemType.Explore_L10);

            UnityEngine.Debug.Log($"[CraftingSystem] 已初始化 {_recipeMap.Count} 个合成规则");
        }

        /// <summary>
        /// 添加简单的2合1合成规则
        /// </summary>
        private void AddRecipe(ItemType input, ItemType output)
        {
            _recipeMap[input] = new CraftRecipe(input, output);
            OnRecipeAdded?.Invoke(input, output);
        }

        /// <summary>
        /// 获取合成规则
        /// </summary>
        public CraftRecipe GetRecipe(ItemType inputItem)
        {
            if (_recipeMap.TryGetValue(inputItem, out var recipe))
                return recipe;
            return null;
        }

        /// <summary>
        /// 检查是否可以合成
        /// </summary>
        public bool CanCraft(ItemType inputItem, int availableCount)
        {
            var recipe = GetRecipe(inputItem);
            if (recipe == null)
                return false;

            // 简单2合1，需要至少2个物品
            return availableCount >= 2;
        }

        /// <summary>
        /// 执行合成
        /// 返回合成结果（输出物品类型和数量）
        /// </summary>
        public (ItemType outputItem, int outputCount) Craft(ItemType inputItem, int inputCount)
        {
            var recipe = GetRecipe(inputItem);
            if (recipe == null || inputCount < 2)
                return (ItemType.None, 0);

            // 每2个输入物品，生成1个输出物品
            int craftCount = inputCount / 2;
            return (recipe.outputItem, craftCount);
        }

        /// <summary>
        /// 获取输出物品
        /// </summary>
        public ItemType GetOutputItem(ItemType inputItem)
        {
            var recipe = GetRecipe(inputItem);
            return recipe?.outputItem ?? ItemType.None;
        }

        /// <summary>
        /// 获取所有可合成的物品
        /// </summary>
        public IEnumerable<ItemType> GetAllCraftableItems()
        {
            return _recipeMap.Keys;
        }
    }
}
