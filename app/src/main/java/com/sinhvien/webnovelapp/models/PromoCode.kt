package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class PromoCode(
    @SerializedName("id") val id: Int,
    @SerializedName("code") val code: String,
    @SerializedName("description") val description: String?,
    @SerializedName("promoType") val promoType: String,
    @SerializedName("value") val value: Int,
    @SerializedName("maxUses") val maxUses: Int?,
    @SerializedName("usedCount") val usedCount: Int?,
    @SerializedName("validFrom") val validFrom: String?,
    @SerializedName("validUntil") val validUntil: String?,
    @SerializedName("isActive") val isActive: Boolean
)
