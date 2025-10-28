package com.sinhvien.webnovelapp.models

data class Tag(
    val Id: Int = 0,
    val Name: String = "",
    val Description: String? = null,
    val Color: String? = null,
    val IsActive: Boolean = true,
    val CreatedAt: String? = null
)
