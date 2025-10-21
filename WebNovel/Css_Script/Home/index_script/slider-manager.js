// Unified Slider Manager - prevents conflicts between multiple sliders
class SliderManager {
    constructor() {
        this.sliders = new Map();
        this.isInitializing = false;
        this.initializationQueue = [];
        this.boundCleanup = this.cleanup.bind(this);

        // Global event listeners
        window.addEventListener('beforeunload', this.boundCleanup);
        document.addEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
    }

    registerSlider(name, sliderClass, config = {}) {
        if (this.sliders.has(name)) {
            console.warn(`Slider ${name} already registered`);
            return;
        }

        this.sliders.set(name, {
            class: sliderClass,
            instance: null,
            config: config,
            initialized: false
        });

        console.log(`Slider ${name} registered`);
    }

    async initializeSlider(name, forceReinit = false) {
        const sliderData = this.sliders.get(name);
        if (!sliderData) {
            console.warn(`Slider ${name} not registered`);
            return false;
        }

        if (sliderData.initialized && !forceReinit) {
            console.log(`Slider ${name} already initialized`);
            return true;
        }

        try {
            // Destroy existing instance if any
            if (sliderData.instance) {
                sliderData.instance.destroy();
                sliderData.instance = null;
            }

            // Create new instance
            sliderData.instance = new sliderData.class(sliderData.config);
            sliderData.initialized = true;

            console.log(`Slider ${name} initialized successfully`);
            return true;
        } catch (error) {
            console.error(`Failed to initialize slider ${name}:`, error);
            sliderData.initialized = false;
            return false;
        }
    }

    async initializeAllSliders() {
        if (this.isInitializing) {
            console.log('Slider initialization already in progress');
            return;
        }

        this.isInitializing = true;
        console.log('Initializing all sliders...');

        try {
            // Initialize sliders sequentially to prevent conflicts
            for (const [name, sliderData] of this.sliders) {
                if (!sliderData.initialized) {
                    await new Promise(resolve => {
                        setTimeout(async () => {
                            await this.initializeSlider(name);
                            resolve();
                        }, 100); // Small delay between initializations
                    });
                }
            }
        } finally {
            this.isInitializing = false;
        }

        console.log('All sliders initialization complete');
    }

    getSlider(name) {
        const sliderData = this.sliders.get(name);
        return sliderData ? sliderData.instance : null;
    }

    destroySlider(name) {
        const sliderData = this.sliders.get(name);
        if (sliderData && sliderData.instance) {
            sliderData.instance.destroy();
            sliderData.instance = null;
            sliderData.initialized = false;
            console.log(`Slider ${name} destroyed`);
        }
    }

    handleVisibilityChange() {
        // Pause/resume all sliders based on page visibility
        for (const [name, sliderData] of this.sliders) {
            if (sliderData.instance) {
                if (document.hidden) {
                    if (typeof sliderData.instance.pauseAutoSlide === 'function') {
                        sliderData.instance.pauseAutoSlide();
                    } else if (typeof sliderData.instance.pauseAutoPlay === 'function') {
                        sliderData.instance.pauseAutoPlay();
                    }
                } else {
                    if (typeof sliderData.instance.resumeAutoSlide === 'function') {
                        sliderData.instance.resumeAutoSlide();
                    } else if (typeof sliderData.instance.resumeAutoPlay === 'function') {
                        sliderData.instance.resumeAutoPlay();
                    }
                }
            }
        }
    }

    cleanup() {
        console.log('Cleaning up all sliders...');
        for (const [name] of this.sliders) {
            this.destroySlider(name);
        }

        // Remove global event listeners
        window.removeEventListener('beforeunload', this.boundCleanup);
        document.removeEventListener('visibilitychange', this.handleVisibilityChange);
    }

    // Utility method to safely wait for DOM
    waitForDOM() {
        return new Promise(resolve => {
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', resolve, { once: true });
            } else {
                resolve();
            }
        });
    }

    // Utility method to check if required elements exist
    checkRequiredElements(selectors) {
        const missing = [];
        for (const selector of selectors) {
            if (!document.querySelector(selector)) {
                missing.push(selector);
            }
        }
        return missing;
    }
}

// Enhanced Base Slider Class with better conflict prevention
class BaseSlider {
    constructor(config = {}) {
        this.config = {
            autoPlayDelay: 5000,
            transitionDuration: 500,
            pauseOnHover: true,
            enableKeyboard: true,
            enableTouch: true,
            ...config
        };

        this.isDestroyed = false;
        this.isTransitioning = false;
        this.autoPlayInterval = null;
        this.eventHandlers = new Map();

        // Generate unique ID for this instance
        this.instanceId = 'slider_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    // Safe event listener management
    addEventListener(element, event, handler, options = {}) {
        if (!element || this.isDestroyed) return;

        const wrappedHandler = (e) => {
            if (!this.isDestroyed) {
                handler.call(this, e);
            }
        };

        element.addEventListener(event, wrappedHandler, options);

        // Store for cleanup
        const key = `${element.tagName}_${event}_${Date.now()}`;
        this.eventHandlers.set(key, {
            element,
            event,
            handler: wrappedHandler,
            options
        });
    }

    // Clean up all event listeners
    removeAllEventListeners() {
        for (const [key, { element, event, handler }] of this.eventHandlers) {
            try {
                element.removeEventListener(event, handler);
            } catch (error) {
                console.warn('Error removing event listener:', error);
            }
        }
        this.eventHandlers.clear();
    }

    // Safe auto-play management
    startAutoPlay() {
        if (this.isDestroyed || this.config.autoPlayDelay <= 0) return;

        this.stopAutoPlay();
        this.autoPlayInterval = setInterval(() => {
            if (!this.isDestroyed && typeof this.next === 'function') {
                this.next();
            }
        }, this.config.autoPlayDelay);
    }

    stopAutoPlay() {
        if (this.autoPlayInterval) {
            clearInterval(this.autoPlayInterval);
            this.autoPlayInterval = null;
        }
    }

    pauseAutoPlay() {
        this.stopAutoPlay();
    }

    resumeAutoPlay() {
        if (!this.isDestroyed) {
            this.startAutoPlay();
        }
    }

    restartAutoPlay() {
        this.pauseAutoPlay();
        this.resumeAutoPlay();
    }

    // Base destroy method
    destroy() {
        if (this.isDestroyed) return;

        console.log(`Destroying slider instance: ${this.instanceId}`);
        this.isDestroyed = true;

        this.stopAutoPlay();
        this.removeAllEventListeners();

        // Clear any pending timeouts/intervals
        if (this.resizeTimeout) {
            clearTimeout(this.resizeTimeout);
        }
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }
    }
}

// Global slider manager instance
window.sliderManager = new SliderManager();

// Enhanced initialization function
async function initializeSliders() {
    console.log('Starting slider initialization...');

    try {
        // Wait for DOM to be ready
        await window.sliderManager.waitForDOM();

        // Register all sliders with their configurations
        window.sliderManager.registerSlider('heroSlider', NovelSlider, {
            containerId: 'novel-slider',
            autoPlayDelay: 5000
        });

        window.sliderManager.registerSlider('nvsSlider', NovelViewerSlider, {
            trackId: 'nvsSliderTrack',
            autoPlayDelay: 4000
        });

        window.sliderManager.registerSlider('genreSlider', SimpleGenreSlider, {
            trackId: 'genresSliderTrack',
            genreApiUrl: window.genreSliderConfig?.apiUrl || '/Home/GetNovelsByGenre',
            autoPlayDelay: 4000
        });

        // Initialize all sliders
        await window.sliderManager.initializeAllSliders();

        // Store references in global scope for backward compatibility
        window.novelSliderInstance = window.sliderManager.getSlider('heroSlider');
        window.nvsSliderInstance = window.sliderManager.getSlider('nvsSlider');
        window.genreSliderInstance = window.sliderManager.getSlider('genreSlider');

        console.log('All sliders initialized successfully');

    } catch (error) {
        console.error('Error during slider initialization:', error);
    }
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeSliders);
} else {
    // DOM is already ready
    setTimeout(initializeSliders, 0);
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { SliderManager, BaseSlider };
}