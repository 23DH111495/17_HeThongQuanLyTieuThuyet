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
import com.sinhvien.webnovelapp.CongThuong.Login.TokenManager
import kotlinx.coroutines.launch
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import org.json.JSONObject
import android.content.Context
import android.util.Log
import com.sinhvien.webnovelapp.api.UnlockedChapterInfo
import com.sinhvien.webnovelapp.models.ChapterListResponse

class NovelDetailActivity : AppCompatActivity() {

    private lateinit var apiService: NovelApiService
    private lateinit var chapterAdapter: ChapterSummaryAdapter
    private lateinit var synopsisGradient: View
    private lateinit var tokenManager: TokenManager

    // flag to track activity state
    private var isActivityDestroyed = false

    // Add call management
    private var novelDetailCall: Call<NovelDetailResponse>? = null

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
    private var currentUserId: Int? = null

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
        isActivityDestroyed = false

        // Initialize TokenManager
        tokenManager = TokenManager.getInstance(this)

        // Get user ID from token
        currentUserId = getUserIdFromToken()

        // Fallback: If token decode fails, try to get from SharedPreferences directly
        if (currentUserId == null) {
            val prefs = getSharedPreferences("WebNovelAppPrefs", Context.MODE_PRIVATE)
            currentUserId = prefs.getInt("USER_ID", -1).takeIf { it != -1 }
            android.util.Log.d("NovelDetailActivity", "Using fallback userId: $currentUserId")
        }

        android.util.Log.d("NovelDetailActivity", "Final currentUserId: $currentUserId")

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

        loadNovelRating()//DUYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

    }

    override fun onDestroy() {
        super.onDestroy()
        isActivityDestroyed = true

        // Cancel any pending API calls
        novelDetailCall?.cancel()
    }

    private fun getUserIdFromToken(): Int? {
        val token = tokenManager.getToken()

        // Debug: Log token status
        android.util.Log.d("NovelDetailActivity", "Token exists: ${token != null}")

        if (token == null) {
            Toast.makeText(this, "Please login to interact with comments", Toast.LENGTH_LONG).show()
            return null
        }

        return try {
            // Decode JWT token to get user ID
            val parts = token.split(".")

            android.util.Log.d("NovelDetailActivity", "Token parts count: ${parts.size}")

            if (parts.size != 3) {
                android.util.Log.e("NovelDetailActivity", "Invalid token format")
                return null
            }

            val payload = String(android.util.Base64.decode(parts[1], android.util.Base64.URL_SAFE))
            val jsonObject = JSONObject(payload)

            // Debug: Log the entire payload
            android.util.Log.d("NovelDetailActivity", "Token payload: $payload")

            // Try different possible claim names for user ID
            // Backend uses: ClaimTypes.NameIdentifier which translates to:
            // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            val userId = when {
                jsonObject.has("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") ->
                    jsonObject.getString("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").toIntOrNull()
                jsonObject.has("nameid") -> jsonObject.getString("nameid").toIntOrNull()
                jsonObject.has("userId") -> jsonObject.getInt("userId")
                jsonObject.has("sub") -> jsonObject.getString("sub").toIntOrNull()
                jsonObject.has("id") -> jsonObject.getInt("id")
                jsonObject.has("user_id") -> jsonObject.getInt("user_id")
                jsonObject.has("UserID") -> jsonObject.getInt("UserID")
                jsonObject.has("uid") -> jsonObject.getInt("uid")
                else -> {
                    android.util.Log.e("NovelDetailActivity", "No userId field found in token")
                    android.util.Log.e("NovelDetailActivity", "Available keys: ${jsonObject.keys().asSequence().toList()}")
                    null
                }
            }

            android.util.Log.d("NovelDetailActivity", "Extracted userId: $userId")
            userId

        } catch (e: Exception) {
            android.util.Log.e("NovelDetailActivity", "Error decoding token", e)
            e.printStackTrace()
            null
        }
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
                if (currentUserId == null) {
                    Toast.makeText(this, "Please login to reply", Toast.LENGTH_SHORT).show()
                    return@CommentAdapter
                }
                replyToCommentId = comment.id
                currentReplyInput = replyInput
                currentReplyImageView = imageView
                currentReplyCallback = onComplete
            },
            onLikeClick = { comment ->
                if (currentUserId == null) {
                    Toast.makeText(this, "Please login to like comments", Toast.LENGTH_SHORT).show()
                    return@CommentAdapter
                }
                lifecycleScope.launch {
                    likeComment(comment.id)
                }
            },
            onDislikeClick = { comment ->
                if (currentUserId == null) {
                    Toast.makeText(this, "Please login to dislike comments", Toast.LENGTH_SHORT).show()
                    return@CommentAdapter
                }
                lifecycleScope.launch {
                    dislikeComment(comment.id)
                }
            },
            onImagePickClick = { imageView ->
                if (currentUserId == null) {
                    Toast.makeText(this, "Please login to add images", Toast.LENGTH_SHORT).show()
                    return@CommentAdapter
                }
                currentReplyImageView = imageView
                selectImageForReply()
            },
            onPostReplyClick = { comment, replyInput, imageView ->
                if (currentUserId == null) {
                    Toast.makeText(this, "Please login to post replies", Toast.LENGTH_SHORT).show()
                    return@CommentAdapter
                }
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
            onEditClick = { comment ->
                if (currentUserId == null) {
                    Toast.makeText(this, "Please login to edit comments", Toast.LENGTH_SHORT).show()
                    return@CommentAdapter
                }
                showEditCommentDialog(comment)
            },
            onDeleteClick = { comment ->
                if (currentUserId == null) {
                    Toast.makeText(this, "Please login to delete comments", Toast.LENGTH_SHORT).show()
                    return@CommentAdapter
                }
                showDeleteConfirmDialog(comment)
            },
            currentUserId = currentUserId ?: -1
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
            if (currentUserId == null) {
                Toast.makeText(this, "Please login to post comments", Toast.LENGTH_SHORT).show()
                // Optional: redirect to login
                // val intent = Intent(this, LoginActivity::class.java)
                // startActivity(intent)
                return@setOnClickListener
            }
            postComment()
        }

        btnAddImage.setOnClickListener {
            if (currentUserId == null) {
                Toast.makeText(this, "Please login to add images", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }
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


        tvNovelRating.setOnClickListener {
            showRatingDialog()
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

                        // Load chapters with proper unlock status
                        loadChaptersWithUnlockStatus()
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

    private fun loadChaptersWithUnlockStatus() {
        val userId = currentUserId ?: return

        // Load all chapters with userId to get unlock status from API
        val chaptersCall = apiService.getNovelChapters(
            novelId = novelId,
            page = 1,
            pageSize = 50,
            userId = userId // API returns correct IsUnlocked status
        )

        chaptersCall.enqueue(object : Callback<ChapterListResponse> {
            override fun onResponse(
                call: Call<ChapterListResponse>,
                response: Response<ChapterListResponse>
            ) {
                if (response.isSuccessful && response.body() != null) {
                    val chapterResponse = response.body()!!
                    if (chapterResponse.Success) {
                        // Get recent chapters (last 12, newest first)
                        val recentChapters = chapterResponse.Data
                            .sortedByDescending { it.ChapterNumber }
                            .take(12)

                        // Update the adapter - API already has correct IsUnlocked status
                        chapterAdapter.submitList(recentChapters)

                        Log.d("NovelDetail", "Loaded ${recentChapters.size} chapters with correct unlock status")
                    }
                }
            }

            override fun onFailure(call: Call<ChapterListResponse>, t: Throwable) {
                Log.e("NovelDetail", "Failed to load chapters: ${t.message}")
            }
        })
    }

    private fun loadChaptersWithoutUnlockInfo() {
        val call = apiService.getNovelChapters(novelId, 1, 50)

        call.enqueue(object : Callback<ChapterListResponse> {
            override fun onResponse(call: Call<ChapterListResponse>, response: Response<ChapterListResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val chapterResponse = response.body()!!
                    if (chapterResponse.Success) {
                        val recentChapters = chapterResponse.Data
                            .sortedByDescending { it.ChapterNumber }
                            .take(12)

                        chapterAdapter.submitList(recentChapters)
                    }
                }
            }

            override fun onFailure(call: Call<ChapterListResponse>, t: Throwable) {
                Log.e("NovelDetail", "Failed to load chapters: ${t.message}")
            }
        })
    }

    private fun loadComments() {
        if (isActivityDestroyed || isFinishing) {
            return
        }

        lifecycleScope.launch {
            try {
                progressBarComments.visibility = View.VISIBLE
                val response = commentApiService.getNovelComments(novelId, currentCommentPage, 10)

                // Check again after async operation
                if (isActivityDestroyed || isFinishing) {
                    return@launch
                }

                if (response.isSuccessful && response.body() != null) {
                    val paginatedResponse = response.body()!!
                    if (paginatedResponse.success && paginatedResponse.data != null) {
                        if (currentCommentPage == 1) {
                            allComments.clear()
                            allComments.addAll(paginatedResponse.data)
                        } else {
                            allComments.addAll(paginatedResponse.data)
                        }

                        commentAdapter.submitList(allComments.toList())

                        totalCommentPages = paginatedResponse.totalPages

                        btnLoadMoreComments.visibility = if (currentCommentPage < totalCommentPages) {
                            View.VISIBLE
                        } else {
                            View.GONE
                        }

                        btnLoadMoreComments.text = "Load More (Page $currentCommentPage of $totalCommentPages)"
                    }
                }
            } catch (e: Exception) {
                if (!isActivityDestroyed && !isFinishing) {
                    Toast.makeText(this@NovelDetailActivity, "Failed to load comments: ${e.message}", Toast.LENGTH_SHORT).show()
                }
            } finally {
                if (!isActivityDestroyed && !isFinishing) {
                    progressBarComments.visibility = View.GONE
                }
            }
        }
    }

    private fun postComment() {
        val userId = currentUserId
        if (userId == null) {
            Toast.makeText(this, "Please login to post comments", Toast.LENGTH_SHORT).show()
            return
        }

        val content: String
        val imageUri: android.net.Uri?

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

                val userIdBody = okhttp3.RequestBody.create("text/plain".toMediaTypeOrNull(), userId.toString())
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
                    userIdBody,
                    novelIdBody,
                    null,
                    parentCommentIdBody,
                    contentBody,
                    imagePart
                )

                if (response.isSuccessful && response.body()?.success == true) {
                    Toast.makeText(this@NovelDetailActivity, "Comment posted!", Toast.LENGTH_SHORT).show()

                    etComment.text.clear()
                    selectedImageUri = null
                    ivSelectedImage.visibility = View.GONE

                    currentReplyInput?.text?.clear()
                    currentReplyImageView?.let {
                        it.visibility = View.GONE
                        it.tag = null
                    }
                    currentReplyCallback?.invoke()

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
                    selectedImageUri = data.data
                    ivSelectedImage.visibility = View.VISIBLE
                    Glide.with(this)
                        .load(selectedImageUri)
                        .into(ivSelectedImage)
                }
                IMAGE_PICK_CODE_REPLY -> {
                    val uri = data.data
                    currentReplyImageUri = uri
                    currentReplyImageView?.let { imageView ->
                        imageView.visibility = View.VISIBLE
                        imageView.tag = uri
                        Glide.with(this)
                            .load(uri)
                            .into(imageView)
                    }
                }
            }
        }
    }

    private suspend fun likeComment(commentId: Int) {
        val userId = currentUserId ?: return

        try {
            val request = VoteCommentRequest(userId = userId)
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
        val userId = currentUserId ?: return

        try {
            val request = VoteCommentRequest(userId = userId)
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

        val dialog = android.app.AlertDialog.Builder(this)
            .setView(dialogView)
            .create()

        dialog.window?.setBackgroundDrawableResource(android.R.color.transparent)

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
        val userId = currentUserId ?: return

        lifecycleScope.launch {
            try {
                progressBarComments.visibility = View.VISIBLE

                val userIdBody = okhttp3.RequestBody.create(
                    "text/plain".toMediaTypeOrNull(),
                    userId.toString()
                )

                val contentBody = okhttp3.RequestBody.create(
                    "text/plain".toMediaTypeOrNull(),
                    newContent
                )

                val response = commentApiService.updateComment(
                    commentId = commentId,
                    userId = userIdBody,
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
        val userId = currentUserId ?: return

        lifecycleScope.launch {
            try {
                progressBarComments.visibility = View.VISIBLE

                val request = DeleteCommentRequest(userId = userId)

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
        // Safety check before updating UI
        if (isActivityDestroyed || isFinishing) {
            return
        }

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

        // Check activity state before loading images with Glide
        if (!isActivityDestroyed && !isFinishing) {
            try {
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
            } catch (e: Exception) {
                android.util.Log.e("NovelDetailActivity", "Error loading images: ${e.message}")
            }
        }
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

    
    //DUYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
// Replace the rating-related methods in NovelDetailActivity.kt with these:

    private fun showRatingDialog() {
        // Check if user is logged in using token
        if (currentUserId == null) {
            Toast.makeText(this, "Please login to rate this novel", Toast.LENGTH_LONG).show()
            // Optional: Redirect to login
            // val intent = Intent(this, LoginActivity::class.java)
            // startActivity(intent)
            return
        }

        val dialogView = layoutInflater.inflate(R.layout.dialog_rating, null)
        val ratingBar = dialogView.findViewById<android.widget.RatingBar>(R.id.dialogRatingBar)
        val btnSubmit = dialogView.findViewById<Button>(R.id.btnSubmitRating)

        val dialog = android.app.AlertDialog.Builder(this)
            .setView(dialogView)
            .create()

        btnSubmit.setOnClickListener {
            val selectedRating = ratingBar.rating.toInt()
            if (selectedRating in 1..5) {
                submitRating(selectedRating)
                dialog.dismiss()
            } else {
                Toast.makeText(this, "Please select a rating between 1 and 5 stars", Toast.LENGTH_SHORT).show()
            }
        }

        dialog.show()
    }

    private fun submitRating(value: Int) {
        val userId = currentUserId

        // Double-check authentication
        if (userId == null) {
            Toast.makeText(this, "Please login to rate this novel", Toast.LENGTH_SHORT).show()
            return
        }

        // Show loading indicator
        progressBarComments?.visibility = View.VISIBLE

        val rating = com.sinhvien.webnovelapp.models.Rating(
            readerId = userId,
            novelId = novelId,
            ratingValue = value
        )

        val api = ApiClient.getClient().create(com.sinhvien.webnovelapp.api.RatingApi::class.java)

        api.createOrUpdateRating(rating).enqueue(object : retrofit2.Callback<Map<String, Any>> {
            override fun onResponse(
                call: retrofit2.Call<Map<String, Any>>,
                response: retrofit2.Response<Map<String, Any>>
            ) {
                progressBarComments?.visibility = View.GONE

                if (response.isSuccessful) {
                    Toast.makeText(
                        this@NovelDetailActivity,
                        "Rating submitted successfully!",
                        Toast.LENGTH_SHORT
                    ).show()
                    loadNovelRating()
                } else {
                    // Handle authentication errors specifically
                    when (response.code()) {
                        401 -> {
                            Toast.makeText(
                                this@NovelDetailActivity,
                                "Session expired. Please login again.",
                                Toast.LENGTH_LONG
                            ).show()
                            // Optional: Clear token and redirect to login
                            // tokenManager.clearToken()
                            // val intent = Intent(this@NovelDetailActivity, LoginActivity::class.java)
                            // startActivity(intent)
                            // finish()
                        }
                        else -> {
                            android.util.Log.e("RatingError", "Response code: ${response.code()}")
                            android.util.Log.e("RatingError", "Response body: ${response.errorBody()?.string()}")
                            Toast.makeText(
                                this@NovelDetailActivity,
                                "Failed to submit rating. Please try again.",
                                Toast.LENGTH_SHORT
                            ).show()
                        }
                    }
                }
            }

            override fun onFailure(call: retrofit2.Call<Map<String, Any>>, t: Throwable) {
                progressBarComments?.visibility = View.GONE

                android.util.Log.e("RatingError", "Network error: ${t.message}", t)

                Toast.makeText(
                    this@NovelDetailActivity,
                    "Network error: ${t.message}",
                    Toast.LENGTH_SHORT
                ).show()
            }
        })
    }

    private fun loadNovelRating() {
        if (isActivityDestroyed || isFinishing) {
            return
        }

        val api = ApiClient.getClient().create(com.sinhvien.webnovelapp.api.RatingApi::class.java)

        api.getAverageByNovel(novelId).enqueue(object : retrofit2.Callback<Map<String, Any>> {
            override fun onResponse(
                call: retrofit2.Call<Map<String, Any>>,
                response: retrofit2.Response<Map<String, Any>>
            ) {
                if (isActivityDestroyed || isFinishing) {
                    return
                }

                if (response.isSuccessful) {
                    val data = response.body()
                    val avg = data?.get("averageRating") ?: 0.0
                    val total = data?.get("totalRatings") ?: 0

                    tvNovelRating.text = "★ ${String.format("%.1f", avg)} ($total ratings)"
                } else {
                    tvNovelRating.text = "★ 0.0 (0 ratings)"
                }
            }

            override fun onFailure(call: retrofit2.Call<Map<String, Any>>, t: Throwable) {
                if (!isActivityDestroyed && !isFinishing) {
                    android.util.Log.e("RatingError", "Failed to load rating: ${t.message}")
                    tvNovelRating.text = "★ 0.0 (0 ratings)"
                }
            }
        })
    }

//DUYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

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