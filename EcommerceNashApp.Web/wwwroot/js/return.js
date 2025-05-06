async function initialize() {
    const queryString = window.location.search;
    const urlParams = new URLSearchParams(queryString);
    const sessionId = urlParams.get('session_id');
    if (!sessionId) {
        window.location.href = '/Checkout';
        return;
    }

    const response = await fetch(`/session-status?session_id=${sessionId}`, {
        headers: {
            'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        }
    });
    const session = await response.json();

    if (session.status === 'open') {
        window.location.href = '/Checkout';
    } else if (session.status === 'complete') {
        document.getElementById('success').classList.remove('hidden');
        document.getElementById('customer-email').textContent = session.customer_email || 'bạn';
    }
}

document.addEventListener('DOMContentLoaded', initialize);