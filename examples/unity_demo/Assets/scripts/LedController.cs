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

public class LedController : MonoBehaviour {

    public float MoveSpeed = 200;


    Transform _Led1;
    Transform _Led2;
    Transform _Led1LeftPlaceholder;
    Transform _Led1RightPlaceholder;
    Transform _Led2LeftPlaceholder;
    Transform _Led2RightPlaceholder;


    // Use this for initialization
    void Start () {
        _Led1 = transform.GetChild(0);
        _Led1LeftPlaceholder = transform.GetChild(1);
        _Led1RightPlaceholder = transform.GetChild(2);

        _Led2 = transform.GetChild(5);
        _Led2LeftPlaceholder = transform.GetChild(3);
        _Led2RightPlaceholder = transform.GetChild(4);

        iTween.Init(_Led1.gameObject);
        Dance();
	}


    public void Dance()
    {
        iTween.MoveTo(_Led1.gameObject, iTween.Hash(
          "x", _Led1RightPlaceholder.position.x,
          "y", _Led1RightPlaceholder.position.y,
          "speed", MoveSpeed,
          "name", "ledDanceBottom",
          "easetype", "linear",
          "looptype", "pingPong"));


        iTween.MoveTo(_Led2.gameObject, iTween.Hash(
          "x", _Led2LeftPlaceholder.position.x,
          "y", _Led2LeftPlaceholder.position.y,
          "speed", MoveSpeed,
          "name", "ledDanceTop",
          "easetype", "linear",
          "looptype", "pingPong"));
    }


}
