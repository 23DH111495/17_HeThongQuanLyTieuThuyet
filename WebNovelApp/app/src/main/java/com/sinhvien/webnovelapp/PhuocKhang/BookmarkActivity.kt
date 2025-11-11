package com.sinhvien.webnovelapp.PhuocKhang

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.view.View
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import com.sinhvien.webnovelapp.CongThuong.Login.LoginActivity
import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import com.sinhvien.webnovelapp.activities.NovelDetailActivity
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.BookmarkApiService
import com.sinhvien.webnovelapp.databinding.ActivityBookmarkBinding
import com.sinhvien.webnovelapp.models.Bookmark
import com.sinhvien.webnovelapp.models.BookmarkResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class BookmarkActivity : AppCompatActivity() {

    private lateinit var binding: ActivityBookmarkBinding
    private lateinit var bookmarkAdapter: BookmarkAdapter
    private lateinit var bookmarkApiService: BookmarkApiService
    private lateinit var tokenManager: TokenManager

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityBookmarkBinding.inflate(layoutInflater)
        setContentView(binding.root)

        tokenManager = TokenManager.getInstance(this)
        val token = tokenManager.getToken()

        if (token.isNullOrEmpty()) {
            Toast.makeText(this, "Bạn chưa đăng nhập", Toast.LENGTH_SHORT).show()
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
            return
        }

        // *** ĐÃ SỬA LỖI: ***
        // Gọi client ĐÃ XÁC THỰC (không cần truyền context)
        // ApiClient đã được khởi tạo trong MyApplication
        bookmarkApiService = ApiClient.getAuthenticatedClient().create(BookmarkApiService::class.java)

        setupToolbar()
        setupRecyclerView()
        loadBookmarks()
    }

    private fun setupToolbar() {
        setSupportActionBar(binding.toolbar)
        supportActionBar?.setDisplayHomeAsUpEnabled(true)
        supportActionBar?.title = "My Bookmarks"
        binding.toolbar.setNavigationOnClickListener {
            onBackPressedDispatcher.onBackPressed()
        }
    }

    private fun setupRecyclerView() {
        bookmarkAdapter = BookmarkAdapter(this) { novelId ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novelId)
            startActivity(intent)
        }

        binding.recyclerViewBookmarks.apply {
            layoutManager = LinearLayoutManager(this@BookmarkActivity)
            adapter = bookmarkAdapter
        }
    }

    private fun loadBookmarks() {
        showLoading(true)

        // AuthInterceptor sẽ tự động thêm Token,
        // nên chúng ta không cần gửi readerId
        bookmarkApiService.getMyBookmarks()
            .enqueue(object : Callback<BookmarkResponse> {
                override fun onResponse(
                    call: Call<BookmarkResponse>,
                    response: Response<BookmarkResponse>
                ) {
                    showLoading(false)

                    if (response.isSuccessful) {
                        val bookmarkResponse = response.body()
                        if (bookmarkResponse != null) {
                            val bookmarks = bookmarkResponse.bookmarks

                            if (bookmarks.isNotEmpty()) {
                                bookmarkAdapter.submitList(bookmarks)
                                binding.tvEmptyList.visibility = View.GONE

                                // Hiển thị tổng số bookmark
                                supportActionBar?.subtitle = "${bookmarkResponse.totalBookmarks} novels"
                            } else {
                                showEmptyState()
                            }
                        } else {
                            showEmptyState()
                        }
                    } else {
                        Toast.makeText(
                            this@BookmarkActivity,
                            "Lỗi: ${response.code()}",
                            Toast.LENGTH_SHORT
                        ).show()
                        Log.e("BookmarkActivity", "Error: ${response.errorBody()?.string()}")
                        showEmptyState()
                    }
                }

                override fun onFailure(call: Call<BookmarkResponse>, t: Throwable) {
                    showLoading(false)
                    Toast.makeText(
                        this@BookmarkActivity,
                        "Lỗi kết nối: ${t.message}",
                        Toast.LENGTH_SHORT
                    ).show()
                    Log.e("BookmarkActivity", "API failed: ${t.message}", t)
                    showEmptyState()
                }
            })
    }

    private fun showLoading(show: Boolean) {
        binding.progressBar.visibility = if (show) View.VISIBLE else View.GONE
        binding.recyclerViewBookmarks.visibility = if (show) View.GONE else View.VISIBLE
    }

    private fun showEmptyState() {
        binding.tvEmptyList.visibility = View.VISIBLE
        binding.recyclerViewBookmarks.visibility = View.GONE
    }

    override fun onResume() {
        super.onResume()
        // Refresh lại danh sách khi quay về
        loadBookmarks()
    }
}