package com.sinhvien.webnovelapp.ThieuKhang

import android.content.Context
import android.net.ConnectivityManager
import android.net.NetworkCapabilities
import android.util.Log
import com.google.mlkit.common.model.DownloadConditions
import com.google.mlkit.common.model.RemoteModelManager
import com.google.mlkit.nl.translate.TranslateLanguage
import com.google.mlkit.nl.translate.TranslateRemoteModel
import com.google.mlkit.nl.translate.Translation
import com.google.mlkit.nl.translate.Translator
import com.google.mlkit.nl.translate.TranslatorOptions
import kotlinx.coroutines.*
import kotlin.coroutines.resume
import kotlin.coroutines.suspendCoroutine

/**
 * Optimized TranslationModelManager with faster downloads and better UX
 */
class TranslationModelManager private constructor(private val context: Context) {

    private val prefs = context.getSharedPreferences("TranslationModels", Context.MODE_PRIVATE)
    private val scope = CoroutineScope(Dispatchers.IO + SupervisorJob())
    private val modelManager = RemoteModelManager.getInstance()

    // Cache for translator instances to avoid recreating
    private val translatorCache = mutableMapOf<String, Translator>()

    companion object {
        private const val TAG = "TranslationModel"

        @Volatile
        private var instance: TranslationModelManager? = null

        fun getInstance(context: Context): TranslationModelManager {
            return instance ?: synchronized(this) {
                instance ?: TranslationModelManager(context.applicationContext).also {
                    instance = it
                }
            }
        }

        private val PRIORITY_LANGUAGES = listOf(
            TranslateLanguage.SPANISH,
            TranslateLanguage.FRENCH,
            TranslateLanguage.GERMAN,
            TranslateLanguage.PORTUGUESE
        )
    }

    data class ModelInfo(
        val languageCode: String,
        val languageName: String,
        val isDownloaded: Boolean,
        val size: String = "~10-30MB",
        val isPriority: Boolean = false
    )

    /**
     * OPTIMIZATION 1: Check actual model existence, not just SharedPreferences
     * This prevents false positives when models are deleted outside the app
     */
    suspend fun isModelDownloaded(sourceLanguage: String, targetLanguage: String): Boolean {
        return suspendCoroutine { continuation ->
            val model = TranslateRemoteModel.Builder(targetLanguage).build()

            modelManager.isModelDownloaded(model)
                .addOnSuccessListener { isDownloaded ->
                    // Sync SharedPreferences with actual state
                    val key = "${sourceLanguage}_$targetLanguage"
                    prefs.edit().putBoolean(key, isDownloaded).apply()
                    continuation.resume(isDownloaded)
                }
                .addOnFailureListener { exception ->
                    Log.e(TAG, "Error checking model: ${exception.message}")
                    // Fallback to SharedPreferences
                    val key = "${sourceLanguage}_$targetLanguage"
                    continuation.resume(prefs.getBoolean(key, false))
                }
        }
    }

    private fun markModelAsDownloaded(sourceLanguage: String, targetLanguage: String) {
        val key = "${sourceLanguage}_$targetLanguage"
        prefs.edit().putBoolean(key, true).apply()
        Log.d(TAG, "Marked as downloaded: $key")
    }

    /**
     * OPTIMIZATION 2: Use DownloadConditions for faster, more reliable downloads
     * Note: ML Kit doesn't provide real-time progress for downloads, so we simulate it
     */
    fun downloadModel(
        sourceLanguage: String,
        targetLanguage: String,
        onProgress: (progress: Int, status: String) -> Unit,
        onComplete: (success: Boolean, message: String) -> Unit
    ) {
        scope.launch {
            // Check if already downloaded
            if (isModelDownloaded(sourceLanguage, targetLanguage)) {
                withContext(Dispatchers.Main) {
                    onComplete(true, "Already downloaded")
                }
                return@launch
            }

            withContext(Dispatchers.Main) {
                onProgress(10, "Preparing download...")
            }

            val options = TranslatorOptions.Builder()
                .setSourceLanguage(sourceLanguage)
                .setTargetLanguage(targetLanguage)
                .build()

            val translator = Translation.getClient(options)
            val startTime = System.currentTimeMillis()

            // Use download conditions for better control
            val conditions = DownloadConditions.Builder()
                .requireWifi() // Remove this if you want to allow mobile data
                .build()

            withContext(Dispatchers.Main) {
                onProgress(20, "Downloading model...")
            }

            // Simulate progress since ML Kit doesn't provide real progress callbacks
            val progressJob = scope.launch(Dispatchers.Main) {
                var currentProgress = 20
                while (currentProgress < 90) {
                    delay(500)
                    currentProgress += 10
                    onProgress(currentProgress, "Downloading...")
                }
            }

            try {
                suspendCoroutine<Unit> { continuation ->
                    translator.downloadModelIfNeeded(conditions)
                        .addOnSuccessListener {
                            progressJob.cancel()
                            val duration = (System.currentTimeMillis() - startTime) / 1000
                            markModelAsDownloaded(sourceLanguage, targetLanguage)

                            // Cache the translator for immediate use
                            val cacheKey = "${sourceLanguage}_$targetLanguage"
                            translatorCache[cacheKey] = translator

                            scope.launch(Dispatchers.Main) {
                                onProgress(100, "Complete!")
                                onComplete(true, "Downloaded in ${duration}s")
                            }
                            continuation.resume(Unit)
                        }
                        .addOnFailureListener { exception ->
                            progressJob.cancel()
                            Log.e(TAG, "Download failed: ${exception.message}", exception)
                            translator.close()

                            val errorMsg = when {
                                exception.message?.contains("network", ignoreCase = true) == true ->
                                    "Network error. Check your connection."
                                exception.message?.contains("space", ignoreCase = true) == true ->
                                    "Not enough storage space."
                                else -> "Failed: ${exception.message ?: "Unknown error"}"
                            }

                            scope.launch(Dispatchers.Main) {
                                onComplete(false, errorMsg)
                            }
                            continuation.resume(Unit)
                        }
                }
            } catch (e: Exception) {
                progressJob.cancel()
                Log.e(TAG, "Exception during download: ${e.message}", e)
                withContext(Dispatchers.Main) {
                    onComplete(false, "Error: ${e.message ?: "Unknown error"}")
                }
            }
        }
    }

    /**
     * OPTIMIZATION 3: Fast model switching with pre-cached translators
     */
    fun getOrCreateTranslator(sourceLanguage: String, targetLanguage: String): Translator {
        val cacheKey = "${sourceLanguage}_$targetLanguage"

        return translatorCache.getOrPut(cacheKey) {
            val options = TranslatorOptions.Builder()
                .setSourceLanguage(sourceLanguage)
                .setTargetLanguage(targetLanguage)
                .build()
            Translation.getClient(options)
        }
    }

    /**
     * OPTIMIZATION 4: Parallel model downloads for faster bulk operations
     */
    fun autoDownloadPriorityModels(onlyOnWiFi: Boolean = true, onProgress: ((Int, Int) -> Unit)? = null) {
        if (!shouldAutoDownload(onlyOnWiFi)) {
            Log.d(TAG, "Auto-download skipped (conditions not met)")
            return
        }

        scope.launch {
            val toDownload = PRIORITY_LANGUAGES.filter {
                !isModelDownloaded(TranslateLanguage.ENGLISH, it)
            }

            if (toDownload.isEmpty()) {
                prefs.edit().putBoolean("auto_download_completed", true).apply()
                return@launch
            }

            var completed = 0
            val total = toDownload.size

            // Download in parallel with limit to avoid overwhelming the network
            toDownload.chunked(2).forEach { batch ->
                val jobs = batch.map { targetLanguage ->
                    async {
                        Log.d(TAG, "Auto-downloading: $targetLanguage")
                        downloadModelSilently(TranslateLanguage.ENGLISH, targetLanguage)
                        completed++
                        onProgress?.invoke(completed, total)
                    }
                }
                jobs.awaitAll()
                delay(1000) // Small delay between batches
            }

            prefs.edit().putBoolean("auto_download_completed", true).apply()
            Log.d(TAG, "Auto-download completed: $completed models")
        }
    }

    private fun shouldAutoDownload(onlyOnWiFi: Boolean): Boolean {
        val alreadyDownloaded = prefs.getBoolean("auto_download_completed", false)
        if (alreadyDownloaded) return false

        if (onlyOnWiFi && !isWiFiConnected()) return false

        return true
    }

    private fun isWiFiConnected(): Boolean {
        val connectivityManager = context.getSystemService(Context.CONNECTIVITY_SERVICE) as? ConnectivityManager
        val network = connectivityManager?.activeNetwork ?: return false
        val capabilities = connectivityManager.getNetworkCapabilities(network) ?: return false
        return capabilities.hasTransport(NetworkCapabilities.TRANSPORT_WIFI)
    }

    private suspend fun downloadModelSilently(sourceLanguage: String, targetLanguage: String) {
        return suspendCoroutine { continuation ->
            val options = TranslatorOptions.Builder()
                .setSourceLanguage(sourceLanguage)
                .setTargetLanguage(targetLanguage)
                .build()

            val translator = Translation.getClient(options)

            val conditions = DownloadConditions.Builder()
                .requireWifi()
                .build()

            translator.downloadModelIfNeeded(conditions)
                .addOnSuccessListener {
                    markModelAsDownloaded(sourceLanguage, targetLanguage)

                    // Cache for immediate use
                    val cacheKey = "${sourceLanguage}_$targetLanguage"
                    translatorCache[cacheKey] = translator

                    continuation.resume(Unit)
                }
                .addOnFailureListener { exception ->
                    Log.e(TAG, "Background download failed: ${exception.message}")
                    translator.close()
                    continuation.resume(Unit)
                }
        }
    }

    fun getAllModelInfo(): List<ModelInfo> {
        val languages = listOf(
            TranslateLanguage.VIETNAMESE to "Vietnamese",
            TranslateLanguage.SPANISH to "Spanish",
            TranslateLanguage.FRENCH to "French",
            TranslateLanguage.GERMAN to "German",
            TranslateLanguage.ITALIAN to "Italian",
            TranslateLanguage.PORTUGUESE to "Portuguese",
            TranslateLanguage.RUSSIAN to "Russian",
            TranslateLanguage.JAPANESE to "Japanese",
            TranslateLanguage.KOREAN to "Korean",
            TranslateLanguage.CHINESE to "Chinese (Simplified)",
            TranslateLanguage.ARABIC to "Arabic",
            TranslateLanguage.HINDI to "Hindi",
            TranslateLanguage.THAI to "Thai",
            TranslateLanguage.TURKISH to "Turkish"
        )

        return languages.map { (code, name) ->
            // Use synchronous check from SharedPreferences for UI speed
            val key = "${TranslateLanguage.ENGLISH}_$code"
            ModelInfo(
                languageCode = code,
                languageName = name,
                isDownloaded = prefs.getBoolean(key, false),
                isPriority = code in PRIORITY_LANGUAGES
            )
        }
    }

    fun deleteModel(sourceLanguage: String, targetLanguage: String, onComplete: (success: Boolean) -> Unit) {
        val model = TranslateRemoteModel.Builder(targetLanguage).build()
        val cacheKey = "${sourceLanguage}_$targetLanguage"

        // Remove from cache
        translatorCache.remove(cacheKey)?.close()

        modelManager.deleteDownloadedModel(model)
            .addOnSuccessListener {
                val key = "${sourceLanguage}_$targetLanguage"
                prefs.edit().putBoolean(key, false).apply()
                Log.d(TAG, "Deleted model: $key")
                onComplete(true)
            }
            .addOnFailureListener { exception ->
                Log.e(TAG, "Failed to delete model: ${exception.message}")
                val key = "${sourceLanguage}_$targetLanguage"
                prefs.edit().putBoolean(key, false).apply()
                onComplete(false)
            }
    }

    fun deleteAllModels(onComplete: (deleted: Int) -> Unit) {
        scope.launch {
            var deletedCount = 0

            // Clear cache first
            translatorCache.values.forEach { it.close() }
            translatorCache.clear()

            getAllModelInfo().filter { it.isDownloaded }.forEach { model ->
                suspendCoroutine<Unit> { continuation ->
                    deleteModel(TranslateLanguage.ENGLISH, model.languageCode) { success ->
                        if (success) deletedCount++
                        continuation.resume(Unit)
                    }
                }
            }
            withContext(Dispatchers.Main) {
                onComplete(deletedCount)
            }
        }
    }

    /**
     * Clear translator cache to free memory
     */
    fun clearCache() {
        translatorCache.values.forEach { it.close() }
        translatorCache.clear()
        Log.d(TAG, "Translator cache cleared")
    }

    fun cleanup() {
        clearCache()
        scope.cancel()
        Log.d(TAG, "Cleanup complete")
    }
}