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
using UnityEngine.EventSystems;

public class TerminalOptionController : MonoBehaviour,IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public TerminalController TerminalControllerInstance;
    [HideInInspector]
    public int OptionConfigID;
    [HideInInspector]
    public int RelatedSlotID; //when option is dropped to some slot, save the slot's id

    RectTransform _Slot;

    Transform _OriginalParent;


    private void Start()
    {
        _OriginalParent = transform.parent;
    }

    public void OnDrag(PointerEventData eventData)
    {

        TerminalControllerInstance.IsDraggingOption = true;
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (TerminalControllerInstance.IsDraggedOptionOverSlot)
        {

            transform.position = TerminalControllerInstance.OptionsDestTransform.position;
            transform.parent = TerminalControllerInstance.OptionsDestTransform;
            RelatedSlotID = TerminalControllerInstance.ActiveSlotID;

        }
        else
        {

           // transform.localPosition = Vector3.zero;
            RelatedSlotID = -1;
            transform.position = _OriginalParent.position;
            transform.parent = _OriginalParent;

        }


        TerminalControllerInstance.IsDraggingOption = false;

    }
}
