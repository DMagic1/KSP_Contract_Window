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
using FinePrint.Contracts.Parameters;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	internal class contractAssembly : MonoBehaviour
	{

		private void Start()
		{
			//DMLoaded = DMScienceAvailable();
			//DMALoaded = DMAnomalyAvailable();
			//DMAstLoaded = DMAsteroidAvailable();
			//MCELoaded = MCEAvailable();
			FPPartLoaded = FPPartListLoaded();
			FPModLoaded = FPModListLoaded();
			FPVesselSystemsLoaded = FPVesSysListLoaded();
			DMLoaded = DMALoaded = DMAstLoaded = MCELoaded = false;
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		internal static string Version;

		internal static bool DMLoaded, DMALoaded, DMAstLoaded, MCELoaded, FPPartLoaded, FPModLoaded, FPVesselSystemsLoaded;

		private const string DMCollectTypeName = "DMagic.DMCollectScience";
		private const string DMAnomalyTypeName = "DMagic.DMAnomalyParameter";
		private const string DMAsteroidTypeName = "DMagic.DMAsteroidParameter";
		private const string MCETypeName = "MissionControllerEC.PartGoal";

		private static bool DMCRun, DMARun, DMAstRun, MCERun, FPPartRun, FPModRun, FPVesRun;

		private delegate string DMCollectSci(ContractParameter cP);
		private delegate string DMAnomalySci(ContractParameter cP);
		private delegate string DMAstSci(ContractParameter cP);
		private delegate string MCESci(ContractParameter cP);

		private static DMCollectSci _DMCollect;
		private static DMAnomalySci _DMAnomaly;
		private static DMAstSci _DMAst;
		private static MCESci _MCE;
		private static FieldInfo _FPPartList;
		private static FieldInfo _FPModList;
		private static FieldInfo _FPSysList;

		internal static Type _DMCType;
		internal static Type _DMAType;
		internal static Type _DMAstType;
		internal static Type _MCEType;

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

		internal static List<string> FPPartRequestList(PartRequestParameter p)
		{
			List<string> l = new List<string>();

			try
			{
				l = (List<string>)_FPPartList.GetValue(p);
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error In Detecting Fine Print Part Request Part List Field\n{0}", e);
			}

			return l;
		}

		internal static List<string> FPModuleRequestList(PartRequestParameter p)
		{
			List<string> l = new List<string>();

			try
			{
				l = (List<string>)_FPModList.GetValue(p);
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error In Detecting Fine Print Part Request Module List Field\n{0}", e);
			}

			return l;
		}

		internal static List<string> FPVesselSystemsList(VesselSystemsParameter p)
		{
			List<string> l = new List<string>();

			try
			{
				l = (List<string>)_FPSysList.GetValue(p);
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error In Detecting Fine Print Vessel Systems Module List Field\n{0}", e);
			}

			return l;
		}


		private static bool FPPartListLoaded()
		{
			if (_FPPartList != null)
				return true;

			if (FPPartRun)
				return false;

			FPPartRun = true;

			try
			{
				Type fpType = typeof(PartRequestParameter);

				var field = fpType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				_FPPartList = field[4];

				if (_FPPartList == null)
				{
					DMC_MBE.LogFormatted("Fine Print Part Request Part List Field Could Not Be Assigned...");
					return false;
				}

				DMC_MBE.LogFormatted("Fine Print Part Request Part List Field Assigned...");
				return true;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error In Assigning Fine Print Part Request Part List Field\n{0}", e);
			}

			return false;
		}

		private static bool FPModListLoaded()
		{
			if (_FPModList != null)
				return true;

			if (FPModRun)
				return false;

			FPModRun = true;

			try
			{
				Type fpType = typeof(PartRequestParameter);

				var field = fpType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				_FPModList = field[5];

				if (_FPModList == null)
				{
					DMC_MBE.LogFormatted("Fine Print Part Request Module List Field Could Not Be Assigned...");
					return false;
				}

				DMC_MBE.LogFormatted("Fine Print Part Request Module List Field Assigned...");
				return true;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error In Assigning Fine Print Part Request Module List Field\n{0}", e);
			}

			return false;
		}

		private static bool FPVesSysListLoaded()
		{
			if (_FPSysList != null)
				return true;

			if (FPVesRun)
				return false;

			FPVesRun = true;

			try
			{
				Type fpType = typeof(VesselSystemsParameter);

				var field = fpType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

				_FPSysList = field[0];

				if (_FPSysList == null)
				{
					DMC_MBE.LogFormatted("Fine Print Vessel Systems Module List Field Could Not Be Assigned...");
					return false;
				}

				DMC_MBE.LogFormatted("Fine Print Vessel Systems Module List Field Assigned...");
				return true;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Error In Assigning Fine Print Vessel Systems Module List Field\n{0}", e);
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
					DMC_MBE.LogFormatted_DebugOnly("Mision Controller Type Not Found");
					return false;
				}

				_MCEType = MCEType;

				MethodInfo MCEMethod = MCEType.GetMethod("iPartName", new Type[] { typeof(ContractParameter) });

				if (MCEMethod == null)
				{
					DMC_MBE.LogFormatted_DebugOnly("Mission Controller String Method Not Found");
					return false;
				}

				_MCE = (MCESci)Delegate.CreateDelegate(typeof(MCESci), MCEMethod);
				DMC_MBE.LogFormatted("Mission Control Reflection Method Assigned");

				return _MCE != null;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Exception While Loading Mission Controller Accessor: {0}", e);
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
					DMC_MBE.LogFormatted_DebugOnly("DMagic Type Not Found");
					return false;
				}

				_DMCType = DMType;

				MethodInfo DMcollectMethod = DMType.GetMethod("PartName", new Type[] { typeof(ContractParameter) });

				if (DMcollectMethod == null)
				{
					DMC_MBE.LogFormatted_DebugOnly("DMagic String Method Not Found");
					return false;
				}
				else
				{
					_DMCollect = (DMCollectSci)Delegate.CreateDelegate(typeof(DMCollectSci), DMcollectMethod);
					DMC_MBE.LogFormatted("DMagic Standard Reflection Method Assigned");
				}

				return _DMCollect != null;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Exception While Loading DMagic Accessor: {0}", e);
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
					DMC_MBE.LogFormatted_DebugOnly("DMagic Anomaly Type Not Found");
					return false;
				}

				_DMAType = DMAType;

				MethodInfo DManomalyMethod = DMAType.GetMethod("PartName");

				if (DManomalyMethod == null)
				{
					DMC_MBE.LogFormatted_DebugOnly("DMagic Anomaly String Method Not Found");
					return false;
				}
				else
				{
					_DMAnomaly = (DMAnomalySci)Delegate.CreateDelegate(typeof(DMAnomalySci), DManomalyMethod);
					DMC_MBE.LogFormatted("DMagic Anomaly Reflection Method Assigned");
				}

				return _DMAnomaly != null;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Exception While Loading DMagic Anomaly Accessor: {0}", e);
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
					DMC_MBE.LogFormatted_DebugOnly("DMagic Asteroid Type Not Found");
					return false;
				}

				_DMAstType = DMAstType;

				MethodInfo DMastMethod = DMAstType.GetMethod("PartName");

				if (DMastMethod == null)
				{
					DMC_MBE.LogFormatted_DebugOnly("DMagic Asteroid String Method Not Found");
					return false;
				}
				else
				{
					_DMAst = (DMAstSci)Delegate.CreateDelegate(typeof(DMAstSci), DMastMethod);
					DMC_MBE.LogFormatted("DMagic Asteroid Reflection Method Assigned");
				}

				return _DMAst != null;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Exception While Loading DMagic Asteroid Accessor: {0}", e);
			}

			return false;
		}

	}
}
