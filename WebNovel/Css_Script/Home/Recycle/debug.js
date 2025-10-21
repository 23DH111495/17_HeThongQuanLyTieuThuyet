// Add this script to your page to debug the slider
document.addEventListener('DOMContentLoaded', function () {
    console.log('=== SLIDER DEBUG INFO ===');

    // Check slider container
    const sliderContainer = document.getElementById('novel-slider');
    console.log('Slider container found:', !!sliderContainer);

    if (sliderContainer) {
        console.log('Slider container HTML:', sliderContainer.outerHTML.substring(0, 200) + '...');
    }

    // Check slides
    const slides = document.querySelectorAll('.novel-slide');
    console.log('Number of slides found:', slides.length);

    slides.forEach((slide, index) => {
        console.log(`Slide ${index + 1}:`, {
            novelId: slide.dataset.novelId,
            hasImage: !!slide.querySelector('img'),
            hasOverlay: !!slide.querySelector('.slide-overlay'),
            title: slide.querySelector('.slide-title')?.textContent || 'No title',
            visible: getComputedStyle(slide).display !== 'none'
        });
    });

    // Check controls
    const prevBtn = document.getElementById('prev-slide');
    const nextBtn = document.getElementById('next-slide');
    const indicators = document.querySelectorAll('.slider-indicator-btn');

    console.log('Controls found:', {
        prevButton: !!prevBtn,
        nextButton: !!nextBtn,
        indicators: indicators.length
    });

    // Check if slider class is initialized
    console.log('Window.novelSlider:', window.novelSlider);

    // Make an AJAX call to debug endpoint
    fetch('/Home/DebugNovels')
        .then(response => response.json())
        .then(data => {
            console.log('=== DATABASE DEBUG INFO ===');
            console.log('Total novels in database:', data.totalNovels);
            console.log('Active novels:', data.activeNovels);
            console.log('Approved novels:', data.approvedNovels);
            console.log('Slider featured novels:', data.sliderFeatured);
            console.log('Featured novels:', data.featured);
            console.log('Sample novels:', data.sampleNovels);
        })
        .catch(error => {
            console.error('Error fetching debug info:', error);
        });

    // Test refresh slider endpoint
    fetch('/Home/RefreshSlider')
        .then(response => response.json())
        .then(data => {
            console.log('=== REFRESH SLIDER RESPONSE ===');
            console.log('Success:', data.success);
            console.log('Novels count:', data.count);
            console.log('Novels:', data.novels);
        })
        .catch(error => {
            console.error('Error refreshing slider:', error);
        });
});

// Function to manually check slider state
function checkSliderState() {
    if (window.novelSlider) {
        console.log('Slider state:', {
            currentSlide: window.novelSlider.currentSlide,
            totalSlides: window.novelSlider.totalSlides,
            isTransitioning: window.novelSlider.isTransitioning,
            autoSlideActive: window.novelSlider.autoSlideInterval !== null
        });
    } else {
        console.log('Slider not initialized');
    }
}

// Add to window for manual testing
window.checkSliderState = checkSliderState;