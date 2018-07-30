#region license
/*The MIT License (MIT)
CW_MissionCreate - Controls the contract mission creator popup

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_MissionCreate : CW_Popup
	{
		[SerializeField]
		private InputHandler MissionInput = null;
        
        public delegate void MissionCreate(string title, Guid id);
        
        private MissionCreate OnMissionCreate;

        private Guid contractID;

        private void Update()
		{
			if (inputLock)
			{
                if (MissionInput != null && !MissionInput.IsFocused)
                {
                    inputLock = false;
                    OnPopupInputLock.Invoke(false);
                }
			}
		}

        public void setPanel(Guid id, MissionCreate missionCreate, PopupFade popupFade, PopupInputLock popupInputLock)
        {
            contractID = id;

            OnMissionCreate = missionCreate;

            OnPopupFade = popupFade;
            OnPopupInputLock = popupInputLock;

            FadeIn();
        }

		public void OnInputClick(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData))
				return;

			if (((PointerEventData)eventData).button != PointerEventData.InputButton.Left)
				return;

            inputLock = true;
            OnPopupInputLock.Invoke(true);
        }

		public void CreateMission()
		{
			if (MissionInput == null)
				return;

			if (string.IsNullOrEmpty(MissionInput.Text))
				return;

            OnMissionCreate.Invoke(MissionInput.Text, contractID);
            
            inputLock = false;
            OnPopupInputLock.Invoke(false);

            OnPopupFade.Invoke(this);
        }
        
		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
