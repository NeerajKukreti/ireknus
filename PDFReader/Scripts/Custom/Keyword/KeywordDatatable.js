'use strict';
$(window).on('load', function () {
    function notify(message, type) {
        $.notify({
            message: message
        }, {
            type: type,
            allow_dismiss: false,
            label: 'Cancel',
            className: 'btn-xs btn-inverse',
            placement: {
                from: 'bottom',
                align: 'right'
            },
            delay: 2500,
            animate: {
                enter: 'animated fadeInRight',
                exit: 'animated fadeOutRight'
            },
            offset: {
                x: 30,
                y: 30
            }
        });
    };
    //notify('Welcome to Notification page', 'inverse');
});

var KeywordTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Keyword = function (keywordset) {
        //alert(keywordset);
        table = $('#KeywordTable').dataTable({
            retrieve: true,
            "ajax": {
                "url": LoadKeywords,
                "type": "GET",
                "datatype": "json",
                "data": { keywordSetId: keywordset }
            },
            "columns": [
                
                {
                    "title": "Keyword", "data": "KEYWORD",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var labeltext = "<label id='lbl_" + oData.KID + "' for='L" + oData.KID + "'>" + oData.KEYWORD + "</label>";
                        $(nTd).html(labeltext);
                    }
                },
                { "title": "SM_NAME", "data": "SM_NAME", },

                {
                    "title": "EDIT", "data": "KID",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var conf = 'return confirm("Are you sure to delete this record?")';
                        var editbtn = "<i data-id='" + oData.KID + "' data-smid='" + oData.SMID + "' id='btnedit_" + oData.KID + "' class='icon feather icon-edit f-w-600 f-16 m-r-15 text-c-green clsedit' ToolTip='Edit'></i>";
                        var delbtn = "<i data-id='" + oData.KID + "' id='btndel_" + oData.KID + "' class='icon feather icon-trash-2 f-w-600 f-16 m-r-15 text-c-red clsdel' ToolTip='Delete' onclick='" + conf + "'></i>";
                        $(nTd).html(editbtn + delbtn);
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
        init: function (keywordset) {
            //debugger;
            if (!jQuery().dataTable) {
                return;
            }

            Keyword(keywordset);
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        },
        destroy: function () {
            if (table != null)
                table.DataTable().destroy();
        }
    };
}();

jQuery(document).ready(function () {
    //KeywordTables.init($('SMID').val());
});

$(document).on('change', '#SMID', function () {
    //debugger;
    var smid = $(this).val();
    KeywordTables.destroy();
    KeywordTables.init(smid);
    //alert('hi');
});

$(document).ready(function () {

    document.title = "Keywords";

    $(document).on('click', '.clsedit', function () {
        debugger;
        //alert('hi');
        var kid = $(this).attr("data-id");
        var smid = $(this).attr("data-smid");
        $('#KID').val(kid);
        $('#KEYWORD').val($('#lbl_' + kid).text());
        $('#SMID').val(smid);
        $('#ACTION').val('2');
    });

    $(document).on('click', '#btnsubmit', function () {
        debugger;
        var kid = $('#KID').val();
        var insertkeyword = $('#KEYWORD').val();
        var smid = $('#SMID').val();
        var action = $('#ACTION').val();

        var msgs = '';
        if (action == '1') msgs = 'Added';
        if (action == '2') msgs = 'Updated';
        if (action == '3') msgs = 'Deleted';

        if (insertkeyword == '' || smid == '') {
            alert('Select Keyword Set Name and Keyword then proceed');
        }
        else {
            $.ajax({
                url: InsertKeywords,
                type: "POST",
                //contentType: "json",
                //processData: false,
                dataType: "json",
                data: {
                    KID: kid,
                    KEYWORD: insertkeyword,
                    SMID: smid,
                    ACTION: action
                },
                beforeSend: function () {
                    $.blockUI();
                    //App.blockUI({ // sample ui-blockui.js
                    //    animate: true,
                    //    cenrerY: true,

                    //});
                },
                success: function (msg) {
                    
                    KeywordTables.reloadTable(); //reloads Exam datatable

                    $('#KID').val('');
                    $('#KEYWORD').val('');
                    $('#SMID').val('');
                    $('#ACTION').val('1');
                    var nFrom = $(this).attr('data-from');
                    var nAlign = $(this).attr('data-align');
                    var nIcons = $(this).attr('data-notify-icon');
                    var nType = $(this).attr('data-type');
                    var nAnimIn = $(this).attr('data-animation-in');
                    var nAnimOut = $(this).attr('data-animation-out');
                    notify(nFrom, nAlign, nIcons, nType, nAnimIn, nAnimOut, "Success", "Keyword "+msgs+" Successfully");
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

                    $('#KEYWORD').val('');
                }
            });
        }
    });

    $(document).on('click', '#btnImport', function () {
        //debugger;
        var smid = $('#SMID').val();
        if (smid == '') {
            alert('Select Keyword Set Name then proceed');
        }
        else if ($("#uploadfile").get(0).files.length == 0) {
            alert('Please Select Excel File');
        }
        else {
            var frmData1 = new FormData(document.querySelector("#frm_keyword1"));
            var filebase = $("#uploadfile").get(0);
            var files = filebase.files;

            if (filebase.files.length) {
                frmData1.append(files[0].name, files[0]);
                
            }

            $.ajax({
                url: uploadKeywordFileUrl,
                type: "POST",
                contentType: false,
                processData: false,
                data: frmData1,
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
                    KeywordTables.reloadTable();
                }
            });
        }

    });

    $(document).on('click', '.clsdel', function () {
        debugger;
        var id = $(this).attr("data-id");
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
                KID: id,
                KEYWORD: '',
                SMID: '',
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
                var nFrom = $(this).attr('data-from');
                var nAlign = $(this).attr('data-align');
                var nIcons = $(this).attr('data-notify-icon');
                var nType = $(this).attr('data-type');
                var nAnimIn = $(this).attr('data-animation-in');
                var nAnimOut = $(this).attr('data-animation-out');
                notify(nFrom, nAlign, nIcons, nType, nAnimIn, nAnimOut, "Success", "Keyword Deleted Successfully");

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

    function notify(from, align, icon, type, animIn, animOut, title, message) {
        $.notify({
            icon: icon,
            title: title,
            message: message,
            url: ''
        }, {
            element: 'body',
            type: type,
            allow_dismiss: true,
            placement: {
                from: from,
                align: align
            },
            offset: {
                x: 30,
                y: 30
            },
            spacing: 10,
            z_index: 999999,
            delay: 2500,
            timer: 1000,
            url_target: '_blank',
            mouse_over: false,
            animate: {
                enter: animIn,
                exit: animOut
            },
            icon_type: 'class',
            template: '<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
                '<button type="button" aria-hidden="true" class="close" data-notify="dismiss">x</button>' +
                '<span data-notify="icon"></span> ' +
                '<span data-notify="title">{1}</span> ' +
                '<span data-notify="message">{2}</span>' +
                '<div class="progress" data-notify="progressbar">' +
                '<div class="progress-bar progress-bar-{0}" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%;"></div>' +
                '</div>' +
                '<a href="{3}" target="{4}" data-notify="url"></a>' +
                '</div>'
        });
    };
    // [ notification-button ]
    $('.notifications.btn').on('click', function (e) {
        //debugger;
        e.preventDefault();
    });
});