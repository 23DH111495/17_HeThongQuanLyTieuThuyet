package com.sinhvien.webnovelapp.api;

import android.content.Context;
import android.util.Log; // <-- THÊM DÒNG NÀY

import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager;

import java.io.IOException;
import java.util.concurrent.TimeUnit;

import okhttp3.Interceptor;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class LoginApiClient {

    private static final String BASE_URL = "http://10.0.2.2:7200/";

    private static Retrofit retrofit = null;
    private static TokenManager tokenManager = null;

    public static void init(Context context) {
        if (tokenManager == null) {
            tokenManager = TokenManager.getInstance(context);
        }
    }

    public static Retrofit getClient() {
        if (tokenManager == null) {
            throw new IllegalStateException("LoginApiClient.init(Context) must be called before getClient()");
        }

        if (retrofit == null) {
            OkHttpClient.Builder httpClient = new OkHttpClient.Builder();

            httpClient.addInterceptor(new Interceptor() {
                @Override
                public Response intercept(Chain chain) throws IOException {
                    Request original = chain.request();
                    String token = tokenManager.getToken();

                    // ===== THÊM CÁC DÒNG NÀY VÀO =====
                    Log.d("API_TOKEN", "Interceptor đang chạy cho URL: " + original.url());
                    Log.d("API_TOKEN", "Token lấy từ TokenManager: " + token);
                    // ===================================

                    // Nếu có token, thêm vào header
                    if (token != null) {
                        Request.Builder requestBuilder = original.newBuilder()
                                .header("Authorization", "Bearer " + token)
                                .method(original.method(), original.body());
                        Request request = requestBuilder.build();

                        // ===== THÊM DÒNG NÀY VÀO =====
                        Log.d("API_TOKEN", "Đã GỬI token lên server: " + token);
                        // ===================================

                        return chain.proceed(request);
                    }


                    return chain.proceed(original);
                }
            });

            httpClient.connectTimeout(30, TimeUnit.SECONDS);
            httpClient.readTimeout(30, TimeUnit.SECONDS);
            httpClient.writeTimeout(30, TimeUnit.SECONDS);

            OkHttpClient client = httpClient.build();


            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .client(client)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit;
    }
}

