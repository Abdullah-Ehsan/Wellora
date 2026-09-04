document.addEventListener("DOMContentLoaded", function () {

    const dateSortFilter = document.getElementById("dateSortFilter");
    const feeSortFilter = document.getElementById("feeSortFilter");
    const timeSlotFilter = document.getElementById("timeSlotFilter");

    const appointmentsTableBody =
        document.getElementById("appointmentsTableBody");

    const tableWrapper =
        document.getElementById("tableWrapper");

    const emptyStateContainer =
        document.getElementById("emptyStateContainer");


    // =========================================================
    // FILTER EVENTS
    // =========================================================

    dateSortFilter.addEventListener("change", function () {
        fetchAppointments(1);
    });

    feeSortFilter.addEventListener("change", function () {
        fetchAppointments(1);
    });

    timeSlotFilter.addEventListener("change", function () {
        fetchAppointments(1);
    });


    // =========================================================
    // FETCH APPOINTMENTS
    // =========================================================

    function fetchAppointments(page = 1) {

        const sortBy = dateSortFilter.value;
        const feeSort = feeSortFilter.value;
        const timeSlot = timeSlotFilter.value;


        const params = new URLSearchParams({
            sortBy: sortBy,
            feeSort: feeSort,
            timeSlot: timeSlot,
            page: page
        });


        const url =
            `/Patient/MakeAppointment/GetUpcomingAppointments?${params.toString()}`;


        // Optional loading state
        appointmentsTableBody.classList.add("is-loading");


        fetch(url)
            .then(response => {

                if (!response.ok) {
                    throw new Error(
                        "Could not fetch appointments."
                    );
                }

                return response.text();
            })

            .then(htmlString => {

                appointmentsTableBody.innerHTML = htmlString;

                appointmentsTableBody.classList.remove("is-loading");


                // =================================================
                // EMPTY STATE
                // =================================================

                if (
                    document.getElementById("emptyStateFlag") ||
                    !htmlString.trim()
                ) {
                    showEmptyState();
                }
                else {
                    showTable();
                    attachPaginationEvents();
                }

            })

            .catch(error => {

                console.error("AJAX Error:", error);

                appointmentsTableBody.classList.remove("is-loading");

                showEmptyState();
            });
    }


    // =========================================================
    // PAGINATION EVENTS
    // =========================================================

    function attachPaginationEvents() {

        const paginationButtons =
            document.querySelectorAll(
                ".appointments-pagination [data-page]"
            );


        paginationButtons.forEach(button => {

            button.addEventListener("click", function () {

                if (button.disabled) {
                    return;
                }


                const page =
                    parseInt(button.dataset.page);


                if (!page || page < 1) {
                    return;
                }


                fetchAppointments(page);


                // Scroll smoothly to the top of the table
                tableWrapper.scrollIntoView({
                    behavior: "smooth",
                    block: "start"
                });

            });

        });

    }


    // =========================================================
    // SHOW TABLE
    // =========================================================

    function showTable() {

        emptyStateContainer.classList.add("d-none");

        emptyStateContainer.innerHTML = "";

        tableWrapper.classList.remove("d-none");
    }


    // =========================================================
    // SHOW EMPTY STATE
    // =========================================================

    function showEmptyState() {

        tableWrapper.classList.add("d-none");


        emptyStateContainer.innerHTML = `
            <div class="empty-state-container">

                <div class="empty-state-layout-box">

                    <div class="character-graphic-frame">

                        <img
                            src="/images/Patient/Appointment/no_appointment.png"
                            alt="No Appointments Found"
                            class="character-vector-artwork" />

                    </div>


                    <div class="empty-message-details-block">

                        <h2 class="empty-state-main-title">
                            No upcoming<br>
                            appointments.
                        </h2>


                        <p class="empty-state-subtitle">
                            It looks like you don't have any<br>
                            appointments scheduled yet.
                        </p>


                        <a
                            href="/Patient/DoctorInformation/DoctorListing"
                            class="btn-book-appointment-trigger">

                            Book an Appointment

                        </a>

                    </div>

                </div>

            </div>
        `;


        emptyStateContainer.classList.remove("d-none");
    }


    // =========================================================
    // INITIAL LOAD
    // =========================================================

    fetchAppointments(1);

});
