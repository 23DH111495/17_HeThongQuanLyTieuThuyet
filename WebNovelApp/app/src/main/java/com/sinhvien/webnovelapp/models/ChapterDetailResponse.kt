package com.sinhvien.webnovelapp.models

data class ChapterDetailResponse(
    val Success: Boolean,
    val Data: ChapterDetail,
    val AccessInfo: AccessInfo,
    val Novel: NovelInfo
)

data class ChapterDetail(
    val Id: Int,
    val NovelId: Int,
    val ChapterNumber: Int,
    val Title: String,
    val Content: String?,
    val WordCount: Int,
    val PublishDate: String?,
    val IsPremium: Boolean,
    val UnlockPrice: Int,
    val PreviewContent: String?
)

data class AccessInfo(
    val HasAccess: Boolean,
    val AccessReason: String,
    val RequiredCoins: Int,
    val IsPremium: Boolean
)

data class NovelInfo(
    val Id: Int,
    val Title: String,
    val AuthorName: String
)