function showModerateModal(novelId, novelTitle) {
    document.getElementById('moderateNovelTitle').textContent = novelTitle;
    document.getElementById('moderateForm').action = '/Admin/Novel_Manager/ModerateNovel/' + novelId;
    document.getElementById('moderateModal').style.display = 'block';
    document.body.classList.add('modal-open');
}

function closeModerateModal() {
    document.getElementById('moderateModal').style.display = 'none';
    document.body.classList.remove('modal-open');
    document.getElementById('moderateActionError').style.display = 'none';
}

function submitModerateForm() {
    const form = document.getElementById('moderateForm');
    const actionSelect = document.getElementById('moderationAction');
    const errorMessage = document.getElementById('moderateActionError');
    if (actionSelect.value === "") {
        errorMessage.style.display = 'block';
        return false;
    }
    errorMessage.style.display = 'none';
    form.submit();
}

function showDeactivateModal(novelId, novelTitle, isActive) {
    const action = isActive ? 'deactivate' : 'activate';
    document.getElementById('deactivateAction').textContent = action;
    document.getElementById('deactivateNovelTitle').textContent = novelTitle;

    const form = document.getElementById('deactivateForm');
    const searchParams = new URLSearchParams(window.location.search);

    let dynamicInputs = '';
    dynamicInputs += '<input type="hidden" name="search" value="' + (searchParams.get('search') || '') + '">';
    dynamicInputs += '<input type="hidden" name="statusFilter" value="' + (searchParams.get('statusFilter') || 'all') + '">';
    dynamicInputs += '<input type="hidden" name="moderationFilter" value="' + (searchParams.get('moderationFilter') || 'all') + '">';
    dynamicInputs += '<input type="hidden" name="activeFilter" value="' + (searchParams.get('activeFilter') || 'all') + '">';
    dynamicInputs += '<input type="hidden" name="page" value="' + (searchParams.get('page') || '1') + '">';

    form.action = '/Admin/Novel_Manager/ToggleStatusForm/' + novelId;

    Array.from(form.querySelectorAll('[name="search"], [name="statusFilter"], [name="moderationFilter"], [name="activeFilter"], [name="page"]')).forEach(el => el.remove());

    form.insertAdjacentHTML('beforeend', dynamicInputs);

    document.getElementById('deactivateModal').style.display = 'block';
    document.body.classList.add('modal-open');
}

function closeDeactivateModal() {
    document.getElementById('deactivateModal').style.display = 'none';
    document.body.classList.remove('modal-open');
}

function submitDeactivateForm() {
    document.getElementById('deactivateForm').submit();
}

document.addEventListener('click', function (e) {
    if (e.target.classList.contains('novels-modal-overlay')) {
        closeModerateModal();
        closeDeactivateModal();
    }
});

// Close modal with Escape key
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeModerateModal();
        closeDeactivateModal();
    }
});