
$(document).ready(function () {

    $('#refreshDashboard').click(function () {

        $.get('/Admin/GetDashboardStats', function (result) {
            $('#dashboard').html(result);
        });

        $.get('/Admin/GetParkingLocations', function (result) {
            $('#parkingLocations').html(result);
        });

        $.get('/Admin/GetUsers', function (result) {
            $('#registeredUsers').html(result);
        });

        $.get('/Admin/GetBookings', function (result) {
            $('#allBookings').html(result);
        });

    });

});

