#region license
/*The MIT License (MIT)
CW_IntervalTypes - Controls the progress node interval type UI element

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
	public class CW_IntervalTypes : MonoBehaviour
	{
		[SerializeField]
		private TextHandler IntervalType = null;
		[SerializeField]
		private GameObject IntervalPrefab = null;
		[SerializeField]
		private Transform IntervalTransform = null;
		[SerializeField]
		private Toggle IntervalToggle = null;

		private List<CW_IntervalNode> nodes = new List<CW_IntervalNode>();
		private IIntervalNode intervalInterface;

		public IIntervalNode IntervalInterface
		{
			get { return intervalInterface; }
		}

		public void setIntervalType(IIntervalNode node)
		{
			if (node == null)
				return;

			intervalInterface = node;
			
			if (IntervalType != null)
				IntervalType.OnTextUpdate.Invoke(node.NodeTitle);

			CreateIntervalNodes(node);

			if (IntervalTransform != null)
				IntervalTransform.gameObject.SetActive(false);
		}

		public void Refresh()
		{
			if (IntervalToggle != null && IntervalToggle.isOn)
				NodesOn(true);
		}

		public void NodesOn(bool isOn)
		{
			if (intervalInterface == null)
				return;

			if (!intervalInterface.IsReached)
				return;

			if (IntervalTransform == null)
				return;

			IntervalTransform.gameObject.SetActive(isOn);

			if (!isOn)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				CW_IntervalNode node = nodes[i];

				if (node == null)
					continue;

				node.UpdateText();

				node.gameObject.SetActive(isOn && intervalInterface.NodeInterval > i + 1);
			}
		}

		private void CreateIntervalNodes(IIntervalNode node)
		{
			if (intervalInterface == null)
				return;

			if (node == null)
				return;

			for (int i = 1; i <= node.Intervals; i++)
			{
				CreateIntervalNode(node, i);
			}
		}

		private void CreateIntervalNode(IIntervalNode node, int i)
		{			
			if (IntervalTransform == null || IntervalPrefab == null)
				return;

			GameObject obj = Instantiate(IntervalPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(IntervalTransform, false);

			CW_IntervalNode nodeObject = obj.GetComponent<CW_IntervalNode>();

			if (nodeObject == null)
				return;

			nodeObject.setNode(node, i);

			nodes.Add(nodeObject);
		}
	}
}
