
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

var userSettingsTree;
var userTreeCheckedUsers=[];
var isUserchecked = false;


function showUsersConfig() {

    var modalHtml = '<div class="modules-popup-content-class container"> <div class="modules-left-title-container-class"> <p class="modules-left-title-class">Users</p> <button class="modules-new-module-span" onclick="showNewUserPrompt();">New</button><button onclick="removeUser();" class="modules-remove-module-span">Remove</button> </div> <div class="modules-left-class"> <div id="usersTree"> </div> </div>'
    var dialog = BootstrapDialog.show({
        title: "Users Settings",
        draggable: true,
        closable: false,
        message: modalHtml,
        onshown: function (dialogRef) {

            //get users
            showUsersTree();

           


        },
        onhidden: function (dialogRef) {
            location.reload(); //to refresh badge with number of unassigned sources shown next to users link on dashboard
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
    userSettingsTree= undefined;

    userTreeCheckedUsers = [];
    usersTreeSuppressEvent = false;
    isUserchecked = false;
}

function showUsersTree() {


    $.ajax({
        url: "/get_active_users",
        type: "get"

    }).done(function (data) {
        var users= data["users"];
        console.log("obtained " + users.length + " users.");
        if (users.length > 0) {
           
            //users come from backend as strings, convert to json objects
            var user_jsons = []
            for (var i = 0; i < users.length; i++) {
                var userJson = JSON.parse(users[i]);

                user_jsons.push(userJson);
            }

            if (userSettingsTree == undefined) {

                //show a tree with saved users
                userSettingsTree = $('#usersTree').jstree({
                    'core': {
                        'data': user_jsons
                    },
                    'types': {
                        "user_entry": {
                            "icon": "glyphicon glyphicon-user"
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

                        onUserChecked(data);

                    })
                    .on("uncheck_node.jstree", function (e, data) {

                        onUserUnchecked(data);

                    });
            } else {

                $('#usersTree').jstree(true).settings.core.data = user_jsons;
                $('#usersTree').jstree(true).refresh();
            }


        } else {
            //no user,remove user tree
            $("#usersTree").html("</br><p>There are no users. Please create some.</p>");


        }
    });

    //clear
    userTreeCheckedUsers = [];
    isUserChecked = false;


}



function onUserChecked(data) {
    userTreeCheckedUsers.push(data.node.id);
    isUserChecked = true;
}


function onUserUnchecked(data) {

    removeItemFromList(userTreeCheckedUsers, data.node.id);
    isUserChecked = false;

    
}


function showNewUserPrompt() {
    var username = prompt("Please  enter new user name", "Create User");

    if (username != null) {
        createUser(username);
    }

}


function createUser(userName) {

    $.ajax({
        url: "/create_user",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: userName


    }).done(function (data, textStatus, xhr) {
        if (xhr.status==200) {
            showUsersTree();
        } else if (data == "duplicate") {
            alert("User with that name already exists!");
        }

    });

}


function removeUser() {
    if (!isUserChecked) {
        alert("Select some user!");
        return;
    }

    $.ajax({
        url: "/remove_user",
        type: "post",
        async: false,
        contentType: 'application/json',
        data: JSON.stringify({
            "users": userTreeCheckedUsers
        })


    }).done(function (data,textStatus, xhr) {
        if (xhr.status==200) {
            //refresh
            showUsersTree();

        }
        else {
            alert("There was an error while attempting to remove users! check backend logs.")
        }
    });
}