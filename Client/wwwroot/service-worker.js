// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development dangerous (seeing old files instead of new edits).
self.addEventListener('fetch', () => { });
