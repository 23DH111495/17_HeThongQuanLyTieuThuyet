document.addEventListener('DOMContentLoaded', function () {
    setupCharacterCounters();
    setupDropdownHandlers();
    setupNestedReplyHandlers();
    initializeVoteTracking();
    initializeEditForms();
    initializeExistingComments();
});

const votedComments = new Map();
const MAX_NESTING_LEVEL = 5;  


document.addEventListener('DOMContentLoaded', function () {
    // Remove the problematic styling that centers text
    const commentTexts = document.querySelectorAll('.comment-text');
    commentTexts.forEach(text => {
        text.style.margin = '0';
        text.style.padding = '0';
        text.style.textAlign = 'left';
        text.style.display = 'block';
        text.style.width = '100%';
    });

    const commentContents = document.querySelectorAll('.comment-content');
    commentContents.forEach(content => {
        content.style.marginLeft = '0px';
        content.style.padding = '0';
        content.style.textAlign = 'left';
        content.style.display = 'block';
        content.style.width = '100%';
    });
});
document.addEventListener('DOMContentLoaded', function () {
    const nestedReplies = document.querySelectorAll('.comment-item.reply');
    nestedReplies.forEach(reply => {
        const commentId = reply.getAttribute('data-comment-id');
        const commentStats = reply.querySelector('.comment-stats');
        const replyButton = commentStats?.querySelector('button[onclick*="showReplyForm"]');
        const isLoggedIn = document.querySelector('input[name="__RequestVerificationToken"]') !== null;
        if (commentStats && !replyButton && isLoggedIn) { // Only add if logged in
            const replyDepth = parseInt(reply.getAttribute('data-reply-depth') || '1');
            if (replyDepth < MAX_NESTING_LEVEL) {
                const replyBtn = document.createElement('button');
                replyBtn.type = 'button';
                replyBtn.className = 'comment-stat';
                replyBtn.onclick = () => showReplyForm(commentId);
                replyBtn.innerHTML = '<i class="fas fa-reply"></i><span>Reply</span>';
                commentStats.appendChild(replyBtn);
            }
        }
        const hasEditMenu = reply.querySelector('.comment-dropdown button[onclick*="showEditForm"]');
        if (hasEditMenu && !document.getElementById(`edit-form-${commentId}`)) {
            const editFormHTML = `
                <div class="comment-edit-form" id="edit-form-${commentId}" style="display: none;">
                    <form onsubmit="return handleEditSubmit(event, ${commentId})">
                        <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''}" />
                        <textarea name="content" 
                                 id="edit-content-${commentId}" 
                                 maxlength="2000" 
                                 required 
                                 class="nested-edit-textarea">${reply.querySelector('.comment-content').textContent}</textarea>
                        <div class="character-count">
                            <span id="edit-char-count-${commentId}">0</span>/2000 characters
                        </div>
                        <div class="comment-edit-actions">
                            <button type="submit" class="comment-btn">Save</button>
                            <button type="button" class="comment-btn-secondary" onclick="hideEditForm(${commentId})">Cancel</button>
                        </div>
                    </form>
                </div>
            `;

            // Insert edit form after comment content
            const commentContent = reply.querySelector('.comment-content');
            if (commentContent) {
                commentContent.insertAdjacentHTML('afterend', editFormHTML);
            }
        }
    });
});
document.addEventListener('DOMContentLoaded', function () {
    setupCharacterCounters();
    setupDropdownHandlers();
    setupNestedReplyHandlers();
    initializeVoteTracking();
    initializeEditForms();
    initializeExistingComments();

    document.querySelectorAll('.replies-container').forEach(container => {
        const parentId = container.id.replace('replies-', '');
        updateReplyCount(parentId);
    });
});
function toggleReplies(commentId) {
    const repliesContainer = document.getElementById(`replies-${commentId}`);
    const replyIcon = document.getElementById(`reply-icon-${commentId}`);
    const replyText = document.getElementById(`reply-text-${commentId}`);

    if (repliesContainer.style.display === 'none') {
        repliesContainer.style.display = 'block';
        replyIcon.className = 'fas fa-chevron-up';
        replyText.textContent = 'Hide replies';
    } else {
        repliesContainer.style.display = 'none';
        replyIcon.className = 'fas fa-chevron-down';
        const replyCount = repliesContainer.querySelectorAll('.comment-item.reply').length;
        replyText.textContent = `Show ${replyCount} replies`;
    }
}
function updateReplyCount(parentCommentId) {
    const repliesContainer = document.getElementById(`replies-${parentCommentId}`);
    const replyText = document.getElementById(`reply-text-${parentCommentId}`);
    const showRepliesBtn = document.querySelector(`[onclick="toggleReplies(${parentCommentId})"]`);

    if (repliesContainer && replyText) {
        const replyCount = repliesContainer.querySelectorAll('.comment-item.reply').length;

        if (replyCount === 0) {
            if (showRepliesBtn) showRepliesBtn.style.display = 'none';
        } else {
            if (showRepliesBtn) showRepliesBtn.style.display = 'block';
            if (repliesContainer.style.display === 'none') {
                replyText.textContent = `Show ${replyCount} replies`;
            }
        }
    }
}
function initializeExistingComments(){
    document.addEventListener('click', function (e) {
        const commentId = e.target.getAttribute('data-comment-id') || e.target.closest('[data-comment-id]')?.getAttribute('data-comment-id');
        if (!commentId) return;

        const action = e.target.getAttribute('data-action');
        if (!action) return;

        switch (action) {
            case 'edit':
                showEditForm(parseInt(commentId));
                break;
            case 'delete':
                deleteComment(parseInt(commentId));
                break;
            case 'like':
                likeComment(parseInt(commentId), true);
                break;
            case 'dislike':
                likeComment(parseInt(commentId), false);
                break;
            case 'reply':
                showReplyForm(parseInt(commentId));
                break;
            case 'deep-reply':
                showDeepReplyOption(parseInt(commentId));
                break;
        }
    });

    document.querySelectorAll('.comment-menu-btn[data-comment-id]').forEach(btn => {
        if (!btn.hasAttribute('data-initialized')) {
            btn.setAttribute('data-initialized', 'true');
            btn.addEventListener('click', function () {
                const commentId = this.getAttribute('data-comment-id');
                toggleMenu(parseInt(commentId));
            });
        }
    });
}
function initializeEditForms() {
    const editTextareas = document.querySelectorAll('textarea[id^="edit-content-"]');
    editTextareas.forEach(textarea => {
        const commentId = textarea.id.replace('edit-content-', '');
        const counter = document.getElementById(`edit-char-count-${commentId}`);
        if (counter && !textarea.hasAttribute('data-initialized')) {
            textarea.setAttribute('data-initialized', 'true');
            textarea.addEventListener('input', function () {
                counter.textContent = this.value.length;
                const counterContainer = counter.parentElement;
                if (counterContainer) {
                    counterContainer.classList.toggle('warning', this.value.length > 1800);
                }
            });
        }
    });
}
function getCurrentVoteStateBackup(commentId) {
    // First try the votedComments map
    const memoryVote = votedComments.get(commentId);
    if (memoryVote) {
        console.log(`[DEBUG] Found vote in memory: ${memoryVote}`);
        return memoryVote;
    }

    // Fallback to DOM inspection
    return getCurrentVoteStateFromUI(commentId);
}
function initializeVoteTracking() {
    votedComments.clear();

    const commentElements = document.querySelectorAll('[data-comment-id]');

    commentElements.forEach(element => {
        const commentId = parseInt(element.getAttribute('data-comment-id'));

        const likeButton = element.querySelector('.comment-stat.user-voted.user-liked');
        const dislikeButton = element.querySelector('.comment-stat.user-voted.user-disliked');

        console.log(`[DEBUG] Tracking init for comment ${commentId} - Like button: ${!!likeButton}, Dislike button: ${!!dislikeButton}`);

        if (likeButton) {
            votedComments.set(commentId, 'like');
            console.log(`[DEBUG] Set memory vote: comment ${commentId} = like`);
        } else if (dislikeButton) {
            votedComments.set(commentId, 'dislike');
            console.log(`[DEBUG] Set memory vote: comment ${commentId} = dislike`);
        }
    });

    console.log('[DEBUG] Vote tracking initialized. Memory state:', Array.from(votedComments.entries()));
}
async function handleCommentSubmit(event) {
    event.preventDefault();
    const form = event.target;
    const formData = new FormData(form);
    try {
        const response = await fetch('/Book/AddComment', {
            method: 'POST',
            body: formData,
            credentials: 'same-origin'
        });
        const result = await response.json();
        if (result.success) {
            form.reset();
            updateCharacterCount(form.querySelector('textarea[name="content"]'));
            const previewElement = document.getElementById('comment-preview');
            if (previewElement) {
                previewElement.innerHTML = '';
                previewElement.style.display = 'none';
            }
            const imageInput = document.getElementById('comment-image');
            if (imageInput) {
                imageInput.value = '';
            }
            const parentCommentId = form.querySelector('input[name="parentCommentId"]')?.value;
            if (parentCommentId) {
                addNestedReplyToDOM(result.comment, parseInt(parentCommentId));
                hideReplyForm(parentCommentId);
            } else {
                addCommentToDOM(result.comment, 'comments-list', false);
                updateCommentsCount(1);
            }
            showNotification(result.message, 'success');
        } else {
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('Error submitting comment:', error);
        showNotification('Failed to post comment', 'error');
    }
    return false;
}
async function handleReplySubmit(event, parentCommentId) {
    event.preventDefault();

    const form = event.target;
    const formData = new FormData(form);

    try {
        const response = await fetch('/Book/AddComment', {
            method: 'POST',
            body: formData,
            credentials: 'same-origin'
        });

        const result = await response.json();

        if (result.success) {
            form.reset();
            hideReplyForm(parentCommentId);
            addNestedReplyToDOM(result.comment, parentCommentId);
            showNotification(result.message, 'success');
        } else {
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('Error submitting reply:', error);
        showNotification('Failed to post reply', 'error');
    }

    return false;
}
function setupNestedReplyHandlers() {
    // Handle dynamic reply forms
    document.addEventListener('click', function (e) {
        if (e.target.matches('[data-reply-btn]')) {
            const commentId = e.target.getAttribute('data-comment-id');
            showReplyForm(commentId);
        }

        if (e.target.matches('[data-cancel-reply]')) {
            const commentId = e.target.getAttribute('data-comment-id');
            hideReplyForm(commentId);
        }
    });
}
async function handleEditSubmit(event, commentId) {
    event.preventDefault();

    const form = event.target;
    const formData = new FormData(form);
    formData.append('commentId', commentId);

    try {
        const response = await fetch('/Book/EditComment', {
            method: 'POST',
            body: formData,
            credentials: 'same-origin'
        });

        const result = await response.json();

        if (result.success) {
            const contentDiv = document.getElementById(`content-${commentId}`);
            if (contentDiv) {
                let imageHtml = '';
                if (result.hasImage) {
                    imageHtml = `<div class="comment-image">
                        <img src="/Book/GetCommentImage/${commentId}?t=${Date.now()}" alt="Comment image" onclick="showImageModal(this.src)" />
                    </div>`;
                }

                contentDiv.innerHTML = `
                    <p class="comment-message">${result.content}</p>
                    ${imageHtml}
                `;
            }

            const existingImageSection = document.getElementById(`existing-image-${commentId}`);
            if (existingImageSection) {
                if (result.hasImage) {
                    const img = existingImageSection.querySelector('img');
                    if (img) {
                        img.src = `/Book/GetCommentImage/${commentId}?t=${Date.now()}`;
                    }
                    existingImageSection.style.display = 'block';
                    document.getElementById(`remove-image-${commentId}`).value = 'false';
                } else {
                    existingImageSection.remove();
                }
            } else if (result.hasImage) {
                const editForm = document.getElementById(`edit-form-${commentId}`);
                const textarea = editForm.querySelector('textarea');
                const imageSection = `
                    <div class="existing-image-section" id="existing-image-${commentId}">
                        <div class="existing-image-preview">
                            <img src="/Book/GetCommentImage/${commentId}?t=${Date.now()}" alt="Current image" />
                            <button type="button" class="remove-existing-image" onclick="removeExistingImage(${commentId})">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <input type="hidden" name="removeImage" id="remove-image-${commentId}" value="false" />
                    </div>
                `;
                textarea.insertAdjacentHTML('afterend', imageSection);
            }

            const editPreview = document.getElementById(`edit-preview-${commentId}`);
            if (editPreview) {
                editPreview.innerHTML = '';
                editPreview.style.display = 'none';
            }

            const editImageInput = document.getElementById(`edit-image-${commentId}`);
            if (editImageInput) {
                editImageInput.value = '';
            }

            hideEditForm(commentId);
            showNotification(result.message, 'success');
        } else {
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('Error editing comment:', error);
        showNotification('Failed to edit comment', 'error');
    }

    return false;
}
async function likeComment(commentId, isLike) {
    console.log(`[DEBUG] likeComment called - CommentId: ${commentId}, IsLike: ${isLike}`);

    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    const likeBtn = commentElement?.querySelector(`#like-count-${commentId}`).closest('button');
    const dislikeBtn = commentElement?.querySelector(`#dislike-count-${commentId}`).closest('button');

    if (!likeBtn || !dislikeBtn) {
        console.error('Vote buttons not found');
        return;
    }

    likeBtn.style.pointerEvents = 'none';
    dislikeBtn.style.pointerEvents = 'none';

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!token) {
        showNotification('Please refresh the page and try again', 'error');
        likeBtn.style.pointerEvents = 'auto';
        dislikeBtn.style.pointerEvents = 'auto';
        return;
    }

    const formData = new FormData();
    formData.append('__RequestVerificationToken', token);
    formData.append('commentId', commentId);
    formData.append('isLike', isLike);

    try {
        const response = await fetch('/Book/LikeComment', {
            method: 'POST',
            body: formData,
            credentials: 'same-origin'
        });

        const result = await response.json();
        console.log(`[DEBUG] Backend response:`, result);

        if (result.success) {
            updateVoteUI(commentId, result.likeCount, result.dislikeCount, result.userVote);

            if (result.userVote) {
                votedComments.set(commentId, result.userVote);
            } else {
                votedComments.delete(commentId);
            }
        } else {
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('Error processing vote:', error);
        showNotification('Failed to process vote', 'error');
    } finally {
        likeBtn.style.pointerEvents = 'auto';
        dislikeBtn.style.pointerEvents = 'auto';
    }
}
function getCurrentVoteStateFromUI(commentId) {
    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    if (!commentElement) {
        console.log(`[DEBUG] Comment element not found for ID: ${commentId}`);
        return null;
    }

    const commentStats = commentElement.querySelector('.comment-stats');
    if (!commentStats) {
        console.log(`[DEBUG] Comment stats section not found for ID: ${commentId}`);
        return null;
    }

    const likeButton = commentStats.querySelector('.comment-stat.user-voted.user-liked');
    const dislikeButton = commentStats.querySelector('.comment-stat.user-voted.user-disliked');

    if (likeButton) {
        console.log(`[DEBUG] Found LIKED state for comment ${commentId}`);
        return 'like';
    }
    if (dislikeButton) {
        console.log(`[DEBUG] Found DISLIKED state for comment ${commentId}`);
        return 'dislike';
    }

    console.log(`[DEBUG] No vote state found for comment ${commentId}`);
    return null;
}
function updateVoteUI(commentId, likeCount, dislikeCount, userVote) {
    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    if (!commentElement) return;

    console.log(`[DEBUG] UpdateVoteUI - CommentId: ${commentId}, Likes: ${likeCount}, Dislikes: ${dislikeCount}, UserVote: ${userVote}`);

    const likeElement = document.getElementById(`like-count-${commentId}`);
    const dislikeElement = document.getElementById(`dislike-count-${commentId}`);
    const likeBtn = commentElement.querySelector('.comment-stat[onclick*="likeComment"][onclick*="true"]');
    const dislikeBtn = commentElement.querySelector('.comment-stat[onclick*="likeComment"][onclick*="false"]');

    if (likeElement) {
        likeElement.textContent = likeCount;
    }
    if (dislikeElement) {
        dislikeElement.textContent = dislikeCount;
    }

    if (likeBtn && dislikeBtn) {
        likeBtn.classList.remove('user-voted', 'user-liked');
        dislikeBtn.classList.remove('user-voted', 'user-disliked');

        likeBtn.removeAttribute('disabled');
        dislikeBtn.removeAttribute('disabled');

        if (userVote === 'like') {
            likeBtn.classList.add('user-voted', 'user-liked');
            votedComments.set(commentId, 'like');
            console.log(`[DEBUG] UI - Applied like state and updated memory`);
        } else if (userVote === 'dislike') {
            dislikeBtn.classList.add('user-voted', 'user-disliked');
            votedComments.set(commentId, 'dislike');
            console.log(`[DEBUG] UI - Applied dislike state and updated memory`);
        } else {
            votedComments.delete(commentId);
            console.log(`[DEBUG] UI - Cleared all vote states and memory`);
        }
    }
}
async function deleteComment(commentId) {
    if (!confirm('Are you sure you want to delete this comment? This action cannot be undone.')) {
        return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!token) {
        showNotification('Please refresh the page and try again', 'error');
        return;
    }

    const formData = new FormData();
    formData.append('__RequestVerificationToken', token);
    formData.append('commentId', commentId);

    try {
        const response = await fetch('/Book/DeleteComment', {
            method: 'POST',
            body: formData,
            credentials: 'same-origin'
        });

        const result = await response.json();

        if (result.success) {
            const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
            if (commentElement) {
                commentElement.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                commentElement.style.opacity = '0';
                commentElement.style.transform = 'translateY(-20px)';

                setTimeout(() => {
                    commentElement.remove();
                    updateCommentsCount(-1);
                }, 300);
            }

            showNotification(result.message, 'success');
        } else {
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('Error deleting comment:', error);
        showNotification('Failed to delete comment', 'error');
    }
}
function setupCharacterCounters() {
    // Setup for existing textareas and handle dynamically added ones
    document.addEventListener('input', function (e) {
        if (e.target.matches('textarea[name="content"]')) {
            updateCharacterCount(e.target);
        }
    });

    // Initialize existing textareas
    const textareas = document.querySelectorAll('textarea[name="content"]');
    textareas.forEach(textarea => {
        updateCharacterCount(textarea);
    });
}
function updateCharacterCount(textarea) {
    const count = textarea.value.length;
    const maxLength = 2000;

    // Find the character counter for this specific textarea
    const form = textarea.closest('form, .comment-form, .comment-edit-form, .reply-form');
    const counter = form?.querySelector('[id$="char-count"]');

    if (counter) {
        counter.textContent = count;
        const counterContainer = counter.parentElement;
        if (counterContainer) {
            counterContainer.classList.toggle('warning', count > 1800);
        }
    }
}
function setupDropdownHandlers() {
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.comment-menu')) {
            document.querySelectorAll('.comment-dropdown').forEach(dropdown => {
                dropdown.style.display = 'none';
            });
        }
    });
}
function toggleMenu(commentId) {
    const dropdown = document.getElementById(`menu-${commentId}`);
    if (dropdown) {
        document.querySelectorAll('.comment-dropdown').forEach(d => {
            if (d !== dropdown) d.style.display = 'none';
        });

        dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';
    }
}
function showEditForm(commentId) {
    const contentDiv = document.getElementById(`content-${commentId}`);
    const editForm = document.getElementById(`edit-form-${commentId}`);
    const dropdown = document.getElementById(`menu-${commentId}`);

    if (contentDiv && editForm) {
        contentDiv.style.display = 'none';
        editForm.style.display = 'block';

        if (dropdown) {
            dropdown.style.display = 'none';
        }

        const textarea = document.getElementById(`edit-content-${commentId}`);
        if (textarea) {
            textarea.focus();
            textarea.setSelectionRange(textarea.value.length, textarea.value.length);
            // Update character count for edit form
            updateCharacterCount(textarea);
        }
    }
}
function hideEditForm(commentId) {
    const contentDiv = document.getElementById(`content-${commentId}`);
    const editForm = document.getElementById(`edit-form-${commentId}`);

    if (contentDiv && editForm) {
        contentDiv.style.display = 'block';
        editForm.style.display = 'none';
    }
}
function showReplyForm(commentId) {
    if (!document.querySelector('input[name="__RequestVerificationToken"]')) {
        return;
    }

    document.querySelectorAll('.reply-form').forEach(form => {
        if (form.id !== `reply-form-${commentId}`) {
            form.remove();
        }
    });

    let replyForm = document.getElementById(`reply-form-${commentId}`);

    if (!replyForm) {
        createReplyForm(commentId);
        replyForm = document.getElementById(`reply-form-${commentId}`);
    }

    const dropdown = document.getElementById(`menu-${commentId}`);
    if (dropdown) {
        dropdown.style.display = 'none';
    }

    if (replyForm) {
        replyForm.style.display = 'block';
        const textarea = document.getElementById(`reply-content-${commentId}`);
        if (textarea) {
            textarea.focus();
        }
    }
}
function createReplyForm(parentCommentId) {
    const parentElement = document.querySelector(`[data-comment-id="${parentCommentId}"]`);
    if (!parentElement) return;

    const commentStats = parentElement.querySelector('.comment-stats');
    if (!commentStats) return;

    const replyFormHTML = `
        <div class="reply-form" id="reply-form-${parentCommentId}" style="display: none;">
            <form onsubmit="return handleReplySubmit(event, ${parentCommentId})" enctype="multipart/form-data">
                <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''}" />
                <input type="hidden" name="novelId" value="${typeof currentNovelId !== 'undefined' ? currentNovelId : ''}" />
                <input type="hidden" name="parentCommentId" value="${parentCommentId}" />
                
                <textarea name="content" 
                         id="reply-content-${parentCommentId}" 
                         placeholder="Add a reply..." 
                         maxlength="2000" 
                         required
                         class="reply-textarea"></textarea>
                         
                <div class="image-upload-section">
                    <input type="file" 
                           name="commentImage" 
                           id="reply-image-${parentCommentId}"
                           accept="image/jpeg,image/jpg,image/png,image/gif,image/webp" 
                           onchange="handleImagePreview(this, 'reply-preview-${parentCommentId}')" />
                    <label for="reply-image-${parentCommentId}" class="image-upload-label">
                        <i class="fas fa-image"></i> Add Image
                    </label>
                    <div id="reply-preview-${parentCommentId}" class="image-preview"></div>
                </div>
                         
                <div class="character-count">
                    <span id="reply-char-count-${parentCommentId}">0</span>/2000
                </div>
                
                <div class="reply-actions">
                    <button type="button" 
                            class="reply-btn secondary" 
                            onclick="hideReplyForm(${parentCommentId})">Cancel</button>
                    <button type="submit" class="reply-btn primary">Reply</button>
                </div>
            </form>
        </div>
    `;

    commentStats.insertAdjacentHTML('afterend', replyFormHTML);

    const textarea = document.getElementById(`reply-content-${parentCommentId}`);
    if (textarea) {
        textarea.addEventListener('input', function () {
            updateCharacterCount(this);
        });
    }
}
function handleImagePreview(input, previewId) {
    const preview = document.getElementById(previewId);
    const file = input.files[0];

    if (file) {
        if (file.size > 2 * 1024 * 1024) {
            showNotification('Image size cannot exceed 2MB', 'error');
            input.value = '';
            return;
        }

        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        if (!allowedTypes.includes(file.type.toLowerCase())) {
            showNotification('Only JPEG, PNG, GIF, and WebP images are allowed', 'error');
            input.value = '';
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            preview.innerHTML = `
                <div class="image-preview-container">
                    <img src="${e.target.result}" alt="Preview" />
                    <button type="button" class="remove-image" onclick="removeImagePreview('${input.id}', '${previewId}')">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `;
            preview.style.display = 'block';
        };
        reader.readAsDataURL(file);
    } else {
        preview.innerHTML = '';
        preview.style.display = 'none';
    }
}
function removeImagePreview(inputId, previewId) {
    document.getElementById(inputId).value = '';
    const preview = document.getElementById(previewId);
    preview.innerHTML = '';
    preview.style.display = 'none';
}
function findOrCreateRepliesContainer(parentElement) {
    let repliesContainer = parentElement.querySelector(':scope > .replies-container');

    if (!repliesContainer) {
        repliesContainer = document.createElement('div');
        repliesContainer.className = 'replies-container';
        repliesContainer.style.cssText = `
            margin-top: 15px;
            margin-left: 20px;
            border-left: 2px solid #e9ecef;
            padding-left: 15px;
            position: relative;
        `;
        parentElement.appendChild(repliesContainer);
    }

    return repliesContainer;
}
function addNestedReplyToDOM(reply, parentCommentId) {
    const parentComment = document.querySelector(`[data-comment-id="${parentCommentId}"]`);
    if (!parentComment) return;

    let repliesContainer = document.getElementById(`replies-${parentCommentId}`);
    let showRepliesBtn = document.querySelector(`[onclick="toggleReplies(${parentCommentId})"]`);

    if (!repliesContainer) {
        const repliesSection = document.createElement('div');
        repliesSection.className = 'replies-section';
        repliesSection.innerHTML = `
            <button type="button" class="show-replies-btn" onclick="toggleReplies(${parentCommentId})">
                <i class="fas fa-reply" id="reply-icon-${parentCommentId}"></i>
                <span id="reply-text-${parentCommentId}">Show 1 reply</span>
            </button>
            <div class="replies-container" id="replies-${parentCommentId}" style="display: none;">
            </div>
        `;

        const commentStats = parentComment.querySelector('.comment-stats');
        commentStats.parentNode.insertBefore(repliesSection, commentStats.nextSibling);

        repliesContainer = document.getElementById(`replies-${parentCommentId}`);
        showRepliesBtn = document.querySelector(`[onclick="toggleReplies(${parentCommentId})"]`);
    }

    const replyHTML = generateNestedReplyHTML(reply);
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = replyHTML;
    const newReplyElement = tempDiv.firstElementChild;

    repliesContainer.appendChild(newReplyElement);

    if (reply.UserVoteType) {
        votedComments.set(reply.Id, reply.UserVoteType);
    }

    const currentCount = repliesContainer.querySelectorAll('.comment-item.reply').length;
    const replyText = document.getElementById(`reply-text-${parentCommentId}`);
    if (replyText && repliesContainer.style.display === 'none') {
        replyText.textContent = `Show ${currentCount} ${currentCount === 1 ? 'reply' : 'replies'}`;
    }

    if (newReplyElement) {
        newReplyElement.style.opacity = '0';
        newReplyElement.style.transform = 'translateY(-10px)';
        requestAnimationFrame(() => {
            newReplyElement.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
            newReplyElement.style.opacity = '1';
            newReplyElement.style.transform = 'translateY(0)';
        });
    }
}
function updateReplyCount(parentCommentId) {
    const repliesContainer = document.getElementById(`replies-${parentCommentId}`);
    const replyText = document.getElementById(`reply-text-${parentCommentId}`);
    const showRepliesBtn = document.querySelector(`[onclick="toggleReplies(${parentCommentId})"]`);

    if (repliesContainer && replyText) {
        const replyCount = repliesContainer.querySelectorAll('.comment-item.reply').length;

        if (replyCount === 0) {
            if (showRepliesBtn) showRepliesBtn.style.display = 'none';
        } else {
            if (showRepliesBtn) showRepliesBtn.style.display = 'block';
            if (repliesContainer.style.display === 'none') {
                replyText.textContent = `Show ${replyCount} ${replyCount === 1 ? 'reply' : 'replies'}`;
            }
        }
    }
}

function removeExistingImage(commentId) {
    const existingImageSection = document.getElementById(`existing-image-${commentId}`);
    const removeImageInput = document.getElementById(`remove-image-${commentId}`);

    if (existingImageSection && removeImageInput) {
        existingImageSection.style.display = 'none';
        removeImageInput.value = 'true';

        const label = document.querySelector(`label[for="edit-image-${commentId}"]`);
        if (label) {
            label.innerHTML = '<i class="fas fa-image"></i> Add Image';
        }
    }
}
function generateNestedReplyHTML(reply) {
    const escapedContent = (reply.Content || '').trim();
    const isLoggedIn = document.querySelector('input[name="__RequestVerificationToken"]') !== null;
    return `
        <div class="comment-item reply" 
             data-comment-id="${reply.Id}"
             data-reply-depth="1"
             data-parent-id="${reply.ParentCommentId || ''}">
            <div class="comment-header">
                <div class="comment-user">
                    <div class="comment-avatar small">
                        <img src="/Book/GetUserAvatar/${reply.UserId || ''}"
                             alt="${reply.CommenterName || ''}"
                             onerror="this.style.display='none'; this.nextElementSibling.style.display='inline-flex';" />
                        <span style="display:none;">${reply.CommenterInitials || ''}</span>
                    </div>
                    <div class="comment-user-info">
                        <h5>${reply.CommenterName || ''}</h5>
                        <div class="comment-time">${reply.TimeAgo || formatTimeAgo(reply.CreatedDate)}</div>
                    </div>
                </div>
                ${isLoggedIn ? `
                <div class="comment-menu">
                    <button class="comment-menu-btn" data-comment-id="${reply.Id}" data-initialized="true">
                        <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <div class="comment-dropdown" id="menu-${reply.Id}" style="display: none;">
                        ${reply.CanEdit ? `
                            <button type="button" data-action="edit" data-comment-id="${reply.Id}">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button type="button" data-action="delete" data-comment-id="${reply.Id}">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        ` : ''}
                        <button type="button" data-action="like" data-comment-id="${reply.Id}">
                            <i class="fas fa-thumbs-up"></i> Like
                        </button>
                        <button type="button" data-action="dislike" data-comment-id="${reply.Id}">
                            <i class="fas fa-thumbs-down"></i> Dislike
                        </button>
                        <button type="button" data-action="reply" data-comment-id="${reply.Id}">
                            <i class="fas fa-reply"></i> Reply
                        </button>
                    </div>
                </div>
                ` : ''}
            </div>
            <div class="comment-body" id="content-${reply.Id}">
                <p class="comment-message">${escapedContent}</p>
                ${reply.HasImage ? `<div class="comment-image"><img src="/Book/GetCommentImage/${reply.Id}" alt="Comment image" onclick="showImageModal(this.src)" /></div>` : ''}
            </div>
            ${reply.CanEdit ? `
                <div class="comment-edit-form" id="edit-form-${reply.Id}" style="display: none;">
                    <form onsubmit="return handleEditSubmit(event, ${reply.Id})" enctype="multipart/form-data">
                        <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''}" />
                        <textarea name="content" 
                                 id="edit-content-${reply.Id}" 
                                 maxlength="2000" 
                                 required 
                                 class="nested-edit-textarea">${escapedContent.replace(/"/g, '&quot;')}</textarea>
                        ${reply.HasImage ? `
                            <div class="existing-image-section" id="existing-image-${reply.Id}">
                                <div class="existing-image-preview">
                                    <img src="/Book/GetCommentImage/${reply.Id}" alt="Current image" />
                                    <button type="button" class="remove-existing-image" onclick="removeExistingImage(${reply.Id})">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                                <input type="hidden" name="removeImage" id="remove-image-${reply.Id}" value="false" />
                            </div>
                        ` : ''}
                        <div class="image-upload-section">
                            <input type="file"
                                   name="commentImage"
                                   id="edit-image-${reply.Id}"
                                   accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                                   onchange="handleImagePreview(this, 'edit-preview-${reply.Id}')" />
                            <label for="edit-image-${reply.Id}" class="image-upload-label">
                                <i class="fas fa-image"></i> ${reply.HasImage ? 'Change Image' : 'Add Image'}
                            </label>
                            <div id="edit-preview-${reply.Id}" class="image-preview"></div>
                        </div>
                        <div class="character-count">
                            <span id="edit-char-count-${reply.Id}">${escapedContent.length}</span>/2000
                        </div>
                        <div class="comment-edit-actions">
                            <button type="submit" class="comment-btn">Save</button>
                            <button type="button" class="comment-btn-secondary" onclick="hideEditForm(${reply.Id})">Cancel</button>
                        </div>
                    </form>
                </div>
            ` : ''}
            <div class="comment-stats">
                ${isLoggedIn ? `
                <button type="button" class="comment-stat ${reply.UserVoteType === 'like' ? 'user-voted user-liked' : ''}" data-action="like" data-comment-id="${reply.Id}">
                    <i class="fas fa-thumbs-up"></i>
                    <span id="like-count-${reply.Id}">${reply.LikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat ${reply.UserVoteType === 'dislike' ? 'user-voted user-disliked' : ''}" data-action="dislike" data-comment-id="${reply.Id}">
                    <i class="fas fa-thumbs-down"></i>
                    <span id="dislike-count-${reply.Id}">${reply.DislikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat" data-action="reply" data-comment-id="${reply.Id}">
                    <i class="fas fa-reply"></i>
                    <span>Reply</span>
                </button>
                ` : `
                <button type="button" class="comment-stat" disabled>
                    <i class="fas fa-thumbs-up"></i>
                    <span id="like-count-${reply.Id}">${reply.LikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat" disabled>
                    <i class="fas fa-thumbs-down"></i>
                    <span id="dislike-count-${reply.Id}">${reply.DislikeCount || 0}</span>
                </button>
                `}
            </div>
        </div>
    `;
}
function generateCommentHTML(comment) {
    const escapedContent = comment.Content || '';
    const isLoggedIn = document.querySelector('input[name="__RequestVerificationToken"]') !== null;
    return `
        <div class="comment-item" data-comment-id="${comment.Id}" data-reply-depth="0">
            <div class="comment-header">
                <div class="comment-user">
                    <div class="comment-avatar">
                        <img src="/Book/GetUserAvatar/${comment.UserId || ''}"
                             alt="${comment.CommenterName || ''}"
                             onerror="this.style.display='none'; this.nextElementSibling.style.display='inline-flex';" />
                        <span style="display:none;">${comment.CommenterInitials || ''}</span>
                    </div>
                    <div class="comment-user-info">
                        <h5>${comment.CommenterName || ''}</h5>
                        <div class="comment-time">${comment.TimeAgo || 'Just now'}</div>
                    </div>
                </div>
                ${isLoggedIn ? `
                <div class="comment-menu">
                    <button class="comment-menu-btn" data-comment-id="${comment.Id}" data-initialized="true">
                        <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <div class="comment-dropdown" id="menu-${comment.Id}" style="display: none;">
                        ${comment.CanEdit ? `
                            <button type="button" data-action="edit" data-comment-id="${comment.Id}">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button type="button" data-action="delete" data-comment-id="${comment.Id}">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        ` : ''}
                        <button type="button" data-action="like" data-comment-id="${comment.Id}">
                            <i class="fas fa-thumbs-up"></i> Like
                        </button>
                        <button type="button" data-action="dislike" data-comment-id="${comment.Id}">
                            <i class="fas fa-thumbs-down"></i> Dislike
                        </button>
                        <button type="button" data-action="reply" data-comment-id="${comment.Id}">
                            <i class="fas fa-reply"></i> Reply
                        </button>
                    </div>
                </div>
                ` : ''}
            </div>
            <div class="comment-body" id="content-${comment.Id}">
                <p class="comment-message">${escapedContent}</p>
                ${comment.HasImage ? `<div class="comment-image"><img src="/Book/GetCommentImage/${comment.Id}" alt="Comment image" onclick="showImageModal(this.src)" /></div>` : ''}
            </div>
            ${comment.CanEdit ? `
                <div class="comment-edit-form" id="edit-form-${comment.Id}" style="display: none;">
                    <form onsubmit="return handleEditSubmit(event, ${comment.Id})" enctype="multipart/form-data">
                        <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''}" />
                        <textarea name="content" 
                                 id="edit-content-${comment.Id}" 
                                 maxlength="2000" 
                                 required 
                                 class="nested-edit-textarea">${escapedContent.replace(/"/g, '&quot;')}</textarea>
                        ${comment.HasImage ? `
                            <div class="existing-image-section" id="existing-image-${comment.Id}">
                                <div class="existing-image-preview">
                                    <img src="/Book/GetCommentImage/${comment.Id}" alt="Current image" />
                                    <button type="button" class="remove-existing-image" onclick="removeExistingImage(${comment.Id})">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                                <input type="hidden" name="removeImage" id="remove-image-${comment.Id}" value="false" />
                            </div>
                        ` : ''}
                        <div class="image-upload-section">
                            <input type="file"
                                   name="commentImage"
                                   id="edit-image-${comment.Id}"
                                   accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                                   onchange="handleImagePreview(this, 'edit-preview-${comment.Id}')" />
                            <label for="edit-image-${comment.Id}" class="image-upload-label">
                                <i class="fas fa-image"></i> ${comment.HasImage ? 'Change Image' : 'Add Image'}
                            </label>
                            <div id="edit-preview-${comment.Id}" class="image-preview"></div>
                        </div>
                        <div class="character-count">
                            <span id="edit-char-count-${comment.Id}">${escapedContent.length}</span>/2000
                        </div>
                        <div class="comment-edit-actions">
                            <button type="submit" class="comment-btn">Save</button>
                            <button type="button" class="comment-btn-secondary" onclick="hideEditForm(${comment.Id})">Cancel</button>
                        </div>
                    </form>
                </div>
            ` : ''}
            <div class="comment-stats">
                ${isLoggedIn ? `
                <button type="button" class="comment-stat ${comment.UserVoteType === 'like' ? 'user-voted user-liked' : ''}" data-action="like" data-comment-id="${comment.Id}">
                    <i class="fas fa-thumbs-up"></i>
                    <span id="like-count-${comment.Id}">${comment.LikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat ${comment.UserVoteType === 'dislike' ? 'user-voted user-disliked' : ''}" data-action="dislike" data-comment-id="${comment.Id}">
                    <i class="fas fa-thumbs-down"></i>
                    <span id="dislike-count-${comment.Id}">${comment.DislikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat" data-action="reply" data-comment-id="${comment.Id}">
                    <i class="fas fa-reply"></i>
                    <span>Reply</span>
                </button>
                ` : `
                <button type="button" class="comment-stat" disabled>
                    <i class="fas fa-thumbs-up"></i>
                    <span id="like-count-${comment.Id}">${comment.LikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat" disabled>
                    <i class="fas fa-thumbs-down"></i>
                    <span id="dislike-count-${comment.Id}">${comment.DislikeCount || 0}</span>
                </button>
                `}
            </div>
        </div>
    `;
}
function findTopLevelParentId(commentId) {
    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    if (!commentElement) return commentId;

    const topLevelComment = commentElement.closest('[data-reply-depth="0"]');
    return topLevelComment ? topLevelComment.getAttribute('data-comment-id') : commentId;
}
function generateNestedRepliesHTML(replies, nestingLevel = 1) {
    let html = '';
    replies.forEach(reply => {
        html += generateNestedReplyHTML(reply, 1);
        if (reply.Replies && reply.Replies.length > 0) {
            html += generateNestedRepliesHTML(reply.Replies, 1);
        }
    });
    return html;
}
function showDeepReplyOption(commentId) {
    showNotification('Maximum nesting reached. Your reply will start a new thread.', 'info');
    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    const rootParentId = findRootParentId(commentElement);

    // Show reply form at the root level with context
    showReplyForm(rootParentId);

    // Add context to the reply form
    const replyForm = document.getElementById(`reply-form-${rootParentId}`);
    if (replyForm) {
        const contextDiv = document.createElement('div');
        contextDiv.className = 'reply-context-notice';
        contextDiv.innerHTML = `
            <i class="fas fa-info-circle"></i>
            Continuing conversation from deeply nested reply
        `;
        replyForm.insertBefore(contextDiv, replyForm.firstChild);
    }
}
function getCommenterName(commentId) {
    const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
    const nameElement = commentElement?.querySelector('.comment-user h5');
    return nameElement?.textContent || 'someone';
}
function findRootParentId(commentElement) {
    let current = commentElement;
    let rootId = current.getAttribute('data-comment-id');

    while (current && current.getAttribute('data-parent-id')) {
        const parentId = current.getAttribute('data-parent-id');
        const parentElement = document.querySelector(`[data-comment-id="${parentId}"]`);

        if (parentElement) {
            rootId = parentId;
            current = parentElement;
        } else {
            break;
        }
    }

    return rootId;
}
function formatTimeAgo(dateString) {
    if (!dateString) return 'Just now';

    const now = new Date();
    const date = new Date(dateString);
    const diffInSeconds = Math.floor((now - date) / 1000);

    if (diffInSeconds < 60) return 'Just now';
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)} minutes ago`;
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)} hours ago`;
    if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)} days ago`;

    return date.toLocaleDateString();
}
function hideReplyForm(commentId) {
    const replyForm = document.getElementById(`reply-form-${commentId}`);
    if (replyForm) {
        replyForm.remove();
    }
}
async function loadMoreComments(novelId, skip = 0, take = 10, type = 'comments') {
    try {
        const response = await fetch(`/Book/LoadMoreComments?novelId=${novelId}&skip=${skip}&take=${take}&type=${type}`, {
            credentials: 'same-origin'
        });

        const result = await response.json();

        if (result.success) {
            const listElement = document.getElementById(type === 'reviews' ? 'reviews-list' : 'comments-list');

            if (type === 'reviews') {
                result.reviews.forEach(review => {
                    const reviewHTML = generateReviewHTML(review);
                    listElement.insertAdjacentHTML('beforeend', reviewHTML);
                });
            } else {
                result.comments.forEach(comment => {
                    const commentHTML = generateCommentWithRepliesHTML(comment);
                    listElement.insertAdjacentHTML('beforeend', commentHTML);
                });
            }

            // Hide load more button if no more content
            if (!result.hasMore) {
                const loadMoreBtn = document.getElementById(`load-more-${type}`);
                if (loadMoreBtn) {
                    loadMoreBtn.style.display = 'none';
                }
            }
        }
    } catch (error) {
        console.error('Error loading more comments:', error);
        showNotification('Failed to load more comments', 'error');
    }
}
function generateCommentWithRepliesHTML(comment) {
    let html = generateCommentHTML(comment);

    if (comment.Replies && comment.Replies.length > 0) {
        const repliesContainer = `<div class="replies-container" style="margin-left: 20px; border-left: 2px solid #e9ecef; padding-left: 15px;">`;
        html = html.replace('</div>', repliesContainer + generateNestedRepliesHTML(comment.Replies) + '</div></div>');
    }

    return html;
}
function generateNestedReplyHTML(reply) {
    const escapedContent = (reply.Content || '').trim();
    const isLoggedIn = document.querySelector('input[name="__RequestVerificationToken"]') !== null;
    return `
        <div class="comment-item reply" 
             data-comment-id="${reply.Id}"
             data-reply-depth="1"
             data-parent-id="${reply.ParentCommentId || ''}">
            <div class="comment-header">
                <div class="comment-user">
                    <div class="comment-avatar small">
                        <img src="/Book/GetUserAvatar/${reply.UserId || ''}"
                             alt="${reply.CommenterName || ''}"
                             onerror="this.style.display='none'; this.nextElementSibling.style.display='inline-flex';" />
                        <span style="display:none;">${reply.CommenterInitials || ''}</span>
                    </div>
                    <div class="comment-user-info">
                        <h5>${reply.CommenterName || ''}</h5>
                        <div class="comment-time">${reply.TimeAgo || formatTimeAgo(reply.CreatedDate)}</div>
                    </div>
                </div>
                ${isLoggedIn ? `
                <div class="comment-menu">
                    <button class="comment-menu-btn" data-comment-id="${reply.Id}" data-initialized="true">
                        <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <div class="comment-dropdown" id="menu-${reply.Id}" style="display: none;">
                        ${reply.CanEdit ? `
                            <button type="button" data-action="edit" data-comment-id="${reply.Id}">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button type="button" data-action="delete" data-comment-id="${reply.Id}">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        ` : ''}
                        <button type="button" data-action="like" data-comment-id="${reply.Id}">
                            <i class="fas fa-thumbs-up"></i> Like
                        </button>
                        <button type="button" data-action="dislike" data-comment-id="${reply.Id}">
                            <i class="fas fa-thumbs-down"></i> Dislike
                        </button>
                        <button type="button" data-action="reply" data-comment-id="${reply.Id}">
                            <i class="fas fa-reply"></i> Reply
                        </button>
                    </div>
                </div>
                ` : ''}
            </div>
            <div class="comment-body" id="content-${reply.Id}">
                <p class="comment-message">${escapedContent}</p>
                ${reply.HasImage ? `<div class="comment-image"><img src="/Book/GetCommentImage/${reply.Id}" alt="Comment image" onclick="showImageModal(this.src)" /></div>` : ''}
            </div>
            ${reply.CanEdit ? `
                <div class="comment-edit-form" id="edit-form-${reply.Id}" style="display: none;">
                    <form onsubmit="return handleEditSubmit(event, ${reply.Id})" enctype="multipart/form-data">
                        <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''}" />
                        <textarea name="content" 
                                 id="edit-content-${reply.Id}" 
                                 maxlength="2000" 
                                 required 
                                 class="nested-edit-textarea">${escapedContent.replace(/"/g, '&quot;')}</textarea>
                        ${reply.HasImage ? `
                            <div class="existing-image-section" id="existing-image-${reply.Id}">
                                <div class="existing-image-preview">
                                    <img src="/Book/GetCommentImage/${reply.Id}" alt="Current image" />
                                    <button type="button" class="remove-existing-image" onclick="removeExistingImage(${reply.Id})">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                                <input type="hidden" name="removeImage" id="remove-image-${reply.Id}" value="false" />
                            </div>
                        ` : ''}
                        <div class="image-upload-section">
                            <input type="file"
                                   name="commentImage"
                                   id="edit-image-${reply.Id}"
                                   accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                                   onchange="handleImagePreview(this, 'edit-preview-${reply.Id}')" />
                            <label for="edit-image-${reply.Id}" class="image-upload-label">
                                <i class="fas fa-image"></i> ${reply.HasImage ? 'Change Image' : 'Add Image'}
                            </label>
                            <div id="edit-preview-${reply.Id}" class="image-preview"></div>
                        </div>
                        <div class="character-count">
                            <span id="edit-char-count-${reply.Id}">${escapedContent.length}</span>/2000
                        </div>
                        <div class="comment-edit-actions">
                            <button type="submit" class="comment-btn">Save</button>
                            <button type="button" class="comment-btn-secondary" onclick="hideEditForm(${reply.Id})">Cancel</button>
                        </div>
                    </form>
                </div>
            ` : ''}
            <div class="comment-stats">
                ${isLoggedIn ? `
                <button type="button" class="comment-stat ${reply.UserVoteType === 'like' ? 'user-voted user-liked' : ''}" data-action="like" data-comment-id="${reply.Id}">
                    <i class="fas fa-thumbs-up"></i>
                    <span id="like-count-${reply.Id}">${reply.LikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat ${reply.UserVoteType === 'dislike' ? 'user-voted user-disliked' : ''}" data-action="dislike" data-comment-id="${reply.Id}">
                    <i class="fas fa-thumbs-down"></i>
                    <span id="dislike-count-${reply.Id}">${reply.DislikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat" data-action="reply" data-comment-id="${reply.Id}">
                    <i class="fas fa-reply"></i>
                    <span>Reply</span>
                </button>
                ` : `
                <button type="button" class="comment-stat" disabled>
                    <i class="fas fa-thumbs-up"></i>
                    <span id="like-count-${reply.Id}">${reply.LikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat" disabled>
                    <i class="fas fa-thumbs-down"></i>
                    <span id="dislike-count-${reply.Id}">${reply.DislikeCount || 0}</span>
                </button>
                `}
            </div>
        </div>
    `;
}
function addCommentToDOM(comment, listId, append = false) {
    const listElement = document.getElementById(listId);
    if (!listElement) return;

    const commentHTML = generateCommentHTML(comment);

    if (append) {
        listElement.insertAdjacentHTML('beforeend', commentHTML);
    } else {
        listElement.insertAdjacentHTML('afterbegin', commentHTML);
    }

    if (comment.UserVoteType) {
        votedComments.set(comment.Id, comment.UserVoteType);
    }

    if (!append) {
        const newCommentElement = listElement.querySelector(`[data-comment-id="${comment.Id}"]`);
        if (newCommentElement) {
            newCommentElement.classList.remove('reply');
            newCommentElement.setAttribute('data-reply-depth', '0');
            newCommentElement.style.opacity = '0';
            newCommentElement.style.transform = 'translateY(-20px)';
            requestAnimationFrame(() => {
                newCommentElement.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                newCommentElement.style.opacity = '1';
                newCommentElement.style.transform = 'translateY(0)';
            });
        }
    }
}
function generateCommentHTML(comment) {
    const escapedContent = comment.Content || '';

    return `
        <div class="comment-item" data-comment-id="${comment.Id}" data-reply-depth="0">
            <div class="comment-header">
                <div class="comment-user">
                    <div class="comment-avatar">${comment.CommenterInitials || ''}</div>
                    <div class="comment-user-info">
                        <h5>${comment.CommenterName || ''}</h5>
                        <div class="comment-time">${comment.TimeAgo || 'Just now'}</div>
                    </div>
                </div>
                <div class="comment-menu">
                    <button class="comment-menu-btn" data-comment-id="${comment.Id}" data-initialized="true">
                        <i class="fas fa-ellipsis-v"></i>
                    </button>
                    <div class="comment-dropdown" id="menu-${comment.Id}" style="display: none;">
                        ${comment.CanEdit ? `
                            <button type="button" data-action="edit" data-comment-id="${comment.Id}">
                                <i class="fas fa-edit"></i> Edit
                            </button>
                            <button type="button" data-action="delete" data-comment-id="${comment.Id}">
                                <i class="fas fa-trash"></i> Delete
                            </button>
                        ` : ''}
                        <button type="button" data-action="like" data-comment-id="${comment.Id}">
                            <i class="fas fa-thumbs-up"></i> Like
                        </button>
                        <button type="button" data-action="dislike" data-comment-id="${comment.Id}">
                            <i class="fas fa-thumbs-down"></i> Dislike
                        </button>
                        <button type="button" data-action="reply" data-comment-id="${comment.Id}">
                            <i class="fas fa-reply"></i> Reply
                        </button>
                    </div>
                </div>
            </div>
            
            <div class="comment-content" id="content-${comment.Id}">
                <div class="comment-text" style="text-align: left; margin: 0; padding: 0;">
                    ${escapedContent}
                </div>
                ${comment.HasImage ? `<div class="comment-image"><img src="/Comments/GetCommentImage/${comment.Id}" alt="Comment image" onclick="showImageModal(this.src)" /></div>` : ''}
            </div>
            
            ${comment.CanEdit ? `
                <div class="comment-edit-form" id="edit-form-${comment.Id}" style="display: none;">
                    <form onsubmit="return handleEditSubmit(event, ${comment.Id})">
                        <input type="hidden" name="__RequestVerificationToken" value="${document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''}" />
                        <textarea name="content" 
                                 id="edit-content-${comment.Id}" 
                                 maxlength="2000" 
                                 required 
                                 class="nested-edit-textarea">${escapedContent.replace(/"/g, '&quot;')}</textarea>
                        <div class="character-count">
                            <span id="edit-char-count-${comment.Id}">${escapedContent.length}</span>/2000
                        </div>
                        <div class="comment-edit-actions">
                            <button type="submit" class="comment-btn">Save</button>
                            <button type="button" class="comment-btn-secondary" onclick="hideEditForm(${comment.Id})">Cancel</button>
                        </div>
                    </form>
                </div>
            ` : ''}
            
            <div class="comment-stats">
                <button type="button" class="comment-stat ${comment.UserVoteType === 'like' ? 'user-voted user-liked' : ''}" data-action="like" data-comment-id="${comment.Id}">
                    <i class="fas fa-thumbs-up"></i>
                    <span id="like-count-${comment.Id}">${comment.LikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat ${comment.UserVoteType === 'dislike' ? 'user-voted user-disliked' : ''}" data-action="dislike" data-comment-id="${comment.Id}">
                    <i class="fas fa-thumbs-down"></i>
                    <span id="dislike-count-${comment.Id}">${comment.DislikeCount || 0}</span>
                </button>
                <button type="button" class="comment-stat" data-action="reply" data-comment-id="${comment.Id}">
                    <i class="fas fa-reply"></i>
                    <span>Reply</span>
                </button>
            </div>
        </div>
    `;
}
function showImageModal(imageSrc) {
    const modal = document.createElement('div');
    modal.className = 'image-modal';
    modal.innerHTML = `
        <div class="image-modal-content">
            <span class="image-modal-close">&times;</span>
            <img src="${imageSrc}" alt="Full size image" />
        </div>
    `;

    modal.style.cssText = `
        display: block;
        position: fixed;
        z-index: 10000;
        left: 0;
        top: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0,0,0,0.8);
        animation: fadeIn 0.3s ease;
    `;

    const content = modal.querySelector('.image-modal-content');
    content.style.cssText = `
        margin: 5% auto;
        padding: 20px;
        max-width: 90%;
        max-height: 90%;
        position: relative;
        text-align: center;
    `;

    const img = modal.querySelector('img');
    img.style.cssText = `
        max-width: 100%;
        max-height: 80vh;
        object-fit: contain;
    `;

    const closeBtn = modal.querySelector('.image-modal-close');
    closeBtn.style.cssText = `
        position: absolute;
        top: 10px;
        right: 25px;
        color: white;
        font-size: 35px;
        font-weight: bold;
        cursor: pointer;
        z-index: 10001;
    `;

    closeBtn.onclick = () => modal.remove();
    modal.onclick = (e) => {
        if (e.target === modal) modal.remove();
    };

    document.body.appendChild(modal);
}
function updateCommentsCount(delta) {
    const countElement = document.getElementById('comments-count');
    if (countElement) {
        const currentCount = parseInt(countElement.textContent) || 0;
        countElement.textContent = Math.max(0, currentCount + delta);
    }
}
function showNotification(message, type = 'success') {
    document.querySelectorAll('.notification').forEach(n => n.remove());

    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.textContent = message;

    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 20px;
        border-radius: 5px;
        color: white;
        font-weight: bold;
        z-index: 9999;
        max-width: 400px;
        word-wrap: break-word;
        transition: all 0.3s ease;
        transform: translateX(100%);
        ${type === 'error' ? 'background-color: #e74c3c;' :
            type === 'info' ? 'background-color: #3498db;' :
                'background-color: #2ecc71;'}
    `;

    document.body.appendChild(notification);

    requestAnimationFrame(() => {
        notification.style.transform = 'translateX(0)';
    });

    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => notification.remove(), 300);
    }, 5000);

    notification.addEventListener('click', () => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => notification.remove(), 300);
    });
}
function showTab(tabName) {
    document.querySelectorAll('.book-tab').forEach(tab => {
        tab.classList.remove('book-active');
    });
    document.querySelectorAll('.book-tab-content').forEach(content => {
        content.classList.remove('book-active');
    });

    event.target.classList.add('book-active');

    const tabContent = document.getElementById(`${tabName}-tab`);
    if (tabContent) {
        tabContent.classList.add('book-active');
    }

    window.location.hash = tabName;
}
function showTabByName(tabName) {
    document.querySelectorAll('.book-tab').forEach(tab => {
        tab.classList.remove('book-active');
    });
    document.querySelectorAll('.book-tab-content').forEach(content => {
        content.classList.remove('book-active');
    });

    const targetTab = document.querySelector(`.book-tab[onclick*="${tabName}"]`);
    if (targetTab) {
        targetTab.classList.add('book-active');
    }

    const tabContent = document.getElementById(`${tabName}-tab`);
    if (tabContent) {
        tabContent.classList.add('book-active');
    }
}

