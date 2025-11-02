package com.sinhvien.webnovelapp.models

data class GenreResponse(
    val Success: Boolean = false,
    val Data: List<Genre> = emptyList(),
    val Count: Int = 0
)