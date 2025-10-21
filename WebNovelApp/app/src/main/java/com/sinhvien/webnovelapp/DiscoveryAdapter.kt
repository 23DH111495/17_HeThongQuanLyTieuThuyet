package com.sinhvien.webnovelapp.adapters

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
import com.sinhvien.webnovelapp.models.Novel
import com.sinhvien.webnovelapp.models.Genre
class DiscoveryAdapter(
    private val onNovelClick: (Novel) -> Unit
) : ListAdapter<Novel, DiscoveryAdapter.ViewHolder>(NovelDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_discovery_novel, parent, false)
        return ViewHolder(view, onNovelClick)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class ViewHolder(
        itemView: View,
        private val onNovelClick: (Novel) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val ivCover: ImageView = itemView.findViewById(R.id.ivDiscoverCover)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvDiscoverTitle)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvDiscoverAuthor)
        private val tvDescription: TextView = itemView.findViewById(R.id.tvDiscoverDescription)
        private val tvGenres: TextView = itemView.findViewById(R.id.tvDiscoverGenres)
        private val tvStatus: TextView = itemView.findViewById(R.id.tvDiscoverStatus)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvDiscoverChapters)
        private val tvViews: TextView = itemView.findViewById(R.id.tvDiscoverViews)

        fun bind(novel: Novel) {
            tvTitle.text = novel.Title
            tvAuthor.text = novel.AuthorName ?: "Unknown Author"
            tvDescription.text = novel.Synopsis ?: "No description available"

            // Format genres
            val genreNames = novel.Genres?.joinToString(", ") ?: "Unknown"
            tvGenres.text = genreNames

            // Status badge
            tvStatus.text = novel.Status
            tvStatus.setTextColor(itemView.context.getColor(android.R.color.black))
            when (novel.Status?.lowercase()) {
                "ongoing" -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                }
                "completed" -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_completed)
                }
                else -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                }
            }

            // Chapters and views
            tvChapters.text = "${novel.TotalChapters ?: 0} Chapters"
            tvViews.text = formatNumber(novel.ViewCount ?: 0)

            // Load cover image
            val coverUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"
            Glide.with(itemView.context)
                .load(coverUrl)
                .placeholder(R.drawable.ic_book_placeholder)
                .error(R.drawable.ic_book_placeholder)
                .centerCrop()
                .into(ivCover)

            itemView.setOnClickListener {
                onNovelClick(novel)
            }
        }

        private fun formatNumber(number: Long): String {
            return when {
                number >= 1_000_000 -> String.format("%.1fM views", number / 1_000_000.0)
                number >= 1_000 -> String.format("%.1fK views", number / 1_000.0)
                else -> "$number views"
            }
        }
    }

    private class NovelDiffCallback : DiffUtil.ItemCallback<Novel>() {
        override fun areItemsTheSame(oldItem: Novel, newItem: Novel): Boolean {
            return oldItem.Id == newItem.Id
        }

        override fun areContentsTheSame(oldItem: Novel, newItem: Novel): Boolean {
            return oldItem == newItem
        }
    }
}