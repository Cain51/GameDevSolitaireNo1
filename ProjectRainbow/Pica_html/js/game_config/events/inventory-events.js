// 随机拾取物品表配置
const LOOT_TABLES = {
  // 默认 (综合杂物)
  默认: [
    { itemId: "废金属", quantity: 1, weight: 30 },
    { itemId: "布料", quantity: 1, weight: 25 },
    { itemId: "草药", quantity: 1, weight: 25 },
    { itemId: "空罐", quantity: 1, weight: 15 },
    { itemId: "原油", quantity: 1, weight: 10 },
    { itemId: "零食", quantity: 1, weight: 20 },
    { itemId: "精炼剂", quantity: 1, weight: 5 },
    { itemId: "铜线", quantity: 1, weight: 8 },
    { itemId: "橡胶", quantity: 1, weight: 6 },
    { itemId: "电池", quantity: 1, weight: 4 },
  ],
  // 稀有 (高价值)
  稀有: [
    { itemId: "急救箱", quantity: 1, weight: 10 },
    { itemId: "修理包", quantity: 1, weight: 10 },
    { itemId: "油桶", quantity: 1, weight: 10 },
    { itemId: "精炼剂", quantity: 2, weight: 20 },
    { itemId: "坐垫", quantity: 1, weight: 5 },
    { itemId: "零食", quantity: 3, weight: 15 },
    { itemId: "铜线", quantity: 2, weight: 10 },
    { itemId: "橡胶", quantity: 1, weight: 8 },
    { itemId: "电池", quantity: 1, weight: 6 },
  ],
  // 燃油相关
  燃油: [
    { itemId: "原油", quantity: 2, weight: 40 },
    { itemId: "油桶", quantity: 1, weight: 10 },
    { itemId: "精炼剂", quantity: 1, weight: 20 },
    { itemId: "空罐", quantity: 2, weight: 30 },
  ],
  // 食物/补给
  食物: [
    { itemId: "零食", quantity: 1, weight: 50 },
    { itemId: "零食", quantity: 2, weight: 20 },
    { itemId: "草药", quantity: 2, weight: 30 },
  ],
  // 废料
  废料: [
    { itemId: "废金属", quantity: 2, weight: 40 },
    { itemId: "布料", quantity: 2, weight: 30 },
    { itemId: "空罐", quantity: 3, weight: 20 },
    { itemId: "铜线", quantity: 1, weight: 12 },
    { itemId: "橡胶", quantity: 1, weight: 10 },
    { itemId: "电池", quantity: 1, weight: 8 },
  ]
};

// 根据权重随机拾取
// tableId: 默认, 稀有, 燃油, 食物, 废料
function getRandomLoot(tableId = '默认') {
  const table = LOOT_TABLES[tableId] || LOOT_TABLES['默认'];
  const results = [];
  // 拾取1~3种物品
  const count = 1 + Math.floor(Math.random() * 3);

  for (let i = 0; i < count; i++) {
    const totalWeight = table.reduce(
      (sum, item) => sum + (item.weight || 10),
      0,
    );
    let roll = Math.random() * totalWeight;

    for (const loot of table) {
      roll -= (loot.weight || 10);
      if (roll <= 0) {
        const existing = results.find((r) => r.itemId === loot.itemId);
        if (existing) {
          existing.quantity += loot.quantity;
        } else {
          results.push({ itemId: loot.itemId, quantity: loot.quantity });
        }
        break;
      }
    }
  }

  return results;
}
