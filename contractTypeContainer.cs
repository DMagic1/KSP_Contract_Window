using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Contracts;
using Contracts.Parameters;
using Contracts.Templates;
using UnityEngine;

namespace ContractsWindow
{
	class contractTypeContainer
	{
		private Type contractType;
		private string name = "";
		private Contract contractC = null;
		private float rewardFund, penaltyFund, advanceFund, rewardRep, penaltyRep, rewardScience, durationTime, expirationTime;
		private float maxOffer, maxActive;

		internal contractTypeContainer (Type CType)
		{
			contractType = CType;
			try
			{
			contractC = (Contract)Activator.CreateInstance(CType);
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("This Contract Type: {0} Does Not Have An Empty Constructor And Will Be Skipped: {1]", CType.Name, e);
				
			}
			name = CType.Name;
			name = Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
			rewardFund = penaltyFund = advanceFund = rewardRep = penaltyRep = rewardScience = durationTime = expirationTime = 1f;
			maxOffer = maxActive = 20;
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
