var finalText = "";
$(document).ready(function () {

    $("#ex_company").hide();

    $("#ex_com").click(function () {
        $("#ex_company").toggle(1000);
    });

    StateTables.init();


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
    function addColumn(str, column) {
        if (/^(like|not like)/i.test(str)) {
            return `${column} ${str}`;
        } else {
            return `${column} = ${str}`;
        }
    }


    $(document).on('click', '#btnSearch', function () {

        if ($("#txtProject").val().length == 0 && $("#txtCompany").val().length == 0)
            return;

        /* project */
        var projectList1 = $("#txtProject").val().replace(/\s+/g, " ").trim();
        var projectText = "";

        if (projectList1.length > 0) {
            var xx1 = t(projectList1, "proposal_name");
            projectText = addColumn(xx1, "proposal_name");
        }

        /* Company */
        var companyList1 = $("#txtCompany").val().replace(/\s+/g, " ").trim();
        var companyText = "";

        if (companyList1.length > 0) {
            var xx1 = t(companyList1, "Company_or_proponent");
            companyText = addColumn(xx1, "Company_or_proponent");
        }

        finalText = " and ";

        if (projectText.length > 0)
            finalText = finalText.concat(`(${projectText})`);

        if (companyText.length > 0) {
            if (projectText.length > 0)
                finalText = finalText.concat(` and (${companyText})`);
            else
                finalText = finalText.concat(` (${companyText})`);
        }

        debugger;
        $('#StateTable').DataTable().ajax.reload();
    });
});

function getQueryText() {
    return finalText;
}