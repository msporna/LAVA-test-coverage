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

var modulesSettingsModulesTree;
var modulesSettingsSourcesTree;
var modulesSettingsLastCheckedSources = [];
var modulesSettingsCheckedModules = [];
var modulesSettingsSourcesTreeSuppressEvent = false;
var modulesSettingsModulesTreeSuppressEvent = false;
var modulesSettingsCheckedModuleSources = [];

var isModuleChecked = false;


function showModulesConfig() {

    var modalHtml = '<div class="modules-popup-content-class container"> <div class="modules-left-title-container-class"> <p class="modules-left-title-class">Modules</p> <button class="modules-new-module-span" onclick="showNewModuleNamePrompt();">New</button><button onclick="removeModule();" class="modules-remove-module-span">Remove</button> </div> <div class="modules-left-class"> <div id="jstree_1"> </div> </div> <div class="modules-center-class"> <div class="modules-buttons-class"> <p><button onclick="addFileToSelectedModule()" class="modules-add-button-class"><-- assign</button></p> <p><button onclick="removeFileFromSelectedModule()" class="modules-remove-button-class">unassign--></button></p> </div> </div> <div class="modules-right-title-container-class"> <p class="modules-right-title-class">Unassigned Sources</p> </div> <div class="modules-right-class"> <div id="jstree_2"></div> </div> </div> ';
    var dialog = BootstrapDialog.show({
        title: "Modules Settings",
        draggable: true,
        closable: false,
        message: modalHtml,
        onshown: function (dialogRef) {

            //get modules
            showModulesTree();

            //get sources
            showUnassignedFilesTree();


        },
        onhidden: function (dialogRef) {
            location.reload(); //to refresh badge with number of unassigned sources shown next to Modules link on dashboard
        },

        buttons: [{
            label: 'Done',
            cssClass: 'btn-success',
            action: function (dialog) {
                doCleanup();


                dialog.close();
            }
        }],
    });


    dialog.getModalHeader().css('background-color', '#16A085');
    dialog.getModalDialog().css("width", '90vw');
    dialog.getModalBody().css("height", '60vh');
}

function doCleanup() {
    modulesSettingsSourcesTree = undefined;
    modulesSettingsModulesTree = undefined;

    modulesSettingsLastCheckedSources = [];
    modulesSettingsCheckedModules = [];
    modulesSettingsSourcesTreeSuppressEvent = false;
    modulesSettingsModulesTreeSuppressEvent = false;
    modulesSettingsCheckedModuleSources = [];
}

function showModulesTree() {


    $.ajax({
        url: "/get_modules",
        type: "get",
        data: {
            "with_files_only": "False",
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


            if (modulesSettingsModulesTree == undefined) {

                //show a tree with saved modules
                modulesSettingsModulesTree = $('#jstree_1').jstree({
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

                        onModuleChecked(data);

                    })
                    .on("uncheck_node.jstree", function (e, data) {

                        onModuleUnchecked(data);

                    });
            } else {

                $('#jstree_1').jstree(true).settings.core.data = module_jsons;
                $('#jstree_1').jstree(true).refresh();
            }


        } else {
            //no modules, remove modules tree
            $("#jstree_1").html("</br><p>There are no modules. Please create some.</p>");


        }
    });

    //clear
    modulesSettingsCheckedModules = [];
    modulesSettingsCheckedModuleSources = [];
    isModuleChecked = false;


}



function onModuleChecked(data) {
    //can check source within module, then module will be checked too
    //if checking module itself,sources are not being checked, no need for it




    if (data.node.children.length > 0 || data.node.parent == "#") {
        //module checked
        addToList(modulesSettingsCheckedModules, data.node.id);
        isModuleChecked = true;

        for (var i = 0; i < data.node.children.length; i++) {
            data.instance.check_node(data.node.children[i]);

        }
    } else {
        //source checked
        addToList(modulesSettingsCheckedModuleSources, data.node.id);
        //add parent module to checked too
        addToList(modulesSettingsCheckedModules, data.node.parent);

    }
}


function onModuleUnchecked(data) {


    //if unchecking module, uncheck also children
    if (data.node.children.length > 0 || data.node.parent == "#") {
        //module unchecked
        removeItemFromList(modulesSettingsCheckedModules, data.node.id);
        isModuleChecked = false;

        for (var i = 0; i < data.node.children.length; i++) {
            data.instance.uncheck_node(data.node.children[i]);

        }
    } else {
        //source unchecked
        removeItemFromList(modulesSettingsCheckedModuleSources, data.node.id);

        var module_node = data.instance.get_node(data.node.parent);
        //get number of items checked for the parent module
        if (data.instance.get_checked_descendants(module_node.id).length == 0) {
            //no more source is checked in this module
            //if parent is not checked, then remove from checked modules collection
            if (!data.instance.is_checked(module_node)) {
                removeItemFromList(modulesSettingsCheckedModules, data.node.parent);
            }

        }

    }
}


function showUnassignedFilesTree() {
    $.ajax({
        url: "/get_sources",
        type: "get",
        async: false


    }).done(function (data) {
        var sources = data["sources"];

        //sources come from backend as strings, convert to json objects
        var source_jsons = []
        for (var i = 0; i < sources.length; i++) {
            var sourcesJson = JSON.parse(sources[i]);
            source_jsons.push(sourcesJson);
        }

        if (sources.length > 0) {

            if (modulesSettingsSourcesTree == undefined) {
                //and the one with yet unassigned souce files
                modulesSettingsSourcesTree = $('#jstree_2').jstree({
                        'core': {
                            'data': source_jsons
                        },
                        'types': {
                            "source": {
                                "icon": "glyphicon glyphicon-file"
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
                        onSourceSelected(data);
                    })
                    .on("uncheck_node.jstree", function (e, data) {
                        onSourceUnchecked(data);
                    });

            } else {
                $('#jstree_2').jstree(true).settings.core.data = source_jsons;
                $('#jstree_2').jstree(true).refresh();
            }
        } else {

            if (modulesSettingsSourcesTree != undefined) {
                $('#jstree_2').jstree(true).destroy();
                modulesSettingsSourcesTree = undefined;
            }


            $("#jstree_2").html("</br><p>There is nothing to show.</p>");
        }

    });

    modulesSettingsLastCheckedSources = [];

}

function onSourceSelected(data) {

    if (data.node.children.length > 0 || data.node.parent == "#") {
        for (var i = 0; i < data.node.children.length; i++) {

            data.instance.check_node(data.node.children[i]);
            addToList(modulesSettingsLastCheckedSources, data.node.children[i]);
        }



    } else {


        addToList(modulesSettingsLastCheckedSources, data.node.id);
    }

}



function onSourceUnchecked(data) {

    //remove from checked sources
    removeItemFromList(modulesSettingsLastCheckedSources, data.node.id);


    if (data.node.children.length > 0 || data.node.parent == "#") {

        for (var i = 0; i < data.node.children.length; i++) {

            data.instance.uncheck_node(data.node.children[i]);
        }


    } else {
        //do not uncheck last checked node
        data.instance.uncheck_node(data.node.id);

        if (data.instance.get_checked_descendants(data.node.parent).length == 0) {
            data.instance.uncheck_node(data.node.parent);
        }
    }

}



function deselectAllModules(data) {
    if (modulesSettingsModulesTreeSuppressEvent) {
        return;
    }

    for (var i = 0; i < data.selected.length; i++) {

        if (data.selected[i] != data.node.id) {
            //(avoid deselecting node that was checked recently
            data.instance.uncheck_node(data.selected[i]);
            //uncheck children too
            if (data.node.children.length > 0) {
                for (var i = 0; i < data.node.children.length; i++) {
                    data.instance.uncheck_node(data.node.children[i]);
                }

            }

        }

    }
}




function addFileToSelectedModule() {

    if (!isModuleChecked) {
        alert("Select some module!");
        return;
    } else if (isModuleChecked && modulesSettingsCheckedModules.length > 1) {
        alert("You have more than 1 module selected! Choose only 1 to assign sources to.");
        return;
    } else if (modulesSettingsLastCheckedSources.length == 0) {
        alert("Select source to assign to module!");
        return;
    }

    $.ajax({
        url: "/assign_file",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: JSON.stringify({
            "module_id": modulesSettingsCheckedModules[0],
            "sources": modulesSettingsLastCheckedSources
        })


    }).done(function (data, textStatus, xhr) {


        if (xhr.status == 200) {
            //refresh
            showModulesTree();
            showUnassignedFilesTree();
        }




    });
}

//moduleSettingsCheckedModule will be checke automatically when sources of modules are checked
function removeFileFromSelectedModule() {
    if (modulesSettingsCheckedModuleSources.length == 0) {
        alert("Select some sources to be removed from module!");
        return;
    }

    $.ajax({
        url: "/unassign_file",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: JSON.stringify({
            "sources": modulesSettingsCheckedModuleSources
        })


    }).done(function (data, textStatus, xhr) {


        if (xhr.status == 200) {
            //refresh
            showModulesTree();
            showUnassignedFilesTree();
        }




    });
}

function showNewModuleNamePrompt() {
    var moduleName = prompt("Please enter new module name", "Module1");

    if (moduleName != null) {
        createModule(moduleName);
    }

}



function createModule(moduleName) {

    $.ajax({
        url: "/create_module",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: moduleName


    }).success(function (data, textStatus, xhr) {
        if (xhr.status == 200) {
            showModulesTree();
        } else if (data == "duplicate") {
            alert("Module with that name already exists!");
        }

    });

}



function removeModule() {
    if (!isModuleChecked) {
        alert("Select some module!");
        return;
    }

    $.ajax({
        url: "/remove_module",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: JSON.stringify({
            "modules": modulesSettingsCheckedModules
        })


    }).done(function (data,xhr) {


        if (xhr.status== 200) {
            //refresh
            showModulesTree();
            showUnassignedFilesTree();
        }




    });
}