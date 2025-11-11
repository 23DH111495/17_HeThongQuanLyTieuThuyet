package com.sinhvien.webnovelapp.fragments

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.TextView
import androidx.core.content.ContextCompat
import androidx.fragment.app.Fragment
import androidx.fragment.app.FragmentTransaction
import com.google.android.material.tabs.TabLayout
import com.sinhvien.webnovelapp.PhuocKhang.BookmarksRankingFragment
import com.sinhvien.webnovelapp.PhuocKhang.RatingRankingFragment
import com.sinhvien.webnovelapp.PhuocKhang.ViewsRankingFragment
import com.sinhvien.webnovelapp.R
import com.sinhvien.webnovelapp.databinding.FragmentRankingBinding
import kotlin.collections.indices
import kotlin.ranges.until

class RankingFragment : Fragment() {

    private var _binding: FragmentRankingBinding? = null
    private val binding get() = _binding!!

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View {
        // Sử dụng View Binding để inflate layout cha (fragment_ranking.xml)
        _binding = FragmentRankingBinding.inflate(inflater, container, false)
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        // Setup Tabs (sẽ dùng custom view)
        setupTabsWithCustomView()
    }

    // (GỘP) Logic tạo Tab với Custom View và Icon/Text
    private fun setupTabsWithCustomView() {

        // Cần tham chiếu đến tabLayoutRanking từ binding
        val tabLayout = binding.tabLayoutRanking

        val tabs = listOf("Views", "Rating", "Bookmarks")
        // Đảm bảo bạn có các drawable này trong res/drawable
        val icons = listOf(R.drawable.icon_view, R.drawable.icon_rating, R.drawable.icon_bookmark)

        for (i in tabs.indices) {
            val tab = tabLayout.newTab()
            // Inflate custom layout item
            val tabView = LayoutInflater.from(requireContext())
                .inflate(R.layout.custom_tab_item, null)

            val icon = tabView.findViewById<ImageView>(R.id.tabIcon)
            val text = tabView.findViewById<TextView>(R.id.tabText)

            icon.setImageResource(icons[i])
            text.text = tabs[i]

            tab.customView = tabView
            tabLayout.addTab(tab)
        }

        // Mặc định là Views (Phải gọi sau khi thêm tabs)
        replaceFragment(ViewsRankingFragment())
        highlightSelectedTab(0) // Tô màu tab đầu tiên

        // Xử lý khi click đổi tab
        tabLayout.addOnTabSelectedListener(object : TabLayout.OnTabSelectedListener {
            override fun onTabSelected(tab: TabLayout.Tab) {
                highlightSelectedTab(tab.position) // Tô màu tab mới chọn
                when (tab.position) {
                    0 -> replaceFragment(ViewsRankingFragment())
                    1 -> replaceFragment(RatingRankingFragment())
                    2 -> replaceFragment(BookmarksRankingFragment())
                }
            }

            override fun onTabUnselected(tab: TabLayout.Tab) {
                // Đổi màu tab cũ về màu xám
                val icon = tab.customView?.findViewById<ImageView>(R.id.tabIcon)
                val text = tab.customView?.findViewById<TextView>(R.id.tabText)
                val gray = ContextCompat.getColor(requireContext(), R.color.ranking_text_secondary)

                icon?.setColorFilter(gray)
                text?.setTextColor(gray)
            }

            override fun onTabReselected(tab: TabLayout.Tab) {}
        })
    }

    // (GỘP) Hàm tô màu Icon/Text
    private fun highlightSelectedTab(position: Int) {
        // Dùng binding.tabLayoutRanking thay vì tabLayout (private lateinit var)
        val tabLayout = binding.tabLayoutRanking

        for (i in 0 until tabLayout.tabCount) {
            val tab = tabLayout.getTabAt(i)
            val icon = tab?.customView?.findViewById<ImageView>(R.id.tabIcon)
            val text = tab?.customView?.findViewById<TextView>(R.id.tabText)

            if (i == position) {
                val green = ContextCompat.getColor(requireContext(), R.color.ranking_green_label)
                icon?.setColorFilter(green)
                text?.setTextColor(green)
            } else {
                val gray = ContextCompat.getColor(requireContext(), R.color.ranking_text_secondary)
                icon?.setColorFilter(gray)
                text?.setTextColor(gray)
            }
        }
    }

    private fun replaceFragment(fragment: Fragment) {
        val transaction: FragmentTransaction = childFragmentManager.beginTransaction()
        transaction.replace(R.id.rankingContentContainer, fragment)
        transaction.commit()
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }
}