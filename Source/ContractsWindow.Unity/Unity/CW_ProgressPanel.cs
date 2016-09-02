using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_ProgressPanel : MonoBehaviour
	{
		[SerializeField]
		private GameObject IntervalPrefab = null;
		[SerializeField]
		private Transform IntervalTransform = null;
		[SerializeField]
		private GameObject StandardPrefab = null;
		[SerializeField]
		private Transform StandardTransform = null;
		[SerializeField]
		private Transform POITransform = null;
		[SerializeField]
		private GameObject BodyPrefab = null;
		[SerializeField]
		private Transform BodyTransform = null;

		private IProgressPanel panelInterface;
		private List<CW_IntervalTypes> intervalTypes = new List<CW_IntervalTypes>();
		private List<CW_StandardNode> poiNodes = new List<CW_StandardNode>();
		private List<CW_StandardNode> standardNodes = new List<CW_StandardNode>();
		private List<CW_BodyNode> bodyNodes = new List<CW_BodyNode>();

		public void setPanel(IProgressPanel panel)
		{
			if (panel == null)
				return;

			panelInterface = panel;

			CreateIntervalTypes(panel.GetIntervalTypes());

			CreateStandardNodes(panel.GetStandardNodes());

			CreatePOINodes(panel.GetPOINodes());

			CreateBodies(panel.GetBodies());
		}

		public void SetProgressVisible(bool isOn)
		{
			if (panelInterface == null)
				return;

			panelInterface.IsVisible = isOn;

			gameObject.SetActive(isOn);
		}

		public void ToggleIntervals(bool isOn)
		{
			for (int i = intervalTypes.Count - 1; i >= 0; i--)
			{
				CW_IntervalTypes node = intervalTypes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn);
			}
		}

		public void TogglePOIs(bool isOn)
		{
			for (int i = poiNodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = poiNodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn);
			}
		}

		public void ToggleStandards(bool isOn)
		{
			for (int i = standardNodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = standardNodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn);
			}
		}

		public void ToggleBodies(bool isOn)
		{
			for (int i = bodyNodes.Count - 1; i >= 0; i--)
			{
				CW_BodyNode node = bodyNodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn);
			}
		}

		private void CreateIntervalTypes(IList<IIntervalType> types)
		{
			if (types == null)
				return;

			if (IntervalPrefab == null || IntervalTransform == null)
				return;

			for (int i = types.Count - 1; i >= 0; i--)
			{
				IIntervalType type = types[i];

				if (type == null)
					continue;

				CreateIntervalType(type);
			}
		}

		private void CreateIntervalType(IIntervalType type)
		{
			GameObject obj = Instantiate(IntervalPrefab);

			if (obj == null)
				return;

			panelInterface.ProcessStyle(obj);

			obj.transform.SetParent(IntervalTransform, false);

			CW_IntervalTypes nodeObject = obj.GetComponent<CW_IntervalTypes>();

			if (nodeObject == null)
				return;

			nodeObject.setIntervalType(type);

			intervalTypes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		public void AddIntervalType(IIntervalType type)
		{
			if (type == null)
				return;

			CreateIntervalType(type);
		}

		private void CreatePOINodes(IList<IStandardNode> nodes)
		{
			if (nodes == null)
				return;

			if (StandardPrefab == null || POITransform == null)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				IStandardNode node = nodes[i];

				if (node == null)
					continue;

				CreatePOINode(node);
			}
		}

		private void CreatePOINode(IStandardNode node)
		{
			GameObject obj = Instantiate(StandardPrefab);

			if (obj == null)
				return;

			panelInterface.ProcessStyle(obj);

			obj.transform.SetParent(POITransform, false);

			CW_StandardNode nodeObject = obj.GetComponent<CW_StandardNode>();

			if (nodeObject == null)
				return;

			nodeObject.setNode(node);

			poiNodes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		public void AddPOINode(IStandardNode node)
		{
			if (node == null)
				return;

			CreatePOINode(node);
		}

		private void CreateStandardNodes(IList<IStandardNode> nodes)
		{
			if (nodes == null)
				return;

			if (StandardPrefab == null || StandardTransform == null)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				IStandardNode node = nodes[i];

				if (node == null)
					continue;

				CreateStandardNode(node);
			}
		}

		private void CreateStandardNode(IStandardNode node)
		{
			GameObject obj = Instantiate(StandardPrefab);

			if (obj == null)
				return;

			panelInterface.ProcessStyle(obj);

			obj.transform.SetParent(StandardTransform, false);

			CW_StandardNode nodeObject = obj.GetComponent<CW_StandardNode>();

			if (nodeObject == null)
				return;

			nodeObject.setNode(node);

			standardNodes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		public void AddStandardNode(IStandardNode node)
		{
			if (node == null)
				return;

			CreateStandardNode(node);
		}

		private void CreateBodies(IList<IBodyNodes> nodes)
		{
			if (nodes == null)
				return;

			if (BodyPrefab == null || BodyTransform == null)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				IBodyNodes node = nodes[i];

				if (node == null)
					continue;

				CreateBody(node);
			}
		}

		private void CreateBody(IBodyNodes node)
		{
			GameObject obj = Instantiate(BodyPrefab);

			if (obj == null)
				return;

			panelInterface.ProcessStyle(obj);

			obj.transform.SetParent(BodyTransform, false);

			CW_BodyNode nodeObject = obj.GetComponent<CW_BodyNode>();

			if (nodeObject == null)
				return;

			nodeObject.setBodyType(node);

			bodyNodes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		public void AddBody(IBodyNodes node)
		{
			if (node == null)
				return;

			CreateBody(node);
		}

	}
}
