function confirmToggleStatus(form) {
    event.preventDefault(); // Ngăn form gửi ngay lập tức

    const btn = form.querySelector("button");
    const btnText = btn.innerText.trim();
    const isDeactivate = btnText.includes("Deactivate");

    const actionText = isDeactivate ? "vô hiệu hóa" : "kích hoạt";
    const color = "#77dd77";

    Swal.fire({
        title: 'Xác nhận hành động',
        html: `Bạn có chắc muốn <b>${actionText}</b> thẻ này không?`,
        icon: 'question',
        background: '#2d2d2d',
        color: '#fff',
        showCancelButton: true,
        confirmButtonColor: color,
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
        reverseButtons: true,
        customClass: {
            popup: 'rounded-xl shadow-lg',
            title: 'text-left text-xl font-semibold text-[#77dd77]',
            confirmButton: 'px-4 py-2 font-semibold rounded-md',
            cancelButton: 'px-4 py-2 font-semibold rounded-md'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            form.submit(); // Chỉ submit nếu người dùng bấm "Xác nhận"
        }
    });

    return false; // Ngăn form submit trước khi popup xử lý
}
document.addEventListener("DOMContentLoaded", function () {
    // Lắng nghe sự kiện thay đổi trên tất cả dropdown có class recycle-filter-select
    const selects = document.querySelectorAll(".recycle-filter-select");

    selects.forEach(select => {
        select.addEventListener("change", function () {
            const form = this.closest("form");
            if (form) {
                // Khi người dùng đổi filter hoặc pageSize → luôn quay về trang đầu tiên
                const pageInput = form.querySelector("input[name='page']");
                if (pageInput) pageInput.value = 1;

                // Tự động submit form
                form.submit();
            }
        });
    });
});