package com.sinhvien.webnovelapp.PhuocKhang // (Nhớ đổi package của bạn)

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.api.ApiClient // <--- (THÊM IMPORT)
import com.sinhvien.webnovelapp.models.Novel
import java.util.Locale

// Dùng ListAdapter để tối ưu hiệu suất
class RankingAdapter(
    private val onItemClick: (Novel) -> Unit
) : ListAdapter<Novel, RankingAdapter.RankingViewHolder>(NovelDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RankingViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_ranking_list, parent, false)
        return RankingViewHolder(view, onItemClick)
    }

    override fun onBindViewHolder(holder: RankingViewHolder, position: Int) {
        val novel = getItem(position)
        holder.bind(novel, position + 4)
    }

    class RankingViewHolder(
        itemView: View,
        private val onItemClick: (Novel) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        // Ánh xạ tất cả ID từ item_ranking_list.xml
        private val tvRankNumber: TextView = itemView.findViewById(R.id.tvRankNumber)
        private val ivNovelCover: ImageView = itemView.findViewById(R.id.ivNovelCover)
        private val tvNovelTitle: TextView = itemView.findViewById(R.id.tvNovelTitle)
        private val tvNovelAuthor: TextView = itemView.findViewById(R.id.tvNovelAuthor)
        private val tvNovelCategory: TextView = itemView.findViewById(R.id.tvNovelCategory)
        private val tvNovelViews: TextView = itemView.findViewById(R.id.tvNovelViews)
        private val tvNovelRating: TextView = itemView.findViewById(R.id.tvNovelRating)
        private val tvNovelBookmarks: TextView = itemView.findViewById(R.id.tvNovelBookmarks)

        // (THÊM HÀM) Lấy URL đầy đủ
        private fun getFullCoverUrl(path: String?): String? {
            if (path.isNullOrEmpty()) return null
            val baseUrl = ApiClient.getBaseUrl()

            // Nối BASE_URL với đường dẫn, loại bỏ dấu '/' thừa ở đầu nếu cần
            return if (path.startsWith("/")) {
                baseUrl + path.removePrefix("/")
            } else {
                baseUrl + path
            }
        }

        fun bind(novel: Novel, rank: Int) {

            // 1. Gán dữ liệu text (Giữ nguyên)
            tvRankNumber.text = rank.toString()
            tvNovelTitle.text = novel.Title
            tvNovelAuthor.text = novel.AuthorName
            tvNovelCategory.text = novel.Genres.joinToString(", ")

            // 2. Gán Stats (Giữ nguyên)
            tvNovelViews.text = formatK(novel.ViewCount)
            tvNovelRating.text = String.format(Locale.US, "%.1f", novel.AverageRating)
            tvNovelBookmarks.text = formatK(novel.BookmarkCount)

            // 3. Tải ảnh bìa (SỬA Ở ĐÂY)
            val fullUrl = getFullCoverUrl(novel.CoverImageUrl)

            Glide.with(itemView.context)
                .load(fullUrl) // <--- Dùng URL đã nối
                .placeholder(R.drawable.placeholder_cover)
                .error(R.drawable.placeholder_cover)
                .into(ivNovelCover)

            // Bắt sự kiện click
            itemView.setOnClickListener {
                onItemClick(novel)
            }
        }

        // Hàm format (1000 -> 1.0K, 1000000 -> 1.0M)
        private fun formatK(count: Long): String {
            return when {
                count >= 1_000_000 -> String.format(Locale.US, "%.1fM", count / 1_000_000.0)
                count >= 1_000 -> String.format(Locale.US, "%.1fK", count / 1_000.0)
                else -> count.toString()
            }
        }
    }

    // DiffUtil để RecyclerView biết item nào thay đổi
    class NovelDiffCallback : DiffUtil.ItemCallback<Novel>() {
        override fun areItemsTheSame(oldItem: Novel, newItem: Novel): Boolean {
            return oldItem.Id == newItem.Id
        }

        override fun areContentsTheSame(oldItem: Novel, newItem: Novel): Boolean {
            return oldItem == newItem
        }
    }
}