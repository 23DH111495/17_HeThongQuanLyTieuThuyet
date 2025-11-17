package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.models.TagResponse
import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.Query

interface TagApiService {

    @GET("api/tags")
    fun getTags(
        @Query("search") search: String = "",
        @Query("activeOnly") activeOnly: Boolean = true
    ): Call<TagResponse>
}