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
		internal float science, repReward, repPenalty, totalScience, totalRepReward, totalRepPenalty;
		internal double expiration, acceptance, duration, reward, advance, penalty, totalReward, totalPenalty;
		internal bool showParams;
		internal List<parameterContainer> paramList = new List<parameterContainer>();

		//Store info on contracts
		internal contractContainer(Contract Contract)
		{
			contract = Contract;
			expiration = contract.DateDeadline;
			acceptance = contract.DateAccepted;
			duration = expiration - acceptance;
			advance = contract.FundsAdvance;
			reward = totalReward = contract.FundsCompletion;
			penalty = totalPenalty = contract.FundsFailure;
			science = totalScience = contract.ScienceCompletion;
			repReward = totalRepReward = contract.ReputationCompletion;
			repPenalty = totalRepPenalty = contract.ReputationFailure;
			showParams = true;

			foreach (ContractParameter param in contract.AllParameters)
			{
				paramList.Add(new parameterContainer(param));
				totalReward += param.FundsCompletion;
				totalPenalty += param.FundsFailure;
				totalRepReward += param.ReputationCompletion;
				totalRepPenalty += param.ReputationFailure;
				totalScience += param.ScienceCompletion;
			}
		}
	}

	//Store some info about contract parameters
	internal class parameterContainer
	{
		internal ContractParameter cParam;
		internal bool showNote;

		internal parameterContainer(ContractParameter cP)
		{
			cParam = cP;
			showNote = false;
		}
	}

	enum sortClass
	{
		Default = 1,
		Expiration = 2,
		Acceptance = 3,
		Duration = 4,
		Reward = 5,
	}

}
