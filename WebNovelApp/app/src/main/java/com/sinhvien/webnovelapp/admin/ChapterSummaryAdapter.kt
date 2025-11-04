package com.sinhvien.webnovelapp.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.ChapterSummary

class ChapterSummaryAdapter(
    private val onChapterClick: (ChapterSummary) -> Unit
) : RecyclerView.Adapter<ChapterSummaryAdapter.ChapterViewHolder>() {

    private var chapters = listOf<ChapterSummary>()

    fun submitList(newChapters: List<ChapterSummary>) {
        chapters = newChapters
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ChapterViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_chapter_summary, parent, false)
        return ChapterViewHolder(view, onChapterClick)
    }

    override fun onBindViewHolder(holder: ChapterViewHolder, position: Int) {
        holder.bind(chapters[position])
    }

    override fun getItemCount() = chapters.size

    class ChapterViewHolder(
        itemView: View,
        private val onChapterClick: (ChapterSummary) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val tvChapterTitle: TextView = itemView.findViewById(R.id.tvChapterTitle)
        private val tvChapterInfo: TextView = itemView.findViewById(R.id.tvChapterInfo)
        private val tvChapterPremium: TextView = itemView.findViewById(R.id.tvChapterPremium)

        fun bind(chapter: ChapterSummary) {
            val chapterTitle = if (chapter.Title.isNullOrEmpty()) {
                if (chapter.ChapterNumber == 0) "Prologue" else "Chapter ${chapter.ChapterNumber}"
            } else {
                "Chapter ${chapter.ChapterNumber}: ${chapter.Title}"
            }

            tvChapterTitle.text = chapterTitle
            tvChapterInfo.text = "${formatNumber(chapter.WordCount)} words • ${chapter.PublishDate ?: "Unknown date"}"

            // Show premium indicator if needed
            tvChapterPremium.visibility = if (chapter.IsPremium) View.VISIBLE else View.GONE

            itemView.setOnClickListener {
                onChapterClick(chapter)
            }
        }

        private fun formatNumber(number: Int): String {
            return when {
                number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
                else -> number.toString()
            }
        }
    }
}