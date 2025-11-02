package com.sinhvien.webnovelapp.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.ImageButton
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.TextView
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.Novel

class WeeklyFeaturedNetflixAdapter(
    private val onItemClick: (Novel) -> Unit
) : ListAdapter<Novel, WeeklyFeaturedNetflixAdapter.ViewHolder>(NovelDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_weekly_featured_netflix, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position), position)
    }

    inner class ViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val ivCover: ImageView = itemView.findViewById(R.id.ivCoverNetflix)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvTitleNetflix)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvAuthorNetflix)
        private val tvStatus: TextView = itemView.findViewById(R.id.tvStatusNetflix)
        private val tvRating: TextView = itemView.findViewById(R.id.tvRatingNetflix)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvChaptersNetflix)
        private val tvGenre: TextView = itemView.findViewById(R.id.tvGenreNetflix)
        private val tvDescription: TextView = itemView.findViewById(R.id.tvDescriptionNetflix)
        private val tvRank: TextView = itemView.findViewById(R.id.tvRankNetflix)
        private val llRankBadge: LinearLayout = itemView.findViewById(R.id.llRankBadgeNetflix)
        private val btnRead: Button = itemView.findViewById(R.id.btnReadNetflix)
        private val btnAddLibrary: ImageButton = itemView.findViewById(R.id.btnAddLibraryNetflix)

        init {
            itemView.setOnClickListener {
                val position = adapterPosition
                if (position != RecyclerView.NO_POSITION) {
                    onItemClick(getItem(position))
                }
            }

            btnRead.setOnClickListener {
                val position = adapterPosition
                if (position != RecyclerView.NO_POSITION) {
                    onItemClick(getItem(position))
                }
            }

            btnAddLibrary.setOnClickListener {
                // Handle add to library action
                // You can implement this functionality as needed
            }
        }

        fun bind(novel: Novel, position: Int) {
            tvTitle.text = novel.Title
            tvAuthor.text = "by ${novel.AuthorName}"
            tvChapters.text = "${novel.TotalChapters} Chapters"

            // Rating
            val ratingText = if (novel.AverageRating > 0) {
                "%.1f".format(novel.AverageRating)
            } else {
                "N/A"
            }
            tvRating.text = ratingText

            // Status
            tvStatus.text = novel.Status.uppercase()

            // Genre (you might want to add a Genre field to your Novel model)
            tvGenre.text = "Fantasy" // Default genre, replace with actual genre if available

            // Description (truncated)
            tvDescription.text = novel.Synopsis?.take(80) ?: "An exciting novel waiting to be discovered..."

            // Show rank badge for top 10
            if (position < 10) {
                llRankBadge.visibility = View.VISIBLE
                tvRank.text = "${position + 1}"
            } else {
                llRankBadge.visibility = View.GONE
            }

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