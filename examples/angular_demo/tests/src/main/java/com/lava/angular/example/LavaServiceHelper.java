package com.lava.angular.example;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.HttpClientBuilder;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.UnsupportedEncodingException;
import java.net.URL;
import java.net.HttpURLConnection;
import java.net.URLEncoder;
import java.util.Dictionary;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

//sends rest api requests to lava instrument server that collects info about executed tests
public class LavaServiceHelper {


    public static void SendRequest(String url, Map<String,String> params) {

        try {
            String paramString = "";
            if(params!=null) {


                //convert list of params to a param string


                Iterator it = params.entrySet().iterator();
                while (it.hasNext()) {
                    Map.Entry pair = (Map.Entry) it.next();
                    String paramName = URLEncoder.encode(pair.getKey().toString(), "UTF-8");
                    String paramValue = URLEncoder.encode(pair.getValue().toString(), "UTF-8");
                    paramString += paramName + "=" + paramValue + "&";
                    it.remove(); // avoids a ConcurrentModificationException

                }


                //remove last &
                paramString = paramString.substring(0, paramString.length() - 1);
            }



            HttpClient client = HttpClientBuilder.create().build();
            HttpGet get = new HttpGet(url + "?" + paramString);

            HttpResponse response = client.execute(get);
            System.out.println("set test with response:" + response.getStatusLine());


        } catch (IOException e) {
            System.out.println(e.toString());
        }


    }

    //sends request to instrumenter server and tells it which test is being executed now
    public static void SetTest(String testId,String testName,String touchedModule)
    {
        Map<String,String> params=new HashMap<String,String>();
        params.put("test_id",testId);
        params.put("name",testName);
        params.put("touched_module",touchedModule);
        SendRequest("http://localhost:5000/set_current_test",params);
    }

    public static void StopCoverageSession()
    {
        SendRequest("http://localhost:5000/set_test_session_end",null);
    }


    public static void StartCoverageSession(String name)
    {
        Map<String,String> params=new HashMap<String,String>();
        params.put("test_session_name",name);
        SendRequest("http://localhost:5000/set_test_session_start",params);
    }
}