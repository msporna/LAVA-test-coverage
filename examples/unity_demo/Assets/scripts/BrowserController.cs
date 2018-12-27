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

public class BrowserController : MonoBehaviour {

    public MainController MainControllerInstance;
    public Color TabHighlightedColor;
    public Color TabRegularColor;
    [HideInInspector]
    public bool ShowDashboardRefreshButtonOnNextTabEntry;

    Text _UrlTextField;
    Transform _404Panel;
    Transform _welcomePanel;
    Transform _dashboardPanel;
    Transform _demoPanel;
    Transform _DashboardTab;
    Transform _ExampleTab;
    Transform _DashboardTabRefreshButton;
    Transform _ExampleTabRefreshButton;

    string _CurrentPage;
    int _CurrentTab;
    bool _ExamplePageCached;
    bool _DashboardRefreshEnabled;
    bool _DashboardPageCached;


    LavaDashboardController _DashboardController;
    ExampleAppController _ExampleAppController;


    bool _ReferencesSet;
    bool _FocusReceived = false;


    public void SetupReferences()
    {
        if(_ReferencesSet)
        {
            return;
        }

        _UrlTextField = transform.GetChild(1).GetChild(2).GetChild(1).GetComponent<Text>();
        _404Panel = transform.GetChild(0).GetChild(1);
        _welcomePanel = transform.GetChild(0).GetChild(0);
        _dashboardPanel = transform.GetChild(0).GetChild(2);
        _demoPanel = transform.GetChild(0).GetChild(3);
        _DashboardTab = transform.GetChild(1).GetChild(3);
        _ExampleTab = transform.GetChild(1).GetChild(4);
        _DashboardTabRefreshButton = _DashboardTab.GetChild(1);
        _ExampleTabRefreshButton = _ExampleTab.GetChild(1);

        _DashboardController = _dashboardPanel.GetComponent<LavaDashboardController>();
        _ExampleAppController = _demoPanel.GetComponent<ExampleAppController>();

        _ReferencesSet = true;
    }

    public void OnOpen()
    {
        SetupReferences();


    }





    public void NavigateToUrl()
    {


        if (_UrlTextField.text == "http://localhost:8787")
        {

            //example web app
            if(_ExamplePageCached)
            {
                return;
            }

            NavigateToExample();


        }
        else if (_UrlTextField.text == "http://localhost:5000")
        {



            if(_DashboardPageCached)
            {
                return;
            }

            //dashboard
            NavigateToDashboard();
        }
        else
        {
            _404Panel.gameObject.SetActive(true);
        }
    }

    public void NavigateToDashboard()
    {


        if (MainControllerInstance.WasServerStarted())
        {
            _DashboardTab.gameObject.SetActive(true);

            MakeDashboardTabActive();

            _404Panel.gameObject.SetActive(false);
            _welcomePanel.gameObject.SetActive(false);
            _demoPanel.gameObject.SetActive(false);
            _CurrentPage = "dashboard";

            _UrlTextField.text = "http://localhost:5000/dashboard";


            _dashboardPanel.gameObject.SetActive(true);

            _DashboardController.OnDashboardPageLoaded();
            _DashboardPageCached = true;

            DisableDashboardTabRefresh();

            MainControllerInstance.OnMissionCompelete(new int[]{14});


        }
        else
        {
            _DashboardPageCached = false;
            _404Panel.gameObject.SetActive(true);
        }



    }

    public void ShowDashboard()
    {
        _404Panel.gameObject.SetActive(false);
        _welcomePanel.gameObject.SetActive(false);
        _demoPanel.gameObject.SetActive(false);
        if (_DashboardPageCached)
        {

            _dashboardPanel.gameObject.SetActive(true);
            _CurrentPage = "dashboard";

        }
        else
        {
            _404Panel.gameObject.SetActive(true);
        }
    }

    public void ShowExample()
    {
        _404Panel.gameObject.SetActive(false);
        _welcomePanel.gameObject.SetActive(false);
        _dashboardPanel.gameObject.SetActive(false);

        if (_ExamplePageCached)
        {

            _CurrentPage = "demo_page";
            _demoPanel.gameObject.SetActive(true);

        }
       else
        {
            _404Panel.gameObject.SetActive(true);
        }
    }

    public void NavigateToExample()
    {


        //verify that demo server is running:
        if (MainControllerInstance.WasExampleServerStarted())
        {

            _ExampleTab.gameObject.SetActive(true);

            MAkeExampleTabActive();

            _404Panel.gameObject.SetActive(false);
            _welcomePanel.gameObject.SetActive(false);
            _dashboardPanel.gameObject.SetActive(false);
            _CurrentPage = "demo_page";

            _demoPanel.gameObject.SetActive(true);
            _ExamplePageCached = true;
            _ExampleAppController.OnExampleAppOpened();

            MainControllerInstance.OnMissionCompelete(new int[] { 21 });




        }
        else
        {
            _ExamplePageCached = false;
            _404Panel.gameObject.SetActive(true);
        }
    }

    public void OnRefreshButtonClicked()
    {
        MainControllerInstance.HideMissionPopup();

        switch (_CurrentTab)
        {
            case 0:
                {
                    NavigateToDashboard();

                    break;
                }
            case 1:
                {
                    NavigateToExample();
                    break;
                }
        }
    }


    public void OnTabButtonClicked(int whichTab)
    {
        MainControllerInstance.HideMissionPopup();

        if(whichTab==0)
        {
            MakeDashboardTabActive();
            _UrlTextField.text = "http://localhost:5000/dashboard";
        }
        else
        {
            if(_DashboardRefreshEnabled)
            {
                return; //don't allow to go to example tab if dashboard refresh was not used
            }

            MAkeExampleTabActive();
            _UrlTextField.text = "http://localhost:8787";
        }
    }


    void MakeDashboardTabActive()
    {
        ShowDashboard();
        _DashboardTab.GetComponent<Image>().color = TabHighlightedColor;
        _ExampleTab.GetComponent<Image>().color = TabRegularColor;
        _CurrentTab = 0;


        if(ShowDashboardRefreshButtonOnNextTabEntry)
        {
            EnableDashboardTabRefresh();
            ShowDashboardRefreshButtonOnNextTabEntry = false;
        }

    }

    void MAkeExampleTabActive()
    {
        ShowExample();
        _DashboardTab.GetComponent<Image>().color = TabRegularColor;
        _ExampleTab.GetComponent<Image>().color = TabHighlightedColor;
        _CurrentTab = 1;
        _ExampleAppController.OnTabActivated();



    }

    public void DisableDashboardTabRefresh()
    {
        _DashboardTabRefreshButton.gameObject.SetActive(false);
        _DashboardRefreshEnabled = false;
    }

    public void DisableExampleTabRefresh()
    {
        _ExampleTabRefreshButton.gameObject.SetActive(false);

    }

    public void EnableDashboardTabRefresh()
    {
        if(_DashboardController.IsDashboardReadyForGatheringStats)
        {
            _DashboardTabRefreshButton.gameObject.SetActive(true);
            _DashboardRefreshEnabled = true;
        }

    }



    public void EnableExampleTabRefresh()
    {
        _ExampleTabRefreshButton.gameObject.SetActive(true);
    }



    public void UnlockDashboard()
    {
        _DashboardController.Unlock();
    }



    public void SetDashboardCurrentScreen(int screen)
    {
        _DashboardController.SetCurrentScreen(screen);
    }


    public bool IsDashboardReadyForGatheringStats()
    {
        return _DashboardController.IsDashboardReadyForGatheringStats;
    }

    public void CloseExampleTab()
    {
        _ExampleTab.gameObject.SetActive(false);
    }

    public void MakeLavaDashboardTestSessionButtonActive()
    {
        _DashboardController.MakeTestSessionButtonActive();
    }
}
