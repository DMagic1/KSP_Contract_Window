

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
			FPLoaded = FPAssemblyLoaded();
		}

		internal static bool DMLoaded, DMALoaded, FPLoaded;

		private const string DMCollectType = "DMagic.DMCollectScience";
		private const string DMAnomalyType = "DMagic.DMAnomalyParameter";
		private const string FPContractType = "FinePrint.Contracts.Parameters.PartNameParameter";

		private static bool DMCRun, DMARun, FPRun = false;

		private delegate string DMCollectSci(ContractParameter cP);
		private delegate string DMAnomalySci(ContractParameter cP);

		private static DMCollectSci _DMCollect;
		private static DMAnomalySci _DMAnomaly;

		private static MethodInfo DMcollectMethod;
		private static MethodInfo DManomalyMethod;

		internal static Type _DMCType;
		internal static Type _DMAType;
		internal static Type _FPType;

		internal static string DMagicSciencePartName(ContractParameter cParam)
		{
			return _DMCollect(cParam);
		}

		internal static string DMagicAnomalySciencePartName(ContractParameter cParam)
		{
			return _DMAnomaly(cParam);
		}

		private static bool FPAssemblyLoaded()
		{
			if (FPRun)
				return false;

			FPRun = true;

			Type FPType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
				.SingleOrDefault(t => t.FullName == FPContractType);

			if (FPType == null)
			{
				MonoBehaviourExtended.LogFormatted_DebugOnly("Fine Print Type Not Found");
				return false;
			}
			_FPType = FPType;
			return true;
		}

		private static bool DMScienceAvailable()
		{
			if (DMcollectMethod != null && _DMCollect != null)
				return true;

			if (DMCRun)
				return false;

			DMCRun = true;

			try
			{
				Type DMType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == DMCollectType);

				if (DMType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Type Not Found");
					return false;
				}

				_DMCType = DMType;

				DMcollectMethod = DMType.GetMethod("PartName", new Type[] { typeof(ContractParameter) });

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
			if (DManomalyMethod != null && _DMAnomaly != null)
				return true;

			if (DMARun)
				return false;

			DMARun = true;

			try
			{
				Type DMAType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == DMAnomalyType);

				if (DMAType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Anomaly Type Not Found");
					return false;
				}

				_DMAType = DMAType;

				DManomalyMethod = DMAType.GetMethod("PartName");

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

	}
}
