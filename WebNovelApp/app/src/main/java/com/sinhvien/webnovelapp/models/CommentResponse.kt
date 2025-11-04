package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class CommentResponse(
    @SerializedName("id") val id: Int,
    @SerializedName("userId") val userId: Int,
    @SerializedName("username") val username: String?,
    @SerializedName("content") val content: String?,
    @SerializedName("likeCount") val likeCount: Int,
    @SerializedName("dislikeCount") val dislikeCount: Int,
    @SerializedName("hasImage") val hasImage: Boolean,
    @SerializedName("createdAt") val createdAt: String,
    @SerializedName("updatedAt") val updatedAt: String,
    @SerializedName("replies") val replies: List<CommentResponse>?
)

data class DeleteCommentRequest(
    @SerializedName("userId") val userId: Int
)

data class VoteCommentRequest(
    @SerializedName("userId") val userId: Int
)

data class VoteResponse(
    @SerializedName("likeCount") val likeCount: Int,
    @SerializedName("dislikeCount") val dislikeCount: Int
)

data class PaginatedResponse<T>(
    @SerializedName("success") val success: Boolean,
    @SerializedName("data") val data: T?,
    @SerializedName("totalCount") val totalCount: Int,
    @SerializedName("currentPage") val currentPage: Int,
    @SerializedName("totalPages") val totalPages: Int,
    @SerializedName("pageSize") val pageSize: Int,
    @SerializedName("message") val message: String?
)

data class ApiResponse<T>(
    @SerializedName("success") val success: Boolean,
    @SerializedName("message") val message: String?,
    @SerializedName("data") val data: T?
)

