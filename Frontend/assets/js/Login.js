document.addEventListener('DOMContentLoaded', function() {
    const form = document.querySelector('form');
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const roleRadios = document.querySelectorAll('input[name="role"]');
    
    // Base API URL - replace with your actual API URL when deploying
    const API_BASE_URL = 'https://cardiology-department-system.runasp.net/api';
    
    form.addEventListener('submit', async function(e) {
        e.preventDefault();

        // Get selected role
        let selectedRole = '';
        for (const radio of roleRadios) {
            if (radio.checked) {
                selectedRole = radio.value;
                break;
            }
        }
        
        if (!selectedRole) {
            showError('Please select a role (Doctor, Patient, or Admin)');
            return;
        }
        
        // Validate form
        if (!validateForm()) {
            return;
        }
        
        // Prepare login data
        const loginData = {
            email: emailInput.value.trim(),
            password: passwordInput.value
        };
        
        try {
            // Determine the appropriate endpoint based on the selected role
            const endpoint = `${API_BASE_URL}/${capitalize(selectedRole)}/Login`;
            
            console.log("Sending login request to:", endpoint);
            console.log("Login data:", loginData);
            
            // Send login request
            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(loginData),
                credentials: 'include' // Include cookies for session management
            });
            
            console.log("Response status:", response.status);
            
            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Login failed. Please check your credentials.');
            }
            
            const data = await response.json();
            console.log("Login success, received data:", data);
            
            // Save authentication token if provided - UPDATED to use consistent key names
            if (data.token) {
                // Store token with multiple key names for backwards compatibility
                localStorage.setItem('auth_token', data.token);
                localStorage.setItem('jwtToken', data.token); // Add this consistent key name
                localStorage.setItem('authToken', data.token); // Add another common key name
                console.log("Saved auth token with multiple keys for compatibility");
            }
            
            // Save patient email
            localStorage.setItem("patientEmail", emailInput.value.trim());
            console.log("Saved patient email:", emailInput.value.trim());
            
            // Save patient ID - updated to use 'id' field to match API response
            if (data.id) {
                localStorage.setItem("patientId", data.id);
                console.log("Saved patient ID:", data.id);
            } else if (data.patientId) {
                localStorage.setItem("patientId", data.patientId);
                console.log("Saved patient ID (from patientId field):", data.patientId);
            }
            
            // Save user role
            localStorage.setItem('user_role', selectedRole);
            console.log("Saved user role:", selectedRole);
            
            // Redirect to appropriate dashboard based on role
            redirectToDashboard(selectedRole);
            
        } catch (error) {
            showError(error.message || 'Login failed. Please try again later.');
            console.error('Login error:', error);
        }
    });
    
    function validateForm() {
        // Email validation
        const email = emailInput.value.trim();
        if (!email) {
            showError('Email is required');
            return false;
        }
        
        const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        if (!emailRegex.test(email)) {
            showError('Please enter a valid email address');
            return false;
        }
        
        // Password validation
        const password = passwordInput.value;
        if (!password) {
            showError('Password is required');
            return false;
        }
        
        if (password.length < 8) {
            showError('Password must be at least 8 characters long');
            return false;
        }
        
        return true;
    }
    
    function showError(message) {
        // Create error alert if it doesn't exist
        let errorAlert = document.querySelector('.error-alert');
        
        if (!errorAlert) {
            errorAlert = document.createElement('div');
            errorAlert.className = 'error-alert alert alert-danger mt-3';
            form.prepend(errorAlert);
        }
        
        errorAlert.textContent = message;
        errorAlert.style.display = 'block';
        
        // Hide error after 5 seconds
        setTimeout(() => {
            errorAlert.style.display = 'none';
        }, 5000);
    }
    
    function redirectToDashboard(role) {
        switch (role) {
            case 'doctor':
                window.location.href = 'doctor-dashboard.html';
                break;
            case 'patient':
                window.location.href = 'patientprofile.html';
                break;
            case 'admin':
                window.location.href = 'admin-dashboard.html';
                break;
            default:
                window.location.href = 'index.html';
        }
    }

    function capitalize(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }
});