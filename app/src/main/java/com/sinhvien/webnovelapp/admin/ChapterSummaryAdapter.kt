package com.sinhvien.webnovelapp.adapters

import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.TextView
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.ChapterSummary
import java.text.SimpleDateFormat
import java.util.*

class ChapterSummaryAdapter(
    private val onChapterClick: (ChapterSummary) -> Unit
) : ListAdapter<ChapterSummary, ChapterSummaryAdapter.ChapterViewHolder>(ChapterDiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ChapterViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_chapter_summary, parent, false)
        return ChapterViewHolder(view)
    }

    override fun onBindViewHolder(holder: ChapterViewHolder, position: Int) {
        holder.bind(getItem(position), onChapterClick)
    }

    class ChapterViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val tvChapterTitle: TextView = itemView.findViewById(R.id.tvChapterTitle)
        private val tvChapterInfo: TextView = itemView.findViewById(R.id.tvChapterInfo)
        private val llPremiumIndicator: LinearLayout? = itemView.findViewById(R.id.llPremiumIndicator)
        private val llUnlockedIndicator: LinearLayout? = itemView.findViewById(R.id.llUnlockedIndicator)
        private val ivLockIcon: ImageView? = itemView.findViewById(R.id.ivLockIcon)
        private val tvChapterPrice: TextView? = itemView.findViewById(R.id.tvChapterPrice)

        fun bind(chapter: ChapterSummary, onChapterClick: (ChapterSummary) -> Unit) {
            // Set chapter title
            tvChapterTitle.text = "Chapter ${chapter.ChapterNumber}: ${chapter.Title}"

            // Format chapter info
            val wordCount = formatNumber(chapter.WordCount)
            val publishDate = formatDate(chapter.PublishDate)
            tvChapterInfo.text = "$wordCount words • $publishDate"

            // Debug logging
            Log.d("ChapterAdapter", "===== Chapter ${chapter.ChapterNumber} =====")
            Log.d("ChapterAdapter", "Title: ${chapter.Title}")
            Log.d("ChapterAdapter", "IsPremium: ${chapter.IsPremium}")
            Log.d("ChapterAdapter", "UnlockPrice: ${chapter.UnlockPrice}")
            Log.d("ChapterAdapter", "IsUnlocked: ${chapter.IsUnlocked}")

            // Determine if chapter has a price
            val hasPremiumPrice = (chapter.UnlockPrice ?: 0) > 0

            // Check if chapter is unlocked (user already paid)
            if (chapter.IsUnlocked && hasPremiumPrice) {
                // Show UNLOCKED indicator (green checkmark)
                Log.d("ChapterAdapter", "Chapter is UNLOCKED (already paid)")
                llUnlockedIndicator?.visibility = View.VISIBLE
                llPremiumIndicator?.visibility = View.GONE

            } else if (hasPremiumPrice || chapter.IsPremium) {
                // Show LOCKED indicator (gold lock + price)
                Log.d("ChapterAdapter", "Chapter is LOCKED (needs payment)")
                llUnlockedIndicator?.visibility = View.GONE
                llPremiumIndicator?.visibility = View.VISIBLE

                val price = chapter.UnlockPrice ?: 0
                tvChapterPrice?.text = "$price coins"

            } else {
                // Free chapter - hide all indicators
                Log.d("ChapterAdapter", "Chapter is FREE")
                llUnlockedIndicator?.visibility = View.GONE
                llPremiumIndicator?.visibility = View.GONE
            }

            // Set click listener
            itemView.setOnClickListener {
                onChapterClick(chapter)
            }
        }

        private fun formatNumber(number: Int): String {
            return when {
                number >= 1_000_000 -> String.format("%.1fM", number / 1_000_000.0)
                number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
                else -> number.toString()
            }
        }

        private fun formatDate(dateString: String?): String {
            if (dateString == null) return "Unknown date"

            return try {
                val inputFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault())
                val outputFormat = SimpleDateFormat("MMM dd, yyyy", Locale.getDefault())
                val date = inputFormat.parse(dateString)
                outputFormat.format(date ?: Date())
            } catch (e: Exception) {
                try {
                    dateString.substring(0, 10)
                } catch (e: Exception) {
                    "Unknown date"
                }
            }
        }
    }

    class ChapterDiffCallback : DiffUtil.ItemCallback<ChapterSummary>() {
        override fun areItemsTheSame(oldItem: ChapterSummary, newItem: ChapterSummary): Boolean {
            return oldItem.Id == newItem.Id
        }

        override fun areContentsTheSame(oldItem: ChapterSummary, newItem: ChapterSummary): Boolean {
            return oldItem == newItem
        }
    }
}