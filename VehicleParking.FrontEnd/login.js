const API_BASE_URL = "http://localhost:5164/api";

document.getElementById('loginForm').addEventListener('submit', async function(e) {
    e.preventDefault();

    const selectedRole = document.getElementById('loginRole').value;
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    const alertBox = document.getElementById('loginAlert');

    try {
        const response = await fetch(`${API_BASE_URL}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: email, password: password }) 
        });

        const data = await response.json();

        if (response.ok) {
            if (data.user.role.toLowerCase() !== selectedRole.toLowerCase()) {
                alertBox.classList.remove('d-none');
                alertBox.className = "alert alert-danger";
                alertBox.textContent = `Access denied! This account is not registered as an ${selectedRole}.`;
                return;
            }
            localStorage.setItem('jwtToken', data.token);
            localStorage.setItem('userName', data.user.fullName);
            localStorage.setItem('userRole', data.user.role);

            window.location.href = "dashboard.html";
        } else {
            alertBox.classList.remove('d-none');
            alertBox.className = "alert alert-danger";
            alertBox.textContent = data.message || "Invalid email or password!";
        }
    } catch (err) {
        alertBox.classList.remove('d-none');
        alertBox.className = "alert alert-danger";
        alertBox.textContent = "Network error during login.";
    }
});