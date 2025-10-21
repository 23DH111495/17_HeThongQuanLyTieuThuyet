package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class NovelRankingDto(
    @SerializedName("Rank") val Rank: Int,
    @SerializedName("Id") val Id: Int,
    @SerializedName("Title") val Title: String,
    @SerializedName("AuthorName") val AuthorName: String,
    @SerializedName("Synopsis") val Synopsis: String,
    @SerializedName("Status") val Status: String,
    @SerializedName("AverageRating") val AverageRating: Double,
    @SerializedName("TotalRatings") val TotalRatings: Int,
    @SerializedName("TotalChapters") val TotalChapters: Int,
    @SerializedName("ViewCount") val ViewCount: Long,
    @SerializedName("BookmarkCount") val BookmarkCount: Long,
    @SerializedName("WordCount") val WordCount: Long,
    @SerializedName("IsPremium") val IsPremium: Boolean,
    @SerializedName("LastUpdated") val LastUpdated: String,
    @SerializedName("Genres") val Genres: List<String>
)

data class NovelRankingResponse(
    @SerializedName("Success") val Success: Boolean,
    @SerializedName("Data") val Data: List<NovelRankingDto>,
    @SerializedName("TotalCount") val TotalCount: Int,
    @SerializedName("CurrentPage") val CurrentPage: Int,
    @SerializedName("TotalPages") val TotalPages: Int,
    @SerializedName("PageSize") val PageSize: Int,
    @SerializedName("Message") val Message: String?
)

data class TopNovelsData(
    @SerializedName("MostViewed") val MostViewed: List<Novel>,
    @SerializedName("HighestRated") val HighestRated: List<Novel>,
    @SerializedName("MostBookmarked") val MostBookmarked: List<Novel>,
    @SerializedName("MostChapters") val MostChapters: List<Novel>
)

