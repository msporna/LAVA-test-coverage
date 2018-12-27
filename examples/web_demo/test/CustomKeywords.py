import json
from robot.libraries.BuiltIn import BuiltIn
import requests




class CustomKeywords:
    '''
    custom keywords that extend default robot framework and selenium2library's keywords
    '''
    __version__ = '0.1'


    def _make_api_request(self,url,params_list):
        """
        make call to given url and return response
        which is expected to be json
        :param url:
        :return:
        """
        print "Sending request to: "+url
        headers = {'Accept' : 'application/json', 'Content-Type' : 'application/json'}
        response = requests.get(url, headers=headers, params=params_list)
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


        # send the request:
        self._make_api_request("http://localhost:5000/set_test_session_start", param_dict)

    def end_test_session(self):
        

        # send the request:
        self._make_api_request("http://localhost:5000/set_test_session_end", None)