package com.sinhvien.webnovelapp.admin

import android.os.Bundle
import android.view.MenuItem
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.GenreApiService
import com.sinhvien.webnovelapp.models.Genre
import com.sinhvien.webnovelapp.models.GenreResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.sinhvien.webnovelapp.R

class GenreManagementActivity : AppCompatActivity() {
    private lateinit var apiService: GenreApiService
    private lateinit var genreAdapter: GenreAdapter

    // UI Components
    private lateinit var etSearch: EditText
    private lateinit var tvStatus: TextView
    private lateinit var tvCount: TextView
    private lateinit var rvGenres: RecyclerView
    private lateinit var paginationControls: LinearLayout
    private lateinit var tvPageInfo: TextView

    // Buttons
    private lateinit var btnSearch: Button
    private lateinit var btnLoadAllGenres: Button
    private lateinit var btnLoadActiveOnly: Button
    private lateinit var btnClearResults: Button
    private lateinit var btnPrevPage: Button
    private lateinit var btnNextPage: Button

    // Pagination
    private var allGenres = listOf<Genre>()
    private var currentPage = 1
    private val itemsPerPage = 10

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_genre_management)

        // Set up toolbar
        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = "Genre Management"
        } catch (e: Exception) {
            // Toolbar setup failed, continue without it
        }

        // Initialize API service
        apiService = ApiClient.getClient().create(GenreApiService::class.java)

        // Initialize views
        initializeViews()

        // Set up RecyclerView
        setupRecyclerView()

        // Set up click listeners
        setupClickListeners()

        // Auto-load active genres on startup
        loadGenres("", true)
    }

    private fun initializeViews() {
        // EditText
        etSearch = findViewById(R.id.etSearch)

        // TextViews
        tvStatus = findViewById(R.id.tvStatus)
        tvCount = findViewById(R.id.tvCount)
        tvPageInfo = findViewById(R.id.tvPageInfo)

        // RecyclerView
        rvGenres = findViewById(R.id.rvGenres)

        // Pagination controls
        paginationControls = findViewById(R.id.paginationControls)

        // Buttons
        btnSearch = findViewById(R.id.btnSearch)
        btnLoadAllGenres = findViewById(R.id.btnLoadAllGenres)
        btnLoadActiveOnly = findViewById(R.id.btnLoadActiveOnly)
        btnClearResults = findViewById(R.id.btnClearResults)
        btnPrevPage = findViewById(R.id.btnPrevPage)
        btnNextPage = findViewById(R.id.btnNextPage)
    }

    private fun setupRecyclerView() {
        genreAdapter = GenreAdapter()
        rvGenres.apply {
            layoutManager = LinearLayoutManager(this@GenreManagementActivity)
            adapter = genreAdapter
            setHasFixedSize(true)
        }
    }

    private fun setupClickListeners() {
        // Search button - handles both text search and ID lookup
        btnSearch.setOnClickListener {
            val searchInput = etSearch.text.toString().trim()

            if (searchInput.isEmpty()) {
                // If empty, load active genres
                loadGenres("", true)
            } else {
                // Check if input is a number (ID search)
                val genreId = searchInput.toIntOrNull()
                if (genreId != null) {
                    // It's a number, search by ID
                    loadSpecificGenre(genreId)
                } else {
                    // It's text, search by name/description
                    loadGenres(searchInput, true)
                }
            }
        }

        // Load all genres (active and inactive)
        btnLoadAllGenres.setOnClickListener {
            etSearch.setText("")
            loadGenres("", false)
        }

        // Load only active genres
        btnLoadActiveOnly.setOnClickListener {
            etSearch.setText("")
            loadGenres("", true)
        }

        // Clear results
        btnClearResults.setOnClickListener {
            clearResults()
        }

        // Pagination controls
        btnPrevPage.setOnClickListener {
            if (currentPage > 1) {
                currentPage--
                displayPage()
            }
        }

        btnNextPage.setOnClickListener {
            val totalPages = getTotalPages()
            if (currentPage < totalPages) {
                currentPage++
                displayPage()
            }
        }
    }

    private fun loadGenres(search: String = "", activeOnly: Boolean = true) {
        updateStatus("Loading genres...", "#FF9800")

        val call = apiService.getGenres(search, activeOnly)
        call.enqueue(object : Callback<GenreResponse> {
            override fun onResponse(call: Call<GenreResponse>, response: Response<GenreResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val genreResponse = response.body()!!
                    if (genreResponse.Success) {
                        allGenres = genreResponse.Data
                        currentPage = 1
                        displayPage()

                        val statusText = if (search.isNotEmpty()) {
                            "Search results for '$search'"
                        } else if (activeOnly) {
                            "Active genres"
                        } else {
                            "All genres"
                        }
                        updateStatus(statusText, "#4CAF50")
                        tvCount.text = "${genreResponse.Count} total"

                        // Show pagination if needed
                        paginationControls.visibility = if (allGenres.size > itemsPerPage) View.VISIBLE else View.GONE
                    } else {
                        showError("API returned success = false")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<GenreResponse>, t: Throwable) {
                showError("Network Error: ${t.message}")
                Toast.makeText(this@GenreManagementActivity, "Failed to load genres", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun loadSpecificGenre(genreId: Int) {
        updateStatus("Loading genre #$genreId...", "#FF9800")

        // Use the list endpoint with empty search to avoid the single object issue
        val call = apiService.getGenres("", false)
        call.enqueue(object : Callback<GenreResponse> {
            override fun onResponse(call: Call<GenreResponse>, response: Response<GenreResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val genreResponse = response.body()!!
                    if (genreResponse.Success) {
                        // Filter to find the specific genre by ID
                        val genre = genreResponse.Data.find { it.Id == genreId }

                        if (genre != null) {
                            allGenres = listOf(genre)
                            currentPage = 1
                            displayPage()
                            updateStatus("Genre #$genreId loaded", "#4CAF50")
                            tvCount.text = "1 total"
                            paginationControls.visibility = View.GONE
                        } else {
                            showError("Genre ID $genreId not found")
                        }
                    } else {
                        showError("API returned success = false")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<GenreResponse>, t: Throwable) {
                showError("Network Error: ${t.message}")
                Toast.makeText(this@GenreManagementActivity, "Failed to load genre", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun displayPage() {
        if (allGenres.isEmpty()) {
            genreAdapter.submitList(emptyList())
            return
        }

        val startIndex = (currentPage - 1) * itemsPerPage
        val endIndex = minOf(startIndex + itemsPerPage, allGenres.size)
        val pageGenres = allGenres.subList(startIndex, endIndex)

        genreAdapter.submitList(pageGenres)

        // Update pagination UI
        val totalPages = getTotalPages()
        tvPageInfo.text = "Page $currentPage of $totalPages"
        btnPrevPage.isEnabled = currentPage > 1
        btnNextPage.isEnabled = currentPage < totalPages
    }

    private fun getTotalPages(): Int {
        return if (allGenres.isEmpty()) 1 else ((allGenres.size - 1) / itemsPerPage) + 1
    }

    private fun updateStatus(message: String, color: String) {
        tvStatus.text = message
        try {
            tvStatus.setTextColor(android.graphics.Color.parseColor(color))
        } catch (e: IllegalArgumentException) {
            // Fallback to default color if parsing fails
        }
    }

    private fun showError(errorMessage: String) {
        updateStatus("Error: $errorMessage", "#f44336")
        tvCount.text = ""
        allGenres = emptyList()
        genreAdapter.submitList(emptyList())
        paginationControls.visibility = View.GONE
        Toast.makeText(this, errorMessage, Toast.LENGTH_LONG).show()
    }

    private fun clearResults() {
        etSearch.setText("")
        loadGenres("", true)
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