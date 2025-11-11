package com.sinhvien.webnovelapp.fragments

import android.content.Intent
import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.LinearLayout
import android.widget.TextView // (SỬA LỖI 1) Thêm import cho TextView
import com.sinhvien.webnovelapp.CongThuong.Coin.CoinPackageActivity
import com.sinhvien.webnovelapp.R
// (SỬA LỖI 2) Thêm import cho BookmarkActivity (Giả sử package là .activities)
import com.sinhvien.webnovelapp.PhuocKhang.BookmarkActivity

class ProfileFragment : Fragment() {


    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_profile2, container, false)
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)


        val buyCoinsLayout = view.findViewById<LinearLayout>(R.id.layoutBuyCoins)


        buyCoinsLayout.setOnClickListener {

            val intent = Intent(activity, CoinPackageActivity::class.java)
            startActivity(intent)
        }

        val bookmarkButton = view.findViewById<TextView>(R.id.btnBookmark)

        // 2. Thêm sự kiện click
        bookmarkButton.setOnClickListener {
            // Mở BookmarkActivity
            val intent = Intent(activity, BookmarkActivity::class.java)
            startActivity(intent) // (SỬA LỖI 3) Phải gọi startActivity
        }
    }
    // (SỬA LỖI 4) Xóa dấu '}' thừa ở đây (đây là nguyên nhân gây lỗi 'companion')


    companion object {

    }
}