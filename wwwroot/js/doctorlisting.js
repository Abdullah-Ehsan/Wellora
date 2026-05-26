document.addEventListener("DOMContentLoaded", function () {
    const filters = ["specialtyFilter", "languageFilter", "genderFilter"];

    filters.forEach(f => {
        document.getElementById(f).addEventListener("change", function () {
            applyFilters();
        });
    });
});

function applyFilters(pageNumber = 1) {
    const specialty = document.getElementById("specialtyFilter").value;
    const language = document.getElementById("languageFilter").value;
    const gender = document.getElementById("genderFilter").value;

    const query = `?pageNumber=${pageNumber}&specialty=${specialty}&language=${language}&gender=${gender}`;

    // Correct route: Patient area, DoctorListing action
    fetch("/Patient/DoctorListing" + query)
        .then(response => response.text())
        .then(html => {
            document.getElementById("doctorCardsContainer").innerHTML = html;
        })
        .catch(err => console.error("Error loading doctors:", err));
}

