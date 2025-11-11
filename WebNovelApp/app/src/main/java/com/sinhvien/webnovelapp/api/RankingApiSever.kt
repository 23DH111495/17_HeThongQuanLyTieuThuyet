package com.sinhvien.webnovelapp.api

// (SỬA) Import 2 model mới của bạn
import com.sinhvien.webnovelapp.models.NovelRankingDto
import com.sinhvien.webnovelapp.models.NovelRankingResponse
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Query

interface RankingApiService {

    @GET("api/novels/ranking")
    suspend fun getRanking(
        @Query("type") type: String,
        @Query("period") period: String,
        @Query("page") page: Int,
        @Query("pageSize") pageSize: Int
        // (SỬA) Dùng model mới NovelRankingResponse
    ): Response<NovelRankingResponse>
}