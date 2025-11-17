package com.sinhvien.webnovelapp.admin

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.models.Tag

class TagAdapter : RecyclerView.Adapter<TagAdapter.TagViewHolder>() {

    private var tags = listOf<Tag>()

    fun submitList(newTags: List<Tag>) {
        tags = newTags
        notifyDataSetChanged()
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): TagViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_tag, parent, false)
        return TagViewHolder(view)
    }

    override fun onBindViewHolder(holder: TagViewHolder, position: Int) {
        holder.bind(tags[position])
    }

    override fun getItemCount() = tags.size

    class TagViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val tvTagId: TextView = itemView.findViewById(R.id.tvTagId)
        private val tvTagName: TextView = itemView.findViewById(R.id.tvTagName)
        private val tvTagDescription: TextView = itemView.findViewById(R.id.tvTagDescription)
        private val tvTagColor: TextView = itemView.findViewById(R.id.tvTagColor)
        private val tvTagCreatedAt: TextView = itemView.findViewById(R.id.tvTagCreatedAt)
        private val tvTagStatus: TextView = itemView.findViewById(R.id.tvTagStatus)

        fun bind(tag: Tag) {
            tvTagId.text = "ID: ${tag.Id}"
            tvTagName.text = tag.Name
            tvTagDescription.text = tag.Description ?: "No description"
            tvTagColor.text = "Color: ${tag.Color ?: "N/A"}"
            tvTagCreatedAt.text = "Created: ${tag.CreatedAt ?: "Unknown"}"

            if (tag.IsActive) {
                tvTagStatus.text = "Active"
                tvTagStatus.setTextColor(android.graphics.Color.parseColor("#4CAF50"))
                tvTagStatus.setBackgroundResource(R.drawable.bg_status_active)
            } else {
                tvTagStatus.text = "Inactive"
                tvTagStatus.setTextColor(android.graphics.Color.parseColor("#f44336"))
                tvTagStatus.setBackgroundResource(R.drawable.bg_status_inactive)
            }
        }
    }
}