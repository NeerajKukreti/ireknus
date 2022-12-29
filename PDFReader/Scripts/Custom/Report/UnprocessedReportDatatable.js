var UpReportTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function (param) {
        table = $('#UpReportTable').dataTable({
            retrieve: true,
            "autowidth": "true",
            "scrollX": true,
            "ajax": {
                "url": GetUnproccesedRpt + "?" + param,
                "type": "GET",
                "datatype": "json"
            },
            "columns": [
                { "title": "Company Name", "data": "CompanyName" },
                {
                    "title": "Url", "data": "URL",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        oData.Url = oData.Url + " ";
                        $(nTd).html("<a href='" + oData.Url + "' target='_blank'>pdf page</a>");
                    }
                }
            ],
            "sAjaxDataProp": "",
            dom: 'Bfrtip',
            buttons: [
                'copy', 'csv', 'excel', 'pdf', 'print'
            ]
        });
    }

    return {
        //main function to initiate the module
        init: function (param) {
            if (!jQuery().dataTable) {
                return;
            }

            Keyword(param);
        },
        destroy: function () {
            if (table != null)
                table.DataTable().destroy()
        },
        clear: function () {
            if (table != null)
                table.DataTable().clear()
        },
        draw: function () {
            if (table != null)
                table.DataTable().draw()
        },
        reloadTable: function () {
            if (table != null)
                table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }
    };
}();