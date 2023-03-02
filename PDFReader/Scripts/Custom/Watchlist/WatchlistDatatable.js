var WatchlistTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Watchlist = function () {
        table = $('#WatchlistTable');

        
        // begin first table
        table.dataTable({
            //"autowidth": "true",
            //"scrollX": true,
            "ajax": {
                "url": LoadWatchlist,
                "type": "GET",
                "datatype": "json"
            },//
            "columns": [

                {
                    "title": "COMPANY_ID", "data": "COMPANY_ID",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var labelcompanyid = "<label id='lbl_" + oData.COMPANY_ID + "' for='L" + oData.COMPANY_ID + "'>" + oData.COMPANY_ID + "</label>";
                        $(nTd).html(labelcompanyid);
                    }
                },
                {
                    "title": "COMPANY_NAME", "data": "COMPANY_NAME",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var labelcompanyname = "<label id='lbl_cn" + oData.COMPANY_ID + "' for='Lcn" + oData.COMPANY_ID + "'>" + oData.COMPANY_NAME + "</label>";
                        $(nTd).html(labelcompanyname);
                    }
                },
                {
                    "title": "EDIT", "data": "COMPANY_ID",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var conf = 'return confirm("Are you sure to delete this record?")';
                        var editbtn = "&nbsp; <i data-id='" + oData.COMPANY_ID + "' id='btnedit_" + oData.COMPANY_ID + "' class='icon feather icon-edit f-w-600 f-16 m-r-15 text-c-green clsedit' ToolTip='Edit'></i>";
                        var delbtn = "<i data-id='" + oData.COMPANY_ID + "' id='btndel_" + oData.COMPANY_ID + "' class='icon feather icon-trash-2 f-w-600 f-16 m-r-15 text-c-red clsdel' ToolTip='Delete' onclick='" + conf + "'></i>";
                        $(nTd).html(editbtn + delbtn);
                    }
                },

               
            ],
            dom: 'lBfrtip',
            buttons: ['excel', 'csv'],
        });
    }

    return {
        //main function to initiate the module
        init: function () {
            if (!jQuery().dataTable) {
                return;
            }

            Watchlist();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }
    };
}();

jQuery(document).ready(function () {
    WatchlistTables.init();
});

$(document).ready(function () {
    document.title = "Watchlist";

    var table = $('#WatchlistTable').DataTable();

    $('#WatchlistTable tbody').on('click', '.LableText', function () {
        $('.LableText').removeClass("hide_column");
        $('.clstext').addClass("hide_column");
        $('.clsedit').addClass("hide_column");
        $('.clsdel').addClass("hide_column");

        var id = $(this).attr("data-id");
        $('#txt_' + id).removeClass("hide_column");
        $('#lbl_' + id).addClass("hide_column");

        $('#btnok_' + id).removeClass("hide_column");
        $('#btndel_' + id).removeClass("hide_column");
        //alert('id is'+id);
    });

    $('#WatchlistTable tbody').on('click', '.clsdel', function () {
        
        var id = $(this).attr("data-id");
        
        $.ajax({
            url: InsertWatchlist,
            type: "POST",
            //contentType: "json",
            //processData: false,
            dataType: "json",
            data: {
                COMPANY_ID: id,
                COMPANY_NAME: '',
                ACTION: '3'
            },
            beforeSend: function () {
                $.blockUI();
                //App.blockUI({ // sample ui-blockui.js
                //    animate: true,
                //    cenrerY: true,

                //});
            },
            success: function (msg) {
                //$(frmId + " input#ExamId").val(Examid);
                //ShowNotification("success", "Record Saved", "Exam# " + Examid + " Saved Successfully");
                alert('Deleted Successfully');
                WatchlistTables.reloadTable(); //reloads Exam datatable
            },
            error: function (err) {
                //ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                //App.unblockUI();
                $.unblockUI();
                alert("error", "Error occured", err.status + " " + err.statusText);
            },
            complete: function () {
                $.unblockUI();
            }
        });
    });

    $('#WatchlistTable tbody').on('click', '.clsedit', function () {
        
        var watchlistid = $(this).attr("data-id");
        var companyid = $('#lbl_' + watchlistid).text();
        var companyname = $('#lbl_cn' + watchlistid).text();
        $('#COMPANY_ID').val(companyid);
        $('#COMPANY_NAME').val(companyname);
        $('#ACTION').val('2');
        $('#COMPANY_ID').attr('disabled', 'disabled');

        //alert('hi');
    });

    $(document).on('click', '#btnsubmit', function () {
        

        var insertcompanyid = $('#COMPANY_ID').val();
        var companyname = $('#COMPANY_NAME').val();
        var txtaction = $('#ACTION').val();
        //$('#lbl_' + id).addClass("hide_column");

        //$('#btnok_' + id).removeClass("hide_column");
        //$('#btndel_' + id).removeClass("hide_column");
        //alert('id is'+id);

        //alert(id + '-' + watchlist);

        $.ajax({
            url: InsertWatchlist,
            type: "POST",
            //contentType: "json",
            //processData: false,
            dataType: "json",
            data: {
                COMPANY_ID: insertcompanyid,
                COMPANY_NAME: companyname,
                ACTION: txtaction
            },
            beforeSend: function () {
                $.blockUI();
                //App.blockUI({ // sample ui-blockui.js
                //    animate: true,
                //    cenrerY: true,

                //});
            },
            success: function (msg) {
                alert("Data Add/Edited Successfully");
                WatchlistTables.reloadTable(); //reloads Exam datatable
                //notify(nFrom, nAlign, nIcons, nType, nAnimIn, nAnimOut);

                $('#ACTION').val('1');
                $('#COMPANY_ID').removeAttr('disabled');
            },
            error: function (err) {
                //ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                $.unblockUI();
                alert("error", "Error occured", err.status + " " + err.statusText);
            },
            complete: function () {
                $.unblockUI();
                $('#COMPANY_ID').val('');
                $('#COMPANY_NAME').val('');
            }
        });
    });

    $(document).on('click', '#btnImport', function () {
        if ($("#uploadfile").get(0).files.length == 0) {
            alert('Please Select Excel File');
        }
        else {
            var frmData = new FormData(document.querySelector("#frm_watchlist"));
            var filebase = $("#uploadfile").get(0);
            var files = filebase.files;

            if (filebase.files.length) {
                frmData.append(files[0].name, files[0]);
            }

            $.ajax({
                url: uploadWatchlistFileUrl,
                type: "POST",
                contentType: false,
                processData: false,
                data: frmData,
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (result) {
                    //$(frmId + " input#ExamId").val(Examid);
                    //ShowNotification("success", "Record Saved", "Exam# " + Examid + " Saved Successfully");
                    //ExamTable.reloadTable(); //reloads Exam datatable
                    alert(result);
                },
                error: function (err) {
                    //ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                    //App.unblockUI();
                    $.unblockUI();
                    alert(err.status + " " + err.statusText);
                },
                complete: function () {
                    $.unblockUI();
                    WatchlistTables.reloadTable();
                }
            });
        }
    });
});