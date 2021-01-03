/*      Login Page      */

$(".toggle-password").click(function () {

  var input = $($(this).attr("toggle"));
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});




$("#dob").click(function(){

  $(this).datepicker();
  $(this).datepicker('show');

});





function sticky_header() {

  var header_height = $('.nav').innerHeight() / 2;
  var scrollTop = $(window).scrollTop();
  if (scrollTop > header_height) { 
    $('#home-header nav').removeClass('transparant-nav');
    $('#home-header .navbar-brand img').attr('src','img/logo.png');
  } else {
    $('#home-header nav').addClass('transparant-nav');
    $('#home-header .navbar-brand img').attr('src','img/top-logo.png');
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
    
    $("#mobile-nav").css('width','100%');
    $("#home-header nav").removeClass('transparant-nav');
    $("#home-header .navbar-brand img").attr('src','img/logo.png');
    $("#mobile-nav-close-btn").css('display','block');
    $("#mobile-nav-open-btn").css('display','none');
  });

  $("#mobile-nav-close-btn").click(function () {
    
    $("#mobile-nav").css('width','0%');
    $("#home-header nav").addClass('transparant-nav');
    $("#home-header .navbar-brand img").attr('src','img/top-logo.png');
    $("#mobile-nav-close-btn").css('display','none');
    $("#mobile-nav-open-btn").css('display','block');
  });

});


/*    Add Book    */

$(function () {

  $('.paid-price').hide();

  $('input[name="radio"]').click(function () {

    if ($('input[name="radio"]:checked').val() == 'free') {
      $('.paid-price').hide();
    } else {
      $('.paid-price').show();
    }
  });

});