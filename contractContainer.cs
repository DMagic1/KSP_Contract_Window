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

using Contracts;
using UnityEngine;

namespace ContractsWindow
{
	internal class contractContainer
	{
		internal Contract contract;
		internal double totalReward, duration;
		internal bool showParams;
		internal string daysToExpire;
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

			//Generate four layers of parameters
			for (int i = 0; i < contract.ParameterCount; i++)
			{
				ContractParameter param = contract.GetParameter(i);
				paramList.Add(new parameterContainer(param, 0));
				totalReward += param.FundsCompletion;
				for (int j = 0; j < param.ParameterCount; j++)
				{
					ContractParameter subParam1 = param.GetParameter(j);
					paramList.Add(new parameterContainer(subParam1, 1));
					totalReward += subParam1.FundsCompletion;
					for (int k = 0; k < subParam1.ParameterCount; k++)
					{
						ContractParameter subParam2 = param.GetParameter(k);
						paramList.Add(new parameterContainer(subParam2, 2));
						totalReward += subParam2.FundsCompletion;
						for (int l = 0; l < subParam2.ParameterCount; l++)
						{
							ContractParameter subParam3 = param.GetParameter(k);
							paramList.Add(new parameterContainer(subParam3, 3));
							totalReward += subParam3.FundsCompletion;
						}
					}
				}
			}
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
		internal bool showNote;
		internal int level;

		internal parameterContainer(ContractParameter cP, int Level)
		{
			cParam = cP;
			showNote = false;
			level = Level;
		}
	}

	enum sortClass
	{
		Default = 1,
		Expiration = 2,
		Acceptance = 3,
		Difficulty = 4,
		Reward = 5,
		Type = 6,
	}

}
