// AI MODOO - Main JavaScript

// Close dropdowns when clicking outside
document.addEventListener('click', function (e) {
    const dropdowns = ['langMenu', 'userMenu'];
    dropdowns.forEach(id => {
        const menu = document.getElementById(id);
        if (menu && !menu.parentElement.contains(e.target)) {
            menu.classList.add('hidden');
        }
    });
});

// Auto-hide flash messages after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    const flashMessages = document.querySelectorAll('[class*="bg-green-50"], [class*="bg-red-50"]');
    flashMessages.forEach(msg => {
        if (msg.closest('main')) {
            setTimeout(() => {
                msg.style.transition = 'opacity 0.5s';
                msg.style.opacity = '0';
                setTimeout(() => msg.remove(), 500);
            }, 5000);
        }
    });
});

// Toast notification helper
window.showToast = function (message, type = 'success') {
    const colors = {
        success: 'bg-green-500',
        error: 'bg-red-500',
        warning: 'bg-yellow-500',
        info: 'bg-blue-500'
    };

    const icons = {
        success: 'fa-check-circle',
        error: 'fa-exclamation-circle',
        warning: 'fa-exclamation-triangle',
        info: 'fa-info-circle'
    };

    const toast = document.createElement('div');
    toast.className = `toast ${colors[type]} text-white px-6 py-3 rounded-lg shadow-lg flex items-center gap-3`;
    toast.innerHTML = `<i class="fas ${icons[type]}"></i><span>${message}</span>`;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.transition = 'opacity 0.5s';
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 500);
    }, 4000);
};

// Confirm delete helper
window.confirmDelete = function (formId, itemName) {
    const isRtl = document.documentElement.dir === 'rtl';
    const msg = isRtl
        ? `هل أنت متأكد من حذف "${itemName}"؟`
        : `Are you sure you want to delete "${itemName}"?`;

    if (confirm(msg)) {
        document.getElementById(formId).submit();
    }
};

// Format number with commas
window.formatNumber = function (num, decimals = 2) {
    return new Intl.NumberFormat(document.documentElement.lang === 'ar' ? 'ar-SA' : 'en-US', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    }).format(num);
};

// Dynamic form lines (for journal entries, invoices, etc.)
window.addFormLine = function (containerId, template) {
    const container = document.getElementById(containerId);
    const index = container.children.length;
    const html = template.replace(/__INDEX__/g, index);
    container.insertAdjacentHTML('beforeend', html);
};

window.removeFormLine = function (button) {
    button.closest('.form-line').remove();
    // Re-index remaining lines
    const container = button.closest('.form-lines-container');
    if (container) {
        const lines = container.querySelectorAll('.form-line');
        lines.forEach((line, idx) => {
            line.querySelectorAll('[name]').forEach(input => {
                input.name = input.name.replace(/\[\d+\]/, `[${idx}]`);
            });
        });
    }
};
