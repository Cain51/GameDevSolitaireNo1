// 商人配置模块
// 定义商人出售的物品和价格

const MERCHANT_CONFIG = {
  // 普通商人 - 出现在"商"事件中
  流浪商人: {
    name: "流浪商人",
    greeting: "嘿，旅行者！看看我的好货！",
    items: [
      { itemId: "油桶", buyPrice: 18, sellPrice: 9 },
      { itemId: "修理包", buyPrice: 14, sellPrice: 7 },
      { itemId: "坐垫", buyPrice: 12, sellPrice: 6 },
      { itemId: "零食", buyPrice: 4, sellPrice: 2 },
      { itemId: "急救箱", buyPrice: 18, sellPrice: 9 },
      { itemId: "废金属", buyPrice: 6, sellPrice: 3 },
      { itemId: "布料", buyPrice: 4, sellPrice: 2 },
      { itemId: "草药", buyPrice: 5, sellPrice: 2 },
      { itemId: "空罐", buyPrice: 5, sellPrice: 2 },
      { itemId: "原油", buyPrice: 10, sellPrice: 5 },
      { itemId: "精炼剂", buyPrice: 20, sellPrice: 10 },
      { itemId: "医疗补给包", buyPrice: 38, sellPrice: 18 },
      { itemId: "舒适组合包", buyPrice: 30, sellPrice: 14 },
      { itemId: "钻石稿", sellPrice: 50 },
    ],
  },

  // 稀有商人 - 出售特殊物品，价格较高
  神秘商人: {
    name: "神秘商人",
    greeting: "稀客啊...我这里有些特别的东西...",
    items: [
      { itemId: "高级燃油", buyPrice: 45, sellPrice: 22 },
      { itemId: "八音盒", buyPrice: 50, sellPrice: 28 },
      { itemId: "幸运符", buyPrice: 60, sellPrice: 32 },
      { itemId: "急救箱", buyPrice: 20, sellPrice: 12 },
      { itemId: "修理包", buyPrice: 16, sellPrice: 10 },
      { itemId: "精炼剂", buyPrice: 25, sellPrice: 13 },
      { itemId: "应急信号弹", buyPrice: 35, sellPrice: 18 },
      { itemId: "鹰眼地图", buyPrice: 80, sellPrice: 40 },
      { itemId: "水冷系统", buyPrice: 120, sellPrice: 60 },
      { itemId: "轮胎", buyPrice: 55, sellPrice: 28 },
      { itemId: "豪华坐垫", buyPrice: 65, sellPrice: 32 },
      { itemId: "燃油混合液", buyPrice: 90, sellPrice: 45 },
      { itemId: "超级修理包", buyPrice: 85, sellPrice: 42 },
      { itemId: "钻石稿", sellPrice: 50 },
    ],
  },

  // 技术商人
  技术商人: {
    name: "技术商人",
    greeting: "需要修理工具吗？我这里都是精品！",
    items: [
      { itemId: "修理包", buyPrice: 12, sellPrice: 8 },
      { itemId: "废金属", buyPrice: 5, sellPrice: 4 },
      { itemId: "空罐", buyPrice: 4, sellPrice: 3 },
      { itemId: "精炼剂", buyPrice: 18, sellPrice: 12 },
      { itemId: "油桶", buyPrice: 16, sellPrice: 10 },
      { itemId: "高级燃油", buyPrice: 40, sellPrice: 25 },
      { itemId: "电池", buyPrice: 22, sellPrice: 11 },
      { itemId: "铜线", buyPrice: 8, sellPrice: 4 },
      { itemId: "橡胶", buyPrice: 12, sellPrice: 6 },
      { itemId: "轮胎", buyPrice: 50, sellPrice: 25 },
      { itemId: "水冷系统", buyPrice: 110, sellPrice: 55 },
      { itemId: "超级修理包", buyPrice: 78, sellPrice: 38 },
      { itemId: "燃油混合液", buyPrice: 82, sellPrice: 40 },
      { itemId: "钻石稿", sellPrice: 50 },
    ],
  },

  // 物资贩子
  物资贩子: {
    name: "物资贩子",
    greeting: "食物、坐垫、一切生活必需品都有！",
    items: [
      { itemId: "零食", buyPrice: 3, sellPrice: 2 },
      { itemId: "坐垫", buyPrice: 11, sellPrice: 7 },
      { itemId: "布料", buyPrice: 3, sellPrice: 2 },
      { itemId: "草药", buyPrice: 4, sellPrice: 3 },
      { itemId: "急救箱", buyPrice: 16, sellPrice: 8 },
      { itemId: "油桶", buyPrice: 17, sellPrice: 8 },
      { itemId: "皮革", buyPrice: 18, sellPrice: 9 },
      { itemId: "豪华坐垫", buyPrice: 60, sellPrice: 30 },
      { itemId: "医疗补给包", buyPrice: 35, sellPrice: 16 },
      { itemId: "舒适组合包", buyPrice: 28, sellPrice: 12 },
      { itemId: "钻石稿", sellPrice: 50 },
    ],
  },
};
