package com.sinhvien.webnovelapp.CongThuong.Coin;

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

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CoinPackageActivity extends AppCompatActivity {

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
                    Toast.makeText(CoinPackageActivity.this, "Không tải được dữ liệu!", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<List<CoinPackage>> call, Throwable t) {
                Toast.makeText(CoinPackageActivity.this, "Lỗi kết nối server!", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
