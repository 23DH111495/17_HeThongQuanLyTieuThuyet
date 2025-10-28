function handleImageError(userId, profilePictureUrl, userInitial) {
    var img = document.getElementById('img-' + userId);

    // Try URL image if available
    if (profilePictureUrl && profilePictureUrl.trim() !== '') {
        img.onerror = function () {
            // If URL image also fails, show username initial
            showUserInitial(userId, userInitial);
        };
        img.src = profilePictureUrl;
    } else {
        // No URL available, show username initial
        showUserInitial(userId, userInitial);
    }
}

function showUserInitial(userId, userInitial) {
    var img = document.getElementById('img-' + userId);
    var avatar = img.parentElement;

    // Hide the image
    img.style.display = 'none';

    // Create and show initial div
    var initialDiv = document.createElement('div');
    initialDiv.style.cssText = 'width: 100%; height: 100%; border-radius: 50%; background-color: #77dd77; color: black; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 18px;';
    initialDiv.textContent = userInitial;

    avatar.appendChild(initialDiv);
}