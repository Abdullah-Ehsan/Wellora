document.addEventListener("DOMContentLoaded", function () {
    const filters = ["specialtyFilter", "languageFilter", "genderFilter"];

    filters.forEach(f => {
        document.getElementById(f).addEventListener("change", function () {
            applyFilters();
        });
    });
});

function applyFilters() {
    const specialty = document.getElementById("specialtyFilter").value;
    const language = document.getElementById("languageFilter").value;
    const gender = document.getElementById("genderFilter").value;

    // TODO: AJAX call to controller with filters
    console.log("Filters applied:", specialty, language, gender);
}
