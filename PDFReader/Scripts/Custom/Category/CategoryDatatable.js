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

var CategoryTables = function () {
    //TODO:: table-datatables-scroller.js use to fix scroller issue
    var table;
    var Category = function () {
        table = $('#CategoryTable');

        //debugger;
        // begin first table
        table.dataTable({
            //"autowidth": "true",
            //"scrollX": true,
            "ajax": {
                "url": LoadCategory,
                "type": "GET",
                "datatype": "json"
            },//
            "columns": [
                {
                    "title": "CATEGORY", "data": "CATEGORY",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var labeltext = "<label id='lbl_" + oData.CATEGORY_ID + "' for='L" + oData.CATEGORY_ID + "'>" + oData.CATEGORY + "</label>";
                        $(nTd).html(labeltext);
                    }
                },

                {
                    "title": "SEARCH VALUES", "data": "SEARCH_VALUES",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var labelsearch = "<label id='lbl_sv" + oData.CATEGORY_ID + "' for='Lsv" + oData.CATEGORY_ID + "'>" + oData.SEARCH_VALUES + "</label>";
                        $(nTd).html(labelsearch);
                    }
                },

                { "title": "PARENT CATEGORY", "data": "PCATEGORY", },

                {
                    "title": "PRIORITY", "data": "PRIORITY",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var txtpriority = "<input type='text' class='clspriority' data-id='" + oData.CATEGORY_ID + "' id='txt_" + oData.CATEGORY_ID + "' name='" + oData.CATEGORY_ID + "' value='" + oData.PRIORITY + "' style='width: 50px;'></input>";
                        $(nTd).html(txtpriority);
                    }
                },

                {
                    "title": "EDIT", "data": "CATEGORY_ID",
                    fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                        var conf = 'return confirm("Are you sure to delete this record?")';
                        var editbtn = "<i data-id='" + oData.CATEGORY_ID + "' data-parentid='" + oData.PARENT_ID + "' id='btnedit_" + oData.CATEGORY_ID + "' class='icon feather icon-edit f-w-600 f-16 m-r-15 text-c-green clsedit' ToolTip='Edit'></i>";
                        var delbtn = "<i data-id='" + oData.CATEGORY_ID + "' id='btndel_" + oData.CATEGORY_ID + "' class='icon feather icon-trash-2 f-w-600 f-16 m-r-15 text-c-red clsdel' ToolTip='Delete' onclick='" + conf + "'></i>";
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
        init: function () {
            if (!jQuery().dataTable) {
                return;
            }

            Category();
        },
        reloadTable: function () {
            table.DataTable().ajax.reload(null, false); // user paging is not reset on reload
        }
    };
}();

jQuery(document).ready(function () {
    CategoryTables.init();
});

$(document).ready(function () {
    document.title = "Category";

    var table = $('#CategoryTable').DataTable();

    $('#CategoryTable tbody').on('click', '.clsdel', function () {
        //debugger
        var id = $(this).attr("data-id");

        $.ajax({
            url: InsertCategory,
            type: "POST",
            //contentType: "json",
            //processData: false,
            dataType: "json",
            data: {
                CATEGORY_ID: id,
                CATEGORY: '',
                PARENT_ID: '',
                SEARCH_VALUES:'',
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
                CategoryTables.reloadTable(); //reloads Exam datatable

                var nFrom = $(this).attr('data-from');
                var nAlign = $(this).attr('data-align');
                var nIcons = $(this).attr('data-notify-icon');
                var nType = $(this).attr('data-type');
                var nAnimIn = $(this).attr('data-animation-in');
                var nAnimOut = $(this).attr('data-animation-out');
                notify(nFrom, nAlign, nIcons, nType, nAnimIn, nAnimOut, "Deleted", "Category Deleted Successfully");
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

    $('#CategoryTable tbody').on('click', '.clsedit', function () {
        //debugger;
        var catid = $(this).attr("data-id");
        var parentid = $(this).attr("data-parentid");
        $('#CATEGORY_ID').val(catid);
        $('#CATEGORY').val($('#lbl_' + catid).text());
        $('#PARENT_ID').val(parentid);
        $('#SEARCH_VALUES').val($('#lbl_sv' + catid).text());
        $('#ACTION').val('2');

        //alert('hi');
    });

    $(document).on('click', '#btnsetpriority', function () {
        
        var txtpriority = $('.clspriority').map(function () { return $(this).data('id') + "," + $(this).val(); }).get().join("|");
        
        $.ajax({
            url: UpdatePriority + "?OrderValue=" + txtpriority,
            type: "GET",
            beforeSend: function () {
                $.blockUI();
            },
            success: function (msg) {
                
                CategoryTables.reloadTable(); //reloads Exam datatable
                var nFrom = $(this).attr('data-from');
                var nAlign = $(this).attr('data-align');
                var nIcons = $(this).attr('data-notify-icon');
                var nType = $(this).attr('data-type');
                var nAnimIn = $(this).attr('data-animation-in');
                var nAnimOut = $(this).attr('data-animation-out');
                notify(nFrom, nAlign, nIcons, nType, nAnimIn, nAnimOut, "Success", "Category order set successfully");
            },
            error: function (err) {
                $.unblockUI();
                alert("error---", "Error occured", err.status + " " + err.statusText);
            },
            complete: function () {
                $.unblockUI();
                $('#CATEGORY').val('');
            }
        });

    })

    $(document).on('click', '#btnsubmit', function () {
        //debugger

        var txtcategoryid = $('#CATEGORY_ID').val();
        var txtcategory = $('#CATEGORY').val();
        var txtsearchvalues = $('#SEARCH_VALUES').val();
        if ($('#SEARCH_VALUES').val() == '') {
            txtsearchvalues = $('#CATEGORY').val();
        }
        
        var txtparentid = $('#PARENT_ID').val();
        var txtaction = $('#ACTION').val();

        //$('#lbl_' + id).addClass("hide_column");

        //$('#btnedit_' + id).removeClass("hide_column");
        //$('#btndel_' + id).removeClass("hide_column");
        //alert('id is'+id);

        //alert(id + '-' + watchlist);

        $.ajax({
            url: InsertCategory,
            type: "POST",
            //contentType: "json",
            //processData: false,
            dataType: "json",
            data: {
                CATEGORY_ID: txtcategoryid,
                CATEGORY: txtcategory,
                PARENT_ID: txtparentid,
                SEARCH_VALUES: txtsearchvalues,
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
                //$(frmId + " input#ExamId").val(Examid);
                //ShowNotification("success", "Record Saved", "Exam# " + Examid + " Saved Successfully");
                //alert('Added Successfully');
                //alert(insertcompanyid);

                $('#CATEGORY_ID').val('');
                $('#CATEGORY').val('');
                $('#PARENT_ID').val('');
                $('#SEARCH_VALUES').val('');
                $('#ACTION').val('1');

                CategoryTables.reloadTable(); //reloads Exam datatable
                var nFrom = $(this).attr('data-from');
                var nAlign = $(this).attr('data-align');
                var nIcons = $(this).attr('data-notify-icon');
                var nType = $(this).attr('data-type');
                var nAnimIn = $(this).attr('data-animation-in');
                var nAnimOut = $(this).attr('data-animation-out');
                notify(nFrom, nAlign, nIcons, nType, nAnimIn, nAnimOut, "Success", "Category Add/Edit Successfully");
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

                $('#CATEGORY').val('');
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