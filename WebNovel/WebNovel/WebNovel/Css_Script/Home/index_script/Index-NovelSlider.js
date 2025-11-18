class NovelViewerSlider {
    constructor() {
        this.track = document.getElementById('nvsSliderTrack');
        this.prevBtn = document.getElementById('nvsPrevBtn');
        this.nextBtn = document.getElementById('nvsNextBtn');
        this.dotsContainer = document.getElementById('nvsSliderDots');

        this.cards = this.track.children;
        this.cardWidth = 236.6; // 280px + 20px gap
        this.cardsPerPage = 6; // Fixed: always show 5 cards per page
        this.totalPages = 3; // Fixed: exactly 3 pages
        this.currentPage = 0; // Current page (0, 1, 2)

        this.init();
    }

    init() {
        this.createDots();
        this.updateSlider();
        this.bindEvents();

        // Auto-play functionality
        this.startAutoPlay();
    }

    createDots() {
        this.dotsContainer.innerHTML = '';

        for (let i = 0; i < this.totalPages; i++) {
            const dot = document.createElement('div');
            dot.className = 'nvs-dot';
            dot.addEventListener('click', () => this.goToPage(i));
            this.dotsContainer.appendChild(dot);
        }
    }

    updateSlider() {
        // Calculate translate distance: move by 5 cards worth of width per page
        const translateX = -this.currentPage * (this.cardWidth * this.cardsPerPage);
        this.track.style.transform = `translateX(${translateX}px)`;

        // Update navigation buttons (always enabled since we loop)
        this.prevBtn.disabled = false;
        this.nextBtn.disabled = false;

        // Update dots
        document.querySelectorAll('.nvs-dot').forEach((dot, index) => {
            dot.classList.toggle('nvs-active', index === this.currentPage);
        });
    }

    next() {
        this.currentPage = (this.currentPage + 1) % this.totalPages; // Loop: 0->1->2->0
        this.updateSlider();
    }

    prev() {
        this.currentPage = (this.currentPage - 1 + this.totalPages) % this.totalPages; // Loop: 0->2->1->0
        this.updateSlider();
    }

    goToPage(pageIndex) {
        this.currentPage = pageIndex;
        this.updateSlider();
    }

    bindEvents() {
        this.nextBtn.addEventListener('click', () => {
            this.next();
            this.restartAutoPlay();
        });

        this.prevBtn.addEventListener('click', () => {
            this.prev();
            this.restartAutoPlay();
        });

        // Touch/swipe support
        let startX = 0;
        let currentX = 0;
        let isSwipe = false;

        this.track.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            isSwipe = true;
        });

        this.track.addEventListener('touchmove', (e) => {
            if (!isSwipe) return;
            currentX = e.touches[0].clientX;
        });

        this.track.addEventListener('touchend', () => {
            if (!isSwipe) return;

            const diff = startX - currentX;
            if (Math.abs(diff) > 50) {
                if (diff > 0) {
                    this.next();
                } else {
                    this.prev();
                }
            }

            isSwipe = false;
            this.restartAutoPlay();
        });

        // Pause auto-play on hover
        this.track.addEventListener('mouseenter', () => this.pauseAutoPlay());
        this.track.addEventListener('mouseleave', () => this.resumeAutoPlay());

        // Responsive handling
        window.addEventListener('resize', () => {
            this.updateSlider();
        });
    }

    startAutoPlay() {
        this.autoPlayInterval = setInterval(() => {
            this.next();
        }, 4000);
    }

    pauseAutoPlay() {
        if (this.autoPlayInterval) {
            clearInterval(this.autoPlayInterval);
        }
    }

    resumeAutoPlay() {
        this.startAutoPlay();
    }

    restartAutoPlay() {
        this.pauseAutoPlay();
        this.resumeAutoPlay();
    }
}

// Initialize slider when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new NovelViewerSlider();
});