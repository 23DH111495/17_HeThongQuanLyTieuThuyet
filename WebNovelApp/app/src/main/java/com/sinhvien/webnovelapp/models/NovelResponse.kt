package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class NovelResponse(
    @SerializedName("success")
    val Success: Boolean,

    @SerializedName("data")
    val Data: List<Novel>, // Use Novel directly

    @SerializedName("totalCount")
    val TotalCount: Int = 0,

    @SerializedName("currentPage")
    val CurrentPage: Int = 1,

    @SerializedName("totalPages")
    val TotalPages: Int = 1,

    @SerializedName("pageSize")
    val PageSize: Int = 20,

    @SerializedName("message")
    val Message: String? = null
)