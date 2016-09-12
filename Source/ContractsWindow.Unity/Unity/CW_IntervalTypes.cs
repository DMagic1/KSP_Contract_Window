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
		private Text IntervalType = null;
		[SerializeField]
		private GameObject IntervalPrefab = null;
		[SerializeField]
		private Transform IntervalTransform = null;

		private IIntervalType intervalInterface;
		private List<CW_IntervalNode> nodes = new List<CW_IntervalNode>();

		public void setIntervalType(IIntervalType type)
		{
			if (type == null)
				return;

			intervalInterface = type;

			if (IntervalType != null)
				IntervalType.text = type.IntervalName;

			CreateIntervalNodes(type.GetNodes);
		}

		public void NodesOn(bool isOn)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				CW_IntervalNode node = nodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn);
			}
		}

		private void CreateIntervalNodes(IList<IIntervalNode> nodes)
		{
			if (nodes == null)
				return;

			if (IntervalTransform == null || IntervalPrefab == null)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				IIntervalNode node = nodes[i];

				if (node == null)
					continue;

				CreateIntervalNode(node);
			}
		}

		private void CreateIntervalNode(IIntervalNode node)
		{
			if (intervalInterface == null)
				return;

			GameObject obj = Instantiate(IntervalPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(IntervalTransform, false);

			CW_IntervalNode nodeObject = obj.GetComponent<CW_IntervalNode>();

			if (nodeObject == null)
				return;

			nodeObject.setNode(node);

			nodes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		public void AddIntervalNode(IIntervalNode node)
		{
			if (node == null)
				return;

			CreateIntervalNode(node);
		}
	}
}
