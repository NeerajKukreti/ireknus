$(document).ready(function () {
    $("#ddl_KeywordSet").select2({
        placeholder: "Select keyword set"
    });
});

function LoadCounts(year) {
    $.ajax({
        url: reportDataUrl,
        type: 'GET',
        data: { FinancialYear: year },
        beforeSend: function () { $('#spinnertReportCount').show(); },
        success: function (report) {
            //$('#lblcnt').text(report.Processed.length + "/" + report.UnProcessed.length);
            $('#lblcnt').show();
            $('#aprocessed').text(report.Processed.length);
            $('#aunprocessed').text(report.UnProcessed.length);
            $("#btnExecuteSearch").css({ display: "inline-flex" });
            $("#btnDeleteReport").css({ display: "inline-flex" });
            $("#btnDeleteCompany").css({ display: "inline-flex" });

            $("#ReportTable_wrapper").show();

            ReportTables.destroy();
            ReportTables.init($('#ddl_financialyear').val(), new Date().toLocaleString('en-us'), new Date().toLocaleString('en-us'));

            $.ajax({
                url: GetSkippedCount,
                type: 'GET',
                data: { Year: $('#ddl_financialyear').val() },
                success: function (report) {
                    $('#ContainsImg').text(report.ImgSkipped);
                    $('#NonImg').text(report.NonImgSkipped);
                    $('#spinnertReportCount').hide();
                },
                error: function () { alert('error occured while trying to get the skipped count') }
            });
        },
        error: function () { alert('error occured while trying to get the report data') }
    });
}

$(document).on('change', '#ddl_financialyear', function () {
    if ($(this).val() != "Select") {
        $("#lblDtRange").show();
    }
    else { $("#lblDtRange").hide();}

    LoadCounts($(this).val());
});

$(document).on('click', '#btnExecuteSearch', function () {

    if ($('#ddl_KeywordSet').val().length == 0) {
        $('#ddl_KeywordSet').next().addClass('cstErrAlert');
        return;
    }
    else {
        $('#ddl_KeywordSet').next().removeClass('cstErrAlert');
    }

    $.ajax({
        url: performSearch,
        type: 'GET',
        data: { Year: $('#ddl_financialyear').val(), KeywordSetId: $("#ddl_KeywordSet").val().join()},
        beforeSend: function () {
            $('#spinnertReportExecute').show();
        },
        success: function (report) {
            $('#spinnertReportExecute').hide();

            LoadCounts($('#ddl_financialyear').val());
        },
        error: function () { alert('error occured while trying to search') },
        complete: function () {
            ReportTables.reloadTable();
            //UpReportTables.reloadTable();
        }
    });
});

$(document).on('click', '#btnDeleteReport', function () {
    if (confirm('Are you sure you want to delete selected year report?')) {
        $.ajax({
            url: DeleteReport,
            type: 'GET',
            data: { Year: $('#ddl_financialyear').val() },
            beforeSend: function () { $('#spinnertRptDelete').show(); },
            success: function (report) {
                $('#spinnertRptDelete').hide();
                LoadCounts($('#ddl_financialyear').val());
            },
            error: function () { alert('error occured while trying to search') }
            ,
            complete: function () {
                ReportTables.reloadTable();
                //UpReportTables.reloadTable();
            }
        });
    }
});

$(document).on('click', '#btnDeleteCompany', function () {
    if (confirm('Are you sure you want to delete selected year company list?')) {
        $.ajax({
            url: DeleteCompany,
            type: 'GET',
            data: { Year: $('#ddl_financialyear').val() },
            beforeSend: function () { $('#spinnertCompanyDelete').show(); },
            success: function (report) {
                $('#spinnertCompanyDelete').hide();
                LoadCounts($('#ddl_financialyear').val());
            },
            error: function () { alert('error occured while trying to search') }
            ,
            complete: function () {
                ReportTables.reloadTable();
                //UpReportTables.reloadTable();
            }
        });
    }
});

$(document).on('click', '.btnPageTextDownload', function () {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent($(this).data('pagetext')));
    element.setAttribute('download', $(this).data('filename') + ".txt");

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
});

//$(document).on('click', '.showrpt', function () {
//    var year = $('#ddl_financialyear').val();
//    var type = $(this).data('type');

//    //ReportTables.destroy();
//    //UnReportTables.destroy();

//    if (type == 1) {
//        $("#ReportTable_wrapper").show();
//        $("#UpReportTable").hide();
//        $("#UpReportTable_wrapper").hide();
//        ReportTables.init($('#ddl_financialyear').val());
//        //$('#ReportTable').DataTable().ajax.reload();
//    }
//    else {
//        $("#ReportTable_wrapper").hide();
//        $("#UpReportTable").show();
//        $("#UpReportTable_wrapper").show();

//        //$('#UpReportTable').DataTable().ajax.url(GetUnproccesedRpt + "?year=" + year + "&type=" + type);
//        //$('#UpReportTable').DataTable().draw();
//        UpReportTables.destroy();
//        UpReportTables.init("year=" + $('#ddl_financialyear').val() + "&type=" + type);
//    }
//});