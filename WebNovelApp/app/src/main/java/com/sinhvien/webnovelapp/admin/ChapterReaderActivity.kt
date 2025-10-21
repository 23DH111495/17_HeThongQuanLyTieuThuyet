package com.sinhvien.webnovelapp.activities

import android.os.Bundle
import android.view.MenuItem
import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.ChapterDetailResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import androidx.cardview.widget.CardView

class ChapterReaderActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService

    // UI Components
    private lateinit var tvNovelTitle: TextView
    private lateinit var tvChapterTitle: TextView
    private lateinit var tvChapterInfo: TextView
    private lateinit var tvChapterContent: TextView
    private lateinit var tvAccessInfo: TextView
    private lateinit var layoutLocked: CardView
    private lateinit var tvPreviewContent: TextView
    private lateinit var tvUnlockPrice: TextView
    private lateinit var btnUnlock: Button
    private lateinit var progressBar: ProgressBar
    private lateinit var contentLayout: LinearLayout
    private lateinit var navigationButtons: LinearLayout
    private lateinit var btnPrevChapter: Button
    private lateinit var btnNextChapter: Button

    // Data
    private var novelId: Int = 0
    private var chapterNumber: Int = 0
    private var userId: Int? = null // TODO: Get from session/preferences

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_chapter_reader)

        // Get data from intent
        novelId = intent.getIntExtra("NOVEL_ID", 0)
        chapterNumber = intent.getIntExtra("CHAPTER_NUMBER", 0)

        if (novelId == 0) {
            Toast.makeText(this, "Invalid chapter data", Toast.LENGTH_SHORT).show()
            finish()
            return
        }

        // Set up toolbar
        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = "Chapter $chapterNumber"
        } catch (e: Exception) {
            // Toolbar setup failed
        }

        // Initialize API service
        apiService = ApiClient.getClient().create(NovelApiService::class.java)

        // Initialize views
        initializeViews()

        // Set up click listeners
        setupClickListeners()

        // Load chapter
        loadChapter()
    }

    private fun initializeViews() {
        tvNovelTitle = findViewById(R.id.tvNovelTitle)
        tvChapterTitle = findViewById(R.id.tvChapterTitle)
        tvChapterInfo = findViewById(R.id.tvChapterInfo)
        tvChapterContent = findViewById(R.id.tvChapterContent)
        tvAccessInfo = findViewById(R.id.tvAccessInfo)
        layoutLocked = findViewById(R.id.layoutLocked)
        tvPreviewContent = findViewById(R.id.tvPreviewContent)
        tvUnlockPrice = findViewById(R.id.tvUnlockPrice)
        btnUnlock = findViewById(R.id.btnUnlock)
        progressBar = findViewById(R.id.progressBar)
        contentLayout = findViewById(R.id.contentLayout)
        navigationButtons = findViewById(R.id.navigationButtons)
        btnPrevChapter = findViewById(R.id.btnPrevChapter)
        btnNextChapter = findViewById(R.id.btnNextChapter)
    }

    private fun setupClickListeners() {
        btnPrevChapter.setOnClickListener {
            if (chapterNumber > 1) {
                chapterNumber--
                loadChapter()
            }
        }

        btnNextChapter.setOnClickListener {
            chapterNumber++
            loadChapter()
        }

        btnUnlock.setOnClickListener {
            // TODO: Implement unlock functionality
            Toast.makeText(this, "Unlock feature coming soon", Toast.LENGTH_SHORT).show()
        }
    }

    private fun loadChapter() {
        showLoading(true)

        val call = apiService.getChapterDetail(
            novelId = novelId,
            chapterNumber = chapterNumber,
            userId = userId
        )

        call.enqueue(object : Callback<ChapterDetailResponse> {
            override fun onResponse(
                call: Call<ChapterDetailResponse>,
                response: Response<ChapterDetailResponse>
            ) {
                showLoading(false)

                if (response.isSuccessful && response.body() != null) {
                    val chapterResponse = response.body()!!
                    if (chapterResponse.Success) {
                        displayChapter(chapterResponse)
                    } else {
                        showError("Failed to load chapter")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<ChapterDetailResponse>, t: Throwable) {
                showLoading(false)
                showError("Network Error: ${t.message}")
            }
        })
    }

    private fun displayChapter(response: ChapterDetailResponse) {
        val chapter = response.Data
        val accessInfo = response.AccessInfo
        val novel = response.Novel

        // Set novel and chapter titles
        tvNovelTitle.text = novel.Title

        // Handle null or empty title
        val chapterTitle = if (chapter.Title.isNullOrEmpty()) {
            if (chapter.ChapterNumber == 0) "Prologue" else "Chapter ${chapter.ChapterNumber}"
        } else {
            "Chapter ${chapter.ChapterNumber}: ${chapter.Title}"
        }

        tvChapterTitle.text = chapterTitle
        supportActionBar?.title = if (chapter.ChapterNumber == 0) "Prologue" else "Chapter ${chapter.ChapterNumber}"

        // Set chapter info
        tvChapterInfo.text = "${formatNumber(chapter.WordCount)} words • ${chapter.PublishDate ?: "Unknown date"}"

        // Handle access
        if (accessInfo.HasAccess) {
            // User has access - show content
            layoutLocked.visibility = View.GONE
            tvChapterContent.visibility = View.VISIBLE
            tvChapterContent.text = chapter.Content ?: "No content available"

            // Set access info message
            val accessMessage = when (accessInfo.AccessReason) {
                "free" -> "This chapter is free to read"
                "premium" -> "Unlocked with Premium membership"
                "purchased" -> "You've purchased this chapter"
                else -> ""
            }
            tvAccessInfo.text = accessMessage
            tvAccessInfo.visibility = if (accessMessage.isNotEmpty()) View.VISIBLE else View.GONE
        } else {
            // User doesn't have access - show locked state
            layoutLocked.visibility = View.VISIBLE
            tvChapterContent.visibility = View.GONE
            tvAccessInfo.visibility = View.GONE

            // Show preview
            tvPreviewContent.text = chapter.PreviewContent ?: "No preview available"

            // Show unlock info
            val unlockMessage = when (accessInfo.AccessReason) {
                "login_required" -> "Please login to read this chapter"
                "locked" -> "Unlock this chapter to continue reading"
                else -> "This chapter is locked"
            }

            tvUnlockPrice.text = if (accessInfo.RequiredCoins > 0) {
                "Unlock for ${accessInfo.RequiredCoins} coins"
            } else if (accessInfo.IsPremium) {
                "Premium membership required"
            } else {
                unlockMessage
            }

            btnUnlock.text = if (accessInfo.AccessReason == "login_required") {
                "Login"
            } else {
                "Unlock Chapter"
            }
        }

        // Update navigation buttons
        btnPrevChapter.isEnabled = chapterNumber > 1
    }

    private fun formatNumber(number: Int): String {
        return when {
            number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
            else -> number.toString()
        }
    }

    private fun showLoading(show: Boolean) {
        progressBar.visibility = if (show) View.VISIBLE else View.GONE
        contentLayout.visibility = if (show) View.GONE else View.VISIBLE
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