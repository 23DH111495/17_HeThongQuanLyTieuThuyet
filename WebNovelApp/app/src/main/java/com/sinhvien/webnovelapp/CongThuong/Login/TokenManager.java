package com.sinhvien.webnovelapp.CongThuong.Login; // <-- DÒNG QUAN TRỌNG NHẤT

import android.content.Context;
import android.content.SharedPreferences;

public class TokenManager {
    private static final String PREF_NAME = "WebNovelAppPrefs";
    private static final String KEY_AUTH_TOKEN = "AUTH_TOKEN";

    private SharedPreferences prefs;
    private SharedPreferences.Editor editor;

    private static TokenManager INSTANCE = null;
    private Context context;

    private TokenManager(Context context) {
        this.context = context.getApplicationContext();
        prefs = this.context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        editor = prefs.edit();
    }

    public static synchronized TokenManager getInstance(Context context) {
        if (INSTANCE == null) {
            INSTANCE = new TokenManager(context);
        }
        return INSTANCE;
    }

    public void saveToken(String token) {
        editor.putString(KEY_AUTH_TOKEN, token);
        editor.apply();
    }

    public String getToken() {
        return prefs.getString(KEY_AUTH_TOKEN, null);
    }

    public void clearToken() {
        editor.remove(KEY_AUTH_TOKEN);
        editor.apply();
    }
}
