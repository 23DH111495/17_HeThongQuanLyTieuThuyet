package com.sinhvien.webnovelapp.models;

import com.google.gson.annotations.SerializedName;

public class LoginResponse {
    @SerializedName("message")
    private String message;

    @SerializedName("id")
    private int id;

    @SerializedName("username")
    private String username;

    @SerializedName("email")
    private String email;

    // THÊM TRƯỜNG TOKEN
    @SerializedName("token")
    private String token;

    // Thêm getters
    public String getMessage() {
        return message;
    }

    public int getId() {
        return id;
    }

    public String getUsername() {
        return username;
    }

    public String getEmail() {
        return email;
    }

    public String getToken() {
        return token;
    }
    private int readerId;
    public int getReaderId() { return readerId; }
}
