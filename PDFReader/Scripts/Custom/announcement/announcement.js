$(document).ready(function () {
    function ShowAnnouncementCategoryWise() {
        var ids = $('input[type=checkbox]:checked').map(function (i, e) {
            return $(e).parent().find('label').data('cat');
        }).get().join(',');

        if (ids.length == 0) { annTable.clear(); annTable.destroy(); return; }

        annTable.destroy();
        annTable.init(ids);
    }

    $(document).on('change', '#switch-deselect', function () {
        if (this.checked) {
            $('.form-check-input').prop('checked', false);
            $(this).closest('label').find('b').text('Select all')
        }
        else {
            $('.form-check-input').prop('checked', true);
            $(this).closest('label').find('b').text('Deselect all')
        }

        ShowAnnouncementCategoryWise();

        if ($('.ShowAll').is(':checked'))
            $('.ShowAll').closest('label').find('b').text('Show watchList')
        else
            $('.ShowAll').closest('label').find('b').text('Show all')
    });

    $(document).on('click', '.form-check-input:not(.HasSubCat)', function () {
        ShowAnnouncementCategoryWise();
    });

    $(document).on('click', '.ShowAll', function () {
        $('.showfav').prop('checked', false);

        var xx = $('input[name="daterange"]').val().split("-")
        var dtRange = new Date(xx[0].trim()).toDateString() + "|" + new Date(xx[1].trim()).toDateString();

        LoadAnnouncementView(dtRange, $('.ShowAll').is(':checked'), $('.companyList').val(), $('#switch-showrpted').is(':checked'), $('.showfav').is(':checked'));

        if ($('.ShowAll').is(':checked'))
            $('.ShowAll').closest('label').find('b').text('Show WatchList')
        else
            $('.ShowAll').closest('label').find('b').text('Show All')
    });

    $(document).on('click', '.showfav', function () {
        

        var xx = $('input[name="daterange"]').val().split("-")
        var dtRange = new Date(xx[0].trim()).toDateString() + "|" + new Date(xx[1].trim()).toDateString();

        LoadAnnouncementView(dtRange, $('.ShowAll').is(':checked'), $('.companyList').val(), $('#switch-showrpted').is(':checked'), $('.showfav').is(':checked'));

        //if ($('.showfav').is(':checked'))
        //    $('.showfav').closest('label').find('b').text('Show All')
        //else
        //    $('.showfav').closest('label').find('b').text('Show Favourite')
    });

    $(document).on('click', '.lblClearSearch', function () {
        $('.showfav').prop('checked', false);
        $('.lblClearSearch').hide();

        var dtFrm = (new Date($('.dtCtrl').data('daterangepicker').startDate._d)).toISOString();
        var dtTo = (new Date($('.dtCtrl').data('daterangepicker').endDate._d)).toISOString();
        var dtRange = dtFrm + "|" + dtTo;
        LoadAnnouncementView(dtRange, $('.ShowAll').is(':checked'), '', $('#switch-showrpted').is(':checked'), $('.showfav').is(':checked'));

        $('.companyList').val('');
    });

    $(document).on('click', '.lnkCompanyId', function () {
        $('.showfav').prop('checked', false);

        var dtFrm = (new Date($('.dtCtrl').data('daterangepicker').startDate._d)).toISOString();
        var dtTo = (new Date($('.dtCtrl').data('daterangepicker').endDate._d)).toISOString();
        var dtRange = dtFrm + "|" + dtTo;

        $('.companyList').val($(this).data('company'));
        $('.lblClearSearch').show();

        LoadAnnouncementView(dtRange, $('.ShowAll').is(':checked'), $(this).data('company'), $('#switch-showrpted').is(':checked'), $('.showfav').is(':checked'));
    });

    function LoadAnnouncementView(dtRange, showAll, CompanyName, showRepeated, showFav) {
        $.ajax({
            url: getAnnouncementView + "?CompanyName=" + CompanyName + "&ShowAll=" + showAll + "&DateRange=" + dtRange + "&ShowRepeated=" + showRepeated + "&showFav=" + showFav,
            type: 'GET',
            beforeSend: function () { $.blockUI() },
            success: function (res) {
                $('.AnnouncementView').empty();
                $('.AnnouncementView').html(res);
            },
            error: function () { },
            complete: function () { $.unblockUI() }
        });
    }

    $(document).on('click', '.hasSubcategories', function () {
        var catid = $(this).data('catid');

        if ($('.parent' + catid).is(':visible')) {
            $('.parent' + catid).hide();
        }
        else $('.parent' + catid).show();

    });

    $(document).on('change', '.showrpted', function () {
        $('.showfav').prop('checked', false);

        var dtFrm = (new Date($('.dtCtrl').data('daterangepicker').startDate._d)).toISOString();
        var dtTo = (new Date($('.dtCtrl').data('daterangepicker').endDate._d)).toISOString();
        var dtRange = dtFrm + "|" + dtTo;

        if (this.checked) {
            LoadAnnouncementView(dtRange, $('.ShowAll').is(':checked'), $('.companyList ').val(), true, $('.showfav').is(':checked'));
        }
        else {

            LoadAnnouncementView(dtRange, $('.ShowAll').is(':checked'), $('.companyList ').val(), false, $('.showfav').is(':checked'));
        }
    });

    $(document).on('click', '.divIsFavorite', function () {
        var val = $(this).data('isfavorite');
        var obj = this;
        $.ajax({
            url: saveFavoriteUrl + "?annId=" + $(this).data('addnid') + "&IsFavorite=" + val ,
            type: 'GET',
            success: function () {
                $(obj).css('color', val == 1 ? '#ff9600' : 'black');
                $(obj).data('isfavorite',(val == 1 ? 0 : 1));
            }
        })
    });

    $(document).on('click', '.divCatFavorite', function () {
        var annids = $(this).data('annids');
        var obj = this;
        $.ajax({
            url: saveCatFavoriteUrl + "?annIds=" + annids + "&IsFavorite=1",
            type: 'GET',
            success: function () {
                $(obj).css('color', '#ff9600');
                ShowAnnouncementCategoryWise();
            }
        })
    });
});