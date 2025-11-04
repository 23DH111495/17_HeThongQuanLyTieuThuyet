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
import com.sinhvien.webnovelapp.models.Novel

class WeeklyFeaturedWebnovelAdapter(
    private val onItemClick: (Novel) -> Unit
) : ListAdapter<Novel, WeeklyFeaturedWebnovelAdapter.ViewHolder>(NovelDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_weekly_featured_webnovel, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position), position)
    }

    inner class ViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val ivCover: ImageView = itemView.findViewById(R.id.ivCoverWebnovel)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvTitleWebnovel)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvAuthorWebnovel)
        private val tvStatus: TextView = itemView.findViewById(R.id.tvStatusWebnovel)
        private val tvRating: TextView = itemView.findViewById(R.id.tvRatingWebnovel)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvChaptersWebnovel)
        private val tvGenre1: TextView = itemView.findViewById(R.id.tvGenre1Webnovel)
        private val tvGenre2: TextView = itemView.findViewById(R.id.tvGenre2Webnovel)
        private val tvRank: TextView = itemView.findViewById(R.id.tvRankWebnovel)

        init {
            itemView.setOnClickListener {
                val position = adapterPosition
                if (position != RecyclerView.NO_POSITION) {
                    onItemClick(getItem(position))
                }
            }
        }

        fun bind(novel: Novel, position: Int) {
            tvTitle.text = novel.Title
            tvAuthor.text = "by ${novel.AuthorName}"
            tvChapters.text = "${novel.TotalChapters}"

            // Rating
            val ratingText = if (novel.AverageRating > 0) {
                "%.1f".format(novel.AverageRating)
            } else {
                "N/A"
            }
            tvRating.text = ratingText

            // Status badge
            tvStatus.text = when {
                position < 3 -> "HOT"
                novel.Status.equals("completed", ignoreCase = true) -> "NEW"
                else -> "TOP"
            }

            // Rank
            tvRank.text = "#${position + 1}"

            // Genres (you might want to add Genre fields to your Novel model)
            tvGenre1.text = "Fantasy"
            tvGenre2.text = "Adventure"

            // Load cover image
            val coverImageUrl = "${com.sinhvien.webnovelapp.api.ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"

            Glide.with(itemView.context)
                .load(coverImageUrl)
                .placeholder(R.drawable.placeholder_novel_cover)
                .error(R.drawable.placeholder_novel_cover)
                .diskCacheStrategy(com.bumptech.glide.load.engine.DiskCacheStrategy.ALL)
                .centerCrop()
                .into(ivCover)
        }
    }

    class NovelDiffCallback : DiffUtil.ItemCallback<Novel>() {
        override fun areItemsTheSame(oldItem: Novel, newItem: Novel): Boolean {
            return oldItem.Id == newItem.Id
        }

        override fun areContentsTheSame(oldItem: Novel, newItem: Novel): Boolean {
            return oldItem == newItem
        }
    }
}