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
using UnityEngine.UI;

public class TerminalController : MonoBehaviour {

    [System.Serializable]
    public struct OptionConfig
    {
        public int ID;
        public string Name;
        public int MissionRequired; //which mission must be completed for this option to appear

    }

    [System.Serializable]
    public struct ExpectedOptionConfig
    {
        public int[] Options;
        public string Action;
        public string Param;


    }

    [System.Serializable]
    public struct OptionSet
    {
        //1 option set has multiple options and user decides the option to run but
        //there is only 1 option pushing gameplay forward

        public string ID;
        public string Description;
        public ExpectedOptionConfig[] ExpectedOptions; //for example, 0,1 means I expect option 0 and then option 1 exactly in that order
        public OptionConfig[] AllOptions;


    }

    struct PlacedOptionConfig
    {
        public int OptionConfigID;
        public int RelatedSlotIndex;


    }

    public string UserName;
    public GameObject TextPrefab;
    public GameObject OptionPrefab;

    public OptionSet[] OptionSets;
    public int MaxOutputLines = 20;
    [Tooltip("When output lines appear in terminal 1 by 1 automatically, then this value specifies interval between each line appearance")]
    public float AutomatedOutputIntervalSeconds;


    [HideInInspector]
    public string ActiveSetID; //id of active set from optionSets


    [HideInInspector]
    public bool IsDraggingOption;
    [HideInInspector]
    public bool IsDraggedOptionOverSlot;
    [HideInInspector]
    public Transform OptionsDestTransform;
    [HideInInspector]
    public int ActiveSlotID; //the one that currently user is holding option above
    [HideInInspector]

    int _CurrentOutputLineCount;

    string _AutomatedLineShowingMode;//client,server
    int _AutomatedLineCurrentLineCt;

    bool _IsInstrumentationServerStarted;
    bool _WasInstrumentationClientStarted;
    bool _IsWebExampleServerRunning;
    MainController _MainController;

    Transform _OptionsPanel;
    Transform _SlotsPanel;
    Transform _OutputPanel;
    Text _Text;

    bool _ReferencesSet = false;



    void SetupReferences()
    {
        if(_ReferencesSet)
        {
            return;
        }

        _SlotsPanel = transform.GetChild(0).GetChild(1);
        _OptionsPanel = transform.GetChild(1);
        _OutputPanel = transform.GetChild(2);
        _Text = transform.GetChild(0).GetChild(0).GetComponent<Text>();

        _ReferencesSet = true;

    }


    public void OnOpen()
    {
        SetupReferences();

        ShowActiveOptionsSet();
        SetTerminalText();

        _MainController.OnMissionCompelete(new int[] { 2 });
    }


    private void Update()
    {
        if(_IsInstrumentationServerStarted)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    _IsInstrumentationServerStarted = false;
                    ShowOutput("http://localhost:5000 terminated.", 0);
                }
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    _IsWebExampleServerRunning = false;
                    ShowOutput("http://localhost:8787 terminated.", 0);
                }
            }

        }

    }


    public void SetMainController(MainController mc)
    {
        _MainController = mc;
    }


    OptionSet GetOptionSetByID(string id)
    {
        for (int i = 0; i < OptionSets.Length; i++)
        {
            if (OptionSets[i].ID == id)
            {
                return OptionSets[i];
            }
        }

        throw new System.Exception("Option set cannot be found");
    }

    public void SetTerminalText()
    {
        switch(ActiveSetID)
        {
            case "set1":
                {
                    _Text.text = "usr1:~#";
                    break;
                }
            case "set2":
                {
                    _Text.text = "usr1:./lava_master#";
                    break;
                }
            case "set3":
                {
                    _Text.text = "usr1:./lava_master/client#";
                    break;
                }
            case "set4":
                {
                    _Text.text = "usr1:./lava_master/server#";
                    break;
                }
            case "set5":
                {
                    _Text.text = "usr1:./lava_master/examples#";
                    break;
                }
        }
    }

    public void ShowOutput(string outputText,int mode)
    {

        if (_CurrentOutputLineCount > MaxOutputLines)
        {
            for (int i = 0; i < _OutputPanel.childCount; i++)
            {
                Destroy(_OutputPanel.GetChild(i).gameObject);
            }

            _CurrentOutputLineCount = 0;
        }


        GameObject outputTextInstance = Instantiate(this.TextPrefab);
        outputTextInstance.GetComponent<TerminalLineController>().SetText(outputText, mode);


        outputTextInstance.transform.parent = this._OutputPanel;
        outputTextInstance.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        _CurrentOutputLineCount += 1;

    }

    public void ShowActiveOptionsSet()
    {
        //clear slots
        for(int i=0;i<_SlotsPanel.childCount;i++)
        {
            if(_SlotsPanel.GetChild(i).childCount>0)
            {
                Destroy(_SlotsPanel.GetChild(i).GetChild(0).gameObject);
            }
        }
        //clear options
        for(int i=0;i<_OptionsPanel.childCount;i++)
        {
            Destroy(_OptionsPanel.GetChild(i).gameObject);
        }





        //add new options

        OptionSet option_set = GetOptionSetByID(this.ActiveSetID);

        for(int i=0;i<option_set.AllOptions.Length;i++)
        {
            if(_MainController.CurrentMission>=option_set.AllOptions[i].MissionRequired)
            {
                GameObject optionObject = Instantiate(OptionPrefab); //consists of container,option item and text
                optionObject.transform.parent = _OptionsPanel;
                optionObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                Transform optionItem = optionObject.transform.GetChild(0);

                optionItem.GetChild(0).GetComponent<Text>().text = option_set.AllOptions[i].Name;
                TerminalOptionController optionCtrl = optionItem.GetComponent<TerminalOptionController>();
                optionCtrl.TerminalControllerInstance = this;
                optionCtrl.OptionConfigID = option_set.AllOptions[i].ID;
            }


        }
    }


    Dictionary<int, int> RedefineSlotIndexes(List<PlacedOptionConfig> placedOptions)
    {
        //replace relatedSlotIndex with 0 based index, so for example if indexes are 4,5 make them 0,1 if indexes are 2,3,4 make them 0,1,2 etc.
        //because there are 6 slots and user can place 2 required options to slot 4 and 5 and expected options set is based on 0,1 not 4,5 -it cares about which item must go first not exact slot
        List<int> indexes = new List<int>();
        foreach (PlacedOptionConfig placedOption in placedOptions)
        {
            indexes.Add(placedOption.RelatedSlotIndex);
        }
        indexes.Sort();
        Dictionary<int, int> redefinedIndexesDict = new Dictionary<int, int>(); //rdlate, old with new index
        for (int i = 0; i < indexes.Count; i++)
        {
            redefinedIndexesDict[indexes[i]] = i; //so if we have 4,5,1 items placed, after sorting will be 1,4,5 and assign 1=i (0),4=i (1) etc.
        }


        return redefinedIndexesDict;
    }

    public void ExecuteCommand()
    {

        _MainController.HideMissionPopup();

        //get command (placed options)
        List<PlacedOptionConfig> placedOptions = GetPlacedOptions();
        if(placedOptions.Count==0)
        {
            return;
        }

        Dictionary<int,int> redefinedIndexes = RedefineSlotIndexes(placedOptions);


        //validate command - check if placed options are in expectedOptions collection of the current option set and in the right order
        OptionSet option_set = GetOptionSetByID(this.ActiveSetID);


        bool invalidCommand = false;

        List<int> correctOptions = new List<int>();
        for (int i = 0; i < option_set.ExpectedOptions.Length; i++)
        {


            correctOptions.Clear();

            for (int option = 0; option < option_set.ExpectedOptions[i].Options.Length; option++)
            {
                if(placedOptions.Count!= option_set.ExpectedOptions[i].Options.Length)
                {
                    continue; //placed options count must match count of expected options
                }

                foreach (PlacedOptionConfig placedOption in placedOptions)
                {
                    //get item from placed options matching option index
                    //to ensure the right order

                    //use redefined index for this comparison, based on relatedSlotIndex (it's a key,
                    //value is the redefined value we are after here)
                    if (redefinedIndexes[placedOption.RelatedSlotIndex] == option)
                    {
                        //now compare option value
                        if (placedOption.OptionConfigID == option_set.ExpectedOptions[i].Options[option])
                        {
                            correctOptions.Add(placedOption.OptionConfigID);

                        }



                    }

                }




            }

            if (correctOptions.Count == placedOptions.Count)
            {
                //options were placed correctly, proceed

                invalidCommand = false;

                //execute command
                //note: if cd .. is executed, make sure to set active set id to one relative to current page + show the current set & terminal text
                switch(option_set.ExpectedOptions[i].Action)
                {
                    case "go_to":
                        {
                            switch(option_set.ExpectedOptions[i].Param)
                            {

                                case "lava_folder":
                                    {
                                        _MainController.GoTo("lava_folder");

                                        SwitchActiveSetID("set2");
                                        break;
                                    }
                                case "desktop":
                                    {
                                        _MainController.GoTo("desktop");

                                        SwitchActiveSetID("set1");
                                        break;
                                    }
                                case "client_folder":
                                    {
                                        _MainController.GoTo("client_folder");

                                        SwitchActiveSetID("set3");
                                        break;
                                    }
                                case "server_folder":
                                    {
                                        _MainController.GoTo("server_folder");

                                        SwitchActiveSetID("set4");
                                        break;
                                    }
                                case "examples_folder":
                                    {
                                        _MainController.GoTo("examples_folder");

                                        SwitchActiveSetID("set5");
                                        break;
                                    }

                            }

                            break;
                        }

                    case "list":
                        {
                            ShowOutput(option_set.ExpectedOptions[i].Param, 0);
                            break;
                        }
                    case "start":
                        {
                            switch (option_set.ExpectedOptions[i].Param)
                            {
                                case "python_server":
                                    {
                                        SimulateWebExample();
                                        break;
                                    }
                                case "instrument_client":
                                    {
                                        SimulateInstrumentClient();
                                        break;
                                    }
                                case "instrument_server":
                                    {
                                        SimulateInstrumentServer();
                                        break;
                                    }

                            }
                            break;
                        }


                }


                break; //stop searching through expected patterns
            }
            else
            {
                //options placed incorrectly
                //show error (output of terminal)
                invalidCommand = true;
            }
        }

       if(invalidCommand)
        {
            ShowOutput("Invalid command", 1);
        }


    }

    void SimulateInstrumentClient()
    {
        if(!_IsInstrumentationServerStarted)
        {
            ShowOutput("No connection could be made because the target machine actively refused it", 1);
        }

        _AutomatedLineShowingMode = "client";
        _AutomatedLineCurrentLineCt = 0;
        OnOutputlineTimeToShow();

        _WasInstrumentationClientStarted = true;


    }


    void OnOutputlineTimeToShow()
    {
        switch(_AutomatedLineShowingMode)
        {
            case "client":
                {
                    switch(_AutomatedLineCurrentLineCt)
                    {
                        case 0:
                            {
                                ShowOutput("[Parsing config.json]", 0);

                                _AutomatedLineCurrentLineCt += 1;

                                break;
                            }
                        case 1:
                            {
                                ShowOutput("[Detecting sources...]", 0);

                                _AutomatedLineCurrentLineCt += 1;

                                break;
                            }
                        case 2:
                            {
                                //this will simulate that previous operation (detecting sources)
                                //takes some time as requires more resources
                                ShowOutput("", 0);

                                _AutomatedLineCurrentLineCt += 1;

                                break;
                            }
                        case 3:
                            {
                                ShowOutput("+------------------------------------+", 0);
                                ShowOutput("|Detected Sources                    |", 0);
                                ShowOutput("+------------------------------------+", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 4:
                            {
                                ShowOutput("|./lava_master/examples/jquery.js (excluded)  |", 0);
                                ShowOutput("|./lava_master/examples/main.js  |", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 5:
                            {
                                ShowOutput("+--------------------------------+", 0);
                                ShowOutput("[Detecting templates...]", 0);

                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 6:
                            {
                                ShowOutput("+------------------------------------+", 0);
                                ShowOutput("|Detected Templates                   |", 0);
                                ShowOutput("+------------------------------------+", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 7:
                            {
                                ShowOutput("|./lava_master/examples/index.html  |", 0);
                                ShowOutput("|./lava_master/examples/about.html  |", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 8:
                            {
                                ShowOutput("+--------------------------------+", 0);
                                ShowOutput("Injecting...", 0);

                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 9:
                            {
                                ShowOutput("+--------------------------------+", 0);
                                ShowOutput("Injecting...", 0);

                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 10:
                            {
                                ShowOutput("./lava_master/examples/main.js 143ms", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 11:
                            {
                                ShowOutput("Done", 0);

                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 12:
                            {
                                ShowOutput("Uploading...", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 13:
                            {
                                ShowOutput("[============--------------] 33.3% ...index.html:1", 0);
                                ShowOutput("[===================-------] 66.7% ...about.html:1", 0);
                                ShowOutput("[==========================] 100.0% ...main.js:30", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 14:
                            {
                                ShowOutput("Successfully stored instrumentation token: MjAssD2312DSAd21dsa!@#dsa23ASD", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 15:
                            {
                                ShowOutput("Done", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                break;
                            }
                        case 16:
                            {
                                ShowOutput("+-----------------+------+", 0);
                                ShowOutput("|Upload to lava DB|Status|", 0);
                                ShowOutput("+-----------------+------+", 0);
                                ShowOutput("|[main.js]        | 200   |", 0);
                                ShowOutput("|[index.html]     | 200   |", 0);
                                ShowOutput("|[about.html]     | 200   |", 0);
                                ShowOutput("|Routes upload (2)| 200   |", 0);
                                ShowOutput("+-----------------+------+", 0);


                                _AutomatedLineCurrentLineCt += 1;
                                _MainController.OnProbesInjected();

                                _MainController.OnMissionCompelete(new int[] { 11 });
                                break;
                            }
                    }



                    break;
                }
            case "server":
                {

                    break;
                }
        }

        Invoke("OnOutputlineTimeToShow", AutomatedOutputIntervalSeconds);

    }


    void SimulateInstrumentServer()
    {


        ShowOutput(" * Running on http://0.0.0.0:5000/ (Press CTRL+C to quit)", 0);


        _IsInstrumentationServerStarted = true;

        _MainController.OnMissionCompelete(new int[] { 5 });
    }

    void SimulateWebExample()
    {
        ShowOutput(" Serving at port 8787 (Press CTRL+V to quit)", 0);
        _IsWebExampleServerRunning = true;
        _MainController.OnMissionCompelete(new int[] { 19 });

        _MainController.MakeLavaTestSessionButtonActive();
    }


    public void SwitchActiveSetID(string newSetID)
    {

        ActiveSetID = newSetID;
        SetTerminalText();
        ShowActiveOptionsSet();
    }

    /// <summary>
    /// get all options that are currently assigned to terminal slots
    /// </summary>
    /// <returns></returns>
    List<PlacedOptionConfig> GetPlacedOptions()
    {
        List<PlacedOptionConfig> placedOptions = new List<PlacedOptionConfig>();

        for(int i=0;i<_SlotsPanel.childCount;i++)
        {
            Transform slot = _SlotsPanel.GetChild(i);
            if(slot.childCount>0)
            {
                PlacedOptionConfig placedOptionConfig = new TerminalController.PlacedOptionConfig();
                placedOptionConfig.OptionConfigID = slot.GetChild(0).GetComponent<TerminalOptionController>().OptionConfigID;
                placedOptionConfig.RelatedSlotIndex = i;

                placedOptions.Add(placedOptionConfig);
            }
        }

        return placedOptions;

    }

    public bool IsServerRunning()
    {
        return _IsInstrumentationServerStarted;
    }

    public bool WasClientExecuted()
    {
        return _WasInstrumentationClientStarted;
    }

    public bool IsWebExampleServerRunning()
    {
        return _IsWebExampleServerRunning;
    }
}
