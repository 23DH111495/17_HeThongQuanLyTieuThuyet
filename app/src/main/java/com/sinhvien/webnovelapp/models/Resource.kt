package com.sinhvien.webnovelapp.models

/**
 * A generic sealed class that represents the result of an operation
 * Used for handling API responses with success, error, and loading states
 */
sealed class Resource<T>(
    val data: T? = null,
    val message: String? = null
) {
    class Success<T>(data: T) : Resource<T>(data)
    class Error<T>(message: String, data: T? = null) : Resource<T>(data, message)
    class Loading<T>(data: T? = null) : Resource<T>(data)
}