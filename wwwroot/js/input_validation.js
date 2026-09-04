// Toggle password visibility with eye icons
// =====================================================
// TOGGLE PASSWORD VISIBILITY
// Supports Bootstrap Icons and Font Awesome
// =====================================================

function togglePassword(inputId, iconId) {

    var input = document.getElementById(inputId);
    var icon = document.getElementById(iconId);

    if (!input || !icon) {
        return;
    }


    // =================================================
    // SHOW PASSWORD
    // =================================================

    if (input.type === "password") {

        input.type = "text";


        // Bootstrap Icons
        if (icon.classList.contains("bi-eye-slash")) {

            icon.classList.remove("bi-eye-slash");
            icon.classList.add("bi-eye");

        }


        // Font Awesome
        if (icon.classList.contains("fa-eye-slash")) {

            icon.classList.remove("fa-eye-slash");
            icon.classList.add("fa-eye");

        }

    }


    // =================================================
    // HIDE PASSWORD
    // =================================================

    else {

        input.type = "password";


        // Bootstrap Icons
        if (icon.classList.contains("bi-eye")) {

            icon.classList.remove("bi-eye");
            icon.classList.add("bi-eye-slash");

        }


        // Font Awesome
        if (icon.classList.contains("fa-eye")) {

            icon.classList.remove("fa-eye");
            icon.classList.add("fa-eye-slash");

        }

    }
}


// Email validation
function validateEmail(inputId, errorId) {
    var email = document.getElementById(inputId).value;
    var error = document.getElementById(errorId);
    var regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!regex.test(email)) {
        error.textContent = "Invalid email format";
    } else {
        error.textContent = "";
    }
}

// Password strength validation
function validatePassword(inputId, errorId) {
    var password = document.getElementById(inputId).value;
    var error = document.getElementById(errorId);
    var regex = /^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$/;

    if (!regex.test(password)) {
        error.textContent = "Password must be 8+ chars, include uppercase, number, and special character.";
    } else {
        error.textContent = "";
    }
}

// Username validation
function validateUsername(inputId, errorId) {
    var username = document.getElementById(inputId).value;
    var error = document.getElementById(errorId);
    var regex = /^[a-z0-9_]+$/; // only lowercase letters, numbers, underscore

    if (!regex.test(username)) {
        error.textContent = "Username can only contain lowercase letters, numbers, and underscores.";
        return;
    }

    // Clear format error
    error.textContent = "";

    // Check availability via AJAX
    fetch('/Patient/CheckUsername?username=' + encodeURIComponent(username))
        .then(response => response.json())
        .then(data => {
            if (!data.available) {
                error.textContent = "This username is already taken.";
            }
        })
        .catch(err => {
            console.error("Error checking username:", err);
        });
}
