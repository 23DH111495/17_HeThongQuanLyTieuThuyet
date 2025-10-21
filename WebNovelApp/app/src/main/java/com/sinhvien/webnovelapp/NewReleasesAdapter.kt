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
import com.sinhvien.webnovelapp.models.Novel

class NewReleasesAdapter(
    private val onItemClick: (Novel) -> Unit
) : ListAdapter<Novel, NewReleasesAdapter.ViewHolder>(NovelDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_new_release, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    inner class ViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val ivCover: ImageView = itemView.findViewById(R.id.ivCoverNewRelease)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvTitleNewRelease)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvAuthorNewRelease)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvChaptersNewRelease)
        private val tvStatus: TextView = itemView.findViewById(R.id.tvStatusNewRelease)
        private val tvRating: TextView = itemView.findViewById(R.id.tvRatingNewRelease)

        init {
            itemView.setOnClickListener {
                val position = adapterPosition
                if (position != RecyclerView.NO_POSITION) {
                    onItemClick(getItem(position))
                }
            }
        }

        fun bind(novel: Novel) {
            tvTitle.text = novel.Title
            tvAuthor.text = novel.AuthorName
            tvChapters.text = "${novel.TotalChapters} Ch"

            // Show actual status instead of hardcoded "NEW"
            tvStatus.text = novel.Status
            tvStatus.setTextColor(itemView.context.getColor(android.R.color.black))

            // Set status badge color based on status
            when (novel.Status.lowercase()) {
                "ongoing" -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                }
                "completed" -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_completed)
                }
                "hiatus" -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_hiatus)
                }
                "dropped" -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                }
                else -> {
                    tvStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                }
            }

            // Rating
            val ratingText = if (novel.AverageRating > 0) {
                "⭐ %.1f".format(novel.AverageRating)
            } else {
                "⭐ N/A"
            }
            tvRating.text = ratingText

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