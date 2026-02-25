// 事件数据总索引

const GAME_EVENTS = Object.assign(
  {},
  typeof EVENTS_ENCOUNTER !== "undefined" ? EVENTS_ENCOUNTER : {},
  typeof EVENTS_STOP !== "undefined" ? EVENTS_STOP : {},
  typeof EVENTS_MERCHANT !== "undefined" ? EVENTS_MERCHANT : {},
  typeof EVENTS_SPECIAL !== "undefined" ? EVENTS_SPECIAL : {},
);
