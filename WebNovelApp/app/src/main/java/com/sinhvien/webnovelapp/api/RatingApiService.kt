package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.models.Rating
import retrofit2.Call
import retrofit2.http.*

interface RatingApi {

    // Lấy tất cả đánh giá của 1 truyện
    @GET("api/Rating/list/novel/{novelId}")
    fun getRatingsByNovel(@Path("novelId") novelId: Int): Call<List<Rating>>

    // Lấy điểm trung bình của 1 truyện
    @GET("api/Rating/novel/{novelId}")
    fun getAverageByNovel(@Path("novelId") novelId: Int): Call<Map<String, Any>>

    // Tạo mới hoặc cập nhật đánh giá
    @POST("api/Rating")
    fun createOrUpdateRating(@Body rating: Rating): Call<Map<String, Any>>

    // Cập nhật đánh giá theo ID (tuỳ chọn)
    @PUT("api/Rating/{id}")
    fun updateRating(@Path("id") id: Int, @Body rating: Rating): Call<Map<String, Any>>

    // Xóa đánh giá theo ID
    @DELETE("api/Rating/{id}")
    fun deleteRating(@Path("id") id: Int): Call<Map<String, Any>>
}
