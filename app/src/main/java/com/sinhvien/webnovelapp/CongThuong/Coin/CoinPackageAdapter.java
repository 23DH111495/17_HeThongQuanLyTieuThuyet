package com.sinhvien.webnovelapp.CongThuong.Coin;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.sinhvien.webnovelapp.CongThuong.Payment.PaymentActivity;
import com.sinhvien.webnovelapp.R;
import com.sinhvien.webnovelapp.models.CoinPackage;

import java.util.List;

public class CoinPackageAdapter extends RecyclerView.Adapter<CoinPackageAdapter.ViewHolder> {
    private final Activity activity;
    private final List<CoinPackage> packages;

    public CoinPackageAdapter(Activity activity, List<CoinPackage> packages) {
        this.activity = activity;
        this.packages = packages;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        // ✅ Dùng parent.getContext() thay vì context để tránh null
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.item_coin_package, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        CoinPackage pkg = packages.get(position);
        holder.txtName.setText(pkg.getName());
        holder.txtCoins.setText("Coin: " + pkg.getCoinAmount() + " (+Bonus: " + pkg.getBonusCoins() + ")");
        holder.txtPrice.setText("Giá: " + pkg.getPriceVND() + " VND");

        holder.btnBuy.setOnClickListener(v -> {
            double amount = pkg.getPriceVND(); // VNĐ
            Intent intent = new Intent(activity, PaymentActivity.class);
            intent.putExtra("totalBill", amount);
            intent.putExtra("packageId", pkg.getId());
            activity.startActivityForResult(intent, 123);
        });
    }

    @Override
    public int getItemCount() {
        return packages != null ? packages.size() : 0;
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView txtName, txtCoins, txtPrice;
        Button btnBuy;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            txtName = itemView.findViewById(R.id.txtName);
            txtCoins = itemView.findViewById(R.id.txtCoins);
            txtPrice = itemView.findViewById(R.id.txtPrice);
            btnBuy = itemView.findViewById(R.id.btnBuy);
        }
    }
}
