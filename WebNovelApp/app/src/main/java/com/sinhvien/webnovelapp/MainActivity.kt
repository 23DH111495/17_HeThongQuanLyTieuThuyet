package com.sinhvien.webnovelapp

import android.content.Intent
import android.os.Bundle
import android.widget.ImageView
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import com.google.android.material.bottomnavigation.BottomNavigationView
import com.sinhvien.webnovelapp.admin.AdminPanelActivity
import com.sinhvien.webnovelapp.fragments.HomeFragment

class MainActivity : AppCompatActivity() {

    private lateinit var bottomNavigation: BottomNavigationView
    private lateinit var btnSearch: ImageView
    private lateinit var btnMore: ImageView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        // Set up toolbar
        setSupportActionBar(findViewById(R.id.toolbar))
        supportActionBar?.setDisplayShowTitleEnabled(false)

        // Initialize views
        initializeViews()

        // Set up bottom navigation
        setupBottomNavigation()

        // Set up button clicks
        setupButtons()

        // Load default fragment (Home)
        if (savedInstanceState == null) {
            loadFragment(HomeFragment())
        }
    }

    private fun initializeViews() {
        bottomNavigation = findViewById(R.id.bottom_navigation)
        btnSearch = findViewById(R.id.btnSearch)
        btnMore = findViewById(R.id.btnMore)
    }

    private fun setupBottomNavigation() {
        bottomNavigation.setOnItemSelectedListener { item ->
            when (item.itemId) {
                R.id.nav_home -> {
                    loadFragment(HomeFragment())
                    true
                }
//                R.id.nav_library -> {
//                    loadFragment(LibraryFragment())
//                    true
//                }
//                R.id.nav_browse -> {
//                    loadFragment(BrowseFragment())
//                    true
//                }
//                R.id.nav_profile -> {
//                    loadFragment(ProfileFragment())
//                    true
//                }
                else -> false
            }
        }
    }

    private fun setupButtons() {
        // Search button - navigate to SearchActivity
        btnSearch.setOnClickListener {
            val intent = Intent(this, SearchActivity::class.java)
            startActivity(intent)
        }

        // More options button - navigate to AdminPanel
        btnMore.setOnClickListener {
            val intent = Intent(this, AdminPanelActivity::class.java)
            startActivity(intent)
        }
    }

    private fun loadFragment(fragment: Fragment) {
        supportFragmentManager.beginTransaction()
            .replace(R.id.fragment_container, fragment)
            .commit()
    }
}