function confirmToggle(id, name, isActive) {
    const action = isActive ? "deactivate" : "activate";
    const color = "#77dd77"; // màu xanh lá giống trong hình

    Swal.fire({
        title: 'Confirm Action',
        html: `Are you sure you want to ${action} "<b>${name}</b>"?`,
        icon: 'question',
        background: '#2d2d2d', // nền tối
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
document.addEventListener("DOMContentLoaded", function () {
    const statusFilter = document.querySelector("select[name='statusFilter']");
    if (statusFilter) {
        statusFilter.addEventListener("change", function () {
            // Khi thay đổi trạng thái thì tự động chuyển trang về page=1
            const pageInput = document.querySelector("input[name='page']");
            if (pageInput) pageInput.value = 1;

            // Tự động submit form
            this.form.submit();
        });
    }
});