package com.sinhvien.webnovelapp;

import android.app.Application;
import android.util.Log;

import com.paypal.checkout.PayPalCheckout;
import com.paypal.checkout.config.CheckoutConfig;
import com.paypal.checkout.config.Environment;
import com.paypal.checkout.createorder.CurrencyCode;
import com.paypal.checkout.createorder.UserAction;
import com.sinhvien.webnovelapp.api.LoginApiClient;
// *** SỬA 1: IMPORT ApiClient ***
import com.sinhvien.webnovelapp.api.ApiClient;

public class MyApplication extends Application {

    private static final String PAYPAL_CLIENT_ID =
            "AeH7rv_6w309qzm7kbvaXgei87K8W4w1m6g4QPO10CiX70pT4tzzjIEbS4Z8f-r6CUSONtV65S-phkA9";

    private static MyApplication instance;

    @Override
    public void onCreate() {
        super.onCreate();
        instance = this;

        // ✅ Khởi tạo Login API (Giữ nguyên)
        LoginApiClient.init(this);

        // *** SỬA 2: THÊM DÒNG NÀY ĐỂ SỬA LỖI CRASH ***
        // Khởi tạo ApiClient chính (cho Bookmark, Rating, v.v.)
        ApiClient.INSTANCE.init(this); // (Vì ApiClient là object Kotlin, ta dùng .INSTANCE)

        // ✅ Khởi tạo PayPal SDK (Giữ nguyên)
        Log.d("PayPalInit", "Initializing PayPal SDK...");
        CheckoutConfig config = new CheckoutConfig(
                this,
                PAYPAL_CLIENT_ID,
                Environment.SANDBOX,
                CurrencyCode.USD,
                UserAction.PAY_NOW,
                "com.sinhvien.webnovelapp://paypalpay"
        );
        PayPalCheckout.setConfig(config);
    }

    public static synchronized MyApplication getInstance() {
        return instance;
    }
}