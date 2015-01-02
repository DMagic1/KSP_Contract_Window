#region license
/*The MIT License (MIT)
Contract Type Container - An object to store variables for customizing contract rewards/penalties

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

using System;
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
		private float[] contractValues = new float[9];

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
			maxOffer = maxActive = 10f;
		}

		public Contract ContractC
		{
			get { return contractC; }
		}

		public Type ContractType
		{
			get { return contractType; }
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
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
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
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
					penaltyFund = value;
				}
			}
		}

		public float AdvanceFund
		{
			get { return advanceFund; }
			set
			{
				if (value >= 0 && value <= 10)
				{
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
					advanceFund = value;
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
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
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
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
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
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
					rewardScience = value;
				}
			}
		}

		public float DurationTime
		{
			get { return durationTime; }
			set
			{
				if (value > 0 && value <= 10)
				{
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
					durationTime = value;
				}
			}
		}

		public float ExpirationTime
		{
			get { return expirationTime; }
			set
			{
				if (value > 0 && value <= 10)
				{
					if (contractScenario.Instance != null)
					{
						if (!contractScenario.Instance.allowZero && value == 0.00f)
							value = 0.001f;
					}
					if (value == 0.00f)
						value = 0.0000000001f;
					expirationTime = value;
				}
			}
		}

		public float MaxOffer
		{
			get { return maxOffer; }
			set
			{
				if (value >= 0 && value <= 10)
					maxOffer = value;
			}
		}

		public float MaxActive
		{
			get { return maxActive; }
			set
			{
				if (value >= 0 && value <= 10)
					maxActive = value;
			}
		}

		public float[] ContractValues
		{
			get
			{
				contractValues[0] = rewardFund;
				contractValues[1] = advanceFund;
				contractValues[2] = penaltyFund;
				contractValues[3] = rewardRep;
				contractValues[4] = penaltyRep;
				contractValues[5] = rewardScience;
				contractValues[6] = durationTime;
				contractValues[7] = maxOffer;
				contractValues[8] = maxActive;
				return contractValues;
			}
		}


	}
}
