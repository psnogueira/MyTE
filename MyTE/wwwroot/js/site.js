// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    // Obtenha os valores dos atributos data-* no input de pesquisa
    var inputElement = $("#autoComplete");
    var tableName = inputElement.data("table");
    var columnName1 = inputElement.data("column1");
    var columnName2 = inputElement.data("column2");

    inputElement.autocomplete({
        source: function (request, response) {
            var url = 'api/autocomplete/' + tableName + '/search';
            var data = { term: request.term, columnName1: columnName1 };

            if (columnName2) {
                data.columnName2 = columnName2;
            }

            $.ajax({
                url: url,
                data: data,
                success: function (data) {
                    response(data);
                }
            });
        }
    });
});

