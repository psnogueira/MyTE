// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$(document).ready(function () {
    // Set the timeout duration in milliseconds (e.g., 5000 for 5 seconds)
    const timeoutDuration = 5000;

    // Check if the success alert exists
    if ($($(".sucesso").is(":visible"))) {
        // Set a timer to hide the alert after the specified timeout
        setTimeout(function () {
            $(".sucesso").fadeOut();
        }, timeoutDuration);
    }
});


