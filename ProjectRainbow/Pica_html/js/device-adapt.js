/**
 * device-adapt.js — 设备检测 + 屏幕自适应
 *
 * 在 <html> 注入 class：
 *   .device-android  .device-ios  .device-mobile  .device-pc  .device-capacitor
 *   .orient-landscape  .orient-portrait
 *   .screen-xs  .screen-sm  .screen-md  .screen-lg  （按短边分级）
 *
 * 注入 CSS 自定义属性（--da-* ）到 :root，驱动全局缩放：
 *   --da-scale        整体 UI 缩放比（0.52 ~ 1.0）
 *   --da-font-base    基础字号 px
 *   --da-gap          间距 px
 *   --da-pad          内边距 px
 *   --da-radius       圆角 px
 *   --da-canvas-h     游戏画布高度
 *   --da-bar-h        进度条高度 px
 *
 * 布局模式（动态写到 <body> dataset）：
 *   data-layout="landscape"  横屏：左游戏区 + 右库存
 *   data-layout="portrait"   竖屏：上游戏区 + 下标签栏切换
 *   data-layout="pc"         PC
 */

(function () {
    'use strict';

    /* ====================================================================
       1. 设备类型检测
       ==================================================================== */
    const ua = navigator.userAgent || '';
    const isAndroid  = /android/i.test(ua);
    const isIOS      = /iphone|ipad|ipod/i.test(ua) ||
                       (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
    const isMobile   = isAndroid || isIOS || /mobile|tablet/i.test(ua);
    const isPC       = !isMobile;
    const isCapacitor = !!(window.Capacitor &&
                          window.Capacitor.isNativePlatform &&
                          window.Capacitor.isNativePlatform());

    window.__isAndroid   = isAndroid;
    window.__isIOS       = isIOS;
    window.__isMobile    = isMobile;
    window.__isPC        = isPC;
    window.__isCapacitor = isCapacitor;

    const html = document.documentElement;
    if (isAndroid)   html.classList.add('device-android');
    if (isIOS)       html.classList.add('device-ios');
    if (isMobile)    html.classList.add('device-mobile');
    if (isPC)        html.classList.add('device-pc');
    if (isCapacitor) html.classList.add('device-capacitor');

    /* ====================================================================
       2. 移动端 viewport 安全区修正
       ==================================================================== */
    if (isMobile) {
        const mv = document.querySelector('meta[name="viewport"]');
        if (mv && !mv.content.includes('viewport-fit')) {
            mv.content += ', viewport-fit=cover';
        }
    }

    /* ====================================================================
       3. 预设安全区 CSS 变量（供 calc 使用）
       ==================================================================== */
    html.style.setProperty('--sat', 'env(safe-area-inset-top, 0px)');
    html.style.setProperty('--sab', 'env(safe-area-inset-bottom, 0px)');
    html.style.setProperty('--sal', 'env(safe-area-inset-left, 0px)');
    html.style.setProperty('--sar', 'env(safe-area-inset-right, 0px)');

    /* ====================================================================
       4. 屏幕尺寸计算 + CSS 变量注入
       ==================================================================== */
    function calcAndApply() {
        const W = window.innerWidth;
        const H = window.innerHeight;
        const isLandscape = W > H;
        const shortEdge   = Math.min(W, H);
        const longEdge    = Math.max(W, H);

        /* 方向 class */
        html.classList.toggle('orient-landscape', isLandscape);
        html.classList.toggle('orient-portrait',  !isLandscape);

        /* 短边分级 */
        html.classList.remove('screen-xs','screen-sm','screen-md','screen-lg');
        let sizeClass;
        if      (shortEdge < 360) sizeClass = 'screen-xs';
        else if (shortEdge < 480) sizeClass = 'screen-sm';
        else if (shortEdge < 720) sizeClass = 'screen-md';
        else                      sizeClass = 'screen-lg';
        html.classList.add(sizeClass);

        /* PC 不做缩放 */
        if (isPC) {
            _updateBodyLayout(false, isLandscape);
            return;
        }

        /* ----------------------------------------------------------------
           scale 计算（基于 CSS 逻辑像素，非物理像素）
           
           手机 CSS 像素尺寸参考（WebView 默认 DPR）：
             2560×1080 手机横屏 → CSS 约 427×180px 但实测 innerWidth 约 851×360
             1080×1920 竖屏     → CSS 约 360×640
             1440×2560 竖屏     → CSS 约 360×640
           
           设计基准：shortEdge=360px → scale=0.72（偏小以留空间）
                     shortEdge=480px → scale=0.88
           
           关键约束：
             横屏时 scale 受 longEdge/1280 约束（防止大屏横屏 UI 依然过大）
             竖屏时 scale 受 longEdge/800 约束
             全局最大 scale = 0.88（移动端始终比桌面小一点）
           ---------------------------------------------------------------- */
        const baseShort = 400;
        let scale = Math.min(0.88, shortEdge / baseShort);
        if (isLandscape) {
            scale = Math.min(scale, longEdge / 1280);
        } else {
            scale = Math.min(scale, longEdge / 800);
        }
        scale = Math.max(0.45, scale);

        /* 派生尺寸 */
        const fontBase = Math.round(13 * scale);
        const gap      = Math.round(6  * scale);
        const pad      = Math.round(8  * scale);
        const radius   = Math.round(10 * scale);
        const barH     = Math.max(3, Math.round(5 * scale));

        /* 游戏画布高度
           横屏：屏幕高度 - 状态栏估高 - 底部乘客区估高
           竖屏：宽度的 55%（使画布保持 16:9 感）         */
        const canvasH = isLandscape
            ? Math.max(100, H - Math.round(88 * scale))
            : Math.round(W * 0.55);

        /* 注入 CSS 变量 */
        const root = document.documentElement;
        root.style.setProperty('--da-scale',     scale.toFixed(3));
        root.style.setProperty('--da-font-base', fontBase + 'px');
        root.style.setProperty('--da-gap',       gap  + 'px');
        root.style.setProperty('--da-pad',       pad  + 'px');
        root.style.setProperty('--da-radius',    radius + 'px');
        root.style.setProperty('--da-bar-h',     barH + 'px');
        root.style.setProperty('--da-canvas-h',  canvasH + 'px');
        root.style.setProperty('--da-short',     shortEdge + 'px');
        root.style.setProperty('--da-long',      longEdge  + 'px');

        window.__daScale     = scale;
        window.__daLandscape = isLandscape;
        window.__daShortEdge = shortEdge;

        window.dispatchEvent(new CustomEvent('daUpdate', {
            detail: { scale, isLandscape, shortEdge, longEdge, sizeClass }
        }));

        _updateBodyLayout(true, isLandscape);

        console.log('[device-adapt]',
            isAndroid ? 'Android' : isIOS ? 'iOS' : 'Mobile',
            W + 'x' + H,
            isLandscape ? '横屏' : '竖屏',
            sizeClass, 'scale=' + scale.toFixed(2));
    }

    /* ====================================================================
       5. 更新 body 布局模式 data-layout
       ==================================================================== */
    function _updateBodyLayout(mobile, isLandscape) {
        function apply() {
            const body = document.body;
            if (!body) return;
            if (!mobile) { body.dataset.layout = 'pc'; return; }
            body.dataset.layout = isLandscape ? 'landscape' : 'portrait';
        }
        if (document.body) apply();
        else document.addEventListener('DOMContentLoaded', apply);
    }

    /* ====================================================================
       6. 初始计算 + 监听变化
       ==================================================================== */
    calcAndApply();

    let _resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(_resizeTimer);
        _resizeTimer = setTimeout(calcAndApply, 80);
    });
    window.addEventListener('orientationchange', function () {
        setTimeout(calcAndApply, 320);
    });

    /* ====================================================================
       7. 移动端手势优化
       ==================================================================== */
    if (isMobile) {
        document.addEventListener('touchstart', function (e) {
            if (e.touches.length > 1) e.preventDefault();
        }, { passive: false });

        let _lastTap = 0;
        document.addEventListener('touchend', function (e) {
            const now = Date.now();
            if (now - _lastTap < 300) e.preventDefault();
            _lastTap = now;
        }, { passive: false });
    }

})();
