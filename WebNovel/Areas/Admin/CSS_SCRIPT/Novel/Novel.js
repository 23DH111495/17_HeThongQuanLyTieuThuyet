// Novel Manager JavaScript
class NovelManager {
    constructor() {
        this.searchTimer = null;
        this.currentSort = { column: 'title', direction: 'asc' };
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadGenres();
        this.setupSorting();
        this.autoHideAlerts();
        this.updateSortOrderButtons(); // Initialize sort button states
    }

    bindEvents() {
        // Search and filter events
        $('#searchBox').on('input', (e) => this.handleSearch(e));
        $('#statusFilter, #moderationFilter, #activeFilter').on('change', () => this.handleFilterChange());

        // Search and clear buttons
        $('#searchBtn').on('click', () => this.performSearch());
        $('#clearBtn').on('click', () => this.clearFilters());

        // Sort toggle buttons
        $('.sort-option').on('click', (e) => this.handleSort(e));

        // Export and Bulk Actions
        $('.novels-btn-icon[title="Export Results"]').on('click', () => this.handleExport());
        $('.novels-btn-icon[title="Bulk Actions"]').on('click', () => this.handleBulkActions());

        // Checkbox events for bulk selection
        $(document).on('change', '.select-all-checkbox', (e) => this.toggleSelectAll(e));
        $(document).on('change', '.select-novel-checkbox', () => this.updateBulkActionState());

        // Modal events
        $('#addNovelBtn, #addFirstNovelBtn').on('click', () => this.openAddModal());
        $(document).on('click', '.edit-btn', (e) => this.handleEditClick(e));
        $(document).on('click', '.moderate-btn', (e) => this.handleModerateClick(e));

        // Genre functionality
        $('#genreSearchInput').on('keyup', (e) => this.searchGenres(e));
        $(document).on('change', '.novels-genre-item input[type="checkbox"]', () => this.updateSelectedGenres());

        // Cover image preview
        $('#novelCover').on('change', (e) => this.handleCoverImageChange(e));

        // Form submission
        $('#novelForm').on('submit', (e) => this.handleFormSubmit(e));

        // Modal close events
        $(window).on('click', (e) => this.handleWindowClick(e));
        $(document).on('keyup', (e) => this.handleEscapeKey(e));

        // Sort headers (if you have sortable table headers)
        $('.sortable').on('click', (e) => this.handleTableHeaderSort(e));
    }

    handleSearch(e) {
        clearTimeout(this.searchTimer);
        this.searchTimer = setTimeout(() => {
            this.performSearch();
        }, 500);
    }

    handleFilterChange() {
        // Check if all filters are set to "all" - if so, reset to original state
        const search = $('#searchBox').val().trim();
        const statusFilter = $('#statusFilter').val();
        const moderationFilter = $('#moderationFilter').val();
        const activeFilter = $('#activeFilter').val();

        if (!search && statusFilter === 'all' && moderationFilter === 'all' && activeFilter === 'all') {
            // Reset to original state without clearing sort
            this.performSearch(true);
        } else {
            this.performSearch();
        }
    }

    performSearch(isReset = false) {
        const searchParams = new URLSearchParams();

        const search = $('#searchBox').val().trim();
        const statusFilter = $('#statusFilter').val();
        const moderationFilter = $('#moderationFilter').val();
        const activeFilter = $('#activeFilter').val();

        // Only add parameters if they have values and aren't "all"
        if (search) searchParams.append('search', search);
        if (statusFilter && statusFilter !== 'all') searchParams.append('statusFilter', statusFilter);
        if (moderationFilter && moderationFilter !== 'all') searchParams.append('moderationFilter', moderationFilter);
        if (activeFilter && activeFilter !== 'all') searchParams.append('activeFilter', activeFilter);

        // Reset to first page unless it's a sort-only operation
        searchParams.append('page', '1');

        // Always include current sort
        if (this.currentSort.column) {
            searchParams.append('sortBy', this.currentSort.column);
            searchParams.append('sortDirection', this.currentSort.direction);
        }

        // If this is a reset and no filters are active, go to base URL with sort
        if (isReset && !search && statusFilter === 'all' && moderationFilter === 'all' && activeFilter === 'all') {
            if (this.currentSort.column) {
                window.location.href = window.location.pathname + '?' + searchParams.toString();
            } else {
                window.location.href = window.location.pathname;
            }
        } else {
            // Redirect with parameters
            window.location.href = window.location.pathname + '?' + searchParams.toString();
        }
    }

    clearFilters() {
        $('#searchBox').val('');
        $('#statusFilter').val('all');
        $('#moderationFilter').val('all');
        $('#activeFilter').val('all');

        // Reset sort to default
        this.currentSort = { column: 'title', direction: 'asc' };
        this.updateSortOrderButtons();

        // Redirect to base URL
        window.location.href = window.location.pathname;
    }

    setupSorting() {
        // Get current sort from URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        const sortBy = urlParams.get('sortBy');
        const sortDirection = urlParams.get('sortDirection');

        if (sortBy && sortDirection) {
            this.currentSort = { column: sortBy, direction: sortDirection };
        } else {
            // Default sort if none specified
            this.currentSort = { column: 'title', direction: 'asc' };
        }

        // Update button states
        this.updateSortOrderButtons();
    }

    handleSort(e) {
        e.preventDefault();
        const $button = $(e.currentTarget);
        const column = $button.data('sort');
        const direction = $button.data('direction');

        // Update the current sort state
        this.currentSort.column = column;
        this.currentSort.direction = direction;

        // Update button visual states
        this.updateSortOrderButtons();

        // Perform search with the new sort parameters
        this.performSearch();
    }

    updateSortOrderButtons() {
        // Remove 'active' class from all sort buttons
        $('.sort-option').removeClass('active');

        // Find the button that matches the current sort state and add the 'active' class
        const $activeButton = $(`.sort-option[data-sort="${this.currentSort.column}"][data-direction="${this.currentSort.direction}"]`);
        $activeButton.addClass('active');
    }

    handleTableHeaderSort(e) {
        const $header = $(e.currentTarget);
        const column = $header.data('sort');

        if (!column) return;

        // Toggle direction if same column, otherwise default to asc
        let direction = 'asc';
        if (this.currentSort.column === column) {
            direction = this.currentSort.direction === 'asc' ? 'desc' : 'asc';
        }

        this.currentSort.column = column;
        this.currentSort.direction = direction;

        this.updateSortIcon(column, direction);
        this.performSearch();
    }

    updateSortIcon(column, direction) {
        // Reset all sort icons
        $('.sort-icon').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');

        // Update the active sort icon
        const $activeHeader = $(`.sortable[data-sort="${column}"] .sort-icon`);
        $activeHeader.removeClass('fa-sort').addClass(direction === 'asc' ? 'fa-sort-up' : 'fa-sort-down');
    }

    handleExport() {
        // Get current filter parameters
        const urlParams = new URLSearchParams(window.location.search);

        try {
            // Collect table data
            const tableData = [];

            // Get headers (excluding action columns)
            const headers = [];
            $('#novelsTable thead th').each(function (index) {
                const headerText = $(this).text().trim();
                // Skip checkbox and action columns
                if (headerText && !$(this).hasClass('no-export') && headerText !== 'Actions' && headerText !== '') {
                    headers.push(headerText);
                }
            });

            if (headers.length === 0) {
                alert('No data available to export');
                return;
            }

            tableData.push(headers.join(','));

            // Get visible rows data
            $('#novelsTable tbody tr:visible').each(function () {
                const row = [];
                $(this).find('td').each(function (index) {
                    const $cell = $(this);
                    // Skip checkbox and action columns
                    if (!$cell.hasClass('no-export') && !$cell.find('.edit-btn, .moderate-btn').length && !$cell.find('input[type="checkbox"]').length) {
                        // Clean cell text
                        let cellText = $cell.text().trim().replace(/\s+/g, ' ');
                        // Handle special characters and commas
                        cellText = cellText.replace(/"/g, '""');
                        if (cellText.includes(',') || cellText.includes('"') || cellText.includes('\n')) {
                            cellText = `"${cellText}"`;
                        }
                        row.push(cellText);
                    }
                });
                if (row.length > 0) {
                    tableData.push(row.join(','));
                }
            });

            if (tableData.length <= 1) {
                alert('No data rows available to export');
                return;
            }

            // Create and download CSV
            const csvContent = tableData.join('\n');
            const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
            const link = document.createElement('a');
            const url = URL.createObjectURL(blob);

            // Generate filename with timestamp
            const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
            const filename = `novels_export_${timestamp}.csv`;

            link.setAttribute('href', url);
            link.setAttribute('download', filename);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);

            console.log(`Exported ${tableData.length - 1} rows to ${filename}`);

        } catch (error) {
            console.error('Export error:', error);
            alert('Error occurred while exporting data. Please try again.');
        }
    }

    handleBulkActions() {
        const selectedNovels = this.getSelectedNovels();

        if (selectedNovels.length === 0) {
            alert('Please select at least one novel to perform a bulk action.');
            return;
        }

        // Create a simple modal or use confirm dialogs for actions
        const actions = ['approve', 'reject', 'activate', 'deactivate', 'delete'];
        const action = prompt(`Select an action for ${selectedNovels.length} novel(s):\n${actions.join(', ')}\n\nEnter action:`);

        if (action && actions.includes(action.toLowerCase())) {
            const confirmed = confirm(`Are you sure you want to ${action} ${selectedNovels.length} novel(s)?`);

            if (confirmed) {
                console.log(`Performing bulk action "${action}" on novels with IDs: ${selectedNovels.join(', ')}`);

                // Here you would make an AJAX call to the server
                // For now, we'll simulate the action
                alert(`Bulk action "${action}" would be performed on ${selectedNovels.length} novels.\nNovel IDs: ${selectedNovels.join(', ')}`);

                // Example AJAX implementation:
                /*
                $.ajax({
                    url: '/Admin/Novel_Manager/BulkAction',
                    method: 'POST',
                    data: {
                        action: action,
                        novelIds: selectedNovels
                    },
                    success: (response) => {
                        if (response.success) {
                            alert(response.message);
                            this.performSearch(); // Refresh the table
                        } else {
                            alert('Error: ' + response.message);
                        }
                    },
                    error: () => {
                        alert('Error performing bulk action. Please try again.');
                    }
                });
                */
            }
        } else if (action) {
            alert('Invalid action. Please choose from: ' + actions.join(', '));
        }
    }

    toggleSelectAll(e) {
        const isChecked = $(e.currentTarget).prop('checked');
        $('.select-novel-checkbox').prop('checked', isChecked);
        this.updateBulkActionState();
    }

    getSelectedNovels() {
        const selectedIds = [];
        $('.select-novel-checkbox:checked').each(function () {
            selectedIds.push($(this).val());
        });
        return selectedIds;
    }

    updateBulkActionState() {
        const selectedCount = this.getSelectedNovels().length;
        const $bulkBtn = $('.novels-btn-icon[title="Bulk Actions"]');

        if (selectedCount > 0) {
            $bulkBtn.prop('disabled', false).addClass('active').attr('title', `Bulk Actions (${selectedCount} selected)`);
        } else {
            $bulkBtn.prop('disabled', true).removeClass('active').attr('title', 'Bulk Actions');
        }
    }

    // Rest of your existing methods remain the same...
    handleEditClick(e) {
        const novelId = $(e.currentTarget).data('novel-id');
        this.openEditModal(novelId);
    }

    handleModerateClick(e) {
        const novelId = $(e.currentTarget).data('novel-id');
        const novelTitle = $(e.currentTarget).data('novel-title');
        this.openModerationModal(novelId, novelTitle);
    }

    openAddModal() {
        $('#modalTitle').text('Add New Novel');
        $('#novelId').val('0');
        $('#novelForm')[0].reset();
        $('#previewImage').hide();
        $('.novels-cover-placeholder-upload').show();

        $('#novelActive, #novelOriginal').prop('checked', true);
        $('#novelPremium, #novelFeatured, #novelWeeklyFeatured, #novelSliderFeatured').prop('checked', false);

        $('.novels-genre-item input[type="checkbox"]').prop('checked', false);
        this.updateSelectedGenres();

        $('#novelModal').show();
    }

    openEditModal(novelId) {
        const baseUrl = $('#novelForm').data('get-novel-url') || '/Admin/Novel_Manager/GetNovel';

        $.ajax({
            url: baseUrl,
            type: 'GET',
            data: { id: novelId },
            success: (response) => {
                if (response.success) {
                    const novel = response.novel;

                    $('#modalTitle').text('Edit Novel');
                    $('#novelId').val(novel.id);
                    $('#novelTitle').val(novel.title);
                    $('#novelAltTitle').val(novel.alternativeTitle || '');
                    $('#novelSynopsis').val(novel.synopsis || '');
                    $('#novelAuthor').val(novel.authorName || '');
                    $('#novelStatus').val(novel.status || 'Ongoing');
                    $('#novelLanguage').val(novel.language || 'EN');
                    $('#novelOriginalLang').val(novel.originalLanguage || '');
                    $('#translationStatus').val(novel.translationStatus || 'Original');

                    $('#novelActive').prop('checked', novel.isActive);
                    $('#novelPremium').prop('checked', novel.isPremium);
                    $('#novelOriginal').prop('checked', novel.isOriginal);
                    $('#novelFeatured').prop('checked', novel.isFeatured);
                    $('#novelWeeklyFeatured').prop('checked', novel.isWeeklyFeatured);
                    $('#novelSliderFeatured').prop('checked', novel.isSliderFeatured);

                    if (novel.coverImageUrl) {
                        $('#previewImage').attr('src', novel.coverImageUrl).show();
                        $('.novels-cover-placeholder-upload').hide();
                    } else {
                        $('#previewImage').hide();
                        $('.novels-cover-placeholder-upload').show();
                    }

                    $('.novels-genre-item input[type="checkbox"]').prop('checked', false);
                    if (novel.genreIds && novel.genreIds.length > 0) {
                        novel.genreIds.forEach((genreId) => {
                            $(`.novels-genre-item input[value="${genreId}"]`).prop('checked', true);
                        });
                    }
                    this.updateSelectedGenres();

                    $('#novelModal').show();
                } else {
                    alert('Error loading novel: ' + response.message);
                }
            },
            error: (xhr, status, error) => {
                alert('Error loading novel data. Please try again.');
                console.error('AJAX Error:', error);
            }
        });
    }

    closeModal() {
        $('#novelModal').hide();
        $('#novelForm')[0].reset();
        $('#novelId').val('0');
    }

    openModerationModal(novelId, novelTitle) {
        $('#moderateNovelId').val(novelId);
        $('#moderateNovelTitle').text(novelTitle);
        $('#moderationStatus').val('');
        $('#moderationNotes').val('');
        $('#moderationModal').show();
    }

    closeModerationModal() {
        $('#moderationModal').hide();
        $('#moderationForm')[0].reset();
    }

    loadGenres() {
        const baseUrl = $('#novelForm').data('get-genres-url') || '/Admin/Novel_Manager/GetGenres';

        $.ajax({
            url: baseUrl,
            type: 'GET',
            success: (genres) => {
                this.renderGenres(genres);
            },
            error: (xhr, status, error) => {
                console.error('Error loading genres:', error);
                const fallbackGenres = [
                    { id: 1, name: 'Action', colorCode: '#ff6b6b' },
                    { id: 2, name: 'Adventure', colorCode: '#4ecdc4' },
                    { id: 3, name: 'Romance', colorCode: '#ff9ff3' },
                    { id: 4, name: 'Fantasy', colorCode: '#9c88ff' },
                    { id: 5, name: 'Drama', colorCode: '#feca57' },
                    { id: 6, name: 'Comedy', colorCode: '#77dd77' },
                    { id: 7, name: 'Horror', colorCode: '#333333' },
                    { id: 8, name: 'Mystery', colorCode: '#6c5ce7' },
                    { id: 9, name: 'Sci-Fi', colorCode: '#00cec9' },
                    { id: 10, name: 'Thriller', colorCode: '#e84393' }
                ];
                this.renderGenres(fallbackGenres);
            }
        });
    }

    renderGenres(genres) {
        const $genreList = $('#genreList');
        $genreList.empty();

        genres.forEach((genre) => {
            const $genreItem = $('<div class="novels-genre-item">');
            const $genreCheckbox = $('<div class="novels-genre-checkbox">');

            const $checkbox = $('<input>')
                .attr('type', 'checkbox')
                .attr('id', 'genre-' + genre.id)
                .attr('value', genre.id);

            const $label = $('<label>')
                .attr('for', 'genre-' + genre.id)
                .addClass('novels-genre-label')
                .css('background-color', genre.colorCode || '#77dd77')
                .text(genre.name);

            if (genre.iconClass) {
                const $icon = $('<i>').addClass(genre.iconClass);
                $label.prepend($icon).prepend(' ');
            }

            $genreCheckbox.append($checkbox).append($label);
            $genreItem.append($genreCheckbox);
            $genreList.append($genreItem);
        });
    }

    searchGenres(e) {
        const searchTerm = $(e.target).val().toLowerCase();
        $('.novels-genre-item').each(function () {
            const genreName = $(this).find('label').text().toLowerCase();
            if (genreName.includes(searchTerm)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    }

    updateSelectedGenres() {
        const $selectedGenreTags = $('#selectedGenreTags');
        $selectedGenreTags.empty();

        const selectedGenres = [];
        $('.novels-genre-item input[type="checkbox"]:checked').each(function () {
            const $genreLabel = $(this).siblings('label');
            const genreName = $genreLabel.text().trim();
            const genreColor = $genreLabel.css('background-color');

            selectedGenres.push({
                name: genreName,
                color: genreColor
            });
        });

        if (selectedGenres.length === 0) {
            $selectedGenreTags.html('<span class="novels-no-genres">No genres selected</span>');
        } else {
            selectedGenres.forEach((genre) => {
                const $tag = $('<span class="novels-selected-genre-tag">')
                    .css('background-color', genre.color)
                    .text(genre.name);
                $selectedGenreTags.append($tag);
            });
        }
    }

    handleCoverImageChange(e) {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $('#previewImage').attr('src', e.target.result).show();
                $('.novels-cover-placeholder-upload').hide();
            };
            reader.readAsDataURL(file);
        } else {
            $('#previewImage').hide();
            $('.novels-cover-placeholder-upload').show();
        }
    }

    handleFormSubmit(e) {
        const isEditMode = $('#novelId').val() !== '0';
        const $form = $(e.target);

        if (isEditMode) {
            const updateUrl = $form.data('update-url') || $form.attr('action').replace('CreateNovel', 'UpdateNovel');
            $form.attr('action', updateUrl);
        }

        const selectedGenres = [];
        $('.novels-genre-item input[type="checkbox"]:checked').each(function () {
            selectedGenres.push($(this).val());
        });

        $('.genre-hidden-input').remove();
        selectedGenres.forEach((genreId) => {
            $('<input>').attr({
                type: 'hidden',
                name: 'selectedGenres',
                value: genreId,
                class: 'genre-hidden-input'
            }).appendTo('#novelForm');
        });
    }

    handleWindowClick(e) {
        if (e.target.id === 'novelModal') {
            this.closeModal();
        }
        if (e.target.id === 'moderationModal') {
            this.closeModerationModal();
        }
    }

    handleEscapeKey(e) {
        if (e.keyCode === 27) {
            this.closeModal();
            this.closeModerationModal();
        }
    }

    autoHideAlerts() {
        setTimeout(() => {
            $('.novels-alert').fadeOut('slow');
        }, 5000);
    }
}

// Initialize when document is ready
$(document).ready(() => {
    window.novelManager = new NovelManager();
});