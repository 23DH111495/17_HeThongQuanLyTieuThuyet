package com.sinhvien.webnovelapp.api

import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(private val tokenManager: TokenManager) : Interceptor {

    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        // Lấy token mới nhất từ TokenManager
        val token = tokenManager.getToken()

        // Nếu request KHÔNG phải là Login/Register VÀ token tồn tại
        if (!originalRequest.url.encodedPath.contains("login") &&
            !originalRequest.url.encodedPath.contains("register") &&
            !token.isNullOrEmpty()
        ) {
            val requestBuilder = originalRequest.newBuilder()
                .header("Authorization", "Bearer $token")
                .method(originalRequest.method, originalRequest.body)

            val request = requestBuilder.build()
            return chain.proceed(request)
        }

        // Đối với Login/Register hoặc nếu không có token, chạy request gốc
        return chain.proceed(originalRequest)
    }
}