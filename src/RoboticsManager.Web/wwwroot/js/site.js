// SignalR connection for real-time updates
let updateConnection = null;

// Initialize SignalR connection
function initializeSignalR() {
    updateConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/updates")
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry delays in milliseconds
        .build();

    updateConnection.onreconnecting(error => {
        console.warn('SignalR Reconnecting:', error);
        showToast('warning', 'Reconnecting to server...');
    });

    updateConnection.onreconnected(connectionId => {
        console.log('SignalR Reconnected:', connectionId);
        showToast('success', 'Reconnected to server');
        // Refresh data after reconnection
        if (typeof refreshData === 'function') {
            refreshData();
        }
    });

    updateConnection.onclose(error => {
        console.error('SignalR Connection closed:', error);
        showToast('error', 'Connection lost. Please refresh the page.');
    });

    // Start the connection
    startSignalRConnection();
}

// Start SignalR connection with retry logic
async function startSignalRConnection() {
    try {
        await updateConnection.start();
        console.log('SignalR Connected');
    } catch (err) {
        console.error('SignalR Connection Error:', err);
        setTimeout(startSignalRConnection, 5000);
    }
}

// Toast notifications
function showToast(type, message) {
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    const container = document.getElementById('toast-container') || createToastContainer();
    container.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast, {
        animation: true,
        autohide: true,
        delay: 5000
    });

    bsToast.show();

    // Remove toast after it's hidden
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
}

// Create toast container if it doesn't exist
function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
    container.style.zIndex = '1050';
    document.body.appendChild(container);
    return container;
}

// Loading spinner
function showLoading(targetElement) {
    const spinner = document.createElement('div');
    spinner.className = 'loading-spinner';
    spinner.setAttribute('role', 'status');
    spinner.innerHTML = '<span class="visually-hidden">Loading...</span>';

    if (typeof targetElement === 'string') {
        targetElement = document.querySelector(targetElement);
    }

    if (targetElement) {
        targetElement.appendChild(spinner);
    }
    return spinner;
}

function hideLoading(spinner) {
    if (spinner && spinner.parentElement) {
        spinner.remove();
    }
}

// Form helpers
function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this item?');
}

// Initialize Bootstrap tooltips and popovers
function initializeBootstrapComponents() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Handle active navigation links
function setActiveNavLink() {
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-link').forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });
}

// Document ready
document.addEventListener('DOMContentLoaded', function() {
    initializeBootstrapComponents();
    setActiveNavLink();

    // Initialize SignalR if the script is included
    if (typeof signalR !== 'undefined') {
        initializeSignalR();
    }
});

// Prevent double form submission
document.addEventListener('submit', function(e) {
    const form = e.target;
    if (form.classList.contains('submitted')) {
        e.preventDefault();
        return;
    }
    form.classList.add('submitted');

    // Re-enable form after 5 seconds in case submission fails
    setTimeout(() => {
        form.classList.remove('submitted');
    }, 5000);
});

// Handle AJAX errors
window.addEventListener('unhandledrejection', function(event) {
    console.error('Unhandled promise rejection:', event.reason);
    showToast('error', 'An error occurred. Please try again.');
});
