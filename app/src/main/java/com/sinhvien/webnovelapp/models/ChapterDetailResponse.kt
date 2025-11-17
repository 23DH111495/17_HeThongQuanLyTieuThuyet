package com.sinhvien.webnovelapp.models

import com.google.gson.annotations.SerializedName

data class ChapterDetailResponse(
    @SerializedName("success") val Success: Boolean,
    @SerializedName("data") val Data: ChapterDetail,
    @SerializedName("accessInfo") val AccessInfo: AccessInfo,
    @SerializedName("novel") val Novel: NovelInfo
)

data class ChapterDetail(
    @SerializedName("id") val Id: Int,
    @SerializedName("novelId") val NovelId: Int,
    @SerializedName("chapterNumber") val ChapterNumber: Int,
    @SerializedName("title") val Title: String,
    @SerializedName("content") val Content: String?,
    @SerializedName("wordCount") val WordCount: Int,
    @SerializedName("publishDate") val PublishDate: String?,
    @SerializedName("isPremium") val IsPremium: Boolean,
    @SerializedName("unlockPrice") val UnlockPrice: Int,
    @SerializedName("previewContent") val PreviewContent: String?
)

data class AccessInfo(
    @SerializedName("hasAccess") val HasAccess: Boolean,
    @SerializedName("accessReason") val AccessReason: String,
    @SerializedName("requiredCoins") val RequiredCoins: Int,
    @SerializedName("isPremium") val IsPremium: Boolean
)

data class NovelInfo(
    @SerializedName("id") val Id: Int,
    @SerializedName("title") val Title: String,
    @SerializedName("authorName") val AuthorName: String
)