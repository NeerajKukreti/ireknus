$(document).ready(function() {
    setTimeout(function() {
        // [ left-right-fix
        
        var table = $('#right-fix').DataTable({
            scrollY: "300px",
            scrollX: true,
            scrollCollapse: true,
            paging: false,
            fixedColumns: {
                leftColumns: 0,
                rightColumns: 1
            }
        });

    }, 350);
});
