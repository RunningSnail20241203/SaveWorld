using System.Collections.Generic;

namespace TestWebGL.Game.Items
{
    /// <summary>
    /// 物品类型枚举 - 共116种
    /// 按照设计规范中的10条生产线组织
    /// </summary>
    public enum ItemType
    {
        // 生存线 - 水源 (L1-L10)
        Water_L1 = 1001,
        Water_L2 = 1002,
        Water_L3 = 1003,
        Water_L4 = 1004,
        Water_L5 = 1005,
        Water_L6 = 1006,
        Water_L7 = 1007,
        Water_L8 = 1008,
        Water_L9 = 1009,
        Water_L10 = 1010,

        // 生存线 - 食物 (L1-L10)
        Food_L1 = 2001,
        Food_L2 = 2002,
        Food_L3 = 2003,
        Food_L4 = 2004,
        Food_L5 = 2005,
        Food_L6 = 2006,
        Food_L7 = 2007,
        Food_L8 = 2008,
        Food_L9 = 2009,
        Food_L10 = 2010,

        // 建造线 - 工具 (L1-L10)
        Tool_L1 = 3001,
        Tool_L2 = 3002,
        Tool_L3 = 3003,
        Tool_L4 = 3004,
        Tool_L5 = 3005,
        Tool_L6 = 3006,
        Tool_L7 = 3007,
        Tool_L8 = 3008,
        Tool_L9 = 3009,
        Tool_L10 = 3010,

        // 建造线 - 住所 (L1-L10)
        Home_L1 = 4001,
        Home_L2 = 4002,
        Home_L3 = 4003,
        Home_L4 = 4004,
        Home_L5 = 4005,
        Home_L6 = 4006,
        Home_L7 = 4007,
        Home_L8 = 4008,
        Home_L9 = 4009,
        Home_L10 = 4010,

        // 医疗线 (L1-L10)
        Medical_L1 = 5001,
        Medical_L2 = 5002,
        Medical_L3 = 5003,
        Medical_L4 = 5004,
        Medical_L5 = 5005,
        Medical_L6 = 5006,
        Medical_L7 = 5007,
        Medical_L8 = 5008,
        Medical_L9 = 5009,
        Medical_L10 = 5010,

        // 能源线 (L1-L10)
        Energy_L1 = 6001,
        Energy_L2 = 6002,
        Energy_L3 = 6003,
        Energy_L4 = 6004,
        Energy_L5 = 6005,
        Energy_L6 = 6006,
        Energy_L7 = 6007,
        Energy_L8 = 6008,
        Energy_L9 = 6009,
        Energy_L10 = 6010,

        // 文明线 - 知识 (L1-L10)
        Knowledge_L1 = 7001,
        Knowledge_L2 = 7002,
        Knowledge_L3 = 7003,
        Knowledge_L4 = 7004,
        Knowledge_L5 = 7005,
        Knowledge_L6 = 7006,
        Knowledge_L7 = 7007,
        Knowledge_L8 = 7008,
        Knowledge_L9 = 7009,
        Knowledge_L10 = 7010,

        // 文明线 - 希望 (L1-L10)
        Hope_L1 = 8001,
        Hope_L2 = 8002,
        Hope_L3 = 8003,
        Hope_L4 = 8004,
        Hope_L5 = 8005,
        Hope_L6 = 8006,
        Hope_L7 = 8007,
        Hope_L8 = 8008,
        Hope_L9 = 8009,
        Hope_L10 = 8010,

        // 探索线 (L1-L10)
        Explore_L1 = 9001,
        Explore_L2 = 9002,
        Explore_L3 = 9003,
        Explore_L4 = 9004,
        Explore_L5 = 9005,
        Explore_L6 = 9006,
        Explore_L7 = 9007,
        Explore_L8 = 9008,
        Explore_L9 = 9009,
        Explore_L10 = 9010,

        // 特殊物品 & 奖励道具
        EnergyDrink = 10001,
        BigEnergyDrink = 10002,
        ExploreSpeedUp = 10003,
        DoubleExplore = 10004,
        OldPhone = 10005,
        SurvivorPhoto = 10006,

        None = 0,
    }

    /// <summary>
    /// 物品数据定义
    /// 包含物品的基本属性：名称、等级、所属生产线等
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        public ItemType itemType;
        public string itemName;
        public int level;              // 1-10
        public string line;             // 所属生产线（水、食物、工具等)
        public string description;
    }

    /// <summary>
    /// 物品配置管理（数据库）
    /// 维护所有物品的元数据
    /// </summary>
    public static class ItemConfig
    {
        // 物品数据字典
        private static Dictionary<ItemType, ItemData> _itemDatabase;

        static ItemConfig()
        {
            InitializeItemDatabase();
        }

        private static void InitializeItemDatabase()
        {
            _itemDatabase = new Dictionary<ItemType, ItemData>
            {
                // 生存线 - 水源
                { ItemType.Water_L1, new ItemData { itemType = ItemType.Water_L1, itemName = "净水", level = 1, line = "Water", description = "探索获得" } },
                { ItemType.Water_L2, new ItemData { itemType = ItemType.Water_L2, itemName = "净水片", level = 2, line = "Water", description = "净水×2" } },
                { ItemType.Water_L3, new ItemData { itemType = ItemType.Water_L3, itemName = "简易净水器", level = 3, line = "Water", description = "净水片×2" } },
                { ItemType.Water_L4, new ItemData { itemType = ItemType.Water_L4, itemName = "净水器", level = 4, line = "Water", description = "简易净水器×2" } },
                { ItemType.Water_L5, new ItemData { itemType = ItemType.Water_L5, itemName = "净水站", level = 5, line = "Water", description = "净水器×2" } },
                { ItemType.Water_L6, new ItemData { itemType = ItemType.Water_L6, itemName = "供水中心", level = 6, line = "Water", description = "净水站×2" } },
                { ItemType.Water_L7, new ItemData { itemType = ItemType.Water_L7, itemName = "净水网络", level = 7, line = "Water", description = "供水中心×2" } },
                { ItemType.Water_L8, new ItemData { itemType = ItemType.Water_L8, itemName = "净水系统", level = 8, line = "Water", description = "净水网络×2" } },
                { ItemType.Water_L9, new ItemData { itemType = ItemType.Water_L9, itemName = "净水工厂", level = 9, line = "Water", description = "净水系统×2" } },
                { ItemType.Water_L10, new ItemData { itemType = ItemType.Water_L10, itemName = "永动供水站", level = 10, line = "Water", description = "净水工厂+永恒能源核心" } },

                // 生存线 - 食物
                { ItemType.Food_L1, new ItemData { itemType = ItemType.Food_L1, itemName = "罐头", level = 1, line = "Food", description = "探索获得" } },
                { ItemType.Food_L2, new ItemData { itemType = ItemType.Food_L2, itemName = "加热罐头", level = 2, line = "Food", description = "罐头×2" } },
                { ItemType.Food_L3, new ItemData { itemType = ItemType.Food_L3, itemName = "简易厨房", level = 3, line = "Food", description = "加热罐头×2" } },
                { ItemType.Food_L4, new ItemData { itemType = ItemType.Food_L4, itemName = "厨房套装", level = 4, line = "Food", description = "简易厨房×2" } },
                { ItemType.Food_L5, new ItemData { itemType = ItemType.Food_L5, itemName = "食堂", level = 5, line = "Food", description = "厨房套装×2" } },
                { ItemType.Food_L6, new ItemData { itemType = ItemType.Food_L6, itemName = "食品加工站", level = 6, line = "Food", description = "食堂×2" } },
                { ItemType.Food_L7, new ItemData { itemType = ItemType.Food_L7, itemName = "保鲜仓库", level = 7, line = "Food", description = "食品加工站×2" } },
                { ItemType.Food_L8, new ItemData { itemType = ItemType.Food_L8, itemName = "食品工厂", level = 8, line = "Food", description = "保鲜仓库×2" } },
                { ItemType.Food_L9, new ItemData { itemType = ItemType.Food_L9, itemName = "食物合成机", level = 9, line = "Food", description = "食品工厂×2" } },
                { ItemType.Food_L10, new ItemData { itemType = ItemType.Food_L10, itemName = "永动食物机", level = 10, line = "Food", description = "食物合成机+永恒能源核心" } },

                // 建造线 - 工具
                { ItemType.Tool_L1, new ItemData { itemType = ItemType.Tool_L1, itemName = "零件", level = 1, line = "Tool", description = "探索获得" } },
                { ItemType.Tool_L2, new ItemData { itemType = ItemType.Tool_L2, itemName = "简易工具", level = 2, line = "Tool", description = "零件×2" } },
                { ItemType.Tool_L3, new ItemData { itemType = ItemType.Tool_L3, itemName = "工具箱", level = 3, line = "Tool", description = "简易工具×2" } },
                { ItemType.Tool_L4, new ItemData { itemType = ItemType.Tool_L4, itemName = "修理站", level = 4, line = "Tool", description = "工具箱×2" } },
                { ItemType.Tool_L5, new ItemData { itemType = ItemType.Tool_L5, itemName = "工坊", level = 5, line = "Tool", description = "修理站×2" } },
                { ItemType.Tool_L6, new ItemData { itemType = ItemType.Tool_L6, itemName = "机械车间", level = 6, line = "Tool", description = "工坊×2" } },
                { ItemType.Tool_L7, new ItemData { itemType = ItemType.Tool_L7, itemName = "自动化流水线", level = 7, line = "Tool", description = "机械车间×2" } },
                { ItemType.Tool_L8, new ItemData { itemType = ItemType.Tool_L8, itemName = "机器人组装线", level = 8, line = "Tool", description = "自动化流水线×2" } },
                { ItemType.Tool_L9, new ItemData { itemType = ItemType.Tool_L9, itemName = "智能工厂", level = 9, line = "Tool", description = "机器人组装线×2" } },
                { ItemType.Tool_L10, new ItemData { itemType = ItemType.Tool_L10, itemName = "自动化工坊", level = 10, line = "Tool", description = "智能工厂+永恒能源核心" } },

                // 建造线 - 住所
                { ItemType.Home_L1, new ItemData { itemType = ItemType.Home_L1, itemName = "木材", level = 1, line = "Home", description = "探索获得" } },
                { ItemType.Home_L2, new ItemData { itemType = ItemType.Home_L2, itemName = "木料", level = 2, line = "Home", description = "木材×2" } },
                { ItemType.Home_L3, new ItemData { itemType = ItemType.Home_L3, itemName = "简易帐篷", level = 3, line = "Home", description = "木料×2" } },
                { ItemType.Home_L4, new ItemData { itemType = ItemType.Home_L4, itemName = "帐篷", level = 4, line = "Home", description = "简易帐篷×2" } },
                { ItemType.Home_L5, new ItemData { itemType = ItemType.Home_L5, itemName = "木屋", level = 5, line = "Home", description = "帐篷×2+工具箱" } },
                { ItemType.Home_L6, new ItemData { itemType = ItemType.Home_L6, itemName = "石头房子", level = 6, line = "Home", description = "木屋×2" } },
                { ItemType.Home_L7, new ItemData { itemType = ItemType.Home_L7, itemName = "瞭望塔", level = 7, line = "Home", description = "石头房子×2" } },
                { ItemType.Home_L8, new ItemData { itemType = ItemType.Home_L8, itemName = "堡垒", level = 8, line = "Home", description = "瞭望塔×2" } },
                { ItemType.Home_L9, new ItemData { itemType = ItemType.Home_L9, itemName = "避难所", level = 9, line = "Home", description = "堡垒×2+智能工厂" } },
                { ItemType.Home_L10, new ItemData { itemType = ItemType.Home_L10, itemName = "末日堡垒", level = 10, line = "Home", description = "避难所+自动化工坊" } },

                // 医疗线
                { ItemType.Medical_L1, new ItemData { itemType = ItemType.Medical_L1, itemName = "草药", level = 1, line = "Medical", description = "探索获得" } },
                { ItemType.Medical_L2, new ItemData { itemType = ItemType.Medical_L2, itemName = "绷带", level = 2, line = "Medical", description = "草药×2" } },
                { ItemType.Medical_L3, new ItemData { itemType = ItemType.Medical_L3, itemName = "急救包", level = 3, line = "Medical", description = "绷带×2" } },
                { ItemType.Medical_L4, new ItemData { itemType = ItemType.Medical_L4, itemName = "医疗箱", level = 4, line = "Medical", description = "急救包×2" } },
                { ItemType.Medical_L5, new ItemData { itemType = ItemType.Medical_L5, itemName = "医疗站", level = 5, line = "Medical", description = "医疗箱×2" } },
                { ItemType.Medical_L6, new ItemData { itemType = ItemType.Medical_L6, itemName = "简易诊所", level = 6, line = "Medical", description = "医疗站×2" } },
                { ItemType.Medical_L7, new ItemData { itemType = ItemType.Medical_L7, itemName = "诊所", level = 7, line = "Medical", description = "简易诊所×2" } },
                { ItemType.Medical_L8, new ItemData { itemType = ItemType.Medical_L8, itemName = "医院", level = 8, line = "Medical", description = "诊所×2+能源中心" } },
                { ItemType.Medical_L9, new ItemData { itemType = ItemType.Medical_L9, itemName = "医疗中心", level = 9, line = "Medical", description = "医院×2" } },
                { ItemType.Medical_L10, new ItemData { itemType = ItemType.Medical_L10, itemName = "全自动医疗舱", level = 10, line = "Medical", description = "医疗中心+智能工厂" } },

                // 能源线
                { ItemType.Energy_L1, new ItemData { itemType = ItemType.Energy_L1, itemName = "电池", level = 1, line = "Energy", description = "探索获得" } },
                { ItemType.Energy_L2, new ItemData { itemType = ItemType.Energy_L2, itemName = "充电宝", level = 2, line = "Energy", description = "电池×2" } },
                { ItemType.Energy_L3, new ItemData { itemType = ItemType.Energy_L3, itemName = "手摇发电机", level = 3, line = "Energy", description = "充电宝×2" } },
                { ItemType.Energy_L4, new ItemData { itemType = ItemType.Energy_L4, itemName = "太阳能板", level = 4, line = "Energy", description = "手摇发电机×2" } },
                { ItemType.Energy_L5, new ItemData { itemType = ItemType.Energy_L5, itemName = "发电机", level = 5, line = "Energy", description = "太阳能板×2" } },
                { ItemType.Energy_L6, new ItemData { itemType = ItemType.Energy_L6, itemName = "电力站", level = 6, line = "Energy", description = "发电机×2" } },
                { ItemType.Energy_L7, new ItemData { itemType = ItemType.Energy_L7, itemName = "电网", level = 7, line = "Energy", description = "电力站×2" } },
                { ItemType.Energy_L8, new ItemData { itemType = ItemType.Energy_L8, itemName = "能源中心", level = 8, line = "Energy", description = "电网×2+净水站" } },
                { ItemType.Energy_L9, new ItemData { itemType = ItemType.Energy_L9, itemName = "核聚变核心", level = 9, line = "Energy", description = "能源中心×2" } },
                { ItemType.Energy_L10, new ItemData { itemType = ItemType.Energy_L10, itemName = "永恒能源核心", level = 10, line = "Energy", description = "核聚变核心+智能工厂" } },

                // 文明线 - 知识
                { ItemType.Knowledge_L1, new ItemData { itemType = ItemType.Knowledge_L1, itemName = "旧书", level = 1, line = "Knowledge", description = "探索获得" } },
                { ItemType.Knowledge_L2, new ItemData { itemType = ItemType.Knowledge_L2, itemName = "书堆", level = 2, line = "Knowledge", description = "旧书×2" } },
                { ItemType.Knowledge_L3, new ItemData { itemType = ItemType.Knowledge_L3, itemName = "小图书馆", level = 3, line = "Knowledge", description = "书堆×2" } },
                { ItemType.Knowledge_L4, new ItemData { itemType = ItemType.Knowledge_L4, itemName = "图书馆", level = 4, line = "Knowledge", description = "小图书馆×2" } },
                { ItemType.Knowledge_L5, new ItemData { itemType = ItemType.Knowledge_L5, itemName = "读书会", level = 5, line = "Knowledge", description = "图书馆×2" } },
                { ItemType.Knowledge_L6, new ItemData { itemType = ItemType.Knowledge_L6, itemName = "学校", level = 6, line = "Knowledge", description = "读书会×2" } },
                { ItemType.Knowledge_L7, new ItemData { itemType = ItemType.Knowledge_L7, itemName = "学院", level = 7, line = "Knowledge", description = "学校×2+能源" } },
                { ItemType.Knowledge_L8, new ItemData { itemType = ItemType.Knowledge_L8, itemName = "大学", level = 8, line = "Knowledge", description = "学院×2" } },
                { ItemType.Knowledge_L9, new ItemData { itemType = ItemType.Knowledge_L9, itemName = "科研中心", level = 9, line = "Knowledge", description = "大学×2" } },
                { ItemType.Knowledge_L10, new ItemData { itemType = ItemType.Knowledge_L10, itemName = "文明复兴中心", level = 10, line = "Knowledge", description = "科研中心+永恒能源核心" } },

                // 文明线 - 希望
                { ItemType.Hope_L1, new ItemData { itemType = ItemType.Hope_L1, itemName = "种子", level = 1, line = "Hope", description = "探索获得（稀有）" } },
                { ItemType.Hope_L2, new ItemData { itemType = ItemType.Hope_L2, itemName = "幼苗", level = 2, line = "Hope", description = "种子×2" } },
                { ItemType.Hope_L3, new ItemData { itemType = ItemType.Hope_L3, itemName = "小花园", level = 3, line = "Hope", description = "幼苗×2+净水" } },
                { ItemType.Hope_L4, new ItemData { itemType = ItemType.Hope_L4, itemName = "花园", level = 4, line = "Hope", description = "小花园×2" } },
                { ItemType.Hope_L5, new ItemData { itemType = ItemType.Hope_L5, itemName = "菜园", level = 5, line = "Hope", description = "花园×2" } },
                { ItemType.Hope_L6, new ItemData { itemType = ItemType.Hope_L6, itemName = "农场", level = 6, line = "Hope", description = "菜园×2+工具" } },
                { ItemType.Hope_L7, new ItemData { itemType = ItemType.Hope_L7, itemName = "温室", level = 7, line = "Hope", description = "农场×2" } },
                { ItemType.Hope_L8, new ItemData { itemType = ItemType.Hope_L8, itemName = "生态圈", level = 8, line = "Hope", description = "温室×2+净水站" } },
                { ItemType.Hope_L9, new ItemData { itemType = ItemType.Hope_L9, itemName = "植物园", level = 9, line = "Hope", description = "生态圈×2" } },
                { ItemType.Hope_L10, new ItemData { itemType = ItemType.Hope_L10, itemName = "基因库", level = 10, line = "Hope", description = "植物园+科研中心" } },

                // 探索线
                { ItemType.Explore_L1, new ItemData { itemType = ItemType.Explore_L1, itemName = "地图碎片", level = 1, line = "Explore", description = "探索获得" } },
                { ItemType.Explore_L2, new ItemData { itemType = ItemType.Explore_L2, itemName = "简易地图", level = 2, line = "Explore", description = "地图碎片×2" } },
                { ItemType.Explore_L3, new ItemData { itemType = ItemType.Explore_L3, itemName = "指南针", level = 3, line = "Explore", description = "简易地图×2" } },
                { ItemType.Explore_L4, new ItemData { itemType = ItemType.Explore_L4, itemName = "望远镜", level = 4, line = "Explore", description = "指南针×2" } },
                { ItemType.Explore_L5, new ItemData { itemType = ItemType.Explore_L5, itemName = "对讲机", level = 5, line = "Explore", description = "望远镜×2+电池" } },
                { ItemType.Explore_L6, new ItemData { itemType = ItemType.Explore_L6, itemName = "探测仪", level = 6, line = "Explore", description = "对讲机×2" } },
                { ItemType.Explore_L7, new ItemData { itemType = ItemType.Explore_L7, itemName = "越野车", level = 7, line = "Explore", description = "探测仪×2+发动机" } },
                { ItemType.Explore_L8, new ItemData { itemType = ItemType.Explore_L8, itemName = "无人机", level = 8, line = "Explore", description = "越野车×2+电池" } },
                { ItemType.Explore_L9, new ItemData { itemType = ItemType.Explore_L9, itemName = "卫星通讯", level = 9, line = "Explore", description = "无人机×2+能源中心" } },
                { ItemType.Explore_L10, new ItemData { itemType = ItemType.Explore_L10, itemName = "全息探测仪", level = 10, line = "Explore", description = "卫星通讯+科研中心" } },

                // 特殊物品
                { ItemType.EnergyDrink, new ItemData { itemType = ItemType.EnergyDrink, itemName = "能量饮料", level = 0, line = "Special", description = "恢复5体力" } },
                { ItemType.BigEnergyDrink, new ItemData { itemType = ItemType.BigEnergyDrink, itemName = "大能量饮料", level = 0, line = "Special", description = "恢复15体力" } },
                { ItemType.ExploreSpeedUp, new ItemData { itemType = ItemType.ExploreSpeedUp, itemName = "探索加速券", level = 0, line = "Special", description = "下次探索不消耗体力" } },
                { ItemType.DoubleExplore, new ItemData { itemType = ItemType.DoubleExplore, itemName = "双倍探索券", level = 0, line = "Special", description = "下次探索产出×2" } },
                { ItemType.OldPhone, new ItemData { itemType = ItemType.OldPhone, itemName = "旧世界手机", level = 0, line = "Special", description = "收藏品" } },
                { ItemType.SurvivorPhoto, new ItemData { itemType = ItemType.SurvivorPhoto, itemName = "幸存者照片", level = 0, line = "Special", description = "终极收藏" } },
            };
        }

        /// <summary>
        /// 获取物品数据
        /// </summary>
        public static ItemData GetItemData(ItemType itemType)
        {
            if (_itemDatabase.TryGetValue(itemType, out var data))
                return data;
            return null;
        }

        /// <summary>
        /// 获取物品名称
        /// </summary>
        public static string GetItemName(ItemType itemType)
        {
            var data = GetItemData(itemType);
            return data != null ? data.itemName : "未知物品";
        }

        /// <summary>
        /// 获取物品等级
        /// </summary>
        public static int GetItemLevel(ItemType itemType)
        {
            var data = GetItemData(itemType);
            return data != null ? data.level : 0;
        }
    }
}
