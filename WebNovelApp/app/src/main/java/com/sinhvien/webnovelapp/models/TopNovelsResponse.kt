package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class TopNovelsResponse(
    @SerializedName("success") val Success: Boolean,
    @SerializedName("data") val Data: TopNovelsData
)