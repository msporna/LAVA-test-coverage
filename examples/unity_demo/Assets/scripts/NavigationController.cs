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

public class NavigationController : MonoBehaviour {

    public Color ActiveButtonColor; //06CFF3
    public Color GenericRegularColor;

    Transform _TopNavButton1;
    Transform _TopNavButton2;
    Transform _TopNavButton3;
    Transform _TopNavAppButton;
    Transform _ProfileButton;
    Text _XPText;

    MainController _MainController;


    // Use this for initialization
    void Start () {
        _TopNavButton1 = transform.GetChild(2).GetChild(0);
        _TopNavButton2 = transform.GetChild(2).GetChild(1);
        _TopNavButton3 = transform.GetChild(2).GetChild(2);
        _TopNavAppButton = transform.GetChild(2).GetChild(3);
        _ProfileButton = transform.GetChild(3);
        _XPText = _ProfileButton.GetChild(0).GetComponent<Text>();
    }

    public void SetMainController(MainController mc)
    {
        this._MainController = mc;
    }

    public void UpdateXPValue(string xpToShow)
    {
        _XPText.text = xpToShow;
    }

	public void UpdateCurrentLocation(string location)
    {
        _MainController.HideMissionPopup();

        switch (location)
        {
            case "desktop":
                _TopNavButton1.gameObject.SetActive(true);
                _TopNavButton2.gameObject.SetActive(false);
                _TopNavButton3.gameObject.SetActive(false);

                _TopNavButton1.GetComponent<Image>().color = ActiveButtonColor;



                break;
            case "lava_folder":
                _TopNavButton1.gameObject.SetActive(true);
                _TopNavButton2.gameObject.SetActive(true);
                _TopNavButton3.gameObject.SetActive(false);


                _TopNavButton1.GetComponent<Image>().color = GenericRegularColor;
                _TopNavButton2.GetComponent<Image>().color = ActiveButtonColor;

                _MainController.OnMissionCompelete(new int[] { 0 });

                break;
            case "client_folder":
                _TopNavButton1.gameObject.SetActive(true);
                _TopNavButton3.gameObject.SetActive(true);
                _TopNavButton3.gameObject.SetActive(true);


                _TopNavButton1.GetComponent<Image>().color = GenericRegularColor;
                _TopNavButton2.GetComponent<Image>().color = GenericRegularColor;
                _TopNavButton3.GetComponent<Image>().color = ActiveButtonColor;

                _TopNavButton3.GetChild(0).GetComponent<Text>().text = "client";

                _MainController.OnMissionCompelete(new int[] { 1 });

                break;
            case "server_folder":
                _TopNavButton1.gameObject.SetActive(true);
                _TopNavButton3.gameObject.SetActive(true);
                _TopNavButton3.gameObject.SetActive(true);


                _TopNavButton1.GetComponent<Image>().color = GenericRegularColor;
                _TopNavButton2.GetComponent<Image>().color = GenericRegularColor;
                _TopNavButton3.GetComponent<Image>().color = ActiveButtonColor;

                _TopNavButton3.GetChild(0).GetComponent<Text>().text = "server";

                _MainController.OnMissionCompelete(new int[] { 3 });

                break;
            case "examples_folder":
                _TopNavButton1.gameObject.SetActive(true);
                _TopNavButton3.gameObject.SetActive(true);
                _TopNavButton3.gameObject.SetActive(true);


                _TopNavButton1.GetComponent<Image>().color = GenericRegularColor;
                _TopNavButton2.GetComponent<Image>().color = GenericRegularColor;
                _TopNavButton3.GetComponent<Image>().color = ActiveButtonColor;

                _TopNavButton3.GetChild(0).GetComponent<Text>().text = "examples";

                _MainController.OnMissionCompelete(new int[] { 6 });

                break;
        }
    }

    /// <summary>
    /// allows user to go back to ancestor folder
    /// </summary>
    /// <param name="navIndex"></param>
    public void OnItemClick(int navIndex)
    {
        switch(navIndex)
        {
            case 0:
                {
                    _MainController.GoTo("desktop");
                    break;
                }
            case 1:
                {
                    _MainController.GoTo("lava_folder");
                    break;
                }

        }
    }

    public void OpenTerminalApp()
    {
        _TopNavAppButton.gameObject.SetActive(true);
        _TopNavAppButton.GetChild(0).GetComponent<Text>().text = "Terminal";
        _MainController.ShowTerminalPanel();
        _MainController.DisableTopbarNavigation();

        _MainController.OnMissionCompelete(new int[] { 4 });

    }

    public void OpenIDEApp(string whichFile)
    {
        _TopNavAppButton.gameObject.SetActive(true);
        _TopNavAppButton.GetChild(0).GetComponent<Text>().text = "Text Editor";
        _MainController.ShowIDEPanel(whichFile);
        _MainController.DisableTopbarNavigation();
    }

    public void OpenBrowserApp()
    {
        _TopNavAppButton.gameObject.SetActive(true);
        _TopNavAppButton.GetChild(0).GetComponent<Text>().text = "Browser";
        _MainController.ShowWebBrowserPanel();
    }

    public void OnCloseAppButtonClick()
    {
        switch(_MainController.GetOpenedApp())
        {
            case "browser":
                {

                    _MainController.CloseWebBrowser();
                    break;
                }
            case "ide":
                {
                    _MainController.CloseIDEPanel();
                    break;
                }
            case "terminal":
                {
                    _MainController.CloseTerminalApp();

                    break;
                }
        }

        //hide the nav button
        _TopNavAppButton.gameObject.SetActive(false);
    }
}
