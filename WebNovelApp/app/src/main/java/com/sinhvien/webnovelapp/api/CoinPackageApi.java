package com.sinhvien.webnovelapp.api;

import com.sinhvien.webnovelapp.models.CoinPackage;

import java.util.List;
import com.sinhvien.webnovelapp.models.Wallet;

import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;

public interface CoinPackageApi {

    // API GET: trả về danh sách gói coin
    @GET("api/CoinPackage")
    Call<List<CoinPackage>> getAllCoinPackages();

    // API POST: người dùng mua gói theo id
    @POST("api/CoinPackage/buy/{id}")
    Call<Wallet> buyPackage(@Path("id") int id);
}
