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

public class ExampleAppController : MonoBehaviour {

    public Sprite State0;
    public Sprite State1;
    public Sprite State2;
    public Sprite State3;

    Transform _Function1Button;
    Transform _Function2Button;
    Transform _AboutLinkButton;
    Image _Container;
    BrowserController _BrowserController;

    int _Button1ClickCount;
    int _Button2ClickCount;
    bool _IsLocked = false;
    bool _WasLoaded = false;
    int _CurrentState; //0-fresh,1-button 1 clicked, 2 - button 2 clicked, 3- about clicked
    bool _ReferencesSet;

    void SetupReferences()
    {
        if(_ReferencesSet)
        {
            return;
        }


        _Container = transform.GetChild(0).GetComponent<Image>();
        _Function1Button = transform.GetChild(1);
        _Function2Button = transform.GetChild(2);
        _AboutLinkButton = transform.GetChild(3);
        _BrowserController = transform.parent.parent.GetComponent<BrowserController>();
        _WasLoaded = true;

        _ReferencesSet = true;

    }

    public void OnExampleAppOpened()
    {
        SetupReferences();

        ShowContent();

    }

    public void OnFunction1ButtonClicked()
    {
        _CurrentState = 1;
        ShowContent();

        _Button1ClickCount += 1;
        LockContent();
        _BrowserController.ShowDashboardRefreshButtonOnNextTabEntry = true;

    }

    public void OnFunction2ButtonClicked()
    {
        _CurrentState = 2;
        ShowContent();

        _Button2ClickCount += 1;
        LockContent();
        _BrowserController.ShowDashboardRefreshButtonOnNextTabEntry = true;
    }

    public void OnAboutLinkButtonClicked()
    {
        _CurrentState = 3;
        ShowContent();
        LockContent();
        _BrowserController.ShowDashboardRefreshButtonOnNextTabEntry = true;
    }

    public void OnTabActivated()
    {
        SetupReferences();
        ShowContent(justItemState:true);
    }

    public void ShowContent(bool justItemState=false)
    {

        switch(_CurrentState)
        {
            case 0:
                {
                    _AboutLinkButton.gameObject.SetActive(false);
                    _Function2Button.gameObject.SetActive(false);

                    if(_BrowserController.IsDashboardReadyForGatheringStats())
                    {
                        _Function1Button.gameObject.SetActive(true);
                    }
                    else
                    {
                        _Function1Button.gameObject.SetActive(false);
                    }

                    if(!justItemState)
                    {
                        _Container.sprite = State0;
                    }

                    _BrowserController.DisableExampleTabRefresh();

                    break;
                }
            case 1:
                {
                    switch(_Button1ClickCount)
                    {
                        case 0:
                            {
                                _AboutLinkButton.gameObject.SetActive(false);
                                _Function2Button.gameObject.SetActive(true);
                                _Function1Button.gameObject.SetActive(false);

                                _BrowserController.SetDashboardCurrentScreen(17);

                                _CurrentState = 2;
                                break;
                            }
                        case 1:
                            {
                                _AboutLinkButton.gameObject.SetActive(false);
                                _Function2Button.gameObject.SetActive(false);
                                _Function1Button.gameObject.SetActive(true);
                                _BrowserController.SetDashboardCurrentScreen(20);

                                _CurrentState = 1;
                                break;
                            }
                        case 2:
                            {
                                _AboutLinkButton.gameObject.SetActive(true);
                                _Function2Button.gameObject.SetActive(false);
                                _Function1Button.gameObject.SetActive(false);
                                _BrowserController.SetDashboardCurrentScreen(21);

                                _CurrentState = 1;
                                break;
                            }
                    }

                    if (!justItemState)
                    {
                        _Container.sprite = State1;
                    }



                    break;
                }
            case 2:
                {
                    switch (_Button2ClickCount)
                    {

                        case 0:
                            {
                                _AboutLinkButton.gameObject.SetActive(false);
                                _Function2Button.gameObject.SetActive(true);
                                _Function1Button.gameObject.SetActive(false);
                                _BrowserController.SetDashboardCurrentScreen(18);

                                _CurrentState = 2;
                                break;
                            }
                        case 1:
                            {
                                _AboutLinkButton.gameObject.SetActive(false);
                                _Function2Button.gameObject.SetActive(false);
                                _Function1Button.gameObject.SetActive(true);
                                _BrowserController.SetDashboardCurrentScreen(19);

                                _CurrentState =1;
                                break;
                            }

                    }

                    if (!justItemState)
                    {
                        _Container.sprite = State2;
                    }

                    break;
                }
            case 3:
                {
                    if (!justItemState)
                    {
                        _Container.sprite = State3;
                    }
                    _BrowserController.SetDashboardCurrentScreen(22);

                    break;
                }
        }
    }



    /// <summary>
    /// when some button is clicked on the example page
    /// disable all controls
    /// to force user to go to dashboard
    /// refresh
    /// and check stats coming in from the app to dashboard
    /// then after refreshing
    /// ui is unlocked
    /// and locked in dashboard
    /// </summary>
    void LockContent()
    {
        _Function1Button.gameObject.SetActive(false);
        _Function2Button.gameObject.SetActive(false);
        _AboutLinkButton.gameObject.SetActive(false);

        _IsLocked = true;

    }
}
