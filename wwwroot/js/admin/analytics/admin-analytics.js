document.addEventListener("DOMContentLoaded", function () {
    if (typeof Chart === "undefined") {
        console.error("Admin Analytics: Chart.js is not loaded.");
        return;
    }

    if (typeof analyticsData === "undefined") {
        console.error("Admin Analytics: analyticsData was not found.");
        return;
    }


    // =========================================================
    // WELLORA COLOUR PALETTE
    // =========================================================

    const colors = {
        primary: "#2f80ed",
        secondary: "#56ccf2",
        success: "#27ae60",
        warning: "#f2c94c",
        danger: "#eb5757",
        purple: "#9b51e0",
        teal: "#00a6a6",
        orange: "#f2994a",
        pink: "#eb5aa5",
        navy: "#1f3c88",
        gray: "#828282"
    };

    const palette = [
        colors.primary,
        colors.secondary,
        colors.success,
        colors.warning,
        colors.danger,
        colors.purple,
        colors.teal,
        colors.orange,
        colors.pink,
        colors.navy
    ];


    // =========================================================
    // GLOBAL CHART DEFAULTS
    // =========================================================

    Chart.defaults.font.family =
        "Inter, -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif";

    Chart.defaults.color = "#667085";

    Chart.defaults.plugins.legend.labels.usePointStyle = true;

    Chart.defaults.plugins.tooltip.backgroundColor = "#1f2937";
    Chart.defaults.plugins.tooltip.titleColor = "#ffffff";
    Chart.defaults.plugins.tooltip.bodyColor = "#ffffff";
    Chart.defaults.plugins.tooltip.padding = 12;
    Chart.defaults.plugins.tooltip.cornerRadius = 8;


    // =========================================================
    // HELPERS
    // =========================================================

    function getCanvas(id) {
        const canvas = document.getElementById(id);

        if (!canvas) {
            console.warn(`Admin Analytics: Canvas #${id} was not found.`);
            return null;
        }

        return canvas;
    }


    function createChart(id, config) {

        const canvas = getCanvas(id);

        if (!canvas) {
            return null;
        }

        return new Chart(canvas, config);
    }


    function labels(data, property) {
        return (data || []).map(item => item[property]);
    }


    function values(data, property) {
        return (data || []).map(item => item[property]);
    }


    function colorsFor(count) {
        return Array.from(
            { length: count },
            (_, index) => palette[index % palette.length]
        );
    }


    function currencyTooltip() {
        return {
            callbacks: {
                label: function (context) {

                    const value = Number(context.raw || 0);

                    return `${context.dataset.label || "Revenue"}: ${value.toLocaleString(
                        undefined,
                        {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        }
                    )}`;
                }
            }
        };
    }


    function currencyValue(value) {
        return Number(value || 0).toLocaleString(
            undefined,
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        );
    }


    function createGradient(canvas, color) {

        const ctx = canvas.getContext("2d");

        const gradient = ctx.createLinearGradient(
            0,
            0,
            0,
            350
        );

        gradient.addColorStop(0, `${color}80`);
        gradient.addColorStop(1, `${color}05`);

        return gradient;
    }


    // =========================================================
    // FINANCIAL ANALYTICS
    // =========================================================


    // Revenue Over Time - LINE

    const revenueCanvas = getCanvas("revenueOverTimeChart");

    if (revenueCanvas) {

        const data = analyticsData.financial.revenueOverTime;

        createChart("revenueOverTimeChart", {

            type: "line",

            data: {
                labels: labels(data, "date"),

                datasets: [
                    {
                        label: "Revenue",

                        data: values(data, "amount"),

                        borderColor: colors.primary,

                        backgroundColor:
                            createGradient(
                                revenueCanvas,
                                colors.primary
                            ),

                        fill: true,

                        tension: 0.35,

                        pointRadius: 3,

                        pointHoverRadius: 6,

                        pointBackgroundColor: colors.primary
                    }
                ]
            },

            options: {
                responsive: true,

                maintainAspectRatio: false,

                interaction: {
                    intersect: false,
                    mode: "index"
                },

                plugins: {
                    legend: {
                        display: false
                    },

                    tooltip: currencyTooltip()
                },

                scales: {
                    y: {
                        beginAtZero: true,

                        ticks: {
                            callback: value => currencyValue(value)
                        },

                        grid: {
                            color: "rgba(0,0,0,0.06)"
                        }
                    },

                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }


    // Payment Status - DOUGHNUT

    createChart("paymentStatusChart", {

        type: "doughnut",

        data: {
            labels: labels(
                analyticsData.financial.paymentStatus,
                "status"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.financial.paymentStatus,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.financial.paymentStatus.length
                    ),

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            cutout: "62%",

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // Payment Methods - BAR

    createChart("paymentMethodsChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.financial.paymentMethods,
                "paymentMethod"
            ),

            datasets: [
                {
                    label: "Transactions",

                    data: values(
                        analyticsData.financial.paymentMethods,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.financial.paymentMethods.length
                    ),

                    borderRadius: 8,

                    maxBarThickness: 55
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Revenue By Payment Method - DOUGHNUT

    createChart("revenueByPaymentMethodChart", {

        type: "doughnut",

        data: {
            labels: labels(
                analyticsData.financial.revenueByPaymentMethod,
                "paymentMethod"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.financial.revenueByPaymentMethod,
                        "amount"
                    ),

                    backgroundColor: [
                        colors.success,
                        colors.primary
                    ],

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            cutout: "58%",

            plugins: {
                legend: {
                    position: "bottom"
                },

                tooltip: currencyTooltip()
            }
        }
    });


    // Transaction Outcomes - POLAR AREA

    createChart("transactionOutcomesChart", {

        type: "polarArea",

        data: {
            labels: labels(
                analyticsData.financial.transactionOutcomes,
                "status"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.financial.transactionOutcomes,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.financial.transactionOutcomes.length
                    ),

                    borderWidth: 1
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            scales: {
                r: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                }
            },

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // =========================================================
    // APPOINTMENT ANALYTICS
    // =========================================================


    // Appointments Over Time - LINE

    const appointmentCanvas =
        getCanvas("appointmentsOverTimeChart");

    if (appointmentCanvas) {

        const data =
            analyticsData.appointments.overTime;

        createChart("appointmentsOverTimeChart", {

            type: "line",

            data: {
                labels: labels(data, "date"),

                datasets: [
                    {
                        label: "Appointments",

                        data: values(data, "count"),

                        borderColor: colors.teal,

                        backgroundColor:
                            createGradient(
                                appointmentCanvas,
                                colors.teal
                            ),

                        fill: true,

                        tension: 0.35,

                        pointRadius: 3,

                        pointHoverRadius: 6
                    }
                ]
            },

            options: {
                responsive: true,

                maintainAspectRatio: false,

                interaction: {
                    intersect: false,
                    mode: "index"
                },

                plugins: {
                    legend: {
                        display: false
                    }
                },

                scales: {
                    y: {
                        beginAtZero: true,

                        ticks: {
                            precision: 0
                        }
                    },

                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }


    // Appointment Status - DOUGHNUT

    createChart("appointmentStatusChart", {

        type: "doughnut",

        data: {
            labels: labels(
                analyticsData.appointments.status,
                "status"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.appointments.status,
                        "count"
                    ),

                    backgroundColor: [
                        colors.primary,
                        colors.success,
                        colors.danger
                    ],

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            cutout: "62%",

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // Appointment Payment Status - PIE

    createChart("appointmentPaymentStatusChart", {

        type: "pie",

        data: {
            labels: labels(
                analyticsData.appointments.paymentStatus,
                "status"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.appointments.paymentStatus,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.appointments.paymentStatus.length
                    ),

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // Appointments By Weekday - BAR

    const weekdayOrder = [
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday"
    ];

    const weekdayData =
        analyticsData.appointments.byWeekday;

    const orderedWeekdayData =
        weekdayOrder.map(day => {

            const item =
                weekdayData.find(
                    x => x.day === day
                );

            return item ? item.count : 0;
        });

    createChart("appointmentsByWeekdayChart", {

        type: "bar",

        data: {
            labels: weekdayOrder,

            datasets: [
                {
                    label: "Appointments",

                    data: orderedWeekdayData,

                    backgroundColor: colors.primary,

                    borderRadius: 8,

                    maxBarThickness: 45
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Consultation Revenue - LINE

    createChart("consultationRevenueChart", {

        type: "line",

        data: {
            labels: labels(
                analyticsData.appointments.consultationRevenue,
                "date"
            ),

            datasets: [
                {
                    label: "Consultation Revenue",

                    data: values(
                        analyticsData.appointments.consultationRevenue,
                        "amount"
                    ),

                    borderColor: colors.purple,

                    backgroundColor: "rgba(155, 81, 224, 0.12)",

                    fill: true,

                    tension: 0.35,

                    pointRadius: 3
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                tooltip: currencyTooltip()
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        callback: value => currencyValue(value)
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // =========================================================
    // PATIENT ANALYTICS
    // =========================================================


    // Top Visited Patients - HORIZONTAL BAR

    createChart("topVisitedPatientsChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.patients.topVisited,
                "patientName"
            ),

            datasets: [
                {
                    label: "Visits",

                    data: values(
                        analyticsData.patients.topVisited,
                        "visitCount"
                    ),

                    backgroundColor: colors.teal,

                    borderRadius: 8
                }
            ]
        },

        options: {
            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                x: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Top Spending Patients - HORIZONTAL BAR

    createChart("topSpendingPatientsChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.patients.topSpending,
                "patientName"
            ),

            datasets: [
                {
                    label: "Total Spent",

                    data: values(
                        analyticsData.patients.topSpending,
                        "totalSpent"
                    ),

                    backgroundColor: colors.success,

                    borderRadius: 8
                }
            ]
        },

        options: {
            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                },

                tooltip: currencyTooltip()
            },

            scales: {
                x: {
                    beginAtZero: true,

                    ticks: {
                        callback: value => currencyValue(value)
                    }
                },

                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Patient Gender - DOUGHNUT

    createChart("patientGenderChart", {

        type: "doughnut",

        data: {
            labels: labels(
                analyticsData.patients.gender,
                "status"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.patients.gender,
                        "count"
                    ),

                    backgroundColor: [
                        colors.primary,
                        colors.pink,
                        colors.purple,
                        colors.gray
                    ],

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            cutout: "60%",

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // Patient Age Groups - BAR

    createChart("patientAgeGroupsChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.patients.ageGroups,
                "ageGroup"
            ),

            datasets: [
                {
                    label: "Patients",

                    data: values(
                        analyticsData.patients.ageGroups,
                        "count"
                    ),

                    backgroundColor: colors.purple,

                    borderRadius: 8
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Preferred Languages - POLAR AREA

    createChart("preferredLanguagesChart", {

        type: "polarArea",

        data: {
            labels: labels(
                analyticsData.patients.languages,
                "category"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.patients.languages,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.patients.languages.length
                    )
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // Patients By Primary Doctor - HORIZONTAL BAR

    createChart("patientsByPrimaryDoctorChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.patients.primaryDoctors,
                "doctorName"
            ),

            datasets: [
                {
                    label: "Patients",

                    data: values(
                        analyticsData.patients.primaryDoctors,
                        "patientCount"
                    ),

                    backgroundColor: colors.orange,

                    borderRadius: 8
                }
            ]
        },

        options: {
            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                x: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // =========================================================
    // DOCTOR ANALYTICS
    // =========================================================


    // Specializations - DOUGHNUT

    createChart("doctorSpecializationsChart", {

        type: "doughnut",

        data: {
            labels: labels(
                analyticsData.doctors.specializations,
                "category"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.doctors.specializations,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.doctors.specializations.length
                    ),

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            cutout: "55%",

            plugins: {
                legend: {
                    position: "right"
                }
            }
        }
    });


    // Sub Specialties - BAR

    createChart("doctorSubSpecialtiesChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.subSpecialties,
                "category"
            ),

            datasets: [
                {
                    label: "Doctors",

                    data: values(
                        analyticsData.doctors.subSpecialties,
                        "count"
                    ),

                    backgroundColor: colors.secondary,

                    borderRadius: 8
                }
            ]
        },

        options: {
            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                x: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Doctor Gender - PIE

    createChart("doctorGenderChart", {

        type: "pie",

        data: {
            labels: labels(
                analyticsData.doctors.gender,
                "status"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.doctors.gender,
                        "count"
                    ),

                    backgroundColor: [
                        colors.primary,
                        colors.pink,
                        colors.purple,
                        colors.gray
                    ],

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // Experience - BAR

    createChart("doctorExperienceChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.experience,
                "range"
            ),

            datasets: [
                {
                    label: "Doctors",

                    data: values(
                        analyticsData.doctors.experience,
                        "count"
                    ),

                    backgroundColor: colors.navy,

                    borderRadius: 8
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Consultation Fees - BAR

    createChart("doctorFeeChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.fees,
                "range"
            ),

            datasets: [
                {
                    label: "Doctors",

                    data: values(
                        analyticsData.doctors.fees,
                        "count"
                    ),

                    backgroundColor: colors.warning,

                    borderRadius: 8
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Telemedicine - DOUGHNUT

    createChart("telemedicineChart", {

        type: "doughnut",

        data: {
            labels: labels(
                analyticsData.doctors.telemedicine,
                "category"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.doctors.telemedicine,
                        "count"
                    ),

                    backgroundColor: [
                        colors.success,
                        colors.gray
                    ],

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            cutout: "62%",

            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });


    // Countries - BAR

    createChart("doctorCountriesChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.countries,
                "category"
            ),

            datasets: [
                {
                    label: "Doctors",

                    data: values(
                        analyticsData.doctors.countries,
                        "count"
                    ),

                    backgroundColor: colors.teal,

                    borderRadius: 8
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Primary Medical Degrees - BAR

    createChart("primaryMedicalDegreesChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.primaryMedicalDegrees,
                "category"
            ),

            datasets: [
                {
                    label: "Doctors",

                    data: values(
                        analyticsData.doctors.primaryMedicalDegrees,
                        "count"
                    ),

                    backgroundColor: colors.primary,

                    borderRadius: 8
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Postgraduate Degrees - BAR

    createChart("postgraduateDegreesChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.postgraduateDegrees,
                "category"
            ),

            datasets: [
                {
                    label: "Doctors",

                    data: values(
                        analyticsData.doctors.postgraduateDegrees,
                        "count"
                    ),

                    backgroundColor: colors.purple,

                    borderRadius: 8
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Super Specialties - POLAR AREA

    createChart("superSpecialtiesChart", {

        type: "polarArea",

        data: {
            labels: labels(
                analyticsData.doctors.superSpecialties,
                "category"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.doctors.superSpecialties,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.doctors.superSpecialties.length
                    )
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    position: "right"
                }
            }
        }
    });


    // Professional Certifications - DOUGHNUT

    createChart("professionalCertificationsChart", {

        type: "doughnut",

        data: {
            labels: labels(
                analyticsData.doctors.professionalCertifications,
                "category"
            ),

            datasets: [
                {
                    data: values(
                        analyticsData.doctors.professionalCertifications,
                        "count"
                    ),

                    backgroundColor: colorsFor(
                        analyticsData.doctors.professionalCertifications.length
                    ),

                    borderWidth: 0
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            cutout: "55%",

            plugins: {
                legend: {
                    position: "right"
                }
            }
        }
    });


    // Medical Schools - BAR

    createChart("medicalSchoolsChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.medicalSchools,
                "category"
            ),

            datasets: [
                {
                    label: "Doctors",

                    data: values(
                        analyticsData.doctors.medicalSchools,
                        "count"
                    ),

                    backgroundColor: colors.orange,

                    borderRadius: 8
                }
            ]
        },

        options: {
            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                x: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Busiest Doctors - HORIZONTAL BAR

    createChart("busiestDoctorsChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.busiestDoctors,
                "doctorName"
            ),

            datasets: [
                {
                    label: "Appointments",

                    data: values(
                        analyticsData.doctors.busiestDoctors,
                        "appointmentCount"
                    ),

                    backgroundColor: colors.primary,

                    borderRadius: 8
                }
            ]
        },

        options: {
            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                }
            },

            scales: {
                x: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Doctor Revenue - HORIZONTAL BAR

    createChart("doctorRevenueChart", {

        type: "bar",

        data: {
            labels: labels(
                analyticsData.doctors.revenue,
                "doctorName"
            ),

            datasets: [
                {
                    label: "Revenue",

                    data: values(
                        analyticsData.doctors.revenue,
                        "revenue"
                    ),

                    backgroundColor: colors.success,

                    borderRadius: 8
                }
            ]
        },

        options: {
            indexAxis: "y",

            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    display: false
                },

                tooltip: currencyTooltip()
            },

            scales: {
                x: {
                    beginAtZero: true,

                    ticks: {
                        callback: value => currencyValue(value)
                    }
                },

                y: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // Doctor Performance - RADAR

    const performanceData =
        analyticsData.doctors.performance;

    const performanceLabels =
        performanceData.map(
            doctor => doctor.doctorName
        );

    createChart("doctorPerformanceChart", {

        type: "bar",

        data: {
            labels: performanceLabels,

            datasets: [
                {
                    label: "Total Appointments",

                    data: performanceData.map(
                        doctor => doctor.totalAppointments
                    ),

                    backgroundColor: colors.primary,

                    borderRadius: 6
                },

                {
                    label: "Completed",

                    data: performanceData.map(
                        doctor => doctor.completedAppointments
                    ),

                    backgroundColor: colors.success,

                    borderRadius: 6
                },

                {
                    label: "Cancelled",

                    data: performanceData.map(
                        doctor => doctor.cancelledAppointments
                    ),

                    backgroundColor: colors.danger,

                    borderRadius: 6
                }
            ]
        },

        options: {
            responsive: true,

            maintainAspectRatio: false,

            plugins: {
                legend: {
                    position: "top"
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        precision: 0
                    }
                },

                x: {
                    grid: {
                        display: false
                    }
                }
            }
        }
    });


    // =========================================================
    // FINISHED
    // =========================================================

    console.log("Wellora Admin Analytics charts initialized.");
});