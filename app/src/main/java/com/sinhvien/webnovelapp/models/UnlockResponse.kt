package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class UnlockResponse(
    @SerializedName("success")
    val success: Boolean,

    @SerializedName("message")
    val message: String?,

    @SerializedName("data")
    val data: NewBalanceData?
)

data class NewBalanceData(
    @SerializedName("newCoinBalance")
    val newCoinBalance: Double,

    @SerializedName("coinsSpent")
    val coinsSpent: Int
)