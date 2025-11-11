package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

// Phản hồi khi kiểm tra trạng thái bookmark của một truyện
data class BookmarkStatusResponse(
    // Trạng thái: TRUE nếu đã bookmark, FALSE nếu chưa
    @SerializedName("IsBookmarked") val isBookmarked: Boolean,

    // ID của bookmark nếu nó tồn tại (giúp dễ dàng cho các thao tác tiếp theo)
    @SerializedName("BookmarkId") val bookmarkId: Int?,

    // Loại bookmark hiện tại
    @SerializedName("BookmarkType") val bookmarkType: String?
)