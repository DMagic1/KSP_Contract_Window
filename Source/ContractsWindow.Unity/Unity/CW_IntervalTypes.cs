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
				IntervalType.text = node.NodeTitle;

			CreateIntervalNodes(node);
		}

		public void NodesOn(bool isOn)
		{
			if (intervalInterface == null)
				return;

			if (!intervalInterface.IsReached)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				CW_IntervalNode node = nodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn && intervalInterface.NodeInterval >= i - 1);
			}
		}

		private void CreateIntervalNodes(IIntervalNode node)
		{
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

			nodeObject.gameObject.SetActive(false);
		}
	}
}
