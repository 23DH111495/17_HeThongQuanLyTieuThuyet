package com.sinhvien.webnovelapp.QuocDuy;

public class GenreUserAuthor {
    private int id;
    private String penName;

    public GenreUserAuthor(int id, String penName) {
        this.id = id;
        this.penName = penName;
    }

    public int getId() { return id; }
    public String getPenName() { return penName; }

    public void setId(int id) { this.id = id; }
    public void setPenName(String penName) { this.penName = penName; }
}

