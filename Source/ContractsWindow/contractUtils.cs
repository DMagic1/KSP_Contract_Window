#region license
/*The MIT License (MIT)
Contract Utilities - Public utilities for accessing and altering internal information

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
using TMPro;
using Contracts;
using ContractsWindow.PanelInterfaces;
using ContractsWindow.Unity;
using ContractsWindow.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;
using ContractParser;

namespace ContractsWindow
{
	/// <summary>
	/// A static helper class intended primarily for use by external assemblies through reflection
	/// </summary>
	public static class contractUtils
	{
		/// <summary>
		/// A method for manually resetting a locally cached contract title.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <param name="name">The new contract title</param>
		public static void setContractTitle (Contract contract, string name)
		{
			try
			{
				if (string.IsNullOrEmpty(name))
					return;
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				if (c != null)
				{
					c.Title = name;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Contract Title: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting locally cached contract notes.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <param name="notes">The new contract notes</param>
		public static void setContractNotes (Contract contract, string notes)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				if (c != null)
				{
					c.Notes = notes;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign new Contract Notes: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting a locally cached contract parameter title.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <param name="name">The new contract parameter title</param>
		public static void setParameterTitle (Contract contract, ContractParameter parameter, string name)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				if (c != null)
				{
					parameterContainer pC = c.AllParamList.SingleOrDefault(a => a.CParam == parameter);
					if (pC != null)
					{
						pC.Title = name;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Parameter Title: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting locally cached contract parameter notes.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <param name="notes">The new contract parameter notes</param>
		public static void setParameterNotes(Contract contract, ContractParameter parameter, string notes)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				if (c != null)
				{
					parameterContainer pC = c.AllParamList.SingleOrDefault(a => a.CParam == parameter);
					if (pC != null)
					{
						pC.setNotes(notes);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Parameter Notes: " + e);
			}
		}

		/// <summary>
		/// A method for returning a contractContainer object. The contract in question must be loaded by Contracts
		/// Window + and may return null. All fields within the object are publicly accessible through properties.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <returns>contractContainer object</returns>
		public static contractContainer getContractContainer(Contract contract)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				return c;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to get a Contract Container object: " + e);
				return null;
			}
		}

		/// <summary>
		/// A method for returning a parameterContainer object. The contract and parameter in question must be loaded by 
		/// Contracts Window + and may return null. All fields within the object are publicly accessible through properties.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <returns>parameterContainer object</returns>
		public static parameterContainer getParameterContainer(Contract contract, ContractParameter parameter)
		{
			try
			{
				contractContainer c = contractParser.getActiveContract(contract.ContractGuid);
				parameterContainer pC = null;
				if (c != null)
					pC = c.AllParamList.SingleOrDefault(a => a.CParam == parameter);
				return pC;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to get a Parameter Container object: " + e);
				return null;
			}
		}

		/// <summary>
		/// A method for updating all reward values for active contracts
		/// </summary>
		/// <param name="contractType">Type of contract that needs to be updated; must be a subclass of Contracts.Contract</param>
		public static void UpdateContractType(Type contractType)
		{
			if (contractType == null)
			{
				Debug.LogWarning("[Contracts +] Type provided for update contract method is null");
				return;
			}
			if (contractScenario.Instance == null)
			{
				Debug.LogWarning("[Contracts +] Contracts Window + scenario module is not loaded");
				return;
			}

			try
			{
				contractScenario.Instance.contractChanged(contractType);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Error while updating contract values: " + e);
			}
		}

		/// <summary>
		/// A method for updating contract parameter reward values for active contracts
		/// </summary>
		/// <param name="parameterType">Type of parameter that needs to be updated; must be a subclass of Contracts.ContractParameter</param>
		public static void UpdateParameterType(Type parameterType)
		{
			if (parameterType == null)
			{
				Debug.LogWarning("[Contracts +] Type provided for update parameter method is null");
				return;
			}
			if (contractScenario.Instance == null)
			{
				Debug.LogWarning("[Contracts +] Contracts Window + scenario module is not loaded");
				return;
			}

			try
			{
				contractScenario.Instance.paramChanged(parameterType);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Error while updating parameter values: " + e);
			}
		}

		public static void processComponents(GameObject obj)
		{
			if (obj == null)
				return;

			TextHandler[] handlers = obj.GetComponentsInChildren<TextHandler>(true);

			if (handlers == null)
				return;

			for (int i = 0; i < handlers.Length; i++)
				TMProFromText(handlers[i]);
		}

		private static void TMProFromText(TextHandler handler)
		{
			if (handler == null)
				return;

			Text text = handler.GetComponent<Text>();

			if (text == null)
				return;

			string t = text.text;
			Color c = text.color;
			int i = text.fontSize;
			bool r = text.raycastTarget;

			FontStyles sty = getStyle(text.fontStyle);
			TextAlignmentOptions align = getAnchor(text.alignment);

			float spacing = text.lineSpacing;

			GameObject obj = text.gameObject;

			MonoBehaviour.DestroyImmediate(text);

			CWTextMeshProHolder tmp = obj.AddComponent<CWTextMeshProHolder>();

			tmp.text = t;
			tmp.color = c;
			tmp.fontSize = i;
			tmp.raycastTarget = r;

			tmp.alignment = align;
			tmp.fontStyle = sty;
			tmp.lineSpacing = spacing;

			tmp.font = Resources.Load("Fonts/Calibri SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
			tmp.fontSharedMaterial = Resources.Load("Fonts/Materials/Calibri Dropshadow", typeof(Material)) as Material;

			tmp.enableWordWrapping = true;
			tmp.isOverlay = false;
			tmp.richText = true;

			tmp.Setup(handler);
		}

		private static FontStyles getStyle(FontStyle style)
		{
			switch(style)
			{
				case FontStyle.Normal:
					return FontStyles.Normal;
				case FontStyle.Bold:
					return FontStyles.Bold;
				case FontStyle.Italic:
					return FontStyles.Italic;
				case FontStyle.BoldAndItalic:
					return FontStyles.Bold;
				default:
					return FontStyles.Normal;
			}
		}

		private static TextAlignmentOptions getAnchor(TextAnchor anchor)
		{
			switch (anchor)
			{
				case TextAnchor.UpperLeft:
					return TextAlignmentOptions.TopLeft;
				case TextAnchor.UpperCenter:
					return TextAlignmentOptions.Top;
				case TextAnchor.UpperRight:
					return TextAlignmentOptions.TopRight;
				case TextAnchor.MiddleLeft:
					return TextAlignmentOptions.MidlineLeft;
				case TextAnchor.MiddleCenter:
					return TextAlignmentOptions.Midline;
				case TextAnchor.MiddleRight:
					return TextAlignmentOptions.MidlineRight;
				case TextAnchor.LowerLeft:
					return TextAlignmentOptions.BottomLeft;
				case TextAnchor.LowerCenter:
					return TextAlignmentOptions.Bottom;
				case TextAnchor.LowerRight:
					return TextAlignmentOptions.BottomRight;
				default:
					return TextAlignmentOptions.Center;
			}
		}

		//private static void processCompenents(CW_Style style)
		//{
		//	if (style == null)
		//		return;

		//	UISkinDef skin = UISkinManager.defaultSkin;

		//	if (skin == null)
		//		return;

		//	switch (style.ElementType)
		//	{
		//		case CW_Style.ElementTypes.Text:
		//			ProcessTMPro(style, false);
		//			break;
		//		case CW_Style.ElementTypes.TextUpdated:
		//			ProcessTMPro(style, true);
		//			break;
		//		default:
		//			break;
		//	}
		//}
	}
}
