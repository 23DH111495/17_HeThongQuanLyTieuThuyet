package com.sinhvien.webnovelapp.adapters

import android.graphics.Color
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
import com.sinhvien.webnovelapp.models.NovelRankingDto

class CompactRankingAdapter(
    private val onItemClick: (NovelRankingDto) -> Unit
) : ListAdapter<NovelRankingDto, CompactRankingAdapter.CompactRankingViewHolder>(RankingDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): CompactRankingViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_ranking_compact, parent, false)
        return CompactRankingViewHolder(view, onItemClick)
    }

    override fun onBindViewHolder(holder: CompactRankingViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class CompactRankingViewHolder(
        itemView: View,
        private val onItemClick: (NovelRankingDto) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val tvRank: TextView = itemView.findViewById(R.id.tvRank)
        private val ivCover: ImageView = itemView.findViewById(R.id.ivCover)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvTitle)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvAuthor)
        private val tvGenres: TextView = itemView.findViewById(R.id.tvGenres)
        private val tvViews: TextView = itemView.findViewById(R.id.tvViews)
        private val tvRating: TextView = itemView.findViewById(R.id.tvRating)

        fun bind(novel: NovelRankingDto) {
            // Set rank - USE novel.Rank directly, not position
            tvRank.text = novel.Rank.toString()
            tvRank.setTextColor(Color.BLACK) // Always black text

            // Set rank background color based on rank
            when (novel.Rank) {
                4, 5 -> tvRank.setBackgroundResource(R.drawable.circle_rank_bg)
                else -> tvRank.setBackgroundResource(R.drawable.circle_rank_bg)
            }

            // Title
            tvTitle.text = novel.Title

            // Author
            tvAuthor.text = "By ${novel.AuthorName}"

            // Genres - join all genres with comma
            tvGenres.text = if (novel.Genres.isNotEmpty()) {
                novel.Genres.joinToString(", ")
            } else {
                "No genres"
            }

            // Load cover image
            val coverUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"
            Glide.with(itemView.context)
                .load(coverUrl)
                .placeholder(R.drawable.ic_book_placeholder)
                .error(R.drawable.ic_book_placeholder)
                .centerCrop()
                .into(ivCover)

            // Stats
            tvViews.text = formatNumber(novel.ViewCount)
            tvRating.text = String.format("%.1f", novel.AverageRating)

            // Click listener
            itemView.setOnClickListener {
                onItemClick(novel)
            }
        }

        private fun formatNumber(number: Long): String {
            return when {
                number >= 1_000_000 -> String.format("%.1fM", number / 1_000_000.0)
                number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
                else -> number.toString()
            }
        }
    }

    class RankingDiffCallback : DiffUtil.ItemCallback<NovelRankingDto>() {
        override fun areItemsTheSame(oldItem: NovelRankingDto, newItem: NovelRankingDto): Boolean {
            return oldItem.Id == newItem.Id
        }

        override fun areContentsTheSame(oldItem: NovelRankingDto, newItem: NovelRankingDto): Boolean {
            return oldItem == newItem
        }
    }
}