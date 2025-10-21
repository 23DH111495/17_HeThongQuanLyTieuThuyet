package com.example.myapplication.activities; // Thay bằng package của bạn

import android.app.Application;
import com.paypal.checkout.PayPalCheckout;
import com.paypal.checkout.config.CheckoutConfig;
import com.paypal.checkout.config.Environment;
import com.paypal.checkout.createorder.CurrencyCode;
import com.paypal.checkout.createorder.UserAction;

public class MyApplication extends Application {

    // THAY THẾ BẰNG CLIENT ID CỦA BẠN TỪ PAYPAL DEVELOPER DASHBOARD
    private static final String YOUR_CLIENT_ID = "AeH7rv_6w309qzm7kbvaXgei87K8W4w1m6g4QPO10CiX70pT4tzzjIEbS4Z8f-r6CUSONtV65S-phkA9"; // Ví dụ: AYr_p0e_pL....

    @Override
    public void onCreate() {
        super.onCreate();

        // Khởi tạo cấu hình cho PayPal SDK
        CheckoutConfig config = new CheckoutConfig(
                this,
                YOUR_CLIENT_ID,
                Environment.SANDBOX, // hoặc Environment.LIVE cho môi trường thật
                CurrencyCode.USD, // Đơn vị tiền tệ mặc định cho các giao dịch
                UserAction.PAY_NOW, // Hành động mặc định trên nút thanh toán
                "com.example.myapplication://paypalpay" // URL để quay lại ứng dụng của bạn
        );

        // Khởi tạo PayPal SDK với cấu hình trên
        PayPalCheckout.setConfig(config);
    }
}
