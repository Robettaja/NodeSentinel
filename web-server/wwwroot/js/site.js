function handleFormSubmit(form) {
    const btn = form.querySelector('button');
    const icon = btn.querySelector('.btn-icon');
    const spinner = btn.querySelector('.btn-spinner');
    const label = btn.querySelector('.btn-label');
    const originalLabel = label.textContent;

    btn.disabled = true;
    if (icon) {
        icon.classList.add('hidden');
        
    }
    spinner.classList.remove('hidden');
    const text = label.textContent.trim();
    const verbMap = {
        "Stop": "Stopping...",
        "Start": "Starting...",
        "Restart": "Restarting...",
        "Delete": "Deleting...",
        "Create": "Creating...",
        "Restore": "Restoring...",
    };

    label.textContent = verbMap[text] || (text + "...");
}
