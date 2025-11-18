// ✅ SweetAlert toggle active/inactive
function confirmToggle(id, name, isActive) {
    const action = isActive ? "deactivate" : "activate";
    const color = "#77dd77"; // màu xanh chủ đạo

    Swal.fire({
        title: 'Confirm Action',
        html: `Are you sure you want to ${action} "<b>${name}</b>"?`,
        icon: 'question',
        background: '#2d2d2d',
        color: '#fff',
        showCancelButton: true,
        confirmButtonColor: color,
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Confirm',
        cancelButtonText: 'Cancel',
        reverseButtons: true,
        customClass: {
            popup: 'rounded-xl shadow-lg',
            title: 'text-left text-xl font-semibold text-[#77dd77]',
            confirmButton: 'px-4 py-2 font-semibold rounded-md',
            cancelButton: 'px-4 py-2 font-semibold rounded-md'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            document.getElementById('toggleForm_' + id).submit();
        }
    });
}

// ✅ Lọc & phân trang tự động
document.addEventListener("DOMContentLoaded", function () {
    // Khi đổi filter (status, role, email, sort...) thì tự submit
    document.querySelectorAll(".recycle-filter-select").forEach(select => {
        select.addEventListener("change", function () {
            const form = document.getElementById("filterForm");
            if (!form) return;
            const pageInput = form.querySelector("input[name='page']");
            if (pageInput) pageInput.value = 1;
            form.submit();
        });
    });

    // Nếu có pagination thì thêm listener (tuỳ bạn thêm ở dưới)
    document.querySelectorAll(".pagination a[data-page]").forEach(link => {
        link.addEventListener("click", function (e) {
            e.preventDefault();
            const form = document.getElementById("filterForm");
            if (!form) return;
            const pageInput = form.querySelector("input[name='page']");
            if (pageInput) pageInput.value = this.dataset.page;
            form.submit();
        });
    });
});

    document.addEventListener('DOMContentLoaded', function () {
    const paginationLinks = document.querySelectorAll('.users-pagination a');
    paginationLinks.forEach(link => {
        link.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    });
});