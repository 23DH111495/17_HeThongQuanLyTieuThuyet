package com.sinhvien.webnovelapp.ThieuKhang

import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.text.Editable
import android.text.TextWatcher
import android.util.Log
import android.view.View
import android.view.inputmethod.EditorInfo
import android.view.inputmethod.InputMethodManager
import android.widget.EditText
import android.widget.ImageView
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.activities.NovelDetailActivity
import com.sinhvien.webnovelapp.adapters.SearchSuggestionAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.Novel
import com.sinhvien.webnovelapp.models.NovelResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class SearchActivity : AppCompatActivity() {

    private lateinit var etSearch: EditText
    private lateinit var btnBack: ImageView
    private lateinit var btnClearSearch: ImageView
    private lateinit var rvSearchResults: RecyclerView
    private lateinit var tvEmptyState: TextView
    private lateinit var searchSuggestionAdapter: SearchSuggestionAdapter
    private lateinit var novelApiService: NovelApiService

    private val searchHandler = Handler(Looper.getMainLooper())
    private var searchRunnable: Runnable? = null
    private val SEARCH_DELAY = 500L

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_search)

        // Initialize API Service
        novelApiService = ApiClient.getClient().create(NovelApiService::class.java)

        // Initialize views
        initializeViews()

        // Setup RecyclerView
        setupRecyclerView()

        // Setup search functionality
        setupSearch()

        // Show keyboard automatically
        etSearch.requestFocus()
        val imm = getSystemService(INPUT_METHOD_SERVICE) as InputMethodManager
        imm.showSoftInput(etSearch, InputMethodManager.SHOW_IMPLICIT)
    }

    private fun initializeViews() {
        etSearch = findViewById(R.id.etSearch)
        btnBack = findViewById(R.id.btnBack)
        btnClearSearch = findViewById(R.id.btnClearSearch)
        rvSearchResults = findViewById(R.id.rvSearchResults)
        tvEmptyState = findViewById(R.id.tvEmptyState)
    }

    private fun setupRecyclerView() {
        searchSuggestionAdapter = SearchSuggestionAdapter { novel ->
            navigateToNovelDetail(novel)
        }

        rvSearchResults.apply {
            layoutManager = LinearLayoutManager(this@SearchActivity)
            adapter = searchSuggestionAdapter
        }
    }

    private fun setupSearch() {
        // Back button
        btnBack.setOnClickListener {
            finish()
        }

        // Clear button
        btnClearSearch.setOnClickListener {
            etSearch.text.clear()
            searchSuggestionAdapter.clearSuggestions()
            showEmptyState(true, "Start typing to search novels...")
        }

        // Text change listener
        etSearch.addTextChangedListener(object : TextWatcher {
            override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) {}

            override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) {
                btnClearSearch.visibility = if (s.isNullOrEmpty()) View.GONE else View.VISIBLE

                searchRunnable?.let { searchHandler.removeCallbacks(it) }

                if (!s.isNullOrEmpty() && s.length >= 2) {
                    searchRunnable = Runnable {
                        performSearch(s.toString())
                    }
                    searchHandler.postDelayed(searchRunnable!!, SEARCH_DELAY)
                } else if (s.isNullOrEmpty()) {
                    searchSuggestionAdapter.clearSuggestions()
                    showEmptyState(true, "Start typing to search novels...")
                }
            }

            override fun afterTextChanged(s: Editable?) {}
        })

        // Handle search action from keyboard
        etSearch.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_SEARCH) {
                val query = etSearch.text.toString()
                if (query.isNotEmpty()) {
                    performSearch(query)
                    val imm = getSystemService(INPUT_METHOD_SERVICE) as InputMethodManager
                    imm.hideSoftInputFromWindow(etSearch.windowToken, 0)
                }
                true
            } else {
                false
            }
        }

        // Initial empty state
        showEmptyState(true, "Start typing to search novels...")
    }

    private fun performSearch(query: String) {
        if (query.isEmpty() || query.length < 2) return

        Log.d("SearchActivity", "Searching for: $query")

        showEmptyState(false)

        novelApiService.getNovels(
            search = query,
            pageSize = 20
        ).enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                if (response.isSuccessful) {
                    val novels = response.body()?.Data ?: emptyList()
                    Log.d("SearchActivity", "Found ${novels.size} novels")

                    if (novels.isEmpty()) {
                        showEmptyState(true, "No novels found for \"$query\"")
                    } else {
                        searchSuggestionAdapter.updateNovels(novels)
                    }
                } else {
                    Log.e("SearchActivity", "Search failed: ${response.code()}")
                    showEmptyState(true, "Search failed. Please try again.")
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                Log.e("SearchActivity", "Search error: ${t.message}", t)
                showEmptyState(true, "Connection error. Please try again.")
            }
        })
    }

    private fun showEmptyState(show: Boolean, message: String = "") {
        if (show) {
            tvEmptyState.text = message
            tvEmptyState.visibility = View.VISIBLE
            rvSearchResults.visibility = View.GONE
        } else {
            tvEmptyState.visibility = View.GONE
            rvSearchResults.visibility = View.VISIBLE
        }
    }

    private fun navigateToNovelDetail(novel: Novel) {
        val intent = Intent(this, NovelDetailActivity::class.java)
        intent.putExtra("NOVEL_ID", novel.Id)
        startActivity(intent)
    }

    override fun onDestroy() {
        super.onDestroy()
        searchRunnable?.let { searchHandler.removeCallbacks(it) }
    }
}