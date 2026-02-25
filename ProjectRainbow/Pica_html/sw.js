// Service Worker — 无人红色皮卡的安保之旅
const CACHE_NAME = 'red-truck-v3';

// 需要预缓存的核心资源
const PRECACHE_URLS = [
  './',
  './index.html',
  './game.html',
  './favicon.png',
  './manifest.json',
  // 设备适配
  './js/device-adapt.js',
  // JS 配置
  './js/text.js',
  './js/game_config/game-config.js',
  './js/game_config/achievements-config.js',
  './js/game_config/conditional-stories-config.js',
  './js/game_config/items-config.js',
  './js/game_config/merchant-config.js',
  './js/game_config/passenger-config.js',
  './js/game_config/passenger-dialogue-config.js',
  './js/game_config/truck-style-config.js',
  './js/game_config/endings/endings-config.js',
  './js/game_config/events/events-encounter.js',
  './js/game_config/events/events-index.js',
  './js/game_config/events/events-merchant.js',
  './js/game_config/events/events-rare.js',
  './js/game_config/events/events-special.js',
  './js/game_config/events/events-stop.js',
  './js/game_config/events/inventory-events.js',
  // JS 展示
  './js/game_show/audio.js',
  './js/game_show/event-animation.js',
  './js/game_show/inventory-display.js',
  './js/game_show/journey-log.js',
  './js/game_show/road.js',
  './js/game_show/scenery.js',
  './js/game_show/truck-display.js',
  // JS 系统
  './js/game_system/achievement-system.js',
  './js/game_system/game-state.js',
  './js/game_system/inventory-system.js',
  './js/game_system/save-system.js',
  './js/game_system/text-control.js',
  // JS 游戏逻辑
  './js/gameplay/event-handler.js',
  './js/gameplay/game-over.js',
  './js/gameplay/minesweeper.js',
  './js/gameplay/text-generation.js',
];

// 安装：预缓存所有核心资源
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => {
      return cache.addAll(PRECACHE_URLS).catch(err => {
        console.warn('[SW] 部分资源预缓存失败（可能是外部CDN）:', err);
      });
    }).then(() => self.skipWaiting())
  );
});

// 激活：清理旧版本缓存
self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys =>
      Promise.all(
        keys.filter(key => key !== CACHE_NAME).map(key => caches.delete(key))
      )
    ).then(() => self.clients.claim())
  );
});

// 请求拦截：缓存优先，网络回退策略
self.addEventListener('fetch', event => {
  // 只处理 GET 请求，跳过 CDN 等外部请求（直接走网络）
  if (event.request.method !== 'GET') return;

  const url = new URL(event.request.url);

  // 外部 CDN 资源（tailwindcss 等）：网络优先，缓存回退
  if (url.origin !== self.location.origin) {
    event.respondWith(
      fetch(event.request)
        .then(response => {
          const clone = response.clone();
          caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
          return response;
        })
        .catch(() => caches.match(event.request))
    );
    return;
  }

  // 本地资源：缓存优先，网络回退
  event.respondWith(
    caches.match(event.request).then(cached => {
      if (cached) return cached;
      return fetch(event.request).then(response => {
        if (response && response.status === 200) {
          const clone = response.clone();
          caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
        }
        return response;
      });
    })
  );
});
