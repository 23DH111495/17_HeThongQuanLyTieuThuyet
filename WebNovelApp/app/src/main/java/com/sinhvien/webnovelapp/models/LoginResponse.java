package com.sinhvien.webnovelapp.models;

public class LoginResponse {
    private String username;
    private String email;
    private boolean success;

    // Có thể thêm các field khác nếu API trả về
    public String getUsername() { return username; }
    public String getEmail() { return email; }
    public boolean isSuccess() { return success; }
}
