using System.IO;
using System.Reflection;
using UnityEngine;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class contractLoader : MonoBehaviour
	{
		private static AssetBundle prefabs;

		public static AssetBundle Prefabs
		{
			get { return prefabs; }
		}

		private void Awake()
		{
			string path = KSPUtil.ApplicationRootPath + "GameData/DMagicUtilities/ContractsWindow/Resources";

			prefabs = AssetBundle.LoadFromFile(path + "/contracts_window_prefabs.ksp");
		}
	}
}
