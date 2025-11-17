package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.models.PromoCode
import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path


data class ApplyPromoRequest(val code: String)


data class ApplyPromoResponse(
    val message: String,
    val code: String,
    val value: Int
)

interface PromoCodeApi {


    @GET("api/PromoCode/{code}")
    fun getPromoCode(@Path("code") code: String): Call<PromoCode>


    @POST("api/PromoCode/apply")
    fun applyPromoCode(@Body request: ApplyPromoRequest): Call<ApplyPromoResponse>
}