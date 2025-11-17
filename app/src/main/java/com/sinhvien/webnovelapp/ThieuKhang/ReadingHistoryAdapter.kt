package com.sinhvien.webnovelapp.ThieuKhang

import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageButton
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.ReadingHistoryItem
import java.text.SimpleDateFormat
import java.util.*
import java.util.concurrent.TimeUnit

class ReadingHistoryAdapter(
    private val onItemClick: (ReadingHistoryItem) -> Unit,
    private val onDeleteClick: (ReadingHistoryItem) -> Unit
) : ListAdapter<ReadingHistoryItem, ReadingHistoryAdapter.ViewHolder>(DiffCallback()) {

    inner class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        private val ivNovelCover: ImageView = view.findViewById(R.id.ivNovelCover)
        private val tvNovelTitle: TextView = view.findViewById(R.id.tvNovelTitle)
        private val tvRank: TextView = view.findViewById(R.id.tvRank)
        private val tvChapterProgress: TextView = view.findViewById(R.id.tvChapterProgress)
        private val tvPercentProgress: TextView = view.findViewById(R.id.tvPercentProgress)
        private val tvLastReadTime: TextView = view.findViewById(R.id.tvLastReadTime)
        private val tvLastReadChapter: TextView = view.findViewById(R.id.tvLastReadChapter)
        private val btnDelete: ImageButton = view.findViewById(R.id.btnDelete)

        fun bind(item: ReadingHistoryItem) {
            Log.d("ReadingHistoryAdapter", "Binding: ${item.novelTitle}")

            // Load cover image
            val imageUrl = "http://10.0.2.2:7200/api/novels/${item.novelId}/cover"
            Glide.with(itemView.context)
                .load(imageUrl)
                .placeholder(R.drawable.placeholder_novel_cover)
                .error(R.drawable.placeholder_novel_cover)
                .into(ivNovelCover)

            // Set novel title - THIS IS THE MOST IMPORTANT LINE
            tvNovelTitle.text = item.novelTitle
            Log.d("ReadingHistoryAdapter", "Set title to: ${item.novelTitle}")

            // Hide rank for now
            tvRank.visibility = View.GONE

            // Set chapter progress
            tvChapterProgress.text = "${item.lastReadChapterNumber} / ${item.totalChapters}"

            // Format percentage without parentheses
            tvPercentProgress.text = "${item.readProgress}%"

            // Set last read time
            tvLastReadTime.text = formatTimeAgo(item.lastReadDate)

            // Set last read chapter with title if available
            tvLastReadChapter.text = if (!item.lastReadChapterTitle.isNullOrEmpty()) {
                "Ch ${item.lastReadChapterNumber}: ${item.lastReadChapterTitle}"
            } else {
                "Chapter ${item.lastReadChapterNumber}"
            }

            // Click listeners
            itemView.setOnClickListener {
                Log.d("ReadingHistoryAdapter", "Clicked: ${item.novelTitle}")
                onItemClick(item)
            }
            btnDelete.setOnClickListener {
                Log.d("ReadingHistoryAdapter", "Delete clicked: ${item.novelTitle}")
                onDeleteClick(item)
            }
        }

        private fun formatTimeAgo(dateString: String): String {
            return try {
                val sdf = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault())
                sdf.timeZone = TimeZone.getTimeZone("UTC")
                val date = sdf.parse(dateString) ?: return dateString

                val now = System.currentTimeMillis()
                val diff = now - date.time

                when {
                    diff < TimeUnit.MINUTES.toMillis(1) -> "Just now"
                    diff < TimeUnit.HOURS.toMillis(1) -> {
                        val minutes = TimeUnit.MILLISECONDS.toMinutes(diff)
                        "$minutes min ago"
                    }
                    diff < TimeUnit.DAYS.toMillis(1) -> {
                        val hours = TimeUnit.MILLISECONDS.toHours(diff)
                        "$hours hr ago"
                    }
                    diff < TimeUnit.DAYS.toMillis(7) -> {
                        val days = TimeUnit.MILLISECONDS.toDays(diff)
                        "$days day${if (days == 1L) "" else "s"} ago"
                    }
                    diff < TimeUnit.DAYS.toMillis(30) -> {
                        val weeks = TimeUnit.MILLISECONDS.toDays(diff) / 7
                        "$weeks week${if (weeks == 1L) "" else "s"} ago"
                    }
                    diff < TimeUnit.DAYS.toMillis(365) -> {
                        val months = TimeUnit.MILLISECONDS.toDays(diff) / 30
                        "$months month${if (months == 1L) "" else "s"} ago"
                    }
                    else -> {
                        val years = TimeUnit.MILLISECONDS.toDays(diff) / 365
                        "$years year${if (years == 1L) "" else "s"} ago"
                    }
                }
            } catch (e: Exception) {
                Log.e("ReadingHistoryAdapter", "Error formatting date: ${e.message}")
                dateString
            }
        }
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_reading_history, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class DiffCallback : DiffUtil.ItemCallback<ReadingHistoryItem>() {
        override fun areItemsTheSame(oldItem: ReadingHistoryItem, newItem: ReadingHistoryItem): Boolean {
            return oldItem.novelId == newItem.novelId
        }

        override fun areContentsTheSame(oldItem: ReadingHistoryItem, newItem: ReadingHistoryItem): Boolean {
            return oldItem == newItem
        }
    }
}