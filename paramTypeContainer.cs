using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Contracts;
using Contracts.Parameters;
using UnityEngine;

namespace ContractsWindow
{
	class paramTypeContainer
	{
		private Type paramType;
		private ContractParameter param = null;
		private string name;
		private float rewardFund, penaltyFund, rewardRep, penaltyRep, rewardScience;
		private float[] paramValues = new float[5];

		internal paramTypeContainer (Type PType)
		{
			paramType = PType;
			try
			{
				param = (ContractParameter)Activator.CreateInstance(PType);
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("This Parameter Type: {0} Does Not Have An Empty Constructor And Will Be Skipped: {1]", PType.Name, e);
				return;
			}
			name = PType.Name;
			name = Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
			rewardFund = penaltyFund = rewardRep = penaltyRep = rewardScience = 1f;
		}

		public ContractParameter Param
		{
			get { return param; }
		}

		public Type ParamType
		{
			get { return paramType; }
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
				if (value >= 0 && value <= 10)
				{
					if (contractScenario.Instance.allowZero && value == 0.00)
						value = 0.001f;
					rewardFund = value;
				}
			}
		}

		public float PenaltyFund
		{
			get { return penaltyFund; }
			set
			{
				if (value >= 0 && value <= 10)
				{
					if (contractScenario.Instance.allowZero && value == 0.00)
						value = 0.001f;
					penaltyFund = value;
				}
			}
		}

		public float RewardRep
		{
			get { return rewardRep; }
			set
			{
				if (value >= 0 && value <= 10)
				{
					if (contractScenario.Instance.allowZero && value == 0.00)
						value = 0.001f;
					rewardRep = value;
				}
			}
		}

		public float PenaltyRep
		{
			get { return penaltyRep; }
			set
			{
				if (value >= 0 && value <= 10)
				{
					if (contractScenario.Instance.allowZero && value == 0.00)
						value = 0.001f;
					penaltyRep = value;
				}
			}
		}

		public float RewardScience
		{
			get { return rewardScience; }
			set
			{
				if (value >= 0 && value <= 10)
				{
					if (contractScenario.Instance.allowZero && value == 0.00)
						value = 0.001f;
					rewardScience = value;
				}
			}
		}

		public float[] ParamValues
		{
			get
			{
				paramValues[0] = rewardFund;
				paramValues[1] = penaltyFund;
				paramValues[2] = rewardRep;
				paramValues[3] = penaltyRep;
				paramValues[4] = rewardScience;
				return paramValues;
			}
		}
	}
}
