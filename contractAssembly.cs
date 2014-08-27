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
			//FPLoaded = FPAssemblyLoaded();
		}

		internal static bool DMLoaded, DMALoaded, DMAstLoaded; //, FPLoaded;

		private const string DMCollectType = "DMagic.DMCollectScience";
		private const string DMAnomalyType = "DMagic.DMAnomalyParameter";
		private const string DMAsteroidType = "DMagic.DMAsteroidParameter";
		//private const string FPContractType = "FinePrint.Contracts.Parameters.PartNameParameter";

		private static bool DMCRun, DMARun, DMAstRun = false; //, FPRun = false;

		private delegate string DMCollectSci(ContractParameter cP);
		private delegate string DMAnomalySci(ContractParameter cP);
		private delegate string DMAstSci(ContractParameter cP);

		private static DMCollectSci _DMCollect;
		private static DMAnomalySci _DMAnomaly;
		private static DMAstSci _DMAst;

		private static MethodInfo DMcollectMethod;
		private static MethodInfo DManomalyMethod;
		private static MethodInfo DMastMethod;

		internal static Type _DMCType;
		internal static Type _DMAType;
		internal static Type _DMAstType;
		//internal static Type _FPType;

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

		//private static bool FPAssemblyLoaded()
		//{
		//    if (FPRun)
		//        return false;

		//    FPRun = true;

		//    Type FPType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
		//        .SingleOrDefault(t => t.FullName == FPContractType);

		//    if (FPType == null)
		//    {
		//        MonoBehaviourExtended.LogFormatted_DebugOnly("Fine Print Type Not Found");
		//        return false;
		//    }
		//    _FPType = FPType;
		//    return true;
		//}

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

		private static bool DMAsteroidAvailable()
		{
			if (DMastMethod != null && _DMAst != null)
				return true;

			if (DMAstRun)
				return false;

			DMAstRun = true;

			try
			{
				Type DMAstType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
					.SingleOrDefault(t => t.FullName == DMAsteroidType);

				if (DMAstType == null)
				{
					MonoBehaviourExtended.LogFormatted_DebugOnly("DMagic Asteroid Type Not Found");
					return false;
				}

				_DMAstType = DMAstType;

				DMastMethod = DMAstType.GetMethod("PartName");

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
