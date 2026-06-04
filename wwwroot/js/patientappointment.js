document.addEventListener("DOMContentLoaded", function () {
    const dateSortFilter = document.getElementById("dateSortFilter");
    const feeSortFilter = document.getElementById("feeSortFilter");
    const timeSlotFilter = document.getElementById("timeSlotFilter");
    const appointmentsTableBody = document.getElementById("appointmentsTableBody");
    const tableWrapper = document.getElementById("tableWrapper");
    const emptyStateContainer = document.getElementById("emptyStateContainer");

    // Hook AJAX filtering parameters
    dateSortFilter.addEventListener("change", fetchAppointments);
    feeSortFilter.addEventListener("change", fetchAppointments);
    timeSlotFilter.addEventListener("change", fetchAppointments);

    function fetchAppointments() {
        const sortBy = dateSortFilter.value;
        const feeSort = feeSortFilter.value;
        const timeSlot = timeSlotFilter.value;

        const url = `/Patient/MakeAppointment/GetUpcomingAppointments?sortBy=${sortBy}&feeSort=${feeSort}&timeSlot=${timeSlot}`;

        fetch(url)
            .then(response => {
                if (!response.ok) throw new Error("Could not fetch table fragment context matrix.");
                return response.text(); // Expecting raw HTML string from server instead of JSON
            })
            .then(htmlString => {
                appointmentsTableBody.innerHTML = htmlString;

                // Check if the server partial view flagged an empty state response
                if (document.getElementById("emptyStateFlag") || !htmlString.trim()) {
                    showEmptyState();
                } else {
                    showTable();
                }
            })
            .catch(err => {
                console.error("AJAX Error: ", err);
                showEmptyState();
            });
    }

    function showTable() {
        emptyStateContainer.classList.add("d-none");
        emptyStateContainer.innerHTML = "";
        tableWrapper.classList.remove("d-none");
    }

    function showEmptyState() {
        tableWrapper.classList.add("d-none");
        emptyStateContainer.innerHTML = `
            <div class="empty-state-container">
                <div class="empty-state-layout-box">
                    <div class="character-graphic-frame">
                        <img src="/images/Patient/Appointment/no_appointment.png" alt="No Appointments Found" class="character-vector-artwork" />
                    </div>
                    <div class="empty-message-details-block">
                        <h2 class="empty-state-main-title">No upcoming<br>appointments.</h2>
                        <p class="empty-state-subtitle">It looks like you don't have any<br>appointments scheduled yet.</p>
                        <a asp-area="Patient" asp-controller="DoctorInformation" asp-action="DoctorListing" class="btn-book-appointment-trigger">
                            Book an Appointment
                        </a>
                        
                    </div>
                </div>
            </div>
        `;
        emptyStateContainer.classList.remove("d-none");
    }

    // Trigger initial population on document ready execution tracking
    fetchAppointments();
});