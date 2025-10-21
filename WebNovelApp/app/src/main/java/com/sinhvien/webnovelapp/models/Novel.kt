package com.sinhvien.webnovelapp.models

data class Novel(
    val Id: Int = 0,
    val Title: String = "",
    val AlternativeTitle: String? = null,
    val AuthorName: String = "",
    val Synopsis: String? = null,
    val Status: String = "",
    val AverageRating: Double = 0.0,
    val TotalRatings: Int = 0,
    val TotalChapters: Int = 0,
    val ViewCount: Long = 0,
    val BookmarkCount: Long = 0,
    val WordCount: Long = 0,
    val IsPremium: Boolean = false,
    val PublishDate: String? = null,
    val LastUpdated: String? = null,
    val Language: String? = null,
    val Genres: List<String> = emptyList()
)

data class NovelDetail(
    val Id: Int = 0,
    val Title: String = "",
    val AlternativeTitle: String? = null,
    val Synopsis: String? = null,
    val Status: String = "",
    val AverageRating: Double = 0.0,
    val TotalRatings: Int = 0,
    val TotalChapters: Int = 0,
    val ViewCount: Long = 0,
    val BookmarkCount: Long = 0,
    val WordCount: Long = 0,
    val IsPremium: Boolean = false,
    val PublishDate: String? = null,
    val LastUpdated: String? = null,
    val Language: String? = null,
    val Author: Author? = null,
    val Genres: List<GenreInfo> = emptyList(),
    val RecentChapters: List<ChapterSummary> = emptyList()
)

data class Author(
    val Id: Int = 0,
    val PenName: String = "",
    val IsVerified: Boolean = false
)

data class GenreInfo(
    val Id: Int = 0,
    val Name: String = "",
    val ColorCode: String? = null
)

data class ChapterSummary(
    val Id: Int = 0,
    val ChapterNumber: Int = 0,
    val Title: String = "",
    val WordCount: Int = 0,
    val PublishDate: String? = null,
    val IsPremium: Boolean = false,
    val UnlockPrice: Int? = null
)



data class NovelDetailResponse(
    val Success: Boolean,
    val Data: NovelDetail
)

data class ChapterListResponse(
    val Success: Boolean,
    val Data: List<ChapterSummary>,
    val TotalCount: Int = 0,
    val CurrentPage: Int = 1,
    val TotalPages: Int = 1,
    val PageSize: Int = 50
)