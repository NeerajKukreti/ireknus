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
    var Keyword = function (catIds) {
        var xx = $('input[name="daterange"]').val().split("-")
        var dtRange = new Date(xx[0].trim()).toDateString() + "|" + new Date(xx[1].trim()).toDateString();
       
        table = $('#tblAnn').dataTable({
            //serverSide: true,
            ajax: {
                url: getAnnounementUrl + "?catIds=" + catIds + "&showRepeated=" + $('#switch-showrpted').is(':checked') +
                    "&showFav=" + $('.showfav').is(':checked') + "&dtRange=" + dtRange + "&showAll=" + $('.ShowAll').is(':checked') +
                    "&companyName=" + $('.companyList ').val(),
                method: "GET",
                datatype: "json",
               //beforeSend: function () { $.blockUI() },
                //complete: function () { $.unblockUI() },
                error: function (err) {
                    location.reload();
                }
            },
            columns: [
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
                        var dtDessi = /\/Date\((\d*)\)\//.exec(oData.DISSEMINATION_DATE);

                        var options = { hour12: false };
                        //$(nTd).html("" +
                        //    "<b><b style='color:#4680ff'>" + new Date(+dtSub[1]).toLocaleString('en-us', options) + "</b>" +
                        //    "</br><b><b style='color:#4680ff'>" + new Date(+dtDessi[1]).toLocaleString('en-us', options) + "</b>");

                        $(nTd).html("" +
                            "<b><b style='color:#4680ff'>" + oData.NEWS_SUBMISSION_DATE_STR + "</b>" +
                            "</br><b><b style='color:#4680ff'>" + oData.DISSEMINATION_DATE_STR + "</b>");
                    }
                },
                {
                    title: "DT", data: "DISSEMINATION_DATE", className: "hidecolumn"
                },
                {
                    title: "CATEGORY", data: "CategoryName", className: ""
                },
                {
                    title: "PDF", data: "ATTACHMENT", className: "",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var color = oData.IsFavorite ? "#ff9600" : "#000000";
                        var val = oData.IsFavorite ? 0 : 1;

                        $(nTd).html("<a target='_blank' href=" + oData.ATTACHMENT + "><i class='fas fa-file-pdf'></i></a>" +
                            " | <span class='divIsFavorite' data-addnid='" + oData.ANN_ID + "' data-isfavorite='" + val + "' style=' cursor:pointer; color:" + color + "; position:relative; margin-left: -2px;'><i class='fas fa-star'></i></span>");
                    }
                }
            ],
            'iDisplayLength': 100,
            dom: 'plBfrtip',
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