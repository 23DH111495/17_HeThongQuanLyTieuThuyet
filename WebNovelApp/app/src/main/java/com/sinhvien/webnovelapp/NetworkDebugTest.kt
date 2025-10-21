package com.sinhvien.webnovelapp

import android.util.Log
import okhttp3.*
import java.io.IOException
import java.util.concurrent.TimeUnit

class NetworkDebugTest {

    fun runAllTests() {
        Log.d("DEBUG_TEST", "Starting network debugging tests...")

        // Test 1: Basic connectivity
        testBasicConnectivity()

        // Test 2: Raw HTTP without host header
        testRawHttp()

        // Test 3: With host header
        testWithHostHeader()

        // Test 4: Different ports
        testDifferentPort()
    }

    private fun testBasicConnectivity() {
        Log.d("DEBUG_TEST", "=== Test 1: Basic Connectivity ===")

        val client = OkHttpClient.Builder()
            .connectTimeout(10, TimeUnit.SECONDS)
            .addInterceptor { chain ->
                val request = chain.request()
                Log.d("DEBUG_TEST", "Raw request URL: ${request.url}")
                Log.d("DEBUG_TEST", "Raw request headers: ${request.headers}")
                val response = chain.proceed(request)
                Log.d("DEBUG_TEST", "Response code: ${response.code}")
                Log.d("DEBUG_TEST", "Response headers: ${response.headers}")
                response
            }
            .build()

        val request = Request.Builder()
            .url("http://192.168.101.4:51801/")
            .build()

        client.newCall(request).enqueue(object : Callback {
            override fun onFailure(call: Call, e: IOException) {
                Log.e("DEBUG_TEST", "Basic connectivity failed: ${e.message}")
            }

            override fun onResponse(call: Call, response: Response) {
                Log.d("DEBUG_TEST", "Basic connectivity response: ${response.code}")
                response.close()
            }
        })
    }

    private fun testRawHttp() {
        Log.d("DEBUG_TEST", "=== Test 2: Raw HTTP Test ===")

        val client = OkHttpClient.Builder()
            .connectTimeout(10, TimeUnit.SECONDS)
            .build()

        val request = Request.Builder()
            .url("http://192.168.101.4:51801/api/genres?search=&activeOnly=false")
            .build()

        client.newCall(request).enqueue(object : Callback {
            override fun onFailure(call: Call, e: IOException) {
                Log.e("DEBUG_TEST", "Raw HTTP failed: ${e.message}")
            }

            override fun onResponse(call: Call, response: Response) {
                Log.d("DEBUG_TEST", "Raw HTTP response: ${response.code}")
                Log.d("DEBUG_TEST", "Raw HTTP body: ${response.body?.string()}")
            }
        })
    }

    private fun testWithHostHeader() {
        Log.d("DEBUG_TEST", "=== Test 3: With Host Header ===")

        val client = OkHttpClient.Builder()
            .connectTimeout(10, TimeUnit.SECONDS)
            .addInterceptor { chain ->
                val original = chain.request()
                val modified = original.newBuilder()
                    .removeHeader("Host")
                    .addHeader("Host", "localhost")
                    .build()

                Log.d("DEBUG_TEST", "Modified request headers: ${modified.headers}")
                chain.proceed(modified)
            }
            .build()

        val request = Request.Builder()
            .url("http://192.168.101.4:51801/api/genres?search=&activeOnly=false")
            .build()

        client.newCall(request).enqueue(object : Callback {
            override fun onFailure(call: Call, e: IOException) {
                Log.e("DEBUG_TEST", "Host header test failed: ${e.message}")
            }

            override fun onResponse(call: Call, response: Response) {
                Log.d("DEBUG_TEST", "Host header test response: ${response.code}")
                if (response.isSuccessful) {
                    Log.d("DEBUG_TEST", "SUCCESS! Host header worked!")
                    Log.d("DEBUG_TEST", "Response body: ${response.body?.string()}")
                } else {
                    Log.d("DEBUG_TEST", "Host header test failed with: ${response.body?.string()}")
                }
            }
        })
    }

    private fun testDifferentPort() {
        Log.d("DEBUG_TEST", "=== Test 4: Different Port (8080) ===")

        // First test if anything is running on 8080
        val client = OkHttpClient.Builder()
            .connectTimeout(5, TimeUnit.SECONDS)
            .build()

        val request = Request.Builder()
            .url("http://192.168.101.4:8080/")
            .build()

        client.newCall(request).enqueue(object : Callback {
            override fun onFailure(call: Call, e: IOException) {
                Log.d("DEBUG_TEST", "Port 8080 test: Nothing running on 8080 (expected)")
            }

            override fun onResponse(call: Call, response: Response) {
                Log.d("DEBUG_TEST", "Port 8080 test response: ${response.code}")
                response.close()
            }
        })
    }
}