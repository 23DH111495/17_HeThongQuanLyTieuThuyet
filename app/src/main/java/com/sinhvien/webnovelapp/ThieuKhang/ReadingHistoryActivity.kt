package com.sinhvien.webnovelapp.ThieuKhang

import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.view.View
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.Toast
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import com.sinhvien.webnovelapp.ThieuKhang.ReadingHistoryRepository
import com.sinhvien.webnovelapp.activities.ChapterReaderActivity
import com.sinhvien.webnovelapp.ThieuKhang.ReadingHistoryAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.models.ReadingHistoryItem
import com.sinhvien.webnovelapp.models.Resource
import com.sinhvien.webnovelapp.viewmodels.ReadingHistoryViewModel
import com.sinhvien.webnovelapp.viewmodels.ReadingHistoryViewModelFactory
import kotlinx.coroutines.launch
import android.widget.ImageButton

class ReadingHistoryActivity : AppCompatActivity() {

    private lateinit var rvReadingHistory: RecyclerView
    private lateinit var progressBar: ProgressBar
    private lateinit var emptyStateLayout: LinearLayout
    private lateinit var adapter: ReadingHistoryAdapter
    private lateinit var viewModel: ReadingHistoryViewModel
    private lateinit var tokenManager: TokenManager

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_reading_history)

        // Remove or hide the action bar since we have custom back button
        supportActionBar?.hide()

        rvReadingHistory = findViewById(R.id.rvReadingHistory)
        progressBar = findViewById(R.id.progressBar)
        emptyStateLayout = findViewById(R.id.emptyStateLayout)

        // Add back button listener
        val btnBack = findViewById<ImageButton>(R.id.btnBack)
        btnBack.setOnClickListener {
            finish() // or onBackPressed()
        }

        tokenManager = TokenManager.getInstance(this)
        val apiService = ApiClient.getReadingHistoryService()
        val repository = ReadingHistoryRepository(apiService, tokenManager)
        val factory = ReadingHistoryViewModelFactory(repository)
        viewModel = ViewModelProvider(this, factory)[ReadingHistoryViewModel::class.java]

        setupRecyclerView()
        observeViewModel()

        // Load data only once in onCreate
        viewModel.loadReadingHistory(refresh = true)
    }

    private fun setupRecyclerView() {
        adapter = ReadingHistoryAdapter(
            onItemClick = { item ->
                openChapterReader(item)
            },
            onDeleteClick = { item ->
                showDeleteConfirmation(item)
            }
        )

        rvReadingHistory.layoutManager = LinearLayoutManager(this)
        rvReadingHistory.adapter = adapter
    }

    private fun observeViewModel() {
        viewModel.readingHistory.observe(this) { resource ->
            when (resource) {
                is Resource.Loading -> {
                    Log.d("ReadingHistory", "Loading...")
                    showLoading(true)
                }
                is Resource.Success -> {
                    showLoading(false)
                    resource.data?.let { historyList ->
                        Log.d("ReadingHistory", "Success: ${historyList.size} items")
                        historyList.forEach { item ->
                            Log.d("ReadingHistory", "Novel: ${item.novelTitle}, ID: ${item.novelId}")
                        }

                        if (historyList.isEmpty()) {
                            showEmptyState(true)
                        } else {
                            showEmptyState(false)
                            adapter.submitList(historyList.toList()) // Create new list to trigger DiffUtil
                        }
                    } ?: run {
                        Log.d("ReadingHistory", "Success but data is null")
                        showEmptyState(true)
                    }
                }
                is Resource.Error -> {
                    showLoading(false)
                    Log.e("ReadingHistory", "Error: ${resource.message}")
                    Toast.makeText(this, resource.message ?: "Error loading history", Toast.LENGTH_SHORT).show()
                }
            }
        }
    }

    private fun showLoading(show: Boolean) {
        progressBar.visibility = if (show) View.VISIBLE else View.GONE
        rvReadingHistory.visibility = if (show) View.GONE else View.VISIBLE
    }

    private fun showEmptyState(show: Boolean) {
        emptyStateLayout.visibility = if (show) View.VISIBLE else View.GONE
        rvReadingHistory.visibility = if (show) View.GONE else View.VISIBLE
    }

    private fun openChapterReader(item: ReadingHistoryItem) {
        val intent = Intent(this, ChapterReaderActivity::class.java).apply {
            putExtra("NOVEL_ID", item.novelId)
            putExtra("CHAPTER_NUMBER", item.lastReadChapterNumber)
        }
        startActivity(intent)
    }

    private fun showDeleteConfirmation(item: ReadingHistoryItem) {
        AlertDialog.Builder(this, R.style.DarkAlertDialog)
            .setTitle("Remove from History")
            .setMessage("Remove \"${item.novelTitle}\" from your reading history?")
            .setPositiveButton("Remove") { _, _ ->
                deleteFromHistory(item)
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun deleteFromHistory(item: ReadingHistoryItem) {
        lifecycleScope.launch {
            viewModel.deleteFromHistory(item.novelId) { success ->
                if (success) {
                    Toast.makeText(
                        this@ReadingHistoryActivity,
                        "Removed from history",
                        Toast.LENGTH_SHORT
                    ).show()
                } else {
                    Toast.makeText(
                        this@ReadingHistoryActivity,
                        "Failed to remove",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            }
        }
    }

//    override fun onSupportNavigateUp(): Boolean {
//        onBackPressed()
//        return true
//    }

    override fun onResume() {
        super.onResume()
        // Refresh data when returning to this screen
        viewModel.loadReadingHistory(refresh = true)
    }
}