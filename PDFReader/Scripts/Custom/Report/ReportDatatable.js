var ReportTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function (year, query, queryEnabled) {
        table = $('#ReportTable').dataTable({
            retrieve: true,
            "autowidth": "true",
            "scrollX": true,
            "ajax": {
                "url": searchedReport,
                "data": function (d) {
                    d.year = $('#ddl_financialyear').val();
                    d.query = getQueryText();
                    d.queryEnabled = $("#chkComplexSearchEnable").is(":checked");
                },
                "type": "POST",
                "datatype": "json",
            },
            "columns": [
                { "title": "RT", "data": "NEWS_SUBMISSION_DATE", class: "width_20" },
                { "title": "Company Name", "data": "CompanyName" },
                {
                    "title": "Url", "data": "Url",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        oData.Url = oData.Url + " ";
                        $(nTd).html("<a href='" + oData.Url + "' target='_blank'>pdf page</a>");
                    }
                },
                { "title": "Category", "data": "FinancialYear", class:"width_400" },
                { "title": "Page Number", "data": "PDFPageNumber" },
                { "title": "Keywords", "data": "FoundKeywords" },
                { "title": "Total Pages", "data": "TotalPages" },
                {
                    "title": "", "data": "TotalPages",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        oData.Url = oData.Url + " ";
                        $(nTd).html("<a href='/report/advancesearch?reportid=" + oData.ReportId + "' target='_blank'><i class='feather icon-search'></i></a>");
                    } }
            ],
            "sAjaxDataProp": "",
            dom: 'lBfrtip',
            buttons: ['excel', 'csv'],
            iDisplayLength: 100,
            "aLengthMenu": [[100, 200, -1], [100, 200, "All"]],
        });
    }

    return {
        //main function to initiate the module
        init: function (year, query, includeDate) {
            if (!jQuery().dataTable) {
                return;
            }

            Keyword(year, query, includeDate);
        },
        destroy: function () {
            if (table != null)
                table.DataTable().destroy();
        },
        clear: function () {
            if (table != null)
                table.DataTable().clear();
        },
        draw: function () {
            if (table != null)
                table.DataTable().draw();
        },
        reloadTable: function () {
            if (table != null)
                table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }
    };
}();

$(document).ready(function () {
    ReportTables.init($('#ddl_financialyear').val(), encodeURI(finalText), $("#chkComplexSearchEnable").is(":checked"));
});
$('#ReportTable').on('processing.dt', function (e, settings, processing) {
    if (processing) {
        $.blockUI();
    } else {
        //abp.ui.clearBusy(e.currentTarget);
        
        $.unblockUI();
    }
});