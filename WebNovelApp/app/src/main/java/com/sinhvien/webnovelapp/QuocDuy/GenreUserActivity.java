package com.sinhvien.webnovelapp.QuocDuy;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;
import com.sinhvien.webnovelapp.R;
import com.sinhvien.webnovelapp.activities.NovelDetailActivity;
import com.sinhvien.webnovelapp.api.ApiClient;
import com.sinhvien.webnovelapp.api.GenreApiService;
import com.sinhvien.webnovelapp.api.NovelApiService;
import com.sinhvien.webnovelapp.models.Genre;
import com.sinhvien.webnovelapp.models.GenreResponse;
import com.sinhvien.webnovelapp.models.Novel;
import com.sinhvien.webnovelapp.models.NovelResponse;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class GenreUserActivity extends AppCompatActivity {
    private ChipGroup chipGroupGenres;
    private Button btnFilter;
    private RecyclerView recyclerNovels;

    private List<Genre> genreList = new ArrayList<>();
    private List<GenreUserNovel> allNovels = new ArrayList<>();
    private List<GenreUserAuthor> allAuthors = new ArrayList<>();

    private GenreUserAdapter novelAdapter;
    private GenreApiService apiService;  // ✅ Retrofit service

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_genre_user);

        chipGroupGenres = findViewById(R.id.chipGroupGenres);
        btnFilter = findViewById(R.id.btnFilter);
        recyclerNovels = findViewById(R.id.recyclerNovels);

        recyclerNovels.setLayoutManager(new LinearLayoutManager(this));
        novelAdapter = new GenreUserAdapter(allNovels);
        recyclerNovels.setAdapter(novelAdapter);

        // ✅ Khởi tạo Retrofit service
        apiService = ApiClient.INSTANCE.getClient().create(GenreApiService.class);

        // ✅ Gọi API để tải danh sách thể loại từ server
        loadGenresFromApi();

        // ✅ Gọi API để tải danh sách truyện (tùy bạn có endpoint nào)
        loadNovelsFromApi();

        btnFilter.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                List<String> selectedGenres = getSelectedGenres();
                if (selectedGenres.isEmpty()) {
                    Toast.makeText(GenreUserActivity.this, "Bạn chưa chọn thể loại nào!", Toast.LENGTH_SHORT).show();
                    novelAdapter.updateList(allNovels);
                } else {
                    filterNovelsByGenres(selectedGenres);
                }
            }
        });
        novelAdapter.setOnItemClickListener(new GenreUserAdapter.OnItemClickListener() {
            @Override
            public void onItemClick(GenreUserNovel novel) {
                Intent intent = new Intent(GenreUserActivity.this, NovelDetailActivity.class);
                intent.putExtra("NOVEL_ID", novel.getId());
                startActivity(intent);
            }
        });
    }


    // 🔹 Hàm gọi API lấy danh sách thể loại
    private void loadGenresFromApi() {
        apiService.getGenres("", true).enqueue(new Callback<GenreResponse>() {
            @Override
            public void onResponse(Call<GenreResponse> call, Response<GenreResponse> response) {
                if (response.isSuccessful() && response.body() != null && response.body().getSuccess()) {
                    genreList = response.body().getData();
                    displayGenreChips();
                } else {
                    Toast.makeText(GenreUserActivity.this, "Không tải được danh sách thể loại!", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<GenreResponse> call, Throwable t) {
                Toast.makeText(GenreUserActivity.this, "Lỗi kết nối: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    // 🔹 Hiển thị danh sách genre thành các chip
    private void displayGenreChips() {
        chipGroupGenres.removeAllViews();
        for (Genre genre : genreList) {
            Chip chip = new Chip(this);
            chip.setText(genre.getName());
            chip.setCheckable(true);
            chip.setTextColor(getResources().getColor(android.R.color.white));
            chip.setChipBackgroundColorResource(R.color.genre_chip_selector);
            chipGroupGenres.addView(chip);
        }
    }

    // 🔹 Lấy danh sách chip được chọn
    private List<String> getSelectedGenres() {
        List<String> selectedGenres = new ArrayList<>();
        for (int i = 0; i < chipGroupGenres.getChildCount(); i++) {
            Chip chip = (Chip) chipGroupGenres.getChildAt(i);
            if (chip.isChecked()) {
                selectedGenres.add(chip.getText().toString());
            }
        }
        return selectedGenres;
    }

    // 🔹 Lọc danh sách truyện theo thể loại
    private void filterNovelsByGenres(List<String> selectedGenres) {
        List<GenreUserNovel> filtered = new ArrayList<>();
        for (GenreUserNovel novel : allNovels) {
            for (String g : novel.getGenres()) {
                if (selectedGenres.contains(g)) {
                    filtered.add(novel);
                    break;
                }
            }
        }
        novelAdapter.updateList(filtered);
    }
    private void loadNovelsFromApi() {
        ApiClient.INSTANCE.getClient()
                .create(NovelApiService.class)
                .getNovels("", "", null, "updated", 1, 50)
                .enqueue(new Callback<NovelResponse>() {
                    @Override
                    public void onResponse(Call<NovelResponse> call, Response<NovelResponse> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().getSuccess()) {
                            List<Novel> novels = response.body().getData();
                            allNovels = convertToGenreUserNovel(novels);
                            novelAdapter.updateList(allNovels);
                        } else {
                            Toast.makeText(GenreUserActivity.this, "Không tải được danh sách truyện!", Toast.LENGTH_SHORT).show();
                        }
                    }

                    @Override
                    public void onFailure(Call<NovelResponse> call, Throwable t) {
                        Toast.makeText(GenreUserActivity.this, "Lỗi kết nối: " + t.getMessage(), Toast.LENGTH_LONG).show();
                    }
                });
    }
    private List<GenreUserNovel> convertToGenreUserNovel(List<Novel> novels) {
        List<GenreUserNovel> result = new ArrayList<>();
        for (Novel n : novels) {
            result.add(new GenreUserNovel(
                    n.getId(),
                    n.getTitle(),
                    n.getAuthorName(),
                    n.getGenres() != null ? n.getGenres() : new ArrayList<>()
            ));
        }
        return result;
    }

}

