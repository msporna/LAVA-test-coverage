package [YOUR PACKAGE NAME HERE]


import java.io.IOException;
import java.net.URLEncoder;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Date;

import okhttp3.Call;
import okhttp3.Callback;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import okhttp3.ResponseBody;

public class LavaCoverageHelper {

    private static String _LavaUrl = "http://[YOUR LAVA HOST HERE]:5000/send_instrumentation_stats";
    private static List<String> _processedLineGuids = new ArrayList<String>();


    static void SendRequest(Map<String,String> params, String lineGuid) {
        System.out.println("Sending stats for line guid: "+lineGuid);
        OkHttpClient httpClient = new OkHttpClient();

        try {
            String paramString = "";
            if(params!=null && params.size()>0) {

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


                //send
                Request httpRequest = new Request.Builder()
                        .url(_LavaUrl + "?" + paramString)
                        .build();

                httpClient.newCall(httpRequest).enqueue(new Callback() {
                    @Override
                    public void onFailure(Call call, IOException e) {
                        e.printStackTrace();
                    }

                    @Override
                    public void onResponse(Call call, Response response) throws IOException {
                        try (ResponseBody responseBody = response.body()) {
                            if (response.isSuccessful())
                            {
                                _processedLineGuids.add(lineGuid);
                            }
                            else{
                                throw new IOException("Unexpected code " + response);

                            }

                        }
                    }
                });
            }

        } catch (IOException e) {
            System.out.println("Error while sending stats...\n"+e.toString());
        }


    }




    ///called by probes injected
    public static void SendStats(String file, String lineGuid, String fileLine, String probeType,String custom)
    {
        Map<String,String> params=new HashMap<String,String>();
        params.put("file",file);
        params.put("line_guid_p",lineGuid);
        params.put("related_code_line",fileLine);
        params.put("inject_type",probeType);
        params.put("custom_value",custom);
        params.put("send_date",new SimpleDateFormat("yyyy-MM-dd HH:mm:ss:SS").format(new Date()));

        if(!_processedLineGuids.contains(lineGuid))
        {
            ThreadRunner tRunner = new ThreadRunner(params,lineGuid);
            Thread t = new Thread(tRunner);
            t.start();
        }

    }


}

class ThreadRunner implements Runnable {

    private Map<String,String> _paramsToSend;
    private String _lineGuid;

    public ThreadRunner(Map<String,String> params,String lineGuid) {
        this._paramsToSend=params;
        this._lineGuid=lineGuid;
    }

    public void run() {
        LavaCoverageHelper.SendRequest(_paramsToSend,_lineGuid);
    }
}
