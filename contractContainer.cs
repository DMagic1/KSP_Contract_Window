using System;
using System.Collections.Generic;
using System.Linq;

using Contracts;
using UnityEngine;

namespace ContractsWindow
{
	class contractContainer
	{
		internal Contract contract;
		internal float science, repReward, repPenalty, totalScience, totalRepReward, totalRepPenalty;
		internal double expiration, acceptance, duration, reward, advance, penalty, totalReward, totalPenalty;
		internal bool showParams;

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
				totalReward += param.FundsCompletion;
				totalPenalty += param.FundsFailure;
				totalRepReward += param.ReputationCompletion;
				totalRepPenalty += param.ReputationFailure;
				totalScience += param.ScienceCompletion;
			}
		}

	}
}
