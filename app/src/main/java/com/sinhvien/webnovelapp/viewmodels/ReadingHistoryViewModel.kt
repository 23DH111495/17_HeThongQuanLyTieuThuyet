package com.sinhvien.webnovelapp.viewmodels

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.sinhvien.webnovelapp.ThieuKhang.ReadingHistoryRepository
import com.sinhvien.webnovelapp.models.ReadingHistoryItem
import com.sinhvien.webnovelapp.models.Resource
import kotlinx.coroutines.launch

class ReadingHistoryViewModel(
    private val repository: ReadingHistoryRepository
) : ViewModel() {

    private val _readingHistory = MutableLiveData<Resource<List<ReadingHistoryItem>>>()
    val readingHistory: LiveData<Resource<List<ReadingHistoryItem>>> = _readingHistory

    private val _currentPage = MutableLiveData(1)
    val currentPage: LiveData<Int> = _currentPage

    private val _hasMore = MutableLiveData(true)
    val hasMore: LiveData<Boolean> = _hasMore

    private var currentStatus = "all"
    private val historyItems = mutableListOf<ReadingHistoryItem>()
    private var isLoading = false // Add flag to prevent concurrent loads

    fun loadReadingHistory(status: String = "all", refresh: Boolean = false) {
        // Prevent duplicate concurrent loads
        if (isLoading) return

        if (refresh) {
            historyItems.clear()
            _currentPage.value = 1
            _hasMore.value = true
            currentStatus = status
        }

        viewModelScope.launch {
            isLoading = true
            _readingHistory.value = Resource.Loading()

            val result = repository.getReadingHistory(
                page = _currentPage.value ?: 1,
                pageSize = 20,
                status = currentStatus
            )

            when (result) {
                is Resource.Success -> {
                    result.data?.let { response ->
                        val newItems = response.data ?: emptyList()

                        if (refresh) {
                            // Replace all items on refresh
                            historyItems.clear()
                            historyItems.addAll(newItems)
                        } else {
                            // Add only new items that don't exist (for pagination)
                            val existingIds = historyItems.map { it.novelId }.toSet()
                            val uniqueNewItems = newItems.filter { it.novelId !in existingIds }
                            historyItems.addAll(uniqueNewItems)
                        }

                        _hasMore.value = (response.pagination?.currentPage ?: 0) < (response.pagination?.totalPages ?: 0)
                        _readingHistory.value = Resource.Success(historyItems.toList())
                    }
                }
                is Resource.Error -> {
                    _readingHistory.value = Resource.Error(result.message ?: "Unknown error")
                }
                is Resource.Loading -> {
                    // Already handled above
                }
            }

            isLoading = false
        }
    }

    fun loadMore() {
        if (_hasMore.value == true && !isLoading) {
            _currentPage.value = (_currentPage.value ?: 1) + 1
            loadReadingHistory(currentStatus, refresh = false)
        }
    }

    fun updateReadingStatus(novelId: Int, status: String, onComplete: (Boolean) -> Unit) {
        viewModelScope.launch {
            val result = repository.updateReadingStatus(novelId, status)
            when (result) {
                is Resource.Success -> {
                    // Update local list
                    val index = historyItems.indexOfFirst { it.novelId == novelId }
                    if (index != -1) {
                        historyItems[index] = historyItems[index].copy(readingStatus = status)
                        _readingHistory.value = Resource.Success(historyItems.toList())
                    }
                    onComplete(true)
                }
                is Resource.Error -> onComplete(false)
                else -> {}
            }
        }
    }

    fun deleteFromHistory(novelId: Int, onComplete: (Boolean) -> Unit) {
        viewModelScope.launch {
            val result = repository.deleteReadingProgress(novelId)
            when (result) {
                is Resource.Success -> {
                    historyItems.removeAll { it.novelId == novelId }
                    _readingHistory.value = Resource.Success(historyItems.toList())
                    onComplete(true)
                }
                is Resource.Error -> onComplete(false)
                else -> {}
            }
        }
    }
}