package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

// Dữ liệu gửi đi khi nhấn nút Thêm/Xóa Bookmark
data class BookmarkToggleRequest(
    // NovelId là trường bắt buộc để biết truyện nào
    @SerializedName("NovelId") val novelId: Int,

    // Loại Bookmark (mặc định là Favorite)
    @SerializedName("BookmarkType") val bookmarkType: String? = "Favorite"

    // Lưu ý: KHÔNG cần ReaderId ở đây vì nó được lấy từ Token trên Server
)