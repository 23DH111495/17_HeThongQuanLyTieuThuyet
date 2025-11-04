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
                    LoginResponse loginResponse = response.body();
                    String token = loginResponse.getToken();

                    // THIS IS THE CRITICAL PART - SAVE THE TOKEN!
                    if (token != null && !token.isEmpty()) {
                        TokenManager.getInstance(LoginActivity.this).saveToken(token);
                        Log.d("LOGIN", "Token saved successfully: " + token);

                        // Verify token was saved
                        String savedToken = TokenManager.getInstance(LoginActivity.this).getToken();
                        Log.d("LOGIN", "Token verification: " + savedToken);

                        Toast.makeText(LoginActivity.this,
                                "Login successful! Welcome " + loginResponse.getUsername(),
                                Toast.LENGTH_SHORT).show();


                        //DUYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
                        Log.d("LoginDebug", "Login thành công, LoginResponse: " + loginResponse);
                        Log.d("LoginDebug", "LoginResponse.getId() = " + loginResponse.getId());
                        Log.d("LoginDebug", "LoginResponse.getReaderId() = " + loginResponse.getReaderId());

                        int userId = loginResponse.getId();
                        int readerId = loginResponse.getReaderId();

                        // ✅ Lưu cả 2 ID vào SharedPreferences
                        getSharedPreferences("UserRating", MODE_PRIVATE)
                                .edit()
                                .putInt("userId", userId)
                                .putInt("readerId", readerId)
                                .apply();
//DUYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY




                        // Navigate to MainActivity
                        Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                        startActivity(intent);
                        finish();
                    } else {
                        Log.e("LOGIN", "Token is null or empty!");
                        Toast.makeText(LoginActivity.this,
                                "Login failed: No token received",
                                Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Log.e("LOGIN", "Login failed with code: " + response.code());
                    try {
                        if (response.errorBody() != null) {
                            String errorBody = response.errorBody().string();
                            Log.e("LOGIN", "Error body: " + errorBody);
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    Toast.makeText(LoginActivity.this,
                            "Login failed: " + response.message(),
                            Toast.LENGTH_SHORT).show();
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