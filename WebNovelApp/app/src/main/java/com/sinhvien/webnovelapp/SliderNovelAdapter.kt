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

class SliderNovelAdapter(
    private val onNovelClick: (Novel) -> Unit
) : RecyclerView.Adapter<SliderNovelAdapter.SliderViewHolder>() {

    private var novels = listOf<Novel>()

    fun submitList(newNovels: List<Novel>) {
        novels = newNovels
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): SliderViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_slider_novel, parent, false)
        return SliderViewHolder(view, onNovelClick)
    }

    override fun onBindViewHolder(holder: SliderViewHolder, position: Int) {
        holder.bind(novels[position])
    }

    override fun getItemCount() = novels.size

    class SliderViewHolder(
        itemView: View,
        private val onNovelClick: (Novel) -> Unit
    ) : RecyclerView.ViewHolder(itemView) {

        private val ivCover: ImageView = itemView.findViewById(R.id.ivSliderCover)
        private val tvTitle: TextView = itemView.findViewById(R.id.tvSliderTitle)
        private val tvGenre: TextView = itemView.findViewById(R.id.tvSliderGenre)
        private val tvRating: TextView = itemView.findViewById(R.id.tvSliderRating)
        private val tvChapters: TextView = itemView.findViewById(R.id.tvSliderChapters)
        private val tvAuthor: TextView = itemView.findViewById(R.id.tvSliderAuthor)

        fun bind(novel: Novel) {
            tvTitle.text = novel.Title
            // Clean author name without prefix
            tvAuthor.text = novel.AuthorName
            tvRating.text = String.format("%.1f", novel.AverageRating)

            // Show chapter count with "Chapters" text
            tvChapters.text = "${novel.TotalChapters} Chapters"

            if (novel.Genres.isNotEmpty()) {
                tvGenre.text = novel.Genres.first()
                tvGenre.visibility = View.VISIBLE
            } else {
                tvGenre.visibility = View.GONE
            }

            val coverImageUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"

            Glide.with(itemView.context)
                .load(coverImageUrl)
                .placeholder(R.drawable.placeholder_novel_cover)
                .error(R.drawable.placeholder_novel_cover)
                .diskCacheStrategy(DiskCacheStrategy.ALL)
                .into(object : com.bumptech.glide.request.target.ImageViewTarget<android.graphics.drawable.Drawable>(ivCover) {
                    override fun setResource(resource: android.graphics.drawable.Drawable?) {
                        resource?.let {
                            val matrix = android.graphics.Matrix()
                            val viewWidth = ivCover.width.toFloat()
                            val viewHeight = ivCover.height.toFloat()
                            val drawableWidth = it.intrinsicWidth.toFloat()
                            val drawableHeight = it.intrinsicHeight.toFloat()

                            val scale: Float
                            val dx: Float
                            val dy = 0f // Start from top

                            if (drawableWidth * viewHeight > viewWidth * drawableHeight) {
                                scale = viewHeight / drawableHeight
                                dx = (viewWidth - drawableWidth * scale) * 0.5f
                            } else {
                                scale = viewWidth / drawableWidth
                                dx = 0f
                            }

                            matrix.setScale(scale, scale)
                            matrix.postTranslate(dx, dy)

                            ivCover.imageMatrix = matrix
                            ivCover.setImageDrawable(it)
                        }
                    }
                })

            itemView.setOnClickListener {
                onNovelClick(novel)
            }
        }
    }
}