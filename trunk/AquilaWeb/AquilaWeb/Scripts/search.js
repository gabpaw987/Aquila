$(function () {
    // SEARCH //

    // keyboard input
    $("#search").keyup(function () {
        if ($(this).val() != '') {
            ajax_search_symbol($(this).val());
        } else {
            $("#search_results").empty();
        }
    });

    // lost focus
    $("#search").focusout(function () {
        // hide results after 100ms (neccessary for click)
        hide_search_results(this);
    });

    // esc in search
    $("#search").keydown(function (e) {
        // 27 == esc
        if (e.keyCode == 27) {
            hide_search_results(this);
        }
    });

});

function hide_search_results(search)
{
    $(search).delay(100, "myQueue").queue("myQueue", function () {
        $(search).val("");                                  // remove text from search field
        $("#search_results").empty();                       // remove search results list
    }).dequeue("myQueue");
}

function ajax_search_symbol(str) {
    $.ajax({
        type: "POST",
        url: "Portfolio.aspx/SearchForSymbol",
        data: "{'str':'" + str + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var ul = $("#search_results").empty();
            for (var i = 0; i < msg.d.length; i++) {
                $(ul).append("<li><a href=\"Instrument.aspx?symbol=" + msg.d[i] + "\">" + msg.d[i] + "</a></li>");
            }
            // alert(msg.d);
        }
    });
}