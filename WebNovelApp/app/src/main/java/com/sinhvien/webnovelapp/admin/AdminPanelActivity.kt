package com.sinhvien.webnovelapp.admin

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.view.Menu
import android.view.MenuItem
import androidx.appcompat.app.AppCompatActivity
import androidx.cardview.widget.CardView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.ThieuKhang.MainActivity
import com.sinhvien.webnovelapp.activities.NovelListActivity

class AdminPanelActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_admin_panel)

        // Set up toolbar
        setSupportActionBar(findViewById(R.id.toolbar))
        supportActionBar?.setDisplayHomeAsUpEnabled(true)
        supportActionBar?.title = "Admin Panel"

        // Set up click listeners for cards
        setupCardClickListeners()
    }

    private fun setupCardClickListeners() {
        // Novels card
        findViewById<CardView>(R.id.cardNovels).setOnClickListener {
            // TODO: Navigate to Novel Management Activity
             val intent = Intent(this, NovelListActivity::class.java)
             startActivity(intent)
        }

        // Genres card
        findViewById<CardView>(R.id.cardGenres).setOnClickListener {
            val intent = Intent(this, GenreManagementActivity::class.java)
            startActivity(intent)
        }

        // Tags card
        findViewById<CardView>(R.id.cardTags).setOnClickListener {
            // TODO: Navigate to Tag Management Activity
             val intent = Intent(this, TagManagementActivity::class.java)
             startActivity(intent)
        }

        // Settings card
        findViewById<CardView>(R.id.cardSettings).setOnClickListener {
            // TODO: Navigate to Admin Settings Activity
            // val intent = Intent(this, AdminSettingsActivity::class.java)
            // startActivity(intent)
        }

        // Logout card
        findViewById<CardView>(R.id.cardLogout).setOnClickListener {
            logoutAdmin()
        }
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        menuInflater.inflate(R.menu.admin_menu, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        return when (item.itemId) {
            android.R.id.home -> {
                onBackPressed()
                true
            }
            R.id.action_logout -> {
                logoutAdmin()
                true
            }
            R.id.action_settings -> {
                // TODO: Add admin settings
                true
            }
            else -> super.onOptionsItemSelected(item)
        }
    }

    private fun logoutAdmin() {
        val prefs = getSharedPreferences("AdminPrefs", Context.MODE_PRIVATE)
        prefs.edit().putBoolean("admin_logged_in", false).apply()

        val intent = Intent(this, MainActivity::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
        startActivity(intent)
        finish()
    }
}