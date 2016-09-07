using System.IO;
using System.Reflection;
using UnityEngine;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class contractLoader : MonoBehaviour
	{
		private static AssetBundle images;
		private static AssetBundle prefabs;

		public static AssetBundle Images
		{
			get { return images; }
		}

		public static AssetBundle Prefabs
		{
			get { return prefabs; }
		}

		private void Awake()
		{
			string path = KSPUtil.ApplicationRootPath + "GameData/DMagicUtilities/ContractsWindow/Resources";

			images = AssetBundle.CreateFromFile(path + "/cw_images.ksp");
			prefabs = AssetBundle.CreateFromFile(path + "/cw_prefabs.ksp");
		}
	}
}
