package com.sinhvien.webnovelapp.CongThuong.Coin

import android.os.Bundle
import android.util.Log
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.sinhvien.webnovelapp.R
// ⭐️ QUAN TRỌNG: Đảm bảo import đúng LoginApiClient
import com.sinhvien.webnovelapp.api.LoginApiClient
import com.sinhvien.webnovelapp.api.ApplyPromoRequest
import com.sinhvien.webnovelapp.api.ApplyPromoResponse
import com.sinhvien.webnovelapp.api.PromoCodeApi
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class PromoCodeActivity : AppCompatActivity() {

    private lateinit var etPromoCode: EditText
    private lateinit var btnApply: Button
    private lateinit var btnClear: Button
    private lateinit var tvStatusMessage: TextView
    private lateinit var tvAppliedCode: TextView
    private lateinit var llAppliedCode: LinearLayout

    companion object {
        private const val TAG = "PromoCodeActivity"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_promo)

        Log.d(TAG, "⭐ onCreate started")


        try {
            LoginApiClient.init(this)
            Log.d(TAG, "✅ LoginApiClient initialized successfully")
        } catch (e: Exception) {
            Log.e(TAG, "❌ Error initializing LoginApiClient: ${e.message}", e)
        }

        etPromoCode = findViewById(R.id.et_promo_code)
        btnApply = findViewById(R.id.btn_apply)
        btnClear = findViewById(R.id.btn_clear)
        tvStatusMessage = findViewById(R.id.tv_status_message)
        tvAppliedCode = findViewById(R.id.tv_applied_code)
        llAppliedCode = findViewById(R.id.ll_applied_code)

        Log.d(TAG, "✅ All views initialized")


        val api = try {
            LoginApiClient.getClient().create(PromoCodeApi::class.java)
                .also { Log.d(TAG, "✅ PromoCodeApi service created successfully") }
        } catch (e: Exception) {
            Log.e(TAG, "❌ Error creating PromoCodeApi service: ${e.message}", e)
            return
        }

        btnApply.setOnClickListener {
            val code = etPromoCode.text.toString().trim().uppercase()
            Log.d(TAG, "🔍 Button Apply clicked with code: $code")

            if (code.isEmpty()) {
                Log.w(TAG, "⚠️ Promo code is empty")
                Toast.makeText(this, "Vui lòng nhập mã khuyến mãi", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            val request = ApplyPromoRequest(code = code)
            Log.d(TAG, "📤 Creating request with code: ${request.code}")


            val call = api.applyPromoCode(request)
            Log.d(TAG, "📡 Sending API request to apply promo code")

            // 3. Xử lý kết quả
            call.enqueue(object : Callback<ApplyPromoResponse> {
                override fun onResponse(call: Call<ApplyPromoResponse>, response: Response<ApplyPromoResponse>) {
                    Log.d(TAG, "📥 Response received")
                    Log.d(TAG, "📊 Response code: ${response.code()}")
                    Log.d(TAG, "📊 Is successful: ${response.isSuccessful}")

                    if (response.isSuccessful && response.body() != null) {

                        val promoResponse = response.body()!!
                        Log.d(TAG, "✅ Success! Message: ${promoResponse.message}")
                        Log.d(TAG, "✅ Applied code: ${promoResponse.code}")

                        tvStatusMessage.apply {
                            visibility = View.VISIBLE
                            text = "✅ ${promoResponse.message}"
                            setTextColor(resources.getColor(R.color.green))
                        }
                        tvAppliedCode.text = promoResponse.code
                        llAppliedCode.visibility = View.VISIBLE
                    } else {
                        // LỖI TỪ SERVER
                        Log.e(TAG, "❌ Error response from server")
                        Log.e(TAG, "❌ Response body is null: ${response.body() == null}")

                        // Thử lấy error body
                        try {
                            val errorBody = response.errorBody()?.string()
                            Log.e(TAG, "❌ Error body: $errorBody")
                        } catch (e: Exception) {
                            Log.e(TAG, "❌ Could not read error body: ${e.message}")
                        }

                        tvStatusMessage.apply {
                            visibility = View.VISIBLE
                            text = "❌ Mã không hợp lệ, đã hết hạn hoặc đã được sử dụng."
                            setTextColor(resources.getColor(R.color.red))
                        }
                        llAppliedCode.visibility = View.GONE
                    }
                }

                override fun onFailure(call: Call<ApplyPromoResponse>, t: Throwable) {
                    // Lỗi mạng
                    Log.e(TAG, "❌ Network error occurred", t)
                    Log.e(TAG, "❌ Error type: ${t::class.simpleName}")
                    Log.e(TAG, "❌ Error message: ${t.message}")
                    Log.e(TAG, "❌ Full stacktrace:", t)

                    tvStatusMessage.apply {
                        visibility = View.VISIBLE
                        text = "⚠️ Không thể kết nối tới máy chủ: ${t.message}"
                        setTextColor(resources.getColor(R.color.yellow))
                    }
                }
            })
        }

        // Nút "Xóa"
        btnClear.setOnClickListener {
            Log.d(TAG, "🔄 Clear button clicked")
            etPromoCode.text.clear()
            tvStatusMessage.visibility = View.GONE
            llAppliedCode.visibility = View.GONE
        }
    }
}