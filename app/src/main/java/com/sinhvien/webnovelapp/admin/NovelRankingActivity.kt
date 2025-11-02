package com.sinhvien.webnovelapp.activities

import android.content.Intent
import android.graphics.Color
import android.os.Bundle
import android.view.MenuItem
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.google.android.material.appbar.CollapsingToolbarLayout
import com.google.android.material.chip.Chip
import com.google.android.material.tabs.TabLayout
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.adapters.NovelRankingAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.NovelRankingDto
import com.sinhvien.webnovelapp.models.NovelRankingResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import java.text.DecimalFormat

class NovelRankingActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var rankingAdapter: NovelRankingAdapter

    // UI Components
    private lateinit var collapsingToolbar: CollapsingToolbarLayout
    private lateinit var tabLayout: TabLayout
    private lateinit var chipAll: Chip
    private lateinit var chipDaily: Chip
    private lateinit var chipWeekly: Chip
    private lateinit var chipMonthly: Chip
    private lateinit var chipYearly: Chip
    private lateinit var rvRanking: RecyclerView
    private lateinit var progressBar: ProgressBar
    private lateinit var tvTotalNovels: TextView
    private lateinit var tvTotalViews: TextView
    private lateinit var tvAvgRating: TextView
    private lateinit var paginationControls: LinearLayout
    private lateinit var tvPageInfo: TextView
    private lateinit var btnPrevPage: Button
    private lateinit var btnNextPage: Button

    // Top 3 Podium Views
    private lateinit var podiumRank1: View
    private lateinit var podiumRank2: View
    private lateinit var podiumRank3: View

    // Filters
    private var currentType = "views"
    private var currentPeriod = "all"
    private var currentPage = 1
    private var totalPages = 1
    private val pageSize = 50

    // Top 3 novels cache
    private var topNovels = mutableListOf<NovelRankingDto>()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_novel_ranking)

        // Setup toolbar
        setSupportActionBar(findViewById(R.id.toolbar))
        supportActionBar?.setDisplayHomeAsUpEnabled(true)
        supportActionBar?.title = ""

        // Initialize
        apiService = ApiClient.getClient().create(NovelApiService::class.java)
        initializeViews()
        setupTabs()
        setupPeriodChips()
        setupRecyclerView()
        setupClickListeners()

        // Load initial data
        loadRankingData()
    }

    private fun initializeViews() {
        collapsingToolbar = findViewById(R.id.collapsingToolbar)
        collapsingToolbar.title = "Novel Rankings"
        collapsingToolbar.setExpandedTitleColor(Color.WHITE)
        collapsingToolbar.setCollapsedTitleTextColor(Color.WHITE)

        tabLayout = findViewById(R.id.tabLayout)
        chipAll = findViewById(R.id.chipAll)
        chipDaily = findViewById(R.id.chipDaily)
        chipWeekly = findViewById(R.id.chipWeekly)
        chipMonthly = findViewById(R.id.chipMonthly)
        chipYearly = findViewById(R.id.chipYearly)
        rvRanking = findViewById(R.id.rvRanking)
        progressBar = findViewById(R.id.progressBar)
        tvTotalNovels = findViewById(R.id.tvTotalNovels)
        tvTotalViews = findViewById(R.id.tvTotalViews)
        tvAvgRating = findViewById(R.id.tvAvgRating)
        paginationControls = findViewById(R.id.paginationControls)
        tvPageInfo = findViewById(R.id.tvPageInfo)
        btnPrevPage = findViewById(R.id.btnPrevPage)
        btnNextPage = findViewById(R.id.btnNextPage)

        // Podium views
//        podiumRank1 = findViewById(R.id.podiumRank1)
//        podiumRank2 = findViewById(R.id.podiumRank2)
//        podiumRank3 = findViewById(R.id.podiumRank3)
    }

    private fun setupTabs() {
        tabLayout.addTab(tabLayout.newTab().setText("👁 Views"))
        tabLayout.addTab(tabLayout.newTab().setText("⭐ Rating"))
        tabLayout.addTab(tabLayout.newTab().setText("🔖 Bookmarks"))
        tabLayout.addTab(tabLayout.newTab().setText("📚 Chapters"))
        tabLayout.addTab(tabLayout.newTab().setText("📝 Words"))

        tabLayout.addOnTabSelectedListener(object : TabLayout.OnTabSelectedListener {
            override fun onTabSelected(tab: TabLayout.Tab?) {
                currentType = when (tab?.position) {
                    0 -> "views"
                    1 -> "rating"
                    2 -> "bookmarks"
                    3 -> "chapters"
                    4 -> "words"
                    else -> "views"
                }
                currentPage = 1
                loadRankingData()
            }

            override fun onTabUnselected(tab: TabLayout.Tab?) {}
            override fun onTabReselected(tab: TabLayout.Tab?) {}
        })
    }

    private fun setupPeriodChips() {
        chipAll.setOnCheckedChangeListener { _, isChecked ->
            if (isChecked) {
                currentPeriod = "all"
                currentPage = 1
                loadRankingData()
            }
        }

        chipDaily.setOnCheckedChangeListener { _, isChecked ->
            if (isChecked) {
                currentPeriod = "daily"
                currentPage = 1
                loadRankingData()
            }
        }

        chipWeekly.setOnCheckedChangeListener { _, isChecked ->
            if (isChecked) {
                currentPeriod = "weekly"
                currentPage = 1
                loadRankingData()
            }
        }

        chipMonthly.setOnCheckedChangeListener { _, isChecked ->
            if (isChecked) {
                currentPeriod = "monthly"
                currentPage = 1
                loadRankingData()
            }
        }

        chipYearly.setOnCheckedChangeListener { _, isChecked ->
            if (isChecked) {
                currentPeriod = "yearly"
                currentPage = 1
                loadRankingData()
            }
        }
    }

    private fun setupRecyclerView() {
        rankingAdapter = NovelRankingAdapter { novel ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvRanking.apply {
            layoutManager = LinearLayoutManager(this@NovelRankingActivity)
            adapter = rankingAdapter
            setHasFixedSize(true)
        }
    }

    private fun setupClickListeners() {
        btnPrevPage.setOnClickListener {
            if (currentPage > 1) {
                currentPage--
                loadRankingData()
                rvRanking.smoothScrollToPosition(0)
            }
        }

        btnNextPage.setOnClickListener {
            if (currentPage < totalPages) {
                currentPage++
                loadRankingData()
                rvRanking.smoothScrollToPosition(0)
            }
        }

        // Setup podium click listeners using the LinearLayout IDs
        findViewById<View>(R.id.rankingPodium1).setOnClickListener {
            if (topNovels.size > 0) navigateToNovel(topNovels[0].Id)
        }
        findViewById<View>(R.id.rankingPodium2).setOnClickListener {
            if (topNovels.size > 1) navigateToNovel(topNovels[1].Id)
        }
        findViewById<View>(R.id.rankingPodium3).setOnClickListener {
            if (topNovels.size > 2) navigateToNovel(topNovels[2].Id)
        }
    }

    private fun loadRankingData() {
        showLoading(true)

        val call = apiService.getNovelRanking(
            type = currentType,
            period = currentPeriod,
            page = currentPage,
            pageSize = pageSize
        )

        call.enqueue(object : Callback<NovelRankingResponse> {
            override fun onResponse(
                call: Call<NovelRankingResponse>,
                response: Response<NovelRankingResponse>
            ) {
                showLoading(false)

                if (response.isSuccessful && response.body() != null) {
                    val rankingResponse = response.body()!!
                    if (rankingResponse.Success && rankingResponse.Data.isNotEmpty()) {
                        // Update adapter
                        rankingAdapter.submitList(rankingResponse.Data)
                        totalPages = rankingResponse.TotalPages

                        // Update pagination
                        tvPageInfo.text = "Page $currentPage of $totalPages"
                        btnPrevPage.isEnabled = currentPage > 1
                        btnNextPage.isEnabled = currentPage < totalPages
                        paginationControls.visibility = if (totalPages > 1) View.VISIBLE else View.GONE

                        // Update stats
                        updateStats(rankingResponse)

                        // Update top 3 podium (only on first page)
                        if (currentPage == 1) {
                            topNovels.clear()
                            topNovels.addAll(rankingResponse.Data.take(3))
                            updatePodium(topNovels)
                        }
                    } else {
                        showError("No ranking data available")
                    }
                } else {
                    showError("Failed to load rankings: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<NovelRankingResponse>, t: Throwable) {
                showLoading(false)
                showError("Network error: ${t.message}")
            }
        })
    }

    private fun updateStats(response: NovelRankingResponse) {
        tvTotalNovels.text = formatNumber(response.TotalCount.toLong())

        // Calculate total views and average rating
        var totalViews = 0L
        var totalRating = 0.0
        var ratedCount = 0

        response.Data.forEach { novel ->
            totalViews += novel.ViewCount
            if (novel.AverageRating > 0) {
                totalRating += novel.AverageRating
                ratedCount++
            }
        }

        tvTotalViews.text = formatNumber(totalViews)
        tvAvgRating.text = if (ratedCount > 0) {
            String.format("%.1f", totalRating / ratedCount)
        } else {
            "0.0"
        }
    }

    private fun updatePodium(novels: List<NovelRankingDto>) {
        // Update rank 1
        if (novels.isNotEmpty()) {
            val rank1 = novels[0]
            findViewById<TextView>(R.id.tvRank1Title).text = rank1.Title
            findViewById<TextView>(R.id.tvRank1Views).text = "${formatNumber(rank1.ViewCount)} views"

            val coverUrl1 = "${ApiClient.getBaseUrl()}api/novels/${rank1.Id}/cover"
            Glide.with(this)
                .load(coverUrl1)
                .placeholder(R.drawable.ic_book_placeholder)
                .error(R.drawable.ic_book_placeholder)
                .centerCrop()
                .into(findViewById(R.id.ivRank1Cover))
        }

        // Update rank 2
        if (novels.size > 1) {
            val rank2 = novels[1]
            findViewById<TextView>(R.id.tvRank2Title).text = rank2.Title
            findViewById<TextView>(R.id.tvRank2Views).text = "${formatNumber(rank2.ViewCount)} views"

            val coverUrl2 = "${ApiClient.getBaseUrl()}api/novels/${rank2.Id}/cover"
            Glide.with(this)
                .load(coverUrl2)
                .placeholder(R.drawable.ic_book_placeholder)
                .error(R.drawable.ic_book_placeholder)
                .centerCrop()
                .into(findViewById(R.id.ivRank2Cover))
        }

        // Update rank 3
        if (novels.size > 2) {
            val rank3 = novels[2]
            findViewById<TextView>(R.id.tvRank3Title).text = rank3.Title
            findViewById<TextView>(R.id.tvRank3Views).text = "${formatNumber(rank3.ViewCount)} views"

            val coverUrl3 = "${ApiClient.getBaseUrl()}api/novels/${rank3.Id}/cover"
            Glide.with(this)
                .load(coverUrl3)
                .placeholder(R.drawable.ic_book_placeholder)
                .error(R.drawable.ic_book_placeholder)
                .centerCrop()
                .into(findViewById(R.id.ivRank3Cover))
        }
    }

    private fun updatePodiumView(podiumView: View, novel: NovelRankingDto) {
        val ivCover = podiumView.findViewById<ImageView>(R.id.ivCover)
        val tvTitle = podiumView.findViewById<TextView>(R.id.tvTitle)
        val tvViews = podiumView.findViewById<TextView>(R.id.tvViews)

        tvTitle.text = novel.Title
        tvViews.text = "${formatNumber(novel.ViewCount)} views"

        // Load cover
        val coverUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"
        Glide.with(this)
            .load(coverUrl)
            .placeholder(R.drawable.ic_book_placeholder)
            .error(R.drawable.ic_book_placeholder)
            .centerCrop()
            .into(ivCover)
    }

    private fun navigateToNovel(novelId: Int) {
        val intent = Intent(this, NovelDetailActivity::class.java)
        intent.putExtra("NOVEL_ID", novelId)
        startActivity(intent)
    }

    private fun showLoading(show: Boolean) {
        progressBar.visibility = if (show) View.VISIBLE else View.GONE
        rvRanking.visibility = if (show) View.GONE else View.VISIBLE
    }

    private fun showError(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_SHORT).show()
    }

    private fun formatNumber(number: Long): String {
        return when {
            number >= 1_000_000 -> String.format("%.1fM", number / 1_000_000.0)
            number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
            else -> number.toString()
        }
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