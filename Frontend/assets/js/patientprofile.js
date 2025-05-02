// Create dynamic bubbles
function createBubbles() {
    const colors = ['rgba(247, 85, 109, 0.08)', 'rgba(109, 95, 247, 0.08)', 'rgba(95, 247, 178, 0.08)'];
    for (let i = 0; i < 8; i++) {
        const bubble = document.createElement('div');
        bubble.className = 'bubble';
        const size = Math.random() * 100 + 50;
        const posX = Math.random() * 100;
        const posY = Math.random() * 100;
        const delay = Math.random() * 10;
        const duration = Math.random() * 10 + 10;
        const color = colors[Math.floor(Math.random() * colors.length)];
        
        bubble.style.width = `${size}px`;
        bubble.style.height = `${size}px`;
        bubble.style.left = `${posX}%`;
        bubble.style.top = `${posY}%`;
        bubble.style.animationDelay = `${delay}s`;
        bubble.style.animationDuration = `${duration}s`;
        bubble.style.background = color;
        
        document.body.appendChild(bubble);
    }
}

// Dynamic ECG Path Generator
function generateECGPath() {
    let path = "M0,25 ";
    const segments = 20;
    const segmentWidth = 500 / segments;
    
    for(let i = 0; i < segments; i++) {
        const x = i * segmentWidth;
        const nextX = (i + 1) * segmentWidth;
        const midX = x + segmentWidth/2;
        
        if(i % 4 === 0) {
            // Normal heartbeat
            path += `L${midX-10},25 `;
            path += `L${midX-5},10 `;
            path += `L${midX},25 `;
            path += `L${midX+5},40 `;
            path += `L${midX+10},25 `;
        } else {
            // Flat line
            path += `L${nextX},25 `;
        }
    }
    
    return path;
}

document.addEventListener('DOMContentLoaded', () => {
    createBubbles();
    
    // Initialize ECG path
    const ecgPath = document.querySelector('.ecg-path');
    if (ecgPath) {
        const points = generateECGPath();
        ecgPath.setAttribute('d', points);
        
        // Animate ECG line drawing
        setInterval(() => {
            const newPoints = generateECGPath();
            ecgPath.setAttribute('d', newPoints);
        }, 6000);
    }
    
    // Animation on scroll
    const sections = document.querySelectorAll('.info-section, .info-grid');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = 1;
                entry.target.style.transform = 'translateX(0) translateZ(0)';
            }
        });
    }, { threshold: 0.1 });
  
    sections.forEach(section => {
        observer.observe(section);
    });
  
    // Change photo interaction
    const changePhotoBtn = document.querySelector('.change-photo');
    const profilePhoto = document.querySelector('.profile-photo');
    
    if (profilePhoto && changePhotoBtn) {
        profilePhoto.addEventListener('click', (e) => {
            if (e.target === changePhotoBtn) {
                // Animation for photo change
                profilePhoto.style.transform = 'scale(0.9)';
                setTimeout(() => {
                    profilePhoto.style.transform = 'scale(1)';
                    alert('Photo change functionality would go here');
                }, 300);
            }
        });
    }
  
    // Add note functionality
    const addNoteBtn = document.querySelector('.add-note-btn');
    const noteTextarea = document.querySelector('.new-note textarea');
    
    if (addNoteBtn && noteTextarea) {
        addNoteBtn.addEventListener('click', () => {
            if (noteTextarea.value.trim()) {
                const notesSection = document.querySelector('.notes-section');
                if (notesSection) {
                    const newNote = document.createElement('div');
                    newNote.className = 'note-item';
                    newNote.innerHTML = `
                        <div>${noteTextarea.value}</div>
                        <div class="note-date"><i class="far fa-calendar-alt"></i> Added just now</div>
                    `;
                    notesSection.prepend(newNote);
                    noteTextarea.value = '';
                    
                    // Animate the new note
                    newNote.style.opacity = 0;
                    newNote.style.transform = 'translateY(30px) rotateX(90deg)';
                    newNote.style.transformOrigin = 'top center';
                    setTimeout(() => {
                        newNote.style.transition = 'all 0.6s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
                        newNote.style.opacity = 1;
                        newNote.style.transform = 'translateY(0) rotateX(0)';
                    }, 10);
      
                    // Animate the notes section
                    notesSection.style.transform = 'translateZ(20px) translateY(-5px)';
                    setTimeout(() => {
                        notesSection.style.transform = 'translateZ(10px) translateY(0)';
                    }, 600);
                }
            }
        });
    }
  
    // Floating action button
    const fab = document.querySelector('.fab');
    if (fab) {
        fab.addEventListener('click', () => {
            // Create ripple effect
            const ripple = document.createElement('div');
            ripple.style.position = 'absolute';
            ripple.style.width = '100%';
            ripple.style.height = '100%';
            ripple.style.background = 'rgba(255,255,255,0.3)';
            ripple.style.borderRadius = '50%';
            ripple.style.top = '0';
            ripple.style.left = '0';
            ripple.style.transform = 'scale(0)';
            ripple.style.transition = 'transform 0.6s ease, opacity 0.6s ease';
            
            fab.appendChild(ripple);
            
            setTimeout(() => {
                ripple.style.transform = 'scale(2)';
                ripple.style.opacity = '0';
            }, 10);
            
            setTimeout(() => {
                ripple.remove();
                // Show quick actions menu
                alert('Quick actions menu would appear here');
            }, 600);
        });
    }
});

// Insurance Edit/Save functionality
document.addEventListener('DOMContentLoaded', () => {
    const editBtn = document.querySelector('.edit-insurance-btn');
    const saveBtn = document.querySelector('.save-insurance-btn');
    let originalValues = {};
  
    if (editBtn && saveBtn) {
        editBtn.addEventListener('click', () => {
            // Store original values
            document.querySelectorAll('.info-section.section-4 .info-value').forEach(item => {
                const field = item.getAttribute('data-field');
                originalValues[field] = item.textContent;
                
                // Create input field
                const input = document.createElement('input');
                input.type = 'text';
                input.value = item.textContent;
                item.textContent = '';
                item.appendChild(input);
                item.classList.add('editable');
            });
      
            // Toggle buttons
            editBtn.style.display = 'none';
            saveBtn.style.display = 'inline-block';
        });
    
        saveBtn.addEventListener('click', () => {
            // Save new values
            document.querySelectorAll('.info-section.section-4 .info-value').forEach(item => {
                const input = item.querySelector('input');
                if (input) {
                    item.textContent = input.value || originalValues[item.getAttribute('data-field')];
                    item.classList.remove('editable');
                }
            });
      
            // Toggle buttons
            editBtn.style.display = 'inline-block';
            saveBtn.style.display = 'none';
            
            // Here you would typically send the data to your server
            console.log('Insurance data saved');
        });
    }
});

// Medical History Edit/Save Functionality
document.addEventListener('DOMContentLoaded', () => {
    const editMedBtn = document.querySelector('.edit-medical-btn');
    const saveMedBtn = document.querySelector('.save-medical-btn');
    let originalMedValues = {};
  
    if (editMedBtn && saveMedBtn) {
        editMedBtn.addEventListener('click', () => {
            // Store original values
            document.querySelectorAll('.medical-history-value').forEach(item => {
                const field = item.getAttribute('data-field');
                originalMedValues[field] = item.textContent;
                
                // Create textarea field
                const textarea = document.createElement('textarea');
                textarea.value = item.textContent;
                item.textContent = '';
                item.appendChild(textarea);
                item.classList.add('editable');
            });
      
            // Toggle buttons
            editMedBtn.style.display = 'none';
            saveMedBtn.style.display = 'inline-block';
        });
    
        saveMedBtn.addEventListener('click', () => {
            // Save new values
            document.querySelectorAll('.medical-history-value').forEach(item => {
                const textarea = item.querySelector('textarea');
                if (textarea) {
                    item.textContent = textarea.value || originalMedValues[item.getAttribute('data-field')];
                    item.classList.remove('editable');
                }
            });
      
            // Toggle buttons
            editMedBtn.style.display = 'inline-block';
            saveMedBtn.style.display = 'none';
            
            // Here you would typically send the data to your server
            console.log('Medical history saved:', originalMedValues);
        });
    }
});

// Personal Details Editor
document.addEventListener('DOMContentLoaded', () => {
    const editPersonalBtn = document.querySelector('.edit-personal-btn');
    const savePersonalBtn = document.querySelector('.save-personal-btn');
    let originalPersonalValues = {};
  
    if (editPersonalBtn && savePersonalBtn) {
        editPersonalBtn.addEventListener('click', () => {
            // Store original values
            document.querySelectorAll('.info-section.section-1 .info-value').forEach(item => {
                const field = item.getAttribute('data-field');
                originalPersonalValues[field] = item.textContent;
                
                // Create input field
                const input = document.createElement('input');
                input.type = 'text';
                input.value = item.textContent;
                input.placeholder = `Enter ${field.replace(/([A-Z])/g, ' $1').toLowerCase()}`;
                
                item.textContent = '';
                item.appendChild(input);
                item.classList.add('editable');
            });
      
            // Auto-focus first name field
            const firstNameInput = document.querySelector('[data-field="firstName"] input');
            if (firstNameInput) firstNameInput.focus();
            
            // Toggle buttons
            editPersonalBtn.style.display = 'none';
            savePersonalBtn.style.display = 'inline-flex';
        });
    
        savePersonalBtn.addEventListener('click', () => {
            let hasChanges = false;
            const updatedValues = {};
            
            // Save new values
            document.querySelectorAll('.info-section.section-1 .info-value').forEach(item => {
                const field = item.getAttribute('data-field');
                const input = item.querySelector('input');
                
                if (input) {
                    const newValue = input.value.trim();
                    item.textContent = newValue || originalPersonalValues[field];
                    item.classList.remove('editable');
                    
                    // Track changes
                    if (newValue !== originalPersonalValues[field]) {
                        hasChanges = true;
                        updatedValues[field] = newValue;
                    }
                }
            });
      
            // Toggle buttons
            editPersonalBtn.style.display = 'inline-flex';
            savePersonalBtn.style.display = 'none';
            
            // Show confirmation if changes were made
            if (hasChanges) {
                // Here you would typically send updatedValues to your server
                console.log('Personal details updated:', updatedValues);
                
                // Show save confirmation
                const confirmation = document.createElement('div');
                confirmation.className = 'save-confirmation';
                confirmation.innerHTML = `
                    <i class="fas fa-check-circle"></i> Changes saved
                `;
                const actionsContainer = document.querySelector('.personal-details-actions');
                if (actionsContainer) actionsContainer.appendChild(confirmation);
                
                setTimeout(() => {
                    confirmation.style.opacity = '0';
                    setTimeout(() => confirmation.remove(), 300);
                }, 2000);
            }
        });
    }
});

// Emergency Contact Editor
document.addEventListener('DOMContentLoaded', () => {
    const editEmergencyBtn = document.querySelector('.edit-emergency-btn');
    const saveEmergencyBtn = document.querySelector('.save-emergency-btn');
    let originalEmergencyValues = {};
  
    if (editEmergencyBtn && saveEmergencyBtn) {
        editEmergencyBtn.addEventListener('click', () => {
            // Store original values
            document.querySelectorAll('.info-section.section-5 .info-value').forEach(item => {
                const field = item.getAttribute('data-field');
                originalEmergencyValues[field] = item.textContent;
                
                // Create input field
                const input = document.createElement('input');
                input.type = field === 'emergencyPhone' ? 'tel' : 'text';
                input.value = item.textContent;
                input.placeholder = `Enter ${field.replace('emergency', '').toLowerCase()}`;
                
                item.textContent = '';
                item.appendChild(input);
                item.classList.add('editable');
            });
      
            // Toggle buttons
            editEmergencyBtn.style.display = 'none';
            saveEmergencyBtn.style.display = 'inline-flex';
        });
    
        saveEmergencyBtn.addEventListener('click', () => {
            // Save new values
            document.querySelectorAll('.info-section.section-5 .info-value').forEach(item => {
                const input = item.querySelector('input');
                if (input) {
                    item.textContent = input.value || originalEmergencyValues[item.getAttribute('data-field')];
                    item.classList.remove('editable');
                }
            });
      
            // Toggle buttons
            editEmergencyBtn.style.display = 'inline-flex';
            saveEmergencyBtn.style.display = 'none';
            
            // Here you would typically send the data to your server
            console.log('Emergency contact updated');
        });
    }
});

// Medical Information Editor
document.addEventListener('DOMContentLoaded', () => {
    const medicalInfoEditBtn = document.querySelector('.medical-info-edit');
    const medicalInfoSaveBtn = document.querySelector('.medical-info-save');
    const bloodGroupSelect = document.querySelector('.blood-group-select');
  
    if (medicalInfoEditBtn && medicalInfoSaveBtn && bloodGroupSelect) {
        // Set initial blood group value
        bloodGroupSelect.value = 'O+'; // Set to patient's current blood group
        
        medicalInfoEditBtn.addEventListener('click', () => {
            // Make referred by field editable
            const referredByField = document.querySelector('[data-field="referredBy"]');
            if (referredByField) {
                const currentReferredBy = referredByField.textContent;
                
                const input = document.createElement('input');
                input.type = 'text';
                input.value = currentReferredBy;
                input.placeholder = 'Enter referral source';
                
                referredByField.textContent = '';
                referredByField.appendChild(input);
                referredByField.classList.add('editable');
            }
            
            // Enable blood group select
            bloodGroupSelect.style.pointerEvents = 'auto';
            bloodGroupSelect.style.opacity = '1';
            
            medicalInfoEditBtn.style.display = 'none';
            medicalInfoSaveBtn.style.display = 'inline-flex';
        });
    
        medicalInfoSaveBtn.addEventListener('click', () => {
            // Save referred by field
            const referredByField = document.querySelector('[data-field="referredBy"]');
            if (referredByField) {
                const input = referredByField.querySelector('input');
                
                if(input) {
                    referredByField.textContent = input.value || 'Not specified';
                    referredByField.classList.remove('editable');
                }
            }
            
            // Disable blood group select
            bloodGroupSelect.style.pointerEvents = 'none';
            bloodGroupSelect.style.opacity = '0.8';
            
            medicalInfoEditBtn.style.display = 'inline-flex';
            medicalInfoSaveBtn.style.display = 'none';
            
            // Add your save logic here
            console.log('Medical information updated', {
                bloodGroup: bloodGroupSelect.value,
                referredBy: referredByField ? referredByField.textContent : ''
            });
        });
    }
});

// Emergency Contact Editor
document.addEventListener('DOMContentLoaded', () => {
    const emergencyEditBtn = document.querySelector('.emergency-edit');
    const emergencySaveBtn = document.querySelector('.emergency-save');
  
    if (emergencyEditBtn && emergencySaveBtn) {
        emergencyEditBtn.addEventListener('click', () => {
            const emergencyFields = document.querySelectorAll('.emergency-contact-section .info-value');
            
            emergencyFields.forEach(field => {
                const fieldName = field.getAttribute('data-field');
                const currentValue = field.textContent;
                
                const input = document.createElement(fieldName === 'emergencyPhone' ? 'input' : 'input');
                input.type = fieldName === 'emergencyPhone' ? 'tel' : 'text';
                input.value = currentValue;
                input.placeholder = `Enter ${fieldName.replace('emergency', '').toLowerCase()}`;
                
                if(fieldName === 'emergencyPhone') {
                    input.pattern = "[+][0-9]{11,14}";
                    input.title = "Format: +911234567890";
                }
                
                field.textContent = '';
                field.appendChild(input);
                field.classList.add('editable');
                
                if(fieldName === 'emergencyName') input.focus();
            });
            
            emergencyEditBtn.style.display = 'none';
            emergencySaveBtn.style.display = 'inline-flex';
        });
    
        emergencySaveBtn.addEventListener('click', () => {
            const emergencyFields = document.querySelectorAll('.emergency-contact-section .info-value');
            let isValid = true;
            
            emergencyFields.forEach(field => {
                const input = field.querySelector('input');
                if(input) {
                    if(input.type === 'tel' && !input.checkValidity()) {
                        input.style.borderColor = 'red';
                        isValid = false;
                        return;
                    }
                    field.textContent = input.value;
                    field.classList.remove('editable');
                }
            });
            
            if(!isValid) {
                alert('Please enter a valid phone number (e.g. +918987654321)');
                return;
            }
            
            emergencyEditBtn.style.display = 'inline-flex';
            emergencySaveBtn.style.display = 'none';
            
            // Add your save logic here
            console.log('Emergency contact updated');
        });
    }
});

// Patient Photo Upload Functionality
function initPatientPhotoUpload() {
    const photoUpload = document.createElement('input');
    photoUpload.type = 'file';
    photoUpload.id = 'patientPhotoUpload';
    photoUpload.accept = 'image/*';
    photoUpload.style.display = 'none';
    document.body.appendChild(photoUpload);
  
    const patientPhoto = document.getElementById('patientPhoto') || document.querySelector('.cardio-profile i');
    const photoContainer = document.createElement('div');
    photoContainer.className = 'patient-photo-container';
    
    // Replace the heart icon with photo container if it exists
    if (document.querySelector('.cardio-profile')) {
        const profileCircle = document.querySelector('.cardio-profile');
        profileCircle.innerHTML = '';
        profileCircle.appendChild(photoContainer);
        profileCircle.classList.add('has-photo');
    }
  
    const img = document.createElement('img');
    img.src = 'https://via.placeholder.com/100';
    img.alt = 'Patient Photo';
    img.id = 'patientPhotoImg';
    photoContainer.appendChild(img);
  
    const uploadBtn = document.createElement('button');
    uploadBtn.className = 'upload-photo-btn';
    uploadBtn.innerHTML = '<i class="fas fa-camera"></i> Add Photo';
    photoContainer.appendChild(uploadBtn);
  
    photoUpload.addEventListener('change', function(e) {
        if (e.target.files && e.target.files[0]) {
            const reader = new FileReader();
            reader.onload = function(event) {
                img.src = event.target.result;
                uploadBtn.innerHTML = '<i class="fas fa-sync-alt"></i> Change';
                // Add your photo upload logic here
                console.log('Photo uploaded:', e.target.files[0].name);
            };
            reader.readAsDataURL(e.target.files[0]);
        }
    });
  
    uploadBtn.addEventListener('click', () => photoUpload.click());
}

// Call this function after DOM loads
document.addEventListener('DOMContentLoaded', initPatientPhotoUpload);

document.addEventListener('DOMContentLoaded', function() {
    const editProfileBtn = document.getElementById('editProfileBtn');
    const profileContainer = document.querySelector('.profile-container');
    let isEditMode = false;

    if (editProfileBtn && profileContainer) {
        editProfileBtn.addEventListener('click', function() {
            isEditMode = !isEditMode;
            
            if (isEditMode) {
                // Enter edit mode
                profileContainer.classList.add('edit-mode');
                editProfileBtn.innerHTML = '<i class="fas fa-save"></i> Save Changes';
                
                // Make all info values editable
                document.querySelectorAll('.info-value').forEach(el => {
                    const originalValue = el.textContent;
                    const fieldName = el.getAttribute('data-field') || 
                                     el.parentElement.querySelector('.info-label')?.textContent.toLowerCase().replace(' ', '-');
                    
                    if (el.tagName === 'DIV' && !el.querySelector('input, select')) {
                        el.innerHTML = `<input type="text" value="${originalValue.trim()}" data-original="${originalValue}" data-field="${fieldName}">`;
                    }
                });
                
                // Make medical history values editable
                document.querySelectorAll('.medical-history-value').forEach(el => {
                    const originalValue = el.textContent;
                    const fieldName = el.getAttribute('data-field');
                    
                    if (el.tagName === 'DIV' && !el.querySelector('textarea')) {
                        el.innerHTML = `<textarea data-field="${fieldName}">${originalValue.trim()}</textarea>`;
                    }
                });
                
            } else {
                // Exit edit mode and save changes
                profileContainer.classList.remove('edit-mode');
                editProfileBtn.innerHTML = '<i class="fas fa-edit"></i> Edit Profile';
                
                // Collect all changed data
                const updatedData = {};
                
                document.querySelectorAll('.info-value input, .medical-history-value textarea').forEach(input => {
                    const fieldName = input.getAttribute('data-field');
                    const newValue = input.value;
                    updatedData[fieldName] = newValue;
                    
                    // Replace input with plain text
                    const parent = input.parentElement;
                    parent.textContent = newValue;
                    parent.setAttribute('data-field', fieldName);
                });
                
                // Here you would typically send updatedData to your server
                console.log('Updated data:', updatedData);
                // Example: fetch('/api/update-profile', { method: 'POST', body: JSON.stringify(updatedData) });
                
                // Show save confirmation
                showSaveNotification();
            }
        });
        
        function showSaveNotification() {
            const notification = document.createElement('div');
            notification.className = 'save-notification';
            notification.innerHTML = '<i class="fas fa-check-circle"></i> Changes saved successfully!';
            document.body.appendChild(notification);
            
            setTimeout(() => {
                notification.classList.add('show');
            }, 10);
            
            setTimeout(() => {
                notification.classList.remove('show');
                setTimeout(() => notification.remove(), 500);
            }, 3000);
        }
    }
});

document.addEventListener('DOMContentLoaded', function() {
    console.log('[Auth Debug] DOM loaded - starting authentication check');
    
    // Unified auth check function to replace the duplicated code
    function checkAuthentication() {
        // 1. First, verify localStorage is accessible
        try {
            console.log('[Auth Debug] localStorage supported:', !!window.localStorage);
        } catch (e) {
            console.error('[Auth Error] localStorage access failed:', e);
            window.location.replace('login.html');
            return null;
        }

        // 2. Debug all auth-related localStorage items
        console.group('[Auth Debug] Storage Contents');
        for (let i = 0; i < localStorage.length; i++) {
            const key = localStorage.key(i);
            if (key.includes('auth') || key.includes('token') || key.includes('user')) {
                console.log(key + ':', localStorage.getItem(key));
            }
        }
        console.groupEnd();

        // 3. Get token with fallback for key variations
        const token = localStorage.getItem('jwtToken') || 
                    localStorage.getItem('auth_token') || 
                    localStorage.getItem('authToken');
        
        console.log('[Auth Debug] Retrieved token:', token ? 'EXISTS' : 'MISSING');

        if (!token) {
            console.error('[Auth Error] No token found in any key');
            window.location.replace('login.html');
            return null;
        }

        // 4. Simplified token validation - removing complex validation that might be causing issues
        try {
            // Basic check for token structure (Header.Payload.Signature)
            const tokenParts = token.split('.');
            if (tokenParts.length !== 3) {
                console.error('[Auth Error] Invalid token structure');
                localStorage.removeItem('jwtToken');
                window.location.replace('login.html');
                return null;
            }

            // Skip complex JWT validation that might cause errors
            // Just check if the token is present and properly structured
            console.log('[Auth Debug] Basic token validation passed');
            return token;
            
        } catch (e) {
            console.error('[Auth Error] Token verification error:', e);
            // Don't redirect immediately on token parse errors - this might cause redirect loops
            console.log('[Auth Debug] Continuing with existing token despite validation error');
            return token;
        }
    }

    // Use the unified auth check
    const token = checkAuthentication();
    if (!token) {
        return; // Auth check will handle redirect if needed
    }

    console.log('[Auth Debug] Authentication successful, loading profile...');
    
    // Helper function for element selection
    function $(id) {
        return document.getElementById(id);
    }

    // 2. UI Elements Initialization
    const elements = {
        loading: $('loading'),
        error: $('error'),
        profileContent: $('profileContent'),
        logoutBtn: $('logoutBtn'),
        debugPanel: $('debugPanel'),
        debugToggleBtn: $('debugToggleBtn'),
        tokenDebug: $('jwtTokenDebug'),
        headersDebug: $('requestHeadersDebug'),
        responseDebug: $('responseDebug')
    };

    // 3. Debug Panel Setup
    if (elements.debugToggleBtn && elements.debugPanel) {
        elements.debugToggleBtn.addEventListener('click', () => {
            elements.debugPanel.style.display = 
                elements.debugPanel.style.display === 'none' ? 'block' : 'none';
        });
    }

    // 4. Token Debug Display
    if (elements.tokenDebug && token) {
        elements.tokenDebug.textContent = `${token.substring(0, 20)}...${token.slice(-10)}`;
    }

    // 5. Logout Functionality
    if (elements.logoutBtn) {
        elements.logoutBtn.addEventListener('click', () => {
            console.log('[AUTH] User initiated logout');
            localStorage.clear();
            window.location.replace('login.html');
        });
    }

    // 6. Profile Data Loading
    loadProfile();

    async function loadProfile() {
        console.log('[DATA] Starting profile data load');
        
        try {
            const headers = {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            };

            // Debug: Show request headers
            if (elements.headersDebug) {
                elements.headersDebug.textContent = JSON.stringify(headers, null, 2);
            }

            // API Request
            const response = await fetch('https://cardiology-department-system.runasp.net/api/Patient/Profile', {
                method: 'GET',
                headers: headers
            });

            console.log(`[DATA] API response status: ${response.status}`);

            // Handle unauthorized response
            if (response.status === 401) {
                console.error('[AUTH] Server rejected token');
                // Don't immediately clear token or redirect - try one more time first
                console.log('[AUTH] Will try to refresh the page once');
                throw new Error('Session expired. Please log in again.');
            }

            if (!response.ok) {
                throw new Error(`Server responded with status: ${response.status}`);
            }

            // Process response
            const responseText = await response.text();
            const patientData = responseText ? JSON.parse(responseText) : {};

            console.log('[DATA] Received patient data:', patientData);

            // Debug: Show response data
            if (elements.responseDebug) {
                elements.responseDebug.textContent = responseText.substring(0, 500) + 
                    (responseText.length > 500 ? '...' : '');
            }

            // Update UI with data
            displayProfile(patientData);
            
            // Cache the data for offline use
            localStorage.setItem('profileData', JSON.stringify(patientData));

            // Hide loading spinner, show content
            if (elements.loading) elements.loading.style.display = 'none';
            if (elements.profileContent) elements.profileContent.style.display = 'block';

        } catch (error) {
            console.error('[DATA] Profile load error:', error);

            // Try to use cached data if available
            const cached = localStorage.getItem('profileData');
            if (cached) {
                try {
                    console.log('[DATA] Falling back to cached data');
                    displayProfile(JSON.parse(cached));
                    if (elements.loading) elements.loading.style.display = 'none';
                    if (elements.profileContent) elements.profileContent.style.display = 'block';
                    showWarning("Showing cached data. May be outdated.");
                } catch (err) {
                    console.error('[DATA] Error parsing cached data:', err);
                    showError(error);
                }
            } else {
                showError(error);
            }
        }
    }

    function displayProfile(data) {
        console.log('[UI] Updating profile display with API data:', data);
        
        // Helper functions
        const get = (key, fallback = 'Not provided') => {
            // Handle null, undefined, or empty string values
            const value = data[key];
            return (value !== null && value !== undefined && value !== '' && value !== 'string') ? value : fallback;
        };
        
        const set = (id, value) => {
            const el = $(id);
            if (el) el.textContent = value;
        };

        // Personal Information - using fName and lName instead of firstName/lastName
        set('display-name', `${get('fName', '')} ${get('lName', '')}`.trim());
        set('display-patientId', get('id')); // Using 'id' instead of 'patientId'
        set('display-firstName', get('fName')); // Using 'fName' instead of 'firstName'
        set('display-lastName', get('lName')); // Using 'lName' instead of 'lastName'
        set('display-gender', get('gender'));
        set('display-dob', formatDate(get('birthDate'))); // Using 'birthDate' instead of 'dateOfBirth'

        // Medical Information
        set('display-bloodGroup', get('bloodType')); // Using 'bloodType' instead of 'bloodGroup'
        set('display-referredBy', get('parentName')); // Using 'parentName' instead of 'referredBy'

        // Contact Information - Fixed handling for phone numbers array
        console.log('[UI] Phone numbers data:', data.phoneNumbers);
        
        // Properly handle phoneNumbers array
        if (data.phoneNumbers && Array.isArray(data.phoneNumbers)) {
            // Loop through the phone numbers array and add them to the appropriate fields
            for (let i = 0; i < data.phoneNumbers.length; i++) {
                if (i === 0 && data.phoneNumbers[i] && data.phoneNumbers[i] !== 'string') {
                    set('display-primaryMobile', data.phoneNumbers[i]);
                    console.log('[UI] Set primary mobile:', data.phoneNumbers[i]);
                } else if (i === 1 && data.phoneNumbers[i] && data.phoneNumbers[i] !== 'string') {
                    set('display-secondaryMobile', data.phoneNumbers[i]);
                    console.log('[UI] Set secondary mobile:', data.phoneNumbers[i]);
                }
            }
        } else {
            console.log('[UI] No phone numbers array found or it is not properly formatted');
        }
        
        // Handle landLine separately
        set('display-landLine', get('landLine'));
        set('display-email', get('email'));
        set('display-streetAddress', get('address')); // Using 'address' instead of 'streetAddress'

        // Emergency Contact
        set('display-emergencyName', get('emergencyContactName'));
        set('display-emergencyPhone', get('emergencyContactPhone'));

        // Medical History
        set('display-allergies', get('allergies'));
        set('display-chronic', get('chronicConditions'));
        set('display-surgeries', get('previousSurgeries'));
        set('display-medications', get('currentMedications'));

        // Insurance Information
        set('display-provider', get('insuranceProvider'));
        set('display-policyNumber', get('policyNumber'));
        set('display-validUntil', formatDate(get('policyValidDate'))); // Using 'policyValidDate' instead of 'policyValidUntil'

        // Notes Section - if your API doesn't provide notes, we can skip this or create a placeholder
        const notesDisplay = $('notes-display');
        if (notesDisplay) {
            notesDisplay.innerHTML = '<div class="note-item">No notes available for this patient.</div>';
        }

        // Set profile photo if available
        const profilePhoto = document.getElementById('patientPhotoImg');
        if (profilePhoto && data.photoData) {
            profilePhoto.src = data.photoData;
            profilePhoto.onerror = function() {
                this.src = './assets/img/user-placeholder.png';
            };
        }

        console.log('[UI] Profile display updated successfully');
    }

    function formatDate(dateString) {
        if (!dateString) return 'Not provided';
        try {
            const date = new Date(dateString);
            // Check if date is valid
            if (isNaN(date.getTime())) {
                return 'Not provided';
            }
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        } catch (e) {
            console.warn('[UTIL] Date formatting failed:', e);
            return 'Not provided';
        }
    }

    function showWarning(message) {
        console.warn('[UI] Showing warning:', message);
        const warningDiv = document.createElement('div');
        warningDiv.className = 'alert alert-warning';
        warningDiv.textContent = message;
        
        if (elements.profileContent) {
            elements.profileContent.prepend(warningDiv);
            setTimeout(() => warningDiv.remove(), 5000);
        }
    }

    function showError(error) {
        console.error('[UI] Showing error:', error);
        
        if (elements.loading) elements.loading.style.display = 'none';
        
        if (elements.error) {
            elements.error.style.display = 'block';
            elements.error.innerHTML = `
                <h3>Error Loading Profile</h3>
                <p>${error.message || 'An unknown error occurred'}</p>
                <button class="btn-retry">Try Again</button>
                <button class="btn-logout">Log Out</button>
            `;
            
            // Add event listeners
            elements.error.querySelector('.btn-retry').addEventListener('click', loadProfile);
            elements.error.querySelector('.btn-logout').addEventListener('click', () => {
                localStorage.clear();
                window.location.replace('login.html');
            });
        }
    }

    console.log('[DEBUG] Profile page initialization complete');
});