package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.models.Bookmark
import com.sinhvien.webnovelapp.models.BookmarkResponse
import com.sinhvien.webnovelapp.models.BookmarkToggleRequest
import com.sinhvien.webnovelapp.models.BookmarkStatusResponse
import retrofit2.Call
import retrofit2.http.*

interface BookmarkApiService {

    // 1. Lấy danh sách bookmarks của reader
    // ReaderId được server lấy từ Token (Authorization Header), không cần gửi query
    @GET("api/Bookmarks/my-bookmarks")
    fun getMyBookmarks(): Call<BookmarkResponse>

    // 2. Toggle bookmark (thêm/xóa)
    // ReaderId được server lấy từ Token. NovelId và BookmarkType được gửi trong Body
    @POST("api/Bookmarks/toggle")
    fun toggleBookmark(
        // Chỉ cần gửi NovelId và loại bookmark. ReaderId sẽ được server tự động thêm vào
        @Body request: BookmarkToggleRequest
    ): Call<Map<String, Any>> // Map<String, Any> để nhận phản hồi JSON dạng {success: true, action: "added/removed"}

    // 3. Kiểm tra trạng thái bookmark
    // ReaderId được server lấy từ Token. NovelId được gửi qua Path
    @GET("api/Bookmarks/status/{novelId}")
    fun getBookmarkStatus(
        @Path("novelId") novelId: Int // Truyền ID truyện vào URL
    ): Call<BookmarkStatusResponse>

    // 4. Đếm số bookmark
    // ReaderId được server lấy từ Token
    @GET("api/Bookmarks/count")
    fun getBookmarkCount(): Call<Map<String, Any>>
}