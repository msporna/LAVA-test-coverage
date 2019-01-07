import json
from robot.libraries.BuiltIn import BuiltIn
import requests




class CustomKeywords:
    '''
    custom keywords that extend default robot framework and selenium2library's keywords
    '''
    __version__ = '0.1'


    def _make_api_request(self,url,params_list,method):
        """
        make call to given url and return response
        which is expected to be json
        :param url:
        :return:
        """
        print "Sending request to: "+url
        headers = {'Accept' : 'application/json', 'Content-Type' : 'application/json'}
        response=None
        if method=="post":
            response = requests.post(url, headers=headers, data=json.dumps(params_list))
        else:
            response = requests.get(url, headers=headers, params=params_list)

        if response.status_code==400:
            # retry
            self._make_api_request(url,params_list,method)

        try:
            data = json.loads(response.content)
        except ValueError:
            data = None # can't parse the json so no data
        # print json.dumps(data,sort_keys=True,indent=4, separators=(',', ': '))
        return data, response.status_code



    def start_test_session(self,session_name,modules):
        param_dict = {}
        param_dict["test_session_name"] = session_name
        param_dict["test_session_modules"] = modules
        param_dict["test_session_build"] = "0"
        param_dict["test_session_owner_id"] = "0"
        param_dict["test_session_tag_id"] = "0"

        # send the request:
        self._make_api_request("http://localhost:5000/set_test_session_start", param_dict,"post")

    def end_test_session(self):
        # send the request:
        self._make_api_request("http://localhost:5000/set_test_session_end", None,"get")