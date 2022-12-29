var CompanyTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    //alert('hi');
    var table;
    var Company = function (year) {
        table = $('#CompanyTable').dataTable({
            retrieve: true,
            "ajax": {
                "url": loadCompanyList,
                "type": "GET",
                "datatype": "json",
                "data": { financialyear: year },
            },
            "columns": [
                { "title": "CompanyName", "data": "CompanyName", },
                { "title": "Url", "data": "URL", },
            ],

            dom: 'lBfrtip',
            buttons: ['excel', 'csv'],
            iDisplayLength: 100,
        });
    }

    return {
        //main function to initiate the module
        init: function (year) {
            if (!jQuery().dataTable) {
                return;
            }

            Company(year);
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        },
        destroy: function () {
            if (table != null)
                table.DataTable().destroy();
        }
    };
}();

$(document).on('change', '#ddl_financialyear', function () {
    //debugger;
    CompanyTables.destroy();
    CompanyTables.init($(this).val());
    //alert('hi');
});



$(document).on('click', '#btnSubmit', function () {
    if ($('#uploadfile').get(0).files.length == 0) {
        $('.uploadalert').show();
        $('.uploadalert').find('li:first').text('Choose a file before you upload');
        return;
    }
    else { $('.uploadalert').hide(); }

    var frmData = new FormData(document.querySelector('#frmUpload'));
    var filebase = $("#uploadfile").get(0);
    var files = filebase.files;

    if (filebase.files.length) {
        frmData.append(files[0].name, files[0]);
        frmData.append("FinantialYear", $("#ddl_financialyear").val())
    }

    $.ajax({
        url: uploadFileUrl,
        type: "POST",
        contentType: false,
        processData: false,
        data: frmData,
        beforeSend: function () {
            $.blockUI({ message: '<h5>Please wait file is being uploaded</h5>' });
        },
        success: function (result) {
            alert("File uploaded");
        },
        error: function (err) {
            alert(err.status + " " + err.statusText);
        },
        complete: function () {
            $.unblockUI();
        }
    });
});