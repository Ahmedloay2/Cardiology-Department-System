document.addEventListener('DOMContentLoaded', function() {
    // Get DOM elements
    const editBtn = document.getElementById('editBtn');
    const saveBtn = document.getElementById('saveBtn');
    const cancelBtn = document.getElementById('cancelBtn');
    const viewMode = document.getElementById('view-mode');
    const editMode = document.getElementById('edit-mode');
    const notification = document.getElementById('notification');
    const addNoteBtn = document.getElementById('add-note-btn');
    const newNoteText = document.getElementById('new-note-text');
    const notesDisplay = document.getElementById('notes-display');
  
    // Initialize notes
    const notes = [
        { text: "routine check up on Wednesday", date: "Dr. Rashmi G on 22 April 2013" },
        { text: "Needs wheelchair access.", date: "Dr. Anuraag D on 19 April 2013" }
    ];
  
    // Display initial notes
    renderNotes();
  
    // Edit button click handler
    editBtn.addEventListener('click', function() {
        // Transfer current values to edit form
        transferValuesToEditForm();
        
        // Switch modes
        viewMode.style.display = 'none';
        editMode.style.display = 'block';
    });
  
    // Save button click handler
    saveBtn.addEventListener('click', function() {
        // Transfer edited values back to view
        transferValuesToView();
        
        // Switch back to view mode
        editMode.style.display = 'none';
        viewMode.style.display = 'block';
        
        // Show success notification
        showNotification('Changes saved successfully!');
        
        // Here you would typically send data to your server
        // saveToServer();
    });
  
    // Cancel button click handler
    cancelBtn.addEventListener('click', function() {
        // Just switch back to view mode without saving
        editMode.style.display = 'none';
        viewMode.style.display = 'block';
    });
  
    // Add note button click handler
    addNoteBtn.addEventListener('click', function() {
        if (newNoteText.value.trim()) {
            const now = new Date();
            const dateStr = now.toLocaleDateString('en-US', { 
                year: 'numeric', 
                month: 'long', 
                day: 'numeric' 
            });
            
            notes.unshift({
                text: newNoteText.value,
                date: `Added on ${dateStr}`
            });
            
            newNoteText.value = '';
            renderNotes();
        }
    });
  
    // Helper function to transfer values to edit form
    function transferValuesToEditForm() {
        // Personal Details
        document.getElementById('edit-firstName').value = document.getElementById('display-firstName').textContent;
        document.getElementById('edit-lastName').value = document.getElementById('display-lastName').textContent;
        document.getElementById('edit-gender').value = document.getElementById('display-gender').textContent;
        document.getElementById('edit-dob').value = document.getElementById('display-dob').textContent;
        
        // Medical Information
        document.getElementById('edit-bloodGroup').value = document.getElementById('display-bloodGroup').textContent;
        document.getElementById('edit-referredBy').value = document.getElementById('display-referredBy').textContent;
        
        // Family Information
        document.getElementById('edit-parentName').value = document.getElementById('display-parentName').textContent;
        document.getElementById('edit-spouseName').value = document.getElementById('display-spouseName').textContent;
        
        // Contact Details
        document.getElementById('edit-primaryMobile').value = document.getElementById('display-primaryMobile').textContent;
        document.getElementById('edit-secondaryMobile').value = document.getElementById('display-secondaryMobile').textContent;
        document.getElementById('edit-landLine').value = document.getElementById('display-landLine').textContent;
        document.getElementById('edit-email').value = document.getElementById('display-email').textContent;
        document.getElementById('edit-streetAddress').value = document.getElementById('display-streetAddress').textContent;
        
        // Emergency Contact
        document.getElementById('edit-emergencyName').value = document.getElementById('display-emergencyName').textContent;
        document.getElementById('edit-emergencyPhone').value = document.getElementById('display-emergencyPhone').textContent;
        
        // Medical History
        document.getElementById('edit-allergies').value = document.getElementById('display-allergies').textContent;
        document.getElementById('edit-chronic').value = document.getElementById('display-chronic').textContent;
        document.getElementById('edit-surgeries').value = document.getElementById('display-surgeries').textContent;
        document.getElementById('edit-medications').value = document.getElementById('display-medications').textContent;
        
        // Health Insurance
        document.getElementById('edit-provider').value = document.getElementById('display-provider').textContent;
        document.getElementById('edit-policyNumber').value = document.getElementById('display-policyNumber').textContent;
        document.getElementById('edit-validUntil').value = document.getElementById('display-validUntil').textContent;
    }
  
    // Helper function to transfer edited values back to view
    function transferValuesToView() {
        // Personal Details
        document.getElementById('display-firstName').textContent = document.getElementById('edit-firstName').value;
        document.getElementById('display-lastName').textContent = document.getElementById('edit-lastName').value;
        document.getElementById('display-name').textContent = `${document.getElementById('edit-firstName').value} ${document.getElementById('edit-lastName').value}`;
        document.getElementById('display-gender').textContent = document.getElementById('edit-gender').value;
        document.getElementById('display-dob').textContent = document.getElementById('edit-dob').value;
        
        // Medical Information
        document.getElementById('display-bloodGroup').textContent = document.getElementById('edit-bloodGroup').value;
        document.getElementById('display-referredBy').textContent = document.getElementById('edit-referredBy').value;
        
        // Family Information
        document.getElementById('display-parentName').textContent = document.getElementById('edit-parentName').value;
        document.getElementById('display-spouseName').textContent = document.getElementById('edit-spouseName').value;
        
        // Contact Details
        document.getElementById('display-primaryMobile').textContent = document.getElementById('edit-primaryMobile').value;
        document.getElementById('display-secondaryMobile').textContent = document.getElementById('edit-secondaryMobile').value;
        document.getElementById('display-landLine').textContent = document.getElementById('edit-landLine').value;
        document.getElementById('display-email').textContent = document.getElementById('edit-email').value;
        document.getElementById('display-streetAddress').textContent = document.getElementById('edit-streetAddress').value;
        
        // Emergency Contact
        document.getElementById('display-emergencyName').textContent = document.getElementById('edit-emergencyName').value;
        document.getElementById('display-emergencyPhone').textContent = document.getElementById('edit-emergencyPhone').value;
        
        // Medical History
        document.getElementById('display-allergies').textContent = document.getElementById('edit-allergies').value;
        document.getElementById('display-chronic').textContent = document.getElementById('edit-chronic').value;
        document.getElementById('display-surgeries').textContent = document.getElementById('edit-surgeries').value;
        document.getElementById('display-medications').textContent = document.getElementById('edit-medications').value;
        
        // Health Insurance
        document.getElementById('display-provider').textContent = document.getElementById('edit-provider').value;
        document.getElementById('display-policyNumber').textContent = document.getElementById('edit-policyNumber').value;
        document.getElementById('display-validUntil').textContent = document.getElementById('edit-validUntil').value;
    }
  
    // Helper function to render notes
    function renderNotes() {
        notesDisplay.innerHTML = '';
        notes.forEach(note => {
            const noteItem = document.createElement('div');
            noteItem.className = 'note-item';
            noteItem.innerHTML = `
                <div>${note.text}</div>
                <div class="note-date"><i class="far fa-calendar-alt"></i> ${note.date}</div>
            `;
            notesDisplay.appendChild(noteItem);
        });
    }
  
    // Helper function to show notifications
    function showNotification(message) {
        notification.innerHTML = `<i class="fas fa-check-circle"></i> ${message}`;
        notification.classList.add('show');
        
        setTimeout(() => {
            notification.classList.remove('show');
        }, 3000);
    }
  
    // Optional: Function to save to server
    function saveToServer() {
        // This would be your actual API call
        const data = {
            firstName: document.getElementById('edit-firstName').value,
            lastName: document.getElementById('edit-lastName').value,
            // Include all other fields...
        };
        
        console.log('Data to save:', data);
        // fetch('/api/save-profile', { method: 'POST', body: JSON.stringify(data) })
    }
  });
  
  