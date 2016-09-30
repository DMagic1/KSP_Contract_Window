#region license
/*The MIT License (MIT)
CW_MissionSelectObject - Controls the mission selection button element

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
	public class CW_MissionSelectObject : MonoBehaviour
	{
		[SerializeField]
		private TextHandler MissionTitle = null;
		[SerializeField]
		private TextHandler MissionNumber = null;

		private IMissionSection missionInterface;
		private CW_MissionSelect parent;

		public void setMission(IMissionSection mission, CW_MissionSelect p)
		{
			if (mission == null)
				return;

			if (MissionTitle == null || MissionNumber == null)
				return;

			if (p == null)
				return;

			parent = p;

			missionInterface = mission;

			MissionTitle.OnTextUpdate.Invoke(mission.MissionTitle);

			MissionNumber.OnTextUpdate.Invoke(mission.ContractNumber);
		}

		public void SetMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetMission();

			if (parent == null)
				return;

			parent.ToggleToContracts();

			parent.DestroyPanel();
		}

	}
}
