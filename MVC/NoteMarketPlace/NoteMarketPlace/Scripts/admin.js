/*      Mobile Menu     */

$(function () {

    $("#mobile-nav-open-btn").click(function () {
        $("#mobile-nav").css("width", "100%");
        $("#mobile-nav-close-btn").css("display", "block");
        $("#mobile-nav-open-btn").css("display", "none");
      });
    
      $("#mobile-nav-close-btn").click(function () {
        $("#mobile-nav").css("width", "0%");
        $("#mobile-nav-close-btn").css("display", "none");
        $("#mobile-nav-open-btn").css("display", "block");
      });
 
});


// Profile

$("#upload_profile_img_click").click(function () {
    $("#ProfileImage").click();
});


// system config

$("#upload_def_profile_click").click(function () {
    $("#DefaulDpImg").click();
});

$("#upload_def_note_click").click(function () {
    $("#DefaultNoteImg").click();
});



/*  Dashboard  */

$(function () {

    var table = $('#dashboard #dashboard-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#dashboard #dashboard-search-click').click(function () {
        table.search($('#dashboard #dashboard-search-input').val()).draw();
    });

});


/*  Notes Under Review  */

$(function () {

    var table = $('#notes-under-review #noteunderreview-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#notes-under-review #noteunderreview-search-click').click(function () {
        table.search($('#notes-under-review #noteunderreview-search-input').val()).draw();
    });

});


/*  Downloaded Notes */

$(function () {

    var table = $('#downloaded-notes #downloadednotes-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#downloaded-notes #downloadednotes-search-click').click(function () {
        table.search($('#downloaded-notes #downloadednotes-search-input').val()).draw();
    });

});


/*  manage Category */

$(function () {

    var table = $('#manage-category #managecategory-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#manage-category #managecategory-search-click').click(function () {
        table.search($('#manage-category #managecategory-search-input').val()).draw();
    });

});


/*  manage Country */

$(function () {

    var table = $('#manage-country #managecountry-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#manage-country #managecountry-search-click').click(function () {
        table.search($('#manage-country #managecountry-search-input').val()).draw();
    });

});


/*  manage Type */

$(function () {

    var table = $('#manage-type #managetype-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#manage-type #managetype-search-click').click(function () {
        table.search($('#manage-type #managetype-search-input').val()).draw();
    });

});


/*  member details */

$(function () {

    var table = $('#member-details #memberdetails-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="/img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="/img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

});


/*  members */

$(function () {

    var table = $('#members #members-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#members #members-search-click').click(function () {
        table.search($('#members #members-search-input').val()).draw();
    });

});


/*  Published Notes */

$(function () {

    var table = $('#published-notes #publishednotes-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#published-notes #publishednotes-search-click').click(function () {
        table.search($('#published-notes #publishednotes-search-input').val()).draw();
    });

});


/*  rejected Notes */

$(function () {

    var table = $('#rejected-notes #rejectednotes-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#rejected-notes #rejectednotes-search-click').click(function () {
        table.search($('#rejected-notes #rejectednotes-search-input').val()).draw();
    });

});


/*  Spam Reports */

$(function () {

    var table = $('#spam-reports #spamnotereports-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#spam-reports #spamnotereports-search-click').click(function () {
        table.search($('#spam-reports #spamnotereports-search-input').val()).draw();
    });

});


/*  Manage Admin */

$(function () {

    var table = $('#manage-admin #manageadmin-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#manage-admin #manageadmin-search-click').click(function () {
        table.search($('#manage-admin #manageadmin-search-input').val()).draw();
    });

});



function applyDataTable() {

    var table = $('#downloaded-notes #downloadednotes-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        },
        'columnDefs': [
            { orderable: false, targets: ['nosort'] }
        ]
    });

    $('#downloaded-notes #downloadednotes-search-click').click(function () {
        table.search($('#downloaded-notes #downloadednotes-search-input').val()).draw();
    });

}