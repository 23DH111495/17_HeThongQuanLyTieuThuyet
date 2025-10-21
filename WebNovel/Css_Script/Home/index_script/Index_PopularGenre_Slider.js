class SimpleGenreSlider {
    constructor(options = {}) {
        this.track = document.getElementById('genresSliderTrack');
        this.prevBtn = document.getElementById('genresPrevBtn');
        this.nextBtn = document.getElementById('genresNextBtn');
        this.dotsContainer = document.getElementById('genresSliderDots');
        this.tabsContainer = document.getElementById('genreNavigationTabs');

        // Configuration options
        this.config = {
            cardWidth: options.cardWidth || 185,
            cardsPerPage: options.cardsPerPage || 6,
            genreApiUrl: options.genreApiUrl || '/Home/GetNovelsByGenre',
            debounceDelay: options.debounceDelay || 300,
            gapWidth: options.gapWidth || 17.6,
            fadeOutDuration: 250,
            fadeInDuration: 300,
            contentLoadDelay: 50,
            ...options
        };

        this.currentPage = 0;
        this.totalPages = 0;
        this.isLoading = false;
        this.isTransitioning = false;
        this.currentGenreId = null;
        this.debounceTimer = null;
        this.loadingAbortController = null;

        this.init();
    }

    init() {
        if (!this.track) {
            console.warn('SimpleGenreSlider: Required elements not found');
            return;
        }

        this.calculatePages();
        this.createDots();
        this.bindEvents();

        // Set initial genre from first active tab
        const activeTab = this.tabsContainer?.querySelector('.active-genre');
        if (activeTab) {
            this.currentGenreId = activeTab.dataset.genreId;
        }

        // Ensure initial state is visible
        this.ensureInitialVisibility();
    }

    ensureInitialVisibility() {
        if (this.track) {
            this.track.style.opacity = '1';
            this.track.style.transition = 'none';
        }
    }

    calculatePages() {
        const cards = this.track.querySelectorAll('.genres-novel-card');
        this.totalPages = Math.ceil(cards.length / this.config.cardsPerPage);
    }

    createDots() {
        if (!this.dotsContainer) return;

        this.dotsContainer.innerHTML = '';
        for (let i = 0; i < this.totalPages; i++) {
            const dot = document.createElement('div');
            dot.className = 'genres-dot';
            if (i === 0) dot.classList.add('genres-active');
            dot.addEventListener('click', () => this.goToPage(i));
            this.dotsContainer.appendChild(dot);
        }
    }

    updateSlider() {
        if (this.isTransitioning) return;

        // Calculate translate distance including gaps
        const cardWidthWithGap = this.config.cardWidth + this.config.gapWidth;
        const translateX = -this.currentPage * (cardWidthWithGap * this.config.cardsPerPage);

        this.track.style.transform = `translateX(${translateX}px)`;
        this.track.style.transition = 'transform 0.5s cubic-bezier(0.25, 0.46, 0.45, 0.94)';

        // Update dots
        document.querySelectorAll('.genres-dot').forEach((dot, index) => {
            dot.classList.toggle('genres-active', index === this.currentPage);
        });

        // Update navigation button states
        this.updateNavigationStates();
    }

    updateNavigationStates() {
        console.log('Updating navigation states:', {
            currentPage: this.currentPage,
            totalPages: this.totalPages,
            isLoading: this.isLoading,
            isTransitioning: this.isTransitioning
        });

        if (this.prevBtn) {
            // Only disable during loading/transitioning, always enable otherwise
            const shouldDisablePrev = this.isLoading || this.isTransitioning || this.totalPages <= 1;
            this.prevBtn.disabled = shouldDisablePrev;
            this.prevBtn.style.opacity = shouldDisablePrev ? '0.5' : '1';
            console.log('Prev button - disabled:', shouldDisablePrev);
        }
        if (this.nextBtn) {
            // Only disable during loading/transitioning, always enable otherwise
            const shouldDisableNext = this.isLoading || this.isTransitioning || this.totalPages <= 1;
            this.nextBtn.disabled = shouldDisableNext;
            this.nextBtn.style.opacity = shouldDisableNext ? '0.5' : '1';
            console.log('Next button - disabled:', shouldDisableNext);
        }
    }

    next() {
        if (this.isLoading || this.isTransitioning || this.totalPages <= 1) return;

        // Circular navigation: go to first page if at the end
        if (this.currentPage >= this.totalPages - 1) {
            this.currentPage = 0;
        } else {
            this.currentPage++;
        }
        this.updateSlider();
    }

    prev() {
        if (this.isLoading || this.isTransitioning || this.totalPages <= 1) return;

        // Circular navigation: go to last page if at the beginning
        if (this.currentPage <= 0) {
            this.currentPage = this.totalPages - 1;
        } else {
            this.currentPage--;
        }
        this.updateSlider();
    }

    goToPage(pageIndex) {
        if (pageIndex >= 0 && pageIndex < this.totalPages && !this.isLoading && !this.isTransitioning) {
            this.currentPage = pageIndex;
            this.updateSlider();
        }
    }

    // Debounced genre switching to prevent rapid consecutive calls
    debouncedSwitchGenre(genreId, tabElement) {
        // Clear existing timer
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }

        // Cancel any ongoing request
        if (this.loadingAbortController) {
            this.loadingAbortController.abort();
        }

        // Set new timer
        this.debounceTimer = setTimeout(() => {
            this.switchGenre(genreId, tabElement);
        }, this.config.debounceDelay);
    }

    async switchGenre(genreId, tabElement) {
        // Prevent multiple simultaneous requests
        if (this.isLoading || this.isTransitioning || !genreId || !this.config.genreApiUrl || this.currentGenreId === genreId) {
            return;
        }

        // Cancel previous request if still ongoing
        if (this.loadingAbortController) {
            this.loadingAbortController.abort();
        }

        this.isLoading = true;
        this.isTransitioning = true;
        this.currentGenreId = genreId;

        // Create new abort controller for this request
        this.loadingAbortController = new AbortController();

        try {
            // 1. Update tab states immediately (no visual delay)
            this.updateActiveTab(tabElement);

            // 2. Show loading state and fade out current content
            this.showLoadingState();
            await this.fadeOut();

            // 3. Make API request
            const url = `${this.config.genreApiUrl}?genreId=${genreId}&count=18`;
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                },
                signal: this.loadingAbortController.signal
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.success && data.novels) {
                // 4. Update DOM content while hidden
                const limitedNovels = data.novels.slice(0, 18);
                await this.updateContentSmoothly(limitedNovels, data.genre);

                // 5. Fade in new content
                await this.fadeIn();

                // Final update of navigation states after everything is complete
                this.updateNavigationStates();
            } else {
                throw new Error(data.message || 'No novels found');
            }
        } catch (error) {
            if (error.name === 'AbortError') {
                // Request was cancelled, this is expected behavior
                return;
            }

            console.error('Error loading novels:', error);
            this.showErrorState(error.message);
            await this.fadeIn();
        } finally {
            this.isLoading = false;
            this.isTransitioning = false;
            this.loadingAbortController = null;

            // Force update navigation states after clearing flags
            setTimeout(() => {
                this.updateNavigationStates();
            }, 100);
        }
    }

    showLoadingState() {
        // Update navigation states during loading to disable buttons
        this.updateNavigationStates();

        // Optionally show loading indicator on tabs
        if (this.tabsContainer) {
            const activeTab = this.tabsContainer.querySelector('.active-genre');
            if (activeTab) {
                activeTab.style.opacity = '1';
                activeTab.style.pointerEvents = 'none';
            }
        }
    }

    async updateContentSmoothly(novels, genre) {
        // Create new content while completely hidden
        this.createNovelCards(novels, genre);

        // Small delay to ensure DOM updates are complete
        await new Promise(resolve => setTimeout(resolve, this.config.contentLoadDelay));

        // Update slider state
        this.calculatePages();
        this.createDots();
        this.currentPage = 0;

        // Reset transform without transition
        this.track.style.transition = 'none';
        this.track.style.transform = 'translateX(0px)';

        // Force reflow
        this.track.offsetHeight;

        // Update navigation states after content is loaded
        this.updateNavigationStates();
    }

    updateActiveTab(tabElement) {
        if (this.tabsContainer && tabElement) {
            // Remove loading state from all tabs
            this.tabsContainer.querySelectorAll('.genre-tab-button').forEach(tab => {
                tab.classList.remove('active-genre');
                tab.style.opacity = '1';
                tab.style.pointerEvents = 'auto';
            });
            // Add active class to clicked tab
            tabElement.classList.add('active-genre');
        }
    }

    fadeOut() {
        return new Promise(resolve => {
            if (!this.track) {
                resolve();
                return;
            }

            // Use a more reliable transition
            this.track.style.transition = `opacity ${this.config.fadeOutDuration}ms ease-out`;
            this.track.style.opacity = '0';

            setTimeout(resolve, this.config.fadeOutDuration);
        });
    }

    fadeIn() {
        return new Promise(resolve => {
            if (!this.track) {
                resolve();
                return;
            }

            // Ensure opacity starts at 0
            this.track.style.opacity = '0';

            // Use requestAnimationFrame for smooth animation
            requestAnimationFrame(() => {
                this.track.style.transition = `opacity ${this.config.fadeInDuration}ms ease-in`;
                this.track.style.opacity = '1';

                setTimeout(() => {
                    // Clean up transition styles after animation
                    this.track.style.transition = '';
                    resolve();
                }, this.config.fadeInDuration);
            });
        });
    }

    showErrorState(message) {
        if (!this.track) return;

        const errorHtml = `
            <div class="error-container" style="
                display: flex; 
                align-items: center; 
                justify-content: center; 
                height: 300px; 
                color: #ef4444;
                font-size: 1.1rem;
                opacity: 0;
                transition: opacity 300ms ease-in;
            ">
                <i class="fas fa-exclamation-triangle" style="margin-right: 10px;"></i>
                Error: ${this.escapeHtml(message)}
            </div>
        `;

        this.track.innerHTML = errorHtml;

        // Animate error message in
        requestAnimationFrame(() => {
            const errorContainer = this.track.querySelector('.error-container');
            if (errorContainer) {
                errorContainer.style.opacity = '1';
            }
        });
    }

    createNovelCards(novels, genre) {
        if (!novels || !Array.isArray(novels)) {
            this.showErrorState('No novels available');
            return;
        }

        let cardsHtml = '';
        novels.forEach((novel, index) => {
            const bookmarkFormatted = this.formatNumber(novel.bookmarkCount || 0);
            const genreColorCode = genre.colorCode || '#77DD77';

            const coverImage = novel.hasImage && novel.coverImageUrl
                ? `<img src="${novel.coverImageUrl}" alt="${this.escapeHtml(novel.title)}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 6px;" loading="lazy" />`
                : `<div class="genres-card-image" style="background: linear-gradient(145deg, #2a2a2a, #3a3a3a); display: flex; align-items: center; justify-content: center; color: ${genreColorCode}; font-size: 1.5rem;"><i class="fas fa-book"></i></div>`;

            cardsHtml += `
                <div class="genres-novel-card" data-genre="${this.escapeHtml(genre.name?.toLowerCase() || 'unknown')}" style="opacity: 0; transform: translateY(10px); transition: opacity 300ms ease-in ${index * 50}ms, transform 300ms ease-in ${index * 50}ms;">
                    <div class="genres-card-image-container">
                        ${novel.hasImage ? `<div class="genres-card-image">${coverImage}</div>` : coverImage}
                        <div class="genres-status-badge">${this.escapeHtml(novel.status || 'Unknown')}</div>
                        <div class="genres-bookmark-count">
                            <i class="fas fa-bookmark"></i> ${bookmarkFormatted}
                        </div>
                    </div>
                    <div class="genres-card-content">
                        <h3 class="genres-card-title">${this.escapeHtml(novel.title || 'Untitled')}</h3>
                        <div class="genres-card-rating">
                            <i class="fas fa-star genres-star-icon"></i>
                            <span class="genres-rating-value">${(novel.averageRating || 0).toFixed(1)}</span>
                        </div>
                        <div class="genres-card-footer">
                            <span class="genres-genre-tag">${this.escapeHtml(genre.name || 'Unknown')}</span>
                            <span class="genres-chapter-count">Ch. ${novel.totalChapters || 0}</span>
                        </div>
                    </div>
                </div>
            `;
        });

        this.track.innerHTML = cardsHtml;

        // Animate cards in after a short delay
        setTimeout(() => {
            const cards = this.track.querySelectorAll('.genres-novel-card');
            cards.forEach(card => {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            });
        }, 50);
    }

    formatNumber(num) {
        const number = parseInt(num) || 0;
        if (number >= 1000000) {
            return (number / 1000000).toFixed(1) + 'M';
        } else if (number >= 1000) {
            return (number / 1000).toFixed(1) + 'K';
        }
        return number.toString();
    }

    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    bindEvents() {
        // Navigation buttons
        if (this.nextBtn) {
            this.nextBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('Next button clicked, currentPage:', this.currentPage, 'totalPages:', this.totalPages);
                if (!this.isLoading && !this.isTransitioning) {
                    this.next();
                }
            });
        }

        if (this.prevBtn) {
            this.prevBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                console.log('Prev button clicked, currentPage:', this.currentPage, 'totalPages:', this.totalPages);
                if (!this.isLoading && !this.isTransitioning) {
                    this.prev();
                }
            });
        }

        // Genre tab switching with debouncing
        if (this.tabsContainer) {
            this.tabsContainer.addEventListener('click', (e) => {
                const tab = e.target.closest('.genre-tab-button');
                if (tab && !this.isLoading && !this.isTransitioning) {
                    e.preventDefault();
                    const genreId = tab.dataset.genreId;
                    if (genreId && genreId !== this.currentGenreId) {
                        this.debouncedSwitchGenre(genreId, tab);
                    }
                }
            });
        }

        // Handle visibility change
        document.addEventListener('visibilitychange', () => {
            // No auto-play functionality to handle
        });

        // Handle window resize
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                if (!this.isLoading && !this.isTransitioning) {
                    this.updateSlider();
                }
            }, 250);
        });
    }

    // Public method to force update navigation states (for debugging)
    forceUpdateNavigation() {
        console.log('Force updating navigation states');
        this.isLoading = false;
        this.isTransitioning = false;
        this.updateNavigationStates();
    }

    // Public method to destroy the slider
    destroy() {
        // Clear timers
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }

        // Cancel any ongoing requests
        if (this.loadingAbortController) {
            this.loadingAbortController.abort();
        }

        // Remove event listeners by cloning elements
        const elements = [this.nextBtn, this.prevBtn, this.tabsContainer];
        elements.forEach(el => {
            if (el && el.parentNode) {
                el.parentNode.replaceChild(el.cloneNode(true), el);
            }
        });

        // Clear references
        this.track = null;
        this.prevBtn = null;
        this.nextBtn = null;
        this.dotsContainer = null;
        this.tabsContainer = null;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    const sliderTrack = document.getElementById('genresSliderTrack');
    if (sliderTrack) {
        // Initialize with optimized configuration
        window.simpleGenreSlider = new SimpleGenreSlider({
            genreApiUrl: window.genreSliderConfig?.apiUrl || '/Home/GetNovelsByGenre',
            cardWidth: 185,
            cardsPerPage: 6,
            debounceDelay: 200,
            gapWidth: 17.6,
            fadeOutDuration: 250,
            fadeInDuration: 300,
            contentLoadDelay: 50
        });
    }
});