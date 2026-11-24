// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(
    function () {

        $('#refreshBookings').click(function () {

            $.get('/User/GetMyBookings', function (result) {

                $('#bookingTableContainer').html(result).fadeIn("slow");

            });

        });

    }
);
