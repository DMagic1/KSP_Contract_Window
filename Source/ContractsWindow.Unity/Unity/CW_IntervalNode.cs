#region license
/*The MIT License (MIT)
CW_IntervalNode - Controls the interval progress node element

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
	public class CW_IntervalNode : MonoBehaviour
	{
		[SerializeField]
		private TextHandler Title = null;
		[SerializeField]
		private TextHandler Reward = null;

		private IIntervalNode intervalInterface;
		private int interval;

		public void setNode(IIntervalNode node, int i)
		{
			if (node == null)
				return;

			intervalInterface = node;

			interval = i;

			if (Title != null)
				Title.OnTextUpdate.Invoke(getTitle());

			if (Reward != null)
				Reward.OnTextUpdate.Invoke(node.RewardText(i));
		}

		public void UpdateText()
		{
			if (intervalInterface == null)
				return;

			if (Title != null)
				Title.OnTextUpdate.Invoke(getTitle());

			if (Reward != null)
				Reward.OnTextUpdate.Invoke(intervalInterface.RewardText(interval));
		}

		private string getTitle()
		{
			if (intervalInterface == null)
				return "";

			return string.Format("{0} Record {1}: {2}", intervalInterface.NodeTitle, interval, intervalInterface.NodeValue(interval));
		}
	}
}
