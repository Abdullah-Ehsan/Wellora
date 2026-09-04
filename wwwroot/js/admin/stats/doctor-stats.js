/* =========================================================
   DOCTOR STATS - APEXCHARTS
========================================================= */

document.addEventListener("DOMContentLoaded", function () {

    if (typeof ApexCharts === "undefined") {
        console.error("Doctor Stats: ApexCharts is not loaded.");
        return;
    }

    if (typeof doctorStatsData === "undefined") {
        console.error("Doctor Stats: doctorStatsData is not available.");
        return;
    }


    /* =====================================================
       COLORS
    ====================================================== */

    const COLORS = {
        primary: "#167c78",
        secondary: "#2b9b95",
        light: "#76c7c1",
        dark: "#0f5f5b",

        teal: "#167c78",
        green: "#3da58f",
        blue: "#4b8fcf",
        orange: "#e9a24b",
        red: "#dc6b6b",
        purple: "#8c73c9",
        gray: "#91a4a1",

        background: "#eaf6f5"
    };


    /* =====================================================
       THEME HELPERS
    ====================================================== */

    function isDarkMode() {
        return (
            document.documentElement.classList.contains("dark") ||
            document.body.classList.contains("dark") ||
            document.documentElement.getAttribute("data-theme") === "dark" ||
            document.body.getAttribute("data-theme") === "dark"
        );
    }


    function getChartThemeColors() {
        const dark = isDarkMode();

        return {
            text: dark ? "#e5f2f0" : "#637876",
            mutedText: dark ? "#9bb1ae" : "#7b8c8b",
            grid: dark ? "#294341" : "#e8efee",
            tooltip: dark ? "dark" : "light",
            markerStroke: dark ? "#172b2a" : "#ffffff",
            donutLabel: dark ? "#e5f2f0" : "#637876",
            background: dark ? "#172b2a" : "#ffffff"
        };
    }


    let chartTheme = getChartThemeColors();

    // Stores all rendered chart instances
    const charts = [];


    /* =====================================================
       COMMON OPTIONS
    ====================================================== */

    function getCommonChartOptions() {
        chartTheme = getChartThemeColors();

        return {
            chart: {
                fontFamily: "inherit",
                foreColor: chartTheme.text,

                toolbar: {
                    show: false
                },

                animations: {
                    enabled: true,
                    easing: "easeinout",
                    speed: 500
                }
            },

            dataLabels: {
                enabled: false
            },

            grid: {
                borderColor: chartTheme.grid,
                strokeDashArray: 4
            },

            tooltip: {
                theme: chartTheme.tooltip
            },

            legend: {
                fontSize: "12px",

                labels: {
                    colors: chartTheme.text
                }
            },

            theme: {
                mode: chartTheme.tooltip
            }
        };
    }


    /* =====================================================
       HELPER FUNCTIONS
    ====================================================== */

    function safeArray(value) {
        return Array.isArray(value) ? value : [];
    }


    function getValue(item, possibleNames) {
        for (const name of possibleNames) {
            if (
                item !== null &&
                item !== undefined &&
                item[name] !== undefined
            ) {
                return item[name];
            }
        }

        return null;
    }


    function formatCurrency(value) {
        const number = Number(value || 0);

        return number.toLocaleString(undefined, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }


    function renderChart(elementId, options) {
        const element = document.querySelector("#" + elementId);

        if (!element) {
            console.warn(
                "Doctor Stats: Chart element #" +
                elementId +
                " was not found."
            );

            return null;
        }

        const chart = new ApexCharts(element, options);

        chart.render();

        charts.push({
            id: elementId,
            chart: chart
        });

        return chart;
    }


    /* =====================================================
       APPOINTMENTS OVER TIME
    ====================================================== */

    const appointmentTrend =
        safeArray(doctorStatsData.appointments.overTime);

    renderChart("appointmentsOverTimeChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "area",
            height: 330
        },

        series: [
            {
                name: "Appointments",

                data: appointmentTrend.map(item => ({
                    x: getValue(item, ["date", "Date"]),
                    y: Number(
                        getValue(item, ["count", "Count"]) || 0
                    )
                }))
            }
        ],

        colors: [COLORS.primary],

        stroke: {
            curve: "smooth",
            width: 3
        },

        fill: {
            type: "gradient",

            gradient: {
                shadeIntensity: 1,
                opacityFrom: 0.35,
                opacityTo: 0.03,
                stops: [0, 90, 100]
            }
        },

        markers: {
            size: 3,
            colors: [COLORS.primary],
            strokeColors: chartTheme.markerStroke,
            strokeWidth: 2
        },

        xaxis: {
            type: "datetime",

            labels: {
                datetimeUTC: false,

                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        yaxis: {
            min: 0,
            forceNiceScale: true,

            labels: {
                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        tooltip: {
            x: {
                format: "dd MMM yyyy"
            }
        }
    });


    /* =====================================================
       APPOINTMENT STATUS
    ====================================================== */

    const appointmentStatus =
        safeArray(doctorStatsData.appointments.status);

    renderChart("appointmentStatusChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "donut",
            height: 290
        },

        series: appointmentStatus.map(item =>
            Number(getValue(item, ["count", "Count"]) || 0)
        ),

        labels: appointmentStatus.map(item =>
            getValue(item, ["status", "Status"]) || "Unknown"
        ),

        colors: [
            COLORS.primary,
            COLORS.green,
            COLORS.orange,
            COLORS.red,
            COLORS.blue
        ],

        legend: {
            position: "bottom",
            fontSize: "12px",

            labels: {
                colors: chartTheme.text
            }
        },

        plotOptions: {
            pie: {
                donut: {
                    size: "68%",

                    labels: {
                        show: true,

                        total: {
                            show: true,
                            label: "Appointments",
                            color: chartTheme.donutLabel
                        }
                    }
                }
            }
        }
    });


    /* =====================================================
       APPOINTMENTS BY WEEKDAY
    ====================================================== */

    const weeklyAppointments =
        safeArray(doctorStatsData.appointments.weekly);

    renderChart("weeklyAppointmentsChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "bar",
            height: 320
        },

        series: [
            {
                name: "Appointments",

                data: weeklyAppointments.map(item =>
                    Number(getValue(item, ["count", "Count"]) || 0)
                )
            }
        ],

        colors: [COLORS.secondary],

        plotOptions: {
            bar: {
                borderRadius: 5,
                columnWidth: "48%"
            }
        },

        xaxis: {
            categories: weeklyAppointments.map(item =>
                getValue(item, ["day", "Day"]) || ""
            ),

            labels: {
                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        yaxis: {
            min: 0,
            forceNiceScale: true,

            labels: {
                style: {
                    colors: chartTheme.mutedText
                }
            }
        }
    });


    /* =====================================================
       MONTHLY APPOINTMENTS
    ====================================================== */

    const monthlyAppointments =
        safeArray(doctorStatsData.appointments.monthly);

    renderChart("monthlyAppointmentsChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "bar",
            height: 320
        },

        series: [
            {
                name: "Appointments",

                data: monthlyAppointments.map(item =>
                    Number(getValue(item, ["count", "Count"]) || 0)
                )
            }
        ],

        colors: [COLORS.primary],

        plotOptions: {
            bar: {
                borderRadius: 5,
                columnWidth: "55%"
            }
        },

        xaxis: {
            categories: monthlyAppointments.map(item =>
                getValue(item, ["monthName", "MonthName"]) || ""
            ),

            labels: {
                rotate: -45,

                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        yaxis: {
            min: 0,
            forceNiceScale: true,

            labels: {
                style: {
                    colors: chartTheme.mutedText
                }
            }
        }
    });


    /* =====================================================
       APPOINTMENT METHODS
    ====================================================== */

    const appointmentMethods =
        safeArray(doctorStatsData.appointments.methods);

    renderChart("appointmentMethodsChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "donut",
            height: 290
        },

        series: appointmentMethods.map(item =>
            Number(getValue(item, ["count", "Count"]) || 0)
        ),

        labels: appointmentMethods.map(item =>
            getValue(item, ["method", "Method"]) || "Unknown"
        ),

        colors: [
            COLORS.primary,
            COLORS.blue,
            COLORS.orange,
            COLORS.green
        ],

        legend: {
            position: "bottom",

            labels: {
                colors: chartTheme.text
            }
        },

        plotOptions: {
            pie: {
                donut: {
                    size: "68%"
                }
            }
        }
    });


    /* =====================================================
       MONTHLY REVENUE
    ====================================================== */

    const monthlyRevenue =
        safeArray(doctorStatsData.revenue.monthly);

    renderChart("monthlyRevenueChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "bar",
            height: 330
        },

        series: [
            {
                name: "Revenue",

                data: monthlyRevenue.map(item =>
                    Number(getValue(item, ["amount", "Amount"]) || 0)
                )
            }
        ],

        colors: [COLORS.primary],

        plotOptions: {
            bar: {
                borderRadius: 6,
                columnWidth: "50%"
            }
        },

        xaxis: {
            categories: monthlyRevenue.map(item =>
                getValue(item, ["monthName", "MonthName"]) || ""
            ),

            labels: {
                rotate: -45,

                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        yaxis: {
            min: 0,
            forceNiceScale: true,

            labels: {
                style: {
                    colors: chartTheme.mutedText,

                    formatter: function (value) {
                        return formatCurrency(value);
                    }
                }
            }
        },

        tooltip: {
            y: {
                formatter: function (value) {
                    return formatCurrency(value);
                }
            }
        }
    });


    /* =====================================================
       REVENUE OVER TIME
    ====================================================== */

    const revenueOverTime =
        safeArray(doctorStatsData.revenue.overTime);

    renderChart("revenueOverTimeChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "area",
            height: 330
        },

        series: [
            {
                name: "Revenue",

                data: revenueOverTime.map(item => ({
                    x: getValue(item, ["date", "Date"]),
                    y: Number(
                        getValue(item, ["amount", "Amount"]) || 0
                    )
                }))
            }
        ],

        colors: [COLORS.green],

        stroke: {
            curve: "smooth",
            width: 3
        },

        fill: {
            type: "gradient",

            gradient: {
                opacityFrom: 0.35,
                opacityTo: 0.03
            }
        },

        markers: {
            size: 3,
            colors: [COLORS.green],
            strokeColors: chartTheme.markerStroke,
            strokeWidth: 2
        },

        xaxis: {
            type: "datetime",

            labels: {
                datetimeUTC: false,

                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        yaxis: {
            labels: {
                style: {
                    colors: chartTheme.mutedText
                },

                formatter: function (value) {
                    return formatCurrency(value);
                }
            }
        },

        tooltip: {
            x: {
                format: "dd MMM yyyy"
            },

            y: {
                formatter: function (value) {
                    return formatCurrency(value);
                }
            }
        }
    });


    /* =====================================================
       PAYMENT METHODS
    ====================================================== */

    const paymentMethods =
        safeArray(doctorStatsData.revenue.paymentMethods);

    renderChart("paymentMethodsChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "donut",
            height: 290
        },

        series: paymentMethods.map(item =>
            Number(getValue(item, ["count", "Count"]) || 0)
        ),

        labels: paymentMethods.map(item =>
            getValue(item, ["paymentMethod", "PaymentMethod"]) || "Unknown"
        ),

        colors: [
            COLORS.primary,
            COLORS.blue,
            COLORS.orange,
            COLORS.green
        ],

        legend: {
            position: "bottom",

            labels: {
                colors: chartTheme.text
            }
        },

        plotOptions: {
            pie: {
                donut: {
                    size: "68%"
                }
            }
        }
    });


    /* =====================================================
       PAYMENT STATUS
    ====================================================== */

    const paymentStatus =
        safeArray(doctorStatsData.revenue.paymentStatus);

    renderChart("paymentStatusChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "donut",
            height: 290
        },

        series: paymentStatus.map(item =>
            Number(getValue(item, ["count", "Count"]) || 0)
        ),

        labels: paymentStatus.map(item =>
            getValue(item, ["status", "Status"]) || "Unknown"
        ),

        colors: [
            COLORS.green,
            COLORS.orange,
            COLORS.red,
            COLORS.blue
        ],

        legend: {
            position: "bottom",

            labels: {
                colors: chartTheme.text
            }
        },

        plotOptions: {
            pie: {
                donut: {
                    size: "68%"
                }
            }
        }
    });


    /* =====================================================
       MOST VISITED PATIENTS
    ====================================================== */

    const mostVisited =
        safeArray(doctorStatsData.patients.mostVisited);

    renderChart("mostVisitedPatientsChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "bar",
            height: Math.max(320, mostVisited.length * 42)
        },

        series: [
            {
                name: "Appointments",

                data: mostVisited.map(item =>
                    Number(
                        getValue(
                            item,
                            ["appointmentCount", "AppointmentCount"]
                        ) || 0
                    )
                )
            }
        ],

        colors: [COLORS.primary],

        plotOptions: {
            bar: {
                horizontal: true,
                borderRadius: 5,
                barHeight: "55%"
            }
        },

        xaxis: {
            min: 0,

            labels: {
                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        yaxis: {
            categories: mostVisited.map(item =>
                getValue(item, ["patientName", "PatientName"]) ||
                "Unknown Patient"
            ),

            labels: {
                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        tooltip: {
            y: {
                formatter: function (value) {
                    return value + " appointment(s)";
                }
            }
        }
    });


    /* =====================================================
       HIGHEST SPENDING PATIENTS
    ====================================================== */

    const highestSpending =
        safeArray(doctorStatsData.patients.highestSpending);

    renderChart("highestSpendingPatientsChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "bar",
            height: Math.max(320, highestSpending.length * 42)
        },

        series: [
            {
                name: "Total Spent",

                data: highestSpending.map(item =>
                    Number(
                        getValue(
                            item,
                            ["totalSpent", "TotalSpent"]
                        ) || 0
                    )
                )
            }
        ],

        colors: [COLORS.green],

        plotOptions: {
            bar: {
                horizontal: true,
                borderRadius: 5,
                barHeight: "55%"
            }
        },

        xaxis: {
            min: 0,

            labels: {
                style: {
                    colors: chartTheme.mutedText
                },

                formatter: function (value) {
                    return formatCurrency(value);
                }
            }
        },

        yaxis: {
            categories: highestSpending.map(item =>
                getValue(item, ["patientName", "PatientName"]) ||
                "Unknown Patient"
            ),

            labels: {
                style: {
                    colors: chartTheme.mutedText
                }
            }
        },

        tooltip: {
            y: {
                formatter: function (value) {
                    return formatCurrency(value);
                }
            }
        }
    });


    /* =====================================================
       PATIENT TYPES
    ====================================================== */

    const patientTypes =
        safeArray(doctorStatsData.patients.patientTypes);

    renderChart("patientTypesChart", {
        ...getCommonChartOptions(),

        chart: {
            ...getCommonChartOptions().chart,
            type: "donut",
            height: 290
        },

        series: patientTypes.map(item =>
            Number(getValue(item, ["count", "Count"]) || 0)
        ),

        labels: patientTypes.map(item =>
            getValue(item, ["type", "Type"]) || "Unknown"
        ),

        colors: [
            COLORS.primary,
            COLORS.green,
            COLORS.blue,
            COLORS.orange
        ],

        legend: {
            position: "bottom",

            labels: {
                colors: chartTheme.text
            }
        },

        plotOptions: {
            pie: {
                donut: {
                    size: "68%",

                    labels: {
                        show: true,

                        total: {
                            show: true,
                            label: "Patients",
                            color: chartTheme.donutLabel
                        }
                    }
                }
            }
        }
    });


    /* =====================================================
       UPDATE CHARTS WHEN THEME CHANGES
    ====================================================== */

    function updateChartsForTheme() {
        chartTheme = getChartThemeColors();

        charts.forEach(function (item) {
            item.chart.updateOptions({
                chart: {
                    foreColor: chartTheme.text
                },

                grid: {
                    borderColor: chartTheme.grid
                },

                tooltip: {
                    theme: chartTheme.tooltip
                },

                legend: {
                    labels: {
                        colors: chartTheme.text
                    }
                },

                xaxis: {
                    labels: {
                        style: {
                            colors: chartTheme.mutedText
                        }
                    }
                },

                yaxis: {
                    labels: {
                        style: {
                            colors: chartTheme.mutedText
                        }
                    }
                }
            }, false, false);
        });
    }


    const themeObserver = new MutationObserver(function () {
        updateChartsForTheme();
    });


    themeObserver.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ["class", "data-theme"]
    });


    themeObserver.observe(document.body, {
        attributes: true,
        attributeFilter: ["class", "data-theme"]
    });


    /* =====================================================
       RESPONSIVE APEXCHARTS
    ====================================================== */

    window.addEventListener("resize", function () {
        setTimeout(function () {
            window.dispatchEvent(new Event("resize"));
        }, 100);
    });

});