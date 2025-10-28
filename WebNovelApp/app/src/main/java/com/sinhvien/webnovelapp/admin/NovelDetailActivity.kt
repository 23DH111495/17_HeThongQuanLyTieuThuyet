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
import com.sinhvien.webnovelapp.models.GenreInfo
import com.sinhvien.webnovelapp.models.TagInfo
import com.sinhvien.webnovelapp.models.NovelDetailResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class NovelDetailActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var chapterAdapter: ChapterSummaryAdapter
    private lateinit var synopsisGradient: View
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
    private lateinit var btnSeeMore: Button
    private lateinit var chipGroupGenres: ChipGroup
    private lateinit var chipGroupTags: ChipGroup
    private lateinit var rvRecentChapters: RecyclerView
    private lateinit var btnViewAllChapters: Button
    private lateinit var fabBookmark: FloatingActionButton

    private var novelId: Int = 0
    private var isBookmarked = false
    private var isFullSynopsisShown = false
    private var fullSynopsisText = ""

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_novel_detail)

        novelId = intent.getIntExtra("NOVEL_ID", 0)

        if (novelId == 0) {
            Toast.makeText(this, "Invalid novel ID", Toast.LENGTH_SHORT).show()
            finish()
            return
        }

        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = ""
        } catch (e: Exception) {
            // Toolbar setup failed
        }

        apiService = ApiClient.getClient().create(NovelApiService::class.java)

        initializeViews()
        setupRecyclerView()
        setupClickListeners()
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
        btnSeeMore = findViewById(R.id.btnSeeMore)
        //chipGroupGenres = findViewById(R.id.chipGroupGenres)
        chipGroupTags = findViewById(R.id.chipGroupTags)
        rvRecentChapters = findViewById(R.id.rvRecentChapters)
        btnViewAllChapters = findViewById(R.id.btnViewAllChapters)
        fabBookmark = findViewById(R.id.fabBookmark)
        synopsisGradient = findViewById(R.id.synopsisGradient)
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
            val intent = Intent(this, ChapterListActivity::class.java)
            intent.putExtra("NOVEL_ID", novelId)
            startActivity(intent)
        }

        fabBookmark.setOnClickListener {
            toggleBookmark()
        }

        btnSeeMore.setOnClickListener {
            toggleSynopsisExpansion()
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
        tvNovelTitle.text = novel.Title
        supportActionBar?.title = novel.Title

        val authorText = if (novel.Author?.IsVerified == true) {
            "by ${novel.Author.PenName} ✓"
        } else {
            "by ${novel.Author?.PenName ?: "Unknown Author"}"
        }
        tvNovelAuthor.text = authorText

        tvNovelStatus.text = novel.Status
        when (novel.Status.lowercase()) {
            "ongoing" -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#000000"))
                tvNovelStatus.setBackgroundResource(R.drawable.bg_status_active)
            }
            "completed" -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#000000"))
            }
            else -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#000000"))
            }
        }

        tvNovelRating.text = "★ ${String.format("%.2f", novel.AverageRating)} (${novel.TotalRatings} ratings)"
        tvNovelStats.text = "${novel.TotalChapters} chapters • ${formatNumber(novel.WordCount)} words"
        tvNovelViews.text = "${formatNumber(novel.ViewCount)} views • ${formatNumber(novel.BookmarkCount)} bookmarks"

        // Store full synopsis and display truncated version
        fullSynopsisText = novel.Synopsis ?: "No synopsis available."
        displaySynopsisTruncated()

        // Display Genres
        displayGenres(novel.Genres)

        // Display Tags
        displayTags(novel.Tags)

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

    private fun displaySynopsisTruncated() {
        val maxLines = 4
        tvNovelSynopsis.maxLines = maxLines
        tvNovelSynopsis.text = fullSynopsisText

        // Check if text is actually truncated
        tvNovelSynopsis.post {
            val lineCount = tvNovelSynopsis.lineCount
            if (lineCount >= maxLines || fullSynopsisText.length > 200) {
                btnSeeMore.visibility = View.VISIBLE
                synopsisGradient.visibility = View.VISIBLE
                btnSeeMore.text = "See more ▼"
                isFullSynopsisShown = false
            } else {
                btnSeeMore.visibility = View.GONE
                synopsisGradient.visibility = View.GONE
            }
        }
    }

    private fun toggleSynopsisExpansion() {
        if (isFullSynopsisShown) {
            // Collapse
            tvNovelSynopsis.maxLines = 4
            synopsisGradient.visibility = View.VISIBLE
            btnSeeMore.text = "See more ▼"
            isFullSynopsisShown = false
        } else {
            // Expand
            tvNovelSynopsis.maxLines = Int.MAX_VALUE
            synopsisGradient.visibility = View.GONE
            btnSeeMore.text = "See less ▲"
            isFullSynopsisShown = true
        }
    }

    private fun displayGenres(genres: List<GenreInfo>?) {
        val genresContainer = findViewById<LinearLayout>(R.id.genresContainer)
        genresContainer.removeAllViews()

        genres?.forEach { genre ->
            val genreChip = TextView(this).apply {
                text = genre.Name
                textSize = 12f
                setTextColor(android.graphics.Color.BLACK)
                typeface = android.graphics.Typeface.DEFAULT_BOLD

                // Padding for the chip
                setPadding(32, 16, 32, 16)

                // Set background color with slight transparency
                try {
                    val baseColor = android.graphics.Color.parseColor(genre.ColorCode ?: "#4A90E2")
                    // Create a slightly transparent version
                    val alpha = 230 // ~90% opacity
                    val transparentColor = android.graphics.Color.argb(
                        alpha,
                        android.graphics.Color.red(baseColor),
                        android.graphics.Color.green(baseColor),
                        android.graphics.Color.blue(baseColor)
                    )
                    setBackgroundColor(transparentColor)
                } catch (e: Exception) {
                    setBackgroundColor(android.graphics.Color.parseColor("#4A90E2"))
                }

                // Make corners rounded
                background = createRoundedBackground(genre.ColorCode ?: "#4A90E2")

                // Set layout params with margins
                val params = LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.WRAP_CONTENT,
                    LinearLayout.LayoutParams.WRAP_CONTENT
                )
                params.setMargins(0, 0, 16, 0) // Right margin between chips
                layoutParams = params

                // Slight elevation effect
                elevation = 2f
            }

            genresContainer.addView(genreChip)
        }
    }

    // Helper method to create rounded background
    private fun createRoundedBackground(colorCode: String): android.graphics.drawable.GradientDrawable {
        val shape = android.graphics.drawable.GradientDrawable()
        shape.shape = android.graphics.drawable.GradientDrawable.RECTANGLE
        shape.cornerRadius = 20f // Rounded corners

        try {
            val baseColor = android.graphics.Color.parseColor(colorCode)
            val alpha = 230 // ~90% opacity
            val transparentColor = android.graphics.Color.argb(
                alpha,
                android.graphics.Color.red(baseColor),
                android.graphics.Color.green(baseColor),
                android.graphics.Color.blue(baseColor)
            )
            shape.setColor(transparentColor)
        } catch (e: Exception) {
            shape.setColor(android.graphics.Color.parseColor("#4A90E2"))
        }

        return shape
    }

    private fun displayTags(tags: List<TagInfo>?) {
        chipGroupTags.removeAllViews()

        tags?.forEach { tag ->
            try {
                val chip = Chip(this)
                chip.text = tag.Name // Remove the # prefix
                chip.setTextColor(android.graphics.Color.parseColor("#FFFFFF"))
                chip.setChipBackgroundColor(android.content.res.ColorStateList.valueOf(
                    android.graphics.Color.parseColor("#3d3d3d")
                ))
                chip.chipStrokeWidth = 1f
                chip.chipStrokeColor = android.content.res.ColorStateList.valueOf(
                    android.graphics.Color.parseColor("#555555")
                )
                chip.isClickable = false
                chip.isCheckable = false
                chipGroupTags.addView(chip)
            } catch (e: Exception) {
                // If Chip fails, use TextView instead
                val textView = TextView(this)
                textView.text = tag.Name // Remove the # prefix
                textView.textSize = 12f
                textView.setTextColor(android.graphics.Color.parseColor("#FFFFFF"))
                textView.setPadding(24, 12, 24, 12)
                textView.setBackgroundResource(R.drawable.bg_tag_chip) // You'll need to create this drawable

                val params = LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.WRAP_CONTENT,
                    LinearLayout.LayoutParams.WRAP_CONTENT
                )
                params.setMargins(12, 12, 12, 12)
                textView.layoutParams = params
                chipGroupTags.addView(textView)
            }
        }
    }

    private fun toggleBookmark() {
        isBookmarked = !isBookmarked

        if (isBookmarked) {
            fabBookmark.setImageResource(android.R.drawable.btn_star_big_on)
            Toast.makeText(this, "Added to bookmarks", Toast.LENGTH_SHORT).show()
        } else {
            fabBookmark.setImageResource(android.R.drawable.btn_star_big_off)
            Toast.makeText(this, "Removed from bookmarks", Toast.LENGTH_SHORT).show()
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