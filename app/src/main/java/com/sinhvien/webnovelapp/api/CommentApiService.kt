package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.models.ApiResponse
import okhttp3.MultipartBody
import okhttp3.RequestBody
import okhttp3.ResponseBody
import retrofit2.Call
import com.sinhvien.webnovelapp.models.CommentResponse
import com.sinhvien.webnovelapp.models.DeleteCommentRequest
import com.sinhvien.webnovelapp.models.PaginatedResponse
import com.sinhvien.webnovelapp.models.VoteCommentRequest
import com.sinhvien.webnovelapp.models.VoteResponse
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.HTTP
import retrofit2.http.Multipart
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Part
import retrofit2.http.Path
import retrofit2.http.Query

interface CommentApiService {
    @GET("api/comments/novel/{novelId}")
    suspend fun getNovelComments(
        @Path("novelId") novelId: Int,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 10,
        @Query("sortBy") sortBy: String = "newest"
    ): Response<PaginatedResponse<List<CommentResponse>>>

    @GET("api/comments/chapter/{chapterId}")
    suspend fun getChapterComments(
        @Path("chapterId") chapterId: Int,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 10
    ): Response<PaginatedResponse<List<CommentResponse>>>

    @Multipart
    @POST("api/comments")
    suspend fun createComment(
        @Part("UserId") userId: RequestBody,
        @Part("NovelId") novelId: RequestBody?,
        @Part("ChapterId") chapterId: RequestBody?,
        @Part("ParentCommentId") parentCommentId: RequestBody?,
        @Part("Content") content: RequestBody,
        @Part image: MultipartBody.Part?
    ): Response<ApiResponse<CommentResponse>>

    @Multipart
    @PUT("api/comments/{commentId}")
    suspend fun updateComment(
        @Path("commentId") commentId: Int,
        @Part("UserId") userId: RequestBody,
        @Part("Content") content: RequestBody?,
        @Part image: MultipartBody.Part?,
        @Part("RemoveImage") removeImage: RequestBody?
    ): Response<ApiResponse<Any>>

    @HTTP(method = "DELETE", path = "api/comments/{commentId}", hasBody = true)
    suspend fun deleteComment(
        @Path("commentId") commentId: Int,
        @Body request: DeleteCommentRequest
    ): Response<ApiResponse<Any>>

    @POST("api/comments/{commentId}/like")
    suspend fun likeComment(
        @Path("commentId") commentId: Int,
        @Body request: VoteCommentRequest
    ): Response<ApiResponse<VoteResponse>>

    @POST("api/comments/{commentId}/dislike")
    suspend fun dislikeComment(
        @Path("commentId") commentId: Int,
        @Body request: VoteCommentRequest
    ): Response<ApiResponse<VoteResponse>>

    @GET("api/comments/{commentId}/image")
    suspend fun getCommentImage(
        @Path("commentId") commentId: Int
    ): Response<ResponseBody>
}