// js/components/ui.js - TakeNote General UI Utilities (Toasts, Modals, Escaping)

// Modals
export function openModal(modalId) {
    document.getElementById('modalBackdrop').classList.remove('hidden');
    document.getElementById(modalId).classList.remove('hidden');
}

export function closeAllModals() {
    document.getElementById('modalBackdrop').classList.add('hidden');
    
    // Hide all modal cards
    const modals = document.querySelectorAll('.modal-card');
    modals.forEach(m => m.classList.add('hidden'));
}

// Toast Notifications
export function showToast(title, message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;
    
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    
    let iconClass = 'fa-circle-info';
    if (type === 'success') iconClass = 'fa-circle-check';
    else if (type === 'warning') iconClass = 'fa-circle-exclamation';
    else if (type === 'error') iconClass = 'fa-triangle-exclamation';

    toast.innerHTML = `
        <div class="toast-icon"><i class="fa-solid ${iconClass}"></i></div>
        <div class="toast-body">
            <div class="toast-title">${escapeHtml(title)}</div>
            <div class="toast-message">${escapeHtml(message)}</div>
        </div>
        <button class="btn-close-toast"><i class="fa-solid fa-xmark"></i></button>
    `;

    toast.querySelector('.btn-close-toast').onclick = () => toast.remove();

    container.appendChild(toast);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        toast.classList.add('removing');
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}

// Helper to escape HTML characters (security)
export function escapeHtml(unsafe) {
    if (!unsafe) return '';
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/"/g, "&quot;")
         .replace(/'/g, "&#039;");
}
