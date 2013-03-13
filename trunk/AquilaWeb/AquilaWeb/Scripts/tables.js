$(function () {

    // DATA TABLES //

    $('#content_portfolio_table').dataTable();          // init datatables

    // DELETE CONFIRMATION DIALOG //

    $("#del_dialog").dialog({
        autoOpen: false,
        modal: true
    });

    var bt;
    $('.delete').click(function (e) {
        e.preventDefault();

        bt = $(this);
        $('#del_dialog').dialog({
            buttons: {
                'Confirm': function() {
                    $(this).dialog('close');
                    bt.unbind('click');
                    bt.click();
                    return true;
                },
                'Cancel': function() {
                    $(this).dialog('close');
                    return false;
                }
            }
        });

        $('p.message').text("Remove " + $(this).attr('rel') + " from the portfolio?");
        $('#del_dialog').dialog('open');
    });
});

function isNumber(n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

function changeValueInput(cell, symbol, setting) {
    edit_cell(cell, function(e){ setSettingEnter(e, cell, symbol, setting) });
}

var inputShown = false;
var tmp_cell_text;
var cell_input;
function edit_cell(cell, callback)
{
    if (!inputShown) {                                  // semaphor: currently not edited
        inputShown = true;                              // input field is visible
        tmp_cell_text = $(cell).text();                 // save old text in tmp_cell_text
        $(cell).empty();                                // remove cell content
        var input = document.createElement("input");    // create input field for editing
        input.type = "text";                            // text input

        //input.width = "10px";                         // input field width
        //input.value = tmp_cell_text;                  // fill in old cell text
        input.value = $(cell).attr("rel");
        input.id = "cell_input";                        // set input field id to "cell_input" !
        cell.appendChild(input);                        // put input field in DOM inside cell
        $(input).css("width", 80);
        input.focus();                                  // focus cursor inside input

        // input lost focus
        $(input).focusout(function () {
            cell_input = $("#cell_input").val();        // save entered text

            $(cell).empty();                            // remove input field
            $(cell).text(tmp_cell_text);                // insert old text content
            inputShown = false;                         // reset semaphor
        });

        // key pressed
        $(cell).unbind('keypress');
        $(cell).keypress(callback);
    }
}

function addSymbolEnter(e) {
    // enter pressed
    if (e.which === 13) {
        e.preventDefault();                     // no postback!
        cell_input = $("#cell_input").val();    // save entered string
        var doubleEntry = false;
        $('#content_portfolio_table>tbody>tr>td:nth-child(1)').each(function () {
            if ($(this).text().toUpperCase() == cell_input.toUpperCase()) {
                doubleEntry = true;
                alert("This symbol has already beeen added to the portfolio.");
            }
        });
        if (!doubleEntry) {
            ajax_addSymbol(cell_input);
            return false;                       // no postback!
        }
    }
}

function setSettingEnter(e, cell, symbol, setting) {
    // enter pressed
    if (e.which === 13) {
        e.preventDefault();                     // no postback!
        cell_input = $("#cell_input").val();    // save entered string
        //alert(symbol + " " + setting + " " + parseFloat(cell_input));
        if (isNumber(cell_input)) {
            ajax_setSetting(symbol, setting, parseFloat(cell_input), function (msg) {
                $(cell).empty();
                if (msg.d != null) {
                    //alert("Ajax-msg: " + msg.d);
                    $(cell).text(msg.d);
                    $(cell).attr('rel', cell_input);
                } else {
                    //$(cell).text($(cell).attr('rel'));
                    alert("Failure!");
                    $(cell).text(tmp_cell_text);
                }
                inputShown = false;
            });
        } else {
            $(cell).empty();
            $(cell).text(tmp_cell_text);
            inputShown = false;
        }
        return false;                           // no postback!
    }
}

function toggleCssClasses(e, c1, c2){
    if ($(e).hasClass(c1)) {
        $(e).removeClass(c1);
        $(e).addClass(c2);
    } else if ($(e).hasClass(c2)) {
        $(e).removeClass(c2);
        $(e).addClass(c1);
    }
}

function toggleSetting(symbol, setting, values, e, classes) {

    // WCF SetSetting
    ajax_setSetting(symbol, setting, $(e).attr('rel'), function (msg) {
        if (msg.d != null) {
            //alert(msg.d);

            if ($(e).hasClass("auto")) $(e).text("Auto");
            if ($(e).hasClass("manual")) $(e).text("Manual");
            if ($(e).hasClass("inactive")) $(e).text("Inactive");
            if ($(e).hasClass("running")) $(e).text("Active");

            var value;
            // setting value
            for (var i = 0; i < values.length; i++) {
                if (values[i] + "" == $(e).attr('rel')) {
                    if (i != values.length - 1) {
                        value = values[i + 1];
                    } else {
                        value = values[0];
                    }
                    break;
                }
            }
            // change rel action for onclick
            $(e).attr('rel', value);

            // css class
            for (var i = 0; i < classes.length; i++) {
                if ($(e).hasClass(classes[i])) {
                    $(e).removeClass(classes[i]);
                    if (i != values.length - 1) {
                        $(e).addClass(classes[i + 1]);
                    } else {
                        $(e).addClass(classes[0]);
                    }
                    break;
                }
            }
        } else {
            alert("failure!");
        }
    });

    
}

function ajax_setSetting(symbol, setting, value, callback) {
    $.ajax({
        type: "POST",
        url: "Portfolio.aspx/SetSetting",
        data: "{ 'symbol' : '"+symbol+"', 'setting' : '"+setting+"', 'value':'"+value+"'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            callback(msg);
        }
    });
}

function ajax_addSymbol(symbol) {
    $.ajax({
        type: "POST",
        url: "Portfolio.aspx/LoadPortfolioElement",
        data: "{'symbol':'" + symbol + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            if (msg.d != null) {
                window.location.reload();
            } else {
                alert("No database entry for this symbol!");
            }
        }
    });
}

    function ajax_removeSymbol(symbol) {
        $.ajax({
            type: "POST",
            url: "Portfolio.aspx/RemovePortfolioElement",
            data: "{'symbol':'" + symbol + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) { }
        });
    }

    //function addPortfolioRow(pfElement) {
    //    if (pfElement != null) {
    //        // alert(pfElement.Symbol + ": " + pfElement.Position);
    //        $('#content_portfolio_table > tbody:last').append('<tr>' +
    //                                                    '<td>' + pfElement.Symbol + '</td>' +
    //                                                    '<td>' + pfElement.Close + '</td>' +
    //                                                    '<td>' + pfElement.Position + '</td>' +
    //                                                    '<td>' + pfElement.Gain + '</td>' +
    //                                                    '<td>' + pfElement.Maxinvest + '</td>' +
    //                                                    '<td>' + pfElement.Cutloss + '</td>' +
    //                                                    '<td>' + pfElement.Decision + '</td>' +
    //                                                    '<td>' + pfElement.Roi + '</td>' +
    //                                                    '<td>' + pfElement.Auto + '</td>' +
    //                                                    '<td>' + pfElement.Active + '</td>' +
    //                                                    '</tr>');
    //        $("input").blur();
    //    } else {
    //        alert("No database entry for this symbol!");
    //    }
    //}