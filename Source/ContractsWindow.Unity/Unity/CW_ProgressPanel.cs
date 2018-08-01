#region license
/*The MIT License (MIT)
CW_ProgressPanel - Controls the main progress node panel UI element

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

using System.Collections;
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
		private CW_IntervalTypes IntervalPrefab = null;
		[SerializeField]
		private Transform IntervalTransform = null;
		[SerializeField]
		private CW_StandardNode StandardPrefab = null;
		[SerializeField]
		private Transform StandardTransform = null;
		[SerializeField]
		private Transform POITransform = null;
		[SerializeField]
		private CW_BodyNode BodyPrefab = null;
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

        private bool loaded;

        public bool Loaded
        {
            get { return loaded; }
        }
        
        public IEnumerator GeneratePanel(IProgressPanel panel)
        {
            if (panel == null)
                yield break;

            gameObject.SetActive(true);
            
            panelInterface = panel;

            if (panelInterface.GetIntervalNodes != null)
            {
                if (IntervalPrefab != null && IntervalTransform != null)
                {
                    for (int i = panelInterface.GetIntervalNodes.Count - 1; i >= 0; i--)
                    {
                        CreateIntervalType(panelInterface.GetIntervalNodes[i]);

                        yield return null;
                    }
                }
            }

            if (panelInterface.GetStandardNodes != null)
            {
                if (StandardPrefab != null && StandardTransform != null)
                {
                    for (int i = panelInterface.GetStandardNodes.Count - 1; i >= 0; i--)
                    {
                        CreateStandardNode(panelInterface.GetStandardNodes[i]);

                        yield return null;
                    }
                }
            }

            if (panelInterface.GetPOINodes != null)
            {
                if (StandardPrefab != null && POITransform != null)
                {
                    for (int i = panelInterface.GetPOINodes.Count - 1; i >= 0; i--)
                    {
                        CreatePOINode(panelInterface.GetPOINodes[i]);

                        yield return null;
                    }
                }
            }

            if (panelInterface.GetBodies != null)
            {
                if (BodyPrefab != null && BodyTransform != null)
                {
                    for (int i = panelInterface.GetBodies.Count - 1; i >= 0; i--)
                    {
                        string body = panelInterface.GetBodies.ElementAt(i).Key;

                        CreateBody(body, panelInterface.GetBodies[body]);

                        yield return null;
                    }
                }
            }

            FinishPanel();

            loaded = true;
            
            gameObject.SetActive(false);
        }

        private void FinishPanel()
        {
            if (IntervalToggle != null)
            {
                IntervalToggle.gameObject.SetActive(panelInterface.AnyInterval);

                if (panelInterface.IntervalVisible)
                    IntervalToggle.isOn = true;
                else
                    ToggleIntervals(false);
            }

            if (StandardToggle != null)
            {
                StandardToggle.gameObject.SetActive(panelInterface.AnyStandard);

                if (panelInterface.StandardVisible)
                    StandardToggle.isOn = true;
                else
                    ToggleStandards(false);
            }

            if (POIToggle != null)
            {
                POIToggle.gameObject.SetActive(panelInterface.AnyPOI);

                if (panelInterface.POIVisible)
                    POIToggle.isOn = true;
                else
                    TogglePOIs(false);
            }

            if (BodyToggle != null)
            {
                BodyToggle.gameObject.SetActive(panelInterface.AnyBody);

                if (panelInterface.BodyVisible)
                    BodyToggle.isOn = true;
                else
                    ToggleBodies(false);
            }
        }

		public void Refresh()
		{
			if (panelInterface == null)
				return;

			if (POIToggle != null)
			{
				POIToggle.gameObject.SetActive(panelInterface.AnyPOI);

				if (POIToggle.isOn)
					TogglePOIs(true);
			}

			if (StandardToggle != null)
			{
				StandardToggle.gameObject.SetActive(panelInterface.AnyStandard);

				if (StandardToggle.isOn)
					ToggleStandards(true);
			}

			if (IntervalToggle != null)
			{
				IntervalToggle.gameObject.SetActive(panelInterface.AnyInterval);

				if (IntervalToggle.isOn)
				{
					for (int i = intervalTypes.Count - 1; i >= 0; i--)
					{
						CW_IntervalTypes type = intervalTypes[i];

						if (type == null)
							continue;
                        
						if (!type.IsReached)
							continue;

						type.gameObject.SetActive(true);

						type.Refresh();
					}
				}
			}

			if (BodyToggle != null)
			{
				BodyToggle.gameObject.SetActive(panelInterface.AnyBody);

				if (BodyToggle.isOn)
				{
					for (int i = bodyNodes.Count - 1; i >= 0; i--)
					{
						CW_BodyNode body = bodyNodes[i];

						if (body == null)
							continue;

						if (!panelInterface.AnyBodyNode(body.BodyName))
							continue;

						body.gameObject.SetActive(true);

						body.Refresh();
					}
				}
			}
		}

		public void ToggleIntervals(bool isOn)
		{
			if (IntervalTransform == null || panelInterface == null)
				return;

			panelInterface.IntervalVisible = isOn;

			IntervalTransform.gameObject.SetActive(isOn);

			if (!isOn)
				return;

			for (int i = intervalTypes.Count - 1; i >= 0; i--)
			{
				CW_IntervalTypes node = intervalTypes[i];

				if (node == null)
					continue;
                
				node.gameObject.SetActive(node.IsReached);
			}
		}

		public void TogglePOIs(bool isOn)
		{
			if (POITransform == null || panelInterface == null)
				return;

			panelInterface.POIVisible = isOn;

			POITransform.gameObject.SetActive(isOn);

			if (!isOn)
				return;

			for (int i = poiNodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = poiNodes[i];

				if (node == null)
					continue;
                
				node.UpdateText();

				node.gameObject.SetActive(node.IsComplete);
			}
		}

		public void ToggleStandards(bool isOn)
		{
			if (StandardTransform == null || panelInterface == null)
				return;

			panelInterface.StandardVisible = isOn;

			StandardTransform.gameObject.SetActive(isOn);

			if (!isOn)
				return;

			for (int i = standardNodes.Count - 1; i >= 0; i--)
			{
				CW_StandardNode node = standardNodes[i];

				if (node == null)
					continue;
                
				node.UpdateText();

				node.gameObject.SetActive(node.IsComplete);
			}
		}

		public void ToggleBodies(bool isOn)
		{
			if (BodyTransform == null || panelInterface == null)
				return;

			panelInterface.BodyVisible = isOn;

			BodyTransform.gameObject.SetActive(isOn);

			if (!isOn)
				return;

			for (int i = bodyNodes.Count - 1; i >= 0; i--)
			{
				CW_BodyNode node = bodyNodes[i];

				if (node == null)
					continue;

				node.gameObject.SetActive(panelInterface.AnyBodyNode(node.BodyName));
			}
		}
        
		private void CreateIntervalType(IIntervalNode n)
		{
            CW_IntervalTypes nodeObject = Instantiate(IntervalPrefab, IntervalTransform, false);
            
			nodeObject.setIntervalType(n);

			intervalTypes.Add(nodeObject);
		}
        
		private void CreatePOINode(IStandardNode node)
		{
            CW_StandardNode nodeObject = Instantiate(StandardPrefab, POITransform, false);
            
			nodeObject.setNode(node);

			poiNodes.Add(nodeObject);
		}
        
		private void CreateStandardNode(IStandardNode node)
		{
            CW_StandardNode nodeObject = Instantiate(StandardPrefab, StandardTransform, false);
            
			nodeObject.setNode(node);

			standardNodes.Add(nodeObject);
		}

		private void CreateBody(string b, List<IStandardNode> n)
		{
            CW_BodyNode nodeObject = Instantiate(BodyPrefab, BodyTransform, false);
            
			nodeObject.setBodyType(b, n);

			bodyNodes.Add(nodeObject);
		}

	}
}
