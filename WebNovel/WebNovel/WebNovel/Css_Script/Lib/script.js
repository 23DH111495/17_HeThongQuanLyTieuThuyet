const tabs = {
    reading: [
        { id: 1, title: 'Hành Trình Phương Đông', author: 'Tác giả A', chapters: 245, progress: 65, genre: 'fantasy', badge: 'MỚI' },
        { id: 2, title: 'Tình Yêu Trong Mưa', author: 'Tác giả B', chapters: 180, progress: 30, genre: 'romance' },
        { id: 3, title: 'Chiến Binh Vũ Trụ', author: 'Tác giả C', chapters: 320, progress: 85, genre: 'scifi', badge: 'HOT' },
        { id: 4, title: 'Võ Lâm Truyền Kỳ', author: 'Tác giả D', chapters: 156, progress: 20, genre: 'action' },
        { id: 5, title: 'Thế Giới Ảo Ảnh', author: 'Tác giả E', chapters: 290, progress: 50, genre: 'fantasy' },
        { id: 6, title: 'Cuộc Đời Như Mơ', author: 'Tác giả F', chapters: 120, progress: 90, genre: 'romance' }
    ],
    favorite: [
        { id: 7, title: 'Huyền Thoại Rồng', author: 'Tác giả G', chapters: 400, progress: 100, genre: 'fantasy', badge: 'HOÀN' },
        { id: 8, title: 'Trái Tim Băng Giá', author: 'Tác giả H', chapters: 200, progress: 75, genre: 'romance' }
    ],
    history: [
        { id: 9, title: 'Kiếm Hiệp Giang Hồ', author: 'Tác giả I', chapters: 180, progress: 45, genre: 'action' },
        { id: 10, title: 'Vì Sao Phương Xa', author: 'Tác giả J', chapters: 150, progress: 60, genre: 'scifi' }
    ],
    downloaded: []
};

let currentTab = 'reading';
let currentFilter = 'all';

const colors = [
    'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
    'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
    'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
    'linear-gradient(135deg, #30cfd0 0%, #330867 100%)'
];

function renderBooks() {
    const container = document.getElementById('booksContainer');
    let books = tabs[currentTab];

    if (currentFilter !== 'all') {
        books = books.filter(book => book.genre === currentFilter);
    }

    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    if (searchTerm) {
        books = books.filter(book => 
            book.title.toLowerCase().includes(searchTerm) || 
            book.author.toLowerCase().includes(searchTerm)
        );
    }

    if (books.length === 0) {
        container.innerHTML = `
            <div class="empty-state" style="grid-column: 1/-1;">
                <svg fill="currentColor" viewBox="0 0 20 20">
                    <path d="M9 4.804A7.968 7.968 0 005.5 4c-1.255 0-2.443.29-3.5.804v10A7.969 7.969 0 005.5 14c1.669 0 3.218.51 4.5 1.385A7.962 7.962 0 0114.5 14c1.255 0 2.443.29 3.5.804v-10A7.968 7.968 0 0014.5 4c-1.255 0-2.443.29-3.5.804V12a1 1 0 11-2 0V4.804z"/>
                </svg>
                <h3>Chưa có sách nào</h3>
                <p>Bắt đầu thêm sách vào thư viện của bạn</p>
            </div>
        `;
        return;
    }

    container.innerHTML = books.map((book, index) => `
        <div class="book-card" onclick="openBook(${book.id})">
            <div class="book-cover" style="background: ${colors[index % colors.length]}">
                📖
                ${book.badge ? `<div class="book-badge">${book.badge}</div>` : ''}
            </div>
            <div class="book-info">
                <div class="book-title">${book.title}</div>
                <div class="book-author">${book.author}</div>
                <div class="book-stats">
                    <span>📚 ${book.chapters} chương</span>
                    <span>${book.progress}%</span>
                </div>
                ${currentTab === 'reading' ? `
                <div class="reading-progress">
                    <div class="progress-bar" style="width: ${book.progress}%"></div>
                </div>
                ` : ''}
            </div>
        </div>
    `).join('');
}

document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', function() {
        document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        this.classList.add('active');
        currentTab = this.dataset.tab;
        renderBooks();
    });
});

document.querySelectorAll('.filter-btn').forEach(btn => {
    btn.addEventListener('click', function(e) {
        e.stopPropagation();
        document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        currentFilter = this.dataset.filter;
        renderBooks();
    });
});

document.getElementById('searchInput').addEventListener('input', renderBooks);

function openBook(id) {
    alert(`Mở sách ID: ${id}\n(Chức năng đọc sách sẽ được thêm vào)`);
}

renderBooks();
