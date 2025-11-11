package com.sinhvien.webnovelapp.QuocDuy;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.sinhvien.webnovelapp.R;

import java.util.List;

public class GenreUserAdapter extends RecyclerView.Adapter<GenreUserAdapter.NovelViewHolder> {

    private List<GenreUserNovel> novelList;
    private List<GenreUserAuthor> authorList;
    private OnItemClickListener listener;
    public void updateList(List<GenreUserNovel> newList) {
        this.novelList = newList;
        notifyDataSetChanged();
    }
    public GenreUserAdapter(List<GenreUserNovel> novelList, List<GenreUserAuthor> authorList) {
        this.novelList = novelList;
        this.authorList = authorList;
    }
    public GenreUserAdapter(List<GenreUserNovel> novelList) {
        this.novelList = novelList;
    }

    public interface OnItemClickListener {
        void onItemClick(GenreUserNovel novel);
    }
    public void setOnItemClickListener(OnItemClickListener listener) {
        this.listener = listener;
    }

    @NonNull
    @Override
    public NovelViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.genre_user_item, parent, false);
        return new NovelViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull NovelViewHolder holder, int position) {
        GenreUserNovel novel = novelList.get(position);
        holder.tvNovelTitle.setText(novel.getTitle());
        holder.tvAuthorName.setText(novel.getAuthorName());
        holder.itemView.setOnClickListener(v -> {
            if (listener != null) {
                listener.onItemClick(novel);
            }
        });


        // Không set thể loại nữa
        // holder.tvGenres.setText(String.join(", ", novel.getGenres()));
    }

    @Override
    public int getItemCount() {
        return novelList.size();
    }

    public static class NovelViewHolder extends RecyclerView.ViewHolder {
        TextView tvNovelTitle, tvAuthorName, tvGenres;

        public NovelViewHolder(@NonNull View itemView) {
            super(itemView);
            tvNovelTitle = itemView.findViewById(R.id.tvNovelTitle);
            tvAuthorName = itemView.findViewById(R.id.tvAuthorName);

        }
    }

    private String getAuthorName(int authorId) {
        for (GenreUserAuthor a : authorList) {
            if (a.getId() == authorId) return a.getPenName();
        }
        return "Unknown";
    }

}

