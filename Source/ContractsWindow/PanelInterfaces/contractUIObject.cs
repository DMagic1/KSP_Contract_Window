#region license
/*The MIT License (MIT)
Contract UI Object - Object used for contracts in different mission lists

Copyright (c) 2014 DMagic

KSP Plugin Framework by TriggerAu, 2014: http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework

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
using System.Linq;
using UnityEngine;
using ContractParser;
using ContractsWindow.Unity.Interfaces;
using ContractsWindow.Unity.Unity;
using ContractsWindow.Unity;

namespace ContractsWindow.PanelInterfaces
{
	/// <summary>
	/// This is the object actually used by the contract window
	/// It stores data for each contract about ordering and whether its parameters are shown
	/// Different mission lists are able to store contracts in different states using this object
	/// </summary>
	public class contractUIObject : IContractSection
	{
		private contractContainer container;
		private bool _showParams;
		private bool _hidden;
		private int? _order;
		private Sprite _agencyLogo;
		private string _agencyName;
		private Guid _id;
		private int _difficulty;
		private contractMission mission;
		private List<parameterUIObject> paramList = new List<parameterUIObject>();

		internal contractUIObject(contractContainer c, contractMission m)
		{
			container = c;
			mission = m;
			_showParams = true;
			_order = null;

			Texture2D tex = container.RootAgent.Logo;

			_agencyLogo = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
			_agencyName = container.RootAgent.Name;

			_difficulty = (int)container.Root.Prestige;
			_id = container.ID;

			for (int i = 0; i < c.ParameterCount; i++)
			{
				parameterContainer p = c.ParamList[i];

				if (p == null)
					continue;

				paramList.Add(new parameterUIObject(p));
			}
		}

		public Sprite AgencyLogo
		{
			get { return _agencyLogo; }
		}

		public string AgencyName
		{
			get { return _agencyName; }
		}

		public ContractState ContractState
		{
			get
			{
				if (container == null || container.Root == null)
					return Unity.ContractState.Active;

				switch (container.Root.ContractState)
				{
					case Contracts.Contract.State.Active:
					case Contracts.Contract.State.Generated:
					case Contracts.Contract.State.Offered:
						return Unity.ContractState.Active;
					case Contracts.Contract.State.Cancelled:
					case Contracts.Contract.State.DeadlineExpired:
					case Contracts.Contract.State.Declined:
					case Contracts.Contract.State.Failed:
					case Contracts.Contract.State.OfferExpired:
					case Contracts.Contract.State.Withdrawn:
						return Unity.ContractState.Fail;
					case Contracts.Contract.State.Completed:
						return Unity.ContractState.Complete;
					default:
						return Unity.ContractState.Fail;
				}
			}
		}
		
		public string ContractTitle
		{
			get
			{
				if (container == null)
					return "Null...";

				return container.Title;
			}
		}

		public Guid ID
		{
			get { return _id; }
		}

		public int Difficulty
		{
			get { return _difficulty; }
		}

		public bool IsHidden
		{
			get { return _hidden; }
			set
			{
				_hidden = value;

				if (mission == null)
					return;

				if (value)
				{
					contractScenario.ListRemove(mission.ActiveMissionList, _id);

					mission.HiddenMissionList.Add(_id);

					_showParams = false;

					_order = null;
				}
				else
				{
					contractScenario.ListRemove(mission.HiddenMissionList, _id);

					mission.ActiveMissionList.Add(_id);

					_showParams = true;
				}
			}
		}

		public bool ShowParams
		{
			get { return _showParams; }
			set { _showParams = value; }
		}

		public bool IsPinned
		{
			get { return _order != null; }
			set
			{
				if (value)
				{

				}
				else
					_order = null;
			}
		}

		public int? Order
		{
			get { return _order; }
			set { _order = value; }
		}
		
		public string PenaltyText
		{
			get
			{
				if (container == null)
					return "";

				return string.Format("<color=#FA4224FF>£ {0}</color>  <color=#FA4224FF>¡ {1}</color>", container.FundsPenString, container.RepPenString);
			}
		}
		
		public string RewardText
		{
			get
			{
				if (container == null)
					return "";

				return string.Format("<color=#69D84FFF>£ {0}</color>  <color=#02D8E9FF>© {1}</color>  <color=#C9B003FF>¡ {2}</color>", container.FundsRewString, container.SciRewString, container.RepRewString);
			}
		}

		public string GetNote
		{
			get
			{
				if (container == null)
					return "";

				return container.Notes;
			}
		}
		
		public string TimeRemaining
		{
			get
			{
				if (container == null)
					return "";

				return container.DaysToExpire;
			}
		}
		
		public int TimeState
		{
			get
			{
				if (container == null || container.Root == null)
					return 2;

				if (container.Duration >= 2160000)
					return 0;
				else if (container.Duration > 0)
					return 1;
				else if (container.Root.ContractState == Contracts.Contract.State.Completed)
					return 0;
				else
					return 2;
			}
		}

		public IList<IParameterSection> GetParameters
		{
			get { return new List<IParameterSection>(paramList.ToArray()); }
		}

		public void RemoveContractFromAll()
		{
			for (int i = contractScenario.Instance.getAllMissions().Count - 1; i >= 0; i--)
			{
				contractMission m = contractScenario.Instance.getAllMissions()[i];

				if (m == null)
					return;

				m.removeContract(container);
			}
		}

		public contractContainer Container
		{
			get { return container; }
		}
	}
}
