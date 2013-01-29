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

var inputShown = false;
var tmp_cell_id;
var tmp_cell_text;
var cell_input;
function edit_cell(cell)
{
    if (!inputShown) {                                  // semaphor: currently not edited
        inputShown = true;                              // input field is visible
        tmp_cell_id = cell.id;                          // save table cell id
        tmp_cell_text = cell.innerText;                 // save old text in tmp_cell_text
        $("#"+tmp_cell_id).empty();                     // remove cell content
        var input = document.createElement("input");    // create input field for editing
        input.type = "text";                            // text input

        //input.width = "10px";                          // input field width
        input.value = tmp_cell_text;                    // fill in old cell text
        input.id = "cell_input";                        // set input field id to "cell_input" !
        cell.appendChild(input);                        // put input field in DOM inside cell
        $("#cell_input").css("width", 80);
        input.focus();                                  // focus cursor inside input

        // input lost focus
        $("#cell_input").focusout(function () {
            cell_input = $("#cell_input").val();        // save entered text        

            $("#" + tmp_cell_id).empty();               // remove input field
            $("#" + tmp_cell_id).text(tmp_cell_text);   // insert old text content
            inputShown = false;                         // reset semaphor
        });

        // key presses
        $("#" + cell.id).keypress(function (e) {
            // enter pressed
            if (e.which === 13) {
                e.preventDefault();                     // no postback!
                cell_input = $("#cell_input").val();    // save entered string
                ajax_addSymbol(cell_input);
                return false;                           // no postback!
            }
        });
    }
}

function ajax_addSymbol(symbol) {
    $.ajax({
        type: "POST",
        url: "Portfolio.aspx/LoadPortfolioElement",
        data: "{'symbol':'" + symbol + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            addPortfolioRow(msg.d);
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

function addPortfolioRow(pfElement) {
    if (pfElement != null) {
        // alert(pfElement.Symbol + ": " + pfElement.Position);
        $('#content_portfolio_table > tbody:last').append('<tr>' +
                                                    '<td>' + pfElement.Symbol + '</td>' +
                                                    '<td>' + pfElement.Close + '</td>' +
                                                    '<td>' + pfElement.Position + '</td>' +
                                                    '<td>' + pfElement.Gain + '</td>' +
                                                    '<td>' + pfElement.Maxinvest + '</td>' +
                                                    '<td>' + pfElement.Cutloss + '</td>' +
                                                    '<td>' + pfElement.Decision + '</td>' +
                                                    '<td>' + pfElement.Roi + '</td>' +
                                                    '<td>' + pfElement.Auto + '</td>' +
                                                    '<td>' + pfElement.Active + '</td>' +
                                                    '</tr>');
        $("input").blur();
    } else {
        alert("No database entry for this symbol!");
    }
}