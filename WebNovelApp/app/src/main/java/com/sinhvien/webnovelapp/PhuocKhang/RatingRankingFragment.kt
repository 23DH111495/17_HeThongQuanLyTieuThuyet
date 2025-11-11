package com.sinhvien.webnovelapp.PhuocKhang // ĐÃ SỬA: Dùng tên package viết hoa 'PhuocKhang'

import android.os.Bundle
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import androidx.fragment.app.Fragment
import androidx.lifecycle.lifecycleScope
import androidx.recyclerview.widget.LinearLayoutManager
import com.bumptech.glide.Glide
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.RankingApiService
import com.sinhvien.webnovelapp.databinding.FragmentRankingContentBinding
import com.sinhvien.webnovelapp.adapters.NovelRankingAdapter
import com.sinhvien.webnovelapp.models.NovelRankingDto
import kotlinx.coroutines.launch
import java.util.Locale

class RatingRankingFragment : Fragment() {

    private var _binding: FragmentRankingContentBinding? = null
    private val binding get() = _binding!!

    private lateinit var novelAdapter: NovelRankingAdapter

    private val apiService: RankingApiService by lazy {
        // *** ĐÃ SỬA LỖI: ***
        // Trong Fragment, phải dùng requireContext() để lấy Context,
        // không thể dùng 'this'
        ApiClient.getClient().create(RankingApiService::class.java)
    }

    // (ĐÃ XÓA) Hàm getFullCoverUrl (vì chúng ta dùng ID để tải ảnh)
    /* private fun getFullCoverUrl(path: String?): String? { ... } */

    override fun onCreateView(inflater: LayoutInflater, container: ViewGroup?, savedInstanceState: Bundle?): View {
        _binding = FragmentRankingContentBinding.inflate(inflater, container, false)
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        setupRecyclerView()
        loadRankingData("rating", "all", 1, 50)
    }

    private fun setupRecyclerView() {
        novelAdapter = NovelRankingAdapter { novelDto ->
            Toast.makeText(context, "Clicked: ${novelDto.Title}", Toast.LENGTH_SHORT).show()
        }
        binding.recyclerViewRanking.layoutManager = LinearLayoutManager(context)
        binding.recyclerViewRanking.adapter = novelAdapter
    }

    private fun loadRankingData(type: String, period: String, page: Int, pageSize: Int) {
        lifecycleScope.launch {
            try {
                val response = apiService.getRanking(type, period, page, pageSize)
                if (response.isSuccessful) {
                    val rankingResponse = response.body()
                    if (rankingResponse != null && rankingResponse.Success) {
                        val fullList = rankingResponse.Data
                        val top1 = fullList.getOrNull(0)
                        val top2 = fullList.getOrNull(1)
                        val top3 = fullList.getOrNull(2)
                        val listForAdapter = fullList.drop(3)
                        updatePodium(top1, top2, top3)
                        novelAdapter.submitList(listForAdapter)
                    } else {
                        Log.e("RankingFragment", "API Response Error: ${rankingResponse?.Message}")
                    }
                } else {
                    Log.e("RankingFragment", "API Error: ${response.code()}")
                }
            } catch (e: Exception) {
                Log.e("RankingFragment", "Network Error: ${e.message}", e)
            }
        }
    }

    // --- HÀM UPDATE PODIUM (ĐÃ SỬA: Tải ảnh bằng Novel ID) ---
    private fun updatePodium(top1: NovelRankingDto?, top2: NovelRankingDto?, top3: NovelRankingDto?) {

        val baseUrl = ApiClient.getBaseUrl() // Lấy base URL

        // Hạng 1
        if (top1 != null) {
            // (SỬA) Tạo URL tải ảnh trực tiếp bằng ID
            val coverUrl = "${baseUrl}api/novels/${top1.Id}/cover"

            binding.podiumLayout.tvTitleRank1.text = top1.Title
            binding.podiumLayout.tvViewsRank1.text = formatK(top1.ViewCount) + " Xem"
            Log.e("RankingFragment_Glide", "[Rating] Top 1 FINAL URL: $coverUrl")

            Glide.with(this)
                .load(coverUrl)
                .placeholder(R.drawable.placeholder_cover)
                .error(R.drawable.placeholder_cover)
                .into(binding.podiumLayout.ivCoverRank1)
            binding.podiumLayout.layoutRank1.visibility = View.VISIBLE
        } else {
            binding.podiumLayout.layoutRank1.visibility = View.GONE
        }

        // Hạng 2
        if (top2 != null) {
            val coverUrl = "${baseUrl}api/novels/${top2.Id}/cover"

            binding.podiumLayout.tvTitleRank2.text = top2.Title
            binding.podiumLayout.tvViewsRank2.text = formatK(top2.ViewCount) + " Xem"
            Log.e("RankingFragment_Glide", "[Rating] Top 2 FINAL URL: $coverUrl")

            Glide.with(this)
                .load(coverUrl)
                .placeholder(R.drawable.placeholder_cover)
                .error(R.drawable.placeholder_cover)
                .into(binding.podiumLayout.ivCoverRank2)
            binding.podiumLayout.layoutRank2.visibility = View.VISIBLE
        } else {
            binding.podiumLayout.layoutRank2.visibility = View.GONE
        }

        // Hạng 3
        if (top3 != null) {
            val coverUrl = "${baseUrl}api/novels/${top3.Id}/cover"

            binding.podiumLayout.tvTitleRank3.text = top3.Title
            binding.podiumLayout.tvViewsRank3.text = formatK(top3.ViewCount) + " Xem"
            Log.e("RankingFragment_Glide", "[Rating] Top 3 FINAL URL: $coverUrl")

            Glide.with(this)
                .load(coverUrl)
                .placeholder(R.drawable.placeholder_cover)
                .error(R.drawable.placeholder_cover)
                .into(binding.podiumLayout.ivCoverRank3)
            binding.podiumLayout.layoutRank3.visibility = View.VISIBLE
        } else {
            binding.podiumLayout.layoutRank3.visibility = View.GONE
        }
    }

    private fun formatK(count: Long): String {
        return when {
            count >= 1_000_000 -> String.format(Locale.US, "%.1fM", count / 1_000_000.0)
            count >= 1_000 -> String.format(Locale.US, "%.1fK", count / 1_000.0)
            else -> count.toString()
        }
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }
}