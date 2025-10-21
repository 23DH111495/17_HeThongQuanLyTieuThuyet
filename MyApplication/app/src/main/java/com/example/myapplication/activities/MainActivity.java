package com.example.myapplication.activities;

import android.content.Intent;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.example.myapplication.R;

public class MainActivity extends AppCompatActivity {

    private static final int PAYPAL_REQUEST_CODE = 123;

    private EditText edtAmount;
    private Button btnProceedToPayment;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main); // Sử dụng layout bạn vừa tạo

        edtAmount = findViewById(R.id.edtAmount);
        btnProceedToPayment = findViewById(R.id.btnProceedToPayment);

        // ĐÂY LÀ NƠI BẠN THÊM LOGIC CHO NÚT BẤM
        btnProceedToPayment.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String amountString = edtAmount.getText().toString();

                // Kiểm tra xem người dùng đã nhập số tiền chưa
                if (TextUtils.isEmpty(amountString)) {
                    Toast.makeText(MainActivity.this, "Vui lòng nhập số tiền", Toast.LENGTH_SHORT).show();
                    return;
                }

                // Chuyển đổi số tiền sang kiểu double
                double totalBill = Double.parseDouble(amountString);

                // Khởi chạy PaymentActivity và gửi số tiền qua Intent
                Intent intent = new Intent(MainActivity.this, PaymentActivity.class);
                intent.putExtra("totalBill", totalBill); // "totalBill" là key mà PaymentActivity đang chờ
                startActivityForResult(intent, PAYPAL_REQUEST_CODE);
            }
        });
    }

    // Hàm này sẽ xử lý kết quả trả về từ PaymentActivity
    @Override
    protected void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == PAYPAL_REQUEST_CODE) {
            if (resultCode == RESULT_OK) {
                // Thanh toán thành công
                Toast.makeText(this, "Thanh toán thành công!", Toast.LENGTH_LONG).show();
                edtAmount.setText(""); // Xóa số tiền đã nhập
            } else if (resultCode == RESULT_CANCELED) {
                // Thanh toán bị hủy hoặc thất bại
                Toast.makeText(this, "Thanh toán đã bị hủy hoặc thất bại.", Toast.LENGTH_SHORT).show();
            }
        }
    }
}
