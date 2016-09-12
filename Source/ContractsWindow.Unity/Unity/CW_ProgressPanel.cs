using System;
using System.Collections.Generic;
using System.Linq;
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
		[SerializeField]
		private Toggle IntervalToggle = null;
		[SerializeField]
		private Toggle POIToggle = null;
		[SerializeField]
		private Toggle StandardToggle = null;
		[SerializeField]
		private Toggle BodyToggle = null;

		private IProgressPanel panelInterface;
		private List<CW_IntervalTypes> intervalTypes = new List<CW_IntervalTypes>();
		private List<CW_StandardNode> poiNodes = new List<CW_StandardNode>();
		private List<CW_StandardNode> standardNodes = new List<CW_StandardNode>();
		private List<CW_BodyNode> bodyNodes = new List<CW_BodyNode>();

		private static CW_ProgressPanel instance;

		public static CW_ProgressPanel Instance
		{
			get { return instance; }
		}

		public IProgressPanel PanelInterface
		{
			get { return panelInterface; }
		}

		private void Awake()
		{
			instance = this;
		}
		
		public void setPanel(IProgressPanel panel)
		{
			if (panel == null)
				return;

			panelInterface = panel;

			CreateIntervalTypes(panel.GetIntervalNodes);

			CreateStandardNodes(panel.GetStandardNodes);

			CreatePOINodes(panel.GetPOINodes);

			CreateBodies(panel.GetBodies);

			if (IntervalToggle != null)
				IntervalToggle.gameObject.SetActive(panel.AnyInterval);

			if (StandardToggle != null)
				StandardToggle.gameObject.SetActive(panel.AnyStandard);

			if (POIToggle != null)
				POIToggle.gameObject.SetActive(panel.AnyPOI);

			if (BodyToggle != null)
				BodyToggle.gameObject.SetActive(panel.AnyBody);
		}

		private void Update()
		{
			if (panelInterface == null)
				return;

			if (!panelInterface.IsVisible)
				return;

			if (IntervalToggle != null)
				IntervalToggle.gameObject.SetActive(panelInterface.AnyInterval);

			if (StandardToggle != null)
				StandardToggle.gameObject.SetActive(panelInterface.AnyStandard);

			if (POIToggle != null)
				POIToggle.gameObject.SetActive(panelInterface.AnyPOI);

			if (BodyToggle != null)
				BodyToggle.gameObject.SetActive(panelInterface.AnyBody);
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

				if (node.IntervalInterface == null)
					continue;

				node.gameObject.SetActive(isOn && node.IntervalInterface.IsReached);
			}
		}

		public void TogglePOIs(bool isOn)
		{
			for (int i = poiNodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = poiNodes[i];

				if (node == null)
					continue;

				if (node.StandardInterface == null)
					continue;

				node.gameObject.SetActive(isOn && node.StandardInterface.IsComplete);
			}
		}

		public void ToggleStandards(bool isOn)
		{
			for (int i = standardNodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = standardNodes[i];

				if (node == null)
					continue;

				if (node.StandardInterface == null)
					continue;

				node.gameObject.SetActive(isOn && node.StandardInterface.IsComplete);
			}
		}

		public void ToggleBodies(bool isOn)
		{
			if (panelInterface == null)
				return;

			for (int i = bodyNodes.Count - 1; i >= 0; i--)
			{
				CW_BodyNode node = bodyNodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(isOn && panelInterface.AnyBodyNode(node.BodyName));
			}
		}

		private void CreateIntervalTypes(IList<IIntervalNode> nodes)
		{
			if (nodes.Count <= 0)
				return;

			if (IntervalPrefab == null || IntervalTransform == null)
				return;

			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				IIntervalNode n = nodes[i];

				CreateIntervalType(n);
			}
		}

		private void CreateIntervalType(IIntervalNode n)
		{
			GameObject obj = Instantiate(IntervalPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(IntervalTransform, false);

			CW_IntervalTypes nodeObject = obj.GetComponent<CW_IntervalTypes>();

			if (nodeObject == null)
				return;

			nodeObject.setIntervalType(n);

			intervalTypes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		//public void AddIntervalType(string t, List<IIntervalNode> n)
		//{
		//	if (n.Count <= 0)
		//		return;

		//	CreateIntervalType(t, n);
		//}

		//public void AddIntervalNode(string t, IIntervalNode n)
		//{
		//	if (n == null)
		//		return;

		//	for (int i = intervalTypes.Count - 1; i >= 0; i--)
		//	{
		//		CW_IntervalTypes type = intervalTypes[i];

		//		if (type == null)
		//			continue;

		//		if (type.Title != t)
		//			continue;

		//		type.AddIntervalNode(n);

		//		break;
		//	}
		//}

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

			obj.transform.SetParent(POITransform, false);

			CW_StandardNode nodeObject = obj.GetComponent<CW_StandardNode>();

			if (nodeObject == null)
				return;

			nodeObject.setNode(node);

			poiNodes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		//public void AddPOINode(IStandardNode node)
		//{
		//	if (node == null)
		//		return;

		//	CreatePOINode(node);
		//}

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

			obj.transform.SetParent(StandardTransform, false);

			CW_StandardNode nodeObject = obj.GetComponent<CW_StandardNode>();

			if (nodeObject == null)
				return;

			nodeObject.setNode(node);

			standardNodes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		//public void AddStandardNode(IStandardNode node)
		//{
		//	if (node == null)
		//		return;

		//	CreateStandardNode(node);
		//}

		private void CreateBodies(Dictionary<string, List<IStandardNode>> bodies)
		{
			if (bodies.Count <= 0)
				return;

			if (BodyPrefab == null || BodyTransform == null)
				return;

			for (int i = bodies.Count - 1; i >= 0; i--)
			{
				string body = bodies.ElementAt(i).Key;

				List<IStandardNode> nodes = bodies[body];

				if (nodes.Count <= 0)
					continue;

				CreateBody(body, nodes);
			}
		}

		private void CreateBody(string b, List<IStandardNode> n)
		{
			GameObject obj = Instantiate(BodyPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(BodyTransform, false);

			CW_BodyNode nodeObject = obj.GetComponent<CW_BodyNode>();

			if (nodeObject == null)
				return;

			nodeObject.setBodyType(b, n);

			bodyNodes.Add(nodeObject);

			nodeObject.gameObject.SetActive(false);
		}

		//public void AddBody(string body, List<IStandardNode> nodes)
		//{
		//	if (nodes.Count <= 0)
		//		return;

		//	CreateBody(body, nodes);
		//}

		//public void AddBodyNode(string b, IStandardNode n)
		//{
		//	if (n == null)
		//		return;

		//	for (int i = bodyNodes.Count - 1; i >= 0; i--)
		//	{
		//		CW_BodyNode body = bodyNodes[i];

		//		if (body == null)
		//			continue;

		//		if (body.BodyName != b)
		//			continue;

		//		body.AddBodyNode(n);

		//		break;
		//	}
		//}

	}
}
