package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

// Response khi lấy danh sách bookmarks
data class BookmarkResponse(
    @SerializedName("TotalBookmarks") val totalBookmarks: Int,
    @SerializedName("Bookmarks") val bookmarks: List<Bookmark>
)