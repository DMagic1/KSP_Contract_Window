#region license
/*The MIT License (MIT)
CW_SortMenu - Controls the sort menu popup

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

using ContractsWindow.Unity.Interfaces;
using UnityEngine;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_SortMenu : CW_Popup
	{
		private IMissionSection missionInterface;

		public void setSort(IMissionSection m, PopupFade popupFade)
		{
			if (m == null)
				return;

            OnPopupFade = popupFade;

			missionInterface = m;

			FadeIn();
		}

		public void SortDifficulty()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetSort(1);

            OnPopupFade.Invoke(this);
		}

		public void SortExpiration()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetSort(2);

            OnPopupFade.Invoke(this);
        }

		public void SortAccept()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetSort(3);

            OnPopupFade.Invoke(this);
        }

		public void SortReward()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetSort(4);

            OnPopupFade.Invoke(this);
        }

		public void SortType()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetSort(5);

            OnPopupFade.Invoke(this);
        }

		public void SortPlanet()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetSort(6);

            OnPopupFade.Invoke(this);
        }

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
