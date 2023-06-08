var StateTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function () {
        debugger;
        table = $('#StateTable').dataTable({
            retrieve: true,
            "autowidth": "true",
            "scrollX": true,
            "ajax": {
                "url": searchedState,
                "type": "GET",
                "datatype": "json"
            },
            "columns": [
                {
                    "title": "Date__", "data": "DateTimeStamp",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var dtSub = /\/Date\((\d*)\)\//.exec(oData.DateTimeStamp);
                        var options = { hour12: false };
                        $(nTd).html("<b style='color:#4680ff'>" + new Date(+dtSub[1]).toLocaleString('en-us', options) + "</b>");
                    }
                },
                { "title": "PRNo", "data": "Proposal_No" },
                { "title": "FileNo", "data": "File_No" },
                { "title": "Project", "data": "Proposal_Name" },
                { "title": "Company", "data": "Company_or_Proponent" },
                { "title": "Status", "data": "Current_Status" },
                { "title": "Location", "data": "Location" },
                { "title": "ImpDates", "data": "Important_Dates" },
                { "title": "Category", "data": "Category" },
                {
                    "title": "Files", "data": "Files_Detail",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html(oData.Files_Detail + oData.Acknowlegment);
                    }
                }
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