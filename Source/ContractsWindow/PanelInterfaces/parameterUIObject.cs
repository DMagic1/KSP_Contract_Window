#region license
/*The MIT License (MIT)
parameterUIObject - Storage class for information about contract parameters

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

		private string coloredText(string s, string sprite, string color)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			return string.Format("<color={0}>{1}{2}</color>  ", color, sprite, s);
		}

		public string RewardText
		{
			get
			{
				if (container == null)
					return "";

				return string.Format("{0}{1}{2}", coloredText(container.FundsRewString, "<sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1>", "#69D84FFF"), coloredText(container.SciRewString, "<sprite=\"CurrencySpriteAsset\" name=\"Science\" tint=1>", "#02D8E9FF"), coloredText(container.RepRewString, "<sprite=\"CurrencySpriteAsset\" name=\"Reputation\" tint=1>", "#C9B003FF"));
			}
		}

		public string PenaltyText
		{
			get
			{
				if (container == null)
					return "";

				return string.Format("{0}{1}", coloredText(container.FundsPenString, "<sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1>", "#FA4224FF"), coloredText(container.RepPenString, "<sprite=\"CurrencySpriteAsset\" name=\"Reputation\" tint=1>", "#FA4224FF"));
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
