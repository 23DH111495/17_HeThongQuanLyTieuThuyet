package com.sinhvien.webnovelapp.models;

import com.google.gson.annotations.SerializedName;

// Model này khớp với class Wallet.cs bên server
public class Wallet {
    @SerializedName("id")
    private int id;

    @SerializedName("userId")
    private int userId;

    @SerializedName("coinBalance")
    private int coinBalance;

    @SerializedName("totalCoinsEarned")
    private int totalCoinsEarned;

    @SerializedName("totalCoinsSpent")
    private int totalCoinsSpent;

    // Thêm getters
    public int getId() {
        return id;
    }

    public int getUserId() {
        return userId;
    }

    public int getCoinBalance() {
        return coinBalance;
    }

    public int getTotalCoinsEarned() {
        return totalCoinsEarned;
    }

    public int getTotalCoinsSpent() {
        return totalCoinsSpent;
    }
}
