document.addEventListener("DOMContentLoaded", function () {

    // -----------------------------
    // STATE
    // -----------------------------
    let selectedDate = null;
    let selectedSlot = null;

    const prevBtn = document.getElementById("prevMonth");
    const nextBtn = document.getElementById("nextMonth");
    const monthLabel = document.getElementById("monthLabel");

    let year = window.currentYear;
    let month = window.currentMonth; // 0-based

    // -----------------------------
    // TOAST
    // -----------------------------
    function showToast(message, timeout = 3000) {
        const existing = document.querySelector(".toast-overlay");
        if (existing) existing.remove();

        const overlay = document.createElement("div");
        overlay.className = "toast-overlay";

        overlay.innerHTML = `
            <div class="toast-custom">
                <div class="toast-header">Notification</div>
                <div class="toast-body">${message}</div>
                <div class="toast-footer">
                    <button class="btn btn-teal">Close</button>
                </div>
            </div>
        `;

        document.body.appendChild(overlay);

        overlay.querySelector("button").addEventListener("click", () => overlay.remove());
        setTimeout(() => overlay.remove(), timeout);
    }

    window.showToast = showToast;

    // -----------------------------
    // DATE CLICK (EVENT DELEGATION)
    // -----------------------------
    document.addEventListener("click", function (e) {

        const dateEl = e.target.closest(".date-card.available");

        if (dateEl) {

            document.querySelectorAll(".date-card")
                .forEach(x => x.classList.remove("selected"));

            dateEl.classList.add("selected");

            selectedDate = dateEl.dataset.date;
            document.getElementById("SelectedDate").value = selectedDate;

            showToast("Selected " + selectedDate);

            loadSlots(selectedDate);
        }

        // -----------------------------
        // SLOT CLICK
        // -----------------------------
        const slotEl = e.target.closest(".slot-btn.available");

        if (slotEl) {

            document.querySelectorAll(".slot-btn")
                .forEach(x => x.classList.remove("selected"));

            slotEl.classList.add("selected");

            selectedSlot = slotEl.dataset.slot;
            document.getElementById("SelectedSlot").value = selectedSlot;

            showToast("Selected slot " + selectedSlot);
        }
    });

    // -----------------------------
    // LOAD SLOTS (AJAX)
    // -----------------------------
    // -----------------------------
    // LOAD SLOTS (AJAX Optimization)
    // -----------------------------
    function loadSlots(date) {

        fetch(`/Patient/MakeAppointment/GetAvailableSlots?doctorId=${window.doctorId}&date=${date}`)
            .then(response => {
                // Intercept errors cleanly before attempting JSON interpretation
                if (!response.ok) {
                    throw new Error(`Network response error: Status ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log("Database Slots Received:", data);
                updateSlots(data);
            })
            .catch(err => {
                console.error("AJAX Processing Error: ", err);
                showToast("Failed to communicate with database scheduler.");
            });
    }

    // -----------------------------
    // UPDATE SLOTS UI
    // -----------------------------
    function updateSlots(data) {

        render(".morning-slots", data.morningSlots);
        render(".afternoon-slots", data.afternoonSlots);
        render(".evening-slots", data.eveningSlots);

        selectedSlot = null;
        document.getElementById("SelectedSlot").value = "";
    }

    function render(containerSelector, slots) {

        const container = document.querySelector(containerSelector);
        if (!container) return;

        container.innerHTML = "";

        for (let i = 0; i < 10; i++) {

            const slot = slots[i];

            if (!slot || !slot.time) {
                container.innerHTML += `
                    <button type="button" class="slot-btn placeholder-slot" disabled>—</button>
                `;
            }
            else {
                container.innerHTML += `
                    <button type="button"
                            class="slot-btn ${slot.isAvailable ? "available" : "unavailable"}"
                            data-slot="${slot.time}"
                            ${slot.isAvailable ? "" : "disabled"}>
                        ${slot.time}
                    </button>
                `;
            }
        }
    }

    // -----------------------------
    // MONTH NAVIGATION
    // -----------------------------
    function renderMonthLabel() {
        const d = new Date(year, month, 1);
        monthLabel.textContent =
            d.toLocaleString("default", { month: "long", year: "numeric" });
    }

    function changeMonth(offset) {

        month += offset;

        if (month < 0) {
            month = 11;
            year--;
        }

        if (month > 11) {
            month = 0;
            year++;
        }

        const max = new Date(window.currentYear, window.currentMonth + 6, 1);
        const current = new Date(year, month, 1);

        if (current > max) {
            showToast("Only 6 months ahead allowed");
            month -= offset;
            return;
        }

        // reload page with new month (server-driven calendar)
        window.location.href =
            `/Patient/MakeAppointment/AppointmentBooking?doctorId=${window.doctorId}&year=${year}&month=${month + 1}`;
    }

    prevBtn?.addEventListener("click", (e) => {
        e.preventDefault();
        changeMonth(-1);
    });

    nextBtn?.addEventListener("click", (e) => {
        e.preventDefault();
        changeMonth(1);
    });

    renderMonthLabel();

});