document.addEventListener("DOMContentLoaded", function () {

    const data = window.doctorDashboardData;

    // =========================
    // MONTHLY VISITS CHART
    // =========================
    var monthlyOptions = {
        chart: {
            type: 'line',
            height: 300
        },
        series: [{
            name: 'Visits',
            data: data.monthlyVisitValues
        }],
        xaxis: {
            categories: data.monthlyVisitLabels
        },
        colors: ['#004d4d']
    };

    new ApexCharts(document.querySelector("#monthlyChart"), monthlyOptions).render();


    // =========================
    // WEEKLY VISITS CHART
    // =========================
    var weeklyOptions = {
        chart: {
            type: 'bar',
            height: 300
        },
        series: [{
            name: 'Visits',
            data: data.weeklyVisitValues
        }],
        xaxis: {
            categories: data.weeklyVisitLabels
        },
        colors: ['#7dbebe']
    };

    new ApexCharts(document.querySelector("#weeklyChart"), weeklyOptions).render();


    // =========================
    // YEARLY REVENUE CHART
    // =========================
    var revenueOptions = {
        chart: {
            type: 'area',
            height: 300
        },
        series: [{
            name: 'Revenue',
            data: data.revenueValues
        }],
        xaxis: {
            categories: data.revenueLabels
        },
        colors: ['#006d77']
    };

    new ApexCharts(document.querySelector("#revenueChart"), revenueOptions).render();

});