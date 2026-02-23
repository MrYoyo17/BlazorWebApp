// In development, normally we don't cache to allow live reloading.
// But to test offline mode, we will import the published worker logic.
self.importScripts('./service-worker.published.js');
