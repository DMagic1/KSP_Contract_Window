using System;
using System.Collections.Generic;
using UnityEngine;
using ContractParser;
using ContractsWindow.Unity.Interfaces;
using ContractsWindow.Unity.Unity;
using ContractsWindow.Unity;

namespace ContractsWindow.PanelInterfaces
{
	public class parameterUIObject : IParameterSection
	{
		private parameterContainer container;
		private List<parameterUIObject> subParams = new List<parameterUIObject>();

		internal parameterUIObject(parameterContainer p)
		{
			container = p;

			for (int i = 0; i < p.ParameterCount; i++)
			{
				parameterContainer c = p.ParamList[i];

				if (c == null)
					continue;

				subParams.Add(new parameterUIObject(c));
			}
		}

		public ContractState ParameterState
		{
			get
			{
				if (container == null || container.CParam == null)
					return ContractState.Fail;

				switch(container.CParam.State)
				{
					case Contracts.ParameterState.Complete:
						return ContractState.Complete;
					case Contracts.ParameterState.Failed:
						return ContractState.Fail;
					case Contracts.ParameterState.Incomplete:
						return ContractState.Active;
					default:
						return ContractState.Fail;
				}
			}
		}

		public int ParamLayer
		{
			get
			{
				if (container == null)
					return 0;

				return container.Level;
			}
		}

		public string TitleText
		{
			get
			{
				if (container == null)
					return "";

				return container.Title;
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

		public string PenaltyText
		{
			get
			{
				if (container == null)
					return "";

				return string.Format("<color=#FA4224FF>£ {0}</color>  <color=#FA4224FF>¡ {1}</color>", container.FundsPenString, container.RepPenString);
			}
		}

		public string GetNote
		{
			get
			{
				if (container == null)
					return "";

				return container.Notes(HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsEditor);
			}
		}

		public IList<IParameterSection> GetSubParams
		{
			get { return new List<IParameterSection>(subParams.ToArray()); }
		}
	}
}
