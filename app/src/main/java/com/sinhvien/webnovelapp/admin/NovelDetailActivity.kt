package com.sinhvien.webnovelapp.activities

import android.content.Intent
import android.os.Bundle
import android.view.MenuItem
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.ImageButton
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.bumptech.glide.load.engine.DiskCacheStrategy
import com.google.android.material.chip.Chip
import com.google.android.material.chip.ChipGroup
import com.google.android.material.floatingactionbutton.FloatingActionButton
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.adapters.ChapterSummaryAdapter
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.GenreInfo
import com.sinhvien.webnovelapp.models.TagInfo
import com.sinhvien.webnovelapp.models.NovelDetailResponse
import com.sinhvien.webnovelapp.models.DeleteCommentRequest
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import androidx.lifecycle.lifecycleScope
import com.sinhvien.webnovelapp.adapters.CommentAdapter
import com.sinhvien.webnovelapp.api.CommentApiService
import com.sinhvien.webnovelapp.models.CommentResponse
import com.sinhvien.webnovelapp.models.VoteCommentRequest
import kotlinx.coroutines.launch
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaTypeOrNull

class NovelDetailActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var chapterAdapter: ChapterSummaryAdapter
    private lateinit var synopsisGradient: View

    // UI Components
    private lateinit var ivNovelCover: ImageView
    private lateinit var ivNovelCoverLarge: ImageView
    private lateinit var tvNovelTitle: TextView
    private lateinit var tvNovelAuthor: TextView
    private lateinit var tvNovelStatus: TextView
    private lateinit var tvNovelRating: TextView
    private lateinit var tvNovelStats: TextView
    private lateinit var tvNovelViews: TextView
    private lateinit var tvNovelSynopsis: TextView
    private lateinit var btnSeeMore: Button
    private lateinit var chipGroupGenres: ChipGroup
    private lateinit var chipGroupTags: ChipGroup
    private lateinit var rvRecentChapters: RecyclerView
    private lateinit var btnViewAllChapters: Button
    private lateinit var fabBookmark: FloatingActionButton

    private lateinit var commentAdapter: CommentAdapter
    private lateinit var rvComments: RecyclerView
    private lateinit var etComment: EditText
    private lateinit var btnPostComment: Button
    private lateinit var btnAddImage: ImageButton
    private lateinit var ivSelectedImage: ImageView
    private lateinit var btnLoadMoreComments: Button
    private lateinit var progressBarComments: ProgressBar
    private lateinit var commentApiService: CommentApiService

    private var selectedImageUri: android.net.Uri? = null
    private var currentCommentPage = 1
    private var totalCommentPages = 1
    private var replyToCommentId: Int? = null

    private var novelId: Int = 0
    private var isBookmarked = false
    private var isFullSynopsisShown = false
    private var fullSynopsisText = ""

    private var currentReplyInput: EditText? = null
    private var currentReplyImageView: ImageView? = null
    private var currentReplyImageUri: android.net.Uri? = null
    private var currentReplyCallback: (() -> Unit)? = null

    private val IMAGE_PICK_CODE = 1000
    private val IMAGE_PICK_CODE_REPLY = 1001
    private val allComments = mutableListOf<CommentResponse>()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_novel_detail)

        novelId = intent.getIntExtra("NOVEL_ID", 0)

        if (novelId == 0) {
            Toast.makeText(this, "Invalid novel ID", Toast.LENGTH_SHORT).show()
            finish()
            return
        }

        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = ""
        } catch (e: Exception) {
            // Toolbar setup failed
        }

        apiService = ApiClient.getClient().create(NovelApiService::class.java)
        commentApiService = ApiClient.getClient().create(CommentApiService::class.java)

        initializeViews()
        setupRecyclerView()
        setupClickListeners()
        loadNovelDetail()
    }

    private fun initializeViews() {
        ivNovelCover = findViewById(R.id.ivNovelCover)
        ivNovelCoverLarge = findViewById(R.id.ivNovelCoverLarge)
        tvNovelTitle = findViewById(R.id.tvNovelTitle)
        tvNovelAuthor = findViewById(R.id.tvNovelAuthor)
        tvNovelStatus = findViewById(R.id.tvNovelStatus)
        tvNovelRating = findViewById(R.id.tvNovelRating)
        tvNovelStats = findViewById(R.id.tvNovelStats)
        tvNovelViews = findViewById(R.id.tvNovelViews)
        tvNovelSynopsis = findViewById(R.id.tvNovelSynopsis)
        btnSeeMore = findViewById(R.id.btnSeeMore)
        chipGroupTags = findViewById(R.id.chipGroupTags)
        rvRecentChapters = findViewById(R.id.rvRecentChapters)
        btnViewAllChapters = findViewById(R.id.btnViewAllChapters)
        fabBookmark = findViewById(R.id.fabBookmark)
        synopsisGradient = findViewById(R.id.synopsisGradient)

        rvComments = findViewById(R.id.rvComments)
        etComment = findViewById(R.id.etComment)
        btnPostComment = findViewById(R.id.btnPostComment)
        btnAddImage = findViewById(R.id.btnAddImage)
        ivSelectedImage = findViewById(R.id.ivSelectedImage)
        btnLoadMoreComments = findViewById(R.id.btnLoadMoreComments)
        progressBarComments = findViewById(R.id.progressBarComments)
    }

    private fun setupRecyclerView() {
        chapterAdapter = ChapterSummaryAdapter { chapter ->
            val intent = Intent(this, ChapterReaderActivity::class.java)
            intent.putExtra("NOVEL_ID", novelId)
            intent.putExtra("CHAPTER_NUMBER", chapter.ChapterNumber)
            startActivity(intent)
        }

        rvRecentChapters.apply {
            layoutManager = LinearLayoutManager(this@NovelDetailActivity)
            adapter = chapterAdapter
            isNestedScrollingEnabled = false
        }

        commentAdapter = CommentAdapter(
            onReplyClick = { comment, replyInput, imageView, onComplete ->
                replyToCommentId = comment.id
                currentReplyInput = replyInput
                currentReplyImageView = imageView
                currentReplyCallback = onComplete
            },
            onLikeClick = { comment ->
                lifecycleScope.launch {
                    likeComment(comment.id)
                }
            },
            onDislikeClick = { comment ->
                lifecycleScope.launch {
                    dislikeComment(comment.id)
                }
            },
            onImagePickClick = { imageView ->
                currentReplyImageView = imageView
                selectImageForReply()
            },
            onPostReplyClick = { comment, replyInput, imageView ->
                replyToCommentId = comment.id
                currentReplyInput = replyInput
                currentReplyImageView = imageView
                currentReplyImageUri = imageView.tag as? android.net.Uri
                postComment()
            },
            onCancelReplyClick = {
                replyToCommentId = null
                currentReplyInput = null
                currentReplyImageView = null
                currentReplyImageUri = null
                currentReplyCallback = null
            },
            onEditClick = { comment ->  // ADD THIS
                showEditCommentDialog(comment)
            },
            onDeleteClick = { comment ->  // ADD THIS
                showDeleteConfirmDialog(comment)
            },
            currentUserId = 13  // ADD THIS (your hardcoded user ID)
        )

        rvComments.apply {
            layoutManager = LinearLayoutManager(this@NovelDetailActivity)
            adapter = commentAdapter
            isNestedScrollingEnabled = false
        }
    }

    private fun selectImageForReply() {
        val intent = Intent(Intent.ACTION_PICK)
        intent.type = "image/*"
        startActivityForResult(intent, IMAGE_PICK_CODE_REPLY)
    }

    private fun setupClickListeners() {
        btnViewAllChapters.setOnClickListener {
            val intent = Intent(this, ChapterListActivity::class.java)
            intent.putExtra("NOVEL_ID", novelId)
            startActivity(intent)
        }

        fabBookmark.setOnClickListener {
            toggleBookmark()
        }

        btnSeeMore.setOnClickListener {
            toggleSynopsisExpansion()
        }

        btnPostComment.setOnClickListener {
            postComment()
        }

        btnAddImage.setOnClickListener {
            selectImage()
        }

        btnLoadMoreComments.setOnClickListener {
            currentCommentPage++
            loadComments()
        }

        ivSelectedImage.setOnClickListener {
            selectedImageUri = null
            ivSelectedImage.visibility = View.GONE
        }
    }

    private fun loadNovelDetail() {
        val call = apiService.getNovelDetail(novelId)

        call.enqueue(object : Callback<NovelDetailResponse> {
            override fun onResponse(call: Call<NovelDetailResponse>, response: Response<NovelDetailResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val novelResponse = response.body()!!
                    if (novelResponse.Success) {
                        displayNovelDetail(novelResponse.Data)
                    } else {
                        showError("Failed to load novel details")
                    }
                } else {
                    showError("HTTP ${response.code()}: ${response.message()}")
                }
            }

            override fun onFailure(call: Call<NovelDetailResponse>, t: Throwable) {
                showError("Network Error: ${t.message}")
            }
        })

        loadComments()
    }

    private fun loadComments() {
        lifecycleScope.launch {
            try {
                progressBarComments.visibility = View.VISIBLE
                val response = commentApiService.getNovelComments(novelId, currentCommentPage, 10)

                if (response.isSuccessful && response.body() != null) {
                    val paginatedResponse = response.body()!!
                    if (paginatedResponse.success && paginatedResponse.data != null) {
                        // If it's the first page, replace all comments
                        if (currentCommentPage == 1) {
                            allComments.clear()
                            allComments.addAll(paginatedResponse.data)
                        } else {
                            // Otherwise, append new comments
                            allComments.addAll(paginatedResponse.data)
                        }

                        // Submit the accumulated list
                        commentAdapter.submitList(allComments.toList())

                        totalCommentPages = paginatedResponse.totalPages

                        // Show/hide "Load More" button
                        btnLoadMoreComments.visibility = if (currentCommentPage < totalCommentPages) {
                            View.VISIBLE
                        } else {
                            View.GONE
                        }

                        // Update button text with page info
                        btnLoadMoreComments.text = "Load More (Page $currentCommentPage of $totalCommentPages)"
                    }
                }
            } catch (e: Exception) {
                Toast.makeText(this@NovelDetailActivity, "Failed to load comments: ${e.message}", Toast.LENGTH_SHORT).show()
            } finally {
                progressBarComments.visibility = View.GONE
            }
        }
    }

    private fun postComment() {
        val content: String
        val imageUri: android.net.Uri?

        // Check if we're posting from inline reply or main input
        if (currentReplyInput != null && replyToCommentId != null) {
            content = currentReplyInput?.text.toString().trim()
            imageUri = currentReplyImageUri
        } else {
            content = etComment.text.toString().trim()
            imageUri = selectedImageUri
        }

        if (content.isEmpty() && imageUri == null) {
            Toast.makeText(this, "Please write something or add an image", Toast.LENGTH_SHORT).show()
            return
        }

        lifecycleScope.launch {
            try {
                progressBarComments.visibility = View.VISIBLE

                val userId = okhttp3.RequestBody.create("text/plain".toMediaTypeOrNull(), "13")
                val novelIdBody = okhttp3.RequestBody.create("text/plain".toMediaTypeOrNull(), novelId.toString())
                val contentBody = okhttp3.RequestBody.create("text/plain".toMediaTypeOrNull(), content)
                val parentCommentIdBody = replyToCommentId?.let {
                    okhttp3.RequestBody.create("text/plain".toMediaTypeOrNull(), it.toString())
                }

                val imagePart = imageUri?.let { uri ->
                    val inputStream = contentResolver.openInputStream(uri)
                    val bytes = inputStream?.readBytes()
                    inputStream?.close()

                    bytes?.let {
                        val requestFile = okhttp3.RequestBody.create("image/*".toMediaTypeOrNull(), it)
                        okhttp3.MultipartBody.Part.createFormData("image", "comment_image.jpg", requestFile)
                    }
                }

                val response = commentApiService.createComment(
                    userId,
                    novelIdBody,
                    null,
                    parentCommentIdBody,
                    contentBody,
                    imagePart
                )

                if (response.isSuccessful && response.body()?.success == true) {
                    Toast.makeText(this@NovelDetailActivity, "Comment posted!", Toast.LENGTH_SHORT).show()

                    // Clear main input
                    etComment.text.clear()
                    selectedImageUri = null
                    ivSelectedImage.visibility = View.GONE

                    // Clear inline reply input and trigger callback
                    currentReplyInput?.text?.clear()
                    currentReplyImageView?.let {
                        it.visibility = View.GONE
                        it.tag = null
                    }
                    currentReplyCallback?.invoke()

                    // Reset state
                    replyToCommentId = null
                    currentReplyInput = null
                    currentReplyImageView = null
                    currentReplyImageUri = null
                    currentReplyCallback = null

                    currentCommentPage = 1
                    loadComments()
                } else {
                    Toast.makeText(this@NovelDetailActivity, "Failed to post comment", Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@NovelDetailActivity, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
            } finally {
                progressBarComments.visibility = View.GONE
            }
        }
    }

    private fun selectImage() {
        val intent = Intent(Intent.ACTION_PICK)
        intent.type = "image/*"
        startActivityForResult(intent, IMAGE_PICK_CODE)
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        super.onActivityResult(requestCode, resultCode, data)

        if (resultCode == RESULT_OK && data?.data != null) {
            when (requestCode) {
                IMAGE_PICK_CODE -> {
                    // Main comment image
                    selectedImageUri = data.data
                    ivSelectedImage.visibility = View.VISIBLE
                    Glide.with(this)
                        .load(selectedImageUri)
                        .into(ivSelectedImage)
                }
                IMAGE_PICK_CODE_REPLY -> {
                    // Reply comment image
                    val uri = data.data
                    currentReplyImageUri = uri
                    currentReplyImageView?.let { imageView ->
                        imageView.visibility = View.VISIBLE
                        imageView.tag = uri // Store URI in tag
                        Glide.with(this)
                            .load(uri)
                            .into(imageView)
                    }
                }
            }
        }
    }

    private suspend fun likeComment(commentId: Int) {
        try {
            val request = VoteCommentRequest(userId = 13)
            val response = commentApiService.likeComment(commentId, request)

            if (response.isSuccessful) {
                currentCommentPage = 1
                loadComments()
            }
        } catch (e: Exception) {
            withContext(Dispatchers.Main) {
                Toast.makeText(this@NovelDetailActivity, "Failed to like comment", Toast.LENGTH_SHORT).show()
            }
        }
    }

    private suspend fun dislikeComment(commentId: Int) {
        try {
            val request = VoteCommentRequest(userId = 13)
            val response = commentApiService.dislikeComment(commentId, request)

            if (response.isSuccessful) {
                currentCommentPage = 1
                loadComments()
            }
        } catch (e: Exception) {
            withContext(Dispatchers.Main) {
                Toast.makeText(this@NovelDetailActivity, "Failed to dislike comment", Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun showEditCommentDialog(comment: CommentResponse) {
        val dialogView = layoutInflater.inflate(R.layout.dialog_edit_comment, null)
        val etEditContent = dialogView.findViewById<EditText>(R.id.etEditContent)
        val btnSave = dialogView.findViewById<Button>(R.id.btnSave)
        val btnCancel = dialogView.findViewById<Button>(R.id.btnCancel)

        etEditContent.setText(comment.content)

        // Create dialog without title and default buttons
        val dialog = android.app.AlertDialog.Builder(this)
            .setView(dialogView)
            .create()

        // Set transparent background to remove white corners
        dialog.window?.setBackgroundDrawableResource(android.R.color.transparent)

        // Handle custom button clicks
        btnSave.setOnClickListener {
            val newContent = etEditContent.text.toString().trim()
            if (newContent.isNotEmpty()) {
                editComment(comment.id, newContent)
                dialog.dismiss()
            } else {
                Toast.makeText(this, "Comment cannot be empty", Toast.LENGTH_SHORT).show()
            }
        }

        btnCancel.setOnClickListener {
            dialog.dismiss()
        }

        dialog.show()
    }

    private fun showDeleteConfirmDialog(comment: CommentResponse) {
        android.app.AlertDialog.Builder(this)
            .setTitle("Delete Comment")
            .setMessage("Are you sure you want to delete this comment?")
            .setPositiveButton("Delete") { _, _ ->
                deleteComment(comment.id)
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun editComment(commentId: Int, newContent: String) {
        lifecycleScope.launch {
            try {
                progressBarComments.visibility = View.VISIBLE

                val userId = okhttp3.RequestBody.create(
                    "text/plain".toMediaTypeOrNull(),
                    "13"
                )

                val contentBody = okhttp3.RequestBody.create(
                    "text/plain".toMediaTypeOrNull(),
                    newContent
                )

                // No image change, so pass null for image and removeImage
                val response = commentApiService.updateComment(
                    commentId = commentId,
                    userId = userId,
                    content = contentBody,
                    image = null,
                    removeImage = null
                )

                if (response.isSuccessful) {
                    Toast.makeText(this@NovelDetailActivity, "Comment updated!", Toast.LENGTH_SHORT).show()
                    currentCommentPage = 1
                    loadComments()
                } else {
                    Toast.makeText(this@NovelDetailActivity, "Failed to update comment", Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@NovelDetailActivity, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
            } finally {
                progressBarComments.visibility = View.GONE
            }
        }
    }

    private fun deleteComment(commentId: Int) {
        lifecycleScope.launch {
            try {
                progressBarComments.visibility = View.VISIBLE

                val request = DeleteCommentRequest(userId = 13)

                val response = commentApiService.deleteComment(commentId, request)

                if (response.isSuccessful) {
                    Toast.makeText(this@NovelDetailActivity, "Comment deleted!", Toast.LENGTH_SHORT).show()
                    currentCommentPage = 1
                    loadComments()
                } else {
                    Toast.makeText(this@NovelDetailActivity, "Failed to delete comment", Toast.LENGTH_SHORT).show()
                }
            } catch (e: Exception) {
                Toast.makeText(this@NovelDetailActivity, "Error: ${e.message}", Toast.LENGTH_SHORT).show()
            } finally {
                progressBarComments.visibility = View.GONE
            }
        }
    }

    private fun displayNovelDetail(novel: com.sinhvien.webnovelapp.models.NovelDetail) {
        tvNovelTitle.text = novel.Title
        supportActionBar?.title = novel.Title

        val authorText = if (novel.Author?.IsVerified == true) {
            "by ${novel.Author.PenName} ✓"
        } else {
            "by ${novel.Author?.PenName ?: "Unknown Author"}"
        }
        tvNovelAuthor.text = authorText

        tvNovelStatus.text = novel.Status
        when (novel.Status.lowercase()) {
            "ongoing" -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#000000"))
                tvNovelStatus.setBackgroundResource(R.drawable.bg_status_active)
            }
            "completed" -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#000000"))
            }
            else -> {
                tvNovelStatus.setTextColor(android.graphics.Color.parseColor("#000000"))
            }
        }

        tvNovelRating.text = "★ ${String.format("%.2f", novel.AverageRating)} (${novel.TotalRatings} ratings)"
        tvNovelStats.text = "${novel.TotalChapters} chapters • ${formatNumber(novel.WordCount)} words"
        tvNovelViews.text = "${formatNumber(novel.ViewCount)} views • ${formatNumber(novel.BookmarkCount)} bookmarks"

        fullSynopsisText = novel.Synopsis ?: "No synopsis available."
        displaySynopsisTruncated()

        displayGenres(novel.Genres)
        displayTags(novel.Tags)
        chapterAdapter.submitList(novel.RecentChapters)

        val coverImageUrl = "${ApiClient.getBaseUrl()}api/novels/${novel.Id}/cover"

        Glide.with(this)
            .load(coverImageUrl)
            .placeholder(R.drawable.placeholder_novel_cover)
            .error(R.drawable.placeholder_novel_cover)
            .diskCacheStrategy(DiskCacheStrategy.ALL)
            .into(ivNovelCover)

        Glide.with(this)
            .load(coverImageUrl)
            .placeholder(R.drawable.placeholder_novel_cover)
            .error(R.drawable.placeholder_novel_cover)
            .diskCacheStrategy(DiskCacheStrategy.ALL)
            .into(ivNovelCoverLarge)
    }

    private fun displaySynopsisTruncated() {
        val maxLines = 4
        tvNovelSynopsis.maxLines = maxLines
        tvNovelSynopsis.text = fullSynopsisText

        tvNovelSynopsis.post {
            val lineCount = tvNovelSynopsis.lineCount
            if (lineCount >= maxLines || fullSynopsisText.length > 200) {
                btnSeeMore.visibility = View.VISIBLE
                synopsisGradient.visibility = View.VISIBLE
                btnSeeMore.text = "See more ▼"
                isFullSynopsisShown = false
            } else {
                btnSeeMore.visibility = View.GONE
                synopsisGradient.visibility = View.GONE
            }
        }
    }

    private fun toggleSynopsisExpansion() {
        if (isFullSynopsisShown) {
            tvNovelSynopsis.maxLines = 4
            synopsisGradient.visibility = View.VISIBLE
            btnSeeMore.text = "See more ▼"
            isFullSynopsisShown = false
        } else {
            tvNovelSynopsis.maxLines = Int.MAX_VALUE
            synopsisGradient.visibility = View.GONE
            btnSeeMore.text = "See less ▲"
            isFullSynopsisShown = true
        }
    }

    private fun displayGenres(genres: List<GenreInfo>?) {
        val genresContainer = findViewById<LinearLayout>(R.id.genresContainer)
        genresContainer.removeAllViews()

        genres?.forEach { genre ->
            val genreChip = TextView(this).apply {
                text = genre.Name
                textSize = 12f
                setTextColor(android.graphics.Color.BLACK)
                typeface = android.graphics.Typeface.DEFAULT_BOLD
                setPadding(32, 16, 32, 16)

                try {
                    val baseColor = android.graphics.Color.parseColor(genre.ColorCode ?: "#4A90E2")
                    val alpha = 230
                    val transparentColor = android.graphics.Color.argb(
                        alpha,
                        android.graphics.Color.red(baseColor),
                        android.graphics.Color.green(baseColor),
                        android.graphics.Color.blue(baseColor)
                    )
                    setBackgroundColor(transparentColor)
                } catch (e: Exception) {
                    setBackgroundColor(android.graphics.Color.parseColor("#4A90E2"))
                }

                background = createRoundedBackground(genre.ColorCode ?: "#4A90E2")

                val params = LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.WRAP_CONTENT,
                    LinearLayout.LayoutParams.WRAP_CONTENT
                )
                params.setMargins(0, 0, 16, 0)
                layoutParams = params
                elevation = 2f
            }

            genresContainer.addView(genreChip)
        }
    }

    private fun createRoundedBackground(colorCode: String): android.graphics.drawable.GradientDrawable {
        val shape = android.graphics.drawable.GradientDrawable()
        shape.shape = android.graphics.drawable.GradientDrawable.RECTANGLE
        shape.cornerRadius = 20f

        try {
            val baseColor = android.graphics.Color.parseColor(colorCode)
            val alpha = 230
            val transparentColor = android.graphics.Color.argb(
                alpha,
                android.graphics.Color.red(baseColor),
                android.graphics.Color.green(baseColor),
                android.graphics.Color.blue(baseColor)
            )
            shape.setColor(transparentColor)
        } catch (e: Exception) {
            shape.setColor(android.graphics.Color.parseColor("#4A90E2"))
        }

        return shape
    }

    private fun displayTags(tags: List<TagInfo>?) {
        chipGroupTags.removeAllViews()

        tags?.forEach { tag ->
            try {
                val chip = Chip(this)
                chip.text = tag.Name
                chip.setTextColor(android.graphics.Color.parseColor("#FFFFFF"))
                chip.setChipBackgroundColor(android.content.res.ColorStateList.valueOf(
                    android.graphics.Color.parseColor("#3d3d3d")
                ))
                chip.chipStrokeWidth = 1f
                chip.chipStrokeColor = android.content.res.ColorStateList.valueOf(
                    android.graphics.Color.parseColor("#555555")
                )
                chip.isClickable = false
                chip.isCheckable = false
                chipGroupTags.addView(chip)
            } catch (e: Exception) {
                val textView = TextView(this)
                textView.text = tag.Name
                textView.textSize = 12f
                textView.setTextColor(android.graphics.Color.parseColor("#FFFFFF"))
                textView.setPadding(24, 12, 24, 12)
                textView.setBackgroundResource(R.drawable.bg_tag_chip)

                val params = LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.WRAP_CONTENT,
                    LinearLayout.LayoutParams.WRAP_CONTENT
                )
                params.setMargins(12, 12, 12, 12)
                textView.layoutParams = params
                chipGroupTags.addView(textView)
            }
        }
    }

    private fun toggleBookmark() {
        isBookmarked = !isBookmarked

        if (isBookmarked) {
            fabBookmark.setImageResource(android.R.drawable.btn_star_big_on)
            Toast.makeText(this, "Added to bookmarks", Toast.LENGTH_SHORT).show()
        } else {
            fabBookmark.setImageResource(android.R.drawable.btn_star_big_off)
            Toast.makeText(this, "Removed from bookmarks", Toast.LENGTH_SHORT).show()
        }
    }

    private fun formatNumber(number: Long): String {
        return when {
            number >= 1_000_000 -> String.format("%.1fM", number / 1_000_000.0)
            number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
            else -> number.toString()
        }
    }

    private fun showError(message: String) {
        Toast.makeText(this, message, Toast.LENGTH_LONG).show()
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            android.R.id.home -> {
                onBackPressed()
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }
}