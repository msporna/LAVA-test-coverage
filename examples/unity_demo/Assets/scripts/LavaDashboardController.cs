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


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LavaDashboardController : MonoBehaviour {

    public Sprite Dashboard1;
    public Sprite Dashboard2;
    public Sprite Dashboard3;
    public Sprite Dashbaord4;
    public Sprite ModulesPopup1;
    public Sprite ModulesPopup2;
    public Sprite ModulesPopup3;
    public Sprite ModulesPopup4;
    public Sprite ModulesPopup5;
    public Sprite ModulesPopup6;
    public Sprite ModulesPopup7;
    public Sprite ModulesPopup8;
    public Sprite ModulesPopup9;
    public Sprite ModulesPopup10;
    public Sprite NewSessionPopup1;
    public Sprite NewSessionPopup2;
    public Sprite NewSessionPopup3;
    public Sprite NewSessionPopup4;
    public Sprite Report0;
    public Sprite Report1;
    public Sprite Report2;
    public Sprite Report3;
    public Sprite Report4;
    public Sprite Report5;
    public Sprite Report6;

    [HideInInspector]
    public int CurrentScreen = 0;
    [HideInInspector]
    public bool IsDashboardReadyForGatheringStats=false;


    Image _Image;
    Transform _ModulesButton;
    Transform _CloseModulesButton;
    Transform _NewModuleAlert;
    Transform _NewModuleAlertTextBoxHtmlModule;
    Transform _NewModuleAlertTextBoxJSModule;
    Transform _NewTestSessionButton;
    Transform _NewTestSessionStartButton;
    Transform _NewTestSessionCancelButton;
    Transform _NewTestSessionNameTextField;
    Transform _SessionNameImage;
    Transform _ModulesCheckboxMain;
    Transform _ModulesCheckboxAbout;
    Transform _ModulesCheckboxIndex;
    Transform _ModulesAssignButton;
    Transform _ModulesNewModuleButton;
    Text _SessionNameLabel;
    Transform _ModuleHtmlCheckboxButton;
    Transform _ModuleJSCheckboxButton;
    Transform _SessionButton;
    Button _SessionButtonCtrl;
    Transform _NewSessionCheckboxModule1;
    Transform _NewSessionCheckboxModule2;
    Transform _ReportContainer;
    Transform _ReportScrollbar;
    Transform _GoBackToDashboardButton;
    Image _ReportImage;
    Transform _ReportStartSessionLabelContainer;
    Transform _SessionStartDateContainerDashboard;
    Scrollbar _ScrollbarController;
    bool _ReferencesSet = false;
    BrowserController _BrowserController;


    string _NewSessionName;
    string _NewModuleName;

    int _WhichModuleCreating; //0 or 1
    bool _IndexSourceFileChecked;
    bool _AboutSourceFileChecked;
    bool _MainSourceFileChecked;
    bool _HtmlModuleChecked;
    bool _JsModuleChecked;
    bool _HtmlModuleSupplied;
    bool _JsModuleSupplied;
    bool _HtmlModuleCreated;
    bool _JsModuleCreated;
    bool _ModulesSet;
    bool _SessionCreated;
    int _ReportStage; //0-fresh,1-after gather
    bool _ReportShown;
    int _NumberOfRefreshClicks;
    string _SessionStartDate;
    Color _ReportStartDateLabelOriginalColor;
    Color _ReportStartDateLabelTextOriginalColor;
    Color _ReportGoBackOriginalColor;
    bool _SessionPageShown = false;
    bool _IsLocked = false;
    bool _WasLoaded = false;


    void SetReferences()
    {
        if(_ReferencesSet)
        {
            return;
        }

        _BrowserController = transform.parent.parent.GetComponent<BrowserController>();
        _Image = transform.GetChild(0).GetComponent<Image>();
        _ModulesButton = transform.GetChild(1);
        _NewModuleAlert = transform.GetChild(2);
        _NewModuleAlertTextBoxHtmlModule = _NewModuleAlert.GetChild(2);
        _NewModuleAlertTextBoxJSModule = _NewModuleAlert.GetChild(3);
        _CloseModulesButton = transform.GetChild(3);
        _NewTestSessionButton = transform.GetChild(4);

        _NewTestSessionNameTextField = transform.GetChild(5);
        _NewTestSessionStartButton = transform.GetChild(6);
        _NewTestSessionCancelButton = transform.GetChild(7);

        _SessionNameImage = transform.GetChild(8);
        _SessionButton = transform.GetChild(9);
        _SessionButtonCtrl = _SessionButton.GetComponent<Button>();
        _SessionNameLabel = transform.GetChild(8).GetChild(0).GetComponent<Text>();

        _ModulesCheckboxIndex = transform.GetChild(11);
        _ModulesCheckboxAbout = transform.GetChild(10);
        _ModulesCheckboxMain = transform.GetChild(12);
        _ModulesAssignButton = transform.GetChild(13);
        _ModulesNewModuleButton = transform.GetChild(14);

        _ModuleHtmlCheckboxButton = transform.GetChild(15);
        _ModuleJSCheckboxButton = transform.GetChild(16);

        _NewSessionCheckboxModule1 = transform.GetChild(17);
        _NewSessionCheckboxModule2 = transform.GetChild(18);
        _ReportContainer = transform.GetChild(19);
        _ReportImage = _ReportContainer.GetChild(0).GetComponent<Image>();
        _ReportScrollbar = transform.GetChild(20);
        _ScrollbarController = _ReportScrollbar.GetComponent<Scrollbar>();
        _GoBackToDashboardButton = transform.GetChild(21);
        _ReportGoBackOriginalColor = _GoBackToDashboardButton.GetComponent<Image>().color;
        _ReportStartSessionLabelContainer = transform.GetChild(22);
        _ReportStartDateLabelOriginalColor = _ReportStartSessionLabelContainer.GetComponent<Image>().color;
        _ReportStartDateLabelTextOriginalColor = _ReportStartSessionLabelContainer.GetChild(0).GetComponent<Text>().color;
        _SessionStartDateContainerDashboard = transform.GetChild(23);

        _ReferencesSet = true;

    }

    public void OnDashboardPageLoaded()
    {
        this.SetReferences();
        SetCurrentScreen();


        _WasLoaded = true;
    }

    public void OnModulesButtonClick()
    {
        if(_ModulesSet)
        {
            CurrentScreen = 12;
        }
        else
        {
            CurrentScreen = 3;
        }

        SetCurrentScreen();



    }

    public void OnCloseModulesButtonClick()
    {
        if(_SessionCreated)
        {
            CurrentScreen = 2;
        }
        else
        {
            CurrentScreen = 1;
        }

        SetCurrentScreen();
    }

    public void OnNewTestSessionButtonClick()
    {
        CurrentScreen = 13;
        SetCurrentScreen();

    }

    public void OnCheckbox1Checked()
    {
        _AboutSourceFileChecked = true;
        CurrentScreen = 6;
        SetCurrentScreen();
    }

    public void OnCheckbox2Checked()
    {
        _IndexSourceFileChecked = true;
        CurrentScreen = 7;
        SetCurrentScreen();
    }

    public void OnCheckbox3Checked()
    {
        CurrentScreen = 10;
        SetCurrentScreen();
        _MainSourceFileChecked = true;
    }

    public void OnModuleAssignButtonClicked()
    {
        if(_HtmlModuleChecked && _AboutSourceFileChecked && _IndexSourceFileChecked && !_HtmlModuleSupplied)
        {
            _HtmlModuleSupplied = true;
            CurrentScreen = 9;
            SetCurrentScreen();
        }
        else if(_JsModuleChecked && _MainSourceFileChecked && !_JsModuleSupplied)
        {
            _JsModuleSupplied = true;
            CurrentScreen = 12;
            SetCurrentScreen();
        }

    }

    public void OnModuleNewButtonClicked()
    {
        _NewModuleAlert.gameObject.SetActive(true);

        if(!_HtmlModuleCreated && !_JsModuleCreated)
        {
            _NewModuleAlertTextBoxHtmlModule.gameObject.SetActive(true);
            _NewModuleAlertTextBoxJSModule.gameObject.SetActive(false);

        }
        else if(_HtmlModuleCreated && !_JsModuleCreated)
        {
            _NewModuleAlertTextBoxJSModule.gameObject.SetActive(true);
            _NewModuleAlertTextBoxHtmlModule.gameObject.SetActive(false);
        }

        _ModulesCheckboxIndex.gameObject.SetActive(false);
        _ModulesCheckboxAbout.gameObject.SetActive(false);
        _ModulesCheckboxMain.gameObject.SetActive(false);
        _ModulesAssignButton.gameObject.SetActive(false);
        _ModuleJSCheckboxButton.gameObject.SetActive(false);
        _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
        _ModulesNewModuleButton.gameObject.SetActive(false);
        _CloseModulesButton.gameObject.SetActive(false);



    }

    public void OnNewModulePopupOKButtonClicked()
    {
        _NewModuleAlert.gameObject.SetActive(false);

        _ModulesCheckboxIndex.gameObject.SetActive(true);
        _ModulesCheckboxAbout.gameObject.SetActive(true);
        _ModulesCheckboxMain.gameObject.SetActive(true);
        _ModulesAssignButton.gameObject.SetActive(true);
        _ModulesNewModuleButton.gameObject.SetActive(true);
        _CloseModulesButton.gameObject.SetActive(true);

        if (_WhichModuleCreating == 0)
        {
            CurrentScreen = 4;
            SetCurrentScreen();

            _WhichModuleCreating += 1;
            _HtmlModuleCreated = true;
        }
        else
        {
            CurrentScreen = 5;
            SetCurrentScreen();
            _JsModuleCreated = true;
        }




    }

    public void OnNewModulePopupCancelButtonClicked()
    {
        _NewModuleAlert.gameObject.SetActive(false);

        _ModulesCheckboxIndex.gameObject.SetActive(true);
        _ModulesCheckboxAbout.gameObject.SetActive(true);
        _ModulesCheckboxMain.gameObject.SetActive(true);
        _ModulesAssignButton.gameObject.SetActive(true);
        _ModulesNewModuleButton.gameObject.SetActive(true);
        _CloseModulesButton.gameObject.SetActive(true);
    }

    public void OnNewSessionModuleCheckbox1Checked()
    {
        CurrentScreen = 14;
        SetCurrentScreen();
    }

    public void OnNewSessionModuleCheckbox2Checked()
    {
        CurrentScreen = 15;
        SetCurrentScreen();
    }

    public void OnModuleHtmlModuleChecked()
    {
        _HtmlModuleChecked = true;
        CurrentScreen = 8;
        SetCurrentScreen();
    }

    public void OnModuleJSModuleChecked()
    {
        _JsModuleChecked = true;
        CurrentScreen = 11;
        SetCurrentScreen();
    }

    public void OnNewTestSessionStartButtonClick()
    {
        string sessionName = _NewTestSessionNameTextField.GetComponent<InputField>().text;
        if(sessionName.Length>0)
        {
            CurrentScreen = 2;
            SetCurrentScreen();
            _NewSessionName = sessionName;
            _SessionNameLabel.text = sessionName;

            _SessionCreated = true;
            _SessionStartDate =  String.Format("{0:yyyy-MM-dd HH-mm-ss}", DateTime.Now);
            _ReportStartSessionLabelContainer.GetChild(0).GetComponent<Text>().text = _SessionStartDate;
            _SessionStartDateContainerDashboard.GetChild(0).GetComponent<Text>().text = _SessionStartDate;
        }


    }

    public void OnSessionButtonClick()
    {
        _Image.gameObject.SetActive(false);
        _ReportContainer.gameObject.SetActive(true);
        _ReportScrollbar.gameObject.SetActive(true);

        CurrentScreen = 16;
        SetCurrentScreen();

        _SessionPageShown = true;

    }

    public void OnGoBackToDashboardButtonClick()
    {
        _Image.gameObject.SetActive(true);
        _ReportContainer.gameObject.SetActive(false);
        _ReportScrollbar.gameObject.SetActive(false);

        //switch(_ReportStage)
        //{
        //    case 0:
        //        {
        //            CurrentScreen = 2;
        //            break;
        //        }
        //    case 1:
        //        {
        //            CurrentScreen = 23;
        //            break;
        //        }
        //}

        SetCurrentScreen();
        _SessionPageShown = false;
    }

    public void OnNewTestSessionCancelButtonClick()
    {
        CurrentScreen = 1;
        SetCurrentScreen();
    }

    public void SetCurrentScreen()
    {
        switch(CurrentScreen)
        {

            case 0:
                {
                    _Image.sprite = this.Dashboard1;
                    _ModulesButton.gameObject.SetActive(true);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);

                    _BrowserController.DisableDashboardTabRefresh();
                    break;
                }
            case 1:
                {
                    _Image.sprite = this.Dashboard2;
                    _ModulesButton.gameObject.SetActive(true);
                    _NewTestSessionButton.gameObject.SetActive(true);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    break;
                }
            case 2:
                {
                    //after session was created
                    _Image.sprite = this.Dashboard3;
                    _ModulesButton.gameObject.SetActive(true);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(true);
                    _SessionButtonCtrl.interactable = false;
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(true);
                    _SessionNameLabel.gameObject.SetActive(true);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(true);

                    _BrowserController.MainControllerInstance.OnMissionCompelete(new int[] { 18 });
                    break;
                }
            case 3:
                {
                    _Image.sprite = this.ModulesPopup1;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(true);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);


                    _BrowserController.MainControllerInstance.OnMissionCompelete(new int[] { 15 });
                    break;
                }
            case 4:
                {
                    //html module created

                    _Image.sprite = this.ModulesPopup2;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);

                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(true);


                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    break;
                }
            case 5:
                {
                    //js module created

                    _Image.sprite = this.ModulesPopup3;

                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(true);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);

                    _BrowserController.MainControllerInstance.OnMissionCompelete(new int[] { 16 });
                    break;
                }
            case 6:
                {
                    //about checked

                    _Image.sprite = this.ModulesPopup4;

                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(true);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    break;
                }
            case 7:
                {
                    //index and about checked

                    _Image.sprite = this.ModulesPopup5;

                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(true);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    break;
                }
            case 8:
                {
                    //html module checked + index and about

                    _Image.sprite = this.ModulesPopup6;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(true);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    break;
                }
            case 9:
                {
                    //html module supplied

                    _Image.sprite = this.ModulesPopup7;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(true);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);


                    break;
                }
            case 10:
                {
                    //main js checked

                    _Image.sprite = this.ModulesPopup8;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(true);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    break;
                }
            case 11:
                {
                    //js module checked

                    _Image.sprite = this.ModulesPopup9;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(true);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    break;
                }
            case 12:
                {
                    //js module supplied
                    _Image.sprite = this.ModulesPopup10;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(true);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _SessionNameLabel.text = _NewSessionName;
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);

                    _ModulesSet = true;
                    _BrowserController.MainControllerInstance.OnMissionCompelete(new int[] { 17 });
                    break;
                }
            case 13:
                {
                    _Image.sprite = this.NewSessionPopup1;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(true);
                    _NewTestSessionCancelButton.gameObject.SetActive(true);
                    _NewTestSessionStartButton.gameObject.SetActive(true);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(true);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    break;
                }
            case 14:
                {

                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(true);
                    _NewTestSessionCancelButton.gameObject.SetActive(true);
                    _NewTestSessionStartButton.gameObject.SetActive(true);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(true);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _Image.sprite = this.NewSessionPopup3;
                    break;
                }
            case 15:
                {

                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(true);
                    _NewTestSessionCancelButton.gameObject.SetActive(true);
                    _NewTestSessionStartButton.gameObject.SetActive(true);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _Image.sprite = this.NewSessionPopup4;


                    break;
                }
            case 16:
                {
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);

                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportImage.sprite = this.Report0;


                    _ReportStartSessionLabelContainer.gameObject.SetActive(true);
                    ResetScrollbar();
                    IsDashboardReadyForGatheringStats = true;

                    _BrowserController.MainControllerInstance.OnMissionCompelete(new int[] { 20 });

                    break;

                }
            case 17:
                {
                    //example-function1 button clicked x1

                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);

                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportImage.sprite = this.Report1;



                    _ReportStartSessionLabelContainer.gameObject.SetActive(true);
                    ResetScrollbar();
                    break;

                }
            case 18:
                {
                    //example-function2 button clicked x1
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);

                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportImage.sprite = this.Report2;


                    _ReportStartSessionLabelContainer.gameObject.SetActive(true);
                    ResetScrollbar();
                    break;

                }
            case 19:
                {
                    //example-function2 button clicked x2
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);

                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportImage.sprite = this.Report3;


                    _ReportStartSessionLabelContainer.gameObject.SetActive(true);
                    ResetScrollbar();
                    break;

                }
            case 20:
                {
                    //example-function1 button clicked x2
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);

                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportImage.sprite = this.Report4;


                    _ReportStartSessionLabelContainer.gameObject.SetActive(true);
                    ResetScrollbar();
                    break;

                }
                case 21:
                {
                    //example-function1 button clicked x3
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);

                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportImage.sprite = this.Report5;

                    _ReportStartSessionLabelContainer.gameObject.SetActive(true);
                    ResetScrollbar();
                    break;

                }
                case 22:
                {
                    //example-about clicked
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);

                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);

                    _SessionNameImage.gameObject.SetActive(false);
                    _SessionNameLabel.gameObject.SetActive(false);
                    _SessionButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(true);

                    _SessionStartDateContainerDashboard.gameObject.SetActive(false);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportImage.sprite = this.Report6;
                    CurrentScreen += 1; //increase to show dashboard if back button is clicked


                    _ReportStartSessionLabelContainer.gameObject.SetActive(true);
                    ResetScrollbar();



                    _BrowserController.CloseExampleTab();
                    _BrowserController.MainControllerInstance.OnMissionCompelete(new int[] { 22 });


                    break;

                }
            case 23:
                {
                    _Image.sprite = Dashbaord4;
                    _ModulesButton.gameObject.SetActive(false);
                    _NewTestSessionButton.gameObject.SetActive(false);
                    _CloseModulesButton.gameObject.SetActive(false);
                    _SessionNameImage.gameObject.SetActive(true);
                    _SessionNameLabel.gameObject.SetActive(true);
                    _SessionButton.gameObject.SetActive(false);
                    _NewTestSessionCancelButton.gameObject.SetActive(false);
                    _NewTestSessionStartButton.gameObject.SetActive(false);
                    _ModulesCheckboxIndex.gameObject.SetActive(false);
                    _ModulesCheckboxAbout.gameObject.SetActive(false);
                    _ModulesCheckboxMain.gameObject.SetActive(false);
                    _ModulesAssignButton.gameObject.SetActive(false);
                    _ModulesNewModuleButton.gameObject.SetActive(false);
                    _ModuleJSCheckboxButton.gameObject.SetActive(false);
                    _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
                    _NewSessionCheckboxModule1.gameObject.SetActive(false);
                    _NewSessionCheckboxModule2.gameObject.SetActive(false);
                    _GoBackToDashboardButton.gameObject.SetActive(false);
                    _SessionStartDateContainerDashboard.gameObject.SetActive(true);
                    _NewTestSessionNameTextField.gameObject.SetActive(false);
                    _ReportStartSessionLabelContainer.gameObject.SetActive(false);

                    _BrowserController.MainControllerInstance.ActivateLogoutButton();

                    _BrowserController.MainControllerInstance.OnMissionCompelete(new int[] { 23 });

                    break;
                }


        }
    }

    public void ResetScrollbar()
    {
        _ScrollbarController.value = 1;
        HandleScrollbar();
    }

    public void HandleScrollbar()
    {
       // Debug.Log("ON SCROLL>VALUE:" + _ScrollbarController.value.ToString());
        if (_ScrollbarController.value >0.98)
        {
            _GoBackToDashboardButton.GetComponent<Image>().color = new Color(_ReportGoBackOriginalColor.r, _ReportGoBackOriginalColor.g, _ReportGoBackOriginalColor.b, 45);
            _GoBackToDashboardButton.GetComponent<Button>().interactable = true;
            _ReportStartSessionLabelContainer.GetChild(0).GetComponent<Text>().color = new Color(_ReportStartDateLabelTextOriginalColor.r, _ReportStartDateLabelTextOriginalColor.g, _ReportStartDateLabelTextOriginalColor.b, 255);
            _ReportStartSessionLabelContainer.GetComponent<Image>().color = new Color(_ReportStartDateLabelOriginalColor.r, _ReportStartDateLabelOriginalColor.g, _ReportStartDateLabelOriginalColor.b, 255);
            // _GoBackToDashboardButton.gameObject.SetActive(true);
            // _ReportStartSessionLabelContainer.gameObject.SetActive(true);
        }
        else
        {
            //disabling causes some exceptions thus simply not showing...
            // _GoBackToDashboardButton.gameObject.SetActive(false);
            // _ReportStartSessionLabelContainer.gameObject.SetActive(false);

            _GoBackToDashboardButton.GetComponent<Image>().color = new Color(_ReportGoBackOriginalColor.r, _ReportGoBackOriginalColor.g, _ReportGoBackOriginalColor.b, 0);
            _GoBackToDashboardButton.GetComponent<Button>().interactable = false;
            _ReportStartSessionLabelContainer.GetChild(0).GetComponent<Text>().color = new Color(_ReportStartDateLabelTextOriginalColor.r, _ReportStartDateLabelTextOriginalColor.g, _ReportStartDateLabelTextOriginalColor.b, 0);
            _ReportStartSessionLabelContainer.GetComponent<Image>().color = new Color(_ReportStartDateLabelOriginalColor.r,_ReportStartDateLabelOriginalColor.g,_ReportStartDateLabelOriginalColor.b,0);
        }

    }



    public void OnReportPageScrollbarScrolled()
    {

        HandleScrollbar();
    }

    public void Unlock()
    {
        if(!_WasLoaded)
        {
            return;
        }

        SetCurrentScreen();
        _IsLocked = false;

    }

    public void MakeTestSessionButtonActive()
    {
        _SessionButton.GetComponent<Button>().interactable = true;
    }

    public void LockContent()
    {

        _ModulesButton.gameObject.SetActive(false);
        _NewTestSessionButton.gameObject.SetActive(true);
        _CloseModulesButton.gameObject.SetActive(false);
        _SessionNameImage.gameObject.SetActive(false);
        _SessionNameLabel.gameObject.SetActive(false);
        _SessionButton.gameObject.SetActive(false);
        _NewTestSessionCancelButton.gameObject.SetActive(false);
        _NewTestSessionStartButton.gameObject.SetActive(false);
        _SessionButton.gameObject.SetActive(false);
        _ModulesCheckboxIndex.gameObject.SetActive(false);
        _ModulesCheckboxAbout.gameObject.SetActive(false);
        _ModulesCheckboxMain.gameObject.SetActive(false);
        _ModulesAssignButton.gameObject.SetActive(false);
        _ModulesNewModuleButton.gameObject.SetActive(false);
        _ModuleJSCheckboxButton.gameObject.SetActive(false);
        _ModuleHtmlCheckboxButton.gameObject.SetActive(false);
        _NewSessionCheckboxModule1.gameObject.SetActive(false);
        _NewSessionCheckboxModule2.gameObject.SetActive(false);
        _GoBackToDashboardButton.gameObject.SetActive(false);
        _ReportStartSessionLabelContainer.gameObject.SetActive(false);
        _SessionStartDateContainerDashboard.gameObject.SetActive(false);
        _NewTestSessionNameTextField.gameObject.SetActive(false);


        _IsLocked = true;


    }

    public void SetCurrentScreen(int screenNumber)
    {
        CurrentScreen = screenNumber;
    }
}
