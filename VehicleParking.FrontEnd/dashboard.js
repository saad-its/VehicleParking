// 1. CONFIGURATION & INITIAL SETUP
const API_BASE_URL = "http://localhost:5164/api/Parking";

const token = localStorage.getItem("jwtToken");
const userName = localStorage.getItem("userName");
const userRole = localStorage.getItem("userRole");

//if tokin is not available so push the login page
if (!token) {
  window.location.href = "login.html";
} else {
  document.getElementById("welcomeUser").textContent =
    `Welcome, ${userName || "User"}!`;
}
if (userRole && userRole.toLowerCase() === "admin") {
  const adminSec = document.getElementById("adminSection");
  if (adminSec) {
    adminSec.classList.remove("d-none");
  }
}
document.addEventListener("DOMContentLoaded", function () {
  loadStatsAndSlots();
  loadTotalRevenue();
});

// 2. FETCH & DISPLAY PARKING SLOTS
async function loadParkingSlots() {
  try {
    const response = await fetch(`${API_BASE_URL}/slots`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    });

    if (response.ok) {
      const slots = await response.json();
      const tbody = document.getElementById("slotsTableBody");
      tbody.innerHTML = "";

      slots.forEach((slot) => {
        //checked the slot to park the vehicle or not
        let hasVehicle =
          slot.currentVehicleNumber &&
          slot.currentVehicleNumber !== "N/A" &&
          slot.currentVehicleNumber.trim() !== "";

        const badgeClass = hasVehicle ? "bg-danger" : "bg-success";
        const statusText = hasVehicle ? "Booked" : "Available";
        const vehicleDisplay = hasVehicle ? slot.currentVehicleNumber : "N/A";
        const ticketDisplay = hasVehicle && slot.ticketNumber ? slot.ticketNumber : "N/A";

        let sNum = slot.slotNumber || "N/A";
        let sType = slot.vehicleType || "Car";
        let vNum = slot.currentVehicleNumber || "N/A";
        let tNum = slot.ticketNumber || "TKT-" + slot.id;
        let bTime = slot.bookingTime || slot.checkInTime || slot.createdAt || "";

        // Agar vehicle booked hai toh Release aur Print button dikhao, warna '--'
let actionBtn = hasVehicle
    ? `<button class="btn btn-warning btn-sm me-1" onclick="openPaymentModal(${slot.id}, '${slot.vehicleType}', '${slot.bookingTime}')">Release</button>
       <button class="btn btn-secondary btn-sm" onclick="printTicket('${slot.slotNumber}', '${slot.ticketNumber}', '${slot.vehicleType}', '${slot.currentVehicleNumber}', '${slot.bookingTime}')">Print</button>`
    : `<span class="text-muted">--</span>`;

        const row = `
                    <tr>
                        <td><strong>Slot #${slot.slotNumber}</strong></td>
                        <td>${slot.vehicleType}</td>
                        <td><span class="badge ${badgeClass}">${statusText}</span></td>
                        <td>${vehicleDisplay}</td>
                        <td><code>${ticketDisplay}</code></td>
                        <td>${actionBtn}</td>
                    </tr>
                `;
        tbody.innerHTML += row;
      });
    } else if (response.status === 401) {
      alert("Session expired! Please login again.");
      logout();
    } else {
      showDashAlert("Failed to load parking slots.", "danger");
    }
  } catch (err) {
    console.error("Error loading slots:", err);
    showDashAlert("Network error while loading slots.", "danger");
  }
}

// 3. STATS & TOTAL REVENUE FUNCTIONS
async function loadStatsAndSlots() {
  await loadParkingSlots();

  try {
    const res = await fetch(`${API_BASE_URL}/stats`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.ok) {
      const stats = await res.json();
      document.getElementById("carSpaceCount").textContent = stats.carAvailable;
      document.getElementById("bikeSpaceCount").textContent =
        stats.bikeAvailable;
      document.getElementById("truckSpaceCount").textContent =
        stats.truckAvailable;
    }
  } catch (err) {
    console.error("Error loading stats", err);
  }
}

//Load total revenue function through backend
async function loadTotalRevenue() {
  try {
    const response = await fetch(`${API_BASE_URL}/revenue`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    });

    if (response.ok) {
      const data = await response.json();
      const revenueElement = document.getElementById("totalRevenueDisplay");
      if (revenueElement) {
        revenueElement.innerText = `Rs. ${data.totalRevenue}`;
      }
    }
  } catch (error) {
    console.error("Error fetching revenue:", error);
  }
}

// 4. BOOKING & SLOT DROPDOWNS
async function loadAvailableSlotDropdown() {
  const vehicleType = document.getElementById("selectVehicleType").value;
  const slotDropdown = document.getElementById("selectSlotId");
  slotDropdown.innerHTML = '<option value="">Loading...</option>';

  if (!vehicleType) {
    slotDropdown.innerHTML =
      '<option value=""> Select Slot Type First </option>';
    return;
  }

  try {
    const response = await fetch(`${API_BASE_URL}/available`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (response.ok) {
      const slots = await response.json();
      const filteredSlots = slots.filter(
        (s) => s.vehicleType.toLowerCase() === vehicleType.toLowerCase(),
      );

      slotDropdown.innerHTML = "";
      if (filteredSlots.length === 0) {
        alert(`Sorry! Space is full for ${vehicleType}s.`);
        slotDropdown.innerHTML = '<option value="">No slots available</option>';
        return;
      }

      filteredSlots.forEach((slot) => {
        const opt = document.createElement("option");
        opt.value = slot.id;
        opt.textContent = `Slot #${slot.slotNumber}`;
        slotDropdown.appendChild(opt);
      });
    }
  } catch (err) {
    console.error("Error fetching available slots", err);
  }
}

async function bookFromForm() {
  const slotId = document.getElementById("selectSlotId").value;
  const vehicleNumber = document.getElementById("inputVehicleNumber").value;

  if (!slotId || !vehicleNumber) {
    alert("Please fill all fields!");
    return;
  }

  try {
    const response = await fetch(`${API_BASE_URL}/book`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        slotId: parseInt(slotId),
        vehicleNumber: vehicleNumber,
      }),
    });

    const data = await response.json();
    if (response.ok) {
      alert(
        data.message +
          (data.ticketNumber ? `\nTicket Number: ${data.ticketNumber}` : ""),
      );
      document.getElementById("inputVehicleNumber").value = "";
      loadStatsAndSlots();
      document.getElementById("selectSlotId").innerHTML =
        '<option value="">-- Select Slot Type First --</option>';
      document.getElementById("selectVehicleType").value = "";
    } else {
      alert(data.message || "Booking failed");
    }
  } catch (err) {
    alert("Network error during booking.");
  }
}

// 5. PAYMENT MODAL & SLOT RELEASE
let currentSlotIdForPayment = null;
let calculatedFeeGlobal = 0;
let totalHoursGlobal = 1;

// Jab user 'Release' dabaye toh hours calculate ho kar modal khule
function openPaymentModal(slotId, vehicleType, bookingTime) {
    currentSlotIdForPayment = slotId;
    document.getElementById('payVehicleType').innerText = vehicleType || 'Car';

    // 1. Hourly rate set 
    let hourlyRate = 50;
    if (vehicleType && vehicleType.toLowerCase() === "car") {
        hourlyRate = 100;
    } else if (vehicleType && vehicleType.toLowerCase() === "truck") {
        hourlyRate = 200;
    }
    // 2. Hours calculate 
    let totalHours = 1;
    if (bookingTime && bookingTime !== "null" && bookingTime !== "undefined") {
        const checkIn = new Date(bookingTime);
        if (!isNaN(checkIn.getTime())) {
            const now = new Date();
            const diffMs = now - checkIn;
            const diffHours = diffMs / (1000 * 60 * 60);
            totalHours = Math.max(1, Math.ceil(diffHours));
        }
    }

    totalHoursGlobal = totalHours;
    calculatedFeeGlobal = totalHours * hourlyRate;

    // 3. Modal ke andar fee show karna
    document.getElementById('payAmount').innerText = calculatedFeeGlobal;

    // 4. Modal open karna
    var paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
    paymentModal.show();
}

// Jab user payment select kar ke 'Confirm & Release' dabaye
async function confirmPaymentAndRelease() {
    let selectedMethod = document.getElementById('paymentMethodSelect').value;

    if (!currentSlotIdForPayment) return;

    try {
        const token = localStorage.getItem("jwtToken");
        const targetUrl = `${API_BASE_URL}/release/${currentSlotIdForPayment}`;

        const response = await fetch(targetUrl, {
            method: "POST",
            headers: {
                Authorization: `Bearer ${token}`,
                "Content-Type": "application/json",
            },
        });

        if (response.ok) {
            const data = await response.json();

            //Show the alert notification with hour and payment method
            showDashAlert(
        `Slot released successfully! Total Hours: ${data.hours}, Collected Fee: Rs. ${data.fee}, Paid via ${selectedMethod}`,
        "success",
      );
            // Slots and revenue refresh
            if (typeof loadStatsAndSlots === "function") loadStatsAndSlots();
            if (typeof loadTotalRevenue === "function") loadTotalRevenue();
        } else {
            let errorText = "Release failed";
            try {
                const errData = await response.json();
                errorText = errData.message || errorText;
            } catch (e) {}
            showDashAlert(errorText, "danger");
        }
    } catch (err) {
        console.error("Release error details:", err);
        showDashAlert("Network error while releasing.", "danger");
    }

    // close the model
    var modalElement = document.getElementById('paymentModal');
    var modalInstance = bootstrap.Modal.getInstance(modalElement);
    modalInstance.hide();
}

// 6. TICKET PRINTING FUNCTION
// Print Ticket function with rates and hourly note
// Print Ticket function with safe date handling
function printTicket(slotNo, ticketNo, vehicleType, vehicleNo, checkInTime) {
    let parsedDate = (checkInTime && checkInTime !== "null" && checkInTime !== "undefined") 
        ? new Date(checkInTime) 
        : new Date();

    let formattedDate = !isNaN(parsedDate.getTime()) ? parsedDate.toLocaleString() : "N/A";

    let printWindow = window.open('', '_blank', 'height=600,width=400');

    printWindow.document.write(`
        <html>
            <head>
                <title>Parking Ticket</title>
                <style>
                    body {
                        font-family: 'Courier New', Courier, monospace;
                        text-align: center;
                        padding: 20px;
                        color: #000;
                    }
                    h2 { margin-bottom: 5px; }
                    .subtitle { font-size: 12px; margin-bottom: 15px; }
                    .divider { border-top: 1px dashed #000; margin: 10px 0; }
                    .details { text-align: left; margin: 15px 0; font-size: 14px; }
                    .details p { margin: 6px 0; }
                    .rates-box {
                        border: 1px dashed #000;
                        padding: 8px;
                        margin: 15px 0;
                        text-align: left;
                        font-size: 12px;
                    }
                    .rates-box h4 { margin: 0 0 5px 0; text-align: center; font-size: 13px; }
                    .footer-note {
                        font-size: 11px;
                        font-style: italic;
                        margin-top: 10px;
                        color: #333;
                    }
                </style>
            </head>
            <body>
                <h2>Vehicle Parking System</h2>
                <div class="subtitle">Official Parking Ticket</div>
                <div class="divider"></div>
                
                <div class="details">
                    <p><strong>Slot No:</strong> ${slotNo}</p>
                    <p><strong>Ticket No:</strong> ${ticketNo}</p>
                    <p><strong>Vehicle Type:</strong> ${vehicleType}</p>
                    <p><strong>Vehicle No:</strong> ${vehicleNo}</p>
                    <p><strong>Check-In Time:</strong> ${formattedDate}</p>
                </div>

                <div class="rates-box">
                    <h4>Parking Rates (Per Hour)</h4>
                    <p>• Bike: Rs. 50 / hr</p>
                    <p>• Car: Rs. 100 / hr</p>
                    <p>• Truck: Rs. 200 / hr</p>
                </div>

                <div class="divider"></div>
                <div class="footer-note">Note: Charges are calculated on an hourly basis.</div>
                <p style="margin-top: 15px; font-size: 12px;">Thank you for parking with us!</p>
            </body>
            </html>
    `);
    
    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 500);
}

// 7. ADMIN USER CREATION FORM
const createUserForm = document.getElementById("createUserForm");
if (createUserForm) {
  createUserForm.addEventListener("submit", async function (e) {
    e.preventDefault();

    const fullName = document.getElementById("newFullName").value;
    const email = document.getElementById("newEmail").value;
    const password = document.getElementById("newPassword").value;
    const role = document.getElementById("newRole").value;

    const alertBox = document.getElementById("createUserAlert");

    try {
      const response = await fetch(
        "http://localhost:5164/api/Auth/create-user",
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ fullName, email, password, role }),
        },
      );

      const data = await response.json();

      alertBox.classList.remove("d-none");
      if (response.ok) {
        alertBox.className = "alert alert-success";
        alertBox.textContent = data.message;
        createUserForm.reset();
      } else {
        alertBox.className = "alert alert-danger";
        alertBox.textContent = data.message || "Failed to create user.";
      }
    } catch (err) {
      alertBox.classList.remove("d-none");
      alertBox.className = "alert alert-danger";
      alertBox.textContent = "Network error while creating user.";
    }
  });
}

// 8. HELPER & UTILITY FUNCTIONS
function showDashAlert(message, type) {
  const alertBox = document.getElementById("dashAlert");
  if (alertBox) {
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove("d-none");
  }
}

function logout() {
  localStorage.removeItem("jwtToken");
  localStorage.removeItem("userName");
  localStorage.removeItem("userRole");
  window.location.href = "login.html";
}
