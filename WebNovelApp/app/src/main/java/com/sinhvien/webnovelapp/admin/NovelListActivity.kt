package com.sinhvien.webnovelapp.activities

import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.view.MenuItem
import android.view.View
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.GridLayoutManager
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import androidx.viewpager2.widget.ViewPager2
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.adapters.NovelAdapter
import com.sinhvien.webnovelapp.adapters.SliderNovelAdapter
import com.sinhvien.webnovelapp.adapters.WeeklyFeaturedAdapter
import com.sinhvien.webnovelapp.adapters.WeeklyFeaturedNetflixAdapter
import com.sinhvien.webnovelapp.adapters.WeeklyFeaturedWebnovelAdapter
import com.sinhvien.webnovelapp.adapters.NewReleasesAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.Novel
import com.sinhvien.webnovelapp.models.NovelResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.sinhvien.webnovelapp.adapters.CompactRankingAdapter
import com.sinhvien.webnovelapp.models.NovelRankingResponse
import com.sinhvien.webnovelapp.models.NovelRankingDto
import com.bumptech.glide.Glide

class NovelListActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var novelAdapter: NovelAdapter
    private lateinit var sliderAdapter: SliderNovelAdapter

    // UI Components
    private lateinit var etSearch: EditText
    private lateinit var spinnerStatus: Spinner
    private lateinit var spinnerSort: Spinner
    private lateinit var tvStatus: TextView
    private lateinit var tvCount: TextView
    private lateinit var rvNovels: RecyclerView
    private lateinit var paginationControls: LinearLayout
    private lateinit var tvPageInfo: TextView
    private lateinit var btnSearch: Button
    private lateinit var btnPrevPage: Button
    private lateinit var btnNextPage: Button
    private lateinit var progressBar: ProgressBar

    // Slider Components
    private lateinit var vpFeaturedSlider: ViewPager2
    private lateinit var sliderIndicator: LinearLayout
    private val sliderHandler = Handler(Looper.getMainLooper())
    private var sliderRunnable: Runnable? = null

    // Pagination
    private var currentPage = 1
    private var totalPages = 1
    private val pageSize = 20

    // Filters
    private var currentSearch = ""
    private var currentStatus = ""
    private var currentSort = "updated"

    // Three different weekly featured adapters
    private lateinit var weeklyFeaturedEnhancedAdapter: WeeklyFeaturedAdapter
    private lateinit var weeklyFeaturedNetflixAdapter: WeeklyFeaturedNetflixAdapter
    private lateinit var weeklyFeaturedWebnovelAdapter: WeeklyFeaturedWebnovelAdapter

    // Three RecyclerViews
    private lateinit var rvWeeklyFeaturedEnhanced: RecyclerView
    private lateinit var rvWeeklyFeaturedNetflix: RecyclerView
    private lateinit var rvWeeklyFeaturedWebnovel: RecyclerView

    private lateinit var newReleasesAdapter: NewReleasesAdapter
    private lateinit var rvNewReleases: RecyclerView

    // Ranking components
    private lateinit var compactRankingAdapter: CompactRankingAdapter
    private lateinit var rvTopRankings: RecyclerView
    private lateinit var tvViewAllRankings: TextView

    // Podium views
    private lateinit var rankingPodium1: LinearLayout
    private lateinit var rankingPodium2: LinearLayout
    private lateinit var rankingPodium3: LinearLayout
    private lateinit var ivRank1Cover: ImageView
    private lateinit var ivRank2Cover: ImageView
    private lateinit var ivRank3Cover: ImageView
    private lateinit var tvRank1Title: TextView
    private lateinit var tvRank2Title: TextView
    private lateinit var tvRank3Title: TextView
    private lateinit var tvRank1Views: TextView
    private lateinit var tvRank2Views: TextView
    private lateinit var tvRank3Views: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_novel_list)

        // Set up toolbar
        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = "Browse Novels"
        } catch (e: Exception) {
            // Toolbar setup failed
        }

        // Initialize API service
        apiService = ApiClient.getClient().create(NovelApiService::class.java)

        // Initialize views
        initializeViews()

        // Set up slider
        setupSlider()

        // Setup all three weekly featured sections
        setupWeeklyFeaturedEnhanced()
        setupWeeklyFeaturedNetflix()
        setupWeeklyFeaturedWebnovel()

        // Load data for all three sections with different sources
        loadWeeklyFeaturedNovels()
        loadPremiumNovels() // Netflix style shows Premium novels
        loadFeaturedNovels() // Webnovel style shows Featured novels

        // Set up RecyclerView
        setupRecyclerView()

        // Set up spinners
        setupSpinners()

        // Set up click listeners
        setupClickListeners()

        // Load slider featured novels
        loadSliderFeaturedNovels()

        // Load initial data
        loadNovels()

        setupNewReleases()
        loadNewReleases()

        setupRankingSection()
        loadTopRankings()
    }

    private fun initializeViews() {
        etSearch = findViewById(R.id.etSearch)
        spinnerStatus = findViewById(R.id.spinnerStatus)
        spinnerSort = findViewById(R.id.spinnerSort)
        tvStatus = findViewById(R.id.tvStatus)
        tvCount = findViewById(R.id.tvCount)
        rvNovels = findViewById(R.id.rvNovels)
        paginationControls = findViewById(R.id.paginationControls)
        tvPageInfo = findViewById(R.id.tvPageInfo)
        btnSearch = findViewById(R.id.btnSearch)
        btnPrevPage = findViewById(R.id.btnPrevPage)
        btnNextPage = findViewById(R.id.btnNextPage)
        progressBar = findViewById(R.id.progressBar)

        // Slider views
        vpFeaturedSlider = findViewById(R.id.vpFeaturedSlider)
        sliderIndicator = findViewById(R.id.sliderIndicator)

        // Three weekly featured RecyclerViews
        rvWeeklyFeaturedEnhanced = findViewById(R.id.rvWeeklyFeaturedEnhanced)
        rvWeeklyFeaturedNetflix = findViewById(R.id.rvWeeklyFeaturedNetflix)
        rvWeeklyFeaturedWebnovel = findViewById(R.id.rvWeeklyFeaturedWebnovel)

        rvNewReleases = findViewById(R.id.rvNewReleases)

        // Ranking views
        rvTopRankings = findViewById(R.id.rvTopRankings)
        tvViewAllRankings = findViewById(R.id.tvViewAllRankings)

        // Podium views
        rankingPodium1 = findViewById(R.id.rankingPodium1)
        rankingPodium2 = findViewById(R.id.rankingPodium2)
        rankingPodium3 = findViewById(R.id.rankingPodium3)
        ivRank1Cover = findViewById(R.id.ivRank1Cover)
        ivRank2Cover = findViewById(R.id.ivRank2Cover)
        ivRank3Cover = findViewById(R.id.ivRank3Cover)
        tvRank1Title = findViewById(R.id.tvRank1Title)
        tvRank2Title = findViewById(R.id.tvRank2Title)
        tvRank3Title = findViewById(R.id.tvRank3Title)
        tvRank1Views = findViewById(R.id.tvRank1Views)
        tvRank2Views = findViewById(R.id.tvRank2Views)
        tvRank3Views = findViewById(R.id.tvRank3Views)
    }

    private fun setupSlider() {
        sliderAdapter = SliderNovelAdapter { novel ->
            // Navigate to novel detail
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        vpFeaturedSlider.adapter = sliderAdapter
        vpFeaturedSlider.offscreenPageLimit = 1

        // Remove page transformer to prevent peek effect
        vpFeaturedSlider.setPageTransformer(null)

        // Page change callback for indicators and auto-scroll
        vpFeaturedSlider.registerOnPageChangeCallback(object : ViewPager2.OnPageChangeCallback() {
            override fun onPageSelected(position: Int) {
                super.onPageSelected(position)
                updateSliderIndicators(position)
                resetAutoScroll()
            }
        })
    }

    private fun loadSliderFeaturedNovels() {
        val call = apiService.getSliderFeaturedNovels(count = 10)

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        sliderAdapter.submitList(novelResponse.Data)
                        setupSliderIndicators(novelResponse.Data.size)
                        startAutoScroll()
                    }
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Silently fail - slider is optional feature
            }
        })
    }

    private fun setupSliderIndicators(count: Int) {
        sliderIndicator.removeAllViews()

        for (i in 0 until count) {
            val dot = View(this)
            val params = LinearLayout.LayoutParams(
                resources.getDimensionPixelSize(R.dimen.slider_indicator_size),
                resources.getDimensionPixelSize(R.dimen.slider_indicator_size)
            )
            params.setMargins(6, 0, 6, 0)
            dot.layoutParams = params
            dot.setBackgroundResource(R.drawable.slider_indicator_inactive)
            sliderIndicator.addView(dot)
        }

        if (count > 0) {
            updateSliderIndicators(0)
        }
    }

    private fun updateSliderIndicators(position: Int) {
        for (i in 0 until sliderIndicator.childCount) {
            val dot = sliderIndicator.getChildAt(i)
            dot.setBackgroundResource(
                if (i == position) R.drawable.slider_indicator_active
                else R.drawable.slider_indicator_inactive
            )
        }
    }

    private fun startAutoScroll() {
        sliderRunnable = Runnable {
            val itemCount = sliderAdapter.itemCount
            if (itemCount > 0) {
                val nextItem = (vpFeaturedSlider.currentItem + 1) % itemCount
                vpFeaturedSlider.setCurrentItem(nextItem, true)
            }
            sliderHandler.postDelayed(sliderRunnable!!, 5000) // 5 seconds
        }
        sliderHandler.postDelayed(sliderRunnable!!, 5000)
    }

    private fun resetAutoScroll() {
        sliderRunnable?.let {
            sliderHandler.removeCallbacks(it)
            sliderHandler.postDelayed(it, 5000)
        }
    }

    private fun stopAutoScroll() {
        sliderRunnable?.let {
            sliderHandler.removeCallbacks(it)
        }
    }

    private fun setupRecyclerView() {
        novelAdapter = NovelAdapter { novel ->
            // Navigate to novel detail
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvNovels.apply {
            layoutManager = GridLayoutManager(this@NovelListActivity, 2)
            adapter = novelAdapter
            setHasFixedSize(true)
        }
    }

    // Setup Style 1: Enhanced Weekly Featured
    private fun setupWeeklyFeaturedEnhanced() {
        weeklyFeaturedEnhancedAdapter = WeeklyFeaturedAdapter { novel ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvWeeklyFeaturedEnhanced.apply {
            layoutManager = LinearLayoutManager(
                this@NovelListActivity,
                LinearLayoutManager.HORIZONTAL,
                false
            )
            adapter = weeklyFeaturedEnhancedAdapter
            setHasFixedSize(true)
        }
    }

    // Setup Style 2: Netflix Style - Premium Novels
    private fun setupWeeklyFeaturedNetflix() {
        weeklyFeaturedNetflixAdapter = WeeklyFeaturedNetflixAdapter { novel ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvWeeklyFeaturedNetflix.apply {
            layoutManager = LinearLayoutManager(
                this@NovelListActivity,
                LinearLayoutManager.HORIZONTAL,
                false
            )
            adapter = weeklyFeaturedNetflixAdapter
            setHasFixedSize(true)
        }
    }

    // Setup Style 3: Webnovel Style - Featured Novels
    private fun setupWeeklyFeaturedWebnovel() {
        weeklyFeaturedWebnovelAdapter = WeeklyFeaturedWebnovelAdapter { novel ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvWeeklyFeaturedWebnovel.apply {
            layoutManager = LinearLayoutManager(
                this@NovelListActivity,
                LinearLayoutManager.HORIZONTAL,
                false
            )
            adapter = weeklyFeaturedWebnovelAdapter
            setHasFixedSize(true)
        }
    }

    // Load weekly featured novels for the Enhanced style
    private fun loadWeeklyFeaturedNovels() {
        val call = apiService.getWeeklyFeaturedNovels(count = 10)

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        weeklyFeaturedEnhancedAdapter.submitList(novelResponse.Data)
                    }
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Silently fail - weekly featured is optional feature
            }
        })
    }

    // Load premium novels for Netflix style adapter
    private fun loadPremiumNovels() {
        val call = apiService.getPremiumNovels(page = 1, pageSize = 10, sortBy = "popular")

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        weeklyFeaturedNetflixAdapter.submitList(novelResponse.Data)
                    }
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Silently fail - premium novels section is optional
            }
        })
    }

    // Load featured novels for Webnovel style adapter
    private fun loadFeaturedNovels() {
        val call = apiService.getFeaturedNovelsList(page = 1, pageSize = 10, sortBy = "popular")

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        weeklyFeaturedWebnovelAdapter.submitList(novelResponse.Data)
                    }
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Silently fail - featured novels section is optional
            }
        })
    }

    private fun setupNewReleases() {
        newReleasesAdapter = NewReleasesAdapter { novel ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvNewReleases.apply {
            layoutManager = LinearLayoutManager(
                this@NovelListActivity,
                LinearLayoutManager.HORIZONTAL,
                false
            )
            adapter = newReleasesAdapter
            setHasFixedSize(true)
        }
    }

    private fun loadNewReleases() {
        val call = apiService.getNewlyReleasedNovels(count = 20)

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        newReleasesAdapter.submitList(novelResponse.Data)
                    }
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Silently fail - new releases section is optional
            }
        })
    }



    private fun setupRankingSection() {
        // Setup compact ranking adapter for ranks 4-10
        compactRankingAdapter = CompactRankingAdapter { novel ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvTopRankings.apply {
            layoutManager = LinearLayoutManager(this@NovelListActivity)
            adapter = compactRankingAdapter
            isNestedScrollingEnabled = false
            setHasFixedSize(false)
        }

        // View all rankings click listener
        tvViewAllRankings.setOnClickListener {
            val intent = Intent(this, NovelRankingActivity::class.java)
            startActivity(intent)
        }

        // Podium click listeners
        rankingPodium1.setOnClickListener {
            // Will be set with actual novel ID when data loads
        }
        rankingPodium2.setOnClickListener {
            // Will be set with actual novel ID when data loads
        }
        rankingPodium3.setOnClickListener {
            // Will be set with actual novel ID when data loads
        }
    }

    private fun loadTopRankings() {
        val call = apiService.getNovelRanking(
            type = "views",
            period = "all",
            page = 1,
            pageSize = 10
        )

        call.enqueue(object : Callback<NovelRankingResponse> {
            override fun onResponse(
                call: Call<NovelRankingResponse>,
                response: Response<NovelRankingResponse>
            ) {
                if (response.isSuccessful && response.body() != null) {
                    val rankingResponse = response.body()!!
                    if (rankingResponse.Success && rankingResponse.Data.isNotEmpty()) {
                        val novels = rankingResponse.Data

                        // Update top 3 podium
                        if (novels.isNotEmpty()) {
                            updatePodiumRank(1, novels[0])
                        }
                        if (novels.size > 1) {
                            updatePodiumRank(2, novels[1])
                        }
                        if (novels.size > 2) {
                            updatePodiumRank(3, novels[2])
                        }

                        // Show ranks 4-10 in RecyclerView
                        if (novels.size > 3) {
                            val remainingRanks = novels.subList(3, novels.size.coerceAtMost(10))
                            compactRankingAdapter.submitList(remainingRanks)
                        }
                    }
                }
            }

            override fun onFailure(call: Call<NovelRankingResponse>, t: Throwable) {
                // Silently fail - ranking is optional feature
            }
        })
    }

    private fun updatePodiumRank(rank: Int, novel: NovelRankingDto) {
        val coverUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"
        val viewsText = formatNumber(novel.ViewCount) + " views"

        when (rank) {
            1 -> {
                tvRank1Title.text = novel.Title
                tvRank1Views.text = viewsText

                // Load image OUTSIDE click listener
                Glide.with(this)
                    .load(coverUrl)
                    .placeholder(R.drawable.ic_book_placeholder)
                    .error(R.drawable.ic_book_placeholder)
                    .centerCrop()
                    .into(ivRank1Cover)

                // Set click listener separately
                rankingPodium1.setOnClickListener {
                    val intent = Intent(this, NovelDetailActivity::class.java)
                    intent.putExtra("NOVEL_ID", novel.Id)
                    startActivity(intent)
                }
            }
            2 -> {
                tvRank2Title.text = novel.Title
                tvRank2Views.text = viewsText

                Glide.with(this)
                    .load(coverUrl)
                    .placeholder(R.drawable.ic_book_placeholder)
                    .error(R.drawable.ic_book_placeholder)
                    .centerCrop()
                    .into(ivRank2Cover)

                rankingPodium2.setOnClickListener {
                    val intent = Intent(this, NovelDetailActivity::class.java)
                    intent.putExtra("NOVEL_ID", novel.Id)
                    startActivity(intent)
                }
            }
            3 -> {
                tvRank3Title.text = novel.Title
                tvRank3Views.text = viewsText

                Glide.with(this)
                    .load(coverUrl)
                    .placeholder(R.drawable.ic_book_placeholder)
                    .error(R.drawable.ic_book_placeholder)
                    .centerCrop()
                    .into(ivRank3Cover)

                rankingPodium3.setOnClickListener {
                    val intent = Intent(this, NovelDetailActivity::class.java)
                    intent.putExtra("NOVEL_ID", novel.Id)
                    startActivity(intent)
                }
            }
        }
    }

    private fun formatNumber(number: Long): String {
        return when {
            number >= 1_000_000 -> String.format("%.1fM", number / 1_000_000.0)
            number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
            else -> number.toString()
        }
    }

    private fun setupSpinners() {
        // Status spinner
        val statusOptions = arrayOf("All", "Ongoing", "Completed", "Hiatus", "Dropped")
        val statusAdapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, statusOptions)
        statusAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        spinnerStatus.adapter = statusAdapter

        // Sort spinner
        val sortOptions = arrayOf("Recently Updated", "Most Popular", "Highest Rated", "Most Bookmarked", "Newest")
        val sortAdapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, sortOptions)
        sortAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        spinnerSort.adapter = sortAdapter
    }

    private fun setupClickListeners() {
        btnSearch.setOnClickListener {
            currentSearch = etSearch.text.toString().trim()
            currentStatus = when (spinnerStatus.selectedItemPosition) {
                0 -> ""
                1 -> "Ongoing"
                2 -> "Completed"
                3 -> "Hiatus"
                4 -> "Dropped"
                else -> ""
            }
            currentSort = when (spinnerSort.selectedItemPosition) {
                0 -> "updated"
                1 -> "popular"
                2 -> "rating"
                3 -> "bookmarks"
                4 -> "newest"
                else -> "updated"
            }
            currentPage = 1
            loadNovels()
        }

        btnPrevPage.setOnClickListener {
            if (currentPage > 1) {
                currentPage--
                loadNovels()
            }
        }

        btnNextPage.setOnClickListener {
            if (currentPage < totalPages) {
                currentPage++
                loadNovels()
            }
        }

        // Optional: Set up "See All" click listeners
        findViewById<TextView>(R.id.tvViewAllNetflix)?.setOnClickListener {
            Toast.makeText(this, "View All Premium Novels", Toast.LENGTH_SHORT).show()
            // TODO: Navigate to a full list of premium novels
        }

        findViewById<TextView>(R.id.tvViewAllWebnovel)?.setOnClickListener {
            Toast.makeText(this, "View All Featured Novels", Toast.LENGTH_SHORT).show()
            // TODO: Navigate to a full list of featured novels
        }
    }

    private fun loadNovels() {
        showLoading(true)
        updateStatus("Loading novels...", "#FF9800")

        val call = apiService.getNovels(
            search = currentSearch,
            status = currentStatus,
            sortBy = currentSort,
            page = currentPage,
            pageSize = pageSize
        )

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                showLoading(false)

                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success) {
                        novelAdapter.submitList(novelResponse.Data)
                        totalPages = novelResponse.TotalPages

                        updateStatus("Loaded ${novelResponse.Data.size} novels", "#4CAF50")
                        tvCount.text = "${novelResponse.TotalCount} total"

                        // Update pagination
                        tvPageInfo.text = "Page $currentPage of $totalPages"
                        btnPrevPage.isEnabled = currentPage > 1
                        btnNextPage.isEnabled = currentPage < totalPages
                        paginationControls.visibility = if (totalPages > 1) View.VISIBLE else View.GONE
                    } else {
                        showError("API returned success = false")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                showLoading(false)
                showError("Network Error: ${t.message}")
                Toast.makeText(this@NovelListActivity, "Failed to load novels", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun showLoading(show: Boolean) {
        progressBar.visibility = if (show) View.VISIBLE else View.GONE
        rvNovels.visibility = if (show) View.GONE else View.VISIBLE
    }

    private fun updateStatus(message: String, color: String) {
        tvStatus.text = message
        try {
            tvStatus.setTextColor(android.graphics.Color.parseColor(color))
        } catch (e: IllegalArgumentException) {
            // Fallback to default color
        }
    }

    private fun showError(errorMessage: String) {
        updateStatus("Error: $errorMessage", "#f44336")
        tvCount.text = ""
        Toast.makeText(this, errorMessage, Toast.LENGTH_LONG).show()
    }

    override fun onResume() {
        super.onResume()
        startAutoScroll()
    }

    override fun onPause() {
        super.onPause()
        stopAutoScroll()
    }

    override fun onDestroy() {
        super.onDestroy()
        stopAutoScroll()
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