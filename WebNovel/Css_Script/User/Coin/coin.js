document.addEventListener("DOMContentLoaded", () => {
    const modal = document.getElementById("paymentModal");
    const box = document.getElementById("paymentBox");
    const closeModal = document.getElementById("closeModal");
    const modalTitle = document.getElementById("modalTitle");
    let selectedPackage = null;

    // Khi click “Mua ngay”
    document.querySelectorAll(".buy-now-btn").forEach(btn => {
        btn.addEventListener("click", e => {
            e.preventDefault();
            const wrapper = btn.closest(".coin-form");
            const pkgName = wrapper.dataset.name;
            const pkgPrice = wrapper.dataset.price;
            selectedPackage = wrapper.dataset.id;

            modalTitle.innerHTML = `Mua gói <strong>${pkgName}</strong><br><span class="text-sm text-gray-600">Giá: ${pkgPrice}₫</span>`;

            modal.classList.remove("hidden");
            setTimeout(() => {
                box.classList.remove("opacity-0", "scale-95");
                box.classList.add("opacity-100", "scale-100");
            }, 50);
        });
    });

    // Khi chọn phương thức thanh toán
    document.querySelectorAll(".payment-option").forEach(opt => {
        opt.addEventListener("click", e => {
            e.preventDefault();
            const method = opt.dataset.method;

            // Ẩn modal
            box.classList.add("opacity-0", "scale-95");
            setTimeout(() => {
                modal.classList.add("hidden");

                // Nếu chọn PayPal → chuyển hướng sang trang thanh toán PayPal
                if (method === "paypal") {
                    window.location.href = `/user/PaymentWithPayPal?packageId=${selectedPackage}`;
                }
                else {
                    alert(`Bạn đã chọn thanh toán bằng ${method} cho gói ID ${selectedPackage}`);
                }
            }, 150);
        });
    });

    // Nút “Hủy”
    closeModal.addEventListener("click", () => {
        box.classList.add("opacity-0", "scale-95");
        setTimeout(() => modal.classList.add("hidden"), 150);
    });
});