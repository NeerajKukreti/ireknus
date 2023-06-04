$(document).ready(function () {

    $("#ex_category").hide();
    $("#ex_company").hide();


    $("#ex_cat").click(function () {
        $("#ex_category").toggle(1000);
    });

    $("#ex_com").click(function () {
        $("#ex_company").toggle(1000);
    });

    $("#ddl_KeywordSet").select2({
        placeholder: "Select keyword set"
    });

    $('.ddlCompany').select2({
        placeholder: {
            id: '-1', // the value of the option
            text: 'Select Company'
        },
        width: 1050,
        tags: true,
        closeOnSelect: false,
        query: function (query) {
            var data = { results: [] };
            for (var i = 0; i < fruits.length; i++) {
                data.results.push({ "id": fruits[i] + "/" + counter, "text": fuits[i] });
            }
            counter += 1;
            query.callback(data);
        },
        formatSelection: function (item) {
            return item.text; // display apple, pear, ...
        },
        formatResult: function (item) {
            return item.id; // display apple/1, pear/2, ... Return item.text to see apple, pear, ...
        }
    })


    $.ajax({
        type: "GET",
        url: GetAllCompanies,
        dataType: "json",
        success: function (res) {
            var div_data = "";
            $.each(res, function (i, data) {

                div_data = "<option value=" + data.value + ">" + data.text + "</option>";

                $(div_data).appendTo('.ddlCompany');
            });

        }

    });
});

function LoadCounts(year) {
    $.ajax({
        url: reportDataUrl,
        type: 'GET',
        data: { FinancialYear: year },
        beforeSend: function () { $('#spinnertReportCount').show(); },
        success: function (report) {
            //$('#lblcnt').text(report.Processed.length + "/" + report.UnProcessed.length);
            $('#lblcnt').show();
            $('#aprocessed').text(report.Processed.length);
            $('#aunprocessed').text(report.UnProcessed.length);
            $("#btnExecuteSearch").css({ display: "inline-flex" });
            $("#btnDeleteReport").css({ display: "inline-flex" });
            $("#btnDeleteCompany").css({ display: "inline-flex" });

            $("#ReportTable_wrapper").show();

            ReportTables.destroy();
            ReportTables.init($('#ddl_financialyear').val(), new Date().toLocaleString('en-us'), new Date().toLocaleString('en-us'));

            $.ajax({
                url: GetSkippedCount,
                type: 'GET',
                data: { Year: $('#ddl_financialyear').val() },
                success: function (report) {
                    $('#ContainsImg').text(report.ImgSkipped);
                    $('#NonImg').text(report.NonImgSkipped);
                    $('#spinnertReportCount').hide();
                },
                error: function () { alert('error occured while trying to get the skipped count') }
            });
        },
        error: function () { alert('error occured while trying to get the report data') }
    });
}

$(document).on('change', '#ddl_financialyear', function () {
    if ($(this).val() != "Select") {
        $("#lblDtRange").show();
    }
    else { $("#lblDtRange").hide(); }

    LoadCounts($(this).val());
});

$(document).on('click', '#btnExecuteSearch', function () {

    if ($('#ddl_KeywordSet').val().length == 0) {
        $('#ddl_KeywordSet').next().addClass('cstErrAlert');
        return;
    }
    else {
        $('#ddl_KeywordSet').next().removeClass('cstErrAlert');
    }

    $.ajax({
        url: performSearch,
        type: 'GET',
        data: { Year: $('#ddl_financialyear').val(), KeywordSetId: $("#ddl_KeywordSet").val().join() },
        beforeSend: function () {
            $('#spinnertReportExecute').show();
        },
        success: function (report) {
            $('#spinnertReportExecute').hide();

            LoadCounts($('#ddl_financialyear').val());
        },
        error: function () { alert('error occured while trying to search') },
        complete: function () {
            ReportTables.reloadTable();
            //UpReportTables.reloadTable();
        }
    });
});

$(document).on('click', '#btnDeleteReport', function () {
    if (confirm('Are you sure you want to delete selected year report?')) {
        $.ajax({
            url: DeleteReport,
            type: 'GET',
            data: { Year: $('#ddl_financialyear').val() },
            beforeSend: function () { $('#spinnertRptDelete').show(); },
            success: function (report) {
                $('#spinnertRptDelete').hide();
                LoadCounts($('#ddl_financialyear').val());
            },
            error: function () { alert('error occured while trying to search') }
            ,
            complete: function () {
                ReportTables.reloadTable();
                //UpReportTables.reloadTable();
            }
        });
    }
});

$(document).on('click', '#btnDeleteCompany', function () {
    if (confirm('Are you sure you want to delete selected year company list?')) {
        $.ajax({
            url: DeleteCompany,
            type: 'GET',
            data: { Year: $('#ddl_financialyear').val() },
            beforeSend: function () { $('#spinnertCompanyDelete').show(); },
            success: function (report) {
                $('#spinnertCompanyDelete').hide();
                LoadCounts($('#ddl_financialyear').val());
            },
            error: function () { alert('error occured while trying to search') }
            ,
            complete: function () {
                ReportTables.reloadTable();
                //UpReportTables.reloadTable();
            }
        });
    }
});

$(document).on('click', '.btnPageTextDownload', function () {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent($(this).data('pagetext')));
    element.setAttribute('download', $(this).data('filename') + ".txt");

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
});

$(document).on('click', '#chkComplexSearchEnable', function () {
    
    if ($(this).is(":checked")) {
        $("#oldBlock").hide();
        $(".complexsearchblock").show();
    }
    else {
        $(".complexsearchblock").hide();
        $("#oldBlock").show();
    }

});

function addColumn(str, column) {
    if (/^(like|not like)/i.test(str)) {
        return `${column} ${str}`;
    } else {
        return `${column} = ${str}`;
    }
}


function transformQuery(query) {
    

    // Replace 'or like' with 'or column like'
    query = query.replace(/\bor\s+like\b/gi, 'or column like');

    // Replace 'or not like' with 'or column not like'
    query = query.replace(/\bor\s+not\s+like\b/gi, 'or column not like');

    // Replace 'and' with 'and column'
    query = query.replace(/\band\b/gi, 'and column');

    // Replace 'and like' with 'and column like'
    query = query.replace(/\band\s+like\b/gi, 'and column like');

    // Replace 'and not like' with 'and column not like'
    query = query.replace(/\band\s+not\s+like\b/gi, 'and column not like');

    // Replace 'or' with 'or column'
    query = query.replace(/\bor\b/gi, 'or column =');

    return query;
}

function t(str, columnName) {
    var orflag = false;
    var andflag = false;

    if (str.toLowerCase().includes('and not like')) {
        str = str.toLowerCase().replaceAll('and not like', `and ${columnName} not like`);
        andflag = true;
    }

    if (str.toLowerCase().includes('and like')) {
        str = str.toLowerCase().replaceAll('and like', `and ${columnName} like`);
        andflag = true;
    }

    if (str.toLowerCase().includes('or like')) {
        str = str.toLowerCase().replaceAll('or like', `or ${columnName} like`);
        orflag = true;
    }

    if (str.toLowerCase().includes('or not like')) {
        str = str.toLowerCase().replaceAll('or not like', `or ${columnName} not like`);
        orflag = true;
    }

    if (!orflag) { 
        if (str.toLowerCase().includes('or')) {
            //str = str.toLowerCase().replace(/\bor\b/gi,`or ${columnName} = `);
            str = str.toLowerCase().replace(/'[^']*'|\band\b/ig, function (match) {
                if (match.toLowerCase() === 'or') {
                    return `or ${columnName} = `;
                } else {
                    return match;
                }
            });
        }
    }

    if (!andflag) {
        if (str.toLowerCase().includes('and')) {
            /*str = str.toLowerCase().replace(/\band\b/gi, `and ${columnName} = `);*/
            str = str.toLowerCase().replace(/'[^']*'|\band\b/ig, function (match) {
                if (match.toLowerCase() === 'and') {
                    return `and ${columnName} = `;
                } else {
                    return match;
                }
            });
        }
    }
    return str;
}
var finalText = "";
$(document).on('click', '#btnSearch', function () {
    if ($("#txtYear").val().length == 0 && $("#txtCompany").val().length == 0 && $("#txtKeyword").val().length == 0)
        return;

    /* Year */
    var yearList1 = $("#txtYear").val().replace(/\s+/g, " ").trim();
    var yearText = "";

    if (yearList1.length > 0) {
        var xx1 = t(yearList1, "FinancialYear");
        yearText = addColumn(xx1, "FinancialYear");
    }

    /* Company */
    var companyList1 = $("#txtCompany").val().replace(/\s+/g, " ").trim();
    var companyText = "";

    if (companyList1.length > 0) {
        var xx1 = t(companyList1, "CompanyName");
        companyText = addColumn(xx1, "CompanyName");
    }

    /* Keyword */
    var keyList1 = $("#txtKeyword").val().replace(/\s+/g, " ").trim();
    var keyText = "";

    if (keyList1.length > 0) {        
        var xx1 = t(keyList1, 'FoundKeywords');
        keyText = addColumn(xx1, "FoundKeywords");
    }

    finalText = "";
    finalText = "where ";

    if (yearText.length > 0)
        finalText = finalText.concat(`(${yearText})`);

    if (companyText.length > 0) {
        if (yearText.length > 0)
            finalText = finalText.concat(` and (${companyText})`);
        else
            finalText = finalText.concat(` (${companyText})`);
    }

    if (keyText.length > 0) {
        if (companyText.length > 0)
            finalText = finalText.concat(` and (${keyText})`);
        else if (yearText.length > 0)
            finalText = finalText.concat(` and (${keyText})`);
        else
            finalText = finalText.concat(` (${keyText})`);
    }
    
    //var finalText = `where (${yearText}) and (${companyText}) and (${keyText})`;

    if ($("#chkIncludeDate").is(":checked")) {
        var dtFrm = (new Date($('.dtCtrl').data('daterangepicker').startDate._d)).toISOString();
        var dtTo = (new Date($('.dtCtrl').data('daterangepicker').endDate._d)).toISOString();
        dtFrm = moment(dtFrm).format('YYYY/MM/DD');
        dtTo = moment(dtTo).format('YYYY/MM/DD');
        finalText = finalText.concat(` and insertdate between '${dtFrm}' and '${dtTo}'`);
    }

    //ReportTables.reloadTable();
    //ReportTables.init($('#ddl_financialyear').val(), encodeURI(finalText), $("#chkComplexSearchEnable").is(":checked"));
    $('#ReportTable').DataTable().ajax.reload();

});

function getQueryText(){
    return finalText;
}