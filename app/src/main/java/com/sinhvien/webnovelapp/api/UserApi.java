package com.sinhvien.webnovelapp.api;

import com.sinhvien.webnovelapp.models.LoginRequest;
import com.sinhvien.webnovelapp.models.LoginResponse;

import com.sinhvien.webnovelapp.api.UserResponse;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;

public interface UserApi {


    @POST("api/user/login")
    Call<LoginResponse> login(@Body LoginRequest request);


    @GET("api/user/profile")
    Call<UserResponse> getUserProfile();
}