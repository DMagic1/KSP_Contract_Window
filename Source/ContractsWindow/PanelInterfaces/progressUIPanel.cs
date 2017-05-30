#region license
/*The MIT License (MIT)
progressUIPanel - Storage class for information about the main progress node panel

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
using ContractsWindow.Unity.Unity;
using ProgressParser;
using UnityEngine;

namespace ContractsWindow.PanelInterfaces
{
	public class progressUIPanel : IProgressPanel
	{
		private bool _intervalVisible;
		private bool _poiVisible;
		private bool _standardVisible;
		private bool _bodyVisible;
		private Dictionary<string, List<IStandardNode>> bodies = new Dictionary<string, List<IStandardNode>>();
		private List<IIntervalNode> intervals = new List<IIntervalNode>();
		private List<IStandardNode> pois = new List<IStandardNode>();
		private List<IStandardNode> standards = new List<IStandardNode>();
		
		public progressUIPanel()
		{			
			loadIntervals(progressParser.getAllIntervalNodes);
			loadStandards(progressParser.getAllStandardNodes);
			loadPOIs(progressParser.getAllPOINodes);
			loadBodies(progressParser.getAllBodyNodes);
		}

		private void loadIntervals(List<progressInterval> nodes)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				IntervalNodeUI node = new IntervalNodeUI(nodes[i]);

				if (node == null)
					continue;

				intervals.Add(node);
			}
		}

		private void loadStandards(List<progressStandard> nodes)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				StandardNodeUI node = new StandardNodeUI(nodes[i]);

				if (node == null)
					continue;

				standards.Add(node);
			}
		}

		private void loadPOIs(List<progressStandard> nodes)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				StandardNodeUI node = new StandardNodeUI(nodes[i]);

				if (node == null)
					continue;

				pois.Add(node);
			}
		}

		private void loadBodies(List<progressBodyCollection> nodes)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				progressBodyCollection body = nodes[i];

				if (body == null)
					continue;

				if (bodies.ContainsKey(body.Body.displayName.LocalizeBodyName()))
					continue;

				List<progressStandard> bodySubNodes = body.getAllNodes;

				List<IStandardNode> newNodes = new List<IStandardNode>();

				for (int j = bodySubNodes.Count - 1; j >= 0; j--)
				{
					StandardNodeUI node = new StandardNodeUI(bodySubNodes[j]);

					if (node == null)
						continue;

					newNodes.Add(node);
				}

				bodies.Add(body.Body.displayName.LocalizeBodyName(), newNodes);
			}
		}

		public bool IntervalVisible
		{
			get { return _intervalVisible; }
			set { _intervalVisible = value; }
		}

		public bool POIVisible
		{
			get { return _poiVisible; }
			set { _poiVisible = value; }
		}

		public bool StandardVisible
		{
			get { return _standardVisible; }
			set { _standardVisible = value; }
		}

		public bool BodyVisible
		{
			get { return _bodyVisible; }
			set { _bodyVisible = value; }
		}

		public bool AnyInterval
		{
			get { return progressParser.AnyInterval; }
		}

		public bool AnyPOI
		{
			get { return progressParser.AnyPOI; }
		}

		public bool AnyStandard
		{
			get { return progressParser.AnyStandard; }
		}

		public bool AnyBody
		{
			get { return progressParser.AnyBody; }
		}

		public bool AnyBodyNode(string s)
		{
			progressBodyCollection body = progressParser.getProgressBody(s);

			if (body == null)
				return false;

			return body.IsReached;
		}
		
		public Dictionary<string, List<IStandardNode>> GetBodies
		{
			get { return bodies; }
		}

		public IList<IIntervalNode> GetIntervalNodes
		{
			get { return new List<IIntervalNode>(intervals.ToArray()); }
		}

		public IList<IStandardNode> GetPOINodes
		{
			get { return new List<IStandardNode>(pois.ToArray()); }
		}

		public IList<IStandardNode> GetStandardNodes
		{
			get { return new List<IStandardNode>(standards.ToArray()); }
		}
	}
}
