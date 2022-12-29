var annTable = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function (catIds) {
        table = $('#tblAnnRpt').dataTable({
            retrieve: true,
            ajax: {
                url: getRepeatedAnnouncements,
                method: "GET",
                datatype: "json"
            },
            columns: [
                {
                    title: "CATEGORY", data: "CategoryName", className: "dt-w2"
                },
                {
                    title: "Code", data: "COMPANY_ID",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                       
                        $(nTd).html(
                            '<b data-company="' + oData.COMPANY_NAME + '" class="lnkCompanyId" style="cursor:pointer; color:#4680ff; text-decoration: underline;">' +
                            + oData.COMPANY_ID + "</b>");
                    }
                },
                {
                    title: "COMPANY", data: "COMPANY_NAME", className: "dt-w2"
                },

                {
                    title: "SUBJECT", data: "NEWS_SUBJECT", className: "dt-w4",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html("<b>" + oData.NEWS_SUBJECT + "</b>");
                    }
                },
                {
                    title: "DETAILS", data: "HEAD_LINE", className: "dt-w6",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html("<div class=''>" + oData.HEAD_LINE + "</div>");
                    }
                },
                {
                    title: "RT/DT", data: "NEWS_SUBMISSION_DATE", className: "dt-w2",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var dtSub = /\/Date\((\d*)\)\//.exec(oData.NEWS_SUBMISSION_DATE);
                        var dtDessi = /\/Date\((\d*)\)\//.exec(oData.NEWS_SUBMISSION_DATE);



                        $(nTd).html("" +
                            "<b><b style='color:#4680ff'>" + new Date(+dtSub[1]).toLocaleString('en-in').toUpperCase() + "</b>" +
                            "</br><b><b style='color:#4680ff'>" + new Date(+dtDessi[1]).toLocaleString('en-in').toUpperCase()+ "</b>");
                    }
                },
                {
                    title: "DT", data: "DISSEMINATION_DATE", className: "hidecolumn"
                },
                {
                    title: "PDF", data: "ATTACHMENT", className: "dt-w-1",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html("<a target='_blank' href=https://www.bseindia.com/xml-data/corpfiling/AttachLive/" + oData.ATTACHMENT + "><i class='fas fa-file-pdf'></i></a>");
                    }
                }
            ],
            "sAjaxDataProp": "",
            responsive: true,
            'iDisplayLength': 100,
            dom: 'lBfrtip',
            buttons: ['excel', 'csv'],
            "lengthMenu": [[25, 50, 100, -1], [25, 50, 100, "All"]],
            
        });
    }

    return {
        //main function to initiate the module
        init: function (year) {
            if (!jQuery().dataTable) {
                return;
            }

            Keyword(year);
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
    annTable.init("");
    //$('input[name="daterange"]').daterangepicker();
});