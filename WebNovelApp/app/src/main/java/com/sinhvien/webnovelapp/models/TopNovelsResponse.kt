package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class TopNovelsResponse(
    @SerializedName("Success") val Success: Boolean,
    @SerializedName("Data") val Data: TopNovelsData
)