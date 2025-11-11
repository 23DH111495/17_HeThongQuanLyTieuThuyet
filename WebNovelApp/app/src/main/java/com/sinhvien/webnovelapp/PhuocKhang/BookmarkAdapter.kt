package com.sinhvien.webnovelapp.PhuocKhang

import android.content.Context
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
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.models.Bookmark
import java.util.Locale

class BookmarkAdapter(
    private val context: Context,
    private val onItemClick: (novelId: Int) -> Unit
) : ListAdapter<Bookmark, BookmarkAdapter.BookmarkViewHolder>(BookmarkDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): BookmarkViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.novel_list_item, parent, false)
        return BookmarkViewHolder(view, context, onItemClick)
    }

    override fun onBindViewHolder(holder: BookmarkViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class BookmarkViewHolder(
        itemView: View,
        private val context: Context,
        private val onItemClick: (novelId: Int) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val ivNovelCover: ImageView = itemView.findViewById(R.id.ivNovelCover)
        private val tvNovelTitle: TextView = itemView.findViewById(R.id.tvNovelTitle)
        private val tvNovelAuthor: TextView = itemView.findViewById(R.id.tvNovelAuthor)
        private val tvViewsCount: TextView = itemView.findViewById(R.id.tvViewsCount)
        private val tvRating: TextView = itemView.findViewById(R.id.tvRating)
        private val tvBookmarkCount: TextView = itemView.findViewById(R.id.tvBookmarkCount)

        fun bind(bookmark: Bookmark) {
            val novel = bookmark.novel
            val baseUrl = ApiClient.getBaseUrl()

            if (novel != null) {
                tvNovelTitle.text = novel.title

                // Hiển thị AuthorId nếu chưa có tên tác giả
                tvNovelAuthor.text = "Author ID: ${novel.authorId}"

                tvViewsCount.text = formatCount(novel.viewCount)
                tvRating.text = String.format(Locale.US, "%.1f", novel.averageRating)
                tvBookmarkCount.text = formatCount(novel.bookmarkCount)

                // Sử dụng CoverImageUrl từ API
                val coverUrl = if (!novel.coverImageUrl.isNullOrEmpty()) {
                    novel.coverImageUrl
                } else {
                    "${baseUrl}api/novels/${novel.id}/cover"
                }

                Glide.with(context)
                    .load(coverUrl)
                    .placeholder(R.drawable.placeholder_cover)
                    .error(R.drawable.placeholder_cover)
                    .centerCrop()
                    .into(ivNovelCover)
            } else {
                // Fallback nếu novel null
                tvNovelTitle.text = "Novel ID: ${bookmark.novelId}"
                tvNovelAuthor.text = "Không có dữ liệu"
                tvViewsCount.text = "0"
                tvRating.text = "0.0"
                tvBookmarkCount.text = "0"

                Glide.with(context)
                    .load(R.drawable.placeholder_cover)
                    .into(ivNovelCover)
            }

            itemView.setOnClickListener {
                onItemClick(bookmark.novelId)
            }
        }

        private fun formatCount(count: Long): String {
            return when {
                count >= 1_000_000 -> String.format(Locale.US, "%.1fM", count / 1_000_000.0)
                count >= 1_000 -> String.format(Locale.US, "%.1fK", count / 1_000.0)
                else -> count.toString()
            }
        }
    }

    class BookmarkDiffCallback : DiffUtil.ItemCallback<Bookmark>() {
        override fun areItemsTheSame(oldItem: Bookmark, newItem: Bookmark) =
            oldItem.bookmarkId == newItem.bookmarkId

        override fun areContentsTheSame(oldItem: Bookmark, newItem: Bookmark) =
            oldItem == newItem
    }
}