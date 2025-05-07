let stripe, elements, addressElement, paymentElement;

async function fetchClientSecret() {
    try {
        const form = document.getElementById('create-payment-intent-form');
        if (!form) {
            console.error('CreatePaymentIntent form not found');
            document.getElementById('payment-message').textContent = 'Không thể tìm thấy biểu mẫu thanh toán. Vui lòng làm mới trang.';
            document.getElementById('submit').disabled = false;
            setLoading(false);
            return null;
        }

        const formData = new FormData(form);
        const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            console.log('Using RequestVerificationToken from form');
        } else {
            console.warn('No RequestVerificationToken found in form');
        }

        const response = await fetch(form.action, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            console.error('Error fetching client secret:', response.status, errorData.message || 'Unknown error');
            document.getElementById('payment-message').textContent = errorData.message || 'Không thể khởi tạo thanh toán. Vui lòng thử lại.';
            document.getElementById('submit').disabled = false;
            setLoading(false);
            return null;
        }

        const data = await response.json();
        if (!data.clientSecret) {
            console.error('No client secret in response:', data);
            document.getElementById('payment-message').textContent = 'Phản hồi không hợp lệ từ máy chủ. Vui lòng thử lại.';
            document.getElementById('submit').disabled = false;
            setLoading(false);
            return null;
        }
        return data.clientSecret;
    } catch (error) {
        console.error('Fetch error:', error);
        document.getElementById('payment-message').textContent = 'Lỗi kết nối máy chủ. Vui lòng kiểm tra kết nối và thử lại.';
        document.getElementById('submit').disabled = false;
        setLoading(false);
        return null;
    }
}

async function initialize() {
    try {
        const response = await fetch('/Checkout/Config');
        const { publishableKey } = await response.json();

        stripe = Stripe(publishableKey);
        console.log('Stripe initialized with publishableKey');

        if (document.getElementById('address-element')) {
            elements = stripe.elements();
            addressElement = elements.create('address', {
                mode: 'shipping',
                allowedCountries: ['VN'],
                fields: {
                    phone: 'always',
                    name: 'always',
                    address: {
                        line1: 'always',
                        line2: 'never',
                        city: 'always',
                        state: 'never',
                        postal_code: 'always',
                        country: 'always'
                    }
                }
            });
            addressElement.mount('#address-element');
            console.log('Address Element mounted with fields: phone, name, line1, city, postal_code, country required');
            addressElement.on('change', (event) => {
                console.log('Address Element state:', event);
            });
        }

        if (document.getElementById('payment-element')) {
            const clientSecret = await fetchClientSecret();
            if (!clientSecret) {
                console.error('Failed to initialize payment: No client secret');
                return;
            }

            try {
                elements = stripe.elements({ clientSecret });
                console.log('Stripe elements initialized with clientSecret');
            } catch (error) {
                console.error('Error initializing Stripe elements:', error);
                document.getElementById('payment-message').textContent = 'Không thể khởi tạo thanh toán. Vui lòng làm mới trang và thử lại.';
                document.getElementById('submit').disabled = false;
                setLoading(false);
                return;
            }

            try {
                paymentElement = elements.create('payment', {
                    layout: 'tabs'
                });
                paymentElement.on('ready', () => console.log('Payment Element ready'));
                paymentElement.on('loaderror', (event) => console.error('Payment Element load error:', event.error));
                paymentElement.mount('#payment-element');
                console.log('Payment Element mounted');
            } catch (error) {
                console.error('Error mounting Payment Element:', error);
                document.getElementById('payment-message').textContent = 'Không thể hiển thị phương thức thanh toán. Vui lòng làm mới trang và thử lại.';
                document.getElementById('submit').disabled = false;
                setLoading(false);
                return;
            }
        }
    } catch (error) {
        console.error('Error in initialize:', error);
        document.getElementById('payment-message').textContent = 'Lỗi khởi tạo thanh toán. Vui lòng làm mới trang và thử lại.';
        setLoading(false);
    }
}

async function handleSubmit(e) {
    let shouldPreventDefault = true;

    setLoading(true);

    try {
        if (document.getElementById('address-element')) {
            if (!elements || !addressElement) {
                console.error('Stripe elements or addressElement not initialized');
                showMessage('Không thể tải thông tin địa chỉ. Vui lòng làm mới trang và thử lại.', 'address-message');
                setLoading(false);
                return;
            }

            const addressData = await elements.getElement('address').getValue();
            console.log('Address data from getValue:', addressData);

            const shippingAddress = {
                'ShippingAddress.FullName': addressData.value.name,
                'ShippingAddress.Address1': addressData.value.address.line1,
                'ShippingAddress.Address2': addressData.value.address.line2 || '',
                'ShippingAddress.City': addressData.value.address.city,
                'ShippingAddress.State': addressData.value.address.state || '',
                'ShippingAddress.Zip': addressData.value.address.postal_code,
                'ShippingAddress.Country': addressData.value.address.country,
                'ShippingAddress.SaveAddress': document.getElementById('save-address').checked.toString()
            };

            const formData = new FormData();
            for (const [key, value] of Object.entries(shippingAddress)) {
                formData.append(key, value);
            }
            formData.append('CurrentStep', document.getElementById('current-step').value);
            formData.append('SessionId', document.querySelector('input[name="SessionId"]').value);
            formData.append('__RequestVerificationToken', document.querySelector('input[name="__RequestVerificationToken"]').value);

            const response = await fetch('/Checkout/NextStep', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: formData
            });

            if (response.ok) {
                console.log('Redirecting to step 2 after address submission');
                window.location.href = '/Checkout?step=2';
            } else {
                const errorData = await response.json().catch(() => ({}));
                console.error('NextStep failed:', errorData);
                showMessage(errorData.message || 'Không thể lưu địa chỉ. Vui lòng thử lại.', 'address-message');
                setLoading(false);
            }
        } else if (document.getElementById('payment-element')) {
            console.log(`${window.location.origin}/Checkout/Return`);
            const { error } = await stripe.confirmPayment({
                elements,
                confirmParams: {
                    return_url: `${window.location.origin}/Checkout/Return`,
                    receipt_email: "test@test.com"
                }
            });

            if (error) {
                console.error('Payment confirmation failed:', error);
                showMessage(error.message || 'Thanh toán không thành công. Vui lòng thử lại.', 'payment-message');
                setLoading(false);
            } else {
                console.log('Payment confirmation initiated, redirecting to return_url');
            }
        } else {
            shouldPreventDefault = false;
            console.log('No Stripe elements present, allowing native form submission');
        }
    } catch (error) {
        console.error('Error in handleSubmit:', error);
        showMessage('Đã xảy ra lỗi. Vui lòng thử lại.', 'payment-message');
        setLoading(false);
    }

    if (shouldPreventDefault) {
        e.preventDefault();
    }
}

function showMessage(messageText, containerId = 'payment-message') {
    const messageContainer = document.getElementById(containerId);
    messageContainer.textContent = messageText;
}

function setLoading(isLoading) {
    const submitButton = document.getElementById('submit');
    const spinner = document.getElementById('spinner');
    const buttonText = document.getElementById('button-text');

    if (submitButton) {
        submitButton.disabled = isLoading;
    }

    if (spinner) {
        spinner.classList.toggle('hidden', !isLoading);
    }

    if (buttonText) {
        buttonText.classList.toggle('hidden', isLoading);
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    await initialize();

    // Attach the submit event listener to the payment form if it exists
    const paymentForm = document.getElementById('payment-form');
    if (paymentForm) {
        paymentForm.addEventListener('submit', handleSubmit);
        console.log('Attached submit event listener to payment-form');
    } else {
        console.log('payment-form not found in DOM, likely on Step 4');
    }
});