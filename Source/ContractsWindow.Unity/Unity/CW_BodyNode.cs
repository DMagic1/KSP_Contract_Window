#region license
/*The MIT License (MIT)
CW_BodyNode - Controls the progress node body tree element

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
	public class CW_BodyNode : MonoBehaviour
	{
		[SerializeField]
		private TextHandler BodyTitle = null;
		[SerializeField]
		private GameObject StandardPrefab = null;
		[SerializeField]
		private Transform StandardTransform = null;
		[SerializeField]
		private Toggle BodyToggle = null;

		private List<CW_StandardNode> nodes = new List<CW_StandardNode>();
		private string bodyName;

		public string BodyName
		{
			get { return bodyName; }
		}

		public void setBodyType(string body, List<IStandardNode> nodes)
		{
			if (nodes.Count <= 0)
				return;

			bodyName = body;

			if (BodyTitle != null)
				BodyTitle.OnTextUpdate.Invoke(body);

			CreateBodyNodes(nodes);

			if (StandardTransform != null)
				StandardTransform.gameObject.SetActive(false);
		}

		public void Refresh()
		{
			if (BodyToggle != null && BodyToggle.isOn)
				NodesOn(true);
		}

		public void NodesOn(bool isOn)
		{
			if (CW_ProgressPanel.Instance == null)
				return;

			if (CW_ProgressPanel.Instance.PanelInterface == null)
				return;

			if (!CW_ProgressPanel.Instance.PanelInterface.AnyBodyNode(bodyName))
				return;

			if (StandardTransform == null)
				return;

			StandardTransform.gameObject.SetActive(isOn);

			if (!isOn)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = nodes[i];

				if (node == null)
					continue;

				if (node.StandardInterface == null)
					continue;

				node.UpdateText();

				node.gameObject.SetActive(isOn && node.StandardInterface.IsComplete);
			}
		}

		private void CreateBodyNodes(IList<IStandardNode> nodes)
		{
			if (CW_ProgressPanel.Instance == null)
				return;

			if (CW_ProgressPanel.Instance.PanelInterface == null)
				return;

			if (nodes == null)
				return;

			if (StandardTransform == null || StandardPrefab == null)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				IStandardNode node = nodes[i];

				if (node == null)
					continue;

				CreateBodyNode(node);
			}
		}

		private void CreateBodyNode(IStandardNode node)
		{
			GameObject obj = Instantiate(StandardPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(StandardTransform, false);

			CW_StandardNode nodeObject = obj.GetComponent<CW_StandardNode>();

			if (nodeObject == null)
				return;

			nodeObject.setNode(node);

			nodes.Add(nodeObject);
		}

		public void AddBodyNode(IStandardNode node)
		{
			if (node == null)
				return;

			CreateBodyNode(node);
		}
	}
}
