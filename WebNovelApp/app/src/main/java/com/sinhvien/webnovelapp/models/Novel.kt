package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class Novel(
    @SerializedName("id")
    val Id: Int = 0,

    @SerializedName("title")
    val Title: String = "",

    @SerializedName("alternativeTitle")
    val AlternativeTitle: String? = null,

    @SerializedName("authorName")
    val AuthorName: String = "",

    @SerializedName("synopsis")
    val Synopsis: String? = null,

    @SerializedName("coverImageUrl")
    val CoverImageUrl: String? = null,

    @SerializedName("status")
    val Status: String = "",

    @SerializedName("averageRating")
    val AverageRating: Double = 0.0,

    @SerializedName("totalRatings")
    val TotalRatings: Int = 0,

    @SerializedName("totalChapters")
    val TotalChapters: Int = 0,

    @SerializedName("viewCount")
    val ViewCount: Long = 0,

    @SerializedName("bookmarkCount")
    val BookmarkCount: Long = 0,

    @SerializedName("wordCount")
    val WordCount: Long = 0,

    @SerializedName("isPremium")
    val IsPremium: Boolean = false,

    @SerializedName("publishDate")
    val PublishDate: String? = null,

    @SerializedName("lastUpdated")
    val LastUpdated: String? = null,

    @SerializedName("language")
    val Language: String? = null,

    @SerializedName("genres")
    val Genres: List<String> = emptyList()
)

data class NovelDetail(
    @SerializedName("id") val Id: Int = 0,
    @SerializedName("title") val Title: String = "",
    @SerializedName("alternativeTitle") val AlternativeTitle: String? = null,
    @SerializedName("synopsis") val Synopsis: String? = null,
    @SerializedName("status") val Status: String = "",
    @SerializedName("averageRating") val AverageRating: Double = 0.0,
    @SerializedName("totalRatings") val TotalRatings: Int = 0,
    @SerializedName("totalChapters") val TotalChapters: Int = 0,
    @SerializedName("viewCount") val ViewCount: Long = 0,
    @SerializedName("bookmarkCount") val BookmarkCount: Long = 0,
    @SerializedName("wordCount") val WordCount: Long = 0,
    @SerializedName("isPremium") val IsPremium: Boolean = false,
    @SerializedName("publishDate") val PublishDate: String? = null,
    @SerializedName("lastUpdated") val LastUpdated: String? = null,
    @SerializedName("language") val Language: String? = null,
    @SerializedName("author") val Author: Author? = null,
    @SerializedName("genres") val Genres: List<GenreInfo> = emptyList(),
    @SerializedName("tags") val Tags: List<TagInfo> = emptyList(),
    @SerializedName("recentChapters") val RecentChapters: List<ChapterSummary> = emptyList()
)

data class Author(
    @SerializedName("id") val Id: Int = 0,
    @SerializedName("penName") val PenName: String = "",
    @SerializedName("isVerified") val IsVerified: Boolean = false
)

data class GenreInfo(
    @SerializedName("id") val Id: Int = 0,
    @SerializedName("name") val Name: String = "",
    @SerializedName("colorCode") val ColorCode: String? = null
)

data class TagInfo(
    @SerializedName("id") val Id: Int = 0,
    @SerializedName("name") val Name: String = ""
)

data class NovelDetailResponse(
    @SerializedName("success") val Success: Boolean,
    @SerializedName("data") val Data: NovelDetail
)

// ===== Chapter Models =====
data class ChapterSummary(
    @SerializedName("id") val Id: Int = 0,
    @SerializedName("chapterNumber") val ChapterNumber: Int = 0,
    @SerializedName("title") val Title: String = "",
    @SerializedName("wordCount") val WordCount: Int = 0,
    @SerializedName("publishDate") val PublishDate: String? = null,
    @SerializedName("isPremium") val IsPremium: Boolean = false,
    @SerializedName("unlockPrice") val UnlockPrice: Int? = null
)

data class ChapterListResponse(
    @SerializedName("success") val Success: Boolean,
    @SerializedName("data") val Data: List<ChapterSummary>,
    @SerializedName("totalCount") val TotalCount: Int = 0,
    @SerializedName("currentPage") val CurrentPage: Int = 1,
    @SerializedName("totalPages") val TotalPages: Int = 1,
    @SerializedName("pageSize") val PageSize: Int = 50
)