function applyFilters(pageNumber = 1) {
    const gender = document.getElementById("genderFilter") ? document.getElementById("genderFilter").value : "";

    // Build query params
    const params = new URLSearchParams();
    if (gender) {
        params.append("gender", gender);
    }
    params.append("pageNumber", pageNumber);

    // Fetch the updated partial view
    fetch(`${patientListingUrl}?${params.toString()}`, {
        method: "GET",
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Network response was not ok");
            }
            return response.text();
        })
        .then(html => {
            const container = document.getElementById("patientCardsContainer");
            if (container) {
                container.innerHTML = html;
            }
        })
        .catch(error => {
            console.error("Error fetching patient cards:", error);
        });
}

// Trigger filter on dropdown change
document.addEventListener("DOMContentLoaded", function () {
    const genderDropdown = document.getElementById("genderFilter");
    if (genderDropdown) {
        genderDropdown.addEventListener("change", function () {
            applyFilters(1); // Always reset to page 1 on filter change
        });
    }
});