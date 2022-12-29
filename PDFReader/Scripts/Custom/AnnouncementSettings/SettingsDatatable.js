var settingsTable = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function () {
        table = $('#SettingsTable').dataTable({
            retrieve: true,
            "autowidth": "true",
            "scrollX": true,
            "ajax": {
                "url": settingsList,
                "type": "GET",
                "datatype": "json"
            },
            "columns": [
                { "title": "ID", "data": "ALERT_ID", className: "hidecolumn" },
                { "title": "Name", "data": "ALERT_NAME", className: "dt-w1" },
                { "title": "CATEGORIES", "data": "CATEGORY", className: "dt-w5" },
                { "title": "KEYWORD SET", "data": "KEYWORD_SET", className: "dt-w5" },
                {
                    "title": "WATCHLIST ", "data": "WATCHLIST", className: "dt-w1",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html(oData.WATCHLIST ? 'Yes' : 'No');
                    }
                },
                {
                    "title": "ACTIVE", "data": "ACTIVE",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html(oData.ACTIVE ? 'Yes' : 'No');
                    }
                },
                {
                    "title": "Edit", "data": "ACTIVE",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var str = ' data-id="' + oData.ALERT_ID + '" data-name="' + oData.ALERT_NAME + '"  data-CATEGORY="' + oData.CATEGORY + '"  data-KEYWORD_SET="' + oData.KEYWORD_SET + '" data-WATCHLIST="' + oData.WATCHLIST + '"  data-ACTIVE="' + oData.ACTIVE +'" ' ;
                        $(nTd).html("<div class='clsEditSettings' " + str +" style='text-decoration: underline;color: blue;cursor: pointer;'><i class='fas fa-edit'></i></div>");
                    }
                }
            ],
            "order": [[0, "desc"]],
            "sAjaxDataProp": "",
            dom: 'Bfrtip',
            iDisplayLength: 100,
            "aLengthMenu": [[100, 200, -1], [100, 200, "All"]],
            buttons: {
                buttons: [
                    { extend: 'csv', className: 'btn-primary' },
                    { extend: 'excel', className: 'btn-primary' }
                ]
            }
        });
    }

    return {
        //main function to initiate the module
        init: function () {
            if (!jQuery().dataTable) {
                return;
            }

            Keyword();
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
    settingsTable.init();
});
