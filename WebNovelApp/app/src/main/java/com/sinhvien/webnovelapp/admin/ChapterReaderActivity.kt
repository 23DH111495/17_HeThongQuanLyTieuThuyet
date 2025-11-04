package com.sinhvien.webnovelapp.activities

import android.app.Dialog
import android.content.Context
import android.graphics.Typeface
import android.os.Bundle
import android.speech.tts.TextToSpeech
import android.speech.tts.UtteranceProgressListener
import android.speech.tts.Voice
import android.view.MenuItem
import android.view.View
import android.view.Window
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.cardview.widget.CardView
import androidx.core.widget.NestedScrollView
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.google.android.material.bottomsheet.BottomSheetDialog
import com.google.android.material.floatingactionbutton.FloatingActionButton
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.api.ApiClient
import com.sinhvien.webnovelapp.api.NovelApiService
import com.sinhvien.webnovelapp.models.ChapterSummary
import com.sinhvien.webnovelapp.models.ChapterDetailResponse
import com.sinhvien.webnovelapp.models.ChapterListResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.google.mlkit.nl.translate.TranslateLanguage
import com.google.mlkit.nl.translate.Translation
import com.google.mlkit.nl.translate.Translator
import com.google.mlkit.nl.translate.TranslatorOptions
import java.util.Locale

class ChapterReaderActivity : AppCompatActivity(), TextToSpeech.OnInitListener {

    private lateinit var apiService: NovelApiService
    private lateinit var tvNovelTitle: TextView
    private lateinit var tvChapterTitle: TextView
    private lateinit var tvChapterInfo: TextView
    private lateinit var tvChapterContent: TextView
    private lateinit var tvAccessInfo: TextView
    private lateinit var layoutLocked: CardView
    private lateinit var tvPreviewContent: TextView
    private lateinit var tvUnlockPrice: TextView
    private lateinit var btnUnlock: Button
    private lateinit var progressBar: ProgressBar
    private lateinit var contentLayout: LinearLayout
    private lateinit var navigationButtons: LinearLayout
    private lateinit var btnPrevChapter: Button
    private lateinit var btnNextChapter: Button
    private lateinit var btnChapterList: Button
    private lateinit var btnSettings: Button
    private lateinit var btnTtsToggle: ImageButton
    private lateinit var scrollView: NestedScrollView
    private lateinit var btnScrollUpToolbar: ImageButton
    private lateinit var btnScrollDownToolbar: ImageButton

    private lateinit var ttsControlBar: LinearLayout
    private lateinit var btnTtsPlayPause: ImageButton
    private lateinit var btnTtsStop: ImageButton
    private lateinit var btnTtsSettings: ImageButton
    private lateinit var spinnerTtsSpeed: Spinner
    private lateinit var spinnerTtsVoice: Spinner

    private var novelId: Int = 0
    private var chapterNumber: Int = 0
    private var userId: Int? = null
    private var chapterList: List<ChapterSummary> = emptyList()
    private var textSize: Float = 16f
    private var currentFontIndex: Int = 0
    private val fontOptions = listOf(
        FontOption("Default", Typeface.DEFAULT),
        FontOption("Serif", Typeface.SERIF),
        FontOption("Sans Serif", Typeface.SANS_SERIF),
        FontOption("Monospace", Typeface.MONOSPACE)
    )

    private var originalContent: String = ""
    private var originalPreviewContent: String = ""
    private var currentLanguageIndex: Int = 0
    private var translator: Translator? = null
    private var isTranslating: Boolean = false
    private var tts: TextToSpeech? = null
    private var isTtsInitialized = false
    private var isPaused = false
    private var currentSentenceIndex = 0
    private var sentences: List<String> = emptyList()
    private var speechSpeed = 1.0f
    private var speechPitch = 1.0f
    private var currentVoiceIndex = 0
    private var availableVoices: List<Voice> = emptyList()
    private var voiceBottomSheet: BottomSheetDialog? = null

    private val languageOptions = listOf(
        LanguageOption("Original", null, null),
        LanguageOption("Vietnamese", TranslateLanguage.ENGLISH, TranslateLanguage.VIETNAMESE),
        LanguageOption("Spanish", TranslateLanguage.ENGLISH, TranslateLanguage.SPANISH),
        LanguageOption("French", TranslateLanguage.ENGLISH, TranslateLanguage.FRENCH),
        LanguageOption("German", TranslateLanguage.ENGLISH, TranslateLanguage.GERMAN),
        LanguageOption("Italian", TranslateLanguage.ENGLISH, TranslateLanguage.ITALIAN),
        LanguageOption("Portuguese", TranslateLanguage.ENGLISH, TranslateLanguage.PORTUGUESE),
        LanguageOption("Russian", TranslateLanguage.ENGLISH, TranslateLanguage.RUSSIAN),
        LanguageOption("Japanese", TranslateLanguage.ENGLISH, TranslateLanguage.JAPANESE),
        LanguageOption("Korean", TranslateLanguage.ENGLISH, TranslateLanguage.KOREAN),
        LanguageOption("Chinese (Simplified)", TranslateLanguage.ENGLISH, TranslateLanguage.CHINESE),
        LanguageOption("Arabic", TranslateLanguage.ENGLISH, TranslateLanguage.ARABIC),
        LanguageOption("Hindi", TranslateLanguage.ENGLISH, TranslateLanguage.HINDI),
        LanguageOption("Thai", TranslateLanguage.ENGLISH, TranslateLanguage.THAI),
        LanguageOption("Turkish", TranslateLanguage.ENGLISH, TranslateLanguage.TURKISH)
    )

    data class FontOption(val name: String, val typeface: Typeface)
    data class LanguageOption(val name: String, val sourceLanguage: String?, val targetLanguage: String?)

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_chapter_reader)

        novelId = intent.getIntExtra("NOVEL_ID", 0)
        chapterNumber = intent.getIntExtra("CHAPTER_NUMBER", 0)

        if (novelId == 0) {
            Toast.makeText(this, "Invalid chapter data", Toast.LENGTH_SHORT).show()
            finish()
            return
        }

        tts = TextToSpeech(this, this)
        loadReadingPreferences()

        try {
            setSupportActionBar(findViewById(R.id.toolbar))
            supportActionBar?.setDisplayHomeAsUpEnabled(true)
            supportActionBar?.title = "Chapter $chapterNumber"
            val toolbar = findViewById<androidx.appcompat.widget.Toolbar>(R.id.toolbar)
            toolbar.navigationIcon?.setTint(android.graphics.Color.parseColor("#77dd77"))
        } catch (e: Exception) {
        }

        apiService = ApiClient.getClient().create(NovelApiService::class.java)
        initializeViews()
        applyReadingPreferences()
        setupClickListeners()
        setupScrollListener()
        loadChapterList()
        loadChapter()
    }

    override fun onInit(status: Int) {
        if (status == TextToSpeech.SUCCESS) {
            val result = tts?.setLanguage(Locale.US)
            if (result == TextToSpeech.LANG_MISSING_DATA || result == TextToSpeech.LANG_NOT_SUPPORTED) {
                Toast.makeText(this, "Language not supported", Toast.LENGTH_SHORT).show()
                isTtsInitialized = false
            } else {
                isTtsInitialized = true
                tts?.setSpeechRate(speechSpeed)
                tts?.setPitch(speechPitch)

                tts?.setAudioAttributes(
                    android.media.AudioAttributes.Builder()
                        .setContentType(android.media.AudioAttributes.CONTENT_TYPE_SPEECH)
                        .setUsage(android.media.AudioAttributes.USAGE_MEDIA)
                        .build()
                )

                // Load and format available voices
                tts?.voices?.let { voices ->
                    availableVoices = voices.filter {
                        it.locale.language == "en" && !it.isNetworkConnectionRequired
                    }.sortedBy { it.name }

                    // CREATE BETTER VOICE NAMES
                    val voiceNames = availableVoices.map { voice ->
                        formatVoiceName(voice)
                    }

                    val adapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, voiceNames)
                    adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
                    spinnerTtsVoice.adapter = adapter
                    spinnerTtsVoice.setSelection(currentVoiceIndex)
                }

                setupTtsListeners()
            }
        } else {
            Toast.makeText(this, "TTS initialization failed", Toast.LENGTH_SHORT).show()
            isTtsInitialized = false
        }
    }

    private fun formatVoiceName(voice: Voice): String {
        // Direct mapping based on actual voice names from Google TTS
        return when (voice.name) {
            // Language variants (general)
            "en-AU-language" -> "Australian (HD)"
            "en-GB-language" -> "British (HD)"
            "en-IN-language" -> "Indian (HD)"
            "en-NG-language" -> "Nigerian (HD)"
            "en-US-language" -> "American (HD)"

            // Australian voices
            "en-au-x-aua-local" -> "Australian Female 1"
            "en-au-x-aub-local" -> "Australian Female 2"
            "en-au-x-auc-local" -> "Australian Male 1"
            "en-au-x-aud-local" -> "Australian Male 2"

            // British voices
            "en-gb-x-gba-local" -> "Grace (British)"
            "en-gb-x-gbb-local" -> "George (British)"
            "en-gb-x-gbc-local" -> "Gabriel (British)"
            "en-gb-x-gbd-local" -> "Gemma (British)"
            "en-gb-x-gbg-local" -> "Gordon (British)"
            "en-gb-x-rjs-local" -> "Rachel (British)"

            // Indian voices
            "en-in-x-ena-local" -> "Indian Female 1"
            "en-in-x-enc-local" -> "Indian Male 1"
            "en-in-x-end-local" -> "Indian Male 2"
            "en-in-x-ene-local" -> "Indian Female 2"

            // Nigerian voices
            "en-ng-x-tfn-local" -> "Nigerian Female"

            // American voices
            "en-us-x-iob-local" -> "Isabella"
            "en-us-x-iog-local" -> "James"
            "en-us-x-iol-local" -> "Olivia"
            "en-us-x-iom-local" -> "Michael"
            "en-us-x-sfg-local" -> "Sophia"
            "en-us-x-tpc-local" -> "Thomas"
            "en-us-x-tpd-local" -> "David"
            "en-us-x-tpf-local" -> "Tiffany"

            // Fallback for any unknown voices
            else -> {
                val index = availableVoices.indexOf(voice)
                "Voice ${if (index >= 0) index + 1 else availableVoices.size + 1}"
            }
        }
    }

    private fun setupTtsListeners() {
        // Speed spinner
        spinnerTtsSpeed.setSelection(2) // Default to 1x
        spinnerTtsSpeed.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                speechSpeed = when (position) {
                    0 -> 0.5f
                    1 -> 0.75f
                    2 -> 1.0f
                    3 -> 1.25f
                    4 -> 1.5f
                    5 -> 2.0f
                    else -> 1.0f
                }
                tts?.setSpeechRate(speechSpeed)
            }
            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }

        // Voice spinner
        spinnerTtsVoice.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                currentVoiceIndex = position
                if (position < availableVoices.size) {
                    tts?.voice = availableVoices[position]
                }
            }
            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }

        // Play/Pause button
        btnTtsPlayPause.setOnClickListener {
            if (tts?.isSpeaking == true && !isPaused) {
                pauseReading()
            } else {
                if (isPaused) {
                    isPaused = false
                    speakSentence(currentSentenceIndex)
                } else {
                    startReading()
                }
            }
        }

        // Stop button
        btnTtsStop.setOnClickListener {
            stopReading()
        }

        // Settings button - shows dialog with pitch control
        btnTtsSettings.setOnClickListener {
            showTtsSettingsDialog()
        }

        tts?.setOnUtteranceProgressListener(object : UtteranceProgressListener() {
            override fun onStart(utteranceId: String?) {
                runOnUiThread {
                    updatePlayPauseButton(true)
                }
            }

            override fun onDone(utteranceId: String?) {
                if (!isPaused) {
                    currentSentenceIndex++
                    if (currentSentenceIndex < sentences.size) {
                        // Small delay to prevent audio glitches
                        android.os.Handler(android.os.Looper.getMainLooper()).postDelayed({
                            if (!isPaused) {
                                speakSentence(currentSentenceIndex)
                            }
                        }, 50) // 50ms delay between sentences
                    } else {
                        runOnUiThread {
                            stopReading()
                        }
                    }
                }
            }

            override fun onError(utteranceId: String?) {
                runOnUiThread {
                    Toast.makeText(this@ChapterReaderActivity, "TTS Error", Toast.LENGTH_SHORT).show()
                    stopReading()
                }
            }
        })

        // Play/Pause button
        btnTtsPlayPause.setOnClickListener {
            if (tts?.isSpeaking == true && !isPaused) {
                pauseReading()
            } else {
                if (isPaused) {
                    isPaused = false
                    speakSentence(currentSentenceIndex)
                } else {
                    startReading()
                }
            }
        }

        // Stop button
        btnTtsStop.setOnClickListener {
            stopReading()
        }

        // Settings button
        btnTtsSettings.setOnClickListener {
            showTtsSettingsDialog()
        }
    }

    private fun showTtsSettingsDialog() {
        val dialog = Dialog(this)
        dialog.requestWindowFeature(Window.FEATURE_NO_TITLE)
        dialog.setContentView(R.layout.dialog_tts_settings)
        dialog.window?.setBackgroundDrawableResource(android.R.color.transparent)

        val seekBarPitch = dialog.findViewById<SeekBar>(R.id.seekBarPitch)
        val tvPitchValue = dialog.findViewById<TextView>(R.id.tvPitchValue)
        val btnClose = dialog.findViewById<Button>(R.id.btnCloseTtsSettings)

        seekBarPitch.progress = ((speechPitch - 0.5f) * 20f / 1.5f).toInt()
        tvPitchValue.text = String.format("%.1fx", speechPitch)

        seekBarPitch.setOnSeekBarChangeListener(object : SeekBar.OnSeekBarChangeListener {
            override fun onProgressChanged(seekBar: SeekBar?, progress: Int, fromUser: Boolean) {
                speechPitch = 0.5f + (progress / 20f) * 1.5f
                tvPitchValue.text = String.format("%.1fx", speechPitch)
                tts?.setPitch(speechPitch)
            }
            override fun onStartTrackingTouch(seekBar: SeekBar?) {}
            override fun onStopTrackingTouch(seekBar: SeekBar?) {}
        })

        btnClose.setOnClickListener { dialog.dismiss() }
        dialog.show()
    }

    private fun initializeViews() {
        tvNovelTitle = findViewById(R.id.tvNovelTitle)
        tvChapterTitle = findViewById(R.id.tvChapterTitle)
        tvChapterInfo = findViewById(R.id.tvChapterInfo)
        tvChapterContent = findViewById(R.id.tvChapterContent)
        tvAccessInfo = findViewById(R.id.tvAccessInfo)
        layoutLocked = findViewById(R.id.layoutLocked)
        tvPreviewContent = findViewById(R.id.tvPreviewContent)
        tvUnlockPrice = findViewById(R.id.tvUnlockPrice)
        btnUnlock = findViewById(R.id.btnUnlock)
        progressBar = findViewById(R.id.progressBar)
        contentLayout = findViewById(R.id.contentLayout)
        navigationButtons = findViewById(R.id.navigationButtons)
        btnPrevChapter = findViewById(R.id.btnPrevChapter)
        btnNextChapter = findViewById(R.id.btnNextChapter)
        btnChapterList = findViewById(R.id.btnChaptersList)
        btnSettings = findViewById(R.id.btnSettings)
        scrollView = findViewById(R.id.scrollView)
        btnScrollUpToolbar = findViewById(R.id.btnScrollUpToolbar)
        btnScrollDownToolbar = findViewById(R.id.btnScrollDownToolbar)
        ttsControlBar = findViewById(R.id.ttsControlBar)
        btnTtsPlayPause = findViewById(R.id.btnTtsPlayPause)
        btnTtsStop = findViewById(R.id.btnTtsStop)
        btnTtsSettings = findViewById(R.id.btnTtsSettings)
        spinnerTtsSpeed = findViewById(R.id.spinnerTtsSpeed)
        spinnerTtsVoice = findViewById(R.id.spinnerTtsVoice)
        btnTtsToggle = findViewById(R.id.btnTtsToggle)
    }

    private fun showVoiceBottomSheet() {
        if (!isTtsInitialized) {
            Toast.makeText(this, "Text-to-Speech not ready", Toast.LENGTH_SHORT).show()
            return
        }

        if (tvChapterContent.text.isEmpty() || tvChapterContent.visibility != View.VISIBLE) {
            Toast.makeText(this, "No content to read", Toast.LENGTH_SHORT).show()
            return
        }

        voiceBottomSheet = BottomSheetDialog(this)
        val view = layoutInflater.inflate(R.layout.bottom_sheet_voice_reader, null)
        voiceBottomSheet?.setContentView(view)

        voiceBottomSheet?.window?.setBackgroundDrawableResource(android.R.color.transparent)


        val btnClose = view.findViewById<ImageButton>(R.id.btnCloseVoiceSheet)
        val spinnerVoice = view.findViewById<Spinner>(R.id.spinnerVoice)
        val seekBarSpeed = view.findViewById<SeekBar>(R.id.seekBarSpeed)
        val tvSpeedValue = view.findViewById<TextView>(R.id.tvSpeedValue)
        val seekBarPitch = view.findViewById<SeekBar>(R.id.seekBarPitch)
        val tvPitchValue = view.findViewById<TextView>(R.id.tvPitchValue)
        val btnPlayPause = view.findViewById<FloatingActionButton>(R.id.btnVoicePlayPause)
        val btnStop = view.findViewById<ImageButton>(R.id.btnVoiceStop)
        val btnTest = view.findViewById<Button>(R.id.btnTestVoice)

        // FIXED: Setup voice spinner using formatVoiceName function
        val voiceNames = if (availableVoices.isEmpty()) {
            listOf("Default Voice")
        } else {
            availableVoices.map { voice ->
                formatVoiceName(voice)  // Use the formatting function
            }
        }

        // Create custom adapter with white text
        val voiceAdapter = object : ArrayAdapter<String>(this, R.layout.spinner_item, voiceNames) {
            override fun getView(position: Int, convertView: View?, parent: android.view.ViewGroup): View {
                val view = super.getView(position, convertView, parent)
                val textView = view.findViewById<TextView>(android.R.id.text1)
                textView?.setTextColor(android.graphics.Color.WHITE)
                return view
            }

            override fun getDropDownView(position: Int, convertView: View?, parent: android.view.ViewGroup): View {
                val view = super.getDropDownView(position, convertView, parent)
                val textView = view.findViewById<TextView>(android.R.id.text1)
                textView?.setTextColor(android.graphics.Color.WHITE)
                return view
            }
        }
        voiceAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        spinnerVoice.adapter = voiceAdapter
        spinnerVoice.setSelection(currentVoiceIndex)

        spinnerVoice.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                currentVoiceIndex = position
                if (position < availableVoices.size) {
                    tts?.voice = availableVoices[position]
                }
            }
            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }

        // Setup speed control
        seekBarSpeed.progress = ((speechSpeed - 0.5f) * 20f / 1.5f).toInt()
        tvSpeedValue.text = String.format("%.1fx", speechSpeed)

        seekBarSpeed.setOnSeekBarChangeListener(object : SeekBar.OnSeekBarChangeListener {
            override fun onProgressChanged(seekBar: SeekBar?, progress: Int, fromUser: Boolean) {
                speechSpeed = 0.5f + (progress / 20f) * 1.5f
                tvSpeedValue.text = String.format("%.1fx", speechSpeed)
                tts?.setSpeechRate(speechSpeed)
            }
            override fun onStartTrackingTouch(seekBar: SeekBar?) {}
            override fun onStopTrackingTouch(seekBar: SeekBar?) {}
        })

        // Setup pitch control
        seekBarPitch.progress = ((speechPitch - 0.5f) * 20f / 1.5f).toInt()
        tvPitchValue.text = String.format("%.1fx", speechPitch)

        seekBarPitch.setOnSeekBarChangeListener(object : SeekBar.OnSeekBarChangeListener {
            override fun onProgressChanged(seekBar: SeekBar?, progress: Int, fromUser: Boolean) {
                speechPitch = 0.5f + (progress / 20f) * 1.5f
                tvPitchValue.text = String.format("%.1fx", speechPitch)
                tts?.setPitch(speechPitch)
            }
            override fun onStartTrackingTouch(seekBar: SeekBar?) {}
            override fun onStopTrackingTouch(seekBar: SeekBar?) {}
        })

        // Setup play/pause button
        updatePlayPauseButtonInSheet(btnPlayPause, tts?.isSpeaking == true && !isPaused)

        btnPlayPause.setOnClickListener {
            if (tts?.isSpeaking == true && !isPaused) {
                pauseReading()
                updatePlayPauseButtonInSheet(btnPlayPause, false)
            } else {
                if (isPaused) {
                    isPaused = false
                    speakSentence(currentSentenceIndex)
                } else {
                    startReading()
                }
                updatePlayPauseButtonInSheet(btnPlayPause, true)
            }
        }

        btnStop.setOnClickListener {
            stopReading()
            updatePlayPauseButtonInSheet(btnPlayPause, false)
        }

        btnTest.setOnClickListener {
            val testText = "Hello! This is a test of the selected voice."
            tts?.speak(testText, TextToSpeech.QUEUE_FLUSH, null, null)
        }

        btnClose.setOnClickListener {
            voiceBottomSheet?.dismiss()
        }

        voiceBottomSheet?.setOnDismissListener {
            if (tts?.isSpeaking != true) {
                // Save preferences when closing
            }
        }

        voiceBottomSheet?.show()
    }

    private fun updatePlayPauseButton(isPlaying: Boolean) {
        if (isPlaying) {
            btnTtsPlayPause.setImageResource(android.R.drawable.ic_media_pause)
        } else {
            btnTtsPlayPause.setImageResource(android.R.drawable.ic_media_play)
        }
    }

    private fun updatePlayPauseButtonInSheet(button: FloatingActionButton, isPlaying: Boolean) {
        if (isPlaying) {
            button.setImageResource(android.R.drawable.ic_media_pause)
        } else {
            button.setImageResource(android.R.drawable.ic_media_play)
        }
    }

    private fun startReading() {
        val content = tvChapterContent.text.toString()
        if (content.isEmpty()) {
            Toast.makeText(this, "No content to read", Toast.LENGTH_SHORT).show()
            return
        }

        // Better sentence splitting that handles edge cases
        sentences = content.split(Regex("(?<=[.!?]\"?)\\s+(?=[A-Z\"'])"))
            .map { it.trim() }
            .filter { it.isNotBlank() && it.length > 1 }

        if (sentences.isEmpty()) {
            Toast.makeText(this, "No content to read", Toast.LENGTH_SHORT).show()
            return
        }

        currentSentenceIndex = 0
        isPaused = false
        speakSentence(0)
        updatePlayPauseButton(true)
    }

    private fun speakSentence(index: Int) {
        if (index >= sentences.size || isPaused) {
            return
        }

        val sentence = sentences[index]
        val params = Bundle()
        params.putString(TextToSpeech.Engine.KEY_PARAM_UTTERANCE_ID, "sentence_$index")

        // CRITICAL: Use QUEUE_ADD instead of QUEUE_FLUSH to prevent stuttering
        // Only use FLUSH for the first sentence or after pause
        val queueMode = if (index == 0 || isPaused) {
            TextToSpeech.QUEUE_FLUSH
        } else {
            TextToSpeech.QUEUE_ADD
        }

        tts?.speak(sentence, queueMode, params, "sentence_$index")
    }

    private fun pauseReading() {
        isPaused = true
        tts?.stop()
        updatePlayPauseButton(false)
    }

    private fun stopReading() {
        isPaused = false
        currentSentenceIndex = 0
        tts?.stop()
        updatePlayPauseButton(false)
    }

    private fun setupClickListeners() {
        btnPrevChapter.setOnClickListener {
            if (chapterNumber > 1) {
                stopReading()
                chapterNumber--
                currentLanguageIndex = 0
                loadChapter()
                scrollView.smoothScrollTo(0, 0)
            }
        }
        btnNextChapter.setOnClickListener {
            stopReading()
            chapterNumber++
            currentLanguageIndex = 0
            loadChapter()
            scrollView.smoothScrollTo(0, 0)
        }
        btnChapterList.setOnClickListener { showChapterListDialog() }
        btnSettings.setOnClickListener { showReadingSettingsDialog() }
        btnUnlock.setOnClickListener { Toast.makeText(this, "Unlock feature coming soon", Toast.LENGTH_SHORT).show() }
        btnScrollUpToolbar.setOnClickListener { scrollView.smoothScrollTo(0, 0) }
        btnScrollDownToolbar.setOnClickListener { scrollView.post { scrollView.fullScroll(View.FOCUS_DOWN) } }

        btnTtsToggle.setOnClickListener {
            showVoiceBottomSheet()
        }

    }

    private fun setupScrollListener() {
        scrollView.setOnScrollChangeListener { v: NestedScrollView, _, scrollY, _, _ ->
            val view = v.getChildAt(v.childCount - 1)
            val diff = view.bottom - (v.height + scrollY)

            btnScrollUpToolbar.alpha = if (scrollY < 10) 0.3f else 1.0f
            btnScrollDownToolbar.alpha = if (diff < 10) 0.3f else 1.0f
        }
    }

    private fun addParagraphBreaks(text: String): String {
        if (text.contains("\n\n")) return text
        val lines = text.split("\n").map { it.trim() }.filter { it.isNotEmpty() }
        if (lines.size <= 1) {
            return text.replace(Regex("""\.\s+([A-Z"])"""), ".\n\n$1")
                .replace(Regex("""!\s+([A-Z"])"""), "!\n\n$1")
                .replace(Regex("""\?\s+([A-Z"])"""), "?\n\n$1")
        }
        return lines.joinToString("\n\n")
    }

    private fun showReadingSettingsDialog() {
        val dialog = Dialog(this)
        dialog.requestWindowFeature(Window.FEATURE_NO_TITLE)
        dialog.setContentView(R.layout.dialog_reading_settings)
        dialog.window?.setBackgroundDrawableResource(android.R.color.transparent)

        val seekBarSize = dialog.findViewById<SeekBar>(R.id.seekBarTextSize)
        val tvSizePreview = dialog.findViewById<TextView>(R.id.tvSizePreview)
        val tvCurrentSize = dialog.findViewById<TextView>(R.id.tvCurrentSize)
        val btnFontPrev = dialog.findViewById<Button>(R.id.btnFontPrev)
        val btnFontNext = dialog.findViewById<Button>(R.id.btnFontNext)
        val tvCurrentFont = dialog.findViewById<TextView>(R.id.tvCurrentFont)
        val tvFontPreview = dialog.findViewById<TextView>(R.id.tvFontPreview)
        val spinnerLanguage = dialog.findViewById<Spinner>(R.id.spinnerLanguage)
        val translationProgressContainer = dialog.findViewById<FrameLayout>(R.id.translationProgressContainer)
        val progressTranslation = dialog.findViewById<ProgressBar>(R.id.progressTranslation)
        val tvTranslationProgress = dialog.findViewById<TextView>(R.id.tvTranslationProgress)
        val tvTranslationStatus = dialog.findViewById<TextView>(R.id.tvTranslationStatus)
        val btnClose = dialog.findViewById<Button>(R.id.btnCloseSettings)

        seekBarSize.min = 12
        seekBarSize.max = 28
        seekBarSize.progress = textSize.toInt()
        tvCurrentSize.text = "${textSize.toInt()}sp"
        tvSizePreview.textSize = textSize

        seekBarSize.setOnSeekBarChangeListener(object : SeekBar.OnSeekBarChangeListener {
            override fun onProgressChanged(seekBar: SeekBar?, progress: Int, fromUser: Boolean) {
                textSize = progress.toFloat()
                tvCurrentSize.text = "${progress}sp"
                tvSizePreview.textSize = textSize
                applyReadingPreferences()
            }
            override fun onStartTrackingTouch(seekBar: SeekBar?) {}
            override fun onStopTrackingTouch(seekBar: SeekBar?) { saveReadingPreferences() }
        })

        fun updateFontDisplay() {
            val currentFont = fontOptions[currentFontIndex]
            tvCurrentFont.text = currentFont.name
            tvFontPreview.typeface = currentFont.typeface
        }
        updateFontDisplay()

        btnFontPrev.setOnClickListener {
            currentFontIndex = if (currentFontIndex > 0) currentFontIndex - 1 else fontOptions.size - 1
            updateFontDisplay()
            applyReadingPreferences()
            saveReadingPreferences()
        }
        btnFontNext.setOnClickListener {
            currentFontIndex = if (currentFontIndex < fontOptions.size - 1) currentFontIndex + 1 else 0
            updateFontDisplay()
            applyReadingPreferences()
            saveReadingPreferences()
        }

        val adapter = ArrayAdapter(this, R.layout.spinner_item, languageOptions.map { it.name })
        adapter.setDropDownViewResource(R.layout.spinner_item)
        spinnerLanguage.adapter = adapter
        spinnerLanguage.setSelection(currentLanguageIndex)

        spinnerLanguage.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(parent: AdapterView<*>?, view: View?, position: Int, id: Long) {
                if (position == currentLanguageIndex) return
                if (isTranslating) {
                    Toast.makeText(this@ChapterReaderActivity, "Please wait", Toast.LENGTH_SHORT).show()
                    spinnerLanguage.setSelection(currentLanguageIndex)
                    return
                }
                stopReading()
                currentLanguageIndex = position
                if (position == 0) {
                    translationProgressContainer.visibility = View.GONE
                    tvChapterContent.text = originalContent
                    if (layoutLocked.visibility == View.VISIBLE) tvPreviewContent.text = originalPreviewContent
                } else {
                    translationProgressContainer.visibility = View.VISIBLE
                    progressTranslation.progress = 0
                    tvTranslationProgress.text = "0%"
                    tvTranslationStatus.text = "Downloading model..."
                    translateContentWithProgress(progressTranslation, tvTranslationProgress, tvTranslationStatus, translationProgressContainer)
                }
                saveReadingPreferences()
            }
            override fun onNothingSelected(parent: AdapterView<*>?) {}
        }

        btnClose.setOnClickListener { dialog.dismiss() }
        dialog.show()
    }

    private fun translateContentWithProgress(progressBar: ProgressBar, tvProgress: TextView, tvStatus: TextView, container: FrameLayout) {
        val selectedLanguage = languageOptions[currentLanguageIndex]
        if (selectedLanguage.sourceLanguage == null || selectedLanguage.targetLanguage == null) {
            tvChapterContent.text = originalContent
            tvPreviewContent.text = originalPreviewContent
            container.visibility = View.GONE
            isTranslating = false
            return
        }

        translator?.close()
        val options = TranslatorOptions.Builder()
            .setSourceLanguage(selectedLanguage.sourceLanguage)
            .setTargetLanguage(selectedLanguage.targetLanguage)
            .build()
        translator = Translation.getClient(options)
        isTranslating = true
        tvStatus.text = "Downloading model..."
        progressBar.isIndeterminate = true

        translator?.downloadModelIfNeeded()
            ?.addOnSuccessListener {
                progressBar.isIndeterminate = false
                tvStatus.text = "Translating..."
                if (tvChapterContent.visibility == View.VISIBLE && originalContent.isNotEmpty()) {
                    translateTextWithProgress(originalContent, tvChapterContent, progressBar, tvProgress, tvStatus, container)
                }
                if (layoutLocked.visibility == View.VISIBLE && originalPreviewContent.isNotEmpty()) {
                    translateTextWithProgress(originalPreviewContent, tvPreviewContent, progressBar, tvProgress, tvStatus, container)
                }
            }
            ?.addOnFailureListener { exception ->
                container.visibility = View.GONE
                isTranslating = false
                Toast.makeText(this, "Translation failed: ${exception.message}", Toast.LENGTH_SHORT).show()
            }
    }

    private fun translateTextWithProgress(text: String, targetTextView: TextView, progressBar: ProgressBar, tvProgress: TextView, tvStatus: TextView, container: FrameLayout) {
        val paragraphs = text.split("\n\n").map { it.trim() }.filter { it.isNotEmpty() }
        if (paragraphs.isEmpty()) {
            container.visibility = View.GONE
            isTranslating = false
            return
        }

        val translatedParagraphs = Array<String?>(paragraphs.size) { null }
        var completedParagraphs = 0
        val totalParagraphs = paragraphs.size

        fun updateProgress() {
            val progress = (completedParagraphs * 100) / totalParagraphs
            runOnUiThread {
                progressBar.progress = progress
                tvProgress.text = "$progress%"
                tvStatus.text = "Translating... ($completedParagraphs/$totalParagraphs)"
            }
        }

        paragraphs.forEachIndexed { index, paragraph ->
            translator?.translate(paragraph)
                ?.addOnSuccessListener { translatedText ->
                    synchronized(translatedParagraphs) {
                        translatedParagraphs[index] = translatedText
                        completedParagraphs++
                        updateProgress()
                        if (completedParagraphs == paragraphs.size) {
                            runOnUiThread {
                                tvStatus.text = "Complete!"
                                targetTextView.text = translatedParagraphs.filterNotNull().joinToString("\n\n")
                                android.os.Handler(android.os.Looper.getMainLooper()).postDelayed({
                                    container.visibility = View.GONE
                                    isTranslating = false
                                }, 1000)
                            }
                        }
                    }
                }
                ?.addOnFailureListener {
                    synchronized(translatedParagraphs) {
                        translatedParagraphs[index] = paragraph
                        completedParagraphs++
                        updateProgress()
                        if (completedParagraphs == paragraphs.size) {
                            runOnUiThread {
                                tvStatus.text = "Complete!"
                                targetTextView.text = translatedParagraphs.filterNotNull().joinToString("\n\n")
                                android.os.Handler(android.os.Looper.getMainLooper()).postDelayed({
                                    container.visibility = View.GONE
                                    isTranslating = false
                                }, 1000)
                            }
                        }
                    }
                }
        }
    }

    private fun loadReadingPreferences() {
        val prefs = getSharedPreferences("ReadingPrefs", Context.MODE_PRIVATE)
        textSize = prefs.getFloat("textSize", 16f)
        currentFontIndex = prefs.getInt("fontIndex", 0)
        currentLanguageIndex = prefs.getInt("languageIndex", 0)
    }

    private fun saveReadingPreferences() {
        val prefs = getSharedPreferences("ReadingPrefs", Context.MODE_PRIVATE)
        prefs.edit().apply {
            putFloat("textSize", textSize)
            putInt("fontIndex", currentFontIndex)
            putInt("languageIndex", currentLanguageIndex)
            apply()
        }
    }

    private fun applyReadingPreferences() {
        tvChapterContent.textSize = textSize
        tvChapterContent.typeface = fontOptions[currentFontIndex].typeface
        tvPreviewContent.textSize = textSize
        tvPreviewContent.typeface = fontOptions[currentFontIndex].typeface
    }

    private fun loadChapterList() {
        apiService.getNovelChapters(novelId).enqueue(object : Callback<ChapterListResponse> {
            override fun onResponse(call: Call<ChapterListResponse>, response: Response<ChapterListResponse>) {
                if (response.isSuccessful && response.body() != null) {
                    val chapterResponse = response.body()!!
                    if (chapterResponse.Success) chapterList = chapterResponse.Data
                }
            }
            override fun onFailure(call: Call<ChapterListResponse>, t: Throwable) {}
        })
    }

    private fun showChapterListDialog() {
        if (chapterList.isEmpty()) {
            Toast.makeText(this, "Loading chapters...", Toast.LENGTH_SHORT).show()
            return
        }
        val dialog = Dialog(this, android.R.style.Theme_Translucent_NoTitleBar)
        dialog.setContentView(R.layout.dialog_chapter_list)
        val dimBackground = dialog.findViewById<View>(R.id.dimBackground)
        val rvChapterList = dialog.findViewById<RecyclerView>(R.id.rvChapterList)
        val tvChapterCount = dialog.findViewById<TextView>(R.id.tvChapterCount)
        val btnCloseDialog = dialog.findViewById<TextView>(R.id.btnCloseDialog)
        tvChapterCount.text = "Chapters (${chapterList.size})"
        dimBackground.setOnClickListener { dialog.dismiss() }
        btnCloseDialog.setOnClickListener { dialog.dismiss() }
        rvChapterList.layoutManager = LinearLayoutManager(this)
        rvChapterList.adapter = ChapterListAdapter(chapterList) { selectedChapter ->
            chapterNumber = selectedChapter.ChapterNumber
            currentLanguageIndex = 0
            stopReading()
            loadChapter()
            dialog.dismiss()
        }
        val currentIndex = chapterList.indexOfFirst { it.ChapterNumber == chapterNumber }
        if (currentIndex != -1) rvChapterList.scrollToPosition(currentIndex)
        dialog.show()
    }

    private fun loadChapter() {
        showLoading(true)
        apiService.getChapterDetail(novelId, chapterNumber, userId).enqueue(object : Callback<ChapterDetailResponse> {
            override fun onResponse(call: Call<ChapterDetailResponse>, response: Response<ChapterDetailResponse>) {
                showLoading(false)
                if (response.isSuccessful && response.body() != null) {
                    val chapterResponse = response.body()!!
                    if (chapterResponse.Success) displayChapter(chapterResponse)
                    else showError("Failed to load chapter")
                } else showError("HTTP ${response.code()}: ${response.message()}")
            }
            override fun onFailure(call: Call<ChapterDetailResponse>, t: Throwable) {
                showLoading(false)
                showError("Network Error: ${t.message}")
            }
        })
    }

    private fun displayChapter(response: ChapterDetailResponse) {
        val chapter = response.Data
        val accessInfo = response.AccessInfo
        val novel = response.Novel

        tvNovelTitle.text = novel.Title
        val chapterTitle = if (chapter.Title.isNullOrEmpty()) {
            if (chapter.ChapterNumber == 0) "Prologue" else "Chapter ${chapter.ChapterNumber}"
        } else "Chapter ${chapter.ChapterNumber}: ${chapter.Title}"
        tvChapterTitle.text = chapterTitle
        supportActionBar?.title = if (chapter.ChapterNumber == 0) "Prologue" else "Chapter ${chapter.ChapterNumber}"
        tvChapterInfo.text = "${formatNumber(chapter.WordCount)} words • ${chapter.PublishDate ?: "Unknown"}"

        if (accessInfo.HasAccess) {
            layoutLocked.visibility = View.GONE
            tvChapterContent.visibility = View.VISIBLE
            ttsControlBar.visibility = View.GONE
            originalContent = addParagraphBreaks(chapter.Content ?: "No content")
            tvChapterContent.text = originalContent
            val accessMessage = when (accessInfo.AccessReason) {
                "free" -> "Free chapter"
                "premium" -> "Premium unlocked"
                "purchased" -> "Purchased"
                else -> ""
            }
            tvAccessInfo.text = accessMessage
            tvAccessInfo.visibility = if (accessMessage.isNotEmpty()) View.VISIBLE else View.GONE
        } else {
            layoutLocked.visibility = View.VISIBLE
            tvChapterContent.visibility = View.GONE
            tvAccessInfo.visibility = View.GONE
            ttsControlBar.visibility = View.GONE
            originalPreviewContent = addParagraphBreaks(chapter.PreviewContent ?: "No preview")
            tvPreviewContent.text = originalPreviewContent
            val unlockMessage = when (accessInfo.AccessReason) {
                "login_required" -> "Login to read"
                "locked" -> "Unlock to continue"
                else -> "Locked"
            }
            tvUnlockPrice.text = if (accessInfo.RequiredCoins > 0) {
                "Unlock for ${accessInfo.RequiredCoins} coins"
            } else if (accessInfo.IsPremium) {
                "Premium required"
            } else {
                unlockMessage
            }
            btnUnlock.text = if (accessInfo.AccessReason == "login_required") "Login" else "Unlock Chapter"
        }

        applyReadingPreferences()
        if (currentLanguageIndex != 0) {
            translateContentWithProgress(
                ProgressBar(this),
                TextView(this),
                TextView(this),
                FrameLayout(this)
            )
        }
        btnPrevChapter.isEnabled = chapterNumber > 1
    }

    private fun formatNumber(number: Int): String {
        return when {
            number >= 1_000 -> String.format("%.1fK", number / 1_000.0)
            else -> number.toString()
        }
    }

    private fun showLoading(show: Boolean) {
        progressBar.visibility = if (show) View.VISIBLE else View.GONE
        contentLayout.visibility = if (show) View.GONE else View.VISIBLE
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

    override fun onDestroy() {
        super.onDestroy()
        stopReading()
        tts?.stop()
        tts?.shutdown()
        translator?.close()
        voiceBottomSheet?.dismiss()
    }

    override fun onPause() {
        super.onPause()
        if (tts?.isSpeaking == true) pauseReading()
    }

    inner class ChapterListAdapter(
        private val chapters: List<ChapterSummary>,
        private val onChapterClick: (ChapterSummary) -> Unit
    ) : RecyclerView.Adapter<ChapterListAdapter.ChapterViewHolder>() {

        inner class ChapterViewHolder(view: View) : RecyclerView.ViewHolder(view) {
            val tvChapterNumber: TextView = view.findViewById(R.id.tvChapterNumber)
            val tvChapterTitle: TextView = view.findViewById(R.id.tvChapterTitle)
            val tvChapterStatus: TextView = view.findViewById(R.id.tvChapterStatus)
        }

        override fun onCreateViewHolder(parent: android.view.ViewGroup, viewType: Int): ChapterViewHolder {
            val view = android.view.LayoutInflater.from(parent.context)
                .inflate(R.layout.item_chapter_list, parent, false)
            return ChapterViewHolder(view)
        }

        override fun onBindViewHolder(holder: ChapterViewHolder, position: Int) {
            val chapter = chapters[position]
            holder.tvChapterNumber.text = if (chapter.ChapterNumber == 0) "Prologue" else "Chapter ${chapter.ChapterNumber}"
            holder.tvChapterTitle.text = if (chapter.Title.isNullOrEmpty()) "Untitled" else chapter.Title
            val isFree = !chapter.IsPremium || (chapter.UnlockPrice ?: 0) == 0
            holder.tvChapterStatus.text = if (isFree) "✓" else "🔒"
            holder.tvChapterStatus.setTextColor(
                if (isFree) android.graphics.Color.parseColor("#77dd77")
                else android.graphics.Color.parseColor("#999999")
            )
            if (chapter.ChapterNumber == chapterNumber) {
                holder.itemView.setBackgroundColor(android.graphics.Color.parseColor("#333333"))
            } else {
                holder.itemView.setBackgroundColor(android.graphics.Color.parseColor("#252525"))
            }
            holder.itemView.setOnClickListener { onChapterClick(chapter) }
        }

        override fun getItemCount() = chapters.size
    }
}