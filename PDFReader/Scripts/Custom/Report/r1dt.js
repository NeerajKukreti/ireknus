var ReportTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var Keyword = function () {
        var groupColumn = 0;
        var table = $('#ReportTable');

        //debugger;
        // begin first table
        table.dataTable({
            "autowidth": "true",
            "scrollX": true,
            "ajax": {
                "url": searchedReport + "?year=2020-2021",
                "type": "GET",
                "datatype": "json"
            },//
            "columns": [
                //{
                //    "class": "details-control",
                //    "orderable": false,
                //    "data": null,
                //    "defaultContent": ""
                //},
                { "title": "Company Name", "data": "CompanyName" },
                {
                    "title": "Url", "data": "Url",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html("<a href='" + oData.Url + "' target='_blank'>pdf page</a>");
                    }
                },
                { "title": "Year", "data": "FinancialYear" },
                { "title": "Page Number", "data": "PDFPageNumber" },
                { "title": "Keywords", "data": "FoundKeywords" },
            ],

            "drawCallback": function (settings) {
                var api = this.api();
                var rows = api.rows({ page: 'current' }).nodes();
                var last = null;

                api.column(groupColumn, { page: 'current' }).data().each(function (group, i) {
                    debugger;
                    if (last !== group) {
                        $(rows).eq(i).before(
                            '<tr class="group"><td colspan="5">' + group + '</td></tr>'
                        );

                        last = group;
                    }
                });
            },
            "sAjaxDataProp": "",
            dom: 'Bfrtip',
            buttons: [
                'copy', 'csv', 'excel', 'pdf', 'print'
            ]
        });
    }

    return {
        //main function to initiate the module
        init: function () {
            if (!jQuery().dataTable) {
                return;
            }

            Keyword();
        }
    };
}();

jQuery(document).ready(function () {
    ReportTables.init();
});

$(document).ready(function () {
    var table = $('#ReportTable').DataTable();

    function format(d) {
        return 'Full name: ' + d.CompanyName + ' ' + d.FinancialYear + '<br>' +
            'Salary: ' + d.PDFPageNumber + '<br>' +
            'The child row can contain any data you wish, including links, images, inner tables etc.';
    }

    var detailRows = [];

    $('#ReportTable tbody').on('click', 'tr td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = table.row(tr);
        var idx = $.inArray(tr.attr('id'), detailRows);

        if (row.child.isShown()) {
            tr.removeClass('details');
            row.child.hide();

            // Remove from the 'open' array
            detailRows.splice(idx, 1);
        }
        else {
            tr.addClass('details');
            row.child(format(row.data())).show();

            // Add to the 'open' array
            if (idx === -1) {
                detailRows.push(tr.attr('id'));
            }
        }
    });

    // On each draw, loop over the `detailRows` array and show any child rows
    table.on('draw', function () {
        $.each(detailRows, function (i, id) {
            $('#' + id + ' td.details-control').trigger('click');
        });
    });
});