package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class NovelRankingDto(
    @SerializedName("rank") val Rank: Int,
    @SerializedName("id") val Id: Int,
    @SerializedName("title") val Title: String,
    @SerializedName("authorName") val AuthorName: String,
    @SerializedName("synopsis") val Synopsis: String,
    @SerializedName("status") val Status: String,
    @SerializedName("averageRating") val AverageRating: Double,
    @SerializedName("totalRatings") val TotalRatings: Int,
    @SerializedName("totalChapters") val TotalChapters: Int,
    @SerializedName("viewCount") val ViewCount: Long,
    @SerializedName("bookmarkCount") val BookmarkCount: Long,
    @SerializedName("wordCount") val WordCount: Long,
    @SerializedName("isPremium") val IsPremium: Boolean,
    @SerializedName("lastUpdated") val LastUpdated: String,
    @SerializedName("genres") val Genres: List<String>
)

data class NovelRankingResponse(
    @SerializedName("success") val Success: Boolean,
    @SerializedName("data") val Data: List<NovelRankingDto>,
    @SerializedName("totalCount") val TotalCount: Int = 0,
    @SerializedName("currentPage") val CurrentPage: Int = 1,
    @SerializedName("totalPages") val TotalPages: Int = 1,
    @SerializedName("pageSize") val PageSize: Int = 50,
    @SerializedName("message") val Message: String? = null
)

data class TopNovelsData(
    @SerializedName("mostViewed") val MostViewed: List<Novel>,
    @SerializedName("highestRated") val HighestRated: List<Novel>,
    @SerializedName("mostBookmarked") val MostBookmarked: List<Novel>,
    @SerializedName("mostChapters") val MostChapters: List<Novel>
)

