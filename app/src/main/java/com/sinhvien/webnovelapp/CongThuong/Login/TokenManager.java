package com.sinhvien.webnovelapp.CongThuong.Login;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Base64;
import org.json.JSONObject;

public class TokenManager {
    private static final String PREF_NAME = "WebNovelAppPrefs";
    private static final String KEY_AUTH_TOKEN = "AUTH_TOKEN";
    private static final String KEY_USERNAME = "USERNAME";
    private static final String KEY_EMAIL = "EMAIL";
    private static final String KEY_USER_ID = "USER_ID";
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

    public void saveUserSession(String token, int id, String username, String email) {
        editor.putString(KEY_AUTH_TOKEN, token);
        editor.putInt(KEY_USER_ID, id);
        editor.putString(KEY_USERNAME, username);
        editor.putString(KEY_EMAIL, email);
        editor.apply();
    }

    public void saveToken(String token) {
        editor.putString(KEY_AUTH_TOKEN, token);
        editor.apply();
    }

    public String getToken() {
        String token = prefs.getString(KEY_AUTH_TOKEN, null);

        // Validate token before returning
        if (token != null && isTokenExpired(token)) {
            android.util.Log.e("TokenManager", "Token has expired, clearing session");
            clearToken();
            return null;
        }

        return token;
    }

    public boolean isTokenExpired(String token) {
        try {
            String[] parts = token.split("\\.");
            if (parts.length != 3) return true;

            String payload = new String(Base64.decode(parts[1], Base64.URL_SAFE));
            JSONObject jsonObject = new JSONObject(payload);

            long exp = jsonObject.getLong("exp");
            long currentTime = System.currentTimeMillis() / 1000;

            android.util.Log.d("TokenManager", "Token exp: " + exp + ", Current time: " + currentTime);

            return currentTime >= exp;
        } catch (Exception e) {
            android.util.Log.e("TokenManager", "Error checking token expiration", e);
            return true;
        }
    }

    public boolean isLoggedIn() {
        String token = getToken();
        return token != null && !isTokenExpired(token);
    }

    public String getUsername() { return prefs.getString(KEY_USERNAME, "Người dùng"); }

    public String getEmail() { return prefs.getString(KEY_EMAIL, ""); }

    public int getUserId() { return prefs.getInt(KEY_USER_ID, 0); }

    public void clearToken() {
        editor.clear();
        editor.apply();
    }
}