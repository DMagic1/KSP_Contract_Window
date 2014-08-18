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
		internal bool showParams, altElement;
		internal string daysToExpire, partTest;
		internal List<parameterContainer> paramList = new List<parameterContainer>();

		//Store info on contracts
		internal contractContainer(Contract Contract)
		{
			contract = Contract;

			if (contract.DateDeadline <= 0)
			{
				duration = double.MaxValue;
				daysToExpire = "----";
			}
			else
			{
				duration = contract.DateDeadline - Planetarium.GetUniversalTime();
				//Calculate time in day values using Kerbin or Earth days
				daysToExpire = timeInDays(duration);
			}

			totalReward = contract.FundsCompletion;
			showParams = true;

			//Generate four layers of parameters, check if each is an altitude parameter
			for (int i = 0; i < contract.ParameterCount; i++)
			{
				altElement = false;
				partTest = "";
				ContractParameter param = contract.GetParameter(i);
				altElement = altParamCheck(param);
				partTest = paramTypeCheck(param);
				paramList.Add(new parameterContainer(param, 0, altElement, partTest));
				totalReward += param.FundsCompletion;

				for (int j = 0; j < param.ParameterCount; j++)
				{
					altElement = false;
					partTest = "";
					ContractParameter subParam1 = param.GetParameter(j);
					altElement = altParamCheck(subParam1);
					partTest = paramTypeCheck(subParam1);
					paramList.Add(new parameterContainer(subParam1, 1, altElement, partTest));
					totalReward += subParam1.FundsCompletion;

					for (int k = 0; k < subParam1.ParameterCount; k++)
					{
						altElement = false;
						partTest = "";
						ContractParameter subParam2 = param.GetParameter(k);
						altElement = altParamCheck(subParam2);
						partTest = paramTypeCheck(subParam2);
						paramList.Add(new parameterContainer(subParam2, 2, altElement, partTest));
						totalReward += subParam2.FundsCompletion;

						for (int l = 0; l < subParam2.ParameterCount; l++)
						{
							altElement = false;
							partTest = "";
							ContractParameter subParam3 = param.GetParameter(k);
							altElement = altParamCheck(subParam3);
							partTest = paramTypeCheck(subParam3);
							paramList.Add(new parameterContainer(subParam3, 3, altElement, partTest));
							totalReward += subParam3.FundsCompletion;
						}
					}
				}
			}
		}

		private bool altParamCheck(ContractParameter param)
		{
			if (param.GetType() == typeof(ReachAltitudeEnvelope))
				return true;
			else
				return false;
		}

		private string paramTypeCheck(ContractParameter param)
		{
			if (param.GetType() == typeof(PartTest))
				return "partTest";

			if (contractAssembly.FPLoaded)
			{
				if (param.GetType() == contractAssembly._FPType)
					return "FinePrint";
			}

			if (contractAssembly.DMLoaded)
			{
				if (param.GetType() == contractAssembly._DMCType)
					return "DMcollectScience";
			}

			if (contractAssembly.DMALoaded)
			{
				if (param.GetType() == contractAssembly._DMAType)
					return "DManomalyScience";
				else
					return "";
			}

			return "";
		}

		internal static string timeInDays(double D)
		{
			if (D <= 0)
				return "----";

			int[] time = KSPUtil.GetDateFromUT((int)D);
			string s = "";

			if (time[4] > 0)
				s = string.Format("{0}y", time[4]);
			if (time[3] > 0)
			{
				if (!string.IsNullOrEmpty(s))
					s += " ";
				s += string.Format("{0}d", time[3]);
			}
			if (time[4] <= 0 && time[2] > 0)
			{
				if (!string.IsNullOrEmpty(s))
					s += " ";
				s += string.Format("{0}h", time[2]);
			}
			if (time[4] <= 0 && time[3] <= 0 && time[2] <= 0 && time[1] > 0)
				s = string.Format("{0}m", time[1]);

			return s;
		}
	}

	//Store some info about contract parameters
	internal class parameterContainer
	{
		internal ContractParameter cParam;
		internal bool showNote, altElement;
		internal int level;
		internal string partTestName;
		internal AvailablePart part;


		internal parameterContainer(ContractParameter cP, int Level, bool AltElement, string PartTestName)
		{
			cParam = cP;
			showNote = false;
			level = Level;
			altElement = AltElement;
			partTestName = PartTestName;
			
			if (!string.IsNullOrEmpty(partTestName))
			{
				if (partTestName == "partTest")
				{
					part = ((PartTest)cParam).tgtPartInfo;
					MonoBehaviourExtended.LogFormatted_DebugOnly("Part Assigned For Stock Part Test");
				}
				else if (partTestName == "DMcollectScience")
				{
					if (contractAssembly.DMLoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicSciencePartName(cParam));
						if (part != null)
							MonoBehaviourExtended.LogFormatted_DebugOnly("Part Assigned For DMagic Contract");
						else
							MonoBehaviourExtended.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (partTestName == "DManomalyScience")
				{
					if (contractAssembly.DMALoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicAnomalySciencePartName(cParam));
						if (part != null)
							MonoBehaviourExtended.LogFormatted_DebugOnly("Part Assigned For DMagic Anomaly Contract");
						else
							MonoBehaviourExtended.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (partTestName == "FinePrint")
				{
					part = PartLoader.getPartInfoByName(cParam.ID);
					if (part != null)
						MonoBehaviourExtended.LogFormatted_DebugOnly("Part Assigned For Fine Print Contract");
					else
						MonoBehaviourExtended.LogFormatted_DebugOnly("Part Not Found");
				}
				else
					part = null;
			}
		}

	}
}
