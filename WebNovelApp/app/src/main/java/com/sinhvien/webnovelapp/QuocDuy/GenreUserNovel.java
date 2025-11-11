package com.sinhvien.webnovelapp.QuocDuy;

import java.util.List;

public class GenreUserNovel {
    private int id;
    private String title;
    private String authorName;
    private List<String> genres;

    public GenreUserNovel(int id, String title, String authorName, List<String> genres) {
        this.id = id;
        this.title = title;
        this.authorName = authorName;
        this.genres = genres;
    }

    public int getId() { return id; }
    public String getTitle() { return title; }

    public String getAuthorName() {
        return authorName;
    }

    public List<String> getGenres() { return genres; }

    public void setId(int id) { this.id = id; }
    public void setTitle(String title) { this.title = title; }

    public void setAuthorName(String authorName) {
        this.authorName = authorName;
    }

    public void setGenres(List<String> genres) { this.genres = genres; }
}
