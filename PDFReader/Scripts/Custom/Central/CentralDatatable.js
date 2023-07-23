var CentralTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function (query) {
        
        table = $('#CentralTable').dataTable({
            retrieve: true,
            "autowidth": "true",
            "scrollX": true,
            "ajax": {
                "url": searchedCentral,
                "data": function (d) {               
                    d.query = getQueryText();
                },
                "type": "POST",
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
                { "title": "FileNo", "data": "MOEFCC_File_No" },
                { "title": "Project", "data": "Project_Name" },
                { "title": "Company", "data": "Company" },
                { "title": "Status", "data": "Proposal_Status" },
                { "title": "Location", "data": "Location" },
                { "title": "ImpDates", "data": "Important_Dates" },
                { "title": "Category", "data": "Category" },
                { "title": "Type", "data": "Type_of_project" },
                {
                    "title": "Files", "data": "Files_Detail",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html(oData.Files_Detail + oData.Acknowlegment);
                    }
                },
                { "title": "Input", "data": "input_company_name" },
                { "title": "Subsidiary", "data": "subsidiary_name" }
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
        init: function (query) {
            if (!jQuery().dataTable) {
                return;
            }

            Keyword(query);
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