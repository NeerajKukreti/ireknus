$(document).ready(function () {
    $("#CATEGORY").select2({
        placeholder: "Select categories",
        width: 500
    });

    $("#KEYWORD_SET").select2({
        placeholder: "Select keyword set",
        width: 500
    });

    $(document).on('click', '#btnSaveSettings', function () {

        if ($("#ALERT_NAME").val() == '') {
            $("#ALERT_NAME").addClass('cstErrAlert')
            return;
        }
        else {
            $("#ALERT_NAME").removeClass('cstErrAlert')
        }

       

        if ($("#CATEGORY").val() == '') {
            $('#CATEGORY').next('.select2').addClass('cstErrAlert')
            return;
        }
        else {
            $('#CATEGORY').next('.select2').removeClass('cstErrAlert')
        }

        if ($("#KEYWORD_SET").val() == '') {
            $('#KEYWORD_SET').next('.select2').addClass('cstErrAlert')
            return;
        }
        else {
            $('#KEYWORD_SET').next('.select2').removeClass('cstErrAlert')
        }
        var fd = new FormData($('#frm_Settings')[0]);
        fd.set('CATEGORY', $("#CATEGORY").val().join("|"));
        fd.set('KEYWORD_SET', $("#KEYWORD_SET").val().join("|"));
        fd.set('WATCHLIST', $(".chkIncludeWatchList").is(':checked'));
        fd.set('ACTIVE', $(".chkActive").is(':checked'));
        
        $.ajax({
            url: saveSettings ,
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            beforeSend: function () { $('#spinnertReportCount').show(); },
            success: function () {
                $('#spinnertReportCount').hide();
                settingsTable.reloadTable();
            },
            error: function () { },
            complete: function () {
                $('#ALERT_NAME').val(0);
                $('#ALERT_NAME').val('');
                $('#CATEGORY').val('');
                $('#CATEGORY').trigger('change');
                $('#KEYWORD_SET').val('');
                $('#KEYWORD_SET').trigger('change');

                $('#spinnertReportCount').hide();
            }
        });
    });

    $(document).on('click', '.clsEditSettings', function () {
        
        $('#ALERT_ID').val($(this).data('id'));
        $('#ALERT_NAME').val($(this).data('name'));
        $("#chkIncludeWatchList").prop("checked", $(this).data('watchlist'));
        $("#chkActive").prop("checked", $(this).data('active'));

        $('#CATEGORY').val($(this).data('category').split('|')).change();
        $('#KEYWORD_SET').val($(this).data('keyword_set').split('|')).change();
        //data-keyword_set
        //data-category



    });
});