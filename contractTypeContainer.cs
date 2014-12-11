using System;
using System.Collections.Generic;
using System.Linq;

using Contracts;
using Contracts.Parameters;
using Contracts.Templates;
using UnityEngine;

namespace ContractsWindow
{
	class contractTypeContainer
	{
		private Type contractType;
		private string name;
		private Contract contractC;
		private float rewardFund, penaltyFund, advanceFund, rewardRep, penaltyRep, rewardScience;
		private float durationTime, expirationTime, maxOffer, maxActive;

		internal contractTypeContainer (Type CType)
		{
			contractType = CType;
			contractC = (Contract)Activator.CreateInstance(CType);
			name = CType.Name;
		}

		public Contract ContractC
		{
			get { return contractC; }
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

		public float AdvanceFund
		{
			get { return advanceFund; }
			set
			{
				if (value >= 0 && value < 1000)
					advanceFund = value;
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

		public float DurationTime
		{
			get { return durationTime; }
			set
			{
				if (value > 0 && value < 1000)
					durationTime = value;
			}
		}

		public float ExpirationTime
		{
			get { return expirationTime; }
			set
			{
				if (value > 0 && value < 1000)
					expirationTime = value;
			}
		}

		public float MaxOffer
		{
			get { return maxOffer; }
			set
			{
				if (value >= 0 && value < 50)
					maxOffer = value;
			}
		}

		public float MaxActive
		{
			get { return maxActive; }
			set
			{
				if (value >= 0 && value < 50)
					maxActive = value;
			}
		}


	}
}
