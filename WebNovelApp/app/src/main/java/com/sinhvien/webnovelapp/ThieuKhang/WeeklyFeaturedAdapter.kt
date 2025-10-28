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

class WeeklyFeaturedAdapter(
    private val onItemClick: (Novel) -> Unit
) : ListAdapter<Novel, WeeklyFeaturedAdapter.ViewHolder>(NovelDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_novel_horizonta, parent, false)  // This should use the horizontal layout
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    inner class ViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val ivNovelCover: ImageView = itemView.findViewById(R.id.ivNovelCover)
        private val tvNovelTitle: TextView = itemView.findViewById(R.id.tvNovelTitle)
        private val tvNovelAuthor: TextView = itemView.findViewById(R.id.tvNovelAuthor)
        private val tvNovelChapters: TextView = itemView.findViewById(R.id.tvNovelChapters)
        private val tvNovelRating: TextView = itemView.findViewById(R.id.tvNovelRating)
        private val tvNovelStatus: TextView = itemView.findViewById(R.id.tvNovelStatus)
        private val tvNovelPremium: TextView = itemView.findViewById(R.id.tvNovelPremium)

        init {
            itemView.setOnClickListener {
                val position = adapterPosition
                if (position != RecyclerView.NO_POSITION) {
                    onItemClick(getItem(position))
                }
            }
        }

        fun bind(novel: Novel) {
            tvNovelTitle.text = novel.Title
            tvNovelAuthor.text = "by ${novel.AuthorName}"
            tvNovelChapters.text = "${novel.TotalChapters} chapters"

            // Rating
            val ratingText = if (novel.AverageRating > 0) {
                "★ %.2f (%d)".format(novel.AverageRating, novel.TotalRatings)
            } else {
                "★ No ratings"
            }
            tvNovelRating.text = ratingText

            // Status with gradient background
            tvNovelStatus.text = novel.Status.uppercase()
            tvNovelStatus.setTextColor(itemView.context.getColor(android.R.color.black))

            when (novel.Status.lowercase()) {
                "ongoing" -> {
                    tvNovelStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                }
                "completed" -> {
                    tvNovelStatus.setBackgroundResource(R.drawable.bg_status_completed)
                }
                "hiatus" -> {
                    tvNovelStatus.setBackgroundResource(R.drawable.bg_status_hiatus)
                }
                "dropped" -> {
                    tvNovelStatus.setBackgroundResource(R.drawable.bg_status_dropped)
                }
                else -> {
                    tvNovelStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                }
            }

            // Premium badge
            tvNovelPremium.visibility = if (novel.IsPremium) View.VISIBLE else View.GONE

            // Load cover image from API endpoint
            val coverImageUrl = "${com.sinhvien.webnovelapp.api.ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"

            Glide.with(itemView.context)
                .load(coverImageUrl)
                .placeholder(R.drawable.placeholder_novel_cover)
                .error(R.drawable.placeholder_novel_cover)
                .diskCacheStrategy(com.bumptech.glide.load.engine.DiskCacheStrategy.ALL)
                .centerCrop()
                .into(ivNovelCover)
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