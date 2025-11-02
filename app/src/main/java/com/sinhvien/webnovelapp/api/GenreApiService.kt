package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.models.GenreResponse
import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Path
import retrofit2.http.Query

interface GenreApiService {
    @GET("api/genres")
    fun getGenres(
        @Query("search") search: String = "",
        @Query("activeOnly") activeOnly: Boolean = true
    ): Call<GenreResponse>

    @GET("api/genres/{id}")
    fun getGenre(@Path("id") id: Int): Call<GenreResponse>
}