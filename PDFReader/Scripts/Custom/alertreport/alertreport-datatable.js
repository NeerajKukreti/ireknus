var buttonCommon = {
    exportOptions: {
        format: {
            body: function (data, row, column, node) {

                var options = { hour12: false };
                if (column === 4) {
                    var dtSub = /\/Date\((\d*)\)\//.exec(data);
                    return new Date(+dtSub[1]).toLocaleString('en-in', options)
                }
                else if (column === 5) {
                    var dtSub = /\/Date\((\d*)\)\//.exec(data);
                    return new Date(+dtSub[1]).toLocaleString('en-in', options)
                }
                else return data

            }
        }
    }
};

var annTable = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    
    var Keyword = function (alertName,alertDate) {
        table = $('#tblAlertReport').dataTable({
            autoWidth: false,
            retrieve: true,
            ajax: {
                url: alertReportListUrl + "?alertName=" + alertName + "&alertDate=" + alertDate,
                method: "GET",
                datatype: "json",
                error: function (err) {
                    location.reload();
                }
            },
            columns: [
                { title: "ALERT", data: "FinancialYear", className: "dt-w2" },
                { title: "COMPANY", data: "CompanyName", className: "dt-w2" },
                {
                    title: "SUBJECT", data: "news_subject", className: "dt-w3 brkwd",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        $(nTd).html("<a target='blank' href='" + oData.Url + "#page=" + oData.PDFPageNumber + "' style='cursor:pointer;'>" + oData.news_subject + "</div>");
                    }
                },
                //{
                //    title: "KEYWORD-PAGE", data: "FoundKeywords", className: "dt-w4",
                //    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                //        $(nTd).html(oData.FoundKeywords + " - " + oData.PDFPageNumber);
                //    }
                //},
                
                { title: "PAGE NUMBER", data: "PDFPageNumber", className: "dt-w1" },
                { title: "KEYWORDS", data: "FoundKeywords", className: "dt-w2" },
                { title: "TOTAL PAGES", data: "TotalPages", className: "dt-w1" },
            ],
            "sAjaxDataProp": "",
            responsive: true,
            'iDisplayLength': 100,
            dom: 'lBfrtip',
            buttons: [

                $.extend(true, {}, buttonCommon, {
                    extend: 'csvHtml5',
                    customize: function (xlsx, test) {
                    }
                }),

                $.extend(true, {}, buttonCommon, {
                    extend: 'excelHtml5'
                    ,
                    customize: function (xlsx, test) {

                        var sheet = xlsx.xl.worksheets['sheet1.xml'];

                        //Loop over all cells in sheet
                        $('row c', sheet).each(function () {

                            // if cell starts with http
                            if ($('is t', this).text().indexOf("http") === 0) {

                                // (2.) change the type to `str` which is a formula
                                $(this).attr('t', 'str');
                                //append the formula
                                $(this).append('<f>' + 'HYPERLINK("' + $('is t', this).text() + '","' + $('is t', this).text() + '")' + '</f>');
                                //remove the inlineStr
                                $('is', this).remove();
                                // (3.) underline
                                $(this).attr('s', '4');
                            }
                        });
                    }
                })
            ],
            "lengthMenu": [[25, 50, 100, -1], [25, 50, 100, "All"]],

        });
    }

    return {
        //main function to initiate the module
        init: function (alertName,alertDate) {
            if (!jQuery().dataTable) {
                return;
            }

            Keyword(alertName, alertDate);
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

$(document).on('click', '#btnsubmit', function () {
    debugger;
    var aname = $('#ALERT_ID option:selected').text();
    var adate = $('#dtAlertRpt').val();

    if (adate == "") {
        alert('Please Select Date');
    }
    else {
        annTable.destroy();
        annTable.init(aname, adate);
    }

});

$(document).ready(function () {
    n = new Date();
    y = n.getFullYear();
    m = n.getMonth() + 1;
    d = n.getDate();
    //debugger;
    $("#dtAlertRpt").val(y + "-" + m + "-" + d);
       
    //annTable.init("","");
});