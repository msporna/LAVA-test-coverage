/*
Copyright (c) 2016-2018 by Michal Sporna and contributors.  See AUTHORS
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// <para>Version: 1.0</para>
/// <para>Author: Michal Sporna</para>
/// <para>Support: https://github.com/msporna/LAVA-test-coverage </para>
/// </summary>
public class LavaHelper : MonoBehaviour
{

    #region declare
    private string _LavaUrl = "http://localhost:5000/send_instrumentation_stats";
    private static List<string> _SentStats;
    private bool _SendingLocked;
    public static List<string[]> Queue;


    #endregion

    #region awake and update
    void Awake()
    {
        Queue = new List<string[]>();
        _SentStats = new List<string>();

        Debug.Log("LAVA_TEST_COVERAGE PLUGIN READY.");
    }

    void Update()
    {
        if(Queue.Count>0)
        {
            if (!_SentStats.Contains(Queue[0][0]+"_"+ Queue[0][1]))
            {
                if(!_SendingLocked)
                {
                    //send one that came first
                    StartCoroutine(ExecuteGET(Queue[0]));
                }


            }
            else
            {
                Queue.RemoveAt(0);//there is a stat waiting on the list that was already sent, so remove it
            }


        }

    }

    #endregion

    #region private
    IEnumerator ExecuteGET(string[] statDetails)
    {
        _SendingLocked = true;
        using (UnityWebRequest www = UnityWebRequest.Get(_LavaUrl + "?file=" + statDetails[0] + "&line_guid_p=" + statDetails[1] + "&route=None&related_code_line=" + statDetails[2] + "&inject_type=" + statDetails[3]))
        {

            Debug.Log("[LavaTestCoverage] Sending stat:" + statDetails[0] + "_" + statDetails[1]);
            yield return www.SendWebRequest();


            if (www.isNetworkError || www.isHttpError)
            {
                //raise exception as I'm assuming you are gathering coverage stats so be aware of
                //the fact that it's just crashed
                throw new System.Exception("[LavaTestCoverage] Error occurred while sending stats to LAVA backend: " + www.error);
            }
            else
            {
                if (www.downloadHandler.text == "saved")
                {
                    Debug.Log("[LavaTestCoverage] Stats sent successfuly! ("+statDetails[0]+"/"+statDetails[1]+")");
                    if(Queue.Count>0)
                    {
                        Queue.RemoveAt(0); //remove stat that was just sent to backend
                    }

                    //store already sent stat so it is not sent again to save backend from ddos
                    _SentStats.Add(statDetails[0] + "_" + statDetails[1]);
                    _SendingLocked = false;

                }

                _SendingLocked = false;

            }
        }
    }

   static bool DoesQueueContainStat(string file, string line_guid)
    {
        if(Queue==null)
        {
            Queue = new List<string[]>();
        }

        for(int i=0;i<Queue.Count;i++)
        {
            if(Queue[i][0]==file&&Queue[i][1]==line_guid)
            {
                return true;
            }
        }

        return false;
    }


    #endregion

    #region public
    public static void SendStats(string file, string line_guid, string file_line, string probe_type)
    {
        //add to queue only if this stat is not already waiting to be sent to backend
        if(!DoesQueueContainStat(file,line_guid))
        {
            Queue.Add(new string[] { file, line_guid, file_line, probe_type });
        }




    }

    #endregion
}
