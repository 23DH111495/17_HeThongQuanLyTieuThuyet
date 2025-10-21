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
import com.sinhvien.webnovelapp.api.TagApiService
import com.sinhvien.webnovelapp.models.Tag
import com.sinhvien.webnovelapp.models.TagResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.sinhvien.webnovelapp.R

class TagManagementActivity : AppCompatActivity() {
    private lateinit var apiService: TagApiService
    private lateinit var tagAdapter: TagAdapter

    // UI Components
    private lateinit var etSearch: EditText
    private lateinit var tvStatus: TextView
    private lateinit var tvCount: TextView
    private lateinit var rvTags: RecyclerView
    private lateinit var paginationControls: LinearLayout
    private lateinit var tvPageInfo: TextView

    // Buttons
    private lateinit var btnSearch: Button
    private lateinit var btnLoadAllTags: Button
    private lateinit var btnLoadActiveOnly: Button
    private lateinit var btnClearResults: Button
    private lateinit var btnPrevPage: Button
    private lateinit var btnNextPage: Button

    // Pagination
    private var allTags = listOf<Tag>()
    private var currentPage = 1
    private val itemsPerPage = 10

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_tag_management)

        // Set up toolbar
        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = "Tag Management"
        } catch (e: Exception) {
            // Toolbar setup failed, continue without it
        }

        // Initialize API service
        apiService = ApiClient.getClient().create(TagApiService::class.java)

        // Initialize views
        initializeViews()

        // Set up RecyclerView
        setupRecyclerView()

        // Set up click listeners
        setupClickListeners()

        // Auto-load active tags on startup
        loadTags("", true)
    }

    private fun initializeViews() {
        // EditText
        etSearch = findViewById(R.id.etSearch)

        // TextViews
        tvStatus = findViewById(R.id.tvStatus)
        tvCount = findViewById(R.id.tvCount)
        tvPageInfo = findViewById(R.id.tvPageInfo)

        // RecyclerView
        rvTags = findViewById(R.id.rvTags)

        // Pagination controls
        paginationControls = findViewById(R.id.paginationControls)

        // Buttons
        btnSearch = findViewById(R.id.btnSearch)
        btnLoadAllTags = findViewById(R.id.btnLoadAllTags)
        btnLoadActiveOnly = findViewById(R.id.btnLoadActiveOnly)
        btnClearResults = findViewById(R.id.btnClearResults)
        btnPrevPage = findViewById(R.id.btnPrevPage)
        btnNextPage = findViewById(R.id.btnNextPage)
    }

    private fun setupRecyclerView() {
        tagAdapter = TagAdapter()
        rvTags.apply {
            layoutManager = LinearLayoutManager(this@TagManagementActivity)
            adapter = tagAdapter
            setHasFixedSize(true)
        }
    }

    private fun setupClickListeners() {
        // Search button - handles both text search and ID lookup
        btnSearch.setOnClickListener {
            val searchInput = etSearch.text.toString().trim()

            if (searchInput.isEmpty()) {
                // If empty, load active tags
                loadTags("", true)
            } else {
                // Check if input is a number (ID search)
                val tagId = searchInput.toIntOrNull()
                if (tagId != null) {
                    // It's a number, search by ID
                    loadSpecificTag(tagId)
                } else {
                    // It's text, search by name/description
                    loadTags(searchInput, true)
                }
            }
        }

        // Load all tags (active and inactive)
        btnLoadAllTags.setOnClickListener {
            etSearch.setText("")
            loadTags("", false)
        }

        // Load only active tags
        btnLoadActiveOnly.setOnClickListener {
            etSearch.setText("")
            loadTags("", true)
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

    private fun loadTags(search: String = "", activeOnly: Boolean = true) {
        updateStatus("Loading tags...", "#FF9800")

        val call = apiService.getTags(search, activeOnly)
        call.enqueue(object : Callback<TagResponse> {
            override fun onResponse(call: Call<TagResponse>, response: Response<TagResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val tagResponse = response.body()!!
                    if (tagResponse.Success) {
                        allTags = tagResponse.Data
                        currentPage = 1
                        displayPage()

                        val statusText = if (search.isNotEmpty()) {
                            "Search results for '$search'"
                        } else if (activeOnly) {
                            "Active tags"
                        } else {
                            "All tags"
                        }
                        updateStatus(statusText, "#4CAF50")
                        tvCount.text = "${tagResponse.Count} total"

                        // Show pagination if needed
                        paginationControls.visibility = if (allTags.size > itemsPerPage) View.VISIBLE else View.GONE
                    } else {
                        showError("API returned success = false")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<TagResponse>, t: Throwable) {
                showError("Network Error: ${t.message}")
                Toast.makeText(this@TagManagementActivity, "Failed to load tags", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun loadSpecificTag(tagId: Int) {
        updateStatus("Loading tag #$tagId...", "#FF9800")

        // Use the list endpoint with empty search to avoid the single object issue
        val call = apiService.getTags("", false)
        call.enqueue(object : Callback<TagResponse> {
            override fun onResponse(call: Call<TagResponse>, response: Response<TagResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val tagResponse = response.body()!!
                    if (tagResponse.Success) {
                        // Filter to find the specific tag by ID
                        val tag = tagResponse.Data.find { it.Id == tagId }

                        if (tag != null) {
                            allTags = listOf(tag)
                            currentPage = 1
                            displayPage()
                            updateStatus("Tag #$tagId loaded", "#4CAF50")
                            tvCount.text = "1 total"
                            paginationControls.visibility = View.GONE
                        } else {
                            showError("Tag ID $tagId not found")
                        }
                    } else {
                        showError("API returned success = false")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<TagResponse>, t: Throwable) {
                showError("Network Error: ${t.message}")
                Toast.makeText(this@TagManagementActivity, "Failed to load tag", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun displayPage() {
        if (allTags.isEmpty()) {
            tagAdapter.submitList(emptyList())
            return
        }

        val startIndex = (currentPage - 1) * itemsPerPage
        val endIndex = minOf(startIndex + itemsPerPage, allTags.size)
        val pageTags = allTags.subList(startIndex, endIndex)

        tagAdapter.submitList(pageTags)

        // Update pagination UI
        val totalPages = getTotalPages()
        tvPageInfo.text = "Page $currentPage of $totalPages"
        btnPrevPage.isEnabled = currentPage > 1
        btnNextPage.isEnabled = currentPage < totalPages
    }

    private fun getTotalPages(): Int {
        return if (allTags.isEmpty()) 1 else ((allTags.size - 1) / itemsPerPage) + 1
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
        allTags = emptyList()
        tagAdapter.submitList(emptyList())
        paginationControls.visibility = View.GONE
        Toast.makeText(this, errorMessage, Toast.LENGTH_LONG).show()
    }

    private fun clearResults() {
        etSearch.setText("")
        loadTags("", true)
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