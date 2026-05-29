// appointmentbooking.js
// Full, self-contained script for calendar, slots, modal, and confirmation.
// Assumes Razor injects `window.doctorId` (and optionally `window.doctorName`) BEFORE this file is included.

document.addEventListener("DOMContentLoaded", () => {
    // ----------------- Config / State -----------------
    const doctorId = (typeof window.doctorId !== "undefined") ? String(window.doctorId) : null;
    const doctorName = (typeof window.doctorName !== "undefined") ? String(window.doctorName) : null;

    if (!doctorId) {
        console.error("doctorId is missing. Ensure Razor injects window.doctorId before this script.");
        return;
    }

    const today = new Date();
    let currentYear = today.getFullYear();
    let currentMonth = today.getMonth(); // 0-based
    let displayedYear = currentYear;
    let displayedMonth = currentMonth;
    const maxMonth = new Date(today.getFullYear(), today.getMonth() + 6, 1);

    let selectedDate = null;
    let selectedSlot = null;

    // ----------------- Helpers -----------------
    function safeGet(id) { return document.getElementById(id); }

    function showToast(message, timeout = 6000) {
        const toast = document.createElement("div");
        toast.className = "toast-custom";
        toast.innerHTML = `<span>${message}</span><button aria-label="Close toast">×</button>`;
        const btn = toast.querySelector("button");
        btn?.addEventListener("click", () => toast.remove());
        document.body.appendChild(toast);
        setTimeout(() => { if (toast.parentElement) toast.remove(); }, timeout);
    }

    // ----------------- Calendar: load & render -----------------
    function loadCalendar(year, month) {
        fetch(`/Patient/MakeAppointment/GetAvailableDates?doctorId=${doctorId}&year=${year}&month=${month + 1}`, { credentials: "same-origin" })
            .then(res => {
                if (!res.ok) throw new Error(`Failed to load dates: ${res.status}`);
                return res.json();
            })
            .then(dates => renderCalendar(year, month, Array.isArray(dates) ? dates : []))
            .catch(err => {
                console.error(err);
                showToast("Unable to load calendar. Please try again later.");
                // render an empty calendar so UI doesn't break
                renderCalendar(year, month, []);
            });
    }

    // Always render 6 rows (6 * 7 = 42 cells) so calendar height is consistent
    function renderCalendar(year, month, availableDates) {
        const firstDayIndex = (new Date(year, month, 1).getDay() + 6) % 7; // Monday = 0
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        let html = "<table class='calendar-table' role='grid' aria-label='Calendar'><thead><tr>";
        ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"].forEach(d => html += `<th scope="col">${d}</th>`);
        html += "</tr></thead><tbody>";

        for (let row = 0; row < 6; row++) {
            html += "<tr>";
            for (let col = 0; col < 7; col++) {
                const globalIndex = row * 7 + col;
                const dayNumber = globalIndex - firstDayIndex + 1;

                if (dayNumber > 0 && dayNumber <= daysInMonth) {
                    const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(dayNumber).padStart(2, '0')}`;
                    const isAvailable = availableDates.includes(dateStr);
                    const cardClass = isAvailable ? "date-card available" : "date-card unavailable";
                    html += `<td><div tabindex="${isAvailable ? 0 : -1}" class="${cardClass}" data-date="${dateStr}">${dayNumber}</div></td>`;
                } else {
                    // placeholder to keep grid consistent
                    html += `<td><div class="date-card placeholder" aria-hidden="true"></div></td>`;
                }
            }
            html += "</tr>";
        }

        html += "</tbody></table>";

        const container = safeGet("calendarContainer");
        if (!container) {
            console.error("Missing #calendarContainer element in DOM.");
            return;
        }
        container.innerHTML = html;

        const monthLabel = safeGet("monthLabel");
        if (monthLabel) {
            monthLabel.textContent = `${new Date(year, month).toLocaleString('default', { month: 'long' })} ${year}`;
        }

        // Prev/Next button states
        const prevBtn = safeGet("prevMonth");
        if (prevBtn) prevBtn.disabled = (year === currentYear && month === currentMonth);

        const nextBtn = safeGet("nextMonth");
        const nextDate = new Date(year, month + 1, 1);
        if (nextBtn) nextBtn.disabled = (nextDate > maxMonth);

        // Attach click/keyboard handlers to available date-cards
        const availableCards = container.querySelectorAll(".date-card.available");
        availableCards.forEach(card => {
            card.addEventListener("click", () => {
                selectDateCard(card);
            });
            card.addEventListener("keydown", (e) => {
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    selectDateCard(card);
                }
            });
        });

        // Auto-select first available date if none selected
        if (!selectedDate) {
            const first = availableCards[0];
            if (first) {
                selectDateCard(first);
            } else {
                // clear slots if no available dates
                renderSlots("morningSlots", []);
                renderSlots("afternoonSlots", []);
                renderSlots("eveningSlots", []);
            }
        }
    }

    function selectDateCard(card) {
        const container = safeGet("calendarContainer");
        if (!container) return;
        container.querySelectorAll(".date-card.selected").forEach(c => c.classList.remove("selected"));
        card.classList.add("selected");
        selectedDate = card.dataset.date;
        loadSlots(selectedDate);
    }

    // ----------------- Slots: load & render -----------------
    function loadSlots(date) {
        fetch(`/Patient/MakeAppointment/GetAvailableSlots?doctorId=${doctorId}&date=${date}`, { credentials: "same-origin" })
            .then(res => {
                if (!res.ok) throw new Error(`Failed to load slots: ${res.status}`);
                return res.json();
            })
            .then(slots => {
                renderSlots("morningSlots", slots?.MorningSlots ?? []);
                renderSlots("afternoonSlots", slots?.AfternoonSlots ?? []);
                renderSlots("eveningSlots", slots?.EveningSlots ?? []);
            })
            .catch(err => {
                console.error(err);
                showToast("Unable to load time slots. Please try again later.");
                renderSlots("morningSlots", []);
                renderSlots("afternoonSlots", []);
                renderSlots("eveningSlots", []);
            });
    }

    function renderSlots(containerId, slots) {
        const container = safeGet(containerId);
        if (!container) return;
        container.innerHTML = "";

        if (!Array.isArray(slots) || slots.length === 0) {
            // show subtle empty state
            const empty = document.createElement("div");
            empty.style.padding = "10px";
            empty.style.color = "rgba(0,63,63,0.6)";
            empty.textContent = "No slots";
            container.appendChild(empty);
            return;
        }

        slots.forEach(slot => {
            const btn = document.createElement("button");
            btn.className = "slot-btn";
            btn.type = "button";
            btn.textContent = slot;
            btn.addEventListener("click", () => {
                // deselect others
                document.querySelectorAll(".slots-section .slot-btn").forEach(b => b.classList.remove("selected"));
                btn.classList.add("selected");
                selectedSlot = slot;
            });
            container.appendChild(btn);
        });
    }

    // ----------------- Confirm appointment -----------------
    const confirmBtn = safeGet("confirmBtn");
    confirmBtn?.addEventListener("click", () => {
        const paymentEl = document.querySelector("input[name='payment']:checked");
        const payment = paymentEl ? paymentEl.value : null;

        if (!selectedDate || !selectedSlot) {
            showToast("Please select a date and time slot.");
            return;
        }

        // Optional: show a confirmation modal before POST
        const detailsHtml = `
      <p><strong>Doctor</strong>: ${doctorName ?? "Selected Doctor"}</p>
      <p><strong>Date</strong>: ${selectedDate}</p>
      <p><strong>Time</strong>: ${selectedSlot}</p>
      <p style="margin-top:12px">Proceed to confirm this appointment.</p>
    `;
        showModal("Confirm Appointment", detailsHtml, [
            { text: "Close", className: "modal-close-btn", action: closeModal },
            {
                text: "Confirm", className: "btn-teal", action: () => {
                    // disable confirm to prevent double submits
                    confirmBtn.disabled = true;
                    fetch("/Patient/MakeAppointment/Confirm", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({
                            DoctorId: doctorId,
                            SelectedDate: selectedDate,
                            SelectedSlot: selectedSlot,
                            PaymentMethod: payment
                        }),
                        credentials: "same-origin"
                    })
                        .then(res => {
                            confirmBtn.disabled = false;
                            if (res.ok) {
                                closeModal();
                                window.location.href = "/Patient/MakeAppointment/PatientAppointment";
                            } else {
                                closeModal();
                                showToast("Error confirming appointment.");
                            }
                        })
                        .catch(err => {
                            confirmBtn.disabled = false;
                            console.error(err);
                            closeModal();
                            showToast("Network error while confirming appointment.");
                        });
                }
            }
        ]);
    });

    // ----------------- Month navigation -----------------
    safeGet("prevMonth")?.addEventListener("click", () => {
        if (!(displayedYear === currentYear && displayedMonth === currentMonth)) {
            displayedMonth--;
            if (displayedMonth < 0) { displayedMonth = 11; displayedYear--; }
            selectedDate = null;
            loadCalendar(displayedYear, displayedMonth);
        }
    });

    safeGet("nextMonth")?.addEventListener("click", () => {
        const nextDate = new Date(displayedYear, displayedMonth + 1, 1);
        if (nextDate <= maxMonth) {
            displayedMonth++;
            if (displayedMonth > 11) { displayedMonth = 0; displayedYear++; }
            selectedDate = null;
            loadCalendar(displayedYear, displayedMonth);
        }
    });

    // ----------------- Modal helpers (overlay + frosted background) -----------------
    function showModal(title, htmlContent, footerButtons = null) {
        // Create overlay if missing
        let overlay = document.querySelector(".modal-overlay");
        if (!overlay) {
            overlay = document.createElement("div");
            overlay.className = "modal-overlay";
            overlay.innerHTML = `
        <div class="modal-card" role="dialog" aria-modal="true">
          <div class="modal-header"></div>
          <div class="modal-body"></div>
          <div class="modal-footer"></div>
        </div>
      `;
            document.body.appendChild(overlay);
        }

        const header = overlay.querySelector(".modal-header");
        const body = overlay.querySelector(".modal-body");
        const footer = overlay.querySelector(".modal-footer");

        header.textContent = title || "Details";
        body.innerHTML = htmlContent || "";
        footer.innerHTML = "";

        // Add footer buttons (array of {text, className, action})
        if (Array.isArray(footerButtons) && footerButtons.length) {
            footerButtons.forEach(btnCfg => {
                const btn = document.createElement("button");
                btn.textContent = btnCfg.text || "Close";
                btn.className = btnCfg.className || "modal-close-btn";
                btn.addEventListener("click", () => {
                    if (typeof btnCfg.action === "function") btnCfg.action();
                });
                footer.appendChild(btn);
            });
        } else {
            // default close button
            const closeBtn = document.createElement("button");
            closeBtn.className = "modal-close-btn";
            closeBtn.textContent = "Close";
            closeBtn.addEventListener("click", closeModal);
            footer.appendChild(closeBtn);
        }

        // show overlay
        requestAnimationFrame(() => overlay.classList.add("open"));
        // prevent background scroll
        document.documentElement.style.overflow = "hidden";

        // close when clicking outside modal-card
        overlay.addEventListener("click", overlayClickHandler);
    }

    function overlayClickHandler(e) {
        if (e.target && e.target.classList && e.target.classList.contains("modal-overlay")) {
            closeModal();
        }
    }

    function closeModal() {
        const overlay = document.querySelector(".modal-overlay");
        if (!overlay) return;
        overlay.classList.remove("open");
        overlay.removeEventListener("click", overlayClickHandler);
        // allow animation to finish then remove
        setTimeout(() => {
            overlay.remove();
            document.documentElement.style.overflow = "";
        }, 260);
    }

    // ----------------- Initial load -----------------
    loadCalendar(displayedYear, displayedMonth);

    // Defensive prefetch: ensure first available date loads slots even if race occurs
    fetch(`/Patient/MakeAppointment/GetAvailableDates?doctorId=${doctorId}&year=${displayedYear}&month=${displayedMonth + 1}`, { credentials: "same-origin" })
        .then(res => { if (!res.ok) throw new Error("prefetch failed"); return res.json(); })
        .then(dates => {
            if (Array.isArray(dates) && dates.length > 0 && !selectedDate) {
                selectedDate = dates[0];
                loadSlots(selectedDate);
            }
        })
        .catch(() => { /* ignore prefetch errors */ });

    // Accessibility: keyboard navigation for month buttons
    const prev = safeGet("prevMonth"), next = safeGet("nextMonth");
    [prev, next].forEach(btn => {
        if (!btn) return;
        btn.addEventListener("keydown", (e) => {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                btn.click();
            }
        });
    });
});
