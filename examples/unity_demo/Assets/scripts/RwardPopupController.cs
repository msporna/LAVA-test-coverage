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

public class RwardPopupController : MonoBehaviour {

    [Tooltip("Expressed in value,not time")]
    public float MoveSpeed = 700;
    [Tooltip("Expressed in seconds")]
    public float TimeToDissolve = 1f;

    Transform _posPlaceholderOut;
    Transform _posPlaceholderIn;
    Text _RewardValueText;
    bool _IsShown;
    List<string> _Queue;
    bool _AnimationInProgress;
    AudioSource _AudioSource;

    public void Init(Transform positionPlaceholderOut,Transform positionPlaceholderIn)
    {
        _posPlaceholderIn = positionPlaceholderIn;
        _posPlaceholderOut = positionPlaceholderOut;
        _RewardValueText = transform.GetChild(4).GetComponent<Text>();
        _Queue = new List<string>();
        _AudioSource = gameObject.GetComponent<AudioSource>();
        iTween.Init(gameObject);

    }

    public void Show(string xpReward)
    {

        _Queue.Insert(0,xpReward);

        if(!_AnimationInProgress)
        {
            _RewardValueText.text = xpReward;

        }

        StartAnimation(xpReward);

    }

    void ShowFromQueue(string xpReward)
    {
        _RewardValueText.text = xpReward;
        StartAnimation(xpReward);
    }

    void StartAnimation(string xpReward)
    {
        _AnimationInProgress = true;
        iTween.MoveTo(gameObject, iTween.Hash(
           "x", _posPlaceholderIn.transform.position.x,
           "y", _posPlaceholderIn.transform.position.y,
           "speed", MoveSpeed,
           "name", "rewardPopupShow_"+ System.Guid.NewGuid().ToString(),
           "easetype", "easeOutBounce",
           "onComplete", "OnShown"));

    }

    public void OnShown()
    {
        _IsShown = true;
        _AudioSource.Play();
        iTween.ValueTo(gameObject, iTween.Hash("From", 0f, "to", 1f, "time", TimeToDissolve, "name", "rewardPopupDissolve", "onupdate", "", "oncomplete", "Hide"));
        _AnimationInProgress = false;
    }

    public void OnHidden()
    {
        try
        {
            _IsShown = false;

            _Queue.RemoveAt(_Queue.Count - 1); //last item is the oldest

            if (_Queue.Count > 0)
            {
                ShowFromQueue(_Queue[_Queue.Count - 1]); //show next item in queue, the oldest one -going from the oldest to most recent
            }

        }
        catch (ArgumentOutOfRangeException e)
        {
            Debug.Log("no reward queued.");
        }

    }


    public void Hide()
    {

        iTween.MoveTo(gameObject, iTween.Hash(
            "x", _posPlaceholderOut.transform.position.x,
            "y", _posPlaceholderOut.transform.position.y,
            "speed", MoveSpeed,
            "name", "rewardPopupHide",
            "easetype", "linear",
            "onComplete", "OnHidden"));
    }
}
