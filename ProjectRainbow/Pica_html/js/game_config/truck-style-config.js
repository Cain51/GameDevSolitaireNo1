// 皮卡字符样式配置
// 定义皮卡中各种字符的颜色、动画和其他视觉属性

const TRUCK_CHAR_STYLES = {
  // 默认皮卡车体字符
  carBody: {
    chars: ["皮", "卡", "后", "车", "厢", "门", "|", "丨"],
    color: "#c41e3a", // 红色
    colorClass: "text-[#c41e3a]",
    hasGlow: true, // 是否发光
    animationClass: "animate-truck-glow",
  },

  // 轮子
  wheel: {
    chars: ["轮"],
    color: "#63635E", // 灰色
    colorClass: "text-[#63635E]",
    hasGlow: false,
  },

  // 车灯
  headlight: {
    chars: ["灯"],
    color: "#fef08a", // 暖黄色
    colorClass: "text-[#fef08a]",
    hasGlow: true,
    animationClass: "animate-truck-glow",
  },

  // 鹿乘客
  deer: {
    chars: ["鹿"],
    color: "#d4a574", // 金色/棕色
    colorClass: "text-[#d4a574]",
    hasGlow: true,
    animationClass: "animate-truck-glow",
  },

  // 猎人乘客
  hunter: {
    chars: ["猎", "人"],
    color: "#8b7355", // 猎人棕
    colorClass: "text-[#8b7355]",
    hasGlow: true,
    animationClass: "animate-truck-glow",
  },

  // 骚福瑞乘客
  saofurry: {
    chars: ["福"],
    color: "#ff69b4", // 骚粉色
    colorClass: "text-[#ff69b4]",
    hasGlow: true,
    animationClass: "animate-truck-glow",
  },

  // 空白字符
  blank: {
    chars: [B],
    color: "#424242", // 浅灰色
    colorClass: "text-gray-700",
    hasGlow: false,
  },
};

// 根据字符获取其样式配置
function getCharStyle(char) {
  for (const styleKey in TRUCK_CHAR_STYLES) {
    const styleConfig = TRUCK_CHAR_STYLES[styleKey];
    if (styleConfig.chars.includes(char)) {
      return {
        colorClass: styleConfig.colorClass,
        animationClass: styleConfig.hasGlow ? styleConfig.animationClass : "",
      };
    }
  }
  // 默认样式（黑色背景字符或未知字符）
  return {
    colorClass: "text-[#0d0d0d]",
    animationClass: "",
  };
}
