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