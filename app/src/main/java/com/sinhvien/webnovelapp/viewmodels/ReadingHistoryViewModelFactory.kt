package com.sinhvien.webnovelapp.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.sinhvien.webnovelapp.ThieuKhang.ReadingHistoryRepository

class ReadingHistoryViewModelFactory(
    private val repository: ReadingHistoryRepository
) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(ReadingHistoryViewModel::class.java)) {
            return ReadingHistoryViewModel(repository) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}