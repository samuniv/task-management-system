// Service Worker Registration and Management
// Handles registration, updates, and communication with the service worker

// Initialize blazorCulture global object for .NET interop
window.blazorCulture = window.blazorCulture || {
  dotNetHelper: null,
  setDotNetHelper: function (helper) {
    this.dotNetHelper = helper;
  },
};

window.serviceWorkerManager = {
  registration: null,
  isOnline: navigator.onLine,

  // Initialize service worker
  async init() {
    if ("serviceWorker" in navigator) {
      try {
        this.registration = await navigator.serviceWorker.register(
          "/service-worker.js"
        );
        console.log(
          "Service Worker registered successfully:",
          this.registration
        );

        // Listen for updates
        this.registration.addEventListener("updatefound", () => {
          console.log("Service Worker update found");
          this.handleUpdate();
        });

        // Listen for controller changes
        navigator.serviceWorker.addEventListener("controllerchange", () => {
          console.log("Service Worker controller changed");
          window.location.reload();
        });

        // Set up online/offline event listeners
        this.setupConnectivityListeners();

        return true;
      } catch (error) {
        console.error("Service Worker registration failed:", error);
        return false;
      }
    } else {
      console.warn("Service Workers are not supported");
      return false;
    }
  },

  // Handle service worker updates
  handleUpdate() {
    const newWorker = this.registration.installing;
    if (newWorker) {
      newWorker.addEventListener("statechange", () => {
        if (
          newWorker.state === "installed" &&
          navigator.serviceWorker.controller
        ) {
          // New version available
          this.showUpdateNotification();
        }
      });
    }
  },

  // Show update notification to user
  showUpdateNotification() {
    if (window.blazorCulture && window.blazorCulture.dotNetHelper) {
      // Notify Blazor component if available
      window.blazorCulture.dotNetHelper.invokeMethodAsync(
        "OnServiceWorkerUpdate"
      );
    } else {
      // Fallback notification
      if (confirm("A new version is available. Refresh to update?")) {
        this.skipWaiting();
      }
    }
  },

  // Skip waiting and activate new service worker
  async skipWaiting() {
    if (this.registration && this.registration.waiting) {
      this.registration.waiting.postMessage({ type: "SKIP_WAITING" });
    }
  },

  // Set up connectivity change listeners
  setupConnectivityListeners() {
    window.addEventListener("online", () => {
      this.isOnline = true;
      this.notifyConnectivityChange(true);
      console.log("Browser is online");
    });

    window.addEventListener("offline", () => {
      this.isOnline = false;
      this.notifyConnectivityChange(false);
      console.log("Browser is offline");
    });
  },

  // Notify Blazor about connectivity changes
  notifyConnectivityChange(isOnline) {
    if (window.blazorCulture && window.blazorCulture.dotNetHelper) {
      window.blazorCulture.dotNetHelper.invokeMethodAsync(
        "OnConnectivityChanged",
        isOnline
      );
    }

    // Dispatch custom event for other components
    window.dispatchEvent(
      new CustomEvent("connectivitychange", {
        detail: { isOnline },
      })
    );
  },

  // Get current connectivity status
  getConnectivityStatus() {
    return {
      isOnline: this.isOnline,
      serviceWorkerActive: !!(this.registration && this.registration.active),
    };
  },

  // Clear all caches
  async clearCaches() {
    if (this.registration && this.registration.active) {
      this.registration.active.postMessage({ type: "CLEAR_CACHE" });
      console.log("Cache clear requested");
    }
  },

  // Get cache status
  async getCacheStatus() {
    return new Promise((resolve) => {
      if (this.registration && this.registration.active) {
        const messageChannel = new MessageChannel();
        messageChannel.port1.onmessage = (event) => {
          resolve(event.data);
        };
        this.registration.active.postMessage({ type: "GET_CACHE_STATUS" }, [
          messageChannel.port2,
        ]);
      } else {
        resolve({ apiCacheSize: 0, staticCacheSize: 0, totalCacheSize: 0 });
      }
    });
  },

  // Force update check
  async checkForUpdate() {
    if (this.registration) {
      await this.registration.update();
    }
  },
};

// Auto-initialize when DOM is loaded
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", () => {
    window.serviceWorkerManager.init();
  });
} else {
  window.serviceWorkerManager.init();
}
