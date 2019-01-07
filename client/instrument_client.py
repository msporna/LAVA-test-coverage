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

import base64
import fnmatch
import os
import re
import argparse
import json
import shutil

import datetime
import subprocess

import requests
import uuid
from terminaltables import AsciiTable
from colorclass import Color
import pickle
import sys
import logging


class Instrumenter:

    def __init__(self):
        self.CONFIG_PATH = ''

        self.SERVER_URL = ''  # obtain from config.json
        self.START_TEST_SESSION_METHOD = 'set_test_session_start'
        self.END_TEST_SESSION_METHOD = 'set_test_session_end'
        self.SET_DETECTED_FILES_API_METHOD = 'set_detected_files'
        self.GET_INSTRUMENT_FUNCTION_NAME_METHOD = 'get_instrument_function_name'
        self.GET_TEST_SESSION_STATUS_METHOD = 'get_test_session_status'
        self.SET_CONFIG_VALUES_METHOD = 'set_config_values'
        self.SET_FILE_CONTENT_METHOD = 'set_file_content'
        self.SET_EXECUTABLE_LINES_COUNT_METHOD = 'set_executable_lines_count_for_file'
        self.SEND_INSTRUMENTATION_STATS_METHOD = 'send_instrumentation_stats'
        # self.SET_MODULES_API_METHOD = 'set_modules' #obsolete
        self.SET_ROUTES_API_METHOD = 'set_routes'
        self.JS_TO_INSTRUMENT_LIST_METHOD = 'get_js_to_instrument'
        self.GET_INSTRUMENTATION_TOKEN = 'get_instrument_token'
        self.SOURCE_FILES_TO_INSTRUMENT = []  # javascript,typescript,c#
        self.SOURCE_TO_EXCLUDE = []  # supports files and directories like /
        # required javascript libraries that will be injected into the webapp
        self.JS_TO_INJECT = ''  # by default: instrument.js
        # list of html files in the project (paths), that should be
        # instrumented; skip index.html
        self.TEMPLATES_TO_INSTRUMENT = []
        self.ROUTES_TO_INSTRUMENT = []
        self.INJECT_MODE = ''  # unity,web,android
        self.SOURCE_ABSOLUTE_PATH = ''
        self.FILES_LINE_COUNT = []
        self.EXTENSION = ''
        self.ANGULAR_MAIN_FILENAME = '' # obsolete 
        self.TS_MODULE_PATH = ''
        self.ANDROID_LAVA_HELPER_IMPORT_NAMESPACE='' # lava helper must be imported to each instrumented file; location is unknown. This specifies location of lava helper class within android project.
        self.INDEX_FILE_PATH = ''  # expecting index.html in source root
        # expecting [self.angular_main_filename] in source root
        self.ANGULAR_INSTANTIATE_FILE = ''
        # SPECIFY REGEX FOR CODE YOU WANT TO APPEND WITH INSTRUMENTATION FUNCTION
        # BY DEFAULT IT FINDS JAVASCRIPT FUNCTION DECLARATIONS:
        self.WEB_REGEX_LIST = [
            (
                r'\b(function)\b[\s\w]+\([\w,\s]*\)[\s]*(?![\s]*;)[\{]*',
                "function"),
            (r'\b(if)\b[\s]*\([\w\s\"=="\&&\||]+\)[\s]*\{', "branch_if"),
            (r'\b(else)\b[\s]*\{', "branch_else"),
            (r'(;)', "statement"),
            (r'\b(case)\b[\s]*[\w\d]+\:', "branch_switch"),
            (r'\b(default)\b\:', "branch_switch")

        ]
        # angular is obsolete 
        self.ANGULAR_REGEX_LIST = [
            (
                r'[\w]+\([\w\s\:\,\=\{\}\[\]]*\)[\:\<\>]*[\s]*[\w\[\]\d\(\)\<\>]*[\s]*[\{]',
                'function')
        ]

        self.UNITY_REGEX_LIST = [
            (r'[\w]+[\s]+[\w]+[\s]*\(.*\)[\s]*\{', "function"),
            (r'(else)?[\s]*(if)[\s]*\(.*\)[\s]*\{', 'branch_if'),
            (r'\b(else)\b[\s]*\{', 'branch_else'),
            (r'(default|case).*\:.*', 'branch_switch'),
            (r'.*(while|for|foreach)[\s]*\(.*\)[\s]*\{', 'loop')
        ]

        self.JAVA_REGEX_LIST = [
            (r'[\w]+[\s]+[\w]+[\s]*\(.*\)[\s]*\{', "function"),
            (r'(else)?[\s]*(if)[\s]*\(.*\)[\s]*\{', 'branch_if'),
            (r'\b(else)\b[\s]*\{', 'branch_else'),
            (r'(default|case).*\:.*', 'branch_switch'),
            (r'.*(while|for|foreach)[\s]*\(.*\)[\s]*\{', 'loop')
        ]

        self.UPLOAD_ENTRIES = []
        # source file name, b64 original content (before injecting probes)
        self.SOURCE_ORIGINAL_CONTENT = {}

        # init logging 
        logging.basicConfig(filename='lava.log',level=logging.INFO)

    def prepare(self, config_path):
        self.CONFIG_PATH = config_path
        # open config and parse
        with open(self.CONFIG_PATH) as config_file:
            config = json.load(config_file)
            self.INJECT_MODE = config["INJECT_MODE"]
            self.SERVER_URL = config["instrument_server_url"]
            # self.ANGULAR_MAIN_FILENAME = config["angular_main_file_name"]
            self.SOURCE_ABSOLUTE_PATH = self.convert_path_to_unix(
                os.path.abspath(config["source_root_absolute_path"]))
            # self.ANGULAR_INSTANTIATE_FILE = self.convert_path_to_unix(os.path.join(self.SOURCE_ABSOLUTE_PATH, self.ANGULAR_MAIN_FILENAME))
            self.INDEX_FILE_PATH = self.convert_path_to_unix(
                os.path.join(self.SOURCE_ABSOLUTE_PATH, 'index.html'))
            self.ANDROID_LAVA_HELPER_IMPORT_NAMESPACE=config["android_lava_helper_namespace"]

            # send source absolute path
            url = self.SERVER_URL + "/" + self.SET_CONFIG_VALUES_METHOD
            headers = {'content-type': 'application/x-www-form-urlencoded'}
            config_entry = {}
            config_entry["SOURCE_ABSOLUTE_PATH"] = self.SOURCE_ABSOLUTE_PATH
            config_entry["CURRENT_INJECT_MODE"] = self.INJECT_MODE
            requests.post(url, data=config_entry, headers=headers)

            for rti in config["web_routes_available"]:
                self.ROUTES_TO_INSTRUMENT.append(rti)
            for ste in config["source_to_exclude"]:
                self.SOURCE_TO_EXCLUDE.append(ste)
            self.JS_TO_INJECT = self.get_path_to_instrumenter_js()
            self.TS_MODULE_PATH = self.get_path_to_ts_module()

        logging.info('Config loaded.')

        if self.INJECT_MODE == "unity":
            self.EXTENSION = "*.cs"
        elif self.INJECT_MODE == "web":
            self.EXTENSION = "*.js"
        elif self.INJECT_MODE == "angular":
            self.print_info(Color('{autored}[ERROR] {/autored}') +
                  'As of version 3 of LAVA code coverage Angular is not supported anymore. For Angular support, use version 2 of LAVA.')
            return  # if source of user's app was already instrumented, cancel.
            self.EXTENSION = "*.ts"
        elif self.INJECT_MODE=="android":
            self.EXTENSION="*.java"

        # actions:
        if self.check_if_instrument_exists():
            self.print_info(Color('{autored}[ERROR] {/autored}') +
                  'It seems like the specified source was previously instrumented. Please use this tool only on fresh source (remove and clone again?).')
            return  # if source of user's app was already instrumented, cancel.

     
        self.detect_sources()
        self.detect_templates()

        
        self.print_info("injecting...")
        self.inject_required_scripts()  # to index.html (applies to web & angular only)
        self.insert_instrument_function_into_templates()  # to other templates if exist
        self.insert_instrument_function()  # to source files
        self.print_info("injecting done...")
        self.print_info("uploading...")
        # update list of the files in the project root [js and html only]
        # send current file list that is under instrumentation to backend
        self.update_file_list()
        self.set_routes()  # end routes that are being instrumented
        # self.set_modules()  # modules that app has and can be visited ; OBSOLETE since version 2
        # upload executable lines
        self.upload_executable_lines_count()

        
        self.print_info("uploading done.")

        # show summary of upload
        self.print_table(["Upload to lava DB", "Status"], self.UPLOAD_ENTRIES)

        self.save_instrument_token()

    def check_if_instrument_exists(self):
        '''
        if user instrumented some source and it has instrument
        :return:
        '''
        # look for lava file in source root and deserialize
        lava_file = os.path.join(self.SOURCE_ABSOLUTE_PATH, "lava.pkl")
        if os.path.isfile(lava_file):
            return True  # source was instrumented previously
        else:
            # source was probably not instrumented before (or someone removed
            # lava.pkl file)
            return False

    def show_progress(self, count, total, status=''):
        '''
        shows progress bar
        source: https://gist.github.com/vladignatyev/06860ec2040cb497f0f3
        :param total:
        :param status:
        :return:
        '''
        bar_len = 60
        filled_len = int(round(bar_len * count / float(total)))

        percents = round(100.0 * count / float(total), 1)
        bar = '=' * filled_len + '-' * (bar_len - filled_len)

        sys.stdout.write('[%s] %s%s ...%s\n' % (bar, percents, '%', status))
        sys.stdout.flush()  # As suggested by Rom Ruben (see: http://stackoverflow.com/questions/3173320/text-progress-bar-in-the-console/27871113#comment50529068_27871113)

    def save_instrument_token(self):
        '''
        after instrumenting is done, generate token and store it in file in soruce root
        '''
        token = base64.b64encode(str(datetime.datetime.now()))

        config_entry = {}
        config_entry["name"] = "INSTRUMENT_TOKEN"
        config_entry["value"] = token

        # serialize to file and store in source root
        with open(os.path.join(self.SOURCE_ABSOLUTE_PATH, "lava.pkl"), "wb") as file:
            pickle.dump(config_entry, file)

        self.print_info(Color(
            '\n {autogreen}Successfully stored instrumentation token: ' + token + '{/autogreen}'))

    def get_path_to_instrumenter_js(self):
        instrument_js_filename = "instrument.js"
        path = '../helpers/javascript/' + instrument_js_filename
        return path

    def get_path_to_ts_module(self):
        module_filename = "instrumenter.ts"
        path = '../helpers/typescript_module/lava_test_coverage/' + module_filename
        return path

    def convert_path_to_unix(self, path):
        if '\\' in path:
            return path.replace('\\', '/')
        else:
            return path

    def detect_sources(self):
        '''
        find source files in the app's root
        and add to SOURCE_FILES_TO_INSTRUMENT
        -skip files from exclude list
        :return:
        '''
        logging.info('Detecting sources started')
        self.print_info(Color('{autoblue}Detecting Sources...{/autoblue}'))
        detected_files_list = []

        for root, dirnames, filenames in os.walk(self.SOURCE_ABSOLUTE_PATH):
            for file in fnmatch.filter(filenames, self.EXTENSION):
                source_path = os.path.join(root, file)

                if file not in self.SOURCE_TO_EXCLUDE:

                    # secondary check-source_to_exclude is set by user in config
                    # but there might be other reasons some source file should not go into source_files_to_instrument list
                    # so do another check here:
                    # example: angular main.ts file should not go into that
                    # list now, because it is handled differently as it's a
                    # special file (instantiating is done in there)
                    if not self.check_if_file_should_be_skipped(source_path):
                        self.SOURCE_FILES_TO_INSTRUMENT.append(source_path)
                        detected_files_list.append(
                            [Color('{autogreen}' + source_path + '{/autogreen}')])
                    else:
                        detected_files_list.append([Color(
                            '{autoblue}' + source_path + '{/autoblue}')])  # to let user know that we know it's a different kind of file

                else:
                    detected_files_list.append(
                        [Color('{autored}' + source_path + ' (excluded){/autored}')])

        if len(detected_files_list):
            self.print_table(["Detected Sources"], detected_files_list)

    def check_if_file_should_be_skipped(self, file_path):
        '''
        skip lava related files, that could be found in the sources
        '''
        if self.INJECT_MODE == "angular":
            if os.path.basename(file_path) == os.path.basename(
                    self.ANGULAR_INSTANTIATE_FILE):
                return True
            elif os.path.basename(file_path) == os.path.basename(self.TS_MODULE_PATH):
                return True
        elif self.INJECT_MODE=="android":
            if os.path.basename(file_path)=="LavaCoverageHelper.java":
                return True
        elif self.INJECT_MODE=="unity":
            if os.path.basename(file_path)=="LavaHelper.cs":
                return True

        return False

    def detect_templates(self):

        if self.INJECT_MODE == "unity":
            return

        self.print_info(Color('{autoblue}Detecting Templates...{/autoblue}'))
        detected_files_list = []

        for root, dirnames, filenames in os.walk(self.SOURCE_ABSOLUTE_PATH):
            for file in fnmatch.filter(filenames, "*.html"):
                template_path = os.path.join(root, file)
                self.TEMPLATES_TO_INSTRUMENT.append(template_path)
                detected_files_list.append(
                    [Color('{autocyan}' + template_path + '{/autocyan}')])

        if len(detected_files_list) > 0:
            self.print_table(["Detected Templates"], detected_files_list)

    def inject_required_scripts(self):
        if self.INJECT_MODE == "web":
            self.inject_scripts_mode_0()
        elif self.INJECT_MODE == "angular":
            self.inject_scripts_mode_1()

    def inject_scripts_mode_0(self):
        """
        web
        create <script></script> in the app's index.html
        and initilize INSTRUMENTER and call InitInstrument() method that is in instrument.js to pass
        some variables from config ,like server url, method names etc.
        :return:
        """
        # prepare script snippet
        script_html = "\n"
        # required scripts
        filename = os.path.basename(self.JS_TO_INJECT)
        shutil.copyfile(self.JS_TO_INJECT, os.path.join(
            self.SOURCE_ABSOLUTE_PATH, filename))
        script_html += '<script type="text/javascript" src="' + self.find_relative_path_to_file(self.INDEX_FILE_PATH,
                                                                                                "instrument.js") + '"></script>\n'
        # now init script into index.html body
        script_html += '<script>'
        script_html += 'var INSTRUMENTER=new jsInstrument("' + \
            self.SERVER_URL + '");\n'
        script_html += 'INSTRUMENTER.InstrumentCode("' + str(
            uuid.uuid4()) + '","index.html","-1","statement","");</script>\n'
        lines = []  # init

        self.store_original_content(self.INDEX_FILE_PATH)
        # get content of index html
        with open(self.INDEX_FILE_PATH, 'r+') as index:
            lines = index.readlines()
            # put init code at the end of the file
            lines.insert(0, script_html)
            # clean file before injecting
            index.seek(0)
            index.truncate()
            index.writelines(lines)

        record = {}
        record["file"] = os.path.basename(self.INDEX_FILE_PATH)
        record["count"] = 1
        record["executable"] = -1
        self.FILES_LINE_COUNT.append(record)

    def inject_scripts_mode_1(self):
        """
        angular
        :return:
        """
        # prepare script snippet
        script_html = "\n"
        # copy required scripts to DIST path, not source path
        filename = os.path.basename(self.JS_TO_INJECT)
        js_destination_path = os.path.join(self.SOURCE_ABSOLUTE_PATH, filename)
        shutil.copyfile(self.JS_TO_INJECT, js_destination_path)
        script_html += '<script type="text/javascript" src="' + self.find_relative_path_to_file(self.INDEX_FILE_PATH,
                                                                                                "instrument.js") + '"></script>\n'

        # now init script into index.html body
        script_html += '<script>'
        script_html += 'var INSTRUMENTER=new jsInstrument("' + \
            self.SERVER_URL + '");\n'
        script_html += 'INSTRUMENTER.InstrumentCode("' + str(
            uuid.uuid4()) + '","index.html","-1","statement","");</script>\n'

        lines = []  # init
        # get content of index html which is in source and will be compiled
        # with injected required js
        self.store_original_content(self.INDEX_FILE_PATH)
        with open(self.INDEX_FILE_PATH, 'r+') as index:
            lines = index.readlines()
            # put init code at the end of the file
            lines.insert(0, script_html)
            # clean file before injecting
            index.seek(0)
            index.truncate()
            index.writelines(lines)

        record = {}
        record["file"] = os.path.basename(self.INDEX_FILE_PATH)
        record["count"] = 1
        record["executable"] = -1
        self.FILES_LINE_COUNT.append(record)

    def find_relative_path_to_file(self, file_path, desired_destination_file):
        '''
        relative path from file_path
        to desired_destination_file: instrument.js,instrument.ts,other...?
        1. find relative path from file path to source root
        2. join relative source path with destination file relative path
        :return:
        '''
        found = False

        # generate destination file relative path
        destination_file_path = ''
        if desired_destination_file == "instrument.js":
            # expect it in source root
            destination_file_path = 'instrument.js'
        elif desired_destination_file == "instrumenter.ts":
            # expect it in source/lava_test_coverage
            destination_file_path = 'lava_test_coverage/instrumenter'  # without .ts on purpose
        elif desired_destination_file == "angular_main":
            # it's a main.ts file, should be in root of angular app, it's a
            # bootstrap file.
            destination_file_path = self.ANGULAR_MAIN_FILENAME.replace(
                ".ts", "").strip()
            # without extension, because imports in angular/ts are without .ts
            # extension

        current_dir = os.path.dirname(file_path)
        relative_path = ''

        while not found:

            if self.convert_path_to_unix(os.path.abspath(
                    current_dir)) == self.SOURCE_ABSOLUTE_PATH:
                return self.convert_path_to_unix(
                    os.path.join(relative_path, destination_file_path))

            else:
                other_dir = os.path.join(current_dir, "..")
                relative_path += "../"
                current_dir = other_dir

    def insert_instrument_function_into_templates(self):
        """
        insert instrument script into each html template
        :return:
        """
        script_html = ""
        for t in self.TEMPLATES_TO_INSTRUMENT:
            if self.convert_path_to_unix(t) == self.INDEX_FILE_PATH:
                continue  # skip index file, it had been already injected

            script_html = "\n"
            # prepare script snippet
            script_html += '<script type="text/javascript" src="' + self.find_relative_path_to_file(
                t, os.path.basename(self.JS_TO_INJECT)) + '"></script>\n'
            script_html += '<script>'
            script_html += 'var INSTRUMENTER=new jsInstrument("' + \
                self.SERVER_URL + '");\n'
            script_html += 'INSTRUMENTER.InstrumentCode("' + str(
                uuid.uuid4()) + '","' + os.path.basename(t) + '","-1","statement","");</script>\n'
            lines = []  # init
            # get content of template html
            self.store_original_content(t)
            with open(t, 'r+') as templ:
                lines = templ.readlines()
                # put instrumentation function at the end of file
                lines.insert(0, script_html)
                # save file - if everythig is ok
                # clean file before injecting
                templ.seek(0)
                templ.truncate()
                templ.writelines(lines)

            record = {}
            record["file"] = os.path.basename(t)
            record["count"] = 1
            record["executable"] = -1

            self.FILES_LINE_COUNT.append(record)

    def update_file_list(self):
        """
        insert all files that we want to instrument to files table in the backend
        :return:
        """
        # set files
        ct = 0
        files = {}
        skipped_files = []

        for source_file in self.SOURCE_FILES_TO_INSTRUMENT:
            source_file = self.convert_path_to_unix(source_file)
            # skip uploading files with line count=0
            if self.check_if_line_count_above_0(os.path.basename(source_file)):
                files.update({ct: source_file})
                ct += 1
            else:
                skipped_files.append([source_file])

        for templ in self.TEMPLATES_TO_INSTRUMENT:
            templ = self.convert_path_to_unix(templ)
            if self.check_if_line_count_above_0(os.path.basename(templ)):
                files.update({ct: templ})
                ct += 1
            else:
                skipped_files.append([templ])

        # send to backend
        url = self.SERVER_URL + "/" + self.SET_DETECTED_FILES_API_METHOD
        requests.post(url, data=json.dumps(files), headers={'content-type': 'application/json'})

        self.send_file_contents(files)

        if len(skipped_files) > 0:
            self.print_info(
                '\n -- some files were skipped because of executable line count equal to 0:')
            self.print_table(["Skipped"], skipped_files)

    def check_if_line_count_above_0(self, filename):
        for record in self.FILES_LINE_COUNT:
            if record["file"] == filename:
                if record["count"] > 0:
                    return True
        return False

    def send_file_contents(self, files):
        """
        encodes each files' content with base 64 and sends to backend
        """
        # now, read content of each of those files and send to backend as base
        # 64
        file_contents = {}
        ct = 0
        for k, v in files.iteritems():
            # k: index number
            # v: file path
            with open(v) as f:
                filename = os.path.basename(v)
                file_contents[
                    "file_content"] = self.SOURCE_ORIGINAL_CONTENT[filename]
                file_contents["filename"] = filename
                # send to backend
                headers = {'content-type': 'application/x-www-form-urlencoded'}
                url = self.SERVER_URL + "/" + self.SET_FILE_CONTENT_METHOD
                r = requests.post(url, data=file_contents, headers=headers)
                # print "file content upload status: " + r.text

                # color status in green,if not 200 then red to indicate problem
                output_color = "autogreen"
                if r.text != "200":
                    output_color = 'autored'

                self.UPLOAD_ENTRIES.append([Color('{autocyan}[' + filename + ']{/autocyan}'), Color(
                    '{' + output_color + '}' + r.text + '{/' + output_color + '}')])

                # report progress
                ct += 1
                self.show_progress(ct, len(files), filename)

    def set_routes(self):
        ct = 0
        routes = {}
        for r in self.ROUTES_TO_INSTRUMENT:
            routes.update({ct: r})
            ct += 1

        # send to backend
        url = self.SERVER_URL + "/" + self.SET_ROUTES_API_METHOD
        r = requests.get(url, params=routes)
        #print("setting routes result: "+r.text)
        self.UPLOAD_ENTRIES.append(
            [Color('{autoblue}Routes upload (' + str(len(routes)) + '){/autoblue}'), Color('{autogreen}' + r.text + '{/autogreen}')])

    def upload_executable_lines_count(self):
        self.print_info("\n ---uploading executable line count for all files....------")
        ct = 0
        for record in self.FILES_LINE_COUNT:
            ct += 1
            if record["count"] > 0:
                # send request
                url = self.SERVER_URL + "/" + self.SET_EXECUTABLE_LINES_COUNT_METHOD
                r = requests.get(url, params=record)

                self.show_progress(ct, len(self.FILES_LINE_COUNT), Color(
                    '{autogreen}' + record["file"] + ":" + str(record["count"]) + '{/autogreen}'))
            else:
                self.show_progress(ct, len(
                    self.FILES_LINE_COUNT), record["file"] + ":" + str(record["count"]) + ' [skipped]')

    def insert_instrument_function(self):
        if self.INJECT_MODE == "web":
            self.insert_instrument_function_into_js_0()
        elif self.INJECT_MODE == "angular":
            self.copy_ts_module_to_source_folder()
            self.insert_instrument_function_into_js_1()
        elif self.INJECT_MODE == "unity":
            self.insert_instrument_function_into_csharp()
        elif self.INJECT_MODE=="android":
            self.insert_instrument_function_into_java()

    def copy_ts_module_to_source_folder(self):
        if os.path.exists(os.path.join(
                self.SOURCE_ABSOLUTE_PATH, "lava_test_coverage")):
            shutil.rmtree(os.path.join(
                self.SOURCE_ABSOLUTE_PATH, "lava_test_coverage"))
        shutil.copytree(os.path.dirname(self.TS_MODULE_PATH),
                        os.path.join(self.SOURCE_ABSOLUTE_PATH, "lava_test_coverage"))
        # update module path
        ts_module_filename = os.path.basename(self.TS_MODULE_PATH)
        # new ts module location,inside source folder
        self.TS_MODULE_PATH = os.path.join(
            self.SOURCE_ABSOLUTE_PATH, "lava_test_coverage")
        self.TS_MODULE_PATH = os.path.join(
            self.TS_MODULE_PATH, ts_module_filename)

    def insert_instrument_function_into_csharp(self):
        for source_file in self.SOURCE_FILES_TO_INSTRUMENT:
            subprocess.call('AStyle --style=java --break-one-line-headers --add-braces --delete-empty-lines --mode=cs "' +
                            self.convert_path_to_unix(source_file) + '"', shell=True)
            self.store_original_content(source_file)

            line_count = 0
            executable_lines = ''

            file_content = []
            filename = os.path.basename(source_file)
            with open(source_file, 'r+') as f:
                file_content = f.readlines()
                class_name = ''

                for l in range(0, len(file_content)):

                    if "using " in file_content[l]:
                        continue

                    p = re.compile(r'[\s]+(class)[\s]+[\w]+')
                    var = p.search(file_content[l])
                    if var is not None:
                        # regex result gives me 'public class SomeName : MonoBehaviour {' and I'm
                        # taking only 'class SomeName', split by space and then take 'SomeName' and
                        # assign to class_name
                        class_name = var.string[var.regs[0][0]:var.regs[0][1]].split(' ')[
                            1]

                    for reg in self.UNITY_REGEX_LIST:
                        p = re.compile(reg[0])
                        var = p.search(file_content[l])
                        if var is not None:
                            if reg[1] == "function" and class_name in var.string:
                                continue  # skip injecting if this line is a constructor, we don't want to probe constructors for some unity serialization related reasons

                            # need to inject right after expression found and
                            # make sure that original string is intact to avoid
                            # breaking the file
                            injected_string = var.string[0:var.regs[0][0]] + var.string[var.regs[0][0]:var.regs[0][
                                1]] + ' LavaHelper.SendStats("' + filename + '","' + str(
                                uuid.uuid4()) + '","' + str(l + 1) + '","' + \
                                reg[1] + '","");' + var.string[
                                var.regs[
                                    0][1]:]
                            file_content[l] = injected_string

                            if len(executable_lines) > 0:
                                executable_lines += "," + str(l + 1)
                            else:
                                executable_lines = str(l + 1)

                            line_count += 1
                            break

                f.seek(0)
                f.truncate()
                # inject
                f.writelines(file_content)

            # format after inejction
            output = subprocess.call('AStyle --style=java --break-one-line-headers --add-braces --delete-empty-lines --mode=cs "' +
                                     self.convert_path_to_unix(source_file) + '"', shell=True)

            if output == 2 or output == 1:
                self.print_info(Color(
                    '\n {autored}[INJECTION ERROR]{/autored}') + ": FILE STRUCTURE BROKEN AFTER INJECTION: " + filename + ". THIS FILE WILL NOT BE COVERED. IF SITUATION PERSISTS ADD THIS FILE TO EXCLUDED LIST. \n")
                continue

            # save line count

            record = {}
            record["file"] = filename
            record["count"] = line_count
            record["executable"] = executable_lines

            self.FILES_LINE_COUNT.append(record)

    def insert_instrument_function_into_java(self):
        for source_file in self.SOURCE_FILES_TO_INSTRUMENT:
            subprocess.call('AStyle --style=java --break-one-line-headers --add-braces --delete-empty-lines --mode=java "' +
                            self.convert_path_to_unix(source_file) + '"', shell=True)
            self.store_original_content(source_file)

            line_count = 0
            executable_lines = ''
            import_statement_injected=False

            file_content = []
            filename = os.path.basename(source_file)
            with open(source_file, 'r+') as f:
                file_content = f.readlines()
                class_name = ''
                package_statement_line=-1

                for l in range(0, len(file_content)):

                    if "package " in file_content[l]:
                        if package_statement_line==-1:
                            package_statement_line=l
                            #otherwise it was already set...
                    else:
                        p = re.compile(r'(class)[\s]+[\w]+')
                        var = p.search(file_content[l])
                        if var is not None:
                            # regex result gives me 'public class SomeName' and I'm
                            # taking only 'class SomeName', split by space and then take 'SomeName' and
                            # assign to class_name
                            class_name = var.string[var.regs[0][0]:var.regs[0][1]].split(' ')[
                                1]

                  
                        for reg in self.JAVA_REGEX_LIST:
                            p = re.compile(reg[0])
                            var = p.search(file_content[l])
                            if var is not None:
                                if reg[1] == "function" and class_name in var.string:
                                    continue  # skip injecting if this line is a constructor
                                
                                if reg[1]=="function":
                                    # skip all creations looking like: new Thread(new Runnable() {
                                    # that might appear like functions but we don't want this 
                                    function_return_type = var.string[var.regs[0][0]:var.regs[0][1]].split(' ')[
                                0]
                                    if function_return_type=="new":
                                        continue 

                                # need to inject right after expression found and
                                # make sure that original string is intact to avoid
                                # breaking the file
                                injected_string = var.string[0:var.regs[0][0]] + var.string[var.regs[0][0]:var.regs[0][
                                    1]] + ' LavaCoverageHelper.SendStats("' + filename + '","' + str(
                                    uuid.uuid4()) + '","' + str(l + 1) + '","' + \
                                    reg[1] + '","");' + var.string[
                                    var.regs[
                                        0][1]:]
                                file_content[l] = injected_string

                                if len(executable_lines) > 0:
                                    executable_lines += "," + str(l + 1)
                                else:
                                    executable_lines = str(l + 1)

                                line_count += 1

                                # if import statement not inserted by now (it means that the file has no imports at all)
                                # insert import for lava helper right under package statement
                                if not import_statement_injected:
                                    file_content[package_statement_line]+=" import "+self.ANDROID_LAVA_HELPER_IMPORT_NAMESPACE+";" # this will be formatted 
                                    import_statement_injected=True 
                                break

                f.seek(0)
                f.truncate()
                # inject
                f.writelines(file_content)

            # format after inejction
            output = subprocess.call('AStyle --style=java --break-one-line-headers --add-braces --delete-empty-lines --mode=java "' +
                                     self.convert_path_to_unix(source_file) + '"', shell=True)

            if output == 2 or output == 1:
                self.print_info(Color(
                    '\n {autored}[INJECTION ERROR]{/autored}') + ": FILE STRUCTURE BROKEN AFTER INJECTION: " + filename + ". THIS FILE WILL NOT BE COVERED. IF SITUATION PERSISTS ADD THIS FILE TO EXCLUDED LIST. \n")
                continue

            # save line count

            record = {}
            record["file"] = filename
            record["count"] = line_count
            record["executable"] = executable_lines

            self.FILES_LINE_COUNT.append(record)

    def insert_instrument_function_into_js_0(self):
        '''
        MODE 0
        INJECT PROBLES INTO UN_MINIFIED JS FILEs
        :return:
        '''
        for js_file in self.SOURCE_FILES_TO_INSTRUMENT:
            line_count = 0  # executable line count
            executable_lines = ''

            # format file to be sure regex expressions work as expected
            subprocess.call('prettier --write "' +
                            self.convert_path_to_unix(js_file) + '"', shell=True)

            file_content = []
            filename = os.path.basename(js_file)

            self.store_original_content(js_file)

            with open(js_file, 'r+') as f:

                file_content = f.readlines()
                for l in range(0, len(file_content)):
                    for reg in self.WEB_REGEX_LIST:
                        p = re.compile(reg[0])
                        var = p.search(file_content[l])
                        if var is not None:
                            if reg[1] == "statement":

                                file_content[l] = 'INSTRUMENTER.InstrumentCode("' + str(
                                    uuid.uuid4()) + '","' + filename + '","' + str(l + 1) + '","' + reg[
                                    1] + '","");' + var.string

                            else:
                                #file_content[l]=var.string+' INSTRUMENTER.InstrumentCode("' + str(uuid.uuid4()) + '","' + filename + '","' + str(l+1) + '","' + reg[1] + '");'
                                injected_string = var.string[0:var.regs[0][0]] + var.string[var.regs[0][0]:var.regs[0][
                                    1]] + ' INSTRUMENTER.InstrumentCode("' + str(
                                    uuid.uuid4()) + '","' + filename + '","' + str(l + 1) + '","' + reg[
                                    1] + '","");' + var.string[var.regs[0][1]:]
                                file_content[l] = injected_string

                            if len(executable_lines) > 0:
                                executable_lines += "," + str(l + 1)
                            else:
                                executable_lines = str(l + 1)
                            line_count += 1
                            break

                # clean file before injecting
                f.seek(0)
                f.truncate()
                f.writelines(file_content)

            # format once again, after injections
            output = subprocess.call(
                'prettier --write "' + self.convert_path_to_unix(js_file) + '"', shell=True)

            if output == 2:
                self.print_info(Color('\n {autored}[INJECTION ERROR]{/autored}') + ": FILE STRUCTURE BROKEN AFTER INJECTION: " +
                      filename + ". THIS FILE WILL NOT BE COVERED. IF SITUATION PERSISTS ADD THIS FILE TO EXCLUDED LIST. \n")
                continue

            # save probes count
            record = {}
            record["file"] = filename
            record["count"] = line_count
            record["executable"] = executable_lines

            self.FILES_LINE_COUNT.append(record)

    def insert_instrument_function_into_js_1(self):
        '''
        MODE 1
        INJECT INTO TS SOURCE FILES
        :return:
        '''

        # first, inject .ts code that instantiates global instance of
        # instrumenter object

        self.ANGULAR_INSTANTIATE_FILE = self.convert_path_to_unix(
            self.ANGULAR_INSTANTIATE_FILE)

        # format before injection
        subprocess.call('prettier --write "' +
                        self.ANGULAR_INSTANTIATE_FILE + '"', shell=True)

        self.store_original_content(self.ANGULAR_INSTANTIATE_FILE)

        with open(self.ANGULAR_INSTANTIATE_FILE, 'r+') as f0:
            content = f0.readlines()
            # print(os.path.splitext(self.TS_MODULE_PATH)[0])
            content.insert(0, 'import {Instrumenter} from "./' + self.find_relative_path_to_file(
                self.ANGULAR_INSTANTIATE_FILE, os.path.basename(self.TS_MODULE_PATH)) + '" //lava \n')
            # now at the bottom
            content.append(
                'export const INSTRUMENTER=new Instrumenter("' + self.SERVER_URL + '"); \n')
            # clean file before injecting
            f0.seek(0)
            f0.truncate()
            # inject instantiate code
            f0.writelines(content)

        # format after injection
        subprocess.call('prettier --write "' +
                        self.ANGULAR_INSTANTIATE_FILE + '"', shell=True)

        # then inject instrument function
        for source_file in self.SOURCE_FILES_TO_INSTRUMENT:
            subprocess.call(
                'prettier --write "' + self.convert_path_to_unix(source_file) + '"', shell=True)
            self.store_original_content(source_file)

            executable_lines = ''
            line_count = 0

            file_content = []
            filename = os.path.basename(source_file)
            with open(source_file, 'r+') as f:
                file_content = f.readlines()

                if '@ngmodule' in ''.join(file_content).lower():
                    continue  # skip modules

                relative_path_to_main = self.find_relative_path_to_file(
                    source_file, "angular_main")
                if "../" not in relative_path_to_main:
                    relative_path_to_main = "./" + relative_path_to_main

                file_content.insert(
                    0, 'import {INSTRUMENTER} from "' + relative_path_to_main + '" //lava \n')

                for l in range(0, len(file_content)):
                    for reg in self.ANGULAR_REGEX_LIST:
                        p = re.compile(reg[0])
                        var = p.search(file_content[l])
                        if var is not None:
                            # need to inject right after expression found and
                            # make sure that original string is intact to avoid
                            # breaking the file
                            injected_string = var.string[0:var.regs[0][0]] + var.string[var.regs[0][0]:var.regs[0][1]] + ' INSTRUMENTER.InstrumentCode("' + str(
                                uuid.uuid4()) + '","' + filename + '","' + str(l + 1) + '","' + reg[1] + '","");' + var.string[var.regs[0][1]:]
                            file_content[l] = injected_string

                            if len(executable_lines) > 0:
                                executable_lines += "," + str(l + 1)
                            else:
                                executable_lines = str(l + 1)
                            line_count += 1
                            break

                # clean file before injecting
                f.seek(0)
                f.truncate()
                # inject
                f.writelines(file_content)

            # format after inejction
            output = subprocess.call(
                'prettier --write "' + self.convert_path_to_unix(source_file) + '"', shell=True)

            if output == 2:
                self.print_info(Color('\n {autored}[INJECTION ERROR]{/autored}') + ": FILE STRUCTURE BROKEN AFTER INJECTION: " +
                      filename + ". THIS FILE WILL NOT BE COVERED. IF SITUATION PERSISTS ADD THIS FILE TO EXCLUDED LIST. \n")
                continue

            # save line count
            record = {}
            record["file"] = filename
            record["count"] = line_count
            record["executable"] = executable_lines

            self.FILES_LINE_COUNT.append(record)

        # now, after instantiate file was instrumented,add it to
        # source_files_to_instrument to make sure it will be sent
        self.SOURCE_FILES_TO_INSTRUMENT.append(self.ANGULAR_INSTANTIATE_FILE)

    def store_original_content(self, file):
        file = self.convert_path_to_unix(file)
        with open(file, 'r+') as f:
            self.SOURCE_ORIGINAL_CONTENT[os.path.basename(
                file)] = base64.b64encode(f.read())

    def remove_line_breaks(self, file):
        file_content = []
        with open(file, 'r+') as f:
            file_content = f.readlines()
            for l in range(0, len(file_content)):
                file_content[l] = file_content[l].rstrip('\n')
            f.seek(0)
            f.truncate()
            f.writelines(file_content)

    def print_table(self, columns, rows):
        '''
        print output to terminal in a form of table
        uses 'terminaltables' module
        :param columns:
        :param rows:
        :return:
        '''
        data = []
        data.append(columns)
        for r in rows:
            data.append(r)
        output = AsciiTable(data)
        logging.info(str(output.table))
        print output.table

    def print_info(self,msg):
        print(msg)
        logging.info(msg)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description='Get test coverage for specified app.')
    parser.add_argument('configPath', metavar='J', type=str, nargs='+',
                        help='Absolute path to config json file. Required')
    args = parser.parse_args()

    # store config json path
    json_path = args.configPath[0]
    print(Color('{autoblue}[Parsing JSON Config]{/autoblue}') + ":" + json_path)

    instrumenter = Instrumenter()
    instrumenter.prepare(json_path)
