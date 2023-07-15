$(document).ready(function () {

    $("#ex_company").hide();

    $("#ex_com").click(function () {
        $("#ex_company").toggle(1000);
    });

    StateTables.init();
});