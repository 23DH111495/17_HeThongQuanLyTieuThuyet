package com.sinhvien.webnovelapp.fragments

import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast
import androidx.core.content.ContextCompat
import androidx.fragment.app.Fragment
import com.bumptech.glide.Glide
import com.google.android.material.button.MaterialButton
import com.google.android.material.imageview.ShapeableImageView
import com.sinhvien.webnovelapp.CongThuong.Coin.CoinPackageActivity
import com.sinhvien.webnovelapp.CongThuong.Login.LoginActivity
import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import com.sinhvien.webnovelapp.ThieuKhang.ReadingHistoryActivity
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.api.*
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.sinhvien.webnovelapp.api.UserResponse
import com.sinhvien.webnovelapp.api.UserApi
class ProfileFragment : Fragment() {
    private lateinit var itemReadingHistory: LinearLayout

    private lateinit var etPromoCode: EditText
    private lateinit var btnApply: Button
    private lateinit var btnClear: Button
    private lateinit var tvStatusMessage: TextView
    private lateinit var tvAppliedCode: TextView
    private lateinit var llAppliedCode: LinearLayout
    private lateinit var btnLogout: MaterialButton

    private lateinit var imgAvatar: ShapeableImageView
    private lateinit var tvUserName: TextView
    private lateinit var tvEmail: TextView
    private lateinit var tvCoinAmount: TextView

    companion object {
        private const val TAG = "ProfileFragment"
    }

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        return inflater.inflate(R.layout.fragment_profile2, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        try {
            LoginApiClient.init(requireContext())
            Log.d(TAG, "✅ LoginApiClient initialized successfully")
        } catch (e: Exception) {
            Log.e(TAG, "❌ Error initializing LoginApiClient: ${e.message}", e)
        }

        etPromoCode = view.findViewById(R.id.et_promo_code)
        btnApply = view.findViewById(R.id.btn_apply)
        btnClear = view.findViewById(R.id.btn_clear)
        tvStatusMessage = view.findViewById(R.id.tv_status_message)
        tvAppliedCode = view.findViewById(R.id.tv_applied_code)
        llAppliedCode = view.findViewById(R.id.ll_applied_code)

        btnLogout = view.findViewById(R.id.btnLogout)
        imgAvatar = view.findViewById(R.id.imgAvatar)
        tvUserName = view.findViewById(R.id.tvUserName)
        tvEmail = view.findViewById(R.id.tvEmail)
        tvCoinAmount = view.findViewById(R.id.tvCoinAmount)

        //Khang
        itemReadingHistory = view.findViewById(R.id.itemReadingHistory)
        itemReadingHistory.setOnClickListener {
            val intent = Intent(requireContext(), ReadingHistoryActivity::class.java)
            startActivity(intent)
        }

        Log.d(TAG, "✅ All views initialized")

        loadUserProfile()

        btnLogout.setOnClickListener {
            performLogout()
        }

        val promoApi = try {
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
                Toast.makeText(requireContext(), "Vui lòng nhập mã khuyến mãi", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            val request = ApplyPromoRequest(code = code)
            promoApi.applyPromoCode(request).enqueue(object : Callback<ApplyPromoResponse> {
                override fun onResponse(call: Call<ApplyPromoResponse>, response: Response<ApplyPromoResponse>) {
                    if (!isAdded) return

                    if (response.isSuccessful && response.body() != null) {
                        val promoResponse = response.body()!!
                        tvStatusMessage.apply {
                            visibility = View.VISIBLE
                            text = "✅ ${promoResponse.message}"
                            setTextColor(ContextCompat.getColor(requireContext(), R.color.green))
                        }
                        tvAppliedCode.text = promoResponse.code
                        llAppliedCode.visibility = View.VISIBLE
                    } else {
                        tvStatusMessage.apply {
                            visibility = View.VISIBLE
                            text = "❌ Mã không hợp lệ, hết hạn hoặc đã dùng."
                            setTextColor(ContextCompat.getColor(requireContext(), R.color.red))
                        }
                        llAppliedCode.visibility = View.GONE
                    }
                }

                override fun onFailure(call: Call<ApplyPromoResponse>, t: Throwable) {
                    if (!isAdded) return
                    tvStatusMessage.apply {
                        visibility = View.VISIBLE
                        text = "⚠️ Lỗi kết nối: ${t.message}"
                        setTextColor(ContextCompat.getColor(requireContext(), R.color.yellow))
                    }
                }
            })
        }

        btnClear.setOnClickListener {
            etPromoCode.text.clear()
            tvStatusMessage.visibility = View.GONE
            llAppliedCode.visibility = View.GONE
        }

        val buyCoinsLayout = view.findViewById<View>(R.id.btnBuyCoin)
        buyCoinsLayout.setOnClickListener {
            val intent = Intent(requireContext(), CoinPackageActivity::class.java)
            startActivity(intent)
        }
    }

    private fun loadUserProfile() {
        Log.d(TAG, "👤 Loading user profile...")

        val tokenManager = TokenManager.getInstance(requireContext())

        tvUserName.text = tokenManager.username ?: "Người dùng"
        tvEmail.text = tokenManager.email ?: ""

        val userApi = LoginApiClient.getClient().create(UserApi::class.java)

        userApi.getUserProfile().enqueue(object : Callback<UserResponse> {
            override fun onResponse(call: Call<UserResponse>, response: Response<UserResponse>) {
                if (!isAdded) return

                if (response.isSuccessful && response.body() != null) {
                    val res = response.body()!!
                    if (res.data != null) {
                        val user = res.data
                        Log.d(TAG, "✅ User profile loaded from API: ${user.username}")

                        tvUserName.text = user.fullName ?: user.username ?: tokenManager.username

                        tvCoinAmount.text = user.balance?.toString() ?: "0"

                        if (!user.avatarUrl.isNullOrEmpty()) {
                            Glide.with(this@ProfileFragment)
                                .load(user.avatarUrl)
                                .placeholder(R.drawable.ic_default_avatar)
                                .error(R.drawable.ic_default_avatar)
                                .into(imgAvatar)
                        }
                    }
                } else {
                    Log.e(TAG, "⚠️ Failed to load profile. Code: ${response.code()}")
                }
            }

            override fun onFailure(call: Call<UserResponse>, t: Throwable) {
                if (!isAdded) return
                Log.e(TAG, "❌ Error loading profile: ${t.message}")
            }
        })
    }

    private fun performLogout() {
        try {
            TokenManager.getInstance(requireContext()).clearToken()
            Log.d(TAG, "✅ Token cleared successfully")
        } catch (e: Exception) {
            Log.e(TAG, "❌ Error clearing token: ${e.message}")
        }

//        Toast.makeText(requireContext(), "Đăng xuất thành công!", Toast.LENGTH_SHORT).show()

        val intent = Intent(requireContext(), LoginActivity::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        startActivity(intent)
        requireActivity().finish()
    }
}