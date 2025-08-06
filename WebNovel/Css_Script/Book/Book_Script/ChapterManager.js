class ChapterManager {
    constructor() {
        this.showingAll = false;
        this.currentSort = 'ascending';
        this.isAnimating = false;
        this.animationSpeed = {
            fade: 300,      // Reduced for snappier feel
            stagger: 20,    // Reduced stagger for smoother flow
            delay: 30       // Reduced delay
        };

        this.init();
    }

    init() {
        document.addEventListener('DOMContentLoaded', () => {
            this.bindEvents();
            this.setInitialSort();
            this.initializeChapterVisibility();
        });
    }

    bindEvents() {
        const sortSelect = document.getElementById('sortSelect');
        const goToChapterInput = document.getElementById('goToChapter');

        if (sortSelect) {
            sortSelect.addEventListener('change', (e) => this.handleSortChange(e));
        }

        if (goToChapterInput) {
            goToChapterInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    this.jumpToChapter();
                }
            });
        }
    }

    setInitialSort() {
        const sortSelect = document.getElementById('sortSelect');
        if (sortSelect) {
            sortSelect.value = this.currentSort;
        }
    }

    initializeChapterVisibility() {
        this.updateChapterVisibility();
        this.updateShowMoreButton();
    }

    async handleSortChange(event) {
        const newSort = event.target.value;
        if (newSort === this.currentSort || this.isAnimating) return;

        this.isAnimating = true;
        this.currentSort = newSort;

        try {
            await this.smoothFadeOut();
            this.sortChapters(newSort);
            await this.smoothFadeIn();
        } finally {
            this.isAnimating = false;
        }
    }

    // Improved smooth fade out animation
    smoothFadeOut() {
        return new Promise((resolve) => {
            const visibleChapters = this.getVisibleChapters();
            let completed = 0;

            if (visibleChapters.length === 0) {
                resolve();
                return;
            }

            visibleChapters.forEach((chapter, index) => {
                setTimeout(() => {
                    chapter.style.transition = `opacity ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1), 
                                               transform ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1)`;
                    chapter.style.opacity = '0';
                    chapter.style.transform = 'translateY(-10px) scale(0.98)';

                    setTimeout(() => {
                        completed++;
                        if (completed === visibleChapters.length) {
                            resolve();
                        }
                    }, this.animationSpeed.fade);
                }, index * this.animationSpeed.stagger);
            });
        });
    }

    // Improved smooth fade in animation
    smoothFadeIn() {
        return new Promise((resolve) => {
            const visibleChapters = this.getVisibleChapters();
            let completed = 0;

            if (visibleChapters.length === 0) {
                resolve();
                return;
            }

            // Set initial state for fade in
            visibleChapters.forEach(chapter => {
                chapter.style.opacity = '0';
                chapter.style.transform = 'translateY(15px) scale(0.96)';
            });

            // Small delay before starting fade in
            setTimeout(() => {
                visibleChapters.forEach((chapter, index) => {
                    setTimeout(() => {
                        chapter.style.transition = `opacity ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1), 
                                                   transform ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1)`;
                        chapter.style.opacity = '1';
                        chapter.style.transform = 'translateY(0) scale(1)';

                        setTimeout(() => {
                            chapter.style.transition = '';
                            completed++;
                            if (completed === visibleChapters.length) {
                                resolve();
                            }
                        }, this.animationSpeed.fade);
                    }, index * this.animationSpeed.stagger);
                });
            }, this.animationSpeed.delay);
        });
    }

    sortChapters(sortType) {
        const chapterList = document.getElementById('chapter-list');
        if (!chapterList) return;

        const chapters = Array.from(chapterList.querySelectorAll('.book-chapter-item'));
        let sortedChapters;

        switch (sortType) {
            case 'ascending':
                sortedChapters = chapters.sort((a, b) => {
                    return parseInt(a.dataset.chapterNumber) - parseInt(b.dataset.chapterNumber);
                });
                break;

            case 'descending':
                sortedChapters = chapters.sort((a, b) => {
                    return parseInt(b.dataset.chapterNumber) - parseInt(a.dataset.chapterNumber);
                });
                break;

            case 'views':
                sortedChapters = chapters.sort((a, b) => {
                    const viewsA = parseInt(a.dataset.views) || 0;
                    const viewsB = parseInt(b.dataset.views) || 0;
                    return viewsB - viewsA;
                });
                break;

            case 'recent':
                sortedChapters = chapters.sort((a, b) => {
                    const dateA = new Date(a.dataset.publishDate);
                    const dateB = new Date(b.dataset.publishDate);
                    return dateB - dateA;
                });
                break;

            default:
                sortedChapters = chapters;
        }

        // Reorder DOM elements
        sortedChapters.forEach(chapter => {
            chapterList.appendChild(chapter);
        });

        this.updateChapterVisibility();
    }

    getVisibleChapters() {
        return Array.from(document.querySelectorAll('.book-chapter-item:not(.chapters-hidden)'));
    }

    getAllChapters() {
        return Array.from(document.querySelectorAll('.book-chapter-item'));
    }

    updateChapterVisibility() {
        const chapters = document.querySelectorAll('.book-chapter-item');
        chapters.forEach((chapter, index) => {
            if (this.showingAll || index < 10) {
                chapter.classList.remove('chapters-hidden');
                chapter.classList.add('chapter-visible');
                chapter.style.display = 'flex';
            } else {
                chapter.classList.add('chapters-hidden');
                chapter.classList.remove('chapter-visible');
                chapter.style.display = 'flex';
            }
        });
        this.updateShowMoreButton();
    }

    updateShowMoreButton() {
        const allChapters = this.getAllChapters();
        const showMoreBtn = document.querySelector('.show-more-btn');
        const btnText = document.getElementById('show-more-text');
        const btnIcon = document.getElementById('show-more-icon');
        const container = document.getElementById('show-more-container');

        if (!showMoreBtn || !btnText || !btnIcon || !container) return;

        const totalChapters = allChapters.length;

        if (totalChapters <= 10) {
            container.style.display = 'none';
            return;
        }

        container.style.display = 'flex';

        if (this.showingAll) {
            btnText.textContent = 'Show Less';
            btnIcon.className = 'fas fa-chevron-up';
        } else {
            const hiddenCount = totalChapters - 10;
            btnText.textContent = `Show more`;
            btnIcon.className = 'fas fa-chevron-down';
        }
    }

    async showMoreChapters() {
        if (this.isAnimating) return;

        const showMoreBtn = document.querySelector('.show-more-btn');
        const btnIcon = document.getElementById('show-more-icon');

        if (!showMoreBtn || !btnIcon) return;

        this.isAnimating = true;

        try {
            if (!this.showingAll) {
                await this.expandChapters(showMoreBtn, btnIcon);
            } else {
                await this.collapseChapters(showMoreBtn, btnIcon);
            }
        } finally {
            this.isAnimating = false;
        }
    }

    async expandChapters(showMoreBtn, btnIcon) {
        const allChapters = this.getAllChapters();
        const hiddenChapters = allChapters.slice(10);

        if (hiddenChapters.length === 0) return;

        // Show subtle loading state
        this.setButtonLoading(showMoreBtn, btnIcon, true);

        // Update visibility state first
        this.showingAll = true;
        this.updateChapterVisibility();
        this.updateShowMoreButton();

        await new Promise(resolve => setTimeout(resolve, 30));

        // Animate chapters appearing with improved staggered fade in
        await new Promise(resolve => {
            let completed = 0;

            hiddenChapters.forEach((chapter, index) => {
                // Set initial hidden state with more natural positioning
                chapter.style.opacity = '0';
                chapter.style.transform = 'translateY(20px) scale(0.96)';
                chapter.style.transition = 'none';

                setTimeout(() => {
                    chapter.style.transition = `opacity ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1), 
                                               transform ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1)`;
                    chapter.style.opacity = '1';
                    chapter.style.transform = 'translateY(0) scale(1)';

                    setTimeout(() => {
                        // Clean up inline styles
                        chapter.style.transition = '';
                        chapter.style.transform = '';
                        completed++;
                        if (completed === hiddenChapters.length) {
                            resolve();
                        }
                    }, this.animationSpeed.fade);
                }, index * this.animationSpeed.stagger);
            });
        });

        // Remove loading state
        this.setButtonLoading(showMoreBtn, btnIcon, false);
    }

    async collapseChapters(showMoreBtn, btnIcon) {
        const allChapters = this.getAllChapters();
        const chaptersToHide = allChapters.slice(10);

        if (chaptersToHide.length === 0) return;

        // Store current scroll position to maintain it
        // const currentScrollY = window.scrollY; // This line can also be removed

        // Animate chapters disappearing (reverse order for smooth effect)
        const reversedChapters = [...chaptersToHide].reverse();

        await new Promise(resolve => {
            let completed = 0;

            reversedChapters.forEach((chapter, index) => {
                setTimeout(() => {
                    chapter.style.transition = `opacity ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1), 
                                           transform ${this.animationSpeed.fade}ms cubic-bezier(0.25, 0.1, 0.25, 1)`;
                    chapter.style.opacity = '0';
                    chapter.style.transform = 'translateY(-15px) scale(0.98)';

                    setTimeout(() => {
                        chapter.classList.add('chapters-hidden');
                        chapter.classList.remove('chapter-visible');

                        // Clean up inline styles
                        chapter.style.transition = '';
                        chapter.style.transform = '';
                        chapter.style.opacity = '';

                        completed++;
                        if (completed === reversedChapters.length) {
                            resolve();
                        }
                    }, this.animationSpeed.fade);
                }, index * this.animationSpeed.stagger);
            });
        });

        // Update state
        this.showingAll = false;
        this.updateShowMoreButton();

        // The problematic scrolling block has been removed
    }

    setButtonLoading(button, icon, isLoading) {
        if (isLoading) {
            button.classList.add('loading');
            icon.className = 'fas fa-spinner fa-spin';
        } else {
            button.classList.remove('loading');
        }
    }

    jumpToChapter() {
        const chapterNumber = document.getElementById('goToChapter')?.value;
        if (!chapterNumber) return;

        const targetChapter = document.querySelector(`[data-chapter-number="${chapterNumber}"]`);
        if (!targetChapter) {
            this.showError('Chapter not found');
            return;
        }

        // If chapter is hidden, show all chapters first
        if (targetChapter.classList.contains('chapters-hidden')) {
            this.showingAll = false; // Reset state to trigger expansion
            this.showMoreChapters().then(() => {
                setTimeout(() => {
                    this.scrollToChapter(targetChapter);
                }, 200); // Reduced delay for better responsiveness
            });
        } else {
            this.scrollToChapter(targetChapter);
        }
    }

    scrollToChapter(chapter) {
        chapter.scrollIntoView({
            behavior: 'smooth',
            block: 'center'
        });

        // Improved highlight effect with more natural animation
        const originalStyles = {
            background: chapter.style.background || '',
            transform: chapter.style.transform || '',
            boxShadow: chapter.style.boxShadow || ''
        };

        chapter.style.transition = 'all 0.3s cubic-bezier(0.25, 0.1, 0.25, 1)';
        chapter.style.background = 'rgba(119, 221, 119, 0.2)';
        chapter.style.transform = 'scale(1.01)';
        chapter.style.boxShadow = '0 4px 20px rgba(119, 221, 119, 0.25)';

        setTimeout(() => {
            chapter.style.background = originalStyles.background;
            chapter.style.transform = originalStyles.transform;
            chapter.style.boxShadow = originalStyles.boxShadow;

            setTimeout(() => {
                chapter.style.transition = '';
            }, 300);
        }, 2000); // Reduced highlight duration
    }

    showError(message) {
        // Create or update error message
        let errorDiv = document.querySelector('.error-message');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'error-message';
            errorDiv.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                background: rgba(220, 53, 69, 0.9);
                color: white;
                padding: 12px 20px;
                border-radius: 8px;
                font-size: 14px;
                font-weight: 500;
                z-index: 1000;
                backdrop-filter: blur(10px);
                box-shadow: 0 4px 20px rgba(220, 53, 69, 0.3);
            `;
            document.body.appendChild(errorDiv);
        }

        errorDiv.textContent = message;
        errorDiv.style.opacity = '0';
        errorDiv.style.transform = 'translateX(100px) scale(0.9)';
        errorDiv.style.transition = 'all 0.3s cubic-bezier(0.25, 0.1, 0.25, 1)';

        setTimeout(() => {
            errorDiv.style.opacity = '1';
            errorDiv.style.transform = 'translateX(0) scale(1)';
        }, 10);

        // Auto-hide after 3 seconds
        setTimeout(() => {
            errorDiv.style.opacity = '0';
            errorDiv.style.transform = 'translateX(100px) scale(0.9)';
            setTimeout(() => {
                if (errorDiv.parentNode) {
                    errorDiv.parentNode.removeChild(errorDiv);
                }
            }, 300);
        }, 3000);
    }
}

// Initialize the chapter manager
const chapterManager = new ChapterManager();

// Expose functions to global scope for onclick handlers
window.showMoreChapters = () => chapterManager.showMoreChapters();
window.jumpToChapter = () => chapterManager.jumpToChapter();