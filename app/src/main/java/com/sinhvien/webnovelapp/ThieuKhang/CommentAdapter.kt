package com.sinhvien.webnovelapp.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.EditText
import android.widget.ImageButton
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.TextView
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.bumptech.glide.load.engine.DiskCacheStrategy
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.models.CommentResponse
import java.text.SimpleDateFormat
import java.util.*

class CommentAdapter(
    private val onReplyClick: (CommentResponse, EditText, ImageView, () -> Unit) -> Unit,
    private val onLikeClick: (CommentResponse) -> Unit,
    private val onDislikeClick: (CommentResponse) -> Unit,
    private val onImagePickClick: (ImageView) -> Unit,
    private val onPostReplyClick: (CommentResponse, EditText, ImageView) -> Unit,
    private val onCancelReplyClick: () -> Unit,
    private val onEditClick: (CommentResponse) -> Unit,
    private val onDeleteClick: (CommentResponse) -> Unit,
    private val currentUserId: Int
) : ListAdapter<CommentResponse, CommentAdapter.CommentViewHolder>(CommentDiffCallback()) {

    private var currentExpandedPosition: Int? = null

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): CommentViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_comment, parent, false)
        return CommentViewHolder(view)
    }

    override fun onBindViewHolder(holder: CommentViewHolder, position: Int) {
        holder.bind(getItem(position), position)
    }

    inner class CommentViewHolder(itemView: View) : RecyclerView.ViewHolder(itemView) {
        private val tvUsername: TextView = itemView.findViewById(R.id.tvCommentUsername)
        private val tvTime: TextView = itemView.findViewById(R.id.tvCommentTime)
        private val tvContent: TextView = itemView.findViewById(R.id.tvCommentContent)
        private val ivCommentImage: ImageView = itemView.findViewById(R.id.ivCommentImage)
        private val btnLike: ImageButton = itemView.findViewById(R.id.btnLike)
        private val btnDislike: ImageButton = itemView.findViewById(R.id.btnDislike)
        private val tvLikeCount: TextView = itemView.findViewById(R.id.tvLikeCount)
        private val tvDislikeCount: TextView = itemView.findViewById(R.id.tvDislikeCount)
        private val btnReply: TextView = itemView.findViewById(R.id.btnReply)
        private val tvShowReplies: TextView = itemView.findViewById(R.id.tvShowReplies)
        private val llReplies: LinearLayout = itemView.findViewById(R.id.llReplies)

        // Reply input components
        private val llReplyInput: LinearLayout = itemView.findViewById(R.id.llReplyInput)
        private val etReplyInput: EditText = itemView.findViewById(R.id.etReplyInput)
        private val btnReplyAddImage: ImageButton = itemView.findViewById(R.id.btnReplyAddImage)
        private val ivReplySelectedImage: ImageView = itemView.findViewById(R.id.ivReplySelectedImage)
        private val btnCancelReply: Button = itemView.findViewById(R.id.btnCancelReply)
        private val btnPostReply: Button = itemView.findViewById(R.id.btnPostReply)

        fun bind(comment: CommentResponse, position: Int) {
            tvUsername.text = comment.username ?: "Anonymous"
            tvTime.text = " • ${getTimeAgo(comment.createdAt)}"
            tvContent.text = comment.content ?: ""
            tvLikeCount.text = comment.likeCount.toString()
            tvDislikeCount.text = comment.dislikeCount.toString()

            // Load comment image if it exists
            if (comment.hasImage) {
                ivCommentImage.visibility = View.VISIBLE
                val baseUrl = ApiClient.getBaseUrl().trimEnd('/')
                val imageUrl = "$baseUrl/api/comments/${comment.id}/image"

                Glide.with(itemView.context)
                    .load(imageUrl)
                    .diskCacheStrategy(DiskCacheStrategy.ALL)
                    .placeholder(R.drawable.placeholder_novel_cover)
                    .error(R.drawable.placeholder_novel_cover)
                    .into(ivCommentImage)
            } else {
                ivCommentImage.visibility = View.GONE
            }

            btnLike.setOnClickListener { onLikeClick(comment) }
            btnDislike.setOnClickListener { onDislikeClick(comment) }

            // Setup reply button - toggles the reply input visibility
            btnReply.setOnClickListener {
                if (llReplyInput.visibility == View.VISIBLE) {
                    // Hide if already visible
                    hideReplyInput()
                    onCancelReplyClick()
                } else {
                    // Hide previously expanded reply input
                    currentExpandedPosition?.let { prevPos ->
                        notifyItemChanged(prevPos)
                    }

                    // Show reply input for this comment
                    showReplyInput(comment)

                    // Call onReplyClick to set up the context in the activity
                    onReplyClick(comment, etReplyInput, ivReplySelectedImage) {
                        // Callback to hide reply input after posting
                        hideReplyInput()
                        currentExpandedPosition = null
                    }

                    currentExpandedPosition = position
                }
            }

            // Setup reply input actions
            btnCancelReply.setOnClickListener {
                hideReplyInput()
                currentExpandedPosition = null
                onCancelReplyClick()
            }

            // Image button - opens image picker
            btnReplyAddImage.setOnClickListener {
                onImagePickClick(ivReplySelectedImage)
            }

            // Post button - submits the reply
            btnPostReply.setOnClickListener {
                onPostReplyClick(comment, etReplyInput, ivReplySelectedImage)
            }

            // Remove image when clicked
            ivReplySelectedImage.setOnClickListener {
                ivReplySelectedImage.visibility = View.GONE
                ivReplySelectedImage.tag = null
            }

            // Setup options menu (edit/delete)
            val btnCommentOptions: ImageButton = itemView.findViewById(R.id.btnCommentOptions)

            // Only show options if user owns this comment
            if (comment.userId == currentUserId) {
                btnCommentOptions.visibility = View.VISIBLE
                btnCommentOptions.setOnClickListener { view ->
                    showOptionsMenu(view, comment)
                }
            } else {
                btnCommentOptions.visibility = View.GONE
            }

            // Restore state
            if (currentExpandedPosition == position) {
                llReplyInput.visibility = View.VISIBLE
            } else {
                llReplyInput.visibility = View.GONE
                etReplyInput.text.clear()
                ivReplySelectedImage.visibility = View.GONE
                ivReplySelectedImage.tag = null
            }

            // Handle replies
            if (comment.replies.isNullOrEmpty()) {
                tvShowReplies.visibility = View.GONE
                llReplies.visibility = View.GONE
            } else {
                tvShowReplies.visibility = View.VISIBLE
                tvShowReplies.text = "▼ Show ${comment.replies.size} ${if (comment.replies.size == 1) "reply" else "replies"}"

                var repliesExpanded = false
                tvShowReplies.setOnClickListener {
                    repliesExpanded = !repliesExpanded
                    if (repliesExpanded) {
                        llReplies.visibility = View.VISIBLE
                        tvShowReplies.text = "▲ Hide ${comment.replies.size} ${if (comment.replies.size == 1) "reply" else "replies"}"
                        displayReplies(comment.replies)
                    } else {
                        llReplies.visibility = View.GONE
                        tvShowReplies.text = "▼ Show ${comment.replies.size} ${if (comment.replies.size == 1) "reply" else "replies"}"
                    }
                }
            }
        }

        private fun showReplyInput(comment: CommentResponse) {
            llReplyInput.visibility = View.VISIBLE
            etReplyInput.hint = "Reply to ${comment.username}..."
            etReplyInput.requestFocus()

            // Show keyboard
            val imm = itemView.context.getSystemService(android.content.Context.INPUT_METHOD_SERVICE)
                    as android.view.inputmethod.InputMethodManager
            imm.showSoftInput(etReplyInput, android.view.inputmethod.InputMethodManager.SHOW_IMPLICIT)
        }

        private fun hideReplyInput() {
            llReplyInput.visibility = View.GONE
            etReplyInput.text.clear()
            ivReplySelectedImage.visibility = View.GONE
            ivReplySelectedImage.tag = null

            // Hide keyboard
            val imm = itemView.context.getSystemService(android.content.Context.INPUT_METHOD_SERVICE)
                    as android.view.inputmethod.InputMethodManager
            imm.hideSoftInputFromWindow(etReplyInput.windowToken, 0)
        }

        private fun showOptionsMenu(view: View, comment: CommentResponse) {
            val popup = android.widget.PopupMenu(itemView.context, view)
            popup.inflate(R.menu.comment_options_menu)

            popup.setOnMenuItemClickListener { item ->
                when (item.itemId) {
                    R.id.action_edit -> {
                        onEditClick(comment)
                        true
                    }
                    R.id.action_delete -> {
                        onDeleteClick(comment)
                        true
                    }
                    else -> false
                }
            }

            popup.show()
        }

        private fun displayReplies(replies: List<CommentResponse>) {
            llReplies.removeAllViews()
            replies.forEach { reply ->
                val replyView = LayoutInflater.from(itemView.context)
                    .inflate(R.layout.item_comment_reply, llReplies, false)

                val tvReplyUsername = replyView.findViewById<TextView>(R.id.tvCommentUsername)
                val tvReplyTime = replyView.findViewById<TextView>(R.id.tvCommentTime)
                val tvReplyContent = replyView.findViewById<TextView>(R.id.tvCommentContent)
                val ivReplyImage = replyView.findViewById<ImageView>(R.id.ivCommentImage)
                val btnReplyLike = replyView.findViewById<ImageButton>(R.id.btnLike)
                val btnReplyDislike = replyView.findViewById<ImageButton>(R.id.btnDislike)
                val tvReplyLikeCount = replyView.findViewById<TextView>(R.id.tvLikeCount)
                val tvReplyDislikeCount = replyView.findViewById<TextView>(R.id.tvDislikeCount)

                tvReplyUsername.text = reply.username ?: "Anonymous"
                tvReplyTime.text = " • ${getTimeAgo(reply.createdAt)}"
                tvReplyContent.text = reply.content ?: ""
                tvReplyLikeCount.text = reply.likeCount.toString()
                tvReplyDislikeCount.text = reply.dislikeCount.toString()

                // Load reply image if it exists
                if (reply.hasImage) {
                    ivReplyImage.visibility = View.VISIBLE
                    val baseUrl = ApiClient.getBaseUrl().trimEnd('/')
                    val imageUrl = "$baseUrl/api/comments/${reply.id}/image"

                    Glide.with(itemView.context)
                        .load(imageUrl)
                        .diskCacheStrategy(DiskCacheStrategy.ALL)
                        .placeholder(R.drawable.placeholder_novel_cover)
                        .error(R.drawable.placeholder_novel_cover)
                        .into(ivReplyImage)
                } else {
                    ivReplyImage.visibility = View.GONE
                }

                btnReplyLike.setOnClickListener { onLikeClick(reply) }
                btnReplyDislike.setOnClickListener { onDislikeClick(reply) }

                // Setup options menu for replies
                val btnReplyOptions = replyView.findViewById<ImageButton>(R.id.btnCommentOptions)

                // Only show options if user owns this reply
                if (reply.userId == currentUserId) {
                    btnReplyOptions.visibility = View.VISIBLE
                    btnReplyOptions.setOnClickListener { view ->
                        val popup = android.widget.PopupMenu(itemView.context, view)
                        popup.inflate(R.menu.comment_options_menu)

                        popup.setOnMenuItemClickListener { item ->
                            when (item.itemId) {
                                R.id.action_edit -> {
                                    onEditClick(reply)
                                    true
                                }
                                R.id.action_delete -> {
                                    onDeleteClick(reply)
                                    true
                                }
                                else -> false
                            }
                        }

                        popup.show()
                    }
                } else {
                    btnReplyOptions.visibility = View.GONE
                }

                llReplies.addView(replyView)
            }
        }

        private fun getTimeAgo(dateTime: String): String {
            try {
                val sdf = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS", Locale.getDefault())
                val date = sdf.parse(dateTime) ?: return "just now"
                val now = Date()
                val diff = now.time - date.time

                val seconds = diff / 1000
                val minutes = seconds / 60
                val hours = minutes / 60
                val days = hours / 24
                val weeks = days / 7
                val months = days / 30
                val years = days / 365

                return when {
                    years > 0 -> "${years}y ago"
                    months > 0 -> "${months}mo ago"
                    weeks > 0 -> "${weeks}w ago"
                    days > 0 -> "${days}d ago"
                    hours > 0 -> "${hours}h ago"
                    minutes > 0 -> "${minutes}m ago"
                    else -> "just now"
                }
            } catch (e: Exception) {
                return "recently"
            }
        }
    }

    class CommentDiffCallback : DiffUtil.ItemCallback<CommentResponse>() {
        override fun areItemsTheSame(oldItem: CommentResponse, newItem: CommentResponse): Boolean {
            return oldItem.id == newItem.id
        }

        override fun areContentsTheSame(oldItem: CommentResponse, newItem: CommentResponse): Boolean {
            return oldItem == newItem
        }
    }
}