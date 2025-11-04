package com.sinhvien.webnovelapp.api;

import com.sinhvien.webnovelapp.models.LoginRequest;
import com.sinhvien.webnovelapp.models.LoginResponse;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface LoginApi {

    @POST("api/Auth/login")  // ← Adjust this path to match your server endpoint
    Call<LoginResponse> login(@Body LoginRequest request);
}