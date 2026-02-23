// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    // Explicitly cache navigation routes needed to boot the app offline
    assetsRequests.push(new Request('/', { cache: 'no-cache' }));
    assetsRequests.push(new Request('offline1', { cache: 'no-cache' }));
    assetsRequests.push(new Request('offline2', { cache: 'no-cache' }));
    assetsRequests.push(new Request('offline3', { cache: 'no-cache' }));

    await caches.open(cacheName).then(async cache => {
        // Cache files individually to prevent a single 404 from completely breaking the SW install
        for (const req of assetsRequests) {
            try {
                const response = await fetch(req);
                if (response.ok) {
                    await cache.put(req, response);
                } else {
                    console.warn(`Service worker: Failed to cache ${req.url} (Status: ${response.status})`);
                }
            } catch (error) {
                console.warn(`Service worker: Network error caching ${req.url}`, error);
            }
        }
    });
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));

    // Take control of uncontrolled clients immediately
    await self.clients.claim();
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.mode === 'navigate') {
        event.respondWith(
            (async () => {
                try {
                    // Try to fetch from the network first to always get the latest HTML
                    const networkResponse = await fetch(event.request);
                    return networkResponse;
                } catch (error) {
                    console.info('Service worker: Network failed, serving offline route fallback');
                    // If network fails (offline), look for cached route exactly
                    const cache = await caches.open(cacheName);
                    cachedResponse = await cache.match(event.request);
                    if (cachedResponse) return cachedResponse;

                    // Fallback to the root if specific route not found in cache
                    return await cache.match('/');
                }
            })()
        );
        return;
    }

    if (event.request.method === 'GET') {
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(event.request);
    }

    return cachedResponse || fetch(event.request);
}
