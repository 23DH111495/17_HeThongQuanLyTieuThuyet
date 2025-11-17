package com.sinhvien.webnovelapp.CongThuong.Login;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.sinhvien.webnovelapp.R;
import com.sinhvien.webnovelapp.ThieuKhang.MainActivity;
import com.sinhvien.webnovelapp.api.UserApi;  // ← CHANGE THIS
import com.sinhvien.webnovelapp.api.LoginApiClient;
import com.sinhvien.webnovelapp.models.LoginRequest;
import com.sinhvien.webnovelapp.models.LoginResponse;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {

    private EditText edtUsername, edtPassword;
    private Button btnLogin;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        // Initialize LoginApiClient - VERY IMPORTANT!
        LoginApiClient.init(this);

        edtUsername = findViewById(R.id.etUsername);
        edtPassword = findViewById(R.id.etPassword);
        btnLogin = findViewById(R.id.btnLogin);

        btnLogin.setOnClickListener(v -> performLogin());
    }

    private void performLogin() {
        String username = edtUsername.getText().toString().trim();
        String password = edtPassword.getText().toString().trim();

        if (username.isEmpty() || password.isEmpty()) {
            Toast.makeText(this, "Please enter username and password", Toast.LENGTH_SHORT).show();
            return;
        }

        // Use UserApi instead of LoginApi
        UserApi api = LoginApiClient.getClient().create(UserApi.class);  // ← CHANGE THIS
        LoginRequest request = new LoginRequest(username, password);

        // Call the login endpoint
        api.login(request).enqueue(new Callback<LoginResponse>() {
            @Override
            public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                Log.d("LOGIN", "Response Code: " + response.code());

                if (response.isSuccessful() && response.body() != null) {
                    LoginResponse res = response.body();
                    String token = res.getToken();

                    if (token != null && !token.isEmpty()) {

                        TokenManager.getInstance(LoginActivity.this).saveUserSession(
                                token,
                                res.getId(),
                                res.getUsername(),
                                res.getEmail()
                        );

                        Log.d("LOGIN", "Token saved successfully: " + token);

                        Toast.makeText(LoginActivity.this,
                                "Login successful! Welcome " + res.getUsername(),
                                Toast.LENGTH_SHORT).show();

                        // Chuyển màn hình
                        Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                        startActivity(intent);
                        finish();
                    } else {
                        Log.e("LOGIN", "Token is null or empty!");
                        Toast.makeText(LoginActivity.this, "Login failed: No token", Toast.LENGTH_SHORT).show();
                    }
                } else {
                    // ... phần xử lý lỗi giữ nguyên
                    Log.e("LOGIN", "Login failed with code: " + response.code());
                    Toast.makeText(LoginActivity.this, "Login failed", Toast.LENGTH_SHORT).show();
                }
            }
            @Override
            public void onFailure(Call<LoginResponse> call, Throwable t) {
                Log.e("LOGIN", "Login request failed: " + t.getMessage(), t);
                Toast.makeText(LoginActivity.this,
                        "Error: " + t.getMessage(),
                        Toast.LENGTH_SHORT).show();
            }
        });
    }
}