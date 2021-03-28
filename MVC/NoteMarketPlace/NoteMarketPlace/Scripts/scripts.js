/*      Login Page      */

$(".toggle-password").click(function () {

  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});


/* forgot password  */

$('.elements #forgot-pwd').click(function () {
    location.href = "Forgot_Password";
});




// header
function sticky_header() {

  var header_height = $('.nav').innerHeight() / 2;
  var scrollTop = $(window).scrollTop();
  if (scrollTop > header_height) {
    $('#home-header nav').removeClass('transparant-nav');
    $('#home-header .navbar-brand img').attr('src', 'img/logo.png');
  } else {
    $('#home-header nav').addClass('transparant-nav');
    $('#home-header .navbar-brand img').attr('src', 'img/top-logo.png');
  }

}

$(document).ready(function () {
  sticky_header();
});
$(window).scroll(function () {
  sticky_header();
});
$(window).resize(function () {
  sticky_header();
});


/*      Mobile Menu     */

$(function () {

  $("#mobile-nav-open-btn").click(function () {

    $("#mobile-nav").css('width', '100%');
    $("#home-header nav").removeClass('transparant-nav');
    $("#home-header .navbar-brand img").attr('src', 'img/logo.png');
    $("#mobile-nav-close-btn").css('display', 'block');
    $("#mobile-nav-open-btn").css('display', 'none');
  });

  $("#mobile-nav-close-btn").click(function () {

    $("#mobile-nav").css('width', '0%');
    $("#home-header nav").addClass('transparant-nav');
    $("#home-header .navbar-brand img").attr('src', 'img/top-logo.png');
    $("#mobile-nav-close-btn").css('display', 'none');
    $("#mobile-nav-open-btn").css('display', 'block');
  });

});


/*    Add Notes    */

$(function () {

    if ($('input[name="SellType"]:checked').val() == 'Paid') {
        $('#Price').prop('disabled', false);
    } else {
        $('#Price').prop('disabled', true);
    }
    

    $('input[name="SellType"]').click(function () {

        if ($('input[name="SellType"]:checked').val() == 'Free')
        {
            $('#Price').prop('disabled', true);
        } else
        {
            $('#Price').prop('disabled', false);
        }
  });

});


// add notes file opens
//$("#upload_dp_click").click(function () {
//    $("#DisplayPicture").click();
//});
//$("#upload_note_click").click(function () {
//    $("#UploadNotes").click();
//});
//$("#upload_preview_click").click(function () {
//    $("#NotePreview").click();
//});









/*   Registered Users   */

// Buyer Request

$(function () {

    var table = $('#buyer-request .table').DataTable({
        'pageLength': 10,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        }
    });

    $('#buyer-request .btn-search').click(function () {
        table.search($('#buyer-request #search-box').val()).draw();
    });

});



//  My Profile

$("#upload_profile_img_click").click(function () {
    $("#ProfilePicture").click();
});





//Dashboard

$(function () {

    // published notes
    var table = $('#dashboard #published-notes-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        }
    });

    $('#published-notes-search').click(function () {
        table.search($('#published-notes-input').val() ).draw();
    });

    // progress notes
    var table1 = $('#dashboard #progress-notes-table').DataTable({
        'pageLength': 5,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        }
    });

    $('#progress-notes-search').click(function () {
        table1.search($('#progress-notes-input').val()).draw();
    });
    
});


/*   MY Downloads   */

$(function () {
    var table = $('#my-downloads #my_downloads_table').DataTable({
        'pageLength': 10,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        }
    });

    $('#mydownload_search').click(function () {
        table.search($('#mydownload_search_input').val()).draw();
    });

});


/*   MY Sold Notes   */

$(function () {
    var table = $('#sold-notes #mysoldnote_table').DataTable({
        'pageLength': 10,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        }
    });

    $('#mysoldnote_search').click(function () {
        table.search($('#mysoldnote_search_input').val()).draw();
    });

});


/*   MY Rejected Notes   */

$(function () {
    var table = $('#rejected-notes #rejectednote_table').DataTable({
        'pageLength': 10,
        'dom': '<t><"paging"p>',
        'language': {
            'paginate': {
                'previous': '<span aria-hidden="true"> <img src="../img/icons/left-arrow.png" alt="previous"> </span>',
                'next': '<span aria-hidden="true"> <img src="../img/icons/right-arrow.png" alt="next"> </span>'
            }
        }
    });

    $('#rejectednote_search').click(function () {
        table.search($('#rejectednote_search_input').val()).draw();
    });

});