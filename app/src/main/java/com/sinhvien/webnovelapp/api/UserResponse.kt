package com.sinhvien.webnovelapp.api

import com.google.gson.annotations.SerializedName

// Class này dùng để hứng dữ liệu JSON trả về từ API Profile
data class UserResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("data") val data: UserData?
)

data class UserData(
    @SerializedName("id") val id: Int,
    @SerializedName("username") val username: String?,
    @SerializedName("fullName") val fullName: String?,
    @SerializedName("email") val email: String?,
    @SerializedName("avatarUrl") val avatarUrl: String?,
    @SerializedName("balance") val balance: Int?,
    @SerializedName("joinDate") val joinDate: String?
)