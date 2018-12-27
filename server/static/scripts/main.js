/*
Copyright (c) 2016-2018 by Michal Sporna and contributors.  See AUTHORS
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



var dashboardTimer;
var coverageChart;
var current_session_total_coverage;
var newSessionModulesTree;

var newSessionCheckedModules = [];


$(document).ready(function () {

    initDashboardPage();
});




function initDashboardPage() {
    $("#testSessionsTable").DataTable({
        "bPaginate": false,
        "order": [
            [3, "desc"]
        ]
    });



}


function updateSessionsList() {

    var sessions = [];
    $.ajax({
        url: "/get_sessions",
        type: "get",
        async: true

    }).done(function (data) {
        sessions = data["sessions"];

        var table = $("#testSessionsTable").DataTable(); //retrieve datatable instance to refresh it
        table.clear().draw(); //clear existing content
        for (var i = 0; i < sessions.length; i++) {
            var subset = [];
            subset.push('<a class="hotlink-paragraph-class" href="/report/' + sessions[i].ID + '">' + sessions[i].name + '</a>');
            subset.push('<a class="hotlink-paragraph-class" href="javascript:showSessionModules(' + sessions[i].ID + ',\'' + sessions[i].name + '\');">' + sessions[i].covered_modules_count + '</a>' + "/" + sessions[i].active_modules_count);
            subset.push(sessions[i].is_active);
            subset.push(sessions[i].start_date);
            subset.push(sessions[i].total_coverage + "%");

            //action buttons are always in the last column
            var table_action_buttons = '<span class="glyphicon glyphicon-remove session-action" aria-hidden="true" onclick=removeSession(' + sessions[i].ID + ')></span> <span class="glyphicon glyphicon-stop session-action" aria-hidden="true" onclick=stopLiveTestSession()></span>'
            subset.push(table_action_buttons);
            //update table
            table.row.add(subset)
                .draw();
        }
    });

}


function startTestSession() {

    var dialog = BootstrapDialog.show({
        title: 'Set name for a new session',
        draggable: true,
        message: 'Name: <input class="new-test-session-name-input" id="testSessionName"></input><br><br><div id="modulesContainer" class="new-session-modules-container"><div id="jstree_1"> </div></div><p class="new-session-modules-title">*Choose modules to cover:</p>',
        onshown: function (dialogRef) {
            //show active modules

            $.ajax({
                url: "/get_modules",
                type: "get",
                async: false,
                data: {
                    "with_files_only": "True",
                    "session_id": "None"
                }


            }).done(function (data) {
                var modules = data["modules"];

                if (modules.length > 0) {
                    hasModules = true;

                    //modules come from backend as strings, convert to json objects
                    var module_jsons = []
                    for (var i = 0; i < modules.length; i++) {
                        var modulesJson = JSON.parse(modules[i]);




                        module_jsons.push(modulesJson);
                    }


                    if (newSessionModulesTree == undefined) {
                        //show a tree with saved modules
                        newSessionModulesTree = $('#jstree_1').jstree({
                                'core': {
                                    'data': module_jsons
                                },
                                'types': {
                                    "module_source": {
                                        "icon": "glyphicon glyphicon-file"
                                    }
                                },
                                checkbox: {
                                    three_state: false,
                                    whole_node: false,
                                    tie_selection: false
                                },
                                'plugins': ["checkbox", "types", "sort"]
                            })
                            .on("check_node.jstree", function (e, data) {

                                onNewSessionModuleChecked(data);


                            })
                            .on("uncheck_node.jstree", function (e, data) {

                                onNewSessionModuleUnchecked(data);

                            });
                    } else {
                        $('#jstree_1').jstree(true).settings.core.data = module_jsons;
                        $('#jstree_1').jstree(true).refresh();
                    }


                } else {
                    //no modules


                    $("#jstree_1").html("</br><p>No modules</p><p>Create some in 'Modules' menu and come back.</p>");


                }
            });

        },

        buttons: [{
            id: 'newSessionSaveButton',
            label: 'Start',
            cssClass: 'btn-success',
            action: function (dialog) {

                if (newSessionCheckedModules.length == 0) {
                    alert("Select at least 1 module!");
                    return;
                }


                var name_p = $("#testSessionName").val();

                if (name_p.length == 0) {
                    alert("Please provide name for new session!");
                    return;
                }


                $.ajax({
                    url: "/set_test_session_start",
                    type: "post",
                    async: true,
                    contentType: 'application/json',
                    data: JSON.stringify({
                        "test_session_name": name_p,
                        "test_session_modules": newSessionCheckedModules
                    })


                }).done(function (data) {


                    if (data == "200") {
                        //refresh
                        updateSessionsList();
                    } else {
                        alert(data);

                    }

                });



                closeNewSessionDialog(dialog);
            }
        }, {
            label: 'Cancel',
            cssClass: 'btn-danger',
            action: function (dialog) {

                closeNewSessionDialog(dialog);
            }
        }]
    });

    dialog.getModalFooter().append("</br> *Showing only modules with sources assigned.");
    dialog.getModalHeader().css('background-color', '#16A085');
    dialog.getModalDialog().css("width", '25vw');
    dialog.getModalBody().css("height", '60vh');
}


function showSessionModules(sessionID, sessionName) {
    var dialog = BootstrapDialog.show({
        title: 'Modules for [' + sessionName + ']',
        draggable: true,
        message: '<div id="modulesContainer" class="session-modules-container"><div id="jstree_1"> </div></div><p class="session-modules-title">Selected test session gathers coverage from the following modules:</p>',
        onshown: function (dialogRef) {
            //show active modules

            $.ajax({
                url: "/get_modules",
                type: "get",
                async: false,
                data: {
                    "with_files_only": "False",
                    "session_id": sessionID

                }


            }).done(function (data) {
                var modules = data["modules"];

                if (modules.length > 0) {
                    hasModules = true;

                    //modules come from backend as strings, convert to json objects
                    var module_jsons = []
                    for (var i = 0; i < modules.length; i++) {
                        var modulesJson = JSON.parse(modules[i]);

                        module_jsons.push(modulesJson);
                    }


                    if (newSessionModulesTree == undefined) {
                        //show a tree with saved modules
                        $('#jstree_1').jstree({
                            'core': {
                                'data': module_jsons
                            },
                            'types': {
                                "module_source": {
                                    "icon": "glyphicon glyphicon-file"
                                }
                            },
                            'plugins': ["types", "sort"]
                        })

                    } else {
                        $('#jstree_1').jstree(true).settings.core.data = module_jsons;
                        $('#jstree_1').jstree(true).refresh();
                    }


                } else {
                    //no modules, remove modules tree
                    $('#jstree_1').jstree(true).destroy();


                    $("#jstree_1").html("</br><p>There are no modules related to this test session. Probably some issue occurred.</p>");


                }
            });

        },

        buttons: [{
            label: 'Close',
            cssClass: 'btn-danger',
            action: function (dialog) {
                $('#jstree_1').jstree(true).destroy();
                dialog.close();
            }
        }]
    });

    dialog.getModalHeader().css('background-color', '#16A085');
    dialog.getModalDialog().css("width", '25vw');
    dialog.getModalBody().css("height", '60vh');



}

function onNewSessionModuleChecked(data) {

    addToList(newSessionCheckedModules, data.node.id);
}

function onNewSessionModuleUnchecked(data) {
    removeItemFromList(newSessionCheckedModules, data.node.id);
}



/*
 * this is to do
 */
function showFiltersPopup() {
    var dialog = BootstrapDialog.show({
        title: 'Filter test sessions you see',
        draggable: true,
        message: 'THIS IS todo AND DOES NOTHING FOR NOW',

        buttons: [{
            label: 'Apply',
            action: function (dialog) {

                dialog.close();
            }
        }, {
            label: 'Cancel',
            action: function (dialog) {

                dialog.close();
            }
        }]
    });

    dialog.getModalHeader().css('background-color', '#2980B9');
}




function showAbout() {
    var dialog = BootstrapDialog.show({
        title: 'About',
        draggable: true,
        message: '<p>Tool created by: Michal Sporna</p>',

        buttons: [{
            label: 'Close',
            action: function (dialog) {

                dialog.close();
            }
        }]
    });

    dialog.getModalHeader().css('background-color', '#1ABC9C');
}

function stopLiveTestSession() {
    sendStopTestSessionRequest();


}


function sendStopTestSessionRequest() {
    $.ajax({
        url: "/set_test_session_end",
        type: "get",
        async: false
    }).done(function (data) {
        if (data == "200") {
            //refresh
            updateSessionsList();
        }

    });



}

function closeNewSessionDialog(dialog) {
    newSessionCheckedModules = [];

    if (newSessionModulesTree != undefined) {
        $('#jstree_1').jstree(true).destroy();
        newSessionModulesTree = undefined;
    }


    dialog.close();
}

function removeSession(sessionID) {
    $.ajax({
        url: "/remove_test_session",
        type: "post",
        async: true,
        contentType: 'application/json',
        data: JSON.stringify({
            "test_session_id": sessionID
        })


    }).done(function (data) {


        if (data == "200") {
            //refresh
            updateSessionsList();
        } else {
            alert(data);

        }

    });

}

function addToList(list, id) {
    var index = list.indexOf(id);
    if (index == -1) {
        list.push(id);
    }
}

function removeItemFromList(list, id) {
    var index = list.indexOf(id);
    if (index > -1) {
        list.splice(index, 1);
    }
}