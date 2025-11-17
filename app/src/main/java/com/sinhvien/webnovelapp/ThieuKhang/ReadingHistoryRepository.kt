package com.sinhvien.webnovelapp.ThieuKhang

import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import com.sinhvien.webnovelapp.api.ReadingHistoryApiService
import com.sinhvien.webnovelapp.models.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class ReadingHistoryRepository(
    private val apiService: ReadingHistoryApiService,
    private val tokenManager: TokenManager
) {

    suspend fun getReadingHistory(
        page: Int = 1,
        pageSize: Int = 20,
        status: String = "all"
    ): Resource<ReadingHistoryResponse> = withContext(Dispatchers.IO) {
        try {
            val token = tokenManager.getToken()

            if (token.isNullOrEmpty()) {
                return@withContext Resource.Error("No authentication token")
            }

            if (tokenManager.isTokenExpired(token)) {
                tokenManager.clearToken()
                return@withContext Resource.Error("Session expired. Please login again.")
            }

            // GET USER ID FROM TOKEN
            val userId = tokenManager.getUserId()
            if (userId == 0) {
                return@withContext Resource.Error("Invalid user ID")
            }

            // PASS userId TO API
            val response = apiService.getReadingHistory("Bearer $token", userId, page, pageSize, status)

            when (response.code()) {
                200 -> {
                    if (response.body() != null) {
                        Resource.Success(response.body()!!)
                    } else {
                        Resource.Error("Empty response")
                    }
                }
                401 -> {
                    tokenManager.clearToken()
                    Resource.Error("Session expired. Please login again.")
                }
                else -> {
                    Resource.Error("Failed to load reading history: ${response.message()}")
                }
            }
        } catch (e: Exception) {
            Resource.Error("Network error: ${e.message}")
        }
    }

    suspend fun updateReadingProgress(
        novelId: Int,
        chapterId: Int,
        readTimeSeconds: Int? = null,
        status: String? = null
    ): Resource<String> = withContext(Dispatchers.IO) {
        try {
            val token = tokenManager.getToken()
                ?: return@withContext Resource.Error("No authentication token")

            val userId = tokenManager.getUserId()
            if (userId == 0) {
                return@withContext Resource.Error("Invalid user ID")
            }

            val authHeader = if (token.startsWith("Bearer ")) token else "Bearer $token"
            android.util.Log.d("READING_HISTORY", "Auth header: ${authHeader.take(50)}...")
            android.util.Log.d("READING_HISTORY", "userId from TokenManager: $userId")

            val request = UpdateProgressRequest(novelId, chapterId, readTimeSeconds, status)
            android.util.Log.d("READING_HISTORY", "Request body: $request")

            // Pass userId as query parameter
            val response = apiService.updateReadingProgress(authHeader, userId, request)

            android.util.Log.d("READING_HISTORY", "Response code: ${response.code()}")
            android.util.Log.d("READING_HISTORY", "Response message: ${response.message()}")

            when (response.code()) {
                200 -> {
                    response.body()?.let {
                        if (it.success) {
                            return@withContext Resource.Success(it.message ?: "Progress updated")
                        }
                    }
                    Resource.Error("Update failed: empty success response")
                }
                400 -> {
                    val error = response.errorBody()?.string()
                    android.util.Log.e("READING_HISTORY", "400 Error: $error")
                    Resource.Error("Bad request: $error")
                }
                401 -> {
                    tokenManager.clearToken()
                    Resource.Error("Session expired. Please login again.")
                }
                else -> {
                    Resource.Error("Failed to update progress: ${response.message()}")
                }
            }
        } catch (e: Exception) {
            android.util.Log.e("READING_HISTORY", "Exception: ${e.message}", e)
            Resource.Error("Network error: ${e.message}")
        }
    }

    suspend fun updateReadingStatus(
        novelId: Int,
        status: String
    ): Resource<String> = withContext(Dispatchers.IO) {
        try {
            val token = tokenManager.getToken()

            if (token.isNullOrEmpty()) {
                return@withContext Resource.Error("No authentication token")
            }

            if (tokenManager.isTokenExpired(token)) {
                tokenManager.clearToken()
                return@withContext Resource.Error("Session expired. Please login again.")
            }

            val request = UpdateStatusRequest(novelId, status)
            val response = apiService.updateReadingStatus("Bearer $token", request)

            when (response.code()) {
                200 -> {
                    if (response.body()?.success == true) {
                        Resource.Success("Status updated")
                    } else {
                        Resource.Error("Failed to update status")
                    }
                }
                401 -> {
                    tokenManager.clearToken()
                    Resource.Error("Session expired. Please login again.")
                }
                else -> {
                    Resource.Error("Failed to update status: ${response.message()}")
                }
            }
        } catch (e: Exception) {
            android.util.Log.e("READING_HISTORY", "Exception: ${e.message}", e)
            Resource.Error("Network error: ${e.message}")
        }
    }

    suspend fun deleteReadingProgress(novelId: Int): Resource<String> = withContext(Dispatchers.IO) {
        try {
            val token = tokenManager.getToken()

            if (token.isNullOrEmpty()) {
                return@withContext Resource.Error("No authentication token")
            }

            if (tokenManager.isTokenExpired(token)) {
                tokenManager.clearToken()
                return@withContext Resource.Error("Session expired. Please login again.")
            }

            val response = apiService.deleteReadingProgress("Bearer $token", novelId)

            when (response.code()) {
                200 -> {
                    if (response.body()?.success == true) {
                        Resource.Success("Removed from history")
                    } else {
                        Resource.Error("Failed to remove")
                    }
                }
                401 -> {
                    tokenManager.clearToken()
                    Resource.Error("Session expired. Please login again.")
                }
                else -> {
                    Resource.Error("Failed to remove: ${response.message()}")
                }
            }
        } catch (e: Exception) {
            android.util.Log.e("READING_HISTORY", "Exception: ${e.message}", e)
            Resource.Error("Network error: ${e.message}")
        }
    }

    suspend fun getNovelProgress(novelId: Int): Resource<NovelProgressData?> = withContext(Dispatchers.IO) {
        try {
            val token = tokenManager.getToken()

            if (token.isNullOrEmpty()) {
                return@withContext Resource.Error("No authentication token")
            }

            if (tokenManager.isTokenExpired(token)) {
                tokenManager.clearToken()
                return@withContext Resource.Error("Session expired. Please login again.")
            }

            val response = apiService.getNovelProgress("Bearer $token", novelId)

            when (response.code()) {
                200 -> {
                    if (response.body()?.success == true) {
                        Resource.Success(response.body()?.data)
                    } else {
                        Resource.Error("Failed to load progress")
                    }
                }
                401 -> {
                    tokenManager.clearToken()
                    Resource.Error("Session expired. Please login again.")
                }
                else -> {
                    Resource.Error("Failed to load progress: ${response.message()}")
                }
            }
        } catch (e: Exception) {
            android.util.Log.e("READING_HISTORY", "Exception: ${e.message}", e)
            Resource.Error("Network error: ${e.message}")
        }
    }
}