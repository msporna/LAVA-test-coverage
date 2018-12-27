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

public class MissionPanelController : MonoBehaviour {

    [Tooltip("Expressed in value,not time")]
    public float MoveSpeed = 100f;


    public string WhyButtonInactiveText = "Why&How?";
    public string WhyButtonActiveText = "Hide";


    bool _ShowReference;
    string _MissionDescription;
    string _MissionWhyStatement;
    string _MissionHowStatement;
    string _MissionReference; //to doc
    Transform _AppearedPlaceholder;
    Text _MissionTextInstance;
    Transform _WhyButtonInstance;
    Text _WhyButtonText;
    Transform _WhyPanelInstanceEasy;
    Transform _WhyPanelInstanceHard;
    Text _WhyTextInstance;
    Text _HowTextInstance;
    Transform _WhyLabelInstance;
    Transform _HowLabelInstance;
    Text _ReferenceTextInstance;
    Transform _ReferenceObject;
    Transform _ReferenceLabelInstance;
    Transform _WhyTextBackground;
    Transform _HowTextBackground;

    MainController _MainControllerInstance;
    bool _AlreadySetup;
    bool _IsShown;
    bool _WhyContentIsShown;
    bool _IsDifficultyHard;

    public void GetReferences()
    {
        if(_AlreadySetup)
        {
            return;
        }


        _MissionTextInstance = transform.GetChild(0).GetComponent<Text>();
        _WhyButtonInstance = transform.GetChild(1).GetChild(0);
        _WhyButtonText = _WhyButtonInstance.GetChild(0).GetComponent<Text>();
        _WhyPanelInstanceEasy = transform.GetChild(2);
        _WhyPanelInstanceHard = transform.GetChild(3);
        _WhyLabelInstance = transform.GetChild(6);
        _HowLabelInstance = transform.GetChild(9);
        _WhyTextInstance = transform.GetChild(5).GetComponent<Text>();
        _HowTextInstance = transform.GetChild(8).GetComponent<Text>();
        _ReferenceObject = transform.GetChild(11);
        _ReferenceTextInstance = _ReferenceObject.GetChild(0).GetComponent<Text>();
        _ReferenceLabelInstance = transform.GetChild(10);
        _WhyTextBackground = transform.GetChild(4);
        _HowTextBackground = transform.GetChild(7);

        iTween.Init(gameObject);
        _AlreadySetup = true;
    }

    public void Init(Transform missionPanelPlaceholder,MainController mc)
    {
        _MainControllerInstance = mc;
        _AppearedPlaceholder = missionPanelPlaceholder;
        GetReferences();
        HideMissionPopup();
    }

    public void HideMissionPopup()
    {


        iTween.MoveTo(gameObject, iTween.Hash(
           "x", _AppearedPlaceholder.transform.position.x+1000,
           "y", _AppearedPlaceholder.transform.position.y,
           "speed", MoveSpeed,
           "name", "missionPopupHide",
           "easetype", "linear",
           "onComplete", "OnPopupHidden"));

        HideWhyContent();


    }

    public void OnPopupHidden()
    {

        _IsShown = false;
    }

    public void ShowMissionPopup(string currentMissionDescription,string currentMissionWhyStatement,string currentMissionHowStatement,string currentMissionReferenceStatement,int difficultyLevel,bool hasRef)
    {
        _ShowReference = hasRef;

        if(_IsShown)
        {
            HideMissionPopup();
            return;
        }

        if(difficultyLevel==1)
        {
            _IsDifficultyHard = true;
        }

        _MissionDescription = currentMissionDescription;
        _MissionWhyStatement = currentMissionWhyStatement;
        _MissionHowStatement = currentMissionHowStatement;
        _MissionReference = currentMissionReferenceStatement;

        _MissionTextInstance.text = _MissionDescription;
        _WhyTextInstance.text = _MissionWhyStatement;
        _HowTextInstance.text = _MissionHowStatement;
        //_ReferenceTextInstance.text = _MissionReference; //hardcoded



        iTween.MoveTo(gameObject, iTween.Hash(
            "x", _AppearedPlaceholder.transform.position.x,
            "y", _AppearedPlaceholder.transform.position.y,
            "speed", MoveSpeed,
            "name", "missionPopupShow",
            "easetype", "easeOutBounce",
            "onComplete", "OnPopupShown"));
    }

    public void OnPopupShown()
    {

        _IsShown = true;
    }

    public void ShowWhyContent()
    {

        if(_WhyContentIsShown)
        {

            HideWhyContent();
            return;
        }



        if(_IsDifficultyHard)
        {
            _WhyPanelInstanceHard.gameObject.SetActive(true);
            _WhyTextInstance.gameObject.SetActive(true);
            _HowTextInstance.gameObject.SetActive(false);
            _WhyLabelInstance.gameObject.SetActive(true);
            _HowLabelInstance.gameObject.SetActive(false);
            _WhyTextBackground.gameObject.SetActive(true);
            _HowTextBackground.gameObject.SetActive(false);
        }
        else
        {
            _WhyPanelInstanceEasy.gameObject.SetActive(true);
            _WhyTextInstance.gameObject.SetActive(true);
            _HowTextInstance.gameObject.SetActive(true);
            _WhyLabelInstance.gameObject.SetActive(true);
            _HowLabelInstance.gameObject.SetActive(true);
            _WhyTextBackground.gameObject.SetActive(true);
            _HowTextBackground.gameObject.SetActive(true);
        }

        if(_ShowReference)
        {
            _ReferenceObject.gameObject.SetActive(true);
            //_ReferenceLabelInstance.gameObject.SetActive(true);
        }


        _WhyButtonText.text = WhyButtonActiveText;
        _WhyContentIsShown = true;



    }

    public void HideWhyContent()
    {
        _WhyPanelInstanceEasy.gameObject.SetActive(false);
        _WhyPanelInstanceHard.gameObject.SetActive(false);
        _WhyTextInstance.gameObject.SetActive(false);
        _HowTextInstance.gameObject.SetActive(false);
        _WhyLabelInstance.gameObject.SetActive(false);
        _HowLabelInstance.gameObject.SetActive(false);
        _ReferenceObject.gameObject.SetActive(false);
       // _ReferenceLabelInstance.gameObject.SetActive(false);
        _WhyButtonText.text = WhyButtonInactiveText;
        _WhyContentIsShown = false;
        _WhyTextBackground.gameObject.SetActive(false);
        _HowTextBackground.gameObject.SetActive(false);
    }

    /// <summary>
    /// using javascript open a link in new tab
    /// it should lead to documentation/wiki related to this mission
    /// </summary>
    public void NavigateToReferenceDoc()
    {
        _MainControllerInstance.NavigateToUrlInNewTab(this._MissionReference);
    }
}
