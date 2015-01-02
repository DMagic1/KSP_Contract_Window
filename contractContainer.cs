#region license
/*The MIT License (MIT)
Contract Container - Object to hold contract info

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

using System.Collections.Generic;
using System;
using System.Linq;
using Contracts;
using Contracts.Parameters;
using UnityEngine;
using System.Reflection;

namespace ContractsWindow
{
	internal class contractContainer
	{
		internal Contract contract;
		internal double totalReward, duration;
		internal bool showParams, showNote;
		internal string daysToExpire;
		internal int? listOrder;
		internal double fundsReward, fundsPenalty;
		internal float repReward, repPenalty, scienceReward, fundsRewStrat, fundsPenStrat, repRewStrat, repPenStrat, sciRewStrat;
		internal string fundsRew, fundsPen, repRew, repPen, sciRew;
		internal List<parameterContainer> paramList = new List<parameterContainer>();

		//Store info on contracts
		internal contractContainer(Contract c)
		{
			contract = c;
			showParams = true;
			listOrder = null;

			if (c.DateDeadline <= 0)
			{
				duration = double.MaxValue;
				daysToExpire = "----";
			}
			else
			{
				duration = contract.DateDeadline - Planetarium.GetUniversalTime();
				//Calculate time in day values using Kerbin or Earth days
				daysToExpire = contractScenario.timeInDays(duration);
			}

			contractRewards(c);
			contractPenalties(c);

			totalReward = c.FundsCompletion;
			foreach (ContractParameter param in c.AllParameters)
				totalReward += param.FundsCompletion;

			//Generate four layers of parameters, check if each is an altitude parameter
			for (int i = 0; i < c.ParameterCount; i++)
			{
				ContractParameter param = c.GetParameter(i);
				addContractParam(param, 0);
			}
		}

		private void addContractParam(ContractParameter param, int Level)
		{
			string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(param, Level, partTest));
		}

		private void contractRewards(Contract c)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)c.FundsCompletion, c.ReputationCompletion, c.ScienceCompletion);
			fundsReward = c.FundsCompletion;
			fundsRew = "+ " + fundsReward.ToString("N0");
			fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsRewStrat != 0f)
			{
				fundsRew = string.Format("+ {0:N0} ({1:N0})", fundsReward + fundsRewStrat, fundsRewStrat);
			}
			repReward = c.ReputationCompletion;
			repRew = "+ " + repReward.ToString("N0");
			repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repRewStrat != 0f)
			{
				repRew = string.Format("+ {0:N0} ({1:N0})", repReward + repRewStrat, repRewStrat);
			}
			scienceReward = c.ScienceCompletion;
			sciRew = "+ " + scienceReward.ToString("N0");
			sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRew = string.Format("+ {0:N0} ({1:N0})", scienceReward + sciRewStrat, sciRewStrat);
			}
		}

		private void contractPenalties(Contract c)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)c.FundsFailure, c.ReputationFailure, 0f);
			fundsPenalty = c.FundsFailure;
			fundsPen = "- " + fundsPenalty.ToString("N0");
			fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsPenStrat != 0f)
			{
				fundsPen = string.Format("- {0:N0} ({1:N0})", fundsPenalty + fundsPenStrat, fundsPenStrat);
			}
			repPenalty = c.ReputationFailure;
			repPen = "- " + repPenalty.ToString("N0");
			repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPen = string.Format("- {0:N0} ({1:N0})", repPenalty + repPenStrat, repPenStrat);
			}
		}

		internal void updateContractInfo()
		{
			contractRewards(contract);
			contractPenalties(contract);
			foreach (parameterContainer pC in paramList)
			{
				pC.paramRewards(pC.cParam);
				pC.paramPenalties(pC.cParam);
			}
		}
	}

	//Store some info about contract parameters
	internal class parameterContainer
	{
		internal ContractParameter cParam;
		internal bool showNote;
		internal int level;
		internal double fundsReward, fundsPenalty;
		internal float repReward, repPenalty, scienceReward, fundsRewStrat, fundsPenStrat, repRewStrat, repPenStrat, sciRewStrat;
		internal string fundsRew, fundsPen, repRew, repPen, sciRew;
		internal AvailablePart part;
		internal List<parameterContainer> paramList = new List<parameterContainer>();

		internal parameterContainer(ContractParameter cP, int Level, string PartTestName)
		{
			cParam = cP;
			showNote = false;
			level = Level;
			//For some reason parameter rewards/penalties reset to zero upon completion/failure
			paramRewards(cP);
			paramPenalties(cP);

			if (level < 4)
			{
				for (int i = 0; i < cParam.ParameterCount; i++)
				{
					ContractParameter param = cParam.GetParameter(i);
					addSubParam(param, level + 1);
				}
			}

			if (!string.IsNullOrEmpty(PartTestName))
			{
				if (PartTestName == "partTest")
				{
					part = ((PartTest)cParam).tgtPartInfo;
					DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Stock Part Test");
				}
				else if (PartTestName == "MCEScience")
				{
					if (contractAssembly.MCELoaded)
					{
						part = PartLoader.Instance.parts.FirstOrDefault(p => p.partPrefab.partInfo.title == contractAssembly.MCEPartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Mission Controller Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "DMcollectScience")
				{
					if (contractAssembly.DMLoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicSciencePartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "DManomalyScience")
				{
					if (contractAssembly.DMALoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicAnomalySciencePartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Anomaly Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "DMasteroidScience")
				{
					if (contractAssembly.DMAstLoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicAsteroidSciencePartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Asteroid Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "FinePrint")
				{
					part = PartLoader.getPartInfoByName(contractAssembly.FPPartName(cParam));
					if (part != null)
						DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Fine Print Contract");
					else
						DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
				}
				else
					part = null;
			}
		}

		internal void paramRewards(ContractParameter cP)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)cP.FundsCompletion, cP.ReputationCompletion, cP.ScienceCompletion);
			fundsReward = cP.FundsCompletion;
			fundsRew = "+ " + fundsReward.ToString("N0");
			fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if(fundsRewStrat != 0f)
			{
				fundsRew = string.Format("+ {0:N0} ({1:N0})", fundsReward + fundsRewStrat, fundsRewStrat);
			}
			repReward = cP.ReputationCompletion;
			repRew = "+ " + repReward.ToString("N0");
			repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if(repRewStrat != 0f)
			{
				repRew = string.Format("+ {0:N0} ({1:N0})", repReward + repRewStrat, repRewStrat);
			}
			scienceReward = cP.ScienceCompletion;
			sciRew = "+ " + scienceReward.ToString("N0");
			sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRew = string.Format("+ {0:N0} ({1:N0})", scienceReward + sciRewStrat, sciRewStrat);
			}
		}

		internal void paramPenalties(ContractParameter cP)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)cP.FundsFailure, cP.ReputationFailure, 0f);
			fundsPenalty = cP.FundsFailure;
			fundsPen = "- " + fundsPenalty.ToString("N0");
			fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if(fundsPenStrat != 0f)
			{
				fundsPen = string.Format("- {0:N0} ({1:N0})", fundsPenalty + fundsPenStrat, fundsPenStrat);
			}
			repPenalty = cP.ReputationFailure;
			repPen = "- " + repPenalty.ToString("N0");
			repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPen = string.Format("- {0:N0} ({1:N0})", repPenalty + repPenStrat, repPenStrat);
			}
		}

		private void addSubParam(ContractParameter param, int Level)
		{
			string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(param, Level, partTest));
		}

	}
}
