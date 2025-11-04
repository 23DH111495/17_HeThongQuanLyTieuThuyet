package com.sinhvien.webnovelapp;

import android.app.Application;
import android.util.Log;

import com.paypal.checkout.PayPalCheckout;
import com.paypal.checkout.config.CheckoutConfig;
import com.paypal.checkout.config.Environment;
import com.paypal.checkout.createorder.CurrencyCode;
import com.paypal.checkout.createorder.UserAction;
import com.sinhvien.webnovelapp.api.LoginApiClient;

public class MyApplication extends Application {

    private static final String PAYPAL_CLIENT_ID =
            "AeH7rv_6w309qzm7kbvaXgei87K8W4w1m6g4QPO10CiX70pT4tzzjIEbS4Z8f-r6CUSONtV65S-phkA9";

    @Override
    public void onCreate() {
        super.onCreate();

        // ✅ Khởi tạo Login API
        LoginApiClient.init(this);

        // ✅ Khởi tạo PayPal SDK
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
}
