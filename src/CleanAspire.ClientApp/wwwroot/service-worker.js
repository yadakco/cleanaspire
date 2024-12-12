// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).

const cacheName = `cache-v1`;
const assetsToCache = [
    '_content/Tavenem.Blazor.IndexedDB/tavenem-indexeddb.js'
];
self.addEventListener('install', event => {
    console.log('V1 installing¡­');
    event.waitUntil(
        caches.open(cacheName).then(cache => cache.addAll(assetsToCache))
    );
});

self.addEventListener('activate', event => {
    console.log('Cache now ready to handle fetches!');
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(cachedResponse => {
            if (cachedResponse) {
                // Return the cached response if found
                return cachedResponse;
            }

            // If the request is not in the cache, fetch from the network
            return fetch(event.request).then(networkResponse => {

                // Open the cache and store the network response
                const responseToCache = networkResponse.clone();
                caches.open(cacheName).then(cache => {
                    cache.put(event.request, responseToCache);
                });

                return networkResponse;
            });
        })
    );
});
