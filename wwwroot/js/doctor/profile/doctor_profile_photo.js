document.addEventListener("DOMContentLoaded", function () {

    const photoInput = document.getElementById("ProfilePicture");
    const photoPreview = document.getElementById("doctorProfilePreview");

    if (!photoInput || !photoPreview) {
        return;
    }

    photoInput.addEventListener("change", function () {

        const file = this.files && this.files[0];

        if (!file) {
            return;
        }

        /*
         * Make sure the selected file is an image.
         * This is only client-side validation.
         * Server-side validation is still required.
         */
        if (!file.type.startsWith("image/")) {

            this.value = "";

            return;
        }

        /*
         * Create a temporary browser URL for the selected image.
         * This lets the user see the new photo immediately
         * without uploading it first.
         */
        const previewUrl = URL.createObjectURL(file);

        photoPreview.src = previewUrl;

        /*
         * Release the temporary object URL after
         * the browser has loaded the image.
         */
        photoPreview.onload = function () {
            URL.revokeObjectURL(previewUrl);
        };
    });
});
