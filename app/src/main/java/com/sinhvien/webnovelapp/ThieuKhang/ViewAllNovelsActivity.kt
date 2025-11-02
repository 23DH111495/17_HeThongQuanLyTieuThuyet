package com.sinhvien.webnovelapp.activities

import android.content.Intent
import android.os.Bundle
import android.view.MenuItem
import android.view.View
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.widget.Toolbar
import androidx.recyclerview.widget.GridLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.adapters.OngoingNovelsAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.NovelResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ViewAllNovelsActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var rvNovels: RecyclerView
    private lateinit var pbLoading: ProgressBar
    private lateinit var tvEmpty: TextView
    private lateinit var novelAdapter: OngoingNovelsAdapter

    private var currentPage = 1
    private var isLoading = false
    private var hasMorePages = true
    private lateinit var listType: String

    companion object {
        const val EXTRA_LIST_TYPE = "list_type"
        const val TYPE_ONGOING = "ongoing"
        const val TYPE_COMPLETED = "completed"
        const val TYPE_RECOMMEND = "recommend"
        const val TYPE_NEW_RELEASES = "new_releases"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_view_all_novels)

        listType = intent.getStringExtra(EXTRA_LIST_TYPE) ?: TYPE_ONGOING

        setupToolbar()
        initializeViews()
        setupRecyclerView()
        loadNovels()
    }

    private fun setupToolbar() {
        val toolbar = findViewById<Toolbar>(R.id.toolbar)
        setSupportActionBar(toolbar)
        supportActionBar?.setDisplayHomeAsUpEnabled(true)

        val title = when (listType) {
            TYPE_ONGOING -> "Ongoing Novels"
            TYPE_COMPLETED -> "Completed Novels"
            TYPE_RECOMMEND -> "Recommend Novels"
            TYPE_NEW_RELEASES -> "Recently Added"
            else -> "Novels"
        }
        supportActionBar?.title = title
    }

    private fun initializeViews() {
        apiService = ApiClient.getClient().create(NovelApiService::class.java)
        rvNovels = findViewById(R.id.rvNovels)
        pbLoading = findViewById(R.id.pbLoading)
        tvEmpty = findViewById(R.id.tvEmpty)
    }

    private fun setupRecyclerView() {
        novelAdapter = OngoingNovelsAdapter { novel ->
            val intent = Intent(this, NovelDetailActivity::class.java)
            intent.putExtra("NOVEL_ID", novel.Id)
            startActivity(intent)
        }

        rvNovels.apply {
            layoutManager = GridLayoutManager(this@ViewAllNovelsActivity, 3)
            adapter = novelAdapter

            // Pagination on scroll
            addOnScrollListener(object : RecyclerView.OnScrollListener() {
                override fun onScrolled(recyclerView: RecyclerView, dx: Int, dy: Int) {
                    super.onScrolled(recyclerView, dx, dy)

                    val layoutManager = recyclerView.layoutManager as GridLayoutManager
                    val visibleItemCount = layoutManager.childCount
                    val totalItemCount = layoutManager.itemCount
                    val firstVisibleItemPosition = layoutManager.findFirstVisibleItemPosition()

                    if (!isLoading && hasMorePages) {
                        if ((visibleItemCount + firstVisibleItemPosition) >= totalItemCount
                            && firstVisibleItemPosition >= 0) {
                            currentPage++
                            loadNovels()
                        }
                    }
                }
            })
        }
    }

    private fun loadNovels() {
        if (isLoading) return

        isLoading = true
        pbLoading.visibility = View.VISIBLE

        val call = when (listType) {
            TYPE_ONGOING -> apiService.getOngoingNovels(
                page = currentPage,
                pageSize = 30,
                sortBy = "popular"
            )
            TYPE_COMPLETED -> apiService.getCompletedNovels(
                page = currentPage,
                pageSize = 30,
                sortBy = "popular"
            )
            TYPE_RECOMMEND -> apiService.getFeaturedNovelsList(
                page = currentPage,
                pageSize = 30,
                sortBy = "popular"
            )
            TYPE_NEW_RELEASES -> apiService.getNewlyReleasedNovels(count = 30)
            else -> apiService.getOngoingNovels(
                page = currentPage,
                pageSize = 30,
                sortBy = "popular"
            )
        }

        call.enqueue(object : Callback<NovelResponse> {
            override fun onResponse(call: Call<NovelResponse>, response: Response<NovelResponse>) {
                isLoading = false
                pbLoading.visibility = View.GONE

                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success && novelResponse.Data.isNotEmpty()) {
                        val currentList = novelAdapter.currentList.toMutableList()
                        currentList.addAll(novelResponse.Data)
                        novelAdapter.submitList(currentList)

                        // Check if there are more pages
                        if (listType == TYPE_NEW_RELEASES) {
                            // For new releases, no more pages after first load
                            hasMorePages = false
                        } else {
                            hasMorePages = novelResponse.Data.size >= 30
                        }

                        tvEmpty.visibility = View.GONE
                    } else {
                        hasMorePages = false
                        if (currentPage == 1) {
                            tvEmpty.visibility = View.VISIBLE
                        }
                    }
                } else {
                    Toast.makeText(
                        this@ViewAllNovelsActivity,
                        "Failed to load novels",
                        Toast.LENGTH_SHORT
                    ).show()
                }
            }

            override fun onFailure(call: Call<NovelResponse>, t: Throwable) {
                isLoading = false
                pbLoading.visibility = View.GONE
                Toast.makeText(
                    this@ViewAllNovelsActivity,
                    "Error: ${t.message}",
                    Toast.LENGTH_SHORT
                ).show()
            }

        })
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        if (item.itemId == android.R.id.home) {
            onBackPressed()
            return true
        }
        return super.onOptionsItemSelected(item)
    }
}