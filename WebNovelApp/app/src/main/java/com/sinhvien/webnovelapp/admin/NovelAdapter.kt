package com.sinhvien.webnovelapp.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.bumptech.glide.load.engine.DiskCacheStrategy
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.Novel
import com.sinhvien.webnovelapp.api.ApiClient


class NovelAdapter(
    private val onNovelClick: (Novel) -> Unit
) : RecyclerView.Adapter<NovelAdapter.NovelViewHolder>() {

    private var novels = listOf<Novel>()

    fun submitList(newNovels: List<Novel>) {
        novels = newNovels
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): NovelViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_novel, parent, false)
        return NovelViewHolder(view, onNovelClick)
    }

    override fun onBindViewHolder(holder: NovelViewHolder, position: Int) {
        holder.bind(novels[position])
    }

    override fun getItemCount() = novels.size

    class NovelViewHolder(
        itemView: View,
        private val onNovelClick: (Novel) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val ivCover: ImageView = itemView.findViewById(R.id.ivNovelCover)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvNovelTitle)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvNovelAuthor)
        private val tvSynopsis: TextView = itemView.findViewById(R.id.tvNovelSynopsis)
        private val tvStatus: TextView = itemView.findViewById(R.id.tvNovelStatus)
        private val tvRating: TextView = itemView.findViewById(R.id.tvNovelRating)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvNovelChapters)
        private val tvGenres: TextView = itemView.findViewById(R.id.tvNovelGenres)
        private val tvPremium: TextView = itemView.findViewById(R.id.tvNovelPremium)

        fun bind(novel: Novel) {
            tvTitle.text = novel.Title
            tvAuthor.text = "by ${novel.AuthorName}"
            tvSynopsis.text = novel.Synopsis ?: "No synopsis available"
            tvStatus.text = novel.Status
            tvRating.text = "★ ${String.format("%.2f", novel.AverageRating)} (${novel.TotalRatings})"
            tvChapters.text = "${novel.TotalChapters} chapters"
            tvGenres.text = novel.Genres.joinToString(", ")

            tvPremium.visibility = if (novel.IsPremium) View.VISIBLE else View.GONE

            val coverImageUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"

            Glide.with(itemView.context)
                .load(coverImageUrl)
                .placeholder(R.drawable.placeholder_novel_cover)
                .error(R.drawable.placeholder_novel_cover)
                .diskCacheStrategy(DiskCacheStrategy.ALL)
                .into(ivCover)

            itemView.setOnClickListener {
                onNovelClick(novel)
            }
        }
    }
}