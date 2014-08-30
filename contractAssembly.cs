#region license
/*The MIT License (MIT)
Contract Assembly - Monobehaviour To Check For Other Addons And Their Methods

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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Contracts;
using Contracts.Parameters;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	internal class contractAssembly : MonoBehaviour
	{

		private void Start()
		{
			DMLoaded = DMScienceAvailable();
			DMALoaded = DMAnomalyAvailable();
			DMAstLoaded = DMAsteroidAvailable();
			MCELoaded = MCEAvailable();
			FPLoaded = FPAssemblyLoaded();
		}

		internal static bool DMLoaded, DMALoaded, DMAstLoaded, MCELoaded, FPLoaded;

		private const string DMCollectTypeName = "DMagic.DMCollectScience";
		private const string DMAnomalyTypeName = "DMagic.DMAnomalyParameter";
		private const string DMAsteroidTypeName = "DMagic.DMAsteroidParameter";
		private const string MCETypeName = "MissionControllerEC.PartGoal";
		private const string FPContractTypeName = "FinePrint.Contracts.Parameters.PartNameParameter";

		private static bool DMCRun, DMARun, DMAstRun, MCERun, FPRun = false;

		private delegate string DMCollectSci(ContractParameter cP);
		private delegate string DMAnomalySci(ContractParameter cP);
		private delegate string DMAstSci(ContractParameter cP);
		private delegate string MCESci(ContractParameter cP);
		private delegate string FPSci(ContractParameter cP);

		private static DMCollectSci _DMCollect;
		private static DMAnomalySci _DMAnomaly;
		private static DMAstSci _DMAst;
		private static MCESci _MCE;
		private static FPSci _FP;

		internal static Type _DMCType;
		internal static Type _DMAType;
		internal static Type _DMAstType;
		internal static Type _MCEType;
		internal static Type _FPType;

		internal static string DMagicSciencePartName(ContractParameter cParam)
		{
			return _DMCollect(cParam);
		}

		internal static string DMagicAnomalySciencePartName(ContractParameter cParam)
		{
			return _DMAnomaly(cParam);
		}

		internal static string DMagicAsteroidSciencePartName(ContractParameter cParam)
		{
			return _DMAst(cParam);
		}

		internal static string MCEPartName(ContractParameter cParam)
		{
			return _MCE(cParam);
		}

		internal static string FPPartName(ContractParameter cParam)
		{
			return _FP(cParam);
		}

		private static bool FPAssemblyLoaded()
		{
			if (_FP != null)
				return true;

			if (FPRun)
				return false;

			FPRun = true;

			try
			{
				Type FPType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == FPContractTypeName);

				if (FPType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("Fine Print Type Not Found");
					return false;
				}

				_FPType = FPType;

				MethodInfo FPMethod = FPType.GetMethod("PartName", new Type[] { typeof(ContractParameter) });

				if (FPMethod == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("Fine Print String Method Not Found");
					return false;
				}

				_FP = (FPSci)Delegate.CreateDelegate(typeof(FPSci), FPMethod);
				MonoBehaviourExtended.LogFormatted_DebugOnly("Reflection Method Assigned");

				return _FP != null;
			}
			catch (Exception e)
			{
				MonoBehaviourExtended.LogFormatted("Exception While Loading Fine Print Accessor: {0}", e);
			}

			return false;
		}

		private static bool MCEAvailable()
		{
			if (_MCE != null)
				return true;

			if (MCERun)
				return false;

			MCERun = true;

			try
			{
				Type MCEType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == MCETypeName);

				if (MCEType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("Mision Controller Type Not Found");
					return false;
				}

				_MCEType = MCEType;

				MethodInfo MCEMethod = MCEType.GetMethod("PartName", new Type[] { typeof(ContractParameter) });

				if (MCEMethod == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("Mission Controller String Method Not Found");
					return false;
				}

				_MCE = (MCESci)Delegate.CreateDelegate(typeof(MCESci), MCEMethod);
				MonoBehaviourExtended.LogFormatted_DebugOnly("Reflection Method Assigned");

				return _MCE != null;
			}
			catch (Exception e)
			{
				MonoBehaviourExtended.LogFormatted("Exception While Loading Mission Controller Accessor: {0}", e);
			}

			return false;
		}

		private static bool DMScienceAvailable()
		{
			if (_DMCollect != null)
				return true;

			if (DMCRun)
				return false;

			DMCRun = true;

			try
			{
				Type DMType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == DMCollectTypeName);

				if (DMType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Type Not Found");
					return false;
				}

				_DMCType = DMType;

				MethodInfo DMcollectMethod = DMType.GetMethod("PartName", new Type[] { typeof(ContractParameter) });

				if (DMcollectMethod == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic String Method Not Found");
					return false;
				}
				else
				{
					_DMCollect = (DMCollectSci)Delegate.CreateDelegate(typeof(DMCollectSci), DMcollectMethod);
					MonoBehaviourExtended.LogFormatted_DebugOnly("Reflection Method Assigned");
				}

				return _DMCollect != null;
			}
			catch (Exception e)
			{
				MonoBehaviourExtended.LogFormatted("Exception While Loading DMagic Accessor: {0}", e);
			}

			return false;
		}

		private static bool DMAnomalyAvailable()
		{
			if (_DMAnomaly != null)
				return true;

			if (DMARun)
				return false;

			DMARun = true;

			try
			{
				Type DMAType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == DMAnomalyTypeName);

				if (DMAType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Anomaly Type Not Found");
					return false;
				}

				_DMAType = DMAType;

				MethodInfo DManomalyMethod = DMAType.GetMethod("PartName");

				if (DManomalyMethod == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Anomaly String Method Not Found");
					return false;
				}
				else
				{
					_DMAnomaly = (DMAnomalySci)Delegate.CreateDelegate(typeof(DMAnomalySci), DManomalyMethod);
					MonoBehaviourExtended.LogFormatted_DebugOnly("Reflection Method Assigned");
				}

				return _DMAnomaly != null;
			}
			catch (Exception e)
			{
				MonoBehaviourExtended.LogFormatted("Exception While Loading DMagic Anomaly Accessor: {0}", e);
			}

			return false;
		}

		private static bool DMAsteroidAvailable()
		{
			if (_DMAst != null)
				return true;

			if (DMAstRun)
				return false;

			DMAstRun = true;

			try
			{
				Type DMAstType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == DMAsteroidTypeName);

				if (DMAstType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Asteroid Type Not Found");
					return false;
				}

				_DMAstType = DMAstType;

				MethodInfo DMastMethod = DMAstType.GetMethod("PartName");

				if (DMastMethod == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Asteroid String Method Not Found");
					return false;
				}
				else
				{
					_DMAst = (DMAstSci)Delegate.CreateDelegate(typeof(DMAstSci), DMastMethod);
					MonoBehaviourExtended.LogFormatted_DebugOnly("Asteroid Reflection Method Assigned");
				}

				return _DMAst != null;
			}
			catch (Exception e)
			{
				MonoBehaviourExtended.LogFormatted("Exception While Loading DMagic Asteroid Accessor: {0}", e);
			}

			return false;
		}

	}
}
