package com.sinhvien.webnovelapp.activities

import android.content.Intent
import android.os.Bundle
import android.view.MenuItem
import android.view.View
import android.widget.Button
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.bumptech.glide.load.engine.DiskCacheStrategy
import com.google.android.material.chip.Chip
import com.google.android.material.chip.ChipGroup
import com.google.android.material.floatingactionbutton.FloatingActionButton
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.adapters.ChapterSummaryAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.NovelDetailResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class NovelDetailActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var chapterAdapter: ChapterSummaryAdapter

    // UI Components
    private lateinit var ivNovelCover: ImageView
    private lateinit var ivNovelCoverLarge: ImageView
    private lateinit var tvNovelTitle: TextView
    private lateinit var tvNovelAuthor: TextView
    private lateinit var tvNovelStatus: TextView
    private lateinit var tvNovelRating: TextView
    private lateinit var tvNovelStats: TextView
    private lateinit var tvNovelViews: TextView
    private lateinit var tvNovelSynopsis: TextView
    private lateinit var chipGroupGenres: ChipGroup
    private lateinit var rvRecentChapters: RecyclerView
    private lateinit var btnViewAllChapters: Button
    private lateinit var fabBookmark: FloatingActionButton

    private var novelId: Int = 0
    private var isBookmarked = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_novel_detail)

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
            supportActionBar?.title = ""
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

        // Load novel details
        loadNovelDetail()
    }

    private fun initializeViews() {
        ivNovelCover = findViewById(R.id.ivNovelCover)
        ivNovelCoverLarge = findViewById(R.id.ivNovelCoverLarge)
        tvNovelTitle = findViewById(R.id.tvNovelTitle)
        tvNovelAuthor = findViewById(R.id.tvNovelAuthor)
        tvNovelStatus = findViewById(R.id.tvNovelStatus)
        tvNovelRating = findViewById(R.id.tvNovelRating)
        tvNovelStats = findViewById(R.id.tvNovelStats)
        tvNovelViews = findViewById(R.id.tvNovelViews)
        tvNovelSynopsis = findViewById(R.id.tvNovelSynopsis)
        chipGroupGenres = findViewById(R.id.chipGroupGenres)
        rvRecentChapters = findViewById(R.id.rvRecentChapters)
        btnViewAllChapters = findViewById(R.id.btnViewAllChapters)
        fabBookmark = findViewById(R.id.fabBookmark)
    }

    private fun setupRecyclerView() {
        chapterAdapter = ChapterSummaryAdapter { chapter ->
            val intent = Intent(this, ChapterReaderActivity::class.java)
            intent.putExtra("NOVEL_ID", novelId)
            intent.putExtra("CHAPTER_NUMBER", chapter.ChapterNumber)
            startActivity(intent)
        }

        rvRecentChapters.apply {
            layoutManager = LinearLayoutManager(this@NovelDetailActivity)
            adapter = chapterAdapter
            isNestedScrollingEnabled = false
        }
    }

    private fun setupClickListeners() {
        btnViewAllChapters.setOnClickListener {
            // Navigate to full chapter list
            val intent = Intent(this, ChapterListActivity::class.java)
            intent.putExtra("NOVEL_ID", novelId)
            startActivity(intent)
        }

        fabBookmark.setOnClickListener {
            toggleBookmark()
        }
    }

    private fun loadNovelDetail() {
        val call = apiService.getNovelDetail(novelId)

        call.enqueue(object : Callback<NovelDetailResponse> {
            override fun onResponse(call: Call<NovelDetailResponse>, response: Response<NovelDetailResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success) {
                        displayNovelDetail(novelResponse.Data)
                    } else {
                        showError("Failed to load novel details")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<NovelDetailResponse>, t: Throwable) {
                showError("Network Error: ${t.message}")
            }
        })
    }

    private fun displayNovelDetail(novel: com.sinhvien.webnovelapp.models.NovelDetail) {
        // Set title
        tvNovelTitle.text = novel.Title
        supportActionBar?.title = novel.Title

        // Set author
        val authorText = if (novel.Author?.IsVerified == true) {
            "by ${novel.Author.PenName} ✓"
        } else {
            "by ${novel.Author?.PenName ?: "Unknown Author"}"
        }
        tvNovelAuthor.text = authorText

        // Set status
        tvNovelStatus.text = novel.Status
        when (novel.Status.lowercase()) {
            "ongoing" -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#4CAF50"))
                tvNovelStatus.setBackgroundResource(R.drawable.bg_status_active)
            }
            "completed" -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#2196F3"))
            }
            else -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#FF9800"))
            }
        }

        // Set rating
        tvNovelRating.text = "★ ${String.format("%.2f", novel.AverageRating)} (${novel.TotalRatings} ratings)"

        // Set stats
        tvNovelStats.text = "${novel.TotalChapters} chapters • ${formatNumber(novel.WordCount)} words"

        // Set views and bookmarks
        tvNovelViews.text = "${formatNumber(novel.ViewCount)} views • ${formatNumber(novel.BookmarkCount)} bookmarks"

        // Set synopsis
        tvNovelSynopsis.text = novel.Synopsis ?: "No synopsis available."

        // Set genres
        chipGroupGenres.removeAllViews()
        for (genre in novel.Genres) {
            val textView = TextView(this)
            textView.text = genre.Name
            textView.setPadding(16, 8, 16, 8)
            textView.setTextColor(android.graphics.Color.parseColor("#ffffff"))

            // Try to parse and set background color if available
            try {
                val color = android.graphics.Color.parseColor(genre.ColorCode ?: "#77dd77")
                textView.setBackgroundColor(color)
            } catch (e: Exception) {
                textView.setBackgroundColor(android.graphics.Color.parseColor("#77dd77"))
            }

            // Add some margin between genres
            val params = LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.WRAP_CONTENT,
                LinearLayout.LayoutParams.WRAP_CONTENT
            )
            params.setMargins(8, 8, 8, 8)
            textView.layoutParams = params

            chipGroupGenres.addView(textView)
        }

        // Set recent chapters
        chapterAdapter.submitList(novel.RecentChapters)

        // Load cover images
        val coverImageUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"

        Glide.with(this)
            .load(coverImageUrl)
            .placeholder(R.drawable.placeholder_novel_cover)
            .error(R.drawable.placeholder_novel_cover)
            .diskCacheStrategy(DiskCacheStrategy.ALL)
            .into(ivNovelCover)

        Glide.with(this)
            .load(coverImageUrl)
            .placeholder(R.drawable.placeholder_novel_cover)
            .error(R.drawable.placeholder_novel_cover)
            .diskCacheStrategy(DiskCacheStrategy.ALL)
            .into(ivNovelCoverLarge)
    }

    private fun toggleBookmark() {
        isBookmarked = !isBookmarked

        if (isBookmarked) {
            fabBookmark.setImageResource(android.R.drawable.btn_star_big_on)
            Toast.makeText(this, "Added to bookmarks", Toast.LENGTH_SHORT).show()
            // TODO: Implement actual bookmark API call
        } else {
            fabBookmark.setImageResource(android.R.drawable.btn_star_big_off)
            Toast.makeText(this, "Removed from bookmarks", Toast.LENGTH_SHORT).show()
            // TODO: Implement actual unbookmark API call
        }
    }

    private fun formatNumber(number: Long): String {
        return when {
            number >= 1_000_000 -> String.format("%.1fM", number / 1_000_000.0)
            number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
            else -> number.toString()
        }
    }

    private fun showError(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_LONG).show()
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