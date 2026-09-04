document.addEventListener("DOMContentLoaded", function () {

    const themeToggle = document.querySelector(".admin-theme-switch");

    if (!themeToggle) {
        return;
    }

    function applyTheme(theme) {

        if (theme === "dark") {

            document.documentElement.classList.add("dark");
            themeToggle.classList.add("dark-selected");

        } else {

            document.documentElement.classList.remove("dark");
            themeToggle.classList.remove("dark-selected");
        }
    }


    // Load saved theme
    const savedTheme = localStorage.getItem("website-theme");

    if (savedTheme === "dark") {
        applyTheme("dark");
    } else {
        applyTheme("light");
    }


    // Toggle theme
    themeToggle.addEventListener("click", function () {

        const isDark =
            document.documentElement.classList.contains("dark");

        if (isDark) {

            localStorage.setItem("website-theme", "light");
            applyTheme("light");

        } else {

            localStorage.setItem("website-theme", "dark");
            applyTheme("dark");
        }

    });

});
