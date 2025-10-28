package com.sinhvien.webnovelapp.CongThuong.Coin;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.sinhvien.webnovelapp.R;
import com.sinhvien.webnovelapp.api.CoinPackageApi;
import com.sinhvien.webnovelapp.api.LoginApiClient;
import com.sinhvien.webnovelapp.models.CoinPackage;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CoinPackageAdapter extends RecyclerView.Adapter<CoinPackageAdapter.ViewHolder> {

    private List<CoinPackage> packages;
    private Context context;

    public CoinPackageAdapter(Context context, List<CoinPackage> packages) {
        this.context = context;
        this.packages = packages;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(context).inflate(R.layout.item_coin_package, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        CoinPackage pkg = packages.get(position);
        holder.txtName.setText(pkg.getName());
        holder.txtCoins.setText("Coin: " + pkg.getCoinAmount() + " (+Bonus: " + pkg.getBonusCoins() + ")");
        holder.txtPrice.setText("Giá: " + pkg.getPriceVND() + " VND");

        holder.btnBuy.setOnClickListener(v -> {
            CoinPackageApi api = LoginApiClient.getClient().create(CoinPackageApi.class);
            api.buyPackage(pkg.getId()).enqueue(new Callback<Void>() {
                @Override
                public void onResponse(Call<Void> call, Response<Void> response) {
                    Toast.makeText(context, "Mua gói " + pkg.getName() + " thành công!", Toast.LENGTH_SHORT).show();
                }

                @Override
                public void onFailure(Call<Void> call, Throwable t) {
                    Toast.makeText(context, "Lỗi khi mua gói!", Toast.LENGTH_SHORT).show();
                }
            });
        });
    }

    @Override
    public int getItemCount() {
        return packages.size();
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
