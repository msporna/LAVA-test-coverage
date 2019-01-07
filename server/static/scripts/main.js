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



var dashboardTimer;
var coverageChart;
var current_session_total_coverage;
var newSessionModulesTree;

var newSessionCheckedModules = [];
var newSessionUserId = -1;
var newSessionTagId = -1;


$(document).ready(function () {

    initDashboardPage();
});




function initDashboardPage() {

    //init sessions table
    $("#testSessionsTable").DataTable({
        "bPaginate": true,
        "order": [
            [6, "desc"]
        ],
        "lengthMenu": [[5,10, 25, 50, 100, -1], [5, 10, 25, 50, 100, "All"]]
    });
}

function startTestSession() {

    var dialog = BootstrapDialog.show({
        title: 'Create Coverage Session',
        draggable: true,
        message: 'Name: <input class="new-test-session-name-input" id="testSessionName"></input><p>Build number: <input class="new-test-session-build-input" id="testSessionBuild"></input></p><br><p class="new-session-tags-row">Select tag:<select id="newSessionPopupTagsCombobox"></select></p><p class="new-session-users-row">Choose user:<select id="newSessionPopupUsersCombobox"></select></p><br><div id="modulesContainer" class="new-session-modules-container"><div id="jstree_1"> </div></div><p class="new-session-modules-title">*Choose modules to cover:</p>',
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
                        dialogRef.settings.core.data = module_jsons;
                        dialogRef.refresh();
                    }


                } else {
                    //no modules


                    $("#jstree_1").html("</br><p>No modules</p><p>Create some in 'Modules' menu and come back.</p>");


                }
                });


            //show active tags combobox
            $.ajax({
                url: "/get_active_tags",
                type: "get",
                async: false

            }).done(function (data) {
                var dataTags = data["tags"];
                var tags = [];
                for (var i = 0; i < dataTags.length; i++) {
                    var tagJson = JSON.parse(dataTags[i]);
                    var tag = { id: tagJson["id"], title: tagJson["tag"] }
                    tags.push(tag);
                }
               


                //show combobox
                $('#newSessionPopupTagsCombobox').selectize({
                    maxItems: 1,
                    valueField: 'id',
                    labelField: 'title',
                    searchField: 'title',
                    options: tags,
                    create: true,
                    sortField: 'title'
                }
                ).on("change", function (v) {

                    
                    newSessionTagId = v.target.value;
                    if (newSessionTagId.length == 0) {
                        newSessionTagId = -1;
                    }
                    

                });


               
            });

            //show active users combobox
            $.ajax({
                url: "/get_active_users",
                type: "get",
                async: false

            }).done(function (data) {
                var dataUsers = data["users"];
                var users = [];
                for (var i = 0; i < dataUsers.length; i++) {
                    var userJson = JSON.parse(dataUsers[i]);
                    var user = { id: userJson["id"], title: userJson["username"] }
                    users.push(user);
                }



                //show combobox
                $('#newSessionPopupUsersCombobox').selectize({
                    maxItems: 1,
                    valueField: 'id',
                    labelField: 'title',
                    searchField: 'title',
                    options: users,
                    create: true,
                    sortField: 'title'
                }
                ).on("change", function (v) {

                    newSessionUserId = v.target.value;
                    if (newSessionUserId.length == 0) {
                        newSessionUserId = -1;
                    }

                });



            });
        },
        onhidden: function (dialogRef) {
            closeNewSessionDialog(dialogRef);
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

                //vet values
                var name_p = $("#testSessionName").val(); 
                var build_p = $("#testSessionBuild").val(); 

                if (name_p.length == 0) {
                    alert("Please provide name for new session!");
                    return;
                }
                if (build_p.length == 0) {
                    alert("Please provide build for new session!");
                    return;
                }
                //check if build # is number
                if (isNaN(build_p) || build_p<0) {
                    alert("build must be a positive number!");
                    return;
                }
                //make sure tag and user were provided
                if (newSessionUserId == -1) {
                    alert("choose owner of the session!");
                    return;
                }
                if (newSessionTagId == -1) {
                    alert("choose tag for the session!");
                    return;
                }


                $.ajax({
                    url: "/set_test_session_start",
                    type: "post",
                    async: true,
                    contentType: 'application/json',
                    data: JSON.stringify({
                        "test_session_name": name_p,
                        "test_session_modules": newSessionCheckedModules,
                        "test_session_build": build_p,
                        "test_session_owner_id": newSessionUserId,
                        "test_session_tag_id": newSessionTagId
                    })


                }).done(function (data,textStatus, xhr) {
                    if (xhr.status == 200) {
                        //refresh
                        location.reload();
                    } 

                    }).fail(function (data, xhr) {
                        alert(data.responseText)

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

    dialog.getModalFooter().append("</br> *Modules without sources are not shown.");
    dialog.getModalHeader().css('background-color', '#16A085');
    dialog.getModalDialog().css("width", '25vw');
    dialog.getModalBody().css("height", '70vh');
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
        message: '<p>Tool created by: Michal Sporna</p><p>Version: ' + APP_VERSION+'</p>',

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
    }).done(function (data, textStatus, xhr) {
        if (xhr.status == 200) {
            //refresh
            location.reload();
        }

    }).fail(function (data, textStatus, xhr) {
        alert(data.responseText);
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

function removeSession(sessionID, sessionName) {
    if (confirm("Do you really want to remove session named " + sessionName+"?")) {
        $.ajax({
            url: "/remove_test_session",
            type: "post",
            async: true,
            contentType: 'application/json',
            data: JSON.stringify({
                "test_session_id": sessionID
            })


        }).done(function (data, textStatus, xhr) {
            if (xhr.status==200) {
                //refresh
                location.reload();
            } else {
                alert(data);

            }

        });
    }
    

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

function showSetProjectNamePopup() {
    var dialog = BootstrapDialog.show({
        title: 'Set Project Name',
        draggable: true,
        message: 'New project name: <input id="projectNameInput"/>',
        buttons: [{
            id: 'saveNewProjectNameButton',
            label: 'Save',
            cssClass: 'btn-success',
            action: function (dialog) {

                var newProjectName = $("#projectNameInput").val();
                if (newProjectName.length == 0) {
                    alert("Project name cannot be empty!");
                    return;
                }

                $.ajax({
                    url: "/set_project_name",
                    type: "post",
                    async: true,
                    contentType: 'application/json',
                    data: JSON.stringify({
                        "project_name": newProjectName
                    })


                }).done(function (data) {

                    if (data == "201") {
                        //refresh page
                        location.reload();
                    } else {
                        alert(data);

                    }

                });



                dialog.close();
            }
        }, {
            label: 'Cancel',
            cssClass: 'btn-danger',
            action: function (dialog) {

                dialog.close();
            }
        }]
    });

 

}


function setMinCoverageValueGoal() {
    var dialog = BootstrapDialog.show({
        title: 'Set Coverage Goal',
        draggable: true,
        message: 'Expected coverage goal: <input id="buildCoverageGoalInput"/>',
        buttons: [{
            id: 'saveCoverageGoalButton',
            label: 'Save',
            cssClass: 'btn-success',
            action: function (dialog) {

                var newCoverageGoal = $("#buildCoverageGoalInput").val();
                if (isNaN(newCoverageGoal) || newCoverageGoal < 0 || newCoverageGoal.length == 0) {
                    alert("Provide a value!");
                    return;
                }

                $.ajax({
                    url: "/set_coverage_goal_value",
                    type: "post",
                    async: true,
                    contentType: 'application/json',
                    data: JSON.stringify({
                        "coverage_value": newCoverageGoal
                    })


                }).done(function (data) {

                    if (data == "201") {
                        //refresh page
                        location.reload();
                    } else {
                        alert(data);

                    }

                });



                dialog.close();
            }
        }, {
            label: 'Cancel',
            cssClass: 'btn-danger',
            action: function (dialog) {

                dialog.close();
            }
        }]
    });
}

function switchCoverageGoalBuild() {
    var currentlyCheckedBuildNodeId;
    var currentlyCheckedBuildId;
    var currentlyCheckedBuildRelatedTagName;
    var currentlyCheckedBuild;

    var dialog = BootstrapDialog.show({
        title: 'Available Builds',
        draggable: true,
        message: '<div id="buildsContainer" class="session-modules-container"><div id="jstree_1"> </div></div><p class="session-modules-title">Build numbers are grouped by tags:</p>',
        onshown: function (dialogRef) {
            //show active modules

            $.ajax({
                url: "/get_available_builds",
                type: "get",
                async: false
            }).done(function (data) {
                var builds = data["builds"];
                

                if (builds.length > 0) {

                    var build_jsons = []
                    for (var i = 0; i < builds.length; i++) {
                        var buildsJson = JSON.parse(builds[i]);
                        build_jsons.push(buildsJson);
                    }


                    //show a tree with saved modules
                    $('#jstree_1').jstree({
                        'core': {
                            'data': build_jsons
                        },
                        'types': {
                            "build": {
                                "icon": "glyphicon glyphicon-tasks"
                            }
                        },
                        checkbox: {
                            three_state: false,
                            whole_node: false,
                            tie_selection: false,
                        },
                        'plugins': ["checkbox", "types", "sort"]
                    })
                        .on("check_node.jstree", function (e, data) {
                            console.log("CHEKCED");
                        //uncheck previously checked build
                            data.instance.uncheck_node(currentlyCheckedBuildNodeId);
                            currentlyCheckedBuildNodeId= data.node.id;
                            currentlyCheckedBuildId = data.node.original.build_id
                            currentlyCheckedBuildRelatedTagName = data.node.original.parent_tag
                            currentlyCheckedBuild = data.node.text;

                    })
                   
                   

                } else {
                    //no builds to show
                    $('#jstree_1').jstree(true).destroy();
                    $("#jstree_1").html("</br><p>There are no builds available.</p>");


                }
            });

        },
        onhidden: function (dialogRef) {

            $.ajax({
                url: "/get_total_coverage_for_specific_build",
                type: "get",
                async: false,
                data:
                {
                    "build_id": currentlyCheckedBuildId
                }
            }).done(function (data) {
                var coverage =data["total_coverage"];
                var coverage_status=data["coverage_status"]
                // update label
                $("#buildTotalCoverageValue").text(coverage+"%");
                $("#buildTotalCoverageBuild").text(currentlyCheckedBuild);
                $("#buildTotalCoverageRelatedTag").text(currentlyCheckedBuildRelatedTagName);
                // update generate build report params
                $("#generateBuildReportLink").attr("href", 'javascript:generateBuildReport(' + currentlyCheckedBuild + ',"' + currentlyCheckedBuildRelatedTagName + '")');

                if (coverage_status == true) {
                    $("#coverageGoalContainer").removeClass("coverage-goal-container-fail");
                }
                else {
                    $("#coverageGoalContainer").addClass("coverage-goal-container-fail");

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

function generateBuildReport(build_number, tag_name) {
    window.location.href = "/report/build/" + build_number + "/tag/" + tag_name;
   
}