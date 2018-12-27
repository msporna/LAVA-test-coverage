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

public class MainController : MonoBehaviour {

    [System.Serializable]
    public struct MissionConfig
    {
        public int ID;
        public string Description;
        public string WhyStatement;
        public string HowStatement;
        public string ReferenceStatement;
        public float Reward;
        public float Time;
        [Tooltip("If false, then reference box will be disabled or hidden.")]
        public bool HasReference;
        [Tooltip("If false, then mission will be skipped.")]
        public bool IsActive;


    }



    [HideInInspector]
    public int CurrentMission = 0;
    public MissionConfig[] Missions;
    public int Difficulty;
    public Color ErrorColor;
    public Color MainThemeColor;



    // public int Difficulty;//0-easy,1-harder

    Transform _DesktopPanel;
    Transform _LavaPanel;
    Transform _LavaClientPanel;
    Transform _LavaServerPanel;
    Transform _LavaExamplesPanel;
    Transform _BrowserPanel;
    Transform _IDEPanel;
    Transform _TerminalPanel;
    Transform _MissionPanelPlaceholder;
    Transform _RewardPopup;
    Transform _RewardPopupPlaceholderOut;
    Transform _RewardPopupPlaceholderIn;
    Transform _IntroPanel;
    Transform _OutroPanel;
    Text _OutroUsernameText;
    Text _UsernameTextField;
    Text _OutroXPText;
    Transform _InstrumentJSFile;
    Transform _LogoutButton;
    Text _IntroUsernameLabel;
    Transform _BrowserButton;

    Transform _Nav1;
    Transform _Nav2;
    Transform _Nav3;


    List<int> _FinishedMissions;

    NavigationController _NavigationController;
    TerminalController _TerminalController;
    IDEController _IDEController;
    BrowserController _BrowserController;
    MissionPanelController _MissionPanelController;
    RwardPopupController _RewardPopupController;
    JSRunner _JSRunner;

    string _OpenedApp = null;
    string _CurrentPanel = "desktop";
    string _PreviousPanel = null;
    float _CurrentXPValue = 0;
    string _Username;
    bool _ProbesInjected;

    void Start()
    {


        //get references
        _DesktopPanel = transform.GetChild(1);
        _LavaPanel = transform.GetChild(2);

        _LavaClientPanel = _LavaPanel.GetChild(12);
        _LavaServerPanel = _LavaPanel.GetChild(13);
        _LavaExamplesPanel = _LavaPanel.GetChild(14);


        _BrowserPanel = transform.GetChild(3);
        _BrowserController = _BrowserPanel.GetComponent<BrowserController>();


        _IDEPanel = transform.GetChild(4);
        _IDEController = _IDEPanel.GetComponent<IDEController>();
        _IDEController.MainController = this;



        _NavigationController = transform.GetChild(0).GetComponent<NavigationController>();
        _NavigationController.SetMainController(this);

        _TerminalPanel = transform.GetChild(5);
        _TerminalController = _TerminalPanel.GetComponent<TerminalController>();
        _TerminalController.SetMainController(this);

        _MissionPanelPlaceholder = transform.GetChild(7);
        _MissionPanelController = transform.GetChild(6).GetComponent<MissionPanelController>();
        _MissionPanelController.Init(_MissionPanelPlaceholder,this);

        _FinishedMissions = new List<int>();

        _RewardPopup = transform.GetChild(10);
        _RewardPopupController = _RewardPopup.GetComponent<RwardPopupController>();
        _RewardPopupPlaceholderIn = transform.GetChild(9);
        _RewardPopupPlaceholderOut = transform.GetChild(8);
        _RewardPopupController.Init(_RewardPopupPlaceholderOut, _RewardPopupPlaceholderIn);

        _JSRunner = gameObject.GetComponent<JSRunner>();

        _IntroPanel = transform.GetChild(11);
        _UsernameTextField = transform.GetChild(11).GetChild(0).GetChild(0).GetComponent<Text>();
        _OutroPanel = transform.GetChild(12);
        _OutroUsernameText = _OutroPanel.GetChild(1).GetComponent<Text>();
        _OutroXPText = _OutroPanel.GetChild(9).GetComponent<Text>();
        _InstrumentJSFile = transform.GetChild(2).GetChild(14).GetChild(5);
        _LogoutButton = transform.GetChild(0).GetChild(2).GetChild(6).GetChild(1);
        _IntroUsernameLabel = _IntroPanel.GetChild(1).GetComponent<Text>();
        _BrowserButton = transform.GetChild(0).GetChild(2).GetChild(5);

        _Nav1 = transform.GetChild(0).GetChild(2).GetChild(0);
        _Nav2 = transform.GetChild(0).GetChild(2).GetChild(1);
        _Nav3 = transform.GetChild(0).GetChild(2).GetChild(2);

    }


   public void GoTo(string dest)
   {
        _PreviousPanel = _CurrentPanel;

        switch(dest)
        {
            case "desktop":
                _DesktopPanel.gameObject.SetActive(true);

                _LavaPanel.gameObject.SetActive(false);
                _LavaExamplesPanel.gameObject.SetActive(false);
                _LavaClientPanel.gameObject.SetActive(false);
                _LavaServerPanel.gameObject.SetActive(false);
                _BrowserPanel.gameObject.SetActive(false);
                _IDEPanel.gameObject.SetActive(false);
                _CurrentPanel = "desktop";


                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                _TerminalController.ActiveSetID = "set1";
                break;
            case "lava_folder":
                _LavaPanel.gameObject.SetActive(true);

                _LavaExamplesPanel.gameObject.SetActive(false);
                _LavaClientPanel.gameObject.SetActive(false);
                _LavaServerPanel.gameObject.SetActive(false);
                _DesktopPanel.gameObject.SetActive(false);
                _BrowserPanel.gameObject.SetActive(false);
                _IDEPanel.gameObject.SetActive(false);
                _CurrentPanel = "lava_folder";

                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                _TerminalController.ActiveSetID = "set2";
                break;
            case "client_folder":
                _LavaClientPanel.gameObject.SetActive(true);

                _DesktopPanel.gameObject.SetActive(false);
                _BrowserPanel.gameObject.SetActive(false);
                _IDEPanel.gameObject.SetActive(false);
                _CurrentPanel = "client_folder";
                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                _TerminalController.ActiveSetID = "set3";

                break;
            case "server_folder":
                _LavaServerPanel.gameObject.SetActive(true);

                _DesktopPanel.gameObject.SetActive(false);
                _BrowserPanel.gameObject.SetActive(false);
                _IDEPanel.gameObject.SetActive(false);
                _CurrentPanel = "server_folder";
                _TerminalController.ActiveSetID = "set4";
                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                break;
            case "examples_folder":
                _LavaExamplesPanel.gameObject.SetActive(true);

                _DesktopPanel.gameObject.SetActive(false);
                _BrowserPanel.gameObject.SetActive(false);
                _IDEPanel.gameObject.SetActive(false);
                _CurrentPanel = "examples_folder";
                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                _TerminalController.ActiveSetID = "set5";
                break;
            case "browser":
                _BrowserPanel.gameObject.SetActive(true);

                //_LavaPanel.gameObject.SetActive(false);
                //_LavaExamplesPanel.gameObject.SetActive(false);
                //_LavaClientPanel.gameObject.SetActive(false);
                //_LavaServerPanel.gameObject.SetActive(false);
                _IDEPanel.gameObject.SetActive(false);
                //_DesktopPanel.gameObject.SetActive(false);
                _TerminalPanel.gameObject.SetActive(false);
                //_CurrentPanel = "browser";
                _OpenedApp = "browser";
                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                _BrowserController.OnOpen();
                break;
            case "ide":
                _IDEPanel.gameObject.SetActive(true);

                _BrowserPanel.gameObject.SetActive(false);
                //_DesktopPanel.gameObject.SetActive(false);
                //_LavaPanel.gameObject.SetActive(false);
                //_LavaExamplesPanel.gameObject.SetActive(false);
                //_LavaClientPanel.gameObject.SetActive(false);
                //_LavaServerPanel.gameObject.SetActive(false);
                _TerminalPanel.gameObject.SetActive(false);
                //_CurrentPanel = "ide";
                _OpenedApp = "ide";
                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                _IDEController.OnOpen(this._ProbesInjected);

                break;
            case "terminal":
                _TerminalPanel.gameObject.SetActive(true);

                _BrowserPanel.gameObject.SetActive(false);
                //_DesktopPanel.gameObject.SetActive(false);
                //_LavaPanel.gameObject.SetActive(false);
                //_LavaExamplesPanel.gameObject.SetActive(false);
                //_LavaClientPanel.gameObject.SetActive(false);
                //_LavaServerPanel.gameObject.SetActive(false);
                _IDEPanel.gameObject.SetActive(false);
                //_CurrentPanel = "terminal";
                _OpenedApp = "terminal";
                _NavigationController.UpdateCurrentLocation(this._CurrentPanel);
                _TerminalController.OnOpen();

                break;
        }


   }

    public string GetCurrentPanel()
    {
        return _CurrentPanel;
    }

    public string GetOpenedApp()
    {
        Debug.Log("OPENED APP: " + _OpenedApp);
        return _OpenedApp;
    }

    public void CloseTerminalApp()
    {
        _TerminalPanel.gameObject.SetActive(false);
        _OpenedApp = null;
        GoTo(_CurrentPanel);
        EnableTopBarNavigation();
    }

    public void ShowTerminalPanel()
    {
        GoTo("terminal");

    }

    public void ShowIDEPanel(string fileName)
    {
        _IDEController.WhichFileToShow = fileName;
        GoTo("ide");
    }

    public void CloseIDEPanel()
    {
        _IDEPanel.gameObject.SetActive(false);
        _IDEController.OnClose();
        _OpenedApp = null;
        GoTo(_CurrentPanel);
    }

    public void ShowWebBrowserPanel()
    {
        GoTo("browser");
    }


    public void CloseWebBrowser()
    {
        _BrowserPanel.gameObject.SetActive(false);
        _OpenedApp = null;
        GoTo(_CurrentPanel);
    }

    public bool WasServerStarted()
    {
        return _TerminalController.IsServerRunning();
    }

    public bool WasExampleServerStarted()
    {
        return _TerminalController.IsWebExampleServerRunning();
    }


    public void ShowMissionPopup()
    {

        MissionConfig currentMission = GetCurrentMissionConfig();
        _MissionPanelController.ShowMissionPopup(currentMission.Description,currentMission.WhyStatement,currentMission.HowStatement,currentMission.ReferenceStatement,Difficulty,currentMission.HasReference);
    }

    public void HideMissionPopup()
    {
        _MissionPanelController.HideMissionPopup();
    }


    public void OnMissionCompelete(int[] whichMission)
    {

        //complete this mission only if it's a current mission and wasn't finished yet

        for (int i = 0; i < whichMission.Length; i++)
        {

            Debug.Log("awarding reward for mission: " + whichMission[i].ToString());
            if (whichMission[i] == CurrentMission )
            {

                MissionConfig currentMission = GetCurrentMissionConfig();

                //show reward
                _RewardPopupController.Show(currentMission.Reward.ToString());
                AwardXP(currentMission.Reward);


                _FinishedMissions.Add(CurrentMission);
                this.CurrentMission += 1;
                HideMissionPopup();

                if(this.CurrentMission>=this.Missions.Length)
                {
                    Debug.Log("breaking onMissionComplete()-no more missions.");
                    return;
                }

                //get new current mission and see if should be skipped
                bool foundActiveMission = false;
                while (!foundActiveMission)
                {
                    currentMission = GetCurrentMissionConfig();
                    if (!currentMission.IsActive)
                    {
                        //skip
                        this.CurrentMission += 1;//go to the next one

                    }
                    else
                    {
                        foundActiveMission = true;
                    }
                }
                Debug.Log("Active mission:" + currentMission.ID.ToString());

                if(currentMission.ID==14)
                {
                    ActivateBrowserButton();
                }


            }
        }
    }


    public void AwardXP(float xp)
    {
        _CurrentXPValue += xp;
        _NavigationController.UpdateXPValue(_CurrentXPValue.ToString());
    }


    public void NavigateToUrlInNewTab(string url)
    {
        _JSRunner.NavigateToURLInNewTab(url);
    }


    public void Login()
    {
        _Username = _UsernameTextField.text;
        if(_Username.Length==0)
        {
            _IntroUsernameLabel.color = ErrorColor;
            return;
        }

        _IntroPanel.gameObject.SetActive(false);
        AdjustScale();
        Screen.fullScreen = !Screen.fullScreen;
        _Username = _UsernameTextField.text;
        Debug.Log("Kept username for later reference:" + this._Username);

    }

    public void OnIntroUsernameBeingEntered()
    {
        _IntroUsernameLabel.color = MainThemeColor;
    }

    public void ShowOutro()
    {
        _OutroUsernameText.text = _Username + ",";

        _OutroPanel.gameObject.SetActive(true);


        OnMissionCompelete(new int[] { 24,25 });




        _OutroXPText.text = "YOUR XP: " + _CurrentXPValue.ToString();


    }



    public void GoToGithub()
    {
        _JSRunner.NavigateToURLInNewTab("https://github.com/msporna/LAVA-test-coverage");
    }

    public void GoToTwitter()
    {
        _JSRunner.NavigateToURLInNewTab("https://twitter.com/msporna");
    }

    public void OnProbesInjected()
    {
        _ProbesInjected = true;
        _InstrumentJSFile.gameObject.SetActive(true);
    }

    /// <summary>
    /// adjust scaleFactor of the canvas scaler based on user's screen resolution
    /// similar to responsive design in web dev.
    /// </summary>
    void AdjustScale()
    {
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        Resolution currentResolution = Screen.currentResolution;
        Debug.Log("current resolution is " + currentResolution.ToString());
        if((currentResolution.width>=800 && currentResolution.width<1025) && (currentResolution.height>=600 && currentResolution.height<768))
        {
            canvasScaler.scaleFactor = 0.55f;
        }
        else if ((currentResolution.width >= 1025 && currentResolution.width < 1281) && (currentResolution.height >= 720 && currentResolution.height < 1080))
        {
            canvasScaler.scaleFactor = 0.666f;
        }
        else if((currentResolution.width >= 1281 && currentResolution.width < 1920) && (currentResolution.height >= 768 && currentResolution.height <1080))
        {
            canvasScaler.scaleFactor = 0.71f;
        }
        else
        {
            canvasScaler.scaleFactor = 1;
        }

        Debug.Log("canvas scaler scale factor is now " + canvasScaler.scaleFactor.ToString());
    }

    public void ActivateLogoutButton()
    {
        _LogoutButton.gameObject.SetActive(true);
    }

    public void ActivateBrowserButton()
    {
        _BrowserButton.gameObject.SetActive(true);
    }

    public void DisableTopbarNavigation()
    {
        _Nav1.gameObject.SetActive(false);
        _Nav2.gameObject.SetActive(false);
        _Nav3.gameObject.SetActive(false);
    }

    public void EnableTopBarNavigation()
    {
        _Nav1.gameObject.SetActive(true);
        _Nav2.gameObject.SetActive(true);
        _Nav3.gameObject.SetActive(true);
    }

    public void SetDifficulty(int difficultyLevel)
    {
        this.Difficulty = difficultyLevel;
    }


    public void MakeLavaTestSessionButtonActive()
    {
        _BrowserController.MakeLavaDashboardTestSessionButtonActive();
    }



    MissionConfig GetCurrentMissionConfig()
    {
        Debug.Log("looking for mission details:" + this.CurrentMission.ToString());

        for (int i = 0; i < Missions.Length; i++)
        {
            if (Missions[i].ID == this.CurrentMission)
            {
                Debug.Log("returning mission " + Missions[i].ID);
                return Missions[i];
            }
        }


        throw new System.Exception("Mission " + this.CurrentMission.ToString() + " was not found");

    }

}
