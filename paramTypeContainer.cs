#region license
/*The MIT License (MIT)
Parameter Type Container - An object to store variables for customizing contract parameter rewards/penalties

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
