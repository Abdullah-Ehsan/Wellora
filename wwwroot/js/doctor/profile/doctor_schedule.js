document.addEventListener("DOMContentLoaded", function () {

    const scheduleRows = document.querySelectorAll(
        ".doctor-schedule-table tbody tr"
    );

    scheduleRows.forEach(function (row) {

        const statusSelect = row.querySelector(".schedule-status");

        if (!statusSelect) {
            return;
        }

        // Initial state
        updateRowState(row);

        // When On / Off / - changes
        statusSelect.addEventListener("change", function () {
            updateRowState(row);
        });
    });


    function updateRowState(row) {

        const statusSelect = row.querySelector(".schedule-status");

        if (!statusSelect) {
            return;
        }

        const status = statusSelect.value;

        const inputs = row.querySelectorAll(".schedule-input");

        /*
         * ON
         * ---------------------------------------------
         * Row is editable.
         *
         * Apply defaults if values are empty.
         */
        if (status === "On") {

            inputs.forEach(function (input) {

                input.disabled = false;

            });

            setDefaultValues(row);

            row.classList.remove("schedule-row-off");
            row.classList.add("schedule-row-on");

            return;
        }


        /*
         * OFF
         * ---------------------------------------------
         * Row is disabled.
         *
         * The C# service will NOT save this day.
         */
        if (status === "Off") {

            inputs.forEach(function (input) {

                input.disabled = true;

            });

            row.classList.remove("schedule-row-on");
            row.classList.add("schedule-row-off");

            return;
        }


        /*
         * -
         * ---------------------------------------------
         * Row is editable according to your rule,
         * but the C# service will NOT save it.
         */
        inputs.forEach(function (input) {

            input.disabled = false;

        });

        row.classList.remove("schedule-row-on");
        row.classList.remove("schedule-row-off");
    }


    function setDefaultValues(row) {

        /*
         * Appointment Duration
         *
         * Default = 30 minutes
         */
        const duration =
            row.querySelector(
                '[name$=".AppointmentDurationMin"]'
            );

        if (duration && !duration.value) {

            duration.value = "30";

        }


        /*
         * Maximum Patients
         *
         * Default = 1
         */
        const maxPatients =
            row.querySelector(
                '[name$=".MaxPatientsPerDay"]'
            );

        if (maxPatients && !maxPatients.value) {

            maxPatients.value = "1";

        }


        /*
         * Buffer Time
         *
         * Default = 0 minutes
         */
        const buffer =
            row.querySelector(
                '[name$=".BufferTimeMin"]'
            );

        if (buffer && !buffer.value) {

            buffer.value = "0";

        }
    }


    /*
     * ---------------------------------------------------------
     * BREAK UI
     * ---------------------------------------------------------
     *
     * Breaks are only useful when the shift is at least
     * 4 hours.
     *
     * The service performs the actual validation.
     */
    scheduleRows.forEach(function (row) {

        const startTime =
            row.querySelector(
                '[name$=".StartTime"]'
            );

        const endTime =
            row.querySelector(
                '[name$=".EndTime"]'
            );

        const breakStart =
            row.querySelector(
                '[name$=".BreakStart"]'
            );

        const breakEnd =
            row.querySelector(
                '[name$=".BreakEnd"]'
            );

        if (!startTime || !endTime) {
            return;
        }

        function updateBreakState() {

            const status =
                row.querySelector(".schedule-status")?.value;

            /*
             * If the row isn't On, break controls don't matter.
             */
            if (status !== "On") {
                return;
            }

            /*
             * No start/end time selected yet.
             */
            if (!startTime.value || !endTime.value) {
                return;
            }

            const start =
                timeToMinutes(startTime.value);

            const end =
                timeToMinutes(endTime.value);

            if (start === null || end === null) {
                return;
            }

            let shiftMinutes = end - start;

            /*
             * Same-day schedule only.
             *
             * Overnight shifts are rejected by the service.
             */
            if (shiftMinutes < 0) {
                shiftMinutes = 0;
            }

            /*
             * Breaks are allowed only for shifts >= 4 hours.
             */
            const breakAllowed =
                shiftMinutes >= 240;

            if (!breakAllowed) {

                if (breakStart) {
                    breakStart.value = "";
                    breakStart.disabled = true;
                }

                if (breakEnd) {
                    breakEnd.value = "";
                    breakEnd.disabled = true;
                }

                return;
            }

            /*
             * Shift is long enough for a break.
             */
            if (breakStart) {
                breakStart.disabled = false;
            }

            if (breakEnd) {
                breakEnd.disabled = false;
            }
        }


        startTime.addEventListener(
            "change",
            updateBreakState
        );

        endTime.addEventListener(
            "change",
            updateBreakState
        );

        updateBreakState();
    });


    /*
     * ---------------------------------------------------------
     * TIME CONVERSION
     * ---------------------------------------------------------
     */
    function timeToMinutes(value) {

        if (!value) {
            return null;
        }

        /*
         * Handles TimeSpan values such as:
         *
         * 09:00
         * 09:00:00
         */
        const parts = value.split(":");

        if (parts.length < 2) {
            return null;
        }

        const hours = parseInt(parts[0], 10);
        const minutes = parseInt(parts[1], 10);

        if (
            Number.isNaN(hours) ||
            Number.isNaN(minutes)
        ) {
            return null;
        }

        return (hours * 60) + minutes;
    }

});
