package com.sinhvien.webnovelapp.models
import com.google.gson.annotations.SerializedName

data class Rating(
    val id: Int? = null,

    @SerializedName("readerId")
    val readerId: Int,

    @SerializedName("novelId")
    val novelId: Int,

    @SerializedName("ratingValue")
    val ratingValue: Int,  // có thể để Int nếu chỉ nhận 1–5, backend sẽ map sang decimal

    @SerializedName("readerName")
    val readerName: String? = null,

    @SerializedName("createdAt")
    val createdAt: String? = null,

    @SerializedName("updatedAt")
    val updatedAt: String? = null
)
