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

		private IBodyNodes bodyInterface;
		private List<CW_StandardNode> nodes = new List<CW_StandardNode>();

		public void setBodyType(IBodyNodes body)
		{
			if (body == null)
				return;

			bodyInterface = body;

			if (BodyTitle != null)
				BodyTitle.text = body.BodyName;

			CreateBodyNodes(body.GetNodes);
		}

		public void NodesOn(bool isOn)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = nodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn);
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
			if (bodyInterface == null)
				return;

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

		public void AddIntervalBodyNode(IStandardNode node)
		{
			if (node == null)
				return;

			CreateBodyNode(node);
		}
	}
}
