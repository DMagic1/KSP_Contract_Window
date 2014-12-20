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
		internal List<parameterContainer> paramList = new List<parameterContainer>();

		//Store info on contracts
		internal contractContainer(Contract Contract)
		{
			contract = Contract;
			showParams = true;
			listOrder = null;

			if (contract.DateDeadline <= 0)
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

			totalReward = contract.FundsCompletion;
			foreach (ContractParameter param in contract.AllParameters)
				totalReward += param.FundsCompletion;

			//Generate four layers of parameters, check if each is an altitude parameter
			for (int i = 0; i < contract.ParameterCount; i++)
			{
				ContractParameter param = contract.GetParameter(i);
				addContractParam(param, 0);
			}
		}

		private void addContractParam(ContractParameter param, int Level)
		{
			string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(param, Level, partTest));
		}
	}

	//Store some info about contract parameters
	internal class parameterContainer
	{
		internal ContractParameter cParam;
		internal bool showNote;
		internal int level;
		//internal double fundsReward, fundsPenalty;
		//internal float repReward, repPenalty, scienceReward;
		internal AvailablePart part;
		internal List<parameterContainer> paramList = new List<parameterContainer>();

		internal parameterContainer(ContractParameter cP, int Level, string PartTestName)
		{
			cParam = cP;
			showNote = false;
			level = Level;
			//For some reason parameter rewards/penalties reset to zero upon completion/failure
			//fundsReward = cP.FundsCompletion;
			//fundsPenalty = cP.FundsFailure;
			//repReward = cP.ReputationCompletion;
			//repPenalty = cP.ReputationFailure;
			//scienceReward = cP.ScienceCompletion;

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

		private void addSubParam(ContractParameter param, int Level)
		{
			string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(param, Level, partTest));
		}

	}
}
