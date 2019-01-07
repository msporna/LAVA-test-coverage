
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

var tagsSettingsTree;
var tagsTreeCheckedTags = [];
var isTagchecked = false;


function showTagsConfig() {

    var modalHtml = '<div class="modules-popup-content-class container"> <div class="modules-left-title-container-class"> <p class="modules-left-title-class">Tags</p> <button class="modules-new-module-span" onclick="showNewTagPrompt();">New</button><button onclick="removeTags();" class="modules-remove-module-span">Remove</button> </div> <div class="modules-left-class"> <div id="tagsTree"> </div> </div>'
    var dialog = BootstrapDialog.show({
        title: "Tag Settings",
        draggable: true,
        closable: false,
        message: modalHtml,
        onshown: function (dialogRef) {
            showTagsTree();
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

}

function doCleanup() {
    tagsSettingsTree = undefined;

    tagsTreeCheckedUsers = [];
    tagsTreeSuppressEvent = false;
    isTagChecked = false;
}

function showTagsTree() {


    $.ajax({
        url: "/get_active_tags",
        type: "get"

    }).done(function (data) {
        var tags = data["tags"];
        console.log("obtained " + tags.length + " tags.");
        if (tags.length > 0) {

            //tags come from backend as strings, convert to json objects
            var tag_jsons = []
            for (var i = 0; i < tags.length; i++) {
                var tagJson = JSON.parse(tags[i]);

                tag_jsons.push(tagJson);
            }

            if (tagsSettingsTree == undefined) {

                //show a tree with saved tags
                tagsSettingsTree = $('#tagsTree').jstree({
                    'core': {
                        'data': tag_jsons
                    },
                    'types': {
                        "tag_entry": {
                            "icon": "glyphicon glyphicon-tag"
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

                        onTagChecked(data);

                    })
                    .on("uncheck_node.jstree", function (e, data) {

                        onTagUnchecked(data);

                    });
            } else {

                $('#tagsTree').jstree(true).settings.core.data = tag_jsons;
                $('#tagsTree').jstree(true).refresh();
            }


        } else {
            //no modules, remove modules tree
            $("#tagsTree").html("</br><p>There are no tags. Please create some.</p>");


        }
    });

    //clear
    tagsTreeCheckedUsers = [];
    isTagChecked = false;


}



function onTagChecked(data) {
    tagsTreeCheckedTags.push(data.node.id);
    isTagChecked = true;
}


function onTagUnchecked(data) {

    removeItemFromList(tagsTreeCheckedTags, data.node.id);
    isTagChecked = false;


}


function showNewTagPrompt() {
    var tag = prompt("Please  enter new tag name", "Create Tag");

    if (tag != null) {
        createTag(tag);
    }

}


function createTag(tag) {

    $.ajax({
        url: "/create_tag",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: tag


    }).done(function (data, textStatus, xhr) {
        if (xhr.status==200) {
            showTagsTree();
        } else if (data == "duplicate") {
            alert("Specified tag already exists!");
        }

    });

}


function removeTags() {
    if (!isTagChecked) {
        alert("Select some tag first!");
        return;
    }

    $.ajax({
        url: "/remove_tags",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: JSON.stringify({
            "tags": tagsTreeCheckedTags
        })


    }).done(function (data, textStatus, xhr) {
        if (xhr.status==200) {
            //refresh
            showTagsTree();

        }
        else {
            alert("There was an error while attempting to remove tags! check backend logs.")
        }
    });
}