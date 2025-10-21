class NovelSlider {
    constructor() {
        this.slider = document.getElementById('novel-slider');
        this.slides = document.querySelectorAll('.novel-slide');
        this.prevBtn = document.getElementById('prev-slide');
        this.nextBtn = document.getElementById('next-slide');
        this.indicators = document.querySelectorAll('.slider-indicator-btn');
        this.debugRuntime = document.getElementById('debug-runtime');

        this.currentSlide = 0;
        this.totalSlides = this.slides.length;
        this.autoSlideInterval = null;
        this.isTransitioning = false;

        this.init();
    }

    init() {
        console.log('NovelSlider: Initializing with', this.totalSlides, 'slides');
        this.updateDebugInfo();

        if (this.totalSlides === 0) {
            console.warn('NovelSlider: No slides found!');
            return;
        }

        // Handle single slide case
        if (this.totalSlides === 1) {
            console.log('NovelSlider: Only one slide found, disabling navigation');
            this.hideSingleSlideControls();
            return;
        }

        this.bindEvents();
        this.updateSlider();
        this.startAutoSlide();
        this.handleImageErrors();
    }

    hideSingleSlideControls() {
        // Hide navigation controls for single slide
        if (this.prevBtn) this.prevBtn.style.display = 'none';
        if (this.nextBtn) this.nextBtn.style.display = 'none';

        const indicatorsContainer = document.querySelector('.slider-indicators-container');
        if (indicatorsContainer) indicatorsContainer.style.display = 'none';
    }

    handleImageErrors() {
        // Add error handling for images that fail to load
        this.slides.forEach(slide => {
            const img = slide.querySelector('img');
            if (img) {
                img.addEventListener('error', function () {
                    console.warn('Failed to load image, using fallback');
                    this.src = '/Content/images/no-cover-placeholder.jpg';
                });

                // Add loading class
                img.addEventListener('load', function () {
                    this.classList.add('loaded');
                });
            }
        });
    }

    bindEvents() {
        // Navigation buttons
        if (this.prevBtn) {
            this.prevBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.prevSlide();
            });
        }

        if (this.nextBtn) {
            this.nextBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.nextSlide();
            });
        }

        // Indicator buttons
        this.indicators.forEach((indicator, index) => {
            indicator.addEventListener('click', (e) => {
                e.preventDefault();
                const slideIndex = parseInt(indicator.dataset.slide) || index;
                this.goToSlide(slideIndex);
            });
        });

        // Mouse hover events - pause auto-slide on hover
        if (this.slider) {
            this.slider.addEventListener('mouseenter', () => this.stopAutoSlide());
            this.slider.addEventListener('mouseleave', () => this.startAutoSlide());
        }

        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowLeft') {
                e.preventDefault();
                this.prevSlide();
            }
            if (e.key === 'ArrowRight') {
                e.preventDefault();
                this.nextSlide();
            }
        });

        // Touch/swipe events for mobile
        this.addTouchEvents();

        // Handle slide clicks (optional - for navigation to novel detail)
        this.addSlideClickHandlers();
    }

    addSlideClickHandlers() {
        this.slides.forEach(slide => {
            const novelId = slide.dataset.novelId;
            if (novelId) {
                slide.style.cursor = 'pointer';
                slide.addEventListener('click', (e) => {
                    // Prevent click during transition
                    if (this.isTransitioning) return;

                    // Navigate to novel detail page
                    console.log('Navigating to novel:', novelId);
                    // window.location.href = `/Novel/Details/${novelId}`;
                });
            }
        });
    }

    addTouchEvents() {
        if (!this.slider) return;

        let startX = 0;
        let endX = 0;
        let startY = 0;
        let endY = 0;

        this.slider.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        }, { passive: true });

        this.slider.addEventListener('touchend', (e) => {
            endX = e.changedTouches[0].clientX;
            endY = e.changedTouches[0].clientY;

            const diffX = startX - endX;
            const diffY = Math.abs(startY - endY);

            // Only trigger swipe if horizontal movement is greater than vertical
            if (Math.abs(diffX) > 50 && diffY < 100) {
                if (diffX > 0) {
                    this.nextSlide(); // Swipe left - next slide
                } else {
                    this.prevSlide(); // Swipe right - prev slide
                }
            }
        }, { passive: true });
    }

    updateSlider() {
        if (this.totalSlides === 0 || this.isTransitioning) return;

        this.isTransitioning = true;

        // Apply transform
        if (this.slider) {
            this.slider.style.transform = `translateX(-${this.currentSlide * 100}%)`;
        }

        // Update indicators
        this.indicators.forEach((indicator, index) => {
            if (index === this.currentSlide) {
                indicator.classList.add('active');
            } else {
                indicator.classList.remove('active');
            }
        });

        // Update debug info
        this.updateDebugInfo();

        // Reset transition flag after animation
        setTimeout(() => {
            this.isTransitioning = false;
        }, 500);

        console.log(`NovelSlider: Moved to slide ${this.currentSlide + 1}/${this.totalSlides}`);
    }

    nextSlide() {
        if (this.totalSlides <= 1 || this.isTransitioning) return;
        this.currentSlide = (this.currentSlide + 1) % this.totalSlides;
        this.updateSlider();
    }

    prevSlide() {
        if (this.totalSlides <= 1 || this.isTransitioning) return;
        this.currentSlide = (this.currentSlide - 1 + this.totalSlides) % this.totalSlides;
        this.updateSlider();
    }

    goToSlide(index) {
        if (index >= 0 &&
            index < this.totalSlides &&
            index !== this.currentSlide &&
            !this.isTransitioning) {
            this.currentSlide = index;
            this.updateSlider();
        }
    }

    startAutoSlide() {
        if (this.totalSlides > 1) {
            this.stopAutoSlide(); // Clear any existing interval
            this.autoSlideInterval = setInterval(() => {
                this.nextSlide();
            }, 8000); // 5 seconds
        }
    }

    stopAutoSlide() {
        if (this.autoSlideInterval) {
            clearInterval(this.autoSlideInterval);
            this.autoSlideInterval = null;
        }
    }

    updateDebugInfo() {
        if (this.debugRuntime) {
            this.debugRuntime.innerHTML = `
                <hr style="margin: 10px 0;">
                <strong>Runtime Info:</strong><br>
                Current slide: ${this.currentSlide + 1}/${this.totalSlides}<br>
                Transform: translateX(-${this.currentSlide * 100}%)<br>
                Is transitioning: ${this.isTransitioning}<br>
                Auto-slide active: ${this.autoSlideInterval !== null}<br>
                Slider width: ${this.slider ? this.slider.offsetWidth : 0}px
            `;
        }
    }

    // Method to refresh slider with new data (useful for dynamic loading)
    refresh() {
        this.slides = document.querySelectorAll('.novel-slide');
        this.indicators = document.querySelectorAll('.slider-indicator-btn');
        this.totalSlides = this.slides.length;
        this.currentSlide = 0;

        if (this.totalSlides === 0) {
            console.warn('NovelSlider: No slides found after refresh!');
            return;
        }

        if (this.totalSlides === 1) {
            this.hideSingleSlideControls();
        } else {
            this.bindEvents();
            this.updateSlider();
            this.startAutoSlide();
        }

        this.handleImageErrors();
    }

    // Method to destroy the slider
    destroy() {
        this.stopAutoSlide();
        // Remove event listeners if needed
        // Clean up resources
    }
}

// Debug toggle function (if debug panel exists)
function toggleDebug() {
    const panel = document.getElementById('debug-panel');
    const button = document.querySelector('.debug-toggle');

    if (panel && button) {
        if (panel.style.display === 'none') {
            panel.style.display = 'block';
            button.textContent = 'Hide Debug';
        } else {
            panel.style.display = 'none';
            button.textContent = 'Debug';
        }
    }
}

// Global slider instance
window.novelSlider = null;

// Initialize slider when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    console.log('DOM loaded, initializing NovelSlider...');

    // Check if slider container exists
    const sliderContainer = document.getElementById('novel-slider');
    if (sliderContainer) {
        window.novelSlider = new NovelSlider();
    } else {
        console.warn('NovelSlider: Slider container not found!');
    }
});

// Optional: Add window resize handler to recalculate slider dimensions
window.addEventListener('resize', () => {
    if (window.novelSlider) {
        // Debounce resize events
        clearTimeout(window.novelSlider.resizeTimeout);
        window.novelSlider.resizeTimeout = setTimeout(() => {
            window.novelSlider.updateSlider();
        }, 250);
    }
});