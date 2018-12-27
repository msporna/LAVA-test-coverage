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
using UnityEngine.UI;

public class TerminalOptionSlotController : MonoBehaviour
{
    public Color HighlightColor;
    public TerminalController TerminalControllerInstance;
    

    RectTransform _Slot;
    Image _SlotImage;
    Color _OriginalColor;

    private void Start()
    {
        _Slot = GetComponent<RectTransform>();
        _SlotImage = GetComponent<Image>();
        _OriginalColor = _SlotImage.color;
        
    }

    /// <summary>
    /// this controller is in 6 slots so happens in 6 different objects
    /// </summary>
    private void Update()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(_Slot, Input.mousePosition) && TerminalControllerInstance.IsDraggingOption)
        {
            _SlotImage.color = HighlightColor;
            TerminalControllerInstance.IsDraggedOptionOverSlot = true;
            TerminalControllerInstance.OptionsDestTransform = transform;
            TerminalControllerInstance.ActiveSlotID = gameObject.GetInstanceID();

            
        }
        else
        {
            if(TerminalControllerInstance.IsDraggedOptionOverSlot && TerminalControllerInstance.ActiveSlotID!=gameObject.GetInstanceID())
            {
                //if currently user is holding option over some slot, but other slot's update happens to no be the one that user is holding option above, then return
                //do not alter 'isDraggedOptionOverSlot'
                //this property can be changed to false in this case, when slot that set it to true has no longer option held above it (became inactive)
                //other slot cannot alter it to false
                
                return;
            }

            _SlotImage.color = _OriginalColor;
            TerminalControllerInstance.IsDraggedOptionOverSlot = false;
            
        }
    }
}
