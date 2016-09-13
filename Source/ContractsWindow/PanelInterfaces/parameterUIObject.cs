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

			if (container.Level >= 4)
				return;

			for (int i = 0; i < p.ParameterCount; i++)
			{
				parameterContainer c = p.getParameter(i);

				if (c == null)
					continue;

				if (string.IsNullOrEmpty(c.Title))
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

		private string coloredText(string s, char c, string color)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			return string.Format("<color={0}>{1}{2}</color>  ", color, c, s);
		}

		public string RewardText
		{
			get
			{
				if (container == null)
					return "";

				return string.Format("{0}{1}{2}", coloredText(container.FundsRewString, '£', "#69D84FFF"), coloredText(container.SciRewString, '©', "#02D8E9FF"), coloredText(container.RepRewString, '¡', "#C9B003FF"));
			}
		}

		public string PenaltyText
		{
			get
			{
				if (container == null)
					return "";

				return string.Format("{0}{1}", coloredText(container.FundsPenString, '£', "#FA4224FF"), coloredText(container.RepPenString, '¡', "#FA4224FF"));
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
