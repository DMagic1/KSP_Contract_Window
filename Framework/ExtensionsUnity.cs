/* Part of KSPPluginFramework
Version 1.2

Forum Thread:http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework
Author: TriggerAu, 2014
License: The MIT License (MIT)
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

namespace ContractsWindow
{
    /// <summary>
    /// CLass containing some extension methods for Unity Objects
    /// </summary>
    public static class DMC_UnityExtensions
    {
        /// <summary>
        /// Ensure that the Rect remains within the screen bounds
        /// </summary>
        public static Rect ClampToScreen(this Rect r)
        {
            return r.ClampToScreen(new RectOffset(0, 0, 0, 0));
        }

        /// <summary>
        /// Ensure that the Rect remains within the screen bounds
        /// </summary>
        /// <param name="ScreenBorder">A Border to the screen bounds that the Rect will be clamped inside (can be negative)</param>
        public static Rect ClampToScreen(this Rect r, RectOffset ScreenBorder)
        {
            r.x = Mathf.Clamp(r.x, ScreenBorder.left, Screen.width - r.width - ScreenBorder.right);
            r.y = Mathf.Clamp(r.y, ScreenBorder.top, Screen.height - r.height - ScreenBorder.bottom);
            return r;
        }

        public static GUIStyle PaddingChange(this GUIStyle g, Int32 PaddingValue)
        {
            GUIStyle gReturn = new GUIStyle(g);
            gReturn.padding = new RectOffset(PaddingValue, PaddingValue, PaddingValue, PaddingValue);
            return gReturn;
        }
        public static GUIStyle PaddingChangeBottom(this GUIStyle g, Int32 PaddingValue)
        {
            GUIStyle gReturn = new GUIStyle(g);
            gReturn.padding.bottom = PaddingValue;
            return gReturn;
        }
		public static float Mathf_Round(this float f, int precision)
		{
			if (precision < -4 || precision > 4)
				throw new ArgumentOutOfRangeException("[Contracts +] Precision Must Be Between -4 And 4 For Rounding Operation");

			if (precision >= 0)
				return (float)Math.Round(f, precision);
			else
			{
				precision = (int)Math.Pow(10, Math.Abs(precision));
				if (f >= 0)
					f += (5 * precision / 10);
				else
					f -= (5 * precision / 10);
				return (float)Math.Round(f - (f % precision), 0);
			}
		}

		//public static float reverseLog(this float f)
		//{
		//	if (f <= 0.009 && !contractScenario.Instance.allowZero)
		//		return -1;
		//	else if (f >= 0.000 && f < 0.95)
		//		return f - 1;
		//	else if (f >= 0.95 && f < 1.05)
		//		return 0;
		//	else if (f > 1.05 && f < 10)
		//		return (float)Math.Log10(f);
		//	else
		//		return 10;
		//}
    }
}