#region license
/*The MIT License (MIT)
IntervalNodeUI - Storage class for information on interval progress nodes

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
using ProgressParser;
using KSP.Localization;

namespace ContractsWindow.PanelInterfaces
{
	public class IntervalNodeUI : IIntervalNode
	{
		private progressInterval node;

		public IntervalNodeUI(progressInterval n)
		{
			if (n == null)
				return;

			node = n;
		}

		public bool IsReached
		{
			get
			{
				if (node == null)
					return false;

				return node.IsReached;
			}
		}

		public int Intervals
		{
			get
			{
				if (node == null)
					return 0;

				return node.TotalIntervals;
			}
		}

		public int NodeInterval
		{
			get
			{
				if (node == null)
					return 0;

				return node.Interval;
			}
		}

		public string NodeTitle
		{
			get
			{
				if (node == null)
					return "";

				return node.Title;
			}
		}

		public string NodeDescriptor(int index)
		{
				if (node == null)
					return "";

				return Localizer.Format(node.Descriptor, string.Format("{0}{1}", node.getRecord(index), node.Units));
		}

		private string coloredText(string s, string sprite, string color)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			return string.Format("<color={0}>{1}{2}</color>  ", color, sprite, s);
		}

		public string RewardText(int i)
		{
			if (node == null)
				return "";

			return string.Format("{0}{1}{2}", coloredText(node.getFundsString(i), "<sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1>", "#69D84FFF"), coloredText(node.getScienceString(i), "<sprite=\"CurrencySpriteAsset\" name=\"Science\" tint=1>", "#02D8E9FF"), coloredText(node.getRepString(i), "<sprite=\"CurrencySpriteAsset\" name=\"Reputation\" tint=1>", "#C9B003FF"));
		}

	}
}
