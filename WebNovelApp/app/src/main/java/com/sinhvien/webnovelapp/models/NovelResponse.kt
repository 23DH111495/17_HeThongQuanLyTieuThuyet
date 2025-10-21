package com.sinhvien.webnovelapp.models

data class NovelResponse(
    val Success: Boolean,
    val Data: List<Novel>,
    val TotalCount: Int = 0,
    val CurrentPage: Int = 1,
    val TotalPages: Int = 1,
    val PageSize: Int = 20
)