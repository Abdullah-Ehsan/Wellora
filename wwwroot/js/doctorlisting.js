document.addEventListener("DOMContentLoaded", function () {
    const filters = ["specialtyFilter", "languageFilter", "genderFilter"];

    filters.forEach(f => {
        const el = document.getElementById(f);
        if (el) {
            el.addEventListener("change", function () {
                applyFilters(); // reset to page 1 when filter changes
            });
        }
    });
});

function applyFilters(pageNumber = 1) {
    const specialty = document.getElementById("specialtyFilter").value;
    const language = document.getElementById("languageFilter").value;
    const gender = document.getElementById("genderFilter").value;

    const query = `?pageNumber=${pageNumber}&specialty=${specialty}&language=${language}&gender=${gender}`;

    fetch("/Patient/DoctorInformation/DoctorListing" + query, {
        headers: { "X-Requested-With": "XMLHttpRequest" }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Network response was not ok");
            }
            return response.text();
        })
        .then(html => {
            const container = document.getElementById("doctorCardsContainer");
            if (container) {
                container.innerHTML = html;
            }
        })
        .catch(err => console.error("Error loading doctors:", err));
}
