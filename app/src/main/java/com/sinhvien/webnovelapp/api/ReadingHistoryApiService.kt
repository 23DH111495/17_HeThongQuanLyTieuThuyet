package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.models.ApiResponse
import com.sinhvien.webnovelapp.models.NovelProgressData
import com.sinhvien.webnovelapp.models.ReadingHistoryResponse
import com.sinhvien.webnovelapp.models.UpdateProgressRequest
import com.sinhvien.webnovelapp.models.UpdateProgressResponseData
import com.sinhvien.webnovelapp.models.UpdateStatusRequest
import retrofit2.Response
import retrofit2.http.*

interface ReadingHistoryApiService {

    @GET("api/reading-history")
    suspend fun getReadingHistory(
        @Header("Authorization") token: String,
        @Query("userId") userId: Int,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("status") status: String = "all"
    ): Response<ReadingHistoryResponse>

    @POST("api/reading-history/update")
    suspend fun updateReadingProgress(
        @Header("Authorization") token: String,
        @Query("userId") userId: Int,
        @Body request: UpdateProgressRequest
    ): Response<ApiResponse<UpdateProgressResponseData>>

    @PUT("api/reading-history/status")
    suspend fun updateReadingStatus(
        @Header("Authorization") token: String,
        @Body request: UpdateStatusRequest
    ): Response<ApiResponse<Any>>

    @DELETE("api/reading-history/{novelId}")
    suspend fun deleteReadingProgress(
        @Header("Authorization") token: String,
        @Path("novelId") novelId: Int
    ): Response<ApiResponse<Any>>

    @GET("api/reading-history/{novelId}")
    suspend fun getNovelProgress(
        @Header("Authorization") token: String,
        @Path("novelId") novelId: Int
    ): Response<ApiResponse<NovelProgressData>>

    @POST("api/reading-history/update")
    suspend fun updateProgress(
        @Query("userId") userId: Int,
        @Body request: UpdateProgressRequest
    ): UpdateProgressResponseData
}