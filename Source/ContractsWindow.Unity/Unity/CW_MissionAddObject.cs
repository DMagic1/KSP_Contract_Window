#region license
/*The MIT License (MIT)
CW_MissionAddObject - Controls the contract mission object

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
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionAddObject : MonoBehaviour
	{
		[SerializeField]
		private TextHandler MissionTitle = null;
		[SerializeField]
		private TextHandler MissionNumber = null;
		[SerializeField]
		private Image Checkmark = null;
		[SerializeField]
		private Button XMark = null;

        public delegate void PopupFader();
        
        private PopupFader OnPopupFade;

        private IMissionSection missionInterface;
		private IContractSection contractInterface;

		public void setMission(IMissionSection mission, IContractSection contract, PopupFader popupFade)
		{
			if (mission == null || contract == null)
				return;

			if (MissionTitle == null || MissionNumber == null || Checkmark == null || XMark == null)
				return;

            OnPopupFade = popupFade;

            missionInterface = mission;

			contractInterface = contract;

			MissionTitle.OnTextUpdate.Invoke(mission.MissionTitle);

			MissionNumber.OnTextUpdate.Invoke(mission.ContractNumber);

			if (mission.MasterMission)
				XMark.gameObject.SetActive(false);

			if (!mission.ContractContained(contract.ID))
			{
				Checkmark.gameObject.SetActive(false);

				XMark.gameObject.SetActive(false);
			}
		}

		public void AddContract()
		{
			if (missionInterface == null)
				return;

			missionInterface.AddContract(contractInterface);

            OnPopupFade.Invoke();
        }

		public void RemoveContract()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveContract(contractInterface);

			if (Checkmark != null && XMark != null)
			{
				Checkmark.gameObject.SetActive(false);

				XMark.gameObject.SetActive(false);
			}

			if (MissionNumber != null)
				MissionNumber.OnTextUpdate.Invoke(missionInterface.ContractNumber);
		}


	}
}
