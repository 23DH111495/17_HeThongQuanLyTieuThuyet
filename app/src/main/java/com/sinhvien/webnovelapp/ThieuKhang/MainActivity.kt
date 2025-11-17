package com.sinhvien.webnovelapp.ThieuKhang

import android.content.Intent
import android.os.Bundle
import android.widget.ImageView
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import com.google.android.material.bottomnavigation.BottomNavigationView
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.admin.AdminPanelActivity
import com.sinhvien.webnovelapp.fragments.HomeFragment
import com.sinhvien.webnovelapp.fragments.ProfileFragment
import com.sinhvien.webnovelapp.QuocDuy.GenreUserActivity

class MainActivity : AppCompatActivity() {

    private lateinit var bottomNavigation: BottomNavigationView
    private lateinit var btnSearch: ImageView
    private lateinit var btnMore: ImageView

    // Keep references to fragments to avoid recreation
    private var homeFragment: HomeFragment? = null
    private var profileFragment: ProfileFragment? = null
    private var activeFragment: Fragment? = null

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

        // Load default fragment (Home) only if savedInstanceState is null
        if (savedInstanceState == null) {
            homeFragment = HomeFragment()
            loadFragment(homeFragment!!)
        } else {
            // Restore fragment references after configuration change
            homeFragment = supportFragmentManager.findFragmentByTag("HOME") as? HomeFragment
            profileFragment = supportFragmentManager.findFragmentByTag("PROFILE") as? ProfileFragment

            // Find which fragment is currently visible
            activeFragment = when {
                homeFragment?.isVisible == true -> homeFragment
                profileFragment?.isVisible == true -> profileFragment
                else -> null
            }
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
                    // Reuse existing fragment or create new one
                    if (homeFragment == null) {
                        homeFragment = HomeFragment()
                    }

                    // Only switch if not already showing this fragment
                    if (activeFragment != homeFragment) {
                        loadFragment(homeFragment!!, "HOME")
                    }
                    true
                }

                R.id.nav_browse -> {
                    // Open GenreUserActivity without changing fragment
                    val intent = Intent(this, GenreUserActivity::class.java)
                    startActivity(intent)
                    false // Don't change selected item
                }

                R.id.nav_profile -> {
                    // Reuse existing fragment or create new one
                    if (profileFragment == null) {
                        profileFragment = ProfileFragment()
                    }

                    // Only switch if not already showing this fragment
                    if (activeFragment != profileFragment) {
                        loadFragment(profileFragment!!, "PROFILE")
                    }
                    true
                }

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

    private fun loadFragment(fragment: Fragment, tag: String? = null) {
        val transaction = supportFragmentManager.beginTransaction()

        // Hide current active fragment if exists
        activeFragment?.let {
            transaction.hide(it)
        }

        // Add or show the target fragment
        if (fragment.isAdded) {
            transaction.show(fragment)
        } else {
            transaction.add(R.id.fragment_container, fragment, tag)
        }

        transaction.commit()
        activeFragment = fragment
    }

    override fun onSaveInstanceState(outState: Bundle) {
        super.onSaveInstanceState(outState)
        // Fragment manager will automatically save fragment states
    }
}