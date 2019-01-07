/*
Copyright (c) 2016-2019 by Michal Sporna and contributors.  See AUTHORS
for more details.

Some rights reserved.

Redistribution and use in source and binary forms of the software as well
as documentation, with or without modification, are permitted provided
that the following conditions are met:

* Redistributions of source code must retain the above copyright
  notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above
  copyright notice, this list of conditions and the following
  disclaimer in the documentation and/or other materials provided
  with the distribution.

* The names of the contributors may not be used to endorse or
  promote products derived from this software without specific
  prior written permission.

THIS SOFTWARE AND DOCUMENTATION IS PROVIDED BY THE COPYRIGHT HOLDERS AND
CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT
NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER
OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE AND DOCUMENTATION, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
DAMAGE.
*/

var filesCoverageTable;
var templateCoverageTable;
var routeCoverageTable;
var modulesCoverageTable;
var callTimelineTable;


$(document).ready(function () {
    initReportPage();

});


function initReportPage() {

    createBarChartForTotals(total_executed_percent);

    //data tables
    filesCoverageTable = $('#filesCoverageTable').DataTable({
        "order": [
            [4, "desc"]
        ],
        "lengthMenu": [[5,10, 25, 50,100, -1], [5,10, 25, 50,100, "All"]]
    });

    templateCoverageTable = $("#templatesCoverageTable").DataTable({
        "order": [
            [4, "desc"]
        ],
        "lengthMenu": [[5, 10, 25, 50, 100, -1], [5, 10, 25, 50, 100, "All"]]
    });

    routeCoverageTable = $("#routeCoverageTable").DataTable();

    modulesCoverageTable = $("#modulesCoverageTable").DataTable({
        "order": [
            [4, "desc"]
        ],
        "lengthMenu": [[5, 10, 25, 50, 100, -1], [5, 10, 25, 50, 100, "All"]]
    });

    callTimelineTable = $("#callTimelineTable").DataTable({
        "ordering": false,
        "lengthMenu": [[5, 10, 25, 50, 100, -1], [5, 10, 25, 50, 100, "All"]]
    });


    //show which lines were executed inside a file when file name is clicked
    filesCoverageTable.on('click', 'td', function () {
        showExecutedLines($(this).closest('tr').find('td:eq(0)').text());

    });

    //show which lines were executed inside a template file when file name is clicked
    templateCoverageTable.on('click', 'td', function () {
        showExecutedLines($(this).closest('tr').find('td:eq(0)').text());

    });
}

function createPieChartForTotals(executed, executable) {
    //not used anymore but left for reference

    //docs:http://www.chartjs.org/docs/#doughnut-pie-chart-example-usage
    var ctx = $("#pieChart");
    var data = {
        labels: ["executed", "executable"],
        datasets: [{
            data: [executed, executable],
            backgroundColor: [
                "#2980B9",
                "#2C3E50"
            ],
            hoverBackgroundColor: ["#F1C40F"]
        }]
    }
    coverageChart = new Chart(ctx, {
        type: 'pie',
        data: data
    });
}


function createBarChartForTotals(total_coverage_value) {
    //docs:http://www.chartjs.org/docs/#bar-chart-example-usage
    var ctx = $("#barChart");
    var data = {
        labels: ["executed"],
        datasets: [{
            label: "coverage",
            data: [total_coverage_value],
            backgroundColor: [
                "#2980B9",
                "#2C3E50"
            ],
            hoverBackgroundColor: ["#F1C40F"]
        }]
    }
    coverageChart = new Chart(ctx, {
        type: 'horizontalBar',
        data: data,
        options: {
            scales: {
                xAxes: [{
                    ticks: {
                        beginAtZero: true,
                        min: 0,
                        max: 100
                    }
                }]
            }
        }
    });
}






function backToDashboard() {
    window.location.href = '/dashboard';
}


/**
Given a filename, get filecontent and show which lines of the file were executed
**/
function showExecutedLines(filename_p) {


    var fileContent = "";
    var executedLines = [];
    var executableLines = [];


    //get file content
    $.ajax({
        url: "/get_file_content",
        type: "get",
        async: false,
        data: {
            filename: filename_p,
            session_id: CURRENT_SESSION_ID
        }

    }).done(function (data) {
        fileContent = data["decoded_content_string"];
        executedLines = data["executed_lines"]
        executableLines = data["executable_lines"]
    });




    //start syntax highlighting
    var modalHtml = '<div id="codeEditor"></div>';



    //https://nakupanda.github.io/bootstrap3-dialog/
    var dialog = BootstrapDialog.show({
        title: filename_p + " (green - executed, yellow- missed)",
        draggable: true,
        message: modalHtml,
        onshown: function (dialogRef) {
            //show code editor, read only, with file content and executed lines highlighted
            var myCodeMirror = CodeMirror(document.getElementById("codeEditor"), {
                value: fileContent,
                mode: "javascript",
                lineNumbers: true,
                readOnly: true
            });

            //now highlight all executables
            for (var i0 = 0; i0 < executableLines.length; i0++) {
                myCodeMirror.markText({
                    line: executableLines[i0] - 1,
                    ch: 0
                }, {
                    line: executableLines[i0] - 1,
                    ch: 10000
                }, {
                    css: "background-color:#FFDC5E;"
                });
            }

            //now highlight all lines that contain executed code
            for (var i1 = 0; i1 < executedLines.length; i1++) {
                myCodeMirror.markText({
                    line: executedLines[i1] - 1,
                    ch: 0
                }, {
                    line: executedLines[i1] - 1,
                    ch: 10000
                }, {
                    css: "background-color:#16A085;"
                });
            }



        },

        buttons: [{
            label: 'Close',
            cssClass: 'btn-warning',
            action: function (dialog) {

                dialog.close();
            }
        }]
    });

    dialog.getModalHeader().css('background-color', '#16A085');
    dialog.getModalDialog().css("width", '90%');

}