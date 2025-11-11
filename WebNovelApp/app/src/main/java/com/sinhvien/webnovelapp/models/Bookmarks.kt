package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

// Model Bookmark (Entity từ Server)
data class Bookmark(
    @SerializedName("BookmarkId") val bookmarkId: Int = 0,
    @SerializedName("NovelId") val novelId: Int = 0,
    @SerializedName("ReaderId") val readerId: Int = 0, // Cần trường này để hiển thị hoặc debug
    @SerializedName("BookmarkType") val bookmarkType: String = "default",
    @SerializedName("CreatedAt") val createdAt: String? = null,
    // Thông tin chi tiết truyện được nhúng vào
    @SerializedName("Novel") val novel: NovelBookmark? = null
)

// Model Chi tiết truyện được nhúng trong Bookmark
data class NovelBookmark(
    @SerializedName("Id") val id: Int,
    @SerializedName("Title") val title: String,
    @SerializedName("AlternativeTitle") val alternativeTitle: String?,
    @SerializedName("CoverImageUrl") val coverImageUrl: String?,
    @SerializedName("CoverImageFileName") val coverImageFileName: String?,
    @SerializedName("Synopsis") val synopsis: String?,
    @SerializedName("AuthorId") val authorId: Int,
    @SerializedName("ViewCount") val viewCount: Long,
    @SerializedName("AverageRating") val averageRating: Double,
    @SerializedName("BookmarkCount") val bookmarkCount: Long,
    @SerializedName("Status") val status: String?,
    @SerializedName("Language") val language: String?,
    @SerializedName("TotalChapters") val totalChapters: Int,
    @SerializedName("LastUpdated") val lastUpdated: String?,
    @SerializedName("IsFeatured") val isFeatured: Boolean,
    @SerializedName("IsPremium") val isPremium: Boolean
)