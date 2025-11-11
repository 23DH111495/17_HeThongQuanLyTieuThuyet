package com.sinhvien.webnovelapp.QuocDuy

import android.os.Bundle
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import com.sinhvien.webnovelapp.adapters.RatingAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.RatingApi
import com.sinhvien.webnovelapp.databinding.ActivityRatingBinding
import com.sinhvien.webnovelapp.models.Rating
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class RatingActivity : AppCompatActivity() {

    private lateinit var binding: ActivityRatingBinding
    private lateinit var api: RatingApi
    private lateinit var adapter: RatingAdapter

    private var novelId: Int = 0
    private var readerId: Int = 0 // lấy từ login/SharedPreferences

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityRatingBinding.inflate(layoutInflater)
        setContentView(binding.root)

        api = ApiClient.getClient().create(RatingApi::class.java)

        novelId = intent.getIntExtra("novelId", 1)
        readerId = intent.getIntExtra("readerId", 1)

        setupRecyclerView()
        loadRatings()
        loadAverage()

        binding.btnSubmit.setOnClickListener {
            val value = binding.ratingBar.rating.toInt()
            val rating = Rating(readerId = readerId, novelId = novelId, ratingValue = value)
            api.createOrUpdateRating(rating).enqueue(object : Callback<Map<String, Any>> {
                override fun onResponse(call: Call<Map<String, Any>>, response: Response<Map<String, Any>>) {
                    Toast.makeText(this@RatingActivity, "Rating submitted", Toast.LENGTH_SHORT).show()
                    loadRatings()
                    loadAverage()
                }

                override fun onFailure(call: Call<Map<String, Any>>, t: Throwable) {
                    Toast.makeText(this@RatingActivity, "Error: ${t.message}", Toast.LENGTH_SHORT).show()
                }
            })
        }
    }

    private fun setupRecyclerView() {
        adapter = RatingAdapter(listOf())
        binding.rvRatings.layoutManager = LinearLayoutManager(this)
        binding.rvRatings.adapter = adapter
    }

    private fun loadRatings() {
        api.getRatingsByNovel(novelId).enqueue(object : Callback<List<Rating>> {
            override fun onResponse(call: Call<List<Rating>>, response: Response<List<Rating>>) {
                response.body()?.let { adapter.updateList(it) }
            }

            override fun onFailure(call: Call<List<Rating>>, t: Throwable) {
                Toast.makeText(this@RatingActivity, "Load failed: ${t.message}", Toast.LENGTH_SHORT).show()
            }
        })
    }

    private fun loadAverage() {
        api.getAverageByNovel(novelId).enqueue(object : Callback<Map<String, Any>> {
            override fun onResponse(call: Call<Map<String, Any>>, response: Response<Map<String, Any>>) {
                val data = response.body()
                val avg = data?.get("averageRating") ?: 0
                val total = data?.get("totalRatings") ?: 0
                binding.tvAverage.text = "Average: $avg ($total ratings)"
            }

            override fun onFailure(call: Call<Map<String, Any>>, t: Throwable) {
                Toast.makeText(this@RatingActivity, "Failed: ${t.message}", Toast.LENGTH_SHORT).show()
            }
        })
    }
}
