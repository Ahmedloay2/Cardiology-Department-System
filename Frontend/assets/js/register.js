// Patient Registration Form Handler with multipart/form-data support
document.addEventListener('DOMContentLoaded', function() {
    // Get form references
    const patientForm = document.getElementById('patient-registration');
    const photoInput = document.getElementById('photo-input');
    const patientPhoto = document.getElementById('patient-photo');
    
    // Handle photo upload preview
    photoInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function(event) {
                patientPhoto.src = event.target.result;
            };
            reader.readAsDataURL(file);
        }
    });
    
    // Handle form submission
    patientForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        // Show loading state
        const submitBtn = patientForm.querySelector('.submit-btn');
        const originalBtnText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
        submitBtn.disabled = true;
        
        // Clear previous error messages
        const errorContainer = document.getElementById('error-container');
        if (errorContainer) {
            errorContainer.innerHTML = '';
        }
        
        try {
            // Create FormData object for multipart/form-data
            const formData = new FormData(patientForm);
            
            // Prepare the data according to API requirements
            // Note: Field names must match exactly what the API expects
            const payload = new FormData();
            
            // Required fields
            payload.append('FName', formData.get('firstName'));
            payload.append('LName', formData.get('lastName'));
            payload.append('BirthDate', new Date(formData.get('dob')).toISOString());
            payload.append('Password', formData.get('password'));
            payload.append('Email', formData.get('email'));
            payload.append('Address', formData.get('address'));
            payload.append('EmergencyContactName', formData.get('emergencyContactName'));
            payload.append('EmergencyContactPhone', formData.get('emergencyContactPhone'));
            payload.append('Gender', formData.get('gender'));
            payload.append('ParentName', formData.get('parentName') || "");
            payload.append('Link', formData.get('referredBy') || "Direct");
            
            // Phone numbers as array
            const primaryPhone = formData.get('primaryMobile');
            const secondaryPhone = formData.get('secondaryMobile');
            payload.append('PhoneNumbers', primaryPhone);
            if (secondaryPhone) {
                payload.append('PhoneNumbers', secondaryPhone);
            }
            
            // Optional fields
            if (formData.get('landline')) payload.append('LandLine', formData.get('landline'));
            if (formData.get('spouseName')) payload.append('SpouseName', formData.get('spouseName'));
            if (formData.get('bloodGroup')) payload.append('BloodType', formData.get('bloodGroup'));
            if (formData.get('allergies')) payload.append('Allergies', formData.get('allergies'));
            if (formData.get('chronicConditions')) payload.append('ChronicConditions', formData.get('chronicConditions'));
            if (formData.get('surgeries')) payload.append('PreviousSurgeries', formData.get('surgeries'));
            if (formData.get('medications')) payload.append('CurrentMedications', formData.get('medications'));
            if (formData.get('policyNumber')) payload.append('PolicyNumber', formData.get('policyNumber'));
            if (formData.get('insuranceProvider')) payload.append('InsuranceProvider', formData.get('insuranceProvider'));
            if (formData.get('policyValidUntil')) {
                payload.append('PolicyValidDate', new Date(formData.get('policyValidUntil')).toISOString());
            }
            
            // Handle photo upload if provided
            const photoFile = photoInput.files[0];
            if (photoFile) {
                payload.append('Photo', photoFile);
                payload.append('PhotoPath', 'uploads/' + photoFile.name);
            }
            
            // Validate the payload before sending
            const validationErrors = validatePatientData(payload);
            
            if (validationErrors.length > 0) {
                displayError('Please correct the following errors:\n' + validationErrors.join('\n'));
                return;
            }
            
            // Make API call to register the patient with multipart/form-data
            const response = await fetch('https://cardiology-department-system.runasp.net/api/Patient/register', {
                method: 'POST',
                body: payload // No Content-Type header needed for FormData, browser sets it automatically
            });
            
            // Handle specific 405 Method Not Allowed error
            if (response.status === 405) {
                const allowHeader = response.headers.get('Allow');
                let methodNotAllowedMsg = 'Registration failed: The server does not support this operation.';
                
                if (allowHeader) {
                    methodNotAllowedMsg += ` Allowed methods are: ${allowHeader}.`;
                    console.info('Allowed methods:', allowHeader);
                }
                
                throw new Error(methodNotAllowedMsg);
            }
            
            // Handle other error responses
            if (!response.ok) {
                let errorMessage = 'Registration failed';
                
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorMessage;
                } catch (jsonError) {
                    try {
                        const textError = await response.text();
                        errorMessage = textError || `Registration failed: ${response.statusText}`;
                    } catch (textError) {
                        errorMessage = `Registration failed with status: ${response.status}`;
                    }
                }
                
                throw new Error(errorMessage);
            }
            
            // Handle successful registration
            alert('Registration successful! You can now log in to your account.');
            window.location.href = 'login.html';
            
        } catch (error) {
            console.error('Registration error:', error);
            displayError(error.message || 'Unknown error occurred during registration');
            
            // Provide recovery guidance for 405 errors
            if (error.message && error.message.includes('Method Not Allowed')) {
                const additionalHelp = document.createElement('div');
                additionalHelp.className = 'alert alert-info mt-3';
                additionalHelp.innerHTML = `
                    <h5>Troubleshooting Tips:</h5>
                    <ul>
                        <li>This appears to be an API configuration issue.</li>
                        <li>Please contact IT support and mention "405 Method Not Allowed" error.</li>
                        <li>If you're a developer, verify the endpoint URL and that it accepts POST requests.</li>
                    </ul>
                `;
                
                if (errorContainer) {
                    errorContainer.appendChild(additionalHelp);
                }
            }
        } finally {
            // Reset button state
            submitBtn.innerHTML = originalBtnText;
            submitBtn.disabled = false;
        }
    });
    
    document.querySelectorAll('.role-option').forEach(option => {
        option.addEventListener('click', () => {
            const role = option.getAttribute('data-role');
            if (role === 'patient') {
                window.location.href = 'register.html';
            } else if (role === 'doctor') {
                window.location.href = 'doctor.html';
            }
        });
    });
    
    // Helper function to display errors consistently
    function displayError(message) {
        const errorContainer = document.getElementById('error-container');
        if (errorContainer) {
            errorContainer.innerHTML = `<div class="alert alert-danger">${message}</div>`;
            errorContainer.scrollIntoView({ behavior: 'smooth' });
        } else {
            alert(message);
        }
    }
});

// Validation function for FormData payload
function validatePatientData(formData) {
    const errors = [];
    
    // Helper function to get values from FormData
    function getFormValue(key) {
        return formData.get(key) || (key === 'PhoneNumbers' ? formData.getAll(key) : null);
    }
    
    // Name validation
    const fName = getFormValue('FName');
    if (!fName || fName.length < 2 || fName.length > 50) {
        errors.push('First name must be between 2 and 50 characters');
    }
    
    const lName = getFormValue('LName');
    if (!lName || lName.length < 2 || lName.length > 50) {
        errors.push('Last name must be between 2 and 50 characters');
    }
    
    // Email validation
    const email = getFormValue('Email');
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!email || !emailRegex.test(email)) {
        errors.push('Please enter a valid email address');
    }
    
    // Password validation
    const password = getFormValue('Password');
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/;
    if (!password || !passwordRegex.test(password)) {
        errors.push('Password must be at least 8 characters and include uppercase, lowercase, number, and special character');
    }
    
    // Phone validation - Egyptian phone format
    const phoneRegex = /^(?:\+20|0)?1[0125]\d{8}$/;
    const phoneNumbers = formData.getAll('PhoneNumbers'); // Ensure we get all phone numbers as an array
    if (!phoneNumbers || phoneNumbers.length === 0) {
        errors.push('At least one phone number is required');
    } else if (!phoneNumbers.some(phone => phoneRegex.test(phone))) {
        errors.push('Please enter at least one valid phone number');
    }
    
    // Emergency contact validation
    const emergencyContactName = getFormValue('EmergencyContactName');
    if (!emergencyContactName || emergencyContactName.length < 2) {
        errors.push('Emergency contact name is required');
    }
    
    const emergencyContactPhone = getFormValue('EmergencyContactPhone');
    if (!emergencyContactPhone || !phoneRegex.test(emergencyContactPhone)) {
        errors.push('Please enter a valid emergency contact phone number');
    }
    
    // Check other required fields
    if (!getFormValue('Gender')) errors.push('Gender is required');
    if (!getFormValue('Address')) errors.push('Address is required');
    if (!getFormValue('ParentName')) errors.push('Parent name is required');
    if (!getFormValue('Link')) errors.push('Referral information is required');
    
    // Birth date validation
    try {
        const birthDate = new Date(getFormValue('BirthDate'));
        if (isNaN(birthDate.getTime())) {
            errors.push('Invalid birth date');
        }
    } catch (e) {
        errors.push('Invalid birth date format');
    }
    
    // Blood type validation if provided
    const bloodType = getFormValue('BloodType');
    if (bloodType) {
        const bloodTypeRegex = /^(A|B|AB|O)[+-]$/;
        if (!bloodTypeRegex.test(bloodType)) {
            errors.push('Please select a valid blood type');
        }
    }
    
    return errors;
}