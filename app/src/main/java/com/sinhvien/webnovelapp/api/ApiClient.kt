package com.sinhvien.webnovelapp.api

import android.content.Context
import android.util.Log
import okhttp3.Cache
import okhttp3.CacheControl
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.io.File
import java.util.concurrent.TimeUnit
import com.google.gson.FieldNamingPolicy
import com.google.gson.GsonBuilder
import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import com.sinhvien.webnovelapp.api.ReadingHistoryApiService

object ApiClient {
    private const val BASE_URL = "http://10.0.2.2:7200/"
    private var retrofit: Retrofit? = null
    private var tokenManager: TokenManager? = null
    private var appContext: Context? = null

    // Keep original init for backward compatibility
    fun init(tokenManager: TokenManager) {
        this.tokenManager = tokenManager
    }

    // Optional: Call this to enable caching (from Application class)
    fun initWithContext(tokenManager: TokenManager, context: Context) {
        this.tokenManager = tokenManager
        this.appContext = context.applicationContext
    }

    fun getClient(): Retrofit {
        if (retrofit == null) {
            val logging = HttpLoggingInterceptor { message ->
                Log.d("API_CLIENT", message)
            }.apply {
                level = HttpLoggingInterceptor.Level.BODY
            }

            val clientBuilder = OkHttpClient.Builder()
                .addInterceptor(logging)
                .addInterceptor { chain ->
                    val request = chain.request()
                    val token = tokenManager?.getToken()

                    val newRequest = if (token != null) {
                        request.newBuilder()
                            .addHeader("Authorization", "Bearer $token")
                            .build()
                    } else {
                        request
                    }

                    chain.proceed(newRequest)
                }
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)

            // Add caching if context is available
            appContext?.let { ctx ->
                val cacheSize = 10L * 1024 * 1024 // 10 MB
                val cache = Cache(File(ctx.cacheDir, "http_cache"), cacheSize)

                clientBuilder
                    .cache(cache)
                    .addNetworkInterceptor { chain ->
                        val request = chain.request()
                        val response = chain.proceed(request)

                        if (request.method == "GET") {
                            val cacheControl = CacheControl.Builder()
                                .maxAge(5, TimeUnit.MINUTES)
                                .build()

                            response.newBuilder()
                                .header("Cache-Control", cacheControl.toString())
                                .removeHeader("Pragma")
                                .build()
                        } else {
                            response
                        }
                    }
                    .connectionPool(okhttp3.ConnectionPool(5, 5, TimeUnit.MINUTES))
                    .retryOnConnectionFailure(true)
            }

            val client = clientBuilder.build()

            val gson = GsonBuilder()
                //.setFieldNamingPolicy(FieldNamingPolicy.LOWER_CASE_WITH_UNDERSCORES)
                .create()

            retrofit = Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create(gson))
                .build()
        }
        return retrofit!!
    }

    // =======================================================
    fun getReadingHistoryService(): ReadingHistoryApiService {
        return getClient().create(ReadingHistoryApiService::class.java)
    }
    // =======================================================

    fun getBaseUrl(): String = BASE_URL

    // Optional: Clear cache when needed
    fun clearCache() {
        appContext?.let {
            try {
                val cacheDir = File(it.cacheDir, "http_cache")
                if (cacheDir.exists()) {
                    cacheDir.deleteRecursively()
                }
            } catch (e: Exception) {
                Log.e("API_CLIENT", "Error clearing cache", e)
            }
        }
    }
}