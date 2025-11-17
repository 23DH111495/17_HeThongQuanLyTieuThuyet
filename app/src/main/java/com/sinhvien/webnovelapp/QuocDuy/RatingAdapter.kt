package com.sinhvien.webnovelapp.adapters

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.databinding.ItemRatingBinding
import com.sinhvien.webnovelapp.models.Rating

class RatingAdapter(private var ratings: List<Rating>) :
    RecyclerView.Adapter<RatingAdapter.RatingViewHolder>() {

    inner class RatingViewHolder(val binding: ItemRatingBinding) :
        RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RatingViewHolder {
        val binding = ItemRatingBinding.inflate(LayoutInflater.from(parent.context), parent, false)
        return RatingViewHolder(binding)
    }

    override fun onBindViewHolder(holder: RatingViewHolder, position: Int) {
        val item = ratings[position]
        holder.binding.tvReader.text = item.readerName ?: "Unknown"
        holder.binding.ratingBar.rating = item.ratingValue.toFloat()
    }

    override fun getItemCount() = ratings.size

    fun updateList(newList: List<Rating>) {
        ratings = newList
        notifyDataSetChanged()
    }
}
