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

class CompletedNovelsAdapter(
    private val onNovelClick: (Novel) -> Unit
) : ListAdapter<Novel, CompletedNovelsAdapter.CompletedNovelViewHolder>(NovelDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): CompletedNovelViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_novel_grid, parent, false)
        return CompletedNovelViewHolder(view, onNovelClick)
    }

    override fun onBindViewHolder(holder: CompletedNovelViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class CompletedNovelViewHolder(
        itemView: View,
        private val onNovelClick: (Novel) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val ivCover: ImageView = itemView.findViewById(R.id.ivNovelCover)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvNovelTitle)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvNovelAuthor)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvNovelChapters)
        private val tvRating: TextView = itemView.findViewById(R.id.tvNovelRating)
        private val tvStatus: TextView = itemView.findViewById(R.id.tvNovelStatus)
        private val tvPremium: TextView = itemView.findViewById(R.id.tvNovelPremium)

        fun bind(novel: Novel) {
            tvTitle.text = novel.Title
            tvAuthor.text = "by ${novel.AuthorName}"
            tvChapters.text = "${novel.TotalChapters} chapters"

            // Rating
            val ratingText = if (novel.AverageRating > 0) {
                "★ %.2f (%d)".format(novel.AverageRating, novel.TotalRatings)
            } else {
                "★ No ratings"
            }
            tvRating.text = ratingText

            tvStatus.text = novel.Status.uppercase()
            when (novel.Status.lowercase()) {
                "completed" -> tvStatus.setBackgroundResource(R.drawable.bg_status_completed)
                "ongoing" -> tvStatus.setBackgroundResource(R.drawable.bg_status_ongoing)
                "hiatus" -> tvStatus.setBackgroundResource(R.drawable.bg_status_hiatus)
                else -> tvStatus.setBackgroundResource(R.drawable.bg_status_completed)
            }

            // Premium badge
            tvPremium.visibility = if (novel.IsPremium) View.VISIBLE else View.GONE

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