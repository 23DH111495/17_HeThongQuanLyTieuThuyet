package com.sinhvien.webnovelapp.fragments

import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.fragment.app.Fragment
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import androidx.viewpager2.widget.ViewPager2
import com.bumptech.glide.Glide
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.activities.NovelDetailActivity
import com.sinhvien.webnovelapp.activities.NovelRankingActivity
import com.sinhvien.webnovelapp.adapters.CompactRankingAdapter
import com.sinhvien.webnovelapp.adapters.NewReleasesAdapter
import com.sinhvien.webnovelapp.adapters.SliderNovelAdapter
import com.sinhvien.webnovelapp.adapters.WeeklyFeaturedAdapter
import com.sinhvien.webnovelapp.adapters.WeeklyFeaturedNetflixAdapter
import com.sinhvien.webnovelapp.adapters.WeeklyFeaturedWebnovelAdapter
import com.sinhvien.webnovelapp.adapters.OngoingNovelsAdapter
import com.sinhvien.webnovelapp.adapters.CompletedNovelsAdapter
import com.sinhvien.webnovelapp.adapters.DiscoveryAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.NovelRankingDto
import com.sinhvien.webnovelapp.models.NovelRankingResponse
import com.sinhvien.webnovelapp.models.NovelResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import androidx.recyclerview.widget.GridLayoutManager
import com.sinhvien.webnovelapp.activities.ViewAllNovelsActivity


class HomeFragment : Fragment() {

    private lateinit var apiService: NovelApiService
    private lateinit var sliderAdapter: SliderNovelAdapter

    // Slider Components
    private lateinit var vpFeaturedSlider: ViewPager2
    private lateinit var sliderIndicator: LinearLayout
    private val sliderHandler = Handler(Looper.getMainLooper())
    private var sliderRunnable: Runnable? = null

    // Three different weekly featured adapters
    private lateinit var weeklyFeaturedEnhancedAdapter: WeeklyFeaturedAdapter
    private lateinit var weeklyFeaturedNetflixAdapter: WeeklyFeaturedNetflixAdapter
    private lateinit var weeklyFeaturedWebnovelAdapter: WeeklyFeaturedWebnovelAdapter

    // Three RecyclerViews
    private lateinit var rvWeeklyFeaturedEnhanced: RecyclerView
    private lateinit var rvWeeklyFeaturedNetflix: RecyclerView
    private lateinit var rvWeeklyFeaturedWebnovel: RecyclerView

    // New releases
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

    private lateinit var ongoingNovelsAdapter: OngoingNovelsAdapter
    private lateinit var completedNovelsAdapter: CompletedNovelsAdapter
    private lateinit var rvOngoingNovels: RecyclerView
    private lateinit var rvCompletedNovels: RecyclerView
    private lateinit var tvViewAllOngoing: TextView
    private lateinit var tvViewAllCompleted: TextView
    private lateinit var discoveryAdapter: DiscoveryAdapter
    private lateinit var rvDiscoverNovels: RecyclerView
    private lateinit var tvDiscoverFilter: TextView
    private lateinit var btnDiscoverPrev: TextView
    private lateinit var btnDiscoverNext: TextView
    private lateinit var tvDiscoverPageInfo: TextView
    private lateinit var pbDiscoverLoading: ProgressBar

    private var currentDiscoverPage = 1
    private var currentPreference = "balanced"  // balanced, popular, recent, random
    private lateinit var llPageNumbers: LinearLayout
    private lateinit var btnDiscoverFirst: TextView
    private lateinit var btnDiscoverLast: TextView
    private var totalDiscoverPages = 3

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        return inflater.inflate(R.layout.fragment_home, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        // Initialize API service
        apiService = ApiClient.getClient().create(NovelApiService::class.java)

        // Initialize all views
        initializeViews(view)

        // Set up all components
        setupSlider()
        setupWeeklyFeaturedEnhanced()
        setupWeeklyFeaturedNetflix()
        setupWeeklyFeaturedWebnovel()
        setupNewReleases()
        setupRankingSection()

        // Load all data
        loadSliderFeaturedNovels()
        loadWeeklyFeaturedNovels()
        loadPremiumNovels()
        loadFeaturedNovels()
        loadNewReleases()
        loadTopRankings()

        setupOngoingNovels()
        setupCompletedNovels()
        loadOngoingNovels()
        loadCompletedNovels()

        setupDiscoverySection()
        loadDiscoverNovels()
    }

    private fun initializeViews(view: View) {
        // Slider views
        vpFeaturedSlider = view.findViewById(R.id.vpFeaturedSlider)
        sliderIndicator = view.findViewById(R.id.sliderIndicator)

        // Three weekly featured RecyclerViews
        rvWeeklyFeaturedEnhanced = view.findViewById(R.id.rvWeeklyFeaturedEnhanced)
        rvWeeklyFeaturedNetflix = view.findViewById(R.id.rvWeeklyFeaturedNetflix)
        rvWeeklyFeaturedWebnovel = view.findViewById(R.id.rvWeeklyFeaturedWebnovel)

        // New releases
        rvNewReleases = view.findViewById(R.id.rvNewReleases)

        // Ranking views
        rvTopRankings = view.findViewById(R.id.rvTopRankings)
        tvViewAllRankings = view.findViewById(R.id.tvViewAllRankings)

        // Podium views
        rankingPodium1 = view.findViewById(R.id.rankingPodium1)
        rankingPodium2 = view.findViewById(R.id.rankingPodium2)
        rankingPodium3 = view.findViewById(R.id.rankingPodium3)
        ivRank1Cover = view.findViewById(R.id.ivRank1Cover)
        ivRank2Cover = view.findViewById(R.id.ivRank2Cover)
        ivRank3Cover = view.findViewById(R.id.ivRank3Cover)
        tvRank1Title = view.findViewById(R.id.tvRank1Title)
        tvRank2Title = view.findViewById(R.id.tvRank2Title)
        tvRank3Title = view.findViewById(R.id.tvRank3Title)
        tvRank1Views = view.findViewById(R.id.tvRank1Views)
        tvRank2Views = view.findViewById(R.id.tvRank2Views)
        tvRank3Views = view.findViewById(R.id.tvRank3Views)

        rvOngoingNovels = view.findViewById(R.id.rvOngoingNovels)
        rvCompletedNovels = view.findViewById(R.id.rvCompletedNovels)
        tvViewAllOngoing = view.findViewById(R.id.tvViewAllOngoing)
        tvViewAllCompleted = view.findViewById(R.id.tvViewAllCompleted)

        rvDiscoverNovels = view.findViewById(R.id.rvDiscoverNovels)
        tvDiscoverFilter = view.findViewById(R.id.tvDiscoverFilter)
        btnDiscoverPrev = view.findViewById(R.id.btnDiscoverPrev)
        btnDiscoverNext = view.findViewById(R.id.btnDiscoverNext)
        pbDiscoverLoading = view.findViewById(R.id.pbDiscoverLoading)

        llPageNumbers = view.findViewById(R.id.llPageNumbers)
        btnDiscoverFirst = view.findViewById(R.id.btnDiscoverFirst)
        btnDiscoverLast = view.findViewById(R.id.btnDiscoverLast)

        val tvViewAllNewReleases = view.findViewById<TextView>(R.id.tvViewAllNewReleases)
        val tvViewAllWebnovel = view.findViewById<TextView>(R.id.tvViewAllWebnovel)

        // Setup click listeners
        tvViewAllOngoing.setOnClickListener {
            val intent = Intent(requireContext(), ViewAllNovelsActivity::class.java)
            intent.putExtra(ViewAllNovelsActivity.EXTRA_LIST_TYPE, ViewAllNovelsActivity.TYPE_ONGOING)
            startActivity(intent)
        }

        tvViewAllCompleted.setOnClickListener {
            val intent = Intent(requireContext(), ViewAllNovelsActivity::class.java)
            intent.putExtra(ViewAllNovelsActivity.EXTRA_LIST_TYPE, ViewAllNovelsActivity.TYPE_COMPLETED)
            startActivity(intent)
        }

        tvViewAllNewReleases.setOnClickListener {
            val intent = Intent(requireContext(), ViewAllNovelsActivity::class.java)
            intent.putExtra(ViewAllNovelsActivity.EXTRA_LIST_TYPE, ViewAllNovelsActivity.TYPE_NEW_RELEASES)
            startActivity(intent)
        }

        tvViewAllWebnovel.setOnClickListener {
            val intent = Intent(requireContext(), ViewAllNovelsActivity::class.java)
            intent.putExtra(ViewAllNovelsActivity.EXTRA_LIST_TYPE, ViewAllNovelsActivity.TYPE_RECOMMEND)
            startActivity(intent)
        }
    }
    private fun setupSlider() {
        sliderAdapter = SliderNovelAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        vpFeaturedSlider.adapter = sliderAdapter
        vpFeaturedSlider.offscreenPageLimit = 1
        vpFeaturedSlider.setPageTransformer(null)

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
            val dot = View(requireContext())
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
            sliderHandler.postDelayed(sliderRunnable!!, 5000)
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
    private fun setupWeeklyFeaturedEnhanced() {
        weeklyFeaturedEnhancedAdapter = WeeklyFeaturedAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvWeeklyFeaturedEnhanced.apply {
            layoutManager = LinearLayoutManager(
                requireContext(),
                LinearLayoutManager.HORIZONTAL,
                false
            )
            adapter = weeklyFeaturedEnhancedAdapter
            setHasFixedSize(true)
        }
    }
    private fun setupWeeklyFeaturedNetflix() {
        weeklyFeaturedNetflixAdapter = WeeklyFeaturedNetflixAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvWeeklyFeaturedNetflix.apply {
            layoutManager = LinearLayoutManager(
                requireContext(),
                LinearLayoutManager.HORIZONTAL,
                false
            )
            adapter = weeklyFeaturedNetflixAdapter
            setHasFixedSize(true)
        }
    }
    private fun setupWeeklyFeaturedWebnovel() {
        weeklyFeaturedWebnovelAdapter = WeeklyFeaturedWebnovelAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvWeeklyFeaturedWebnovel.apply {
            layoutManager = LinearLayoutManager(
                requireContext(),
                LinearLayoutManager.HORIZONTAL,
                false
            )
            adapter = weeklyFeaturedWebnovelAdapter
            setHasFixedSize(true)
        }
    }
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
                // Silently fail
            }
        })
    }
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
                // Silently fail
            }
        })
    }
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
                // Silently fail
            }
        })
    }
    private fun setupNewReleases() {
        newReleasesAdapter = NewReleasesAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvNewReleases.apply {
            layoutManager = LinearLayoutManager(
                requireContext(),
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
                // Silently fail
            }
        })
    }
    private fun setupRankingSection() {
        compactRankingAdapter = CompactRankingAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvTopRankings.apply {
            layoutManager = LinearLayoutManager(requireContext())
            adapter = compactRankingAdapter
            isNestedScrollingEnabled = false
            setHasFixedSize(false)
        }

        tvViewAllRankings.setOnClickListener {
            val intent = Intent(requireContext(), NovelRankingActivity::class.java)
            startActivity(intent)
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

                        if (novels.isNotEmpty()) {
                            updatePodiumRank(1, novels[0])
                        }
                        if (novels.size > 1) {
                            updatePodiumRank(2, novels[1])
                        }
                        if (novels.size > 2) {
                            updatePodiumRank(3, novels[2])
                        }

                        if (novels.size > 3) {
                            val remainingRanks = novels.subList(3, novels.size.coerceAtMost(10))
                            compactRankingAdapter.submitList(remainingRanks)
                        }
                    }
                }
            }

            override fun onFailure(call: Call<NovelRankingResponse>, t: Throwable) {
                // Silently fail
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

                Glide.with(this)
                    .load(coverUrl)
                    .placeholder(R.drawable.ic_book_placeholder)
                    .error(R.drawable.ic_book_placeholder)
                    .centerCrop()
                    .into(ivRank1Cover)

                rankingPodium1.setOnClickListener {
                    val intent = Intent(requireContext(), NovelDetailActivity::class.java)
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
                    val intent = Intent(requireContext(), NovelDetailActivity::class.java)
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
                    val intent = Intent(requireContext(), NovelDetailActivity::class.java)
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
    private fun setupOngoingNovels() {
        ongoingNovelsAdapter = OngoingNovelsAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvOngoingNovels.apply {
            layoutManager = GridLayoutManager(requireContext(), 3) // 3 columns
            adapter = ongoingNovelsAdapter
            isNestedScrollingEnabled = false
            setHasFixedSize(false)
        }


    }
    private fun setupCompletedNovels() {
        completedNovelsAdapter = CompletedNovelsAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvCompletedNovels.apply {
            layoutManager = GridLayoutManager(requireContext(), 3) // 3 columns
            adapter = completedNovelsAdapter
            isNestedScrollingEnabled = false
            setHasFixedSize(false)
        }


    }
    private fun loadOngoingNovels() {
        val call = apiService.getOngoingNovels(page = 1, pageSize = 12, sortBy = "popular")

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        // Take exactly 12 novels, no filtering
                        val novels = novelResponse.Data.take(12)
                        ongoingNovelsAdapter.submitList(novels)
                    }
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Silently fail
            }
        })
    }
    private fun loadCompletedNovels() {
        val call = apiService.getCompletedNovels(page = 1, pageSize = 12, sortBy = "popular")

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        // Take exactly 12 novels, no filtering
                        val novels = novelResponse.Data.take(12)
                        completedNovelsAdapter.submitList(novels)
                    }
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Silently fail
            }
        })
    }
    private fun setupDiscoverySection() {
        discoveryAdapter = DiscoveryAdapter { novel ->
            val intent = Intent(requireContext(), NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvDiscoverNovels.apply {
            layoutManager = LinearLayoutManager(requireContext())
            adapter = discoveryAdapter
            isNestedScrollingEnabled = false
            setHasFixedSize(false)
        }

        // Filter button click
        tvDiscoverFilter.setOnClickListener {
            showPreferenceDialog()
        }

        // First page button - jump to page 1
        btnDiscoverFirst.setOnClickListener {
            if (currentDiscoverPage != 1) {
                currentDiscoverPage = 1
                loadDiscoverNovels()
            }
        }

        // Previous page button - go back one page
        btnDiscoverPrev.setOnClickListener {
            if (currentDiscoverPage > 1) {
                currentDiscoverPage--
                loadDiscoverNovels()
            }
        }

        // Next page button - go forward one page
        btnDiscoverNext.setOnClickListener {
            if (currentDiscoverPage < totalDiscoverPages) {
                currentDiscoverPage++
                loadDiscoverNovels()
            }
        }

        // Last page button - jump to last page
        btnDiscoverLast.setOnClickListener {
            if (currentDiscoverPage != totalDiscoverPages) {
                currentDiscoverPage = totalDiscoverPages
                loadDiscoverNovels()
            }
        }

        // Initialize page number buttons
        setupPageNumbers()
    }
    private fun setupPageNumbers() {
        llPageNumbers.removeAllViews()

        for (page in 1..totalDiscoverPages) {
            val pageButton = TextView(requireContext()).apply {
                text = page.toString()
                textSize = 14f
                setPadding(32, 20, 32, 20)

                // Set initial state based on current page
                if (page == currentDiscoverPage) {
                    setBackgroundResource(R.drawable.page_button_active_bg)
                    setTextColor(resources.getColor(android.R.color.black, null))
                } else {
                    setBackgroundResource(R.drawable.page_button_inactive_bg)
                    setTextColor(resources.getColor(android.R.color.white, null))
                }

                // Add margins between buttons
                val params = LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.WRAP_CONTENT,
                    LinearLayout.LayoutParams.WRAP_CONTENT
                )
                params.setMargins(8, 0, 8, 0)
                layoutParams = params

                // Click listener - navigate to this page
                setOnClickListener {
                    if (currentDiscoverPage != page) {
                        currentDiscoverPage = page
                        loadDiscoverNovels()
                    }
                }
            }

            llPageNumbers.addView(pageButton)
        }
    }
    private fun updatePageButtons() {
        // Update numbered page buttons (1, 2, 3)
        for (i in 0 until llPageNumbers.childCount) {
            val pageButton = llPageNumbers.getChildAt(i) as TextView
            val pageNumber = i + 1

            if (pageNumber == currentDiscoverPage) {
                // Active page - green background, black text
                pageButton.setBackgroundResource(R.drawable.page_button_active_bg)
                pageButton.setTextColor(resources.getColor(android.R.color.black, null))
            } else {
                // Inactive page - dark background, white text
                pageButton.setBackgroundResource(R.drawable.page_button_inactive_bg)
                pageButton.setTextColor(resources.getColor(android.R.color.white, null))
            }
        }

        // Update first page button state
        val isNotFirstPage = currentDiscoverPage > 1
        btnDiscoverFirst.isEnabled = isNotFirstPage
        btnDiscoverFirst.alpha = if (isNotFirstPage) 1.0f else 0.5f
        btnDiscoverFirst.setTextColor(
            if (isNotFirstPage)
                resources.getColor(android.R.color.white, null)
            else
                resources.getColor(android.R.color.darker_gray, null)
        )

        // Update previous button state
        btnDiscoverPrev.isEnabled = isNotFirstPage
        btnDiscoverPrev.alpha = if (isNotFirstPage) 1.0f else 0.5f
        btnDiscoverPrev.setTextColor(
            if (isNotFirstPage)
                resources.getColor(android.R.color.white, null)
            else
                resources.getColor(android.R.color.darker_gray, null)
        )

        // Update next button state
        val isNotLastPage = currentDiscoverPage < totalDiscoverPages
        btnDiscoverNext.isEnabled = isNotLastPage
        btnDiscoverNext.alpha = if (isNotLastPage) 1.0f else 0.5f
        btnDiscoverNext.setTextColor(
            if (isNotLastPage)
                resources.getColor(android.R.color.white, null)
            else
                resources.getColor(android.R.color.darker_gray, null)
        )

        // Update last page button state
        btnDiscoverLast.isEnabled = isNotLastPage
        btnDiscoverLast.alpha = if (isNotLastPage) 1.0f else 0.5f
        btnDiscoverLast.setTextColor(
            if (isNotLastPage)
                resources.getColor(android.R.color.white, null)
            else
                resources.getColor(android.R.color.darker_gray, null)
        )
    }
    private fun showPreferenceDialog() {
        val preferences = arrayOf("Balanced", "Popular", "Recent", "Random")
        val builder = android.app.AlertDialog.Builder(requireContext())
        builder.setTitle("Discovery Preference")
        builder.setItems(preferences) { dialog, which ->
            currentPreference = when (which) {
                0 -> "balanced"
                1 -> "popular"
                2 -> "recent"
                3 -> "random"
                else -> "balanced"
            }
            tvDiscoverFilter.text = preferences[which]

            // Reset to page 1 when changing preference
            currentDiscoverPage = 1
            loadDiscoverNovels()

            dialog.dismiss()
        }
        builder.show()
    }
    private fun loadDiscoverNovels() {
        // Show loading indicator
        pbDiscoverLoading.visibility = View.VISIBLE
        rvDiscoverNovels.visibility = View.GONE

        // Make API call
        val call = apiService.getDiscoverNovels(
            count = 30, // Request 30 novels total (10 per page × 3 pages)
            userId = null,
            preference = currentPreference
        )

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                // Hide loading indicator
                pbDiscoverLoading.visibility = View.GONE
                rvDiscoverNovels.visibility = View.VISIBLE

                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        val allNovels = novelResponse.Data

                        // Calculate which novels to show for current page
                        val itemsPerPage = 10
                        val startIndex = (currentDiscoverPage - 1) * itemsPerPage
                        val endIndex = minOf(startIndex + itemsPerPage, allNovels.size)

                        if (startIndex < allNovels.size) {
                            // Get novels for current page
                            val pageNovels = allNovels.subList(startIndex, endIndex)
                            discoveryAdapter.submitList(pageNovels)

                            // Update all pagination button states
                            updatePageButtons()

                            // Scroll to top of discovery list
                            rvDiscoverNovels.scrollToPosition(0)
                        } else {
                            // No novels for this page, go back to previous page
                            if (currentDiscoverPage > 1) {
                                currentDiscoverPage--
                                loadDiscoverNovels()
                            } else {
                                Toast.makeText(
                                    requireContext(),
                                    "No novels to discover",
                                    Toast.LENGTH_SHORT
                                ).show()
                            }
                        }
                    } else {
                        Toast.makeText(
                            requireContext(),
                            "No novels found",
                            Toast.LENGTH_SHORT
                        ).show()
                    }
                } else {
                    Toast.makeText(
                        requireContext(),
                        "Failed to load novels",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                // Hide loading indicator on error
                pbDiscoverLoading.visibility = View.GONE
                rvDiscoverNovels.visibility = View.VISIBLE

                Toast.makeText(
                    requireContext(),
                    "Error: ${t.message}",
                    Toast.LENGTH_SHORT
                ).show()
            }
        })
    }

    override fun onResume() {
        super.onResume()
        startAutoScroll()
    }

    override fun onPause() {
        super.onPause()
        stopAutoScroll()
    }

    override fun onDestroyView() {
        super.onDestroyView()
        stopAutoScroll()
    }
}