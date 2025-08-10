// Basic service worker for caching
const CACHE_NAME = 'hunger-game-v1';

self.addEventListener('install', event => {
    console.log('Service worker installing...');
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    console.log('Service worker activating...');
    event.waitUntil(self.clients.claim());
});

self.addEventListener('fetch', event => {
    // Basic fetch strategy - network first, then cache
    event.respondWith(
        fetch(event.request)
            .catch(() => caches.match(event.request))
    );
}); 