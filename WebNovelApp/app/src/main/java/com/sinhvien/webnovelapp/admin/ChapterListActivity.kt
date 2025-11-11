package com.sinhvien.webnovelapp.activities

import android.content.Intent
import android.os.Bundle
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
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.adapters.ChapterSummaryAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.ChapterListResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ChapterListActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var chapterAdapter: ChapterSummaryAdapter

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

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_chapter_list)

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
            supportActionBar?.title = "Chapters"
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

            // Optional: Change icon rotation or color to indicate sort direction
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

        val call = apiService.getNovelChapters(
            novelId = novelId,
            page = currentPage,
            pageSize = pageSize
        )

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

                        tvStatus.text = "Chapter List"
                        tvCount.text = "${chapterResponse.TotalCount} chapters"

                        // Update pagination
                        tvPageInfo.text = "Page $currentPage of $totalPages"
                        btnPrevPage.isEnabled = currentPage > 1
                        btnNextPage.isEnabled = currentPage < totalPages
                        paginationControls.visibility = if (totalPages > 1) View.VISIBLE else View.GONE
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