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
using System.Text.RegularExpressions;
using Contracts;
using Contracts.Templates;
using FinePrint.Contracts;
using FinePrint.Contracts.Parameters;
using FinePrint.Utilities;
using UnityEngine;
using System.Reflection;

namespace ContractsWindow
{
	/// <summary>
	/// This object is used to store locally cached data for each contract and a list of its parameters
	/// </summary>
	public class contractContainer
	{
		private Contract contract;
		private double totalReward, duration;
		private bool showNote;
		private string daysToExpire;
		private string targetPlanet;
		private string title = "";
		private string notes = "";
		private string fundsRewString, fundsPenString, repRewString, repPenString, sciRewString;
		private List<parameterContainer> paramList = new List<parameterContainer>();
		private List<parameterContainer> allParamList = new List<parameterContainer>();

		//Store info on contracts
		internal contractContainer(Contract c)
		{
			contract = c;
			showNote = false;
			title = c.Title;
			notes = c.Notes;

			if (c.DateDeadline <= 0)
			{
				duration = double.MaxValue;
				daysToExpire = "----";
			}
			else
			{
				duration = contract.DateDeadline - Planetarium.GetUniversalTime();
				//Calculate time in day values using Kerbin or Earth days
				daysToExpire = contractScenario.timeInDays(duration);
			}

			contractRewards(c);
			contractPenalties(c);

			totalReward = c.FundsCompletion;
			foreach (ContractParameter param in c.AllParameters)
				totalReward += param.FundsCompletion;

			//Generate four layers of parameters, check if each is an altitude parameter
			for (int i = 0; i < c.ParameterCount; i++)
			{
				ContractParameter param = c.GetParameter(i);
				addContractParam(param, 0);
			}

			CelestialBody t = getTargetBody();

			targetPlanet = t == null ? "" : t.name;
		}

		private void addContractParam(ContractParameter param, int Level)
		{
			string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(this, param, Level, partTest));
			allParamList.Add(paramList.Last());
		}

		private void contractRewards(Contract c)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)c.FundsCompletion, c.ScienceCompletion, c.ReputationCompletion);

			fundsRewString = "";
			if (c.FundsCompletion != 0)
				fundsRewString = "+ " + c.FundsCompletion.ToString("N0");
			float fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsRewStrat != 0f)
				fundsRewString = string.Format("+ {0:N0} ({1:N0})", c.FundsCompletion + fundsRewStrat, fundsRewStrat);

			repRewString = "";
			if (c.ReputationCompletion != 0)
				repRewString = "+ " + c.ReputationCompletion.ToString("N0");
			float repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repRewStrat != 0f)
				repRewString = string.Format("+ {0:N0} ({1:N0})", c.ReputationCompletion + repRewStrat, repRewStrat);

			sciRewString = "";
			if (c.ScienceCompletion != 0)
				sciRewString = "+ " + c.ScienceCompletion.ToString("N0");
			float sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRewString = string.Format("+ {0:N0} ({1:N0})", c.ScienceCompletion + sciRewStrat, sciRewStrat);
			}
		}

		private void contractPenalties(Contract c)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)c.FundsFailure, 0f, c.ReputationFailure);

			fundsPenString = "";
			if (c.FundsFailure != 0)
				fundsPenString = "- " + c.FundsFailure.ToString("N0");
			float fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if (fundsPenStrat != 0f)
			{
				fundsPenString = string.Format("- {0:N0} ({1:N0})", c.FundsFailure + fundsPenStrat, fundsPenStrat);
			}

			repPenString = "";
			if (c.ReputationFailure != 0)
				repPenString = "- " + c.ReputationFailure.ToString("N0");
			float repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPenString = string.Format("- {0:N0} ({1:N0})", c.ReputationFailure + repPenStrat, repPenStrat);
			}
		}

		internal void updateContractInfo()
		{
			contractRewards(contract);
			contractPenalties(contract);
		}

		internal void updateFullParamInfo()
		{
			totalReward = contract.FundsCompletion;
			foreach (ContractParameter param in contract.AllParameters)
				totalReward += param.FundsCompletion;

			//Clear out all existing parameters and regenerate new ones

			paramList.Clear();
			allParamList.Clear();

			for (int i = 0; i < contract.ParameterCount; i++)
			{
				ContractParameter param = contract.GetParameter(i);
				addContractParam(param, 0);
			}
		}

		internal void updateParameterInfo()
		{
			foreach (parameterContainer pC in allParamList)
			{
				pC.paramRewards(pC.CParam);
				pC.paramPenalties(pC.CParam);
			}
		}

		internal void updateParameterInfo(Type t)
		{
			foreach (parameterContainer pC in allParamList)
			{
				if (pC.CParam.GetType() == t)
				{
					pC.paramRewards(pC.CParam);
					pC.paramPenalties(pC.CParam);
				}
			}
		}

		internal void addToParamList(parameterContainer pC)
		{
			allParamList.Add(pC);
		}

		private CelestialBody getTargetBody()
		{
			if (contract == null)
				return null;

			bool checkTitle = false;

			Type t = contract.GetType();

			if (t == typeof(CollectScience))
				return ((CollectScience)contract).TargetBody;
			else if (t == typeof(ExploreBody))
				return ((ExploreBody)contract).TargetBody;
			else if (t == typeof(PartTest))
			{
				var fields = typeof(PartTest).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[1].GetValue((PartTest)contract) as CelestialBody;
			}
			else if (t == typeof(PlantFlag))
				return ((PlantFlag)contract).TargetBody;
			else if (t == typeof(RecoverAsset))
			{
				var fields = typeof(RecoverAsset).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[0].GetValue((RecoverAsset)contract) as CelestialBody;
			}
			else if (t == typeof(GrandTour))
				return ((GrandTour)contract).TargetBodies.LastOrDefault();
			else if (t == typeof(ARMContract))
			{
				var fields = typeof(ARMContract).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				return fields[0].GetValue((ARMContract)contract) as CelestialBody;
			}
			else if (t == typeof(BaseContract))
				return ((BaseContract)contract).targetBody;
			else if (t == typeof(ISRUContract))
				return ((ISRUContract)contract).targetBody;
			else if (t == typeof(SatelliteContract))
			{
				SpecificOrbitParameter p = contract.GetParameter<SpecificOrbitParameter>();

				if (p == null)
					return null;

				return p.TargetBody;
			}
			else if (t == typeof(StationContract))
				return ((StationContract)contract).targetBody;
			else if (t == typeof(SurveyContract))
				return ((SurveyContract)contract).targetBody;
			else if (t == typeof(TourismContract))
				return null;
			else if (t == typeof(WorldFirstContract))
			{
				ProgressTrackingParameter p = contract.GetParameter<ProgressTrackingParameter>();

				if (p == null)
					return null;

				var fields = typeof(ProgressTrackingParameter).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				var milestone = fields[0].GetValue(p) as ProgressMilestone;

				if (milestone == null)
					return null;

				return milestone.body;
			}
			else
				checkTitle = true;

			if (checkTitle)
			{
				foreach (CelestialBody b in FlightGlobals.Bodies)
				{
					string n = b.name;

					Regex r = new Regex(string.Format(@"\b{0}\b", n));

					if (r.IsMatch(title))
						return b;
				}
			}

			return null;
		}

		#region Public Accessors

		public Contract Contract
		{
			get { return contract; }
		}

		public int ParameterCount
		{
			get { return allParamList.Count; }
		}

		public double TotalReward
		{
			get { return totalReward; }
		}

		public double Duration
		{
			get { return duration; }
			internal set { duration = value; }
		}

		public bool ShowNote
		{
			get { return showNote; }
			internal set { showNote = value; }
		}

		public string DaysToExpire
		{
			get { return daysToExpire; }
			internal set { daysToExpire = value; }
		}

		public string Title
		{
			get { return title; }
			internal set { title = value; }
		}

		public string Notes
		{
			get { return notes; }
			internal set { notes = value; }
		}

		public string TargetPlanet
		{
			get { return targetPlanet; }
		}

		public string FundsRewString
		{
			get { return fundsRewString; }
		}

		public string FundsPenString
		{
			get { return fundsPenString; }
		}

		public string RepRewString
		{
			get { return repRewString; }
		}

		public string RepPenString
		{
			get { return repPenString; }
		}

		public string SciRewString
		{
			get { return sciRewString; }
		}

		public List<parameterContainer> ParamList
		{
			get { return paramList; }
		}

		public List<parameterContainer> AllParamList
		{
			get { return allParamList; }
		}

		#endregion
	}

	/// <summary>
	/// Stores locally cached data about contract parameters
	/// </summary>
	public class parameterContainer
	{
		private contractContainer root;
		private ContractParameter cParam;
		private bool showNote;
		private string title = "";
		private string notes = "";
		private int level;
		private string fundsRewString, fundsPenString, repRewString, repPenString, sciRewString;
		private AvailablePart part;
		private List<parameterContainer> paramList = new List<parameterContainer>();

		internal parameterContainer(contractContainer Root, ContractParameter cP, int Level, string PartTestName)
		{
			root = Root;
			cParam = cP;
			showNote = false;
			level = Level;
			paramRewards(cP);
			paramPenalties(cP);
			title = cParam.Title;
			notes = cParam.Notes;

			if (level < 4)
			{
				for (int i = 0; i < cParam.ParameterCount; i++)
				{
					ContractParameter param = cParam.GetParameter(i);
					addSubParam(param, level + 1);
				}
			}

			if (!string.IsNullOrEmpty(PartTestName))
			{
				if (PartTestName == "partTest")
				{
					part = ((Contracts.Parameters.PartTest)cParam).tgtPartInfo;
					DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Stock Part Test");
				}
				else if (PartTestName == "MCEScience")
				{
					if (contractAssembly.MCELoaded)
					{
						part = PartLoader.Instance.parts.FirstOrDefault(p => p.partPrefab.partInfo.title == contractAssembly.MCEPartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Mission Controller Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "DMcollectScience")
				{
					if (contractAssembly.DMLoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicSciencePartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "DManomalyScience")
				{
					if (contractAssembly.DMALoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicAnomalySciencePartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Anomaly Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "DMasteroidScience")
				{
					if (contractAssembly.DMAstLoaded)
					{
						part = PartLoader.getPartInfoByName(contractAssembly.DMagicAsteroidSciencePartName(cParam));
						if (part != null)
							DMC_MBE.LogFormatted_DebugOnly("Part Assigned For DMagic Asteroid Contract");
						else
							DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
					}
				}
				else if (PartTestName == "FinePrint")
				{
					part = PartLoader.getPartInfoByName(contractAssembly.FPPartName(cParam));
					if (part != null)
						DMC_MBE.LogFormatted_DebugOnly("Part Assigned For Fine Print Contract");
					else
						DMC_MBE.LogFormatted_DebugOnly("Part Not Found");
				}
				else
					part = null;
			}
		}

		internal void paramRewards(ContractParameter cP)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractReward, (float)cP.FundsCompletion, cP.ScienceCompletion, cP.ReputationCompletion);

			fundsRewString = "";
			if (cP.FundsCompletion != 0)
				fundsRewString = "+ " + cP.FundsCompletion.ToString("N0");
			float fundsRewStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if(fundsRewStrat != 0f)
			{
				fundsRewString = string.Format("+ {0:N0} ({1:N0})", cP.FundsCompletion + fundsRewStrat, fundsRewStrat);
			}

			repRewString = "";
			if (cP.ReputationCompletion != 0)
				repRewString = "+ " + cP.ReputationCompletion.ToString("N0");
			float repRewStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if(repRewStrat != 0f)
			{
				repRewString = string.Format("+ {0:N0} ({1:N0})", cP.ReputationCompletion + repRewStrat, repRewStrat);
			}

			sciRewString = "";
			if (cP.ScienceCompletion != 0)
				sciRewString = "+ " + cP.ScienceCompletion.ToString("N0");
			float sciRewStrat = currencyQuery.GetEffectDelta(Currency.Science);
			if (sciRewStrat != 0f)
			{
				sciRewString = string.Format("+ {0:N0} ({1:N0})", cP.ScienceCompletion + sciRewStrat, sciRewStrat);
			}
		}

		internal void paramPenalties(ContractParameter cP)
		{
			CurrencyModifierQuery currencyQuery = CurrencyModifierQuery.RunQuery(TransactionReasons.ContractPenalty, (float)cP.FundsFailure, 0f, cP.ReputationFailure);

			fundsPenString = "";
			if (cP.FundsFailure != 0)
				fundsPenString = "- " + cP.FundsFailure.ToString("N0");
			float fundsPenStrat = currencyQuery.GetEffectDelta(Currency.Funds);
			if(fundsPenStrat != 0f)
			{
				fundsPenString = string.Format("- {0:N0} ({1:N0})", cP.FundsFailure + fundsPenStrat, fundsPenStrat);
			}

			repPenString = "";
			if (cP.ReputationFailure != 0)
				repPenString = "- " + cP.ReputationFailure.ToString("N0");
			float repPenStrat = currencyQuery.GetEffectDelta(Currency.Reputation);
			if (repPenStrat != 0f)
			{
				repPenString = string.Format("- {0:N0} ({1:N0})", cP.ReputationFailure + repPenStrat, repPenStrat);
			}
		}

		private void addSubParam(ContractParameter param, int Level)
		{
			string partTest = contractScenario.paramTypeCheck(param);
			paramList.Add(new parameterContainer(root, param, Level, partTest));
			root.addToParamList(paramList.Last());
		}

		#region Public Accessors

		public ContractParameter CParam
		{
			get { return cParam; }
		}

		public bool ShowNote
		{
			get { return showNote; }
			internal set { showNote = value; }
		}

		public string Title
		{
			get { return title; }
			internal set { title = value; }
		}

		public string Notes
		{
			get { return notes; }
			internal set { notes = value; }
		}

		public int Level
		{
			get { return level; }
		}

		public string FundsRewString
		{
			get { return fundsRewString; }
		}

		public string FundsPenString
		{
			get { return fundsPenString; }
		}

		public string RepRewString
		{
			get { return repRewString; }
		}

		public string RepPenString
		{
			get { return repPenString; }
		}

		public string SciRewString
		{
			get { return sciRewString; }
		}

		public AvailablePart Part
		{
			get { return part; }
		}

		public List<parameterContainer> ParamList
		{
			get { return paramList; }
		}

		#endregion

	}
}
