package com.sinhvien.webnovelapp.models

data class TagResponse(
    val Success: Boolean,
    val Data: List<Tag>,
    val Count: Int
)