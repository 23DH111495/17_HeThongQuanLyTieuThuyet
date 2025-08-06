// User_Edit.js - Complete JavaScript functionality for Edit User page

// Password Management Variables
let isPasswordVisible = false;
let currentPassword = "";
let isEditingPassword = false;
let userId = null;

document.addEventListener('DOMContentLoaded', function () {
    // Get user ID from the form or URL
    getUserIdFromPage();

    // Initialize all functionality
    initializeProfilePicture();
    initializePassword();

    // Load real password from database
    loadCurrentPassword();
});

// Get user ID from the page
function getUserIdFromPage() {
    // Try to get from hidden field
    const hiddenIdField = document.querySelector('input[name="Id"]');
    if (hiddenIdField) {
        userId = parseInt(hiddenIdField.value);
        return;
    }

    // Try to get from URL
    const urlPath = window.location.pathname;
    const matches = urlPath.match(/\/EditUser\/(\d+)/);
    if (matches) {
        userId = parseInt(matches[1]);
        return;
    }

    // Try to get from query string
    const urlParams = new URLSearchParams(window.location.search);
    const idParam = urlParams.get('id');
    if (idParam) {
        userId = parseInt(idParam);
    }
}

// Load current password from database
function loadCurrentPassword() {
    if (!userId) {
        console.error('User ID not found');
        document.getElementById('password-display').textContent = 'Error: User ID not found';
        return;
    }

    // Make AJAX call to get password
    fetch(`/Admin/User_Manager/GetUserPassword?id=${userId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.hasPassword) {
                currentPassword = data.password;
                document.getElementById('password-display').textContent = '••••••••••••';
            } else {
                currentPassword = "";
                document.getElementById('password-display').textContent = 'No password set';
                console.warn('No password found for user');
            }
        })
        .catch(error => {
            console.error('Error loading password:', error);
            document.getElementById('password-display').textContent = 'Error loading password';
        });
}

// Password Management Functions
function initializePassword() {
    // Set initial password display
    document.getElementById('password-display').textContent = 'Loading...';

    // Clear password fields initially
    const newPasswordField = document.getElementById('new-password');
    const confirmPasswordField = document.getElementById('confirm-password');

    if (newPasswordField) newPasswordField.value = '';
    if (confirmPasswordField) confirmPasswordField.value = '';
}

function togglePasswordVisibility() {
    const passwordDisplay = document.getElementById('password-display');
    const eyeIcon = document.getElementById('eye-icon');

    if (!currentPassword) {
        showTemporaryMessage('No password to display', 'warning');
        return;
    }

    if (isPasswordVisible) {
        passwordDisplay.textContent = '••••••••••••';
        eyeIcon.className = 'fas fa-eye';
        isPasswordVisible = false;
    } else {
        passwordDisplay.textContent = currentPassword;
        eyeIcon.className = 'fas fa-eye-slash';
        isPasswordVisible = true;
    }
}

function enablePasswordEdit() {
    const currentDisplay = document.getElementById('currentPasswordDisplay');
    const editSection = document.getElementById('password-edit-section');

    currentDisplay.style.display = 'none';
    editSection.style.display = 'block';
    isEditingPassword = true;

    // Focus on new password field
    setTimeout(() => {
        const newPasswordField = document.getElementById('new-password');
        if (newPasswordField) {
            newPasswordField.focus();
        }
    }, 100);
}

function cancelPasswordEdit() {
    const currentDisplay = document.getElementById('currentPasswordDisplay');
    const editSection = document.getElementById('password-edit-section');

    editSection.style.display = 'none';
    currentDisplay.style.display = 'block';
    isEditingPassword = false;

    // Clear the password fields
    const newPasswordField = document.getElementById('new-password');
    const confirmPasswordField = document.getElementById('confirm-password');

    if (newPasswordField) newPasswordField.value = '';
    if (confirmPasswordField) confirmPasswordField.value = '';

    // Reset password visibility
    if (isPasswordVisible) {
        togglePasswordVisibility();
    }
}

function savePasswordEdit() {
    const newPasswordField = document.getElementById('new-password');
    const confirmPasswordField = document.getElementById('confirm-password');

    const newPassword = newPasswordField ? newPasswordField.value : '';
    const confirmPassword = confirmPasswordField ? confirmPasswordField.value : '';

    if (!newPassword) {
        showTemporaryMessage('Please enter a new password', 'error');
        return;
    }

    if (newPassword !== confirmPassword) {
        showTemporaryMessage('Passwords do not match', 'error');
        return;
    }

    if (newPassword.length < 6) {
        showTemporaryMessage('Password must be at least 6 characters long', 'error');
        return;
    }

    // Update current password for display purposes
    currentPassword = newPassword;

    // Hide edit section and show current display
    const currentDisplay = document.getElementById('currentPasswordDisplay');
    const editSection = document.getElementById('password-edit-section');

    editSection.style.display = 'none';
    currentDisplay.style.display = 'block';
    isEditingPassword = false;

    // Update password display
    if (isPasswordVisible) {
        document.getElementById('password-display').textContent = currentPassword;
    } else {
        document.getElementById('password-display').textContent = '••••••••••••';
    }

    // Show success message
    showTemporaryMessage('Password updated successfully!', 'success');
}

function showTemporaryMessage(message, type) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `temp-message ${type}`;
    messageDiv.textContent = message;
    messageDiv.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 12px 20px;
        border-radius: 6px;
        color: white;
        font-weight: 500;
        z-index: 1000;
        animation: slideIn 0.3s ease-out;
        background: ${getMessageColor(type)};
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;

    document.body.appendChild(messageDiv);

    setTimeout(() => {
        messageDiv.remove();
    }, 3000);
}

function getMessageColor(type) {
    switch (type) {
        case 'success': return '#28a745';
        case 'error': return '#dc3545';
        case 'warning': return '#ffc107';
        default: return '#17a2b8';
    }
}

// Profile Picture Functions
function initializeProfilePicture() {
    const uploadFileBtn = document.getElementById('uploadFileBtn');
    const urlLinkBtn = document.getElementById('urlLinkBtn');
    const fileUploadSection = document.getElementById('fileUploadSection');
    const urlInputSection = document.getElementById('urlInputSection');
    const fileDropZone = document.getElementById('fileDropZone');
    const profileImageInput = document.getElementById('profileImage');
    const profilePictureUrl = document.getElementById('profilePictureUrl');
    const imagePreview = document.getElementById('imagePreview');

    if (!uploadFileBtn || !urlLinkBtn) {
        console.warn('Profile picture buttons not found');
        return;
    }

    // Method selector functionality
    uploadFileBtn.addEventListener('click', function () {
        uploadFileBtn.classList.add('active');
        urlLinkBtn.classList.remove('active');
        if (fileUploadSection) fileUploadSection.classList.remove('hidden');
        if (urlInputSection) urlInputSection.classList.add('hidden');
    });

    urlLinkBtn.addEventListener('click', function () {
        urlLinkBtn.classList.add('active');
        uploadFileBtn.classList.remove('active');
        if (urlInputSection) urlInputSection.classList.remove('hidden');
        if (fileUploadSection) fileUploadSection.classList.add('hidden');
    });

    if (fileDropZone && profileImageInput) {
        // File drop zone functionality
        fileDropZone.addEventListener('click', function () {
            profileImageInput.click();
        });

        fileDropZone.addEventListener('dragover', function (e) {
            e.preventDefault();
            fileDropZone.classList.add('dragover');
        });

        fileDropZone.addEventListener('dragleave', function (e) {
            e.preventDefault();
            fileDropZone.classList.remove('dragover');
        });

        fileDropZone.addEventListener('drop', function (e) {
            e.preventDefault();
            fileDropZone.classList.remove('dragover');

            const files = e.dataTransfer.files;
            if (files.length > 0) {
                profileImageInput.files = files;
                handleFileSelect(files[0]);
            }
        });

        // File input change
        profileImageInput.addEventListener('change', function (e) {
            if (e.target.files.length > 0) {
                handleFileSelect(e.target.files[0]);
            }
        });
    }

    // URL input change
    if (profilePictureUrl) {
        profilePictureUrl.addEventListener('input', function (e) {
            const url = e.target.value.trim();
            if (url) {
                showImagePreview(url);
            } else {
                showDefaultPreview();
            }
        });
    }

    function handleFileSelect(file) {
        if (file && file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = function (e) {
                showImagePreview(e.target.result);
            };
            reader.readAsDataURL(file);
        } else {
            showTemporaryMessage('Please select a valid image file.', 'error');
            showDefaultPreview();
        }
    }

    function showImagePreview(src) {
        const imagePreview = document.getElementById('imagePreview');
        if (imagePreview) {
            imagePreview.innerHTML = '<img src="' + src + '" alt="Profile Preview" class="preview-image" style="width: 100%; height: 100%; object-fit: cover; border-radius: 8px;" />';
        }
    }

    function showDefaultPreview() {
        const imagePreview = document.getElementById('imagePreview');
        if (imagePreview) {
            imagePreview.innerHTML = '<div class="default-avatar"><i class="fas fa-user" style="font-size: 3rem; color: var(--text-secondary);"></i></div>';
        }
    }
}

// Image error handling function for profile pictures
function handleImageError(userId, fallbackUrl, userInitial) {
    const img = document.getElementById('img-' + userId);
    if (!img) return;

    if (fallbackUrl && fallbackUrl.trim() !== '' && !fallbackUrl.startsWith('~/Uploads/')) {
        // Try fallback URL
        img.onerror = function () {
            // If fallback URL also fails, show user initial
            img.style.display = 'none';
            const avatarDiv = img.parentNode;
            if (avatarDiv) {
                avatarDiv.innerHTML = '<div style="width: 100%; height: 100%; border-radius: 50%; background: #007bff; color: white; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 1.2rem;">' + userInitial + '</div>';
            }
        };
        img.src = fallbackUrl;
    } else {
        // No fallback URL, show user initial
        img.style.display = 'none';
        const avatarDiv = img.parentNode;
        if (avatarDiv) {
            avatarDiv.innerHTML = '<div style="width: 100%; height: 100%; border-radius: 50%; background: #007bff; color: white; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 1.2rem;">' + userInitial + '</div>';
        }
    }
}

// Add CSS animation for messages
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);