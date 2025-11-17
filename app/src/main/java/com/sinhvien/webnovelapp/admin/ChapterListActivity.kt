package com.sinhvien.webnovelapp.activities

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.view.MenuItem
import android.view.View
import android.widget.Button
import android.widget.ImageButton
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.adapters.ChapterSummaryAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.ChapterListResponse
import org.json.JSONObject
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ChapterListActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var chapterAdapter: ChapterSummaryAdapter
    private lateinit var tokenManager: TokenManager

    // UI Components
    private lateinit var tvStatus: TextView
    private lateinit var tvCount: TextView
    private lateinit var rvChapters: RecyclerView
    private lateinit var paginationControls: LinearLayout
    private lateinit var tvPageInfo: TextView
    private lateinit var btnPrevPage: Button
    private lateinit var btnNextPage: Button
    private lateinit var btnSortOrder: ImageButton
    private lateinit var progressBar: ProgressBar

    // Pagination
    private var novelId: Int = 0
    private var currentPage = 1
    private var totalPages = 1
    private val pageSize = 50
    private var isAscending = true // true = oldest first, false = newest first
    private var currentUserId: Int? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_chapter_list)

        // Initialize TokenManager
        tokenManager = TokenManager.getInstance(this)

        // Get user ID from token
        currentUserId = getUserIdFromToken()

        // Fallback: If token decode fails, try to get from SharedPreferences directly
        if (currentUserId == null) {
            val prefs = getSharedPreferences("WebNovelAppPrefs", Context.MODE_PRIVATE)
            currentUserId = prefs.getInt("USER_ID", -1).takeIf { it != -1 }
            Log.d("ChapterListActivity", "Using fallback userId: $currentUserId")
        }

        Log.d("ChapterListActivity", "Final currentUserId: $currentUserId")

        // Get novel ID from intent
        novelId = intent.getIntExtra("NOVEL_ID", 0)

        if (novelId == 0) {
            Toast.makeText(this, "Invalid novel ID", Toast.LENGTH_SHORT).show()
            finish()
            return
        }

        // Set up toolbar
        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = "All Chapters"
        } catch (e: Exception) {
            // Toolbar setup failed
        }

        // Initialize API service
        apiService = ApiClient.getClient().create(NovelApiService::class.java)

        // Initialize views
        initializeViews()

        // Set up RecyclerView
        setupRecyclerView()

        // Set up click listeners
        setupClickListeners()

        // Load chapters
        loadChapters()
    }

    private fun getUserIdFromToken(): Int? {
        val token = tokenManager.getToken()

        Log.d("ChapterListActivity", "Token exists: ${token != null}")

        if (token == null) {
            return null
        }

        return try {
            val parts = token.split(".")

            Log.d("ChapterListActivity", "Token parts count: ${parts.size}")

            if (parts.size != 3) {
                Log.e("ChapterListActivity", "Invalid token format")
                return null
            }

            val payload = String(android.util.Base64.decode(parts[1], android.util.Base64.URL_SAFE))
            val jsonObject = JSONObject(payload)

            Log.d("ChapterListActivity", "Token payload: $payload")

            val userId = when {
                jsonObject.has("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") ->
                    jsonObject.getString("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").toIntOrNull()
                jsonObject.has("nameid") -> jsonObject.getString("nameid").toIntOrNull()
                jsonObject.has("userId") -> jsonObject.getInt("userId")
                jsonObject.has("sub") -> jsonObject.getString("sub").toIntOrNull()
                jsonObject.has("id") -> jsonObject.getInt("id")
                jsonObject.has("user_id") -> jsonObject.getInt("user_id")
                jsonObject.has("UserID") -> jsonObject.getInt("UserID")
                jsonObject.has("uid") -> jsonObject.getInt("uid")
                else -> {
                    Log.e("ChapterListActivity", "No userId field found in token")
                    null
                }
            }

            Log.d("ChapterListActivity", "Extracted userId: $userId")
            userId

        } catch (e: Exception) {
            Log.e("ChapterListActivity", "Error decoding token", e)
            null
        }
    }

    private fun initializeViews() {
        tvStatus = findViewById(R.id.tvStatus)
        tvCount = findViewById(R.id.tvCount)
        rvChapters = findViewById(R.id.rvChapters)
        paginationControls = findViewById(R.id.paginationControls)
        tvPageInfo = findViewById(R.id.tvPageInfo)
        btnPrevPage = findViewById(R.id.btnPrevPage)
        btnNextPage = findViewById(R.id.btnNextPage)
        btnSortOrder = findViewById(R.id.btnSortOrder)
        progressBar = findViewById(R.id.progressBar)
    }

    private fun setupRecyclerView() {
        chapterAdapter = ChapterSummaryAdapter { chapter ->
            val intent = Intent(this, ChapterReaderActivity::class.java)
            intent.putExtra("NOVEL_ID", novelId)
            intent.putExtra("CHAPTER_NUMBER", chapter.ChapterNumber)
            startActivity(intent)
        }

        rvChapters.apply {
            layoutManager = LinearLayoutManager(this@ChapterListActivity)
            adapter = chapterAdapter
            setHasFixedSize(true)
        }
    }

    private fun setupClickListeners() {
        btnPrevPage.setOnClickListener {
            if (currentPage > 1) {
                currentPage--
                loadChapters()
                rvChapters.scrollToPosition(0)
            }
        }

        btnNextPage.setOnClickListener {
            if (currentPage < totalPages) {
                currentPage++
                loadChapters()
                rvChapters.scrollToPosition(0)
            }
        }

        btnSortOrder.setOnClickListener {
            isAscending = !isAscending
            currentPage = 1 // Reset to first page when sorting
            loadChapters()

            // Change icon rotation to indicate sort direction
            btnSortOrder.rotation = if (isAscending) 0f else 180f

            Toast.makeText(
                this,
                if (isAscending) "Oldest first" else "Newest first",
                Toast.LENGTH_SHORT
            ).show()
        }
    }

    private fun loadChapters() {
        showLoading(true)
        tvCount.text = "Loading chapters..."

        val call = if (currentUserId != null) {
            apiService.getNovelChapters(
                novelId = novelId,
                page = currentPage,
                pageSize = pageSize,
                userId = currentUserId!! // Pass userId to get unlock status
            )
        } else {
            apiService.getNovelChapters(
                novelId = novelId,
                page = currentPage,
                pageSize = pageSize
            )
        }

        call.enqueue(object : Callback<ChapterListResponse> {
            override fun onResponse(call: Call<ChapterListResponse>, response: Response<ChapterListResponse>) {
                showLoading(false)

                if (response.isSuccessful && response.body() != null) {
                    val chapterResponse = response.body()!!
                    if (chapterResponse.Success) {
                        var chapters = chapterResponse.Data

                        // Sort chapters based on sort order
                        if (!isAscending) {
                            chapters = chapters.reversed()
                        }

                        chapterAdapter.submitList(chapters)
                        totalPages = chapterResponse.TotalPages

                        tvStatus.text = "All Chapters"
                        tvCount.text = "${chapterResponse.TotalCount} chapters total"

                        // Update pagination
                        tvPageInfo.text = "Page $currentPage of $totalPages"
                        btnPrevPage.isEnabled = currentPage > 1
                        btnNextPage.isEnabled = currentPage < totalPages
                        paginationControls.visibility = if (totalPages > 1) View.VISIBLE else View.GONE

                        Log.d("ChapterList", "Loaded ${chapters.size} chapters with unlock status")
                    } else {
                        showError("Failed to load chapters")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<ChapterListResponse>, t: Throwable) {
                showLoading(false)
                showError("Network Error: ${t.message}")
                Toast.makeText(this@ChapterListActivity, "Failed to load chapters", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun showLoading(show: Boolean) {
        progressBar.visibility = if (show) View.VISIBLE else View.GONE
        rvChapters.visibility = if (show) View.GONE else View.VISIBLE
    }

    private fun showError(errorMessage: String) {
        tvStatus.text = "Error"
        tvCount.text = errorMessage
        Toast.makeText(this, errorMessage, Toast.LENGTH_LONG).show()
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            android.R.id.home -> {
                onBackPressed()
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }
}