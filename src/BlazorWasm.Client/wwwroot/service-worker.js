// Service Worker for Task Management System
// Provides offline support with cache-first strategy for API calls

const CACHE_NAME = 'task-management-v1';
const API_CACHE_NAME = 'task-management-api-v1';
const STATIC_CACHE_NAME = 'task-management-static-v1';

// Static files to cache
const STATIC_FILES = [
    '/',
    '/index.html',
    '/css/app.css',
    '/css/accessibility.css',
    '/js/chartInterop.js',
    '/js/fileDownload.js',
    '/js/focusManagement.js',
    '/js/themeManager.js',
    '/BlazorWasm.Client.styles.css',
    '/favicon.png',
    '/_framework/blazor.webassembly.js'
];

// Install event - cache static files
self.addEventListener('install', event => {
    console.log('Service Worker: Installing');
    event.waitUntil(
        caches.open(STATIC_CACHE_NAME)
            .then(cache => {
                console.log('Service Worker: Caching static files');
                return cache.addAll(STATIC_FILES);
            })
            .then(() => {
                console.log('Service Worker: Static files cached');
                return self.skipWaiting();
            })
            .catch(error => {
                console.error('Service Worker: Error caching static files', error);
            })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    console.log('Service Worker: Activating');
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames.map(cacheName => {
                        if (cacheName !== CACHE_NAME && 
                            cacheName !== API_CACHE_NAME && 
                            cacheName !== STATIC_CACHE_NAME) {
                            console.log('Service Worker: Deleting old cache', cacheName);
                            return caches.delete(cacheName);
                        }
                    })
                );
            })
            .then(() => {
                console.log('Service Worker: Claiming clients');
                return self.clients.claim();
            })
    );
});

// Fetch event - implement cache strategies
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    // Handle API requests with cache-first strategy
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(handleApiRequest(request));
        return;
    }

    // Handle static files with cache-first strategy
    if (isStaticFile(request)) {
        event.respondWith(handleStaticRequest(request));
        return;
    }

    // Handle Blazor framework files
    if (url.pathname.startsWith('/_framework/')) {
        event.respondWith(handleFrameworkRequest(request));
        return;
    }

    // Default: network first, fallback to cache
    event.respondWith(
        fetch(request).catch(() => {
            return caches.match(request);
        })
    );
});

// Handle API requests with cache-first strategy for GET requests
async function handleApiRequest(request) {
    const cache = await caches.open(API_CACHE_NAME);
    
    // For GET requests, try cache first
    if (request.method === 'GET') {
        try {
            // Check cache first
            const cachedResponse = await cache.match(request);
            if (cachedResponse) {
                console.log('Service Worker: Serving API response from cache', request.url);
                
                // Try to update cache in background
                fetch(request)
                    .then(networkResponse => {
                        if (networkResponse.ok) {
                            cache.put(request, networkResponse.clone());
                            console.log('Service Worker: Updated API cache in background', request.url);
                        }
                    })
                    .catch(() => {
                        // Network failed, cache is still valid
                    });
                
                return cachedResponse;
            }

            // Cache miss, try network
            const networkResponse = await fetch(request);
            if (networkResponse.ok) {
                // Cache successful response
                cache.put(request, networkResponse.clone());
                console.log('Service Worker: Cached new API response', request.url);
            }
            return networkResponse;
        } catch (error) {
            console.error('Service Worker: API request failed', error);
            // Return cached response if available
            const cachedResponse = await cache.match(request);
            if (cachedResponse) {
                return cachedResponse;
            }
            throw error;
        }
    }

    // For non-GET requests (POST, PUT, DELETE), always try network
    try {
        const response = await fetch(request);
        
        // If it's a successful POST/PUT/DELETE that might affect cached data,
        // clear related cache entries
        if (response.ok && ['POST', 'PUT', 'DELETE'].includes(request.method)) {
            clearRelatedCache(request.url);
        }
        
        return response;
    } catch (error) {
        console.error('Service Worker: Non-GET API request failed', error);
        throw error;
    }
}

// Handle static file requests
async function handleStaticRequest(request) {
    const cache = await caches.open(STATIC_CACHE_NAME);
    
    // Try cache first
    const cachedResponse = await cache.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    // Cache miss, try network
    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (error) {
        console.error('Service Worker: Static file request failed', error);
        throw error;
    }
}

// Handle Blazor framework requests
async function handleFrameworkRequest(request) {
    const cache = await caches.open(CACHE_NAME);
    
    // Try cache first for framework files
    const cachedResponse = await cache.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    // Cache miss, try network
    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (error) {
        console.error('Service Worker: Framework file request failed', error);
        throw error;
    }
}

// Check if request is for a static file
function isStaticFile(request) {
    const url = new URL(request.url);
    const staticExtensions = ['.css', '.js', '.png', '.jpg', '.jpeg', '.gif', '.svg', '.ico'];
    return staticExtensions.some(ext => url.pathname.endsWith(ext));
}

// Clear cache entries related to a modified resource
async function clearRelatedCache(url) {
    const cache = await caches.open(API_CACHE_NAME);
    const keys = await cache.keys();
    
    // Clear cache for the same resource path
    const urlObj = new URL(url);
    const basePath = urlObj.pathname.split('/').slice(0, -1).join('/');
    
    for (const request of keys) {
        const requestUrl = new URL(request.url);
        if (requestUrl.pathname.startsWith(basePath)) {
            await cache.delete(request);
            console.log('Service Worker: Cleared related cache entry', request.url);
        }
    }
}

// Message handling for cache management
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'CLEAR_CACHE') {
        clearAllCaches();
    } else if (event.data && event.data.type === 'GET_CACHE_STATUS') {
        getCacheStatus().then(status => {
            event.ports[0].postMessage(status);
        });
    }
});

// Clear all caches
async function clearAllCaches() {
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames.map(name => caches.delete(name)));
    console.log('Service Worker: All caches cleared');
}

// Get cache status
async function getCacheStatus() {
    const apiCache = await caches.open(API_CACHE_NAME);
    const staticCache = await caches.open(STATIC_CACHE_NAME);
    
    const apiKeys = await apiCache.keys();
    const staticKeys = await staticCache.keys();
    
    return {
        apiCacheSize: apiKeys.length,
        staticCacheSize: staticKeys.length,
        totalCacheSize: apiKeys.length + staticKeys.length
    };
}
