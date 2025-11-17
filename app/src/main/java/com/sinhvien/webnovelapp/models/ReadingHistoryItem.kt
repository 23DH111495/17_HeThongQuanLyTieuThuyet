package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class ReadingHistoryItem(
    @SerializedName("novelId") val novelId: Int,
    @SerializedName("novelTitle") val novelTitle: String,
    @SerializedName("authorName") val authorName: String,
    @SerializedName("coverImageUrl") val coverImageUrl: String?,
    @SerializedName("lastReadChapterNumber") val lastReadChapterNumber: Int,
    @SerializedName("lastReadChapterTitle") val lastReadChapterTitle: String?,
    @SerializedName("lastReadDate") val lastReadDate: String,
    @SerializedName("readingStatus") val readingStatus: String,
    @SerializedName("totalReadTime") val totalReadTime: Int,
    @SerializedName("totalChapters") val totalChapters: Int,
    @SerializedName("readProgress") val readProgress: Int,
    @SerializedName("novelStatus") val novelStatus: String,
    @SerializedName("averageRating") val averageRating: Double,
    @SerializedName("genres") val genres: List<String>
)

data class ReadingHistoryResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("data") val data: List<ReadingHistoryItem>?,
    @SerializedName("pagination") val pagination: Pagination?
)

data class Pagination(
    @SerializedName("totalCount") val totalCount: Int,
    @SerializedName("currentPage") val currentPage: Int,
    @SerializedName("totalPages") val totalPages: Int,
    @SerializedName("pageSize") val pageSize: Int
)

data class UpdateProgressRequest(
    @SerializedName("novelId") val novelId: Int,
    @SerializedName("chapterId") val chapterId: Int,
    @SerializedName("readTimeSeconds") val readTimeSeconds: Int? = null,
    @SerializedName("status") val status: String? = null
)

data class UpdateStatusRequest(
    @SerializedName("novelId") val novelId: Int,
    @SerializedName("status") val status: String
)

data class NovelProgressData(
    @SerializedName("lastReadChapterNumber") val lastReadChapterNumber: Int,
    @SerializedName("lastReadChapterId") val lastReadChapterId: Int,
    @SerializedName("lastReadDate") val lastReadDate: String,
    @SerializedName("readingStatus") val readingStatus: String,
    @SerializedName("totalReadTime") val totalReadTime: Int,
    @SerializedName("readProgress") val readProgress: Int
)

data class UpdateProgressResponseData(
    @SerializedName("lastReadChapterNumber") val lastReadChapterNumber: Int,
    @SerializedName("readingStatus") val readingStatus: String,
    @SerializedName("totalReadTime") val totalReadTime: Int
)

data class ApiResponse<T>(
    val success: Boolean,
    val data: T?,
    val message: String?
)