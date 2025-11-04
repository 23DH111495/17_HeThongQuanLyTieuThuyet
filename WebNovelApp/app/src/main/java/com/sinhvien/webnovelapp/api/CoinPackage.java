package com.sinhvien.webnovelapp.api;

public class CoinPackage {
    private int id;
    private String name;
    private int coinAmount;
    private int bonusCoins;
    private double priceUSD;
    private double priceVND;
    private boolean isFeatured;

    public int getId() { return id; }
    public String getName() { return name; }
    public int getCoinAmount() { return coinAmount; }
    public int getBonusCoins() { return bonusCoins; }
    public double getPriceUSD() { return priceUSD; }
    public double getPriceVND() { return priceVND; }
    public boolean isFeatured() { return isFeatured; }
}
