package com.sinhvien.webnovelapp.api

import com.google.gson.annotations.SerializedName
import com.sinhvien.webnovelapp.models.NovelResponse
import com.sinhvien.webnovelapp.models.NovelDetailResponse
import com.sinhvien.webnovelapp.models.ChapterListResponse
import com.sinhvien.webnovelapp.models.ChapterDetailResponse
import com.sinhvien.webnovelapp.models.NovelRankingResponse
import com.sinhvien.webnovelapp.models.TopNovelsResponse
import com.sinhvien.webnovelapp.models.UnlockResponse
import retrofit2.Call
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Query

interface NovelApiService {

    @GET("api/novels")
    fun getNovels(
        @Query("search") search: String = "",
        @Query("status") status: String = "",
        @Query("genreId") genreId: Int? = null,
        @Query("sortBy") sortBy: String = "updated",
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20
    ): Call<NovelResponse>

    @GET("api/novels/{id}")
    fun getNovelDetail(
        @Path("id") id: Int
    ): Call<NovelDetailResponse>

    @GET("api/novels/{novelId}/chapters")
    fun getNovelChapters(
        @Path("novelId") novelId: Int,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 50,
        @Query("userId") userId: Int? = null
    ): Call<ChapterListResponse>

    @GET("api/novels/{novelId}/chapters/{chapterNumber}")
    fun getChapterDetail(
        @Path("novelId") novelId: Int,
        @Path("chapterNumber") chapterNumber: Int,
        @Query("userId") userId: Int? = null
    ): Call<ChapterDetailResponse>

    @GET("api/novels/featured")
    fun getFeaturedNovels(): Call<NovelResponse>

    @GET("api/novels/slider-featured")
    fun getSliderFeaturedNovels(
        @Query("count") count: Int = 10
    ): Call<NovelResponse>

    @GET("api/novels/weekly-featured")
    fun getWeeklyFeaturedNovels(
        @Query("count") count: Int = 6
    ): Call<NovelResponse>

    @GET("api/novels/newly-released")
    fun getNewlyReleasedNovels(
        @Query("count") count: Int = 20
    ): Call<NovelResponse>

    @GET("api/novels/premium")
    fun getPremiumNovels(
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("sortBy") sortBy: String = "updated"
    ): Call<NovelResponse>

    @GET("api/novels/featured-list")
    fun getFeaturedNovelsList(
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("sortBy") sortBy: String = "updated"
    ): Call<NovelResponse>

    // NEW: Status filter endpoints
    @GET("api/novels/ongoing")
    fun getOngoingNovels(
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("sortBy") sortBy: String = "updated"
    ): Call<NovelResponse>

    @GET("api/novels/completed")
    fun getCompletedNovels(
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("sortBy") sortBy: String = "updated"
    ): Call<NovelResponse>

    // Ranking APIs
    @GET("api/novels/ranking")
    fun getNovelRanking(
        @Query("type") type: String = "views",
        @Query("period") period: String = "all",
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 50
    ): Call<NovelRankingResponse>

    @GET("api/novels/ranking/top")
    fun getTopNovels(
        @Query("count") count: Int = 10
    ): Call<TopNovelsResponse>

    @GET("api/novels/random")
    fun getRandomNovels(
        @Query("count") count: Int = 20,
        @Query("userId") userId: Int? = null
    ): Call<NovelResponse>

    @GET("api/novels/weighted-random")
    fun getWeightedRandomNovels(
        @Query("count") count: Int = 20,
        @Query("userId") userId: Int? = null
    ): Call<NovelResponse>

    @GET("api/novels/discover")
    fun getDiscoverNovels(
        @Query("count") count: Int = 20,
        @Query("userId") userId: Int? = null,
        @Query("preference") preference: String = "balanced"
    ): Call<NovelResponse>

    @GET("api/novels/surprise-me")
    fun getSurpriseNovel(
        @Query("userId") userId: Int? = null
    ): Call<NovelDetailResponse>
    @POST("api/novels/{novelId}/chapters/{chapterNumber}/unlock")
    fun unlockChapter(
        @Path("novelId") novelId: Int,
        @Path("chapterNumber") chapterNumber: Int
    ): Call<UnlockResponse>

    // Get user's wallet/coin balance
    @GET("api/wallet/{userId}")
    fun getUserWallet(@Path("userId") userId: Int): Call<WalletResponse>

    // Get user's unlocked chapters for a novel
    @GET("api/users/{userId}/novels/{novelId}/unlocked-chapters")
    fun getUserUnlockedChapters(
        @Path("userId") userId: Int,
        @Path("novelId") novelId: Int
    ): Call<List<UnlockedChapterInfo>>
}

data class WalletResponse(
    @SerializedName("coinBalance")
    val coinBalance: Double?,

    @SerializedName("totalCoinsSpent")
    val totalCoinsSpent: Double?
)

data class UnlockedChapterInfo(
    @SerializedName("chapterId")
    val chapterId: Int,

    @SerializedName("chapterNumber")
    val chapterNumber: Int,

    @SerializedName("unlockDate")
    val unlockDate: String?
)