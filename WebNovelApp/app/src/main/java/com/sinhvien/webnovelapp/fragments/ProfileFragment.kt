package com.sinhvien.webnovelapp.fragments

import android.content.Intent
import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.LinearLayout
import com.sinhvien.webnovelapp.CongThuong.Coin.CoinPackageActivity
import com.sinhvien.webnovelapp.R


class ProfileFragment : Fragment() {


    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_profile2, container, false)
    }

    // THÊM HÀM NÀY
    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)


        val buyCoinsLayout = view.findViewById<LinearLayout>(R.id.layoutBuyCoins)


        buyCoinsLayout.setOnClickListener {

            val intent = Intent(activity, CoinPackageActivity::class.java)
            startActivity(intent)
        }

    }

    companion object {

    }
}