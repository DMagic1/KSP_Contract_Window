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
		private Text BodyTitle = null;
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
				BodyTitle.text = body;

			CreateBodyNodes(nodes);

			if (CW_ProgressPanel.Instance == null)
				return;

			if (CW_ProgressPanel.Instance.PanelInterface == null)
				return;

			if (BodyToggle != null)
				BodyToggle.gameObject.SetActive(CW_ProgressPanel.Instance.PanelInterface.AnyBodyNode(body));
		}

		private void Update()
		{
			if (CW_ProgressPanel.Instance == null)
				return;

			if (CW_ProgressPanel.Instance.PanelInterface == null)
				return;

			if (!CW_ProgressPanel.Instance.PanelInterface.IsVisible)
				return;

			if (BodyToggle != null)
				BodyToggle.gameObject.SetActive(CW_ProgressPanel.Instance.PanelInterface.AnyBodyNode(bodyName));
		}

		public void NodesOn(bool isOn)
		{
			if (CW_ProgressPanel.Instance == null)
				return;

			if (CW_ProgressPanel.Instance.PanelInterface == null)
				return;

			if (!CW_ProgressPanel.Instance.PanelInterface.AnyBodyNode(bodyName))
				return;
			
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = nodes[i];

				if (node == null)
					continue;

				if (node.StandardInterface == null)
					continue;

				node.gameObject.SetActive(isOn && node.StandardInterface.IsComplete);
			}
		}

		private void CreateBodyNodes(IList<IStandardNode> nodes)
		{
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

			nodeObject.gameObject.SetActive(false);
		}

		public void AddBodyNode(IStandardNode node)
		{
			if (node == null)
				return;

			CreateBodyNode(node);
		}
	}
}
