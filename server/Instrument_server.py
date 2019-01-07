'''
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
'''

import datetime
import os
import sqlite3
import uuid
import base64
import json
import create_database
from flask_cors import CORS
from flask import render_template, request, Flask, jsonify, redirect
from gevent.pywsgi import WSGIServer

from flask_api import FlaskAPI, status, exceptions

CONNECTION_STRING = 'instrument.db'
CONFIG = {}  # dict holding key/value of config entries



app = Flask(__name__)
CORS(app)
app.config['PROPAGATE_EXCEPTIONS'] = True


############################# VIEWS ######################################

######################### API ############################################
@app.route("/")
def hello():
    return redirect("/dashboard", code=302)


@app.route("/set_test_session_start", methods=["POST"])
def start_test_session():
    active_session=get_active_test_session()
    if active_session is not None:
        return 'There currently be only 1 active session at the time. Please finish active session named ['+active_session[2]+'] and retry',status.HTTP_400_BAD_REQUEST

    data = json.loads(request.data)
    test_session_name = data["test_session_name"]

    modules_list = None
    if "test_session_modules" in data:
        modules_list = data["test_session_modules"]

   
    # user build and tag
    test_session_build_number=int(data["test_session_build"])
    test_session_owner=int(data["test_session_owner_id"])
    test_session_tag=int(data["test_session_tag_id"])


    make_all_sessions_inactive()
    reset_active_test()

    if create_new_test_session(test_session_name, modules_list,test_session_build_number,test_session_owner,test_session_tag):
        return '',status.HTTP_200_OK
    return "Session with specified name already exists! Choose unique name for your session.",status.HTTP_400_BAD_REQUEST


@app.route("/set_test_session_end")
def stop_test_session():
    # first,make sure that stats are not coming anymore
    if not can_session_be_ended():
        return 'Cannot end this session because stats are still coming. Please retry in a few seconds.',status.HTTP_400_BAD_REQUEST

    session_coverage = 0

    session_coverage, live_session_id,total_executed,total_executable = calculate_total_coverage_for_active_session()

    # save test session coverage when it's over

    sql = "UPDATE sessions SET total_coverage=:sc,total_executable=:total_executable,total_executed=:te WHERE ID=:sid"
    params = {"sc": str(session_coverage), "sid": live_session_id,"total_executable":total_executable,"te":total_executed}
    execute_query(sql, params)

    make_all_sessions_inactive()

    # save report
    with open("report.html", "wb") as f:
        f.write(prepare_report_page(live_session_id,use_embeded_template=True).encode('utf-8').strip())

    return '',status.HTTP_200_OK


@app.route("/remove_test_session", methods=["POST"])
def remove_test_session_view():
    data = json.loads(request.data)
    test_session_id = data["test_session_id"]
    remove_test_session(test_session_id)

    return '', status.HTTP_200_OK


@app.route("/refresh_config")
def refresh_config():
    get_config()
    return "200"


@app.route("/set_project_name", methods=["POST"])
def set_project_name():
    data = json.loads(request.data)
    project_name = data["project_name"]
    set_config_value("PROJECT_NAME",project_name)
    
    return "201"


@app.route("/get_instrument_token", methods=["GET"])
def get_instrument_token():
    token = None
    token = get_config_value("INSTRUMENT_TOKEN")
    if token is not None:
        return token
    else:
        # http://www.flaskapi.org/api-guide/status-codes/
        return "204"


@app.route("/set_detected_files", methods=["POST"])
def set_detected_files():
    data = json.loads(request.data)

    for key, value in data.iteritems():

        file, file_extension = os.path.splitext(value)

        # turn absolute file path into file path relative to source root
        source_root = get_config_value("SOURCE_ABSOLUTE_PATH")
        file_path = value.replace(source_root, "")
        filename = os.path.basename(file_path)

        # check if file exists in db at all
        file_id, is_history = get_file_id_by_filename(filename)

        if file_id is not None:
            # file exists in db
            if is_history == 1:
                # and is history, but current package contains it, so should
                # not be a history anymore
                sql = "UPDATE files SET is_history=0,path=:p WHERE ID= :fid"
                # if setting file to active again, update also it's path in
                # case it's changed
                param = {"fid": file_id, "p": file_path}
                execute_query(sql, param)
            else:
                # if exists in db and is active, just update path in case path
                # was changed
                sql = "UPDATE files SET path=:p WHERE ID= :fid"
                param = {"fid": file_id, "p": file_path}
                execute_query(sql, param)

        else:
            # file does not exist in db, insert
            sql = "INSERT INTO files(name,path,type,should_instrument,is_history) VALUES(?,?,?,?,?)"

            # insert
            file_id = execute_query(
                sql, (filename, file_path, file_extension, 1, 0))

        # insert into file_details ,for now without content&line count-this will come later.
        # always new row
        sql = "INSERT INTO file_details(file_id,file_content,executable_lines_count,updated) VALUES(?,?,?,?)"
        execute_query(
            sql, (file_id, None, 0, datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")))

    # now get all active files from db, and check if all of them are in package that came, if not,
    # mark them as history:
    db_files = get_active_files()

    for db_file in db_files:
        found_file = False
        for i in data:
            filename = os.path.basename(data[i])
            if db_file[1] == filename:
                found_file = True
                break
        if not found_file:
            # active file from db was not found in files that came from client
            # so make the file inactive
            sql = "UPDATE files SET is_history=1 WHERE ID=:fid"
            params = {"fid": int(db_file[0])}
            execute_query(sql, params)

            unassign_source_from_module([db_file[0]])

    return "200"


@app.route("/send_instrumentation_stats", methods=["GET"])
def send_instrumentation_stats():

    active_session = get_active_test_session()

    file_id = None
    if active_session is not None:
        data = request.args

        try:
            save_visited_route(data["route"], active_session[0])
        except:
            print("route not present in stats. Skipping saving route.")
        file_id = get_file_id_by_filename(data["file"], True)[0]

        if file_id is not None:
            d=datetime.datetime.strptime(data["send_date"], '%Y-%m-%d %H:%M:%S:%f')
            sql = "INSERT INTO stats(file_id,session_id,date,filename,line,line_guid,coverage_type,send_time,custom_value) VALUES(?,?,?,?,?,?,?,?,?)"
            try:
                execute_query(sql, (
                    file_id, active_session[0], datetime.datetime.now().strftime(
                        "%Y-%m-%d %H:%M:%S"), data["file"],
                    int(data["related_code_line"]), data["line_guid_p"], data["inject_type"],datetime.datetime.strptime(data["send_date"], '%Y-%m-%d %H:%M:%S:%f'),data["custom_value"]))
            except e:
                print(e)

    else:
        print("No active test session found")
        return "500"
    return "saved"


@app.route("/get_total_coverage_for_specific_build", methods=["GET"])
def get_total_coverage_for_specific_build():
    data = request.args
    build_id=data["build_id"]
    total_coverage=calculate_total_coverage_for_build(int(build_id))

    min_coverage_value=0
    min_coverage_value=get_config_value("MIN_BUILD_COVERAGE_VALUE")
    if min_coverage_value==None:
        # get default coverage value
        min_coverage_value=get_config_value("DEFAULT_MIN_COVERAGE_PERCENT")
    coverage_status=True
    if total_coverage<float(min_coverage_value):
        coverage_status=False

    return jsonify(total_coverage=total_coverage,coverage_status=coverage_status)


@app.route("/set_file_content", methods=["GET", "POST"])
def set_files_content():
    data = request.form
    file_id, is_history = get_file_id_by_filename(
        data["filename"], active_only=True)
    file_details = get_latest_file_details(file_id)

    if file_details is not None:
        # insert file content
        sql = "UPDATE file_details SET file_content= :v WHERE ID=:id"
        param = {"v": data["file_content"], "id": int(file_details[0])}
        execute_query(sql, param)
        return "200"
    else:
        return "400"


@app.route("/get_file_content")
def get_files_content():
    filename = request.args["filename"]
    session = request.args["session_id"]

    files=[]
    has_multiple_sessions=False
    sessions_list=[]
    if ',' in session:
        # session var contains multiple session ids, because it's a build report
        sessions_list=session.split(',')
        files = get_all_files_for_sessions(sessions_list)
        has_multiple_sessions=True
    else:
        # need to find file we want content of in all files related to the session
        files = get_all_files_for_session(int(session))
    
    content = None
    executable_lines = []
    executed_line_numbers_list = []
    for f in files:
        if f[0][2] == filename:
            content = f[1][2]
            if f[1][5] is not None:
                executable_lines = f[1][5].split(',')
            # get stats for the file and this session
            stats=[]
            if has_multiple_sessions:
                stats = get_stats_for_file_for_multiple_sessions(f[0][0], sessions_list)
            else:
                stats = get_stats_for_file(f[0][0], session)
            for s in stats:
                executed_line_numbers_list.append(s[5])
            break
    decoded_content = base64.b64decode(content)
    return jsonify(decoded_content_string=decoded_content, executed_lines=executed_line_numbers_list, executable_lines=executable_lines)


@app.route("/set_current_test", methods=["GET"])
def set_active_test():
    data = request.args
    current_test = get_config_value("CURRENT_TEST_NAME")
    if current_test == None:
        sql = "INSERT INTO config(name,value) VALUES(?,?)"
        param = ("CURRENT_TEST_NAME", data["name"])
        execute_query(sql, param)
        sql = "INSERT INTO config(name,value) VALUES(?,?)"
        param = ("CURRENT_TEST_ID", data["test_id"])
        execute_query(sql, param)
    else:
        sql = "UPDATE config SET value= :v WHERE name= :n"
        params = {"n": "CURRENT_TEST_NAME", "v": data["name"]}
        execute_query(sql, params)
        sql = "UPDATE config SET value= :v WHERE name= :n"
        params = {"n": "CURRENT_TEST_ID", "v": data["test_id"]}
        execute_query(sql, params)
    refresh_config()
    # also save what module is touched by this test
    active_test_session = get_active_test_session()
    if active_test_session == None:
        active_test_session = -1
    else:
        active_test_session = active_test_session[0]
    sql = "INSERT INTO visited_modules(module_touched,session_id) VALUES(?,?)"
    params = (data["touched_module"], active_test_session)
    execute_query(sql, params)

    return "200"


@app.route("/reset_current_test", methods=["GET"])
def reset_active_test():
    sql = 'DELETE FROM config WHERE name="CURRENT_TEST_NAME"'
    execute_query(sql)
    sql = 'DELETE FROM config WHERE name="CURRENT_TEST_ID"'
    execute_query(sql)
    refresh_config()
    return "200"


@app.route("/set_executable_lines_count_for_file", methods=["GET", "POST"])
def set_executable_lines_count_for_file():

    data = request.args
    file_id, is_history = get_file_id_by_filename(
        data["file"], active_only=True)
    file_details = get_latest_file_details(file_id)

    if file_details is not None:
        # insert file content
        sql = "UPDATE file_details SET executable_lines_count= :v,executable_lines= :el WHERE ID=:id"
        param = {"v": data["count"], "el": data[
            "executable"], "id": int(file_details[0])}
        execute_query(sql, param)
        return "200"
    return "204"


@app.route("/get_sources", methods=["GET"])
def get_sources():
    sources = get_all_active_files_without_module()
    sources_json = []

    parents = []
    # first need tgo find parents
    for row in sources:
        dir_name = os.path.dirname(row[2])
        if dir_name not in parents:
            parents.append(dir_name)

    # now files
    for parent in parents:

        parent_id = "parent_folder_" + str(uuid.uuid4())
        # create folder
        source_data = {}
        source_data["id"] = parent_id
        source_data["text"] = parent
        source_data["parent"] = "#"
        sources_json.append(json.dumps(source_data))

        # add related files
        for row in sources:
            source_dir = os.path.dirname(row[2])
            if parent == source_dir:
                source_data = {}
                source_data["id"] = row[0]
                source_data["text"] = row[1]
                source_data["parent"] = parent_id
                source_data["type"] = "source"
                sources_json.append(json.dumps(source_data))

    return jsonify(sources=sources_json, sources_count=len(sources))


@app.route("/create_module", methods=["POST"])
def create_module():
    data = request.data

    module_already_exists, module = check_if_module_exists(
        data)  # returns true/false and module object if exists
    if module_already_exists:
        return "duplicate"

    insert_new_module(data, None)

    return '',status.HTTP_200_OK


@app.route("/create_user", methods=["POST"])
def create_user():
    data = request.data

    user_already_exists, user= check_if_user_exists(
        data)  # returns true/false and user object if exists
    if user_already_exists:
        return "duplicate"

    insert_new_user(data)

    return '',status.HTTP_200_OK

@app.route("/create_tag", methods=["POST"])
def create_tag():
    data = request.data

    tag_already_exists, tag= check_if_tag_exists(
        data)  # returns true/false and user object if exists
    if tag_already_exists:
        return "duplicate"

    insert_new_tag(data)

    return '',status.HTTP_200_OK

@app.route("/assign_file", methods=["POST"])
def assign_file_to_module():
    '''
    assign file always to module that is active
    :return:
    '''
    data = json.loads(request.data)
    module_id = data["module_id"]
    param_files = data["sources"]

    # update list of related files
    module = get_module(module_id)
    related_files = module[2]

    if related_files == None:
        related_files = []
    else:
        # because related files are stored as comma separated string
        related_files = related_files.split(',')

    for f in param_files:
        if f not in related_files:
            related_files.append(f)

    # make current module a history, a reference only because there might be a related session to the files in that
    # module and the file state needs to stay the same for this module instance, for reference.
    # create a copy of the module with new files that will be active from now
    # on until another update
    sql = "UPDATE modules SET is_history=1,last_update=:lupd,operation=:op WHERE ID=:mid"
    param = {"mid": int(module_id), "lupd": datetime.datetime.now().strftime(
        "%Y-%m-%d %H:%M:%S"), "op": "add_sources"}
    execute_query(sql, param)

    # now new one
    insert_new_module(module[1], related_files)

    return '',status.HTTP_200_OK


@app.route("/unassign_file", methods=["POST"])
def unassign_file():
    data = json.loads(request.data)

    param_files = []
    for source in data["sources"]:
        param_files.append(source.split('_')[1])

    # update list of related files
    unassign_source_from_module(param_files)

    return '',status.HTTP_200_OK


@app.route("/remove_module", methods=["POST"])
def remove_module():
    data = json.loads(request.data)
    modules_to_remove = data["modules"]

    for m in modules_to_remove:
        module = get_module(m)
        related_files = module[2]

        # make current module a history
        sql = "UPDATE modules SET is_history=1,is_removed=1,last_update=:lupd,operation=:op WHERE ID=:mid"
        param = {"mid": int(m), "lupd": datetime.datetime.now().strftime(
            "%Y-%m-%d %H:%M:%S"), "op": "delete"}
        execute_query(sql, param)

    return '',status.HTTP_200_OK



@app.route("/remove_user", methods=["POST"])
def remove_user():
    data = json.loads(request.data)
    users_to_remove=data["users"]
    for user_to_remove in users_to_remove:
        user_id_to_remove = int(user_to_remove)

        if user_id_to_remove>0:
            sql = "UPDATE users SET is_history=1 WHERE ID=:uid"
            param = {"uid": user_id_to_remove}
            execute_query(sql, param)
      
        else:
            print("attempted to remove default user. Skipped.")
    return '',status.HTTP_200_OK


@app.route("/remove_tags", methods=["POST"])
def remove_tags():
    data = json.loads(request.data)
    tags_to_remove=data["tags"]
    for tag_to_remove in tags_to_remove:
        tag_id_to_remove = int(tag_to_remove)

        if tag_id_to_remove>0:
            sql = "UPDATE tags SET is_history=1 WHERE ID=:uid"
            param = {"uid": tag_id_to_remove}
            execute_query(sql, param)
      
        else:
            print("attempted to remove default tag. Skipped.")
    return '',status.HTTP_200_OK


@app.route("/get_modules", methods=["GET"])
def get_modules():
    '''
    select all modules from table

    :return:
    '''
    modules_jsons = []

    with_files_only = False
    session_id = None

    if request.args is not None and len(request.args) > 0:

        with_files_only = request.args["with_files_only"]
        session_id = request.args["session_id"]

    if session_id != 'None':
        modules_list = get_session_modules(session_id)
    else:
        modules_list = get_all_modules()

    modules_count = len(modules_list)

    # prepare modules json
    for row in modules_list:
        has_related_files = False
        if row[2] is not None and len(row[2]) > 0:
            has_related_files = True

        if with_files_only == "True" and has_related_files == False:
            continue  # skip as we want all modules with files and this entry has no files

        modules_data = {}
        modules_data["id"] = row[0]
        modules_data["text"] = row[1]
        modules_data["parent"] = "#"
        modules_jsons.append(json.dumps(modules_data))


        # get related files
        if with_files_only == "False":  # if want 'with_files_only' then do not return related files,just modules that have files
            if has_related_files:
                related_files = row[2].split(',')
                for related_file in related_files:

                    modules_data = {}
                    modules_data["text"], is_history = get_filename_by_id(
                        related_file, False)

                    if is_history and session_id == 'None':
                        continue  # if file is history,skip it; we want to show only active sources in module

                    modules_data["id"] = str(row[0]) + "_" + str(related_file)
                    modules_data["parent"] = row[0]
                    modules_data["type"] = "module_source"
                    modules_data["state"] = {
                        "opened": False, "disabled": False, "selected": False}
                    modules_jsons.append(json.dumps(modules_data))
                   

    return jsonify(modules=modules_jsons, modules_count=modules_count)


@app.route("/get_available_builds", methods=["GET"])
def get_available_builds():
    builds_json=[] # in a form of : tag - child builds[]
    all_tags=get_all_active_tags()
    for tag_row in all_tags:
        tag_data={}
        tag_data["id"]="tag_"+str(tag_row[0])
        tag_data["tag_id"]=tag_row[0]
        tag_data["parent"]="#"
        tag_data["text"]=tag_row[1]
        tag_data["a_attr"]={"class":"jstree_hide_checkbox"}
        builds_json.append(json.dumps(tag_data)) #parents must be present too
        # get all builds for the tag
        builds=get_build_for_tag(tag_row[0])
        for build_row in builds:
            build_data={}
            build_data["id"]="build_"+str(build_row[0])
            build_data["build_id"]=build_row[0]
            build_data["text"]=str(build_row[1])
            build_data["parent"]="tag_"+str(tag_row[0])
            build_data["parent_tag"]=tag_row[1]
            build_data["type"] = "build"
            build_data["state"] = {
                "opened": False, "disabled": False, "selected": False}
            builds_json.append(json.dumps(build_data))

    return jsonify(builds=builds_json)



@app.route("/set_coverage_goal_value", methods=["POST"])
def set_coverage_goal_value():
    data = json.loads(request.data)
    coverage_value = data["coverage_value"]
    set_config_value("MIN_BUILD_COVERAGE_VALUE",coverage_value)
    return "201"



@app.route("/set_config_values", methods=["POST"])
def set_config_values():
    data = request.form
    for a in data:
        param = None
        value = get_config_value(a)
        if value != None:
            sql = "UPDATE config SET value= :v WHERE name= :n"
            param = {"v": data[a], "n": a}
        else:
            sql = "INSERT INTO config(name,value) VALUES(?,?)"
            param = (a, data[a])
        execute_query(sql, param)
    refresh_config()
    return "200"



@app.route("/set_routes", methods=["GET"])
def set_routes():
    # we are overwriting ,so clear data first
    sql = "DELETE FROM routes"
    execute_query(sql)
    # now save data again
    data = request.args
    for a in data:
        sql = "INSERT INTO routes(route) VALUES(:r)"
        param = {"r": data[a]}
        execute_query(sql, param)

    return "200"


@app.route("/set_modules", methods=["GET"])
def set_modules():
    # we are overwriting ,so clear data first
    sql = "DELETE FROM modules"
    execute_query(sql)
    # now save data again
    data = request.args
    for a in data:
        sql = "INSERT INTO modules(module) VALUES(:m)"
        param = {"m": data[a]}
        execute_query(sql, param)
    return "200"


@app.route("/get_js_to_instrument")
def get_js_to_instrument():
    files = []
    sql = "SELECT name FROM files WHERE should_instrument=1 AND is_history=0"
    conn = sqlite3.connect(CONNECTION_STRING)
    c = conn.cursor()
    c.execute(sql)
    files = c.fetchall()
    conn.close()
    return jsonify(js_to_instrument=files)


@app.route("/get_instrument_function_name")
def get_instrument_function_name():
    return jsonify(instrument_function_name=CONFIG["INSTRUMENT_JS_FUNCTION_NAME"])


@app.route("/get_test_session_status")
def get_test_session_status():
    session_is_active = False
    active_session = get_active_test_session()
    if active_session is not None:
        session_is_active = True
    return jsonify(test_session_status=session_is_active)


@app.route("/version")
def version():
    return str(get_config_value("VERSION"))


@app.route("/get_current_coverage")
def get_current_coverage():
    session = request.args["session_id"]
    is_session_over = check_if_session_is_over(session)
    if is_session_over == None or is_session_over == True:
        is_session_over = "true"
    else:
        is_session_over = "false"
    file = {}
    file_details = []
    all_executable = 0
    covered_modules = []
    all_executed = 0
    total_coverage_percent = 0
    all_files_to_instrument = get_all_active_files()

    # loop for each file that was instrumented
    for r in all_files_to_instrument:
        file = {}

        # now count line executions in this file and session
        executions = 0
        executions_result = get_execution_count_for_session(r[0], session)
        if executions_result != None:
            executions = float(executions_result[0])

        file["filename"] = r[1]
        file["id"] = r[0]
        file["executable"] = r[4]
        file["executed"] = executions
        file["percent_executed"] = (executions / float(r[4])) * 100
        # also, I want a total number of all executable lines across all files
        all_executable += float(r[4])
        # and total of executed lines across all files
        all_executed += executions
        file_details.append(file)

    # return total coverage in percent also
    if all_executable > 0 and all_executed > 0:
        total_coverage_percent = (all_executed / all_executable) * 100

    # covered routes
    covered_routes = get_covered_routes(session)
    # covered modules
    covered_modules = get_covered_modules(session)
    return jsonify(covered_modules_list=covered_modules, covered_routes_list=covered_routes, test_coverage=file_details, executable=all_executable, executed=all_executed, total_coverage_value=total_coverage_percent, session_over=is_session_over)


@app.route("/get_sessions")
def get_sessions():
    sessions_list = get_all_sessions()
    return jsonify(sessions=sessions_list)

@app.route("/get_active_users", methods=["GET"])
def get_active_users():
    '''
    select all active users
    :return:
    '''
    user_jsons = []
    users=get_all_active_users()

    for row in users:
        user_data={}
        user_data["username"]=row[1]
        user_data["text"] = row[1]
        # js node specific
        user_data["id"] = str(row[0]) 
        user_data["type"] = "user_entry"
        user_data["state"] = {"opened": False, "disabled": False, "selected": False}
        user_jsons.append(json.dumps(user_data))

    return jsonify(users=user_jsons, user_count=len(user_jsons))

@app.route("/get_active_tags", methods=["GET"])
def get_active_tags():
    '''
    select all active tags
    :return:
    '''
    tag_jsons = []
    tags=get_all_active_tags()

    for row in tags:
        tag_data={}
        tag_data["tag"]=row[1]
        tag_data["text"] = row[1]
        # js node specific
        tag_data["id"] = str(row[0]) 
        tag_data["type"] = "tag_entry"
        tag_data["state"] = {"opened": False, "disabled": False, "selected": False}
        tag_jsons.append(json.dumps(tag_data))

    return jsonify(tags=tag_jsons, tag_count=len(tag_jsons))


############################## /API ##########################################

###################### TEMPLATES #########################################

@app.route("/dashboard")
def view_dashboard():
    sessions_list = get_all_sessions()
    # get version
    version = get_config_value("VERSION")
    p_name = get_config_value("PROJECT_NAME")
    # get set minimum coverage value
    min_coverage_value=0
    min_coverage_value=get_config_value("MIN_BUILD_COVERAGE_VALUE")
    if min_coverage_value==None:
        # get default coverage value
        min_coverage_value=get_config_value("DEFAULT_MIN_COVERAGE_PERCENT")

    build_number=34404 #ERROR ;)
    build_number,tag_name,build_id=get_latest_build_number()
    # calculate total coverage 
    total_coverage_for_build=calculate_total_coverage_for_build(build_id)
    # is coverage goal reached or not?
    coverage_status_fail_value=False
    if total_coverage_for_build<float(min_coverage_value):
        coverage_status_fail_value=True
    # sources without module
    new_sources_count = len(get_all_active_files_without_module())
    return render_template('dashboard.html', sessions=sessions_list, app_version=version, project_name=p_name, sources_count=new_sources_count, coverage_status_fail=coverage_status_fail_value,min_coverage_value=min_coverage_value,latest_build_num=build_number,latest_tag_name=tag_name,total_build_coverage=total_coverage_for_build)



@app.route("/about")
def view_about():
    return render_template('about.html')


@app.route("/report/latest")
def view_latest_report():
    # get latest session id 
    latest_session_id=get_latest_finished_test_session_id()
    return redirect("/report/"+latest_session_id, code=302)


@app.route("/report/<session>")
def view_report(session=None):
    output_template=prepare_report_page(session)
    return output_template


@app.route("/report/build/<build_number>/tag/<tag_name>")
def view_build_report(build_number,tag_name):
    '''
    generate report for build number of specific tag
    and also save as html to drive
    '''
    output_template,export_template=prepare_report_page_for_build(build_number,tag_name)
    with open("build_report.html", "wb") as f:
        f.write(export_template.encode('utf-8').strip())
    return output_template
  
########################### /TEMPLATES ###################################


########################## /VIEWS #############################################

#######UTIL############
def execute_query(sql, params=None):
    '''
    generic method used to execute sql query against db
    :param sql:
    :param params:
    :return:
    '''
    result = None
    conn = sqlite3.connect(CONNECTION_STRING)
    c = conn.cursor()
    if params != None:
        result = c.execute(sql, params)
    else:
        result = c.execute(sql)

    conn.commit()
    conn.close()
    return result.lastrowid


def execute_select(sql, params, fetchall):
    rows = []
    conn = sqlite3.connect(CONNECTION_STRING)
    cursor = conn.cursor()
    if params is not None:
        cursor.execute(sql, params)
    else:
        cursor.execute(sql)
    if fetchall:
        rows = cursor.fetchall()
    else:
        rows = cursor.fetchone()
    conn.close()
    return rows


def get_execution_count_for_session(file_id, session_id):
    '''
    each test session has some files that were instrumented
    get unique number of executed lines for specified file in specified session
    (across all tests)
    :param file_id:
    :param session_id:
    :return:
    '''
    sql = "SELECT COUNT(DISTINCT line_guid) FROM stats WHERE session_id= :si AND file_id=:f"
    param = {"si": int(session_id), "f": int(file_id)}
    executions = execute_select(sql, param, fetchall=False)
    return executions

def get_execution_count_for_sessions(file_id, session_id_list):
    '''
    the same as get_execution_count_for_session but get stats for file across multiple sessions not 1
    '''
    sql = "SELECT COUNT(DISTINCT line_guid) FROM stats WHERE session_id IN("+','.join(map(str, session_id_list))+") AND file_id=:f"
    param = {"f": int(file_id)}
    executions = execute_select(sql, param, fetchall=False)
    return executions

def get_stats_asc_sorted(session_id):
    '''
    get list of stats for given session
    sorted asc by date to get a list of stats
    from least to most recent 
    '''
    sql = "SELECT filename,line,send_time,custom_value FROM stats WHERE session_id= :si ORDER BY send_time ASC"
    param = {"si": int(session_id)}
    stats= execute_select(sql, param, fetchall=True)
    return stats

def get_stats_asc_sorted_for_multiple_sessions(session_id_list):
    sql = "SELECT filename,line,send_time,custom_value FROM stats WHERE session_id IN ("+','.join(map(str, session_id_list))+") ORDER BY send_time ASC"
    stats= execute_select(sql, None, fetchall=True)
    return stats

def get_latest_file_details(file_id):
    '''
    obtain latest file details entry for specified file
    :param file_id:
    :return:
    '''
    sql = "SELECT * FROM file_details WHERE file_id=:fid ORDER BY updated DESC LIMIT 1;"
    param = {"fid": file_id}
    row = execute_select(sql, param, fetchall=False)
    return row

def get_latest_finished_test_session_id():
    sql = "SELECT ID FROM sessions ORDER BY end_time DESC LIMIT 1;"
    row = execute_select(sql, None, fetchall=False)
    return str(row[0])

def get_file_details(details_id):
    '''

    :param id: row ID (file details id)
    :return:
    '''
    sql = "SELECT * FROM file_details WHERE ID=:id"
    param = {"id": int(details_id)}
    row = execute_select(sql, param, fetchall=False)
    return row


def get_file_id_by_filename(file_name, active_only=False):
    sql = ""
    if active_only:
        sql = "SELECT ID,is_history FROM files WHERE name=:fn AND is_history=0"
    else:
        sql = "SELECT ID,is_history FROM files WHERE name=:fn"
    param = {"fn": file_name}
    row = execute_select(sql, param, fetchall=False)
    if row is not None:
        return row[0], row[1]
    else:
        return None, True


def get_filename_by_id(file_id, active_only=False):
    sql = ""
    if active_only:
        sql = "SELECT name,is_history FROM files WHERE ID=:fid AND is_history=0"
    else:
        sql = "SELECT name,is_history FROM files WHERE Id=:fid"
    param = {"fid": int(file_id)}
    row = execute_select(sql, param, fetchall=False)
    if row is not None:
        return row[0], row[1]
    else:
        return None, True


def get_stats_for_file(file_id, session_id):
    stats = []
    sql = "SELECT * FROM stats WHERE file_id=:fid AND session_id=:sid"
    param = {"fid": file_id, "sid": session_id}
    stats = execute_select(sql, param, fetchall=True)
    return stats

def get_stats_for_file_for_multiple_sessions(file_id,session_id_list):
    stats = []
    sql = "SELECT * FROM stats WHERE file_id=:fid AND session_id IN("+','.join(map(str, session_id_list))+")"
    param = {"fid": file_id}
    stats = execute_select(sql,param, fetchall=True)
    return stats

def get_stats_for_session(session_id):
    stats = []
    sql = "SELECT * FROM stats WHERE session_id=:sid"
    param = {"sid": session_id}
    stats = execute_select(sql, param, fetchall=True)

    return stats

def get_latest_stat_for_session(session_id):
    stats = []
    sql = "SELECT date FROM stats WHERE session_id=:sid ORDER BY date DESC"
    param = {"sid": session_id}
    stat = execute_select(sql, param, fetchall=False)
    stat_date=datetime.datetime(1999,1,1)
    if stat is not None:
        stat_date=datetime.datetime.strptime(stat[0],"%Y-%m-%d %H:%M:%S")

    return stat_date

def get_executable_lines_count_for_file(file_id):
    sql = "SELECT executable_lines_count,name,ID FROM files where ID= :id"
    param = {"id": file_id}
    count = execute_select(sql, param, fetchall=False)
    return count


def get_all_files_for_session(session_id):
    results = []
    sql = "SELECT file_id,file_details FROM sessions_files WHERE session_id=:sid"
    param = {"sid": session_id}
    results = execute_select(sql, param, fetchall=True)
    return get_file(results)


def get_all_files_for_sessions(session_id_list):
    results = []
    sql = "SELECT DISTINCT file_id,file_details FROM sessions_files WHERE session_id IN("+','.join(map(str, session_id_list))+")"
    results = execute_select(sql, None, fetchall=True)
    return get_file(results)


def get_session_file(session_id, file_id):
    results = []
    sql = "SELECT file_id,file_details FROM sessions_files WHERE session_id=:sid AND file_id=:fid"
    param = {"sid": int(session_id), "fid": int(file_id)}
    results = execute_select(sql, param, fetchall=True)
    return results


def get_all_sessions():
    session = {}
    covered_modules = []
    sessions_list = []

    # get all test sessions
    sql = "SELECT ID, is_over, name,total_coverage,start_time,end_time,current_active_modules_count  FROM sessions"
    results = execute_select(sql, None, fetchall=True)
    for r in results:
        session = {}
        session["ID"] = r[0]
        if r[1] == 1:
            session["is_active"] = "false"
        else:
            session["is_active"] = "true"
        session["name"] = r[2]
        session["total_coverage"] = round(float(r[3]), 1)
        session["start_date"] = r[4]
        session["end_date"] = r[5]

        # modules - count active modules,covered module + files in active and
        # covered modules
        covered_modules = get_covered_modules(r[0])
        for cm in covered_modules:
            module = get_module(cm[0], active_only=False)

        # active modules count at the time of creating the session
        session["active_modules_count"] = r[6]
        session["covered_modules_count"] = len(covered_modules)

        # get owner, build and tag
        session["build"]=get_session_build(session["ID"])
        tag,owner=get_session_tag_owner(session["ID"])
        session["tag"]=tag
        session["owner"]=owner

        sessions_list.append(session)
    return sessions_list


def get_session(session_id):
    results = []
    sql = "SELECT * FROM sessions WHERE ID=:sid"
    param = {"sid": int(session_id)}
    result = execute_select(sql, param, fetchall=False)
    return result



def get_session_by_name(session_name):
    results = []
    sql = "SELECT * FROM sessions WHERE name=:name"
    param = {"name": session_name.lower()}
    result = execute_select(sql, param, fetchall=False)
    return result


def get_all_active_files_without_module():
    active_modules = get_all_active_modules()
    active_files = get_all_active_files()
    active_files_without_module = []

    for active_file in active_files:
        found_file = False
        for module in active_modules:  # 1 file can live only in 1 module
            related_files = []
            if module[2] is not None and len(module[2]) > 0:
                related_files = module[2].split(',')
                for related_file in related_files:
                    if int(related_file) == int(active_file[0]):
                        found_file = True
                        break
        if not found_file:
            active_files_without_module.append(active_file)

    return active_files_without_module


def get_all_active_modules():
    results = []
    sql = "SELECT * FROM modules WHERE is_history=0 AND is_removed=0"
    results = execute_select(sql, None, fetchall=True)
    return results


def get_file_list_for_live_test_session():
    '''
    return file list among with related file details for live session
    empty if no live session
    :param session_id:
    :return:
    '''
    results = None
    files = []
    sql = "SELECT file_id,file_details,session_id FROM sessions_files INNER JOIN sessions ON sessions_files.session_id=sessions.ID WHERE sessions.is_over=0"
    results = execute_select(sql, None, fetchall=True)

    live_session_id = None

    if results is not None:
        # now get more details about the files
        for r in results:
            sql = "SELECT * FROM files WHERE ID=:fid"
            param = {"fid": r[0]}
            file = execute_select(sql, param, fetchall=False)

            # and files details
            sql = "SELECT * FROM file_details WHERE ID=:fdid"
            param = {"fdid": r[1]}
            file_details = execute_select(sql, param, fetchall=False)

            files.append((file, file_details))

            if live_session_id is None:
                live_session_id = r[2]

    return files, live_session_id

def get_file_and_file_details_list_for_test_session(session_id):
    '''
    return file list among with related file details for given test session
    :return:
    '''
    results = None
    files = []
    sql = "SELECT file_id,file_details,session_id FROM sessions_files INNER JOIN sessions ON sessions_files.session_id=sessions.ID WHERE sessions_files.session_id=:sid"
    param={"sid":session_id}
    results = execute_select(sql, param, fetchall=True)

  

    if results is not None:
        # now get more details about the files
        for r in results:
            sql = "SELECT * FROM files WHERE ID=:fid"
            param = {"fid": r[0]}
            file = execute_select(sql, param, fetchall=False)

            # and files details
            sql = "SELECT * FROM file_details WHERE ID=:fdid"
            param = {"fdid": r[1]}
            file_details = execute_select(sql, param, fetchall=False)

            files.append((file, file_details))
    return files


def check_if_session_is_over(s_id):
    is_session_over = False
    sql1 = "SELECT is_over FROM sessions WHERE ID= :si"
    param = {"si": int(s_id)}
    is_session_over = int(execute_select(sql1, param, fetchall=False)[0])

    if is_session_over == 0:
        return False
    else:
        return True


def get_file_list_for_test_session(session_id):
    sql = "SELECT DISTINCT(file_id) FROM stats WHERE session_id= :si"
    param = {"si": session_id}
    results = execute_select(sql, param, fetchall=True)
    return results


def get_file(file_ids):
    files = []
    for f in file_ids:
        file_details = get_file_details(f[1])
        sql = "SELECT * FROM files WHERE ID= :fid"
        param = {"fid": f[0]}
        files.append(
            (execute_select(sql, param, fetchall=False), file_details))
    return files


def get_all_active_files():
    """
    all files that are not history
    """
    sql = "SELECT * FROM files WHERE should_instrument=1 AND is_history=0"
    results = execute_select(sql, None, fetchall=True)
    return results

def get_all_active_users():
    sql = "SELECT * FROM users WHERE is_history=0"
    results = execute_select(sql, None, fetchall=True)
    return results



def get_all_users():
    sql = "SELECT * FROM users"
    results = execute_select(sql, None, fetchall=True)
    return results


def get_all_active_tags():
    sql = "SELECT * FROM tags WHERE is_history=0"
    results = execute_select(sql, None, fetchall=True)
    return results


def get_all_tags():
    sql = "SELECT * FROM tags"
    results = execute_select(sql, None, fetchall=True)
    return results


def get_build_id(build_number,tag_id):
    '''
    get build id for given build number and tag
    1 build number can exist within 1 tag
    if does not exist, insert
    '''
    build_id=-1
    sql="SELECT ID from builds WHERE build= :b AND tag_id=:tid"
    params= {"b":build_number,"tid":tag_id}
    result= execute_select(sql, params, fetchall=True)
    
    if len(result)==0:
        sql = "INSERT INTO builds(build,tag_id,update_date) VALUES(?,?,?)"
        build_id=execute_query(sql, (build_number,tag_id,datetime.datetime.now().strftime(
        "%Y-%m-%d %H:%M:%S")))
    else:
        build_id=result[0][0]
    return build_id


def insert_session_build_relation(session_id,build_id):
    sql = "INSERT INTO sessions_builds(session_id,build_id) VALUES(?,?)"
    execute_query(sql, (session_id,build_id))

def insert_session_user_tag_relation(session_id,user_id,tag_id):
    sql = "INSERT INTO sessions_users_tags(session_id,user_id,tag_id) VALUES(?,?,?)"
    execute_query(sql, (session_id,user_id,tag_id))



def get_config():
    """
    get config values from database
    :return:
    """
    CONFIG.clear()  # clear config
    sql = "SELECT * FROM config"
    results = execute_select(sql, None, fetchall=True)
    # iterate through the results now...
    for result_row in results:
        CONFIG[result_row[1]] = result_row[2]


def get_config_value(key_p):
    get_config() # refresh config
    for key, value in CONFIG.iteritems():
        if key == key_p:
            return value
    return None

def set_config_value(key,value):
    check_value = get_config_value(key)
    if check_value != None:
        sql = "UPDATE config SET value= :v WHERE name= :n"
        param = {"n": key, "v": value}
    else:
        sql = "INSERT INTO config(name,value) VALUES(?,?)"
        param = (key, value)
    execute_query(sql, param)

def get_active_test_session():
    """
    connect to db and get active test session if any
    :return: active session ID or -1 if no active session
    """
    result = None
    sql = "SELECT * FROM sessions WHERE is_over=0"
    result = execute_select(sql, None, fetchall=False)
    return result


def save_visited_route(url, session_id):
    '''
    save visited url
    '''
    sql = "INSERT INTO visited_routes(route_visited,session_id) VALUES(?,?)"
    execute_query(sql, (url, session_id))


def get_covered_modules(session_id):
    sql = "SELECT module_id FROM covered_modules WHERE session_id=:sid"
    params = {"sid": session_id}
    covered_modules = execute_select(sql, params, fetchall=True)
    return covered_modules


def get_covered_routes(session_id):
    """
    distinct list of route:visited true/false pair for the session
    """
    routes_list = []
    route_dict = {}
    sql = "SELECT route FROM routes"
    routes = execute_select(sql, None, fetchall=True)
    if routes != None:
        for route in routes:
            route_dict = {}
            sql = "SELECT ID FROM visited_routes WHERE session_id=:sid AND route_visited=:rv"
            params = {"sid": session_id, "rv": route[0]}
            route_id = execute_select(sql, params, fetchall=False)
            route_dict["route"] = route[0]
            if route_id != None: # if route id is not none then it exists in visited routes for this session , so this
                # session visited the route
                # this route was visited
                route_dict["visited"] = "true"
            else:
                route_dict["visited"] = "false"
            routes_list.append(route_dict)
    return routes_list

def get_covered_routes_for_sessions(session_id_list):
    routes_list = []
    route_dict = {}
    sql = "SELECT route FROM routes"
    routes = execute_select(sql, None, fetchall=True)
    if routes != None:
        for route in routes:
            route_dict = {}
            # get sessions that visited this route
            sql = "SELECT ID FROM visited_routes WHERE session_id IN("+','.join(map(str, session_id_list))+") AND route_visited=:rv"
            params = {"rv": route[0]}
            route_ids = execute_select(sql, params, fetchall=True)
            route_dict["route"] = route[0]
            if route_ids is not None:
                # some session visited this route because at least 1 route id was obtained from db
                if route_id != None:
                    # this route was visited - set it to the dict entry
                    route_dict["visited"] = "true"
                else:
                    route_dict["visited"] = "false"
            routes_list.append(route_dict)
    return routes_list

def get_related_files_from_module(module_id):
    '''
    get list of related files from [related_files] column for specified module_id
    files are obtained ofr active module
    :param module_id:
    :return:
    '''
    sql = "SELECT related_files FROM modules WHERE ID=:mid AND is_history=0"
    param = {"mid": module_id}
    related_files = execute_select(sql, param, fetchall=False)
    if related_files is not None:
        return [int(file_id) for file_id in related_files[0].split(',')]
    return []


def calculate_total_coverage_for_active_session():
    '''
    calculate total coverage percentage for active session only
    :return:
    '''
    files, live_session_id = get_file_list_for_live_test_session()

    total_coverage = 0
    total_executed=0
    total_executable=0
    if live_session_id is not None:
        # stats=get_stats_for_session(live_session_id)


        for file_row in files:
            source = file_row[0]
            file_details = file_row[1]

            total_executable += float(file_details[3])

            total_executed += get_execution_count_for_session(
                source[0], live_session_id)[0]

        total_coverage = round(
            (float(total_executed) / total_executable) * 100, 1)

    return total_coverage, live_session_id,total_executed,total_executable


def calculate_total_coverage_for_build(build_id):
    '''
    get all sessions related to given build
    and calculate total coverage across those all sessions
    '''
    total_coverage=0
    total_executed=0
    total_executable=0

    build_sessions=get_sessions_for_build(build_id)

    sessions=[]
    for s in build_sessions:
        sessions.append(s[0])

    
    files=get_all_files_for_sessions(sessions)
    for f in files:
        executions_result = get_execution_count_for_sessions(f[0][0], sessions)
        if executions_result != None:
            executions = float(executions_result[0])
            total_executed+=executions
        total_executable+=float(f[1][3])


    # calc total coverage for all sessions related to specific build
    if total_executable>0:
        total_coverage = round(
                    (float(total_executed) / total_executable) * 100, 1)
    return total_coverage


def insert_new_module(name, related_files):
    '''
    creates a new module
    :param name:
    :param related_files:
    :return:
    '''
    related_files_string = None
    if related_files is not None and len(related_files) > 0:
        related_files_string = ','.join(related_files)
    sql = "INSERT INTO modules(module_name,related_files,last_update,is_removed,operation,is_history) VALUES(?,?,?,?,?,?)"
    param = (name.lower(), related_files_string, datetime.datetime.now().strftime(
        "%Y-%m-%d %H:%M:%S"), False, "insert", False)
    execute_query(sql, param)


def insert_new_user(username):
    sql="INSERT INTO users(username,is_history) VALUES(?,?)"
    param=(username,0)
    execute_query(sql,param)


def insert_new_tag(tag):
    sql="INSERT INTO tags(tag,is_history) VALUES(?,?)"
    param=(tag,0)
    execute_query(sql,param)


def get_module(module_id, active_only=True):
    '''
    get specific module
    :param module_id:
    :param active_only:
    :return:
    '''
    if active_only:
        sql = "SELECT * FROM modules WHERE ID=:mid AND is_history=0"
    else:
        sql = "SELECT * FROM modules WHERE ID=:mid AND is_history=1"
    param = {"mid": int(module_id)}
    module = execute_select(sql, param, fetchall=False)
    return module


def get_session_build(session_id):
    sql = "SELECT build FROM builds INNER JOIN sessions_builds ON builds.ID=sessions_builds.build_id WHERE sessions_builds.session_id=:sid"
    param = {"sid": session_id}
    results = execute_select(sql, param, fetchall=True)
    if len(results)>0:
        return results[0][0]
    else:
        return ""

def get_session_tag_name(session_id):
    sql = "SELECT tag FROM tags INNER JOIN sessions_users_tags ON tags.ID=sessions_users_tags.tag_id WHERE sessions_users_tags.session_id=:sid"
    param = {"sid": session_id}
    result = execute_select(sql, param, fetchall=False)
    if len(result)>0:
        return result[0]
    else:
        return ""


def get_session_tag_owner(session_id):
    # tag
    sql = "SELECT tag FROM tags INNER JOIN sessions_users_tags ON tags.ID=sessions_users_tags.tag_id WHERE sessions_users_tags.session_id=:sid"
    param = {"sid": session_id}
    result=execute_select(sql, param, fetchall=True)
    if len(result)>0:
        tag = result[0][0]
    else:
        tag=""
    # user
    sql = "SELECT username FROM users INNER JOIN sessions_users_tags ON users.ID=sessions_users_tags.user_id WHERE sessions_users_tags.session_id=:sid"
    param = {"sid": session_id}
    result= execute_select(sql, param, fetchall=True)
    if len(result)>0:
        user =result[0][0]
    else:
        user=""

    return tag,user


def unassign_source_from_module(param_files):
    '''

    :param module_id:
    :param param_files: list of files to remove from module
    :return:
    '''

    for source_file in param_files:

        all_modules = get_all_modules()  # all active modules
        for module in all_modules:
            related_files = module[2]

            if related_files is None:
                related_files = []
            else:
                # because related files are stored as comma separated string
                related_files = related_files.split(',')

            if str(source_file) in related_files:
                related_files.remove(str(source_file))

                # make current module a history
                sql = "UPDATE modules SET is_history=1,last_update=:lupd,operation=:op WHERE ID=:mid"
                param = {"mid": int(module[0]), "lupd": datetime.datetime.now().strftime(
                    "%Y-%m-%d %H:%M:%S"), "op": "remove_sources"}
                execute_query(sql, param)

                # now new one
                insert_new_module(module[1], related_files)


def get_all_modules(active_only=True):
    '''
    get all active or inactive
    :param active_only:
    :return:
    '''
    if active_only:
        sql = "SELECT * FROM modules WHERE is_history=0"
    else:
        sql = "SELECT * FROM modules"
    modules_list = execute_select(sql, None, fetchall=True)
    return modules_list


def get_all_module_ids(active_only=True):
    '''
    get id's of all modules
    '''
    if active_only:
        sql = "SELECT ID FROM modules WHERE is_history=0"
    else:
        sql = "SELECT ID FROM modules"
    modules_list = execute_select(sql, None, fetchall=True)
    # execute select returns tuples, we just want 0 element from each
    cleaned_modules_list = []
    for m in modules_list:
        cleaned_modules_list.append(m[0])
    return cleaned_modules_list


def get_session_modules(session_id):
    '''
    get list of modules related to specified test session
    :param session_id:
    :return:
    '''
    related_modules = get_covered_modules(session_id)
    modules = []

    for related_module in related_modules:
        sql = "SELECT * FROM modules WHERE ID=:mid"
        param = {"mid": related_module[0]}
        modules.append(execute_select(sql, param, fetchall=False))

    return modules


def check_if_module_exists(module_name):
    '''
    looks for active module by given name
    :param module_name:
    :return: true/false and module
    '''
    sql = "SELECT * FROM modules WHERE module_name=:mn AND is_history=0"
    param = {"mn": module_name.lower()}
    module = execute_select(sql, param, fetchall=False)
    if module is not None:
        return True, module
    else:
        return False, None

def check_if_user_exists(username):
    sql = "SELECT * FROM users WHERE username=:un AND is_history=0"
    param = {"un": username}
    user = execute_select(sql, param, fetchall=False)
    if user is not None:
        return True, user
    else:
        return False, None

def check_if_tag_exists(tag):
    sql = "SELECT * FROM tags WHERE tag=:t AND is_history=0"
    param = {"t": tag}
    tag = execute_select(sql, param, fetchall=False)
    if tag is not None:
        return True, tag
    else:
        return False, None


def make_all_files_historical():
    """
    when refreshed list of project's sources comes in,
    make all previously stored historical.
    We keep them to be able to show historical reports
    :return:
    """
    sql = "UPDATE files SET is_history=1"
    execute_query(sql)
    sql = "UPDATE files SET should_instrument=0"
    execute_query(sql)


def make_all_sessions_inactive():
    '''
    turn all active sessions into inactive
    :return:
    '''
    sql = "UPDATE sessions SET is_over=1,end_time=:end WHERE is_over=0"
    params = {"end": datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")}
    execute_query(sql, params)


def remove_test_session(session_id):
    '''
    remove specified test session
    :param session_id:
    :return:
    '''
    sql = "DELETE FROM sessions WHERE ID=:sid"
    param = {"sid": session_id}
    execute_query(sql, param)

    # remove from sessions builds
    sql = "DELETE FROM sessions_builds WHERE session_id=:sid"
    param = {"sid": session_id}
    execute_query(sql, param)

    # remove from sessions_users_tags
    sql = "DELETE FROM sessions_users_tags WHERE session_id=:sid"
    param = {"sid": session_id}
    execute_query(sql, param)


def count_module_related_files(module_row):
    '''
    count how many files a specific module has
    :param module_row: object representing row from modules table
    :return:
    '''
    if module_row is not None:
        if module_row[2] is not None:
            related_files = module_row[2].split(',')
            return len(related_files)

    return 0


def create_new_test_session(name, related_modules,build,user_id,tag_id):
    """
    add test session to db and set it to active
    @param related_modules: what modules this session instruments
    @param build: related build number
    @param user_id: user that created session/is responsible for the tests
    @param tag_id: tag for the session, for example release codename
    :return:
    """
    session = get_session_by_name(name)
    if session is not None:
        return False

    now = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    sql = "INSERT INTO sessions(is_over,name,total_coverage,start_time,end_time,current_active_modules_count,total_executable,total_executed) VALUES(?,?,?,?,?,?,?,?)"
    inserted_session_id = execute_query(
        sql, (0, name.lower(), "0", now, None, len(get_all_active_modules()),None,None))
    # insert related files
    # if did not specified related modules then assign all active modules to
    # the session
    if related_modules == None or len(related_modules) == 0 or related_modules[0]=="*":
        related_modules = get_all_module_ids()
    for module in related_modules:
        print("getting files for module: " + str(module))
        related_files = get_related_files_from_module(module)
        for source_id in related_files:
            file_details = get_latest_file_details(source_id)
            sql = "INSERT INTO sessions_files VALUES(?,?,?)"
            execute_query(sql, (inserted_session_id,
                                source_id, file_details[0]))

        sql = "INSERT INTO covered_modules(module_id,session_id) VALUES(?,?)"
        execute_query(sql, (int(module), inserted_session_id))

    # save build  
    build_id=get_build_id(build,tag_id) # if build exists for this tag, then id will be returned, none otherwise
    insert_session_build_relation(inserted_session_id,build_id)
    # save session user & tag
    insert_session_user_tag_relation(inserted_session_id,user_id,tag_id)

    return True


def get_active_files():
    '''
    get all active source files
    :return:
    '''
    sql = "SELECT * FROM files WHERE is_history=0"
    files = execute_select(sql, None, fetchall=True)
    return files


def get_active_file_details():
    '''
    get active files and latest details for each
    :return:
    '''
    file_details = []
    active_files = get_active_files()
    for active_file in active_files:
        details = get_latest_file_details(active_file[0])
        file_details.append(active_file[0], details[0])
    return file_details

def get_latest_build_number():
    '''
    find and return latest build number and also related tag name
    important: it finds build number that has some sessions assigned
    '''
    sql="SELECT * FROM builds ORDER BY update_date DESC"
    builds = execute_select(sql, None, fetchall=True)
    # return build number of first row-first row will be the latest build of all, because sort desc was applied
    # however, latest build must have some sessions, otherwise use other latest (if build has related sessions
    # that don't exist anymore, then it means sessions were removed).
    build_number=0
    build_id=-1
    related_tag_id=0
    for ct in range(len(builds)):
        build_id=builds[ct][0]
        build_number=builds[ct][1]
        related_tag_id=builds[ct][2]
        sql="SELECT * FROM sessions_builds WHERE build_id=:bid"
        param = {"bid": build_id}
        results=execute_select(sql, param, fetchall=True)
        if results is not None:
            if len(results)>0:
                break # found
    # get tag for build
    sql="SELECT tag FROM tags WHERE ID=:id"
    param = {"id": related_tag_id}
    tag_name=execute_select(sql, param, fetchall=True)[0][0] # row 0, column 0 
    return build_number,tag_name,build_id

def get_build_for_tag(tag_id):
    sql="SELECT * FROM builds WHERE tag_id=:tid"
    param={"tid":tag_id}
    builds = execute_select(sql, param, fetchall=True)
    return builds

def get_build_id_by_number_and_tag_name(build_number,tag_name):
    sql = "SELECT ID FROM tags WHERE tag=:tm"
    param={"tm":tag_name}
    tag_id=int(execute_select(sql, param, fetchall=False)[0])

    sql = "SELECT ID FROM builds WHERE tag_id=:tid AND build=:b"
    param={"tid":tag_id,"b":int(build_number)}
    build_id=int(execute_select(sql, param, fetchall=False)[0])

    return build_id



def get_build_number(build_id=None):
    '''
    find build_number and related tag based on build_id
    '''
    sql="SELECT * FROM builds WHERE ID=:bid"
    param={"bid":build_id}
    build = execute_select(sql, param, fetchall=True)

    build_number=build[1]
    related_tag_id=build[2]
    # get tag for build
    sql="SELECT tag FROM tags WHERE ID=:id"
    param = {"id": related_tag_id}
    tag_name=execute_select(sql, param, fetchall=True)[0][0] # row 0, column 0 
    return build_number,tag_name

def get_sessions_for_build(build_id):
    '''
    return list of session id's related to given build id and tag (because builds are unique within tags,tags are parents)
    '''
    sql="SELECT session_id FROM sessions_builds WHERE build_id=:bid"
    param={"bid":build_id}
    session_id_rows=execute_select(sql,param,fetchall=True)
    sessions=[]
    for session_id_row in session_id_rows:
        sessions.append(get_session(session_id_row[0]))
    return sessions

def prepare_report_page(session,use_embeded_template=False):
    file_details = []
    template_details = []
    all_executable = 0
    all_executed = 0
    total_coverage_percent = 0
    timeline_entries=[]
    has_timeline_entries=False

    modules_coverage = []

    template_to_use="report.html"

    if use_embeded_template:
        template_to_use="report_printable.html" # if creating output html, that is saved to disk, then use report 
        # html template but with all of the JS and CSS embeded in the html (no dependencies referenced) so it's a 
        # standalone html file

    build_number=get_session_build(session)
    session_tag=get_session_tag_name(session)
    # modules
    modules = []
    modules_list = get_session_modules(session)
    for module in modules_list:
        module_entry = {}
        module_entry["ID"] = module[0]
        module_entry["name"] = module[1]
        related_files = []
        related_files = module[2].split(',')
        module_entry["files"] = related_files
        module_entry["files_count"] = len(related_files)
        module_entry["coverage"] = 0

        modules.append(module_entry)

    all_files_to_instrument = get_all_files_for_session(session)

    # loop for each file that was instrumented
    for r in all_files_to_instrument:
        file = {}
        # now count line executions in this file and session
        executions = 0
        executions_result = get_execution_count_for_session(r[0][0], session)
        if executions_result != None:
            executions = float(executions_result[0])

        file["filename"] = r[0][2]
        file["id"] = str(r[0][0])

        executable_count = r[1][3]
        file["executable"] = executable_count
        file["executed"] = executions

        try:
            file["percent_executed"] = round(
                (executions / float(executable_count)) * 100, 1)
        except:
            file["percent_executed"] = 0
        # also, I want a total number of all executable lines across all files
        all_executable += float(executable_count)
        # and total of executed lines across all files
        all_executed += executions

        if os.path.splitext(file["filename"])[1] == ".html":
            template_details.append(file)
        else:
            file_details.append(file)

        # update module
        for m in modules:
            if file["id"] in m["files"]:
                file["module"] = m["name"]
                record_found = False
                for mc in modules_coverage:
                    if mc["module_id"] == m["ID"]:
                        record_found = True
                        # increase executable count by summing up executable
                        # count from all files of that module
                        mc["sources_executable"] += executable_count
                        mc["sources_executed"] += executions
                        break
                if not record_found:
                    modules_cov = {}
                    modules_cov["module_id"] = m["ID"]
                    modules_cov["sources_executable"] = executable_count
                    modules_cov["sources_executed"] = executions

                    modules_coverage.append(modules_cov)

                break  # 1 file can be in 1 module so do not iterate anymore if found

    # covered routes
    covered_routes = get_covered_routes(session)

    # set total coverage by module
    for m in modules:
        for mc in modules_coverage:
            if mc["module_id"] == m["ID"]:
                try:
                    m["total_coverage"] = round(
                        (mc["sources_executed"] / float(mc["sources_executable"])) * 100, 1)
                except:
                    m["total_coverage"] = 0
                m["total_executable"] = mc["sources_executable"]
                m["total_executed"] = mc["sources_executed"]

    # return total coverage in percent also
    if all_executable > 0 and all_executed > 0:
        total_coverage_percent = round(
            (all_executed / all_executable) * 100, 1)

    is_web_inject_mode = False
    inject_mode = get_config_value("CURRENT_INJECT_MODE")
    if inject_mode == "web" or inject_mode == "angular":
        is_web_inject_mode = True

    session_details = get_session(session)
    is_history = bool(int(session_details[1]))

    show_templates = False
    if len(template_details) > 0:
        show_templates = True

    # timeline 
    timeline_entries_temp=get_stats_asc_sorted(session)
    for tm_entry in timeline_entries_temp:
        timeline_entry={}
        timeline_entry["filename"]=tm_entry[0]
        timeline_entry["line"]=tm_entry[1]
        timeline_entry["send_time"]=tm_entry[2]
        timeline_entry["custom_value"]=tm_entry[3]
        timeline_entries.append(timeline_entry)
    if len(timeline_entries)>0:
        has_timeline_entries=True

    #return render_template('report.html', covered_modules=modules, covered_routes_list=covered_routes, file_details_list=file_details, template_details_list=template_details, total_executable=all_executable, total_executed=all_executed, session_id=session, total_coverage_value=total_coverage_percent, is_web_inject=is_web_inject_mode, is_history=is_history, session_name=session_details[2], session_start=session_details[4], session_end=session_details[5], show_templates=show_templates,timeline_entries=timeline_entries,has_timeline_entries=has_timeline_entries)
    output_template = render_template(template_to_use, covered_modules=modules, covered_routes_list=covered_routes, file_details_list=file_details, template_details_list=template_details, total_executable=all_executable, total_executed=all_executed, session_id=session, total_coverage_value=total_coverage_percent, is_web_inject=is_web_inject_mode, is_history=is_history, session_name=session_details[2], session_start=session_details[4], session_end=session_details[5], show_templates=show_templates,timeline_entries=timeline_entries,has_timeline_entries=has_timeline_entries,build_number=build_number,session_tag=session_tag)
    return output_template
    
def prepare_report_page_for_build(build_number,tag_name):
    file_details = []
    template_details = []
    all_executable = 0
    all_executed = 0
    total_coverage_percent = 0
    timeline_entries=[]
    has_timeline_entries=False

    modules_coverage = []
    session_ids=[]
    session_tag=tag_name
    modules = []

    # get build id for pair: build_name,tag_name
    build_id=get_build_id_by_number_and_tag_name(build_number,tag_name)
    build_sessions=get_sessions_for_build(build_id)

    session_list_string_array=[]
    session_list_string=""

    for session_row in build_sessions:
        session=session_row[0] #id
        if session not in session_ids:
            session_ids.append(session)
        if session not in session_list_string_array:
            session_list_string_array.append(str(session)) #holds session ids in string format
        # modules
        modules_list = get_session_modules(session)
        for module in modules_list:
            module_entry = {}
            module_entry["ID"] = module[0]
            module_entry["name"] = module[1]
            related_files = []
            related_files = module[2].split(',')
            module_entry["files"] = related_files
            module_entry["files_count"] = len(related_files)
            module_entry["coverage"] = 0

            if module_entry not in modules:
                modules.append(module_entry)

    all_files_to_instrument = get_all_files_for_sessions(session_ids)
    session_list_string=",".join(session_list_string_array)

    # loop for each file that was instrumented
    for r in all_files_to_instrument:
        file = {}
        # now count line executions in this file and session
        executions = 0
        executions_result = get_execution_count_for_sessions(r[0][0], session_ids)
        if executions_result != None:
            executions = float(executions_result[0])

        file["filename"] = r[0][2]
        file["id"] = str(r[0][0])

        executable_count = r[1][3]
        file["executable"] = executable_count
        file["executed"] = executions

        try:
            file["percent_executed"] = round(
                (executions / float(executable_count)) * 100, 1)
        except:
            file["percent_executed"] = 0
        # also, I want a total number of all executable lines across all files
        all_executable += float(executable_count)
        # and total of executed lines across all files
        all_executed += executions


        if os.path.splitext(file["filename"])[1] == ".html":
            template_details.append(file)
        else:
            file_details.append(file)

        # update module
        for m in modules:
            if file["id"] in m["files"]:
                file["module"] = m["name"]
                record_found = False
                for mc in modules_coverage:
                    if mc["module_id"] == m["ID"]:
                        record_found = True
                        # increase executable count by summing up executable
                        # count from all files of that module
                        mc["sources_executable"] += executable_count
                        mc["sources_executed"] += executions
                        break
                if not record_found:
                    modules_cov = {}
                    modules_cov["module_id"] = m["ID"]
                    modules_cov["sources_executable"] = executable_count
                    modules_cov["sources_executed"] = executions

                    modules_coverage.append(modules_cov)

                break  # 1 file can be in 1 module so do not iterate anymore if found

    # covered routes
    covered_routes = get_covered_routes_for_sessions(session_ids)

    # set total coverage by module
    for m in modules:
        for mc in modules_coverage:
            if mc["module_id"] == m["ID"]:
                try:
                    m["total_coverage"] = round(
                        (mc["sources_executed"] / float(mc["sources_executable"])) * 100, 1)
                except:
                    m["total_coverage"] = 0
                m["total_executable"] = mc["sources_executable"]
                m["total_executed"] = mc["sources_executed"]

    # return total coverage in percent also
    if all_executable > 0 and all_executed > 0:
        total_coverage_percent = round(
            (float(all_executed)/ all_executable) * 100, 1)

    is_web_inject_mode = False
    inject_mode = get_config_value("CURRENT_INJECT_MODE")
    if inject_mode == "web" or inject_mode == "angular":
        is_web_inject_mode = True


    found_active_session=False
    for session in session_ids:
        session_details=get_session(session)
        if bool(int(session_details[1])):
            found_active_session=True
            break

    is_history = found_active_session

    show_templates = False
    if len(template_details) > 0:
        show_templates = True

    # timeline 
    timeline_entries_temp=get_stats_asc_sorted_for_multiple_sessions(session_ids)
    for tm_entry in timeline_entries_temp:
        timeline_entry={}
        timeline_entry["filename"]=tm_entry[0]
        timeline_entry["line"]=tm_entry[1]
        timeline_entry["send_time"]=tm_entry[2]
        timeline_entry["custom_value"]=tm_entry[3]
        timeline_entries.append(timeline_entry)
    if len(timeline_entries)>0:
        has_timeline_entries=True

    # this will be saved to drive as html report
    export_template = render_template("report_printable.html", covered_modules=modules, covered_routes_list=covered_routes, file_details_list=file_details, template_details_list=template_details, total_executable=all_executable, total_executed=all_executed, session_id=session_list_string, total_coverage_value=total_coverage_percent, is_web_inject=is_web_inject_mode, is_history=True, session_name="TOTAL COVERAGE FOR BUILD", session_start="N/A", session_end="N/A", show_templates=show_templates,timeline_entries=timeline_entries,has_timeline_entries=has_timeline_entries,build_number=build_number,session_tag=session_tag)
    # this will be shown to user as view
    output_template = render_template("report.html", covered_modules=modules, covered_routes_list=covered_routes, file_details_list=file_details, template_details_list=template_details, total_executable=all_executable, total_executed=all_executed, session_id=session_list_string, total_coverage_value=total_coverage_percent, is_web_inject=is_web_inject_mode, is_history=True, session_name="TOTAL COVERAGE FOR BUILD", session_start="N/A", session_end="N/A", show_templates=show_templates,timeline_entries=timeline_entries,has_timeline_entries=has_timeline_entries,build_number=build_number,session_tag=session_tag)
    return output_template,export_template

def can_session_be_ended():
    buffer_before_closing_session_seconds=get_config_value("BUFFER_TIME_BEFORE_CLOSING_SESSION_SECONDS")
    if buffer_before_closing_session_seconds==None:
        set_config_value("BUFFER_TIME_BEFORE_CLOSING_SESSION_SECONDS",5) # 5 is default
        buffer_before_closing_session_seconds=5
    else:
        buffer_before_closing_session_seconds=int(buffer_before_closing_session_seconds)
    live_session_id=get_active_test_session()
    if live_session_id is not None:
        live_session_id=live_session_id[0]
        latest_stat=get_latest_stat_for_session(live_session_id)
        if(datetime.datetime.now()-latest_stat).total_seconds()>buffer_before_closing_session_seconds:
            return True 
    return False


def init_db():
    '''
    if db does not exist, create
    :return:
    '''
    if not os.path.exists("instrument.db"):
        connection,cursor=create_database.create_connection()
        create_database.create_db(cursor)
        create_database.close_connection(connection)
    else:
        # db exists
        # check if user's db is in latest schema and if not, update specific tables
        version=get_config_value("VERSION")[0] #version is a string in a form of 1.0.0 or 2.1.0 etc. --get only major number here which is first character in the string
        if  float(version)<3:
            # schema for v3
            try:
                 sql = "SELECT * FROM users"
                 results = execute_select(sql, None, fetchall=True)
            except:
                connection,cursor=create_database.create_connection()
                create_database.create_users_table(cursor)
                create_database.close_connection(connection)

            # check if tags table exists ,if not create 
            try:
                 sql = "SELECT * FROM tags"
                 results = execute_select(sql, None, fetchall=True)
            except:
                connection,cursor=create_database.create_connection()
                create_database.create_tags_table(cursor)
                create_database.close_connection(connection)

            try:
                 sql = "SELECT * FROM sessions_users_tags"
                 results = execute_select(sql, None, fetchall=True)
            except:
                connection,cursor=create_database.create_connection()
                create_database.create_sessions_users_tags_table(cursor)
                create_database.close_connection(connection)

            try:
                 sql = "SELECT * FROM builds"
                 results = execute_select(sql, None, fetchall=True)
            except:
                connection,cursor=create_database.create_connection()
                create_database.create_builds_table(cursor)
                create_database.close_connection(connection)


            try:
                 sql = "SELECT * FROM sessions_builds"
                 results = execute_select(sql, None, fetchall=True)
            except:
                connection,cursor=create_database.create_connection()
                create_database.create_session_build_table(cursor)
                create_database.close_connection(connection)


            try:
                 sql = "SELECT custom_value FROM stats"
                 results = execute_select(sql, None, fetchall=True)
            except:
                connection,cursor=create_database.create_connection()
                create_database.update_stats_table_to_v3(cursor)
                create_database.close_connection(connection)

            # assign default owner, tag and build to existing sessions 
            sessions=execute_select("SELECT * FROM sessions", None, fetchall=True)
            for s in sessions:
                insert_session_build_relation(s[0],0)
                insert_session_user_tag_relation(s[0],0,0)
            # update version in config table
            set_config_value("VERSION","3.0.0")
            # minimum coverage threeshold value
            try:
                get_config_value("DEFAULT_MIN_COVERAGE_PERCENT")[0]
            except:
                set_config_value("DEFAULT_MIN_COVERAGE_PERCENT",60)
            try:
                get_config_value("BUFFER_TIME_BEFORE_CLOSING_SESSION_SECONDS")[0]
            except:
                set_config_value("BUFFER_TIME_BEFORE_CLOSING_SESSION_SECONDS",10)   
            # new columns in sessions table
            connection,cursor=create_database.create_connection()
            create_database.update_sessions_table_to_v3(cursor)
            create_database.close_connection(connection)

            

           

################################################## /UTIL #################


# BOOTS HERE #####################################################33


if __name__ == "__main__":
    init_db()
    get_config()
    #app.run(host=CONFIG["SERVER_HOST"], port=int(CONFIG["PORT"]), threaded=True)
    http_server = WSGIServer((CONFIG["SERVER_HOST"], int(CONFIG["PORT"])), app)
    http_server.serve_forever()
