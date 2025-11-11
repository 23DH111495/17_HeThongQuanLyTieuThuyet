//package com.sinhvien.webnovelapp.api
//
//import android.util.Log
//import okhttp3.OkHttpClient
//import okhttp3.logging.HttpLoggingInterceptor
//import retrofit2.Retrofit
//import retrofit2.converter.gson.GsonConverterFactory
//import java.util.concurrent.TimeUnit
//import com.google.gson.FieldNamingPolicy
//import com.google.gson.GsonBuilder
//
//object ApiClient {
//    private const val BASE_URL = "http://10.0.2.2:7200/"
//    private var retrofit: Retrofit? = null
//
//    fun getClient(): Retrofit {
//        if (retrofit == null) {
//           val logging = HttpLoggingInterceptor { message ->
//               Log.d("API_CLIENT", message)
//          }.apply {
//              level = HttpLoggingInterceptor.Level.BODY
//          }
//
//           val client = OkHttpClient.Builder()
//               .addInterceptor(logging)
//               .connectTimeout(30, TimeUnit.SECONDS)
//               .readTimeout(30, TimeUnit.SECONDS)
//               .writeTimeout(30, TimeUnit.SECONDS)
//               .build()
//
//           // Configure Gson with lowercase field naming
//           val gson = GsonBuilder()
//               .setFieldNamingPolicy(FieldNamingPolicy.LOWER_CASE_WITH_UNDERSCORES)
//               .create()
//
//            retrofit = Retrofit.Builder()
//               .baseUrl(BASE_URL)
//               .client(client)
//               .addConverterFactory(GsonConverterFactory.create(gson))
//               .build()
//       }
//       return retrofit!!
//   }
//
//    fun getBaseUrl(): String = BASE_URL
//}
package com.sinhvien.webnovelapp.api

import android.content.Context
import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit

object ApiClient {

    private const val BASE_URL = "http://10.0.2.2:7200/"

    // Cấu hình Logging (Dùng chung)
    private val loggingInterceptor = HttpLoggingInterceptor().apply {
        level = HttpLoggingInterceptor.Level.BODY
    }

    // --- Client CÔNG KHAI (Không Token) ---
    private val publicRetrofit: Retrofit = Retrofit.Builder()
        .baseUrl(BASE_URL)
        .client(
            OkHttpClient.Builder()
                .addInterceptor(loggingInterceptor)
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .build()
        )
        .addConverterFactory(GsonConverterFactory.create())
        .build()

    // --- Client RIÊNG TƯ (Có Token) - Sẽ được khởi tạo ---
    private var authenticatedRetrofit: Retrofit? = null

    /**
     * [QUAN TRỌNG] Phải gọi hàm này từ MyApplication.onCreate()
     * để khởi tạo Client có Token
     */
    fun init(context: Context) {
        if (authenticatedRetrofit == null) {
            val tokenManager = TokenManager.getInstance(context.applicationContext)
            val authInterceptor = AuthInterceptor(tokenManager)

            val privateHttpClient = OkHttpClient.Builder()
                .addInterceptor(authInterceptor)   // Thêm AuthInterceptor
                .addInterceptor(loggingInterceptor) // Thêm Logging
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .build()

            authenticatedRetrofit = Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(privateHttpClient) // Sử dụng client riêng tư
                .addConverterFactory(GsonConverterFactory.create())
                .build()
        }
    }

    /**
     * Lấy Client CÔNG KHAI (Không cần Token).
     * Dùng cho Login, Register, xem danh sách truyện...
     */
    fun getClient(): Retrofit {
        return publicRetrofit
    }

    /**
     * Lấy Client RIÊNG TƯ (Đã xác thực).
     * Dùng cho Bookmark, Rating, Comment...
     * Sẽ crash nếu chưa gọi init() trong MyApplication.
     */
    fun getAuthenticatedClient(): Retrofit {
        return authenticatedRetrofit
            ?: throw IllegalStateException("ApiClient.init(context) must be called in MyApplication.onCreate()")
    }

    fun getBaseUrl(): String {
        return BASE_URL
    }
}

