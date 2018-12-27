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

public class IDEController : MonoBehaviour {

    [HideInInspector]
    public string WhichFileToShow; //about,about_injected,index,index_injected,main,main_injected

    Transform _AboutFileRegular;
    Transform _AboutFileInjected;
    Transform _IndexFileRegular;
    Transform _IndexFileInjected;
    Transform _MainFileRegular;
    Transform _MainFileInjected;
    Transform _ConfigJsonFile;


    Transform _AboutInfoRegular;
    Transform _AboutInfoInjected;
    Transform _IndexInfoRegular;
    Transform _IndexInfoInjected;
    Transform _MainInfoRegular;
    Transform _MainInfoInjected;

    bool _ReferencesSet = false;
    bool _ProbesInjected = false;

    [HideInInspector]
    public MainController MainController;


    void SetupReferences()
    {
        if(_ReferencesSet)
        {
            return;
        }

        _AboutFileRegular = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0);
        _AboutFileInjected = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1);
        _IndexFileRegular= transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0);
        _IndexFileInjected = transform.GetChild(1).GetChild(0).GetChild(2).GetChild(1);
        _MainFileRegular = transform.GetChild(1).GetChild(0).GetChild(3).GetChild(0);
        _MainFileInjected = transform.GetChild(1).GetChild(0).GetChild(3).GetChild(1);
        _ConfigJsonFile = transform.GetChild(1).GetChild(0).GetChild(4).GetChild(0);

        _AboutInfoRegular = transform.GetChild(0).GetChild(0);
        _AboutInfoInjected = transform.GetChild(0).GetChild(1);
        _IndexInfoRegular = transform.GetChild(0).GetChild(2);
        _IndexInfoInjected = transform.GetChild(0).GetChild(3);
        _MainInfoRegular = transform.GetChild(0).GetChild(4);
        _MainInfoInjected = transform.GetChild(0).GetChild(5);
        _ReferencesSet = true;
    }


    public void OnOpen(bool probesInjected)
    {
        SetupReferences();
        _ProbesInjected = probesInjected;
        ShowFile();
    }

    void ClearAll()
    {
        _AboutInfoRegular.gameObject.SetActive(false);
        _AboutInfoInjected.gameObject.SetActive(false);
        _IndexInfoRegular.gameObject.SetActive(false);
        _IndexInfoInjected.gameObject.SetActive(false);
        _MainInfoRegular.gameObject.SetActive(false);
        _MainInfoInjected.gameObject.SetActive(false);

        _AboutFileRegular.gameObject.SetActive(false);
        _AboutFileInjected.gameObject.SetActive(false);
        _IndexFileRegular.gameObject.SetActive(false);
        _IndexFileInjected.gameObject.SetActive(false);
        _MainFileRegular.gameObject.SetActive(false);
        _MainFileInjected.gameObject.SetActive(false);
    }



    void ShowFile()
    {
        ClearAll();

        if(_ProbesInjected)
        {
            if(WhichFileToShow=="about" || WhichFileToShow=="index" || WhichFileToShow=="main")
            {
                WhichFileToShow = WhichFileToShow + "_injected";
            }

        }

        switch(WhichFileToShow)
        {
            case "about":
                {
                    _AboutFileRegular.gameObject.SetActive(true);
                    _AboutInfoRegular.gameObject.SetActive(true);

                    break;
                }
            case "about_injected":
                {
                    _AboutFileInjected.gameObject.SetActive(true);
                    _AboutInfoInjected.gameObject.SetActive(true);

                    break;
                }
            case "index":
                {
                    _IndexFileRegular.gameObject.SetActive(true);
                    _IndexInfoRegular.gameObject.SetActive(true);
                    break;
                }
            case "index_injected":
                {
                    _IndexFileInjected.gameObject.SetActive(true);
                    _IndexInfoInjected.gameObject.SetActive(true);
                    break;
                }
            case "main":
                {
                    _MainFileRegular.gameObject.SetActive(true);
                    _MainInfoRegular.gameObject.SetActive(true);

                    MainController.OnMissionCompelete(new int[] { 7 });
                    break;
                }
            case "main_injected":
                {
                    _MainFileInjected.gameObject.SetActive(true);
                    _MainInfoInjected.gameObject.SetActive(true);
                    MainController.OnMissionCompelete(new int[] { 13 });
                    break;
                }
            case "config_json":
                {
                    _ConfigJsonFile.gameObject.SetActive(true);

                    MainController.OnMissionCompelete(new int[] { 2 });

                    break;
                }
        }
    }

    public void OnClose()
    {
        _ConfigJsonFile.gameObject.SetActive(false);
        _MainFileInjected.gameObject.SetActive(false);
        _MainInfoInjected.gameObject.SetActive(false);
        _MainFileRegular.gameObject.SetActive(false);
        _MainInfoRegular.gameObject.SetActive(false);
        _IndexFileInjected.gameObject.SetActive(false);
        _IndexInfoInjected.gameObject.SetActive(false);
        _IndexFileRegular.gameObject.SetActive(false);
        _IndexInfoRegular.gameObject.SetActive(false);
        _AboutFileInjected.gameObject.SetActive(false);
        _AboutInfoInjected.gameObject.SetActive(false);
        _AboutFileRegular.gameObject.SetActive(false);
        _AboutInfoRegular.gameObject.SetActive(false);

        MainController.EnableTopBarNavigation();

    }
}
