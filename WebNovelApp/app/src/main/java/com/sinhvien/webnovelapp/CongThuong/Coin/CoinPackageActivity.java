package com.sinhvien.webnovelapp.CongThuong.Coin;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Toast;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.sinhvien.webnovelapp.R;
import com.sinhvien.webnovelapp.api.CoinPackageApi;
import com.sinhvien.webnovelapp.api.LoginApiClient;
import com.sinhvien.webnovelapp.models.CoinPackage;
import com.sinhvien.webnovelapp.models.Wallet;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CoinPackageActivity extends AppCompatActivity {

    private static final int PAYPAL_REQUEST_CODE = 123;

    private RecyclerView recyclerView;
    private CoinPackageAdapter adapter;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_coin_packages);

        recyclerView = findViewById(R.id.recyclerViewPackages);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));

        loadCoinPackages();
    }

    private void loadCoinPackages() {
        CoinPackageApi api = LoginApiClient.getClient().create(CoinPackageApi.class);
        api.getAllCoinPackages().enqueue(new Callback<List<CoinPackage>>() {
            @Override
            public void onResponse(Call<List<CoinPackage>> call, Response<List<CoinPackage>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    adapter = new CoinPackageAdapter(CoinPackageActivity.this, response.body());
                    recyclerView.setAdapter(adapter);
                } else {
                    Toast.makeText(CoinPackageActivity.this, "Không tải được dữ liệu gói coin!", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<List<CoinPackage>> call, Throwable t) {
                Toast.makeText(CoinPackageActivity.this, "Lỗi kết nối server: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    // ✅ Nhận kết quả thanh toán từ PaymentActivity
    @Override
    protected void onActivityResult(int requestCode, int resultCode, @Nullable Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == PAYPAL_REQUEST_CODE) {
            if (resultCode == RESULT_OK && data != null) {
                int packageId = data.getIntExtra("packageId", -1);

                if (packageId != -1) {
                    // Thanh toán thành công → gọi API cộng coin
                    processBuyPackage(packageId);
                } else {
                    Toast.makeText(this, "Không tìm thấy thông tin gói thanh toán.", Toast.LENGTH_SHORT).show();
                }

            } else if (resultCode == RESULT_CANCELED) {
                Toast.makeText(this, "Thanh toán bị hủy hoặc thất bại.", Toast.LENGTH_SHORT).show();
            }
        }
    }

    // ✅ Gọi API mua gói sau khi thanh toán thành công
    private void processBuyPackage(int packageId) {
        CoinPackageApi api = LoginApiClient.getClient().create(CoinPackageApi.class);

        api.buyPackage(packageId).enqueue(new Callback<Wallet>() {
            @Override
            public void onResponse(Call<Wallet> call, Response<Wallet> response) {
                if (response.isSuccessful() && response.body() != null) {
                    int newBalance = response.body().getCoinBalance();
                    Toast.makeText(CoinPackageActivity.this,
                            "Thanh toán & mua gói thành công! Số dư mới: " + newBalance,
                            Toast.LENGTH_LONG).show();
                } else {
                    Toast.makeText(CoinPackageActivity.this,
                            "Mua gói thất bại sau khi thanh toán! Mã lỗi: " + response.code(),
                            Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Wallet> call, Throwable t) {
                Toast.makeText(CoinPackageActivity.this,
                        "Lỗi kết nối server khi cộng coin: " + t.getMessage(),
                        Toast.LENGTH_SHORT).show();
            }
        });
    }
}
