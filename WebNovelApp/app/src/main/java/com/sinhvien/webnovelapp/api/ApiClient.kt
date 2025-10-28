package com.sinhvien.webnovelapp.api

import android.util.Log
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {
//    private const val BASE_URL_DEVICE = "http://192.168.101.4//"
    private const val BASE_URL_DEVICE = "http://10.21.9.168//"
    private const val BASE_URL = BASE_URL_DEVICE

    private var retrofit: Retrofit? = null

    fun getClient(): Retrofit {
        if (retrofit == null) {
            // Add logging interceptor to see HTTP requests/responses
            val logging = HttpLoggingInterceptor().apply {
                level = HttpLoggingInterceptor.Level.BODY
            }

            val client = OkHttpClient.Builder()
                // CRITICAL: Add Host header interceptor FIRST
                .addInterceptor { chain ->
                    val original = chain.request()
                    val modified = original.newBuilder()
                        .removeHeader("Host")
                        .addHeader("Host", "localhost")
                        .build()

                    Log.d("API_CLIENT", "Request URL: ${modified.url}")
                    Log.d("API_CLIENT", "Host header: ${modified.header("Host")}")

                    chain.proceed(modified)
                }
                .addInterceptor(logging)
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .hostnameVerifier { _, _ -> true }
                .build()

            retrofit = Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
        }
        return retrofit!!
    }

    // Helper method to test different URLs
    fun getClientWithCustomUrl(baseUrl: String): Retrofit {
        val logging = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        }

        val client = OkHttpClient.Builder()
            .addInterceptor { chain ->
                val original = chain.request()
                val modified = original.newBuilder()
                    .removeHeader("Host")
                    .addHeader("Host", "localhost")
                    .build()
                chain.proceed(modified)
            }
            .addInterceptor(logging)
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .hostnameVerifier { _, _ -> true }
            .build()

        return Retrofit.Builder()
            .baseUrl(baseUrl)
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }
    fun getBaseUrl(): String {
        return BASE_URL
    }
}