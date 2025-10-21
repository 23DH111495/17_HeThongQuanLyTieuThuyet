package com.sinhvien.webnovelapp.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.Novel

class SearchSuggestionAdapter(
    private val onNovelClick: (Novel) -> Unit
) : RecyclerView.Adapter<RecyclerView.ViewHolder>() {

    private val novels = mutableListOf<Novel>()
    private var showNoResults = false

    companion object {
        private const val VIEW_TYPE_NOVEL = 1
        private const val VIEW_TYPE_NO_RESULTS = 2
    }

    override fun getItemViewType(position: Int): Int {
        return if (showNoResults) VIEW_TYPE_NO_RESULTS else VIEW_TYPE_NOVEL
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RecyclerView.ViewHolder {
        return when (viewType) {
            VIEW_TYPE_NO_RESULTS -> {
                val view = LayoutInflater.from(parent.context)
                    .inflate(R.layout.item_no_search_results, parent, false)
                NoResultsViewHolder(view)
            }
            else -> {
                val view = LayoutInflater.from(parent.context)
                    .inflate(R.layout.item_search_suggestion, parent, false)
                NovelViewHolder(view)
            }
        }
    }

    override fun onBindViewHolder(holder: RecyclerView.ViewHolder, position: Int) {
        when (holder) {
            is NovelViewHolder -> holder.bind(novels[position])
            is NoResultsViewHolder -> holder.bind()
        }
    }

    override fun getItemCount(): Int {
        return if (showNoResults) 1 else novels.size
    }

    fun updateNovels(newNovels: List<Novel>) {
        novels.clear()
        novels.addAll(newNovels)
        showNoResults = newNovels.isEmpty()
        notifyDataSetChanged()
    }

    fun clearSuggestions() {
        novels.clear()
        showNoResults = false
        notifyDataSetChanged()
    }

    inner class NovelViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val ivCover: ImageView = itemView.findViewById(R.id.ivNovelCover)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvNovelTitle)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvNovelAuthor)
        private val tvGenres: TextView = itemView.findViewById(R.id.tvNovelGenres)
        private val tvStatus: TextView = itemView.findViewById(R.id.tvNovelStatus)

        fun bind(novel: Novel) {
            tvTitle.text = novel.Title
            tvAuthor.text = novel.AuthorName

            // Format genres
            val genreNames = novel.Genres?.joinToString(", ") ?: "Unknown"
            tvGenres.text = genreNames

            // Display status
            tvStatus.text = novel.Status ?: "Unknown"

            // Load cover image
            val coverImageUrl = "${com.sinhvien.webnovelapp.api.ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"

            Glide.with(itemView.context)
                .load(coverImageUrl)
                .placeholder(R.drawable.placeholder_novel_cover)
                .error(R.drawable.placeholder_novel_cover)
                .diskCacheStrategy(com.bumptech.glide.load.engine.DiskCacheStrategy.ALL)
                .centerCrop()
                .into(ivCover)

            itemView.setOnClickListener {
                onNovelClick(novel)
            }
        }
    }

    inner class NoResultsViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val tvNoResults: TextView = itemView.findViewById(R.id.tvNoResults)
        private val tvSuggestion: TextView = itemView.findViewById(R.id.tvSuggestion)

        fun bind() {
            tvNoResults.text = "No matching novels found"
            tvSuggestion.text = "Try adjusting your search terms or browse our categories"
        }
    }
}