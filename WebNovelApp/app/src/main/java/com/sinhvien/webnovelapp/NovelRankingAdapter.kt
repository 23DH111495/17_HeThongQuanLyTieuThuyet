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
import com.google.android.material.chip.Chip
import com.google.android.material.chip.ChipGroup
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.models.NovelRankingDto
import java.text.DecimalFormat

class NovelRankingAdapter(
    private val onItemClick: (NovelRankingDto) -> Unit
) : ListAdapter<NovelRankingDto, NovelRankingAdapter.RankingViewHolder>(RankingDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RankingViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_novel_ranking, parent, false)
        return RankingViewHolder(view, onItemClick)
    }

    override fun onBindViewHolder(holder: RankingViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class RankingViewHolder(
        itemView: View,
        private val onItemClick: (NovelRankingDto) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val tvRank: TextView = itemView.findViewById(R.id.tvRank)
        private val ivCover: ImageView = itemView.findViewById(R.id.ivCover)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvTitle)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvAuthor)
        private val tvViews: TextView = itemView.findViewById(R.id.tvViews)
        private val tvRating: TextView = itemView.findViewById(R.id.tvRating)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvChapters)
        private val genreChips: ChipGroup = itemView.findViewById(R.id.genreChips)
        private val ivPremiumBadge: ImageView = itemView.findViewById(R.id.ivPremiumBadge)

        fun bind(novel: NovelRankingDto) {
            // Rank with color coding
            tvRank.text = novel.Rank.toString()
            tvRank.setTextColor(when (novel.Rank) {
                1 -> Color.parseColor("#FFD700") // Gold
                2 -> Color.parseColor("#C0C0C0") // Silver
                3 -> Color.parseColor("#CD7F32") // Bronze
                else -> Color.parseColor("#77dd77")
            })

            // Title and Author
            tvTitle.text = novel.Title
            tvAuthor.text = novel.AuthorName

            // Load cover image
            val coverUrl = "${com.sinhvien.webnovelapp.api.ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"
            Glide.with(itemView.context)
                .load(coverUrl)
                .placeholder(R.drawable.ic_book_placeholder)
                .error(R.drawable.ic_book_placeholder)
                .centerCrop()
                .into(ivCover)

            // Stats
            tvViews.text = formatNumber(novel.ViewCount)
            tvRating.text = String.format("%.1f", novel.AverageRating)
            tvChapters.text = "${novel.TotalChapters} ch"


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