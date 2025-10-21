package com.example.myapplication.activities;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.R;
import com.paypal.checkout.approve.Approval;
import com.paypal.checkout.approve.OnApprove;
import com.paypal.checkout.cancel.OnCancel;
import com.paypal.checkout.createorder.CreateOrder;
import com.paypal.checkout.createorder.CreateOrderActions;
import com.paypal.checkout.createorder.CurrencyCode;
import com.paypal.checkout.createorder.OrderIntent;
import com.paypal.checkout.createorder.UserAction;
import com.paypal.checkout.error.ErrorInfo;
import com.paypal.checkout.error.OnError;
import com.paypal.checkout.order.Amount;
import com.paypal.checkout.order.AppContext;
import com.paypal.checkout.order.CaptureOrderResult;
import com.paypal.checkout.order.OnCaptureComplete;
import com.paypal.checkout.order.OrderRequest;
import com.paypal.checkout.order.PurchaseUnit;
import com.paypal.checkout.paymentbutton.PaymentButtonContainer;
// THÊM IMPORT NÀY
import com.paypal.checkout.shipping.OnShippingChange;
import com.paypal.checkout.shipping.ShippingChangeActions;
import com.paypal.checkout.shipping.ShippingChangeData;


import org.jetbrains.annotations.NotNull;

import java.util.ArrayList;

public class PaymentActivity extends AppCompatActivity {
    private static final String TAG = "PaymentActivity";
    PaymentButtonContainer paymentButtonContainer;

    // Tỷ giá hối đoái (ví dụ: 1 USD = 25,000 VNĐ)
    // CẬP NHẬT TỶ GIÁ THỰC TẾ KHI SỬ DỤNG
    private static final double VND_TO_USD_RATE = 25000.0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_payment);

        paymentButtonContainer = findViewById(R.id.payment_button_container);

        // Nhận tổng hóa đơn (giả sử là VNĐ) từ Intent
        double totalBillVND = getIntent().getDoubleExtra("totalBill", 0.0);

        // Chuyển đổi VNĐ sang USD và làm tròn đến 2 chữ số thập phân
        double totalBillUSD = Math.round((totalBillVND / VND_TO_USD_RATE) * 100.0) / 100.0;

        // Log giá trị để kiểm tra
        Log.d(TAG, "Tổng hóa đơn VNĐ: " + totalBillVND);
        Log.d(TAG, "Tổng hóa đơn USD sau chuyển đổi: " + totalBillUSD);

        if (totalBillUSD <= 0) {
            Toast.makeText(this, "Số tiền không hợp lệ.", Toast.LENGTH_SHORT).show();
            setResult(RESULT_CANCELED); // Gửi kết quả hủy về
            finish(); // Đóng activity
            return;
        }

        // Chuyển đổi giá trị double sang String để dùng trong SDK
        String paymentValue = String.valueOf(totalBillUSD);

        paymentButtonContainer.setup(
                new CreateOrder() {
                    @Override
                    public void create(@NotNull CreateOrderActions createOrderActions) {
                        ArrayList<PurchaseUnit> purchaseUnits = new ArrayList<>();
                        purchaseUnits.add(
                                new PurchaseUnit.Builder()
                                        .amount(
                                                new Amount.Builder()
                                                        .currencyCode(CurrencyCode.USD)
                                                        .value(paymentValue)
                                                        .build()
                                        )
                                        .build()
                        );
                        OrderRequest order = new OrderRequest(
                                OrderIntent.CAPTURE,
                                new AppContext.Builder()
                                        .userAction(UserAction.PAY_NOW)
                                        .build(),
                                purchaseUnits
                        );
                        createOrderActions.create(order, (CreateOrderActions.OnOrderCreated) null);
                    }
                },
                new OnApprove() {
                    @Override
                    public void onApprove(@NotNull Approval approval) {
                        approval.getOrderActions().capture(new OnCaptureComplete() {
                            @Override
                            public void onCaptureComplete(@NotNull CaptureOrderResult result) {
                                Log.d(TAG, String.format("CaptureOrderResult: %s", result));
                                Toast.makeText(PaymentActivity.this, "Thanh toán thành công!", Toast.LENGTH_SHORT).show();
                                setResult(RESULT_OK);
                                finish();
                            }
                        });
                    }
                },
                // **THÊM MỚI ĐỂ SỬA LỖI**
                // Thêm OnShippingChange ngay cả khi bạn không dùng đến.
                new OnShippingChange() {
                    @Override
                    public void onShippingChanged(@NotNull ShippingChangeData shippingChangeData, @NotNull ShippingChangeActions shippingChangeActions) {
                        // Đây là nơi bạn xử lý logic khi người dùng thay đổi địa chỉ giao hàng.
                        // Nếu không cần xử lý, bạn có thể để trống hoặc chỉ cần gọi approve().
                        shippingChangeActions.approve();
                    }
                },
                new OnCancel() {
                    @Override
                    public void onCancel() {
                        Log.d(TAG, "Người dùng đã hủy thanh toán.");
                        Toast.makeText(PaymentActivity.this, "Đã hủy thanh toán.", Toast.LENGTH_SHORT).show();
                        setResult(RESULT_CANCELED);
                        finish();
                    }
                },
                new OnError() {
                    @Override
                    public void onError(@NotNull ErrorInfo errorInfo) {
                        Log.e(TAG, String.format("PayPal Error: %s", errorInfo));
                        Toast.makeText(PaymentActivity.this, "Thanh toán thất bại, vui lòng thử lại.", Toast.LENGTH_SHORT).show();
                        setResult(RESULT_CANCELED);
                        finish();
                    }
                }
        );
    }
}
