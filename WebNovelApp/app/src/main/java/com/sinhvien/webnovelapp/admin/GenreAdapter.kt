package com.sinhvien.webnovelapp.admin

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.Genre

class GenreAdapter : RecyclerView.Adapter<GenreAdapter.GenreViewHolder>() {

    private var genres = listOf<Genre>()

    fun submitList(newGenres: List<Genre>) {
        genres = newGenres
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): GenreViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_genre, parent, false)
        return GenreViewHolder(view)
    }

    override fun onBindViewHolder(holder: GenreViewHolder, position: Int) {
        holder.bind(genres[position])
    }

    override fun getItemCount() = genres.size

    class GenreViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val tvGenreId: TextView = itemView.findViewById(R.id.tvGenreId)
        private val tvGenreName: TextView = itemView.findViewById(R.id.tvGenreName)
        private val tvGenreDescription: TextView = itemView.findViewById(R.id.tvGenreDescription)
        private val tvGenreIcon: TextView = itemView.findViewById(R.id.tvGenreIcon)
        private val tvGenreColor: TextView = itemView.findViewById(R.id.tvGenreColor)
        private val tvGenreStatus: TextView = itemView.findViewById(R.id.tvGenreStatus)

        fun bind(genre: Genre) {
            tvGenreId.text = "ID: ${genre.Id}"
            tvGenreName.text = genre.Name
            tvGenreDescription.text = genre.Description
            tvGenreIcon.text = "Icon: ${genre.IconClass}"
            tvGenreColor.text = "Color: ${genre.ColorCode}"

            if (genre.IsActive) {
                tvGenreStatus.text = "Active"
                tvGenreStatus.setTextColor(android.graphics.Color.parseColor("#4CAF50"))
                tvGenreStatus.setBackgroundResource(R.drawable.bg_status_active)
            } else {
                tvGenreStatus.text = "Inactive"
                tvGenreStatus.setTextColor(android.graphics.Color.parseColor("#f44336"))
                tvGenreStatus.setBackgroundResource(R.drawable.bg_status_inactive)
            }
        }
    }
}