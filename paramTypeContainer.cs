using System;
using System.Collections.Generic;
using System.Linq;

using Contracts;
using Contracts.Parameters;
using UnityEngine;

namespace ContractsWindow
{
	class paramTypeContainer
	{
		private Type paramType;
		private ContractParameter param;
		private string name;
		private float rewardFund, penaltyFund, rewardRep, penaltyRep, rewardScience;

		internal paramTypeContainer (Type PType)
		{
			paramType = PType;
			param = (ContractParameter)Activator.CreateInstance(PType);
			name = PType.Name;
		}

		public ContractParameter Param
		{
			get { return param; }
		}

		public string Name
		{
			get { return name; }
		}

		public float RewardFund
		{
			get { return rewardFund; }
			set
			{
				if (value >= 0 && value < 1000)
					rewardFund = value;
			}
		}

		public float PenaltyFund
		{
			get { return penaltyFund; }
			set
			{
				if (value >= 0 && value < 1000)
					penaltyFund = value;
			}
		}

		public float RewardRep
		{
			get { return rewardRep; }
			set
			{
				if (value >= 0 && value < 1000)
					rewardRep = value;
			}
		}

		public float PenaltyRep
		{
			get { return penaltyRep; }
			set
			{
				if (value >= 0 && value < 1000)
					penaltyRep = value;
			}
		}

		public float RewardScience
		{
			get { return rewardScience; }
			set
			{
				if (value >= 0 && value < 1000)
					rewardScience = value;
			}
		}
	}
}
