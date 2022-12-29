var KeywordTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function () {
        table = $('#KeywordSetTable').dataTable({
            retrieve: true,
            "ajax": {
                "url": LoadKeywords,
                "type": "GET",
                "datatype": "json"
            },
            "columns": [
                
                {
                    "title": "SETNAME", "data": "SM_NAME",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var labeltext = "<label id='lbl_" + oData.SMID + "' for='L" + oData.SMID + "' class='LableText' data-id='" + oData.SMID + "'>" + oData.SM_NAME + "</label>";
                        var inputtext = "<input type='text' id='txt_" + oData.SMID + "' name='" + oData.SMID + "' value='" + oData.SM_NAME + "' class='clstext hide_column'>";
                        var okbtn = "&nbsp; <i data-id='" + oData.SMID + "' id='btnok_" + oData.SMID + "' class='icon feather icon-save f-w-600 f-16 m-r-15 text-c-green hide_column clsedit' ToolTip='Save'></i>";
                        var delbtn = "<i data-id='" + oData.SMID + "' id='btndel_" + oData.SMID + "' class='icon feather icon-trash-2 f-w-600 f-16 m-r-15 text-c-red hide_column clsdel' ToolTip='Delete'></i>";
                        $(nTd).html(labeltext + inputtext + okbtn + delbtn);
                    }
                },
            ],
 
            dom: 'lBfrtip',
            buttons: ['excel', 'csv'],
            iDisplayLength: 100,
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
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }
    };
}();

jQuery(document).ready(function () {
    KeywordTables.init();
});

$(document).ready(function () {

    document.title = "Keyword Set Master";

    $('#KeywordSetTable tbody').on('click', '.LableText', function () {
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

    $('#KeywordSetTable tbody').on('click', '.clsdel', function () {
        //debugger
        var id = $(this).attr("data-id");
        var editkeyword = $('#txt_' + id).val();
        //$('#lbl_' + id).addClass("hide_column");

        //$('#btnok_' + id).removeClass("hide_column");
        //$('#btndel_' + id).removeClass("hide_column");
        //alert('id is'+id);

        //alert(id + '-' + keyword);

        $.ajax({
            url: InsertKeywords,
            type: "POST",
            //contentType: "json",
            //processData: false,
            dataType: "json",
            data: {
                SMID: id,
                SM_NAME: editkeyword,
                ACTION:"3"
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
                KeywordTables.reloadTable(); //reloads Exam datatable
            },
            error: function (err) {
                //ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                //App.unblockUI();
                $.unblockUI();
                alert("error", "Error occured", err.status + " " + err.statusText);
            },
            complete: function () {
                $.unblockUI();
                //App.unblockUI();

                //$('.LableText').removeClass("hide_column");
                //$('.clstext').addClass("hide_column");
                //$('.clsedit').addClass("hide_column");
                //$('.clsdel').addClass("hide_column");
                //alert("Complete");
            }
        });
    });

    $('#KeywordSetTable tbody').on('click', '.clsedit', function () {
        //debugger
        var id = $(this).attr("data-id");
        var editkeyword = $('#txt_' + id).val();
        //$('#lbl_' + id).addClass("hide_column");

        //$('#btnok_' + id).removeClass("hide_column");
        //$('#btndel_' + id).removeClass("hide_column");
        //alert('id is'+id);

        //alert(id + '-' + keyword);

        $.ajax({
            url: InsertKeywords,
            type: "POST",
            //contentType: "json",
            //processData: false,
            dataType: "json",
            data: {
                SMID: id,
                SM_NAME: editkeyword,
                ACTION:"2"
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
                alert('Edited Successfully');
                KeywordTables.reloadTable(); //reloads Exam datatable
            },
            error: function (err) {
                //ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                //App.unblockUI();
                $.unblockUI();
                alert("error", "Error occured", err.status + " " + err.statusText);
            },
            complete: function () {
                //App.unblockUI();
                $.unblockUI();

                $('.LableText').removeClass("hide_column");
                $('.clstext').addClass("hide_column");
                $('.clsedit').addClass("hide_column");
                $('.clsdel').addClass("hide_column");
                //alert("Complete");
            }
        });
    });

    $(document).on('click', '#btnsubmit', function () {
        //debugger

        var insertkeyword = $('#SM_NAME').val();
        //$('#lbl_' + id).addClass("hide_column");

        //$('#btnok_' + id).removeClass("hide_column");
        //$('#btndel_' + id).removeClass("hide_column");
        //alert('id is'+id);

        //alert(id + '-' + keyword);

        $.ajax({
            url: InsertKeywords,
            type: "POST",
            //contentType: "json",
            //processData: false,
            dataType: "json",
            data: {
                SM_NAME: insertkeyword,
                ACTION:"1"
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
                //alert('Added Successfully');
                KeywordTables.reloadTable(); //reloads Exam datatable
                //notify(nFrom, nAlign, nIcons, nType, nAnimIn, nAnimOut);
            },
            error: function (err) {
                //ShowNotification("error", "Error occured", err.status + " " + err.statusText);
                $.unblockUI();
                alert("error", "Error occured", err.status + " " + err.statusText);
            },
            complete: function () {
                $.unblockUI();

                //$('.LableText').removeClass("hide_column");
                //$('.clstext').addClass("hide_column");
                //$('.clsedit').addClass("hide_column");
                //$('.clsdel').addClass("hide_column");
                //alert("Complete");

                $('#SM_NAME').val('');
            }
        });
    });

});