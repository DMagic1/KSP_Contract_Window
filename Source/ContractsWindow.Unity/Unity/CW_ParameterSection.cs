#region license
/*The MIT License (MIT)
CW_ParameterSection - Controls contract parameter UI elements

Copyright (c) 2016 DMagic

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
using UnityEngine;
using UnityEngine.UI;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_ParameterSection : MonoBehaviour
	{
		[SerializeField]
		private TextHandler ParameterText = null;
		[SerializeField]
		private TextHandler ParameterRewardText = null;
		[SerializeField]
		private TextHandler ParameterPenaltyText = null;
		[SerializeField]
		private Toggle NoteToggle = null;
		[SerializeField]
		private GameObject NotePrefab = null;
		[SerializeField]
		private Transform NoteTransform = null;
		[SerializeField]
		private Transform SubParamTransform = null;
		[SerializeField]
		private LayoutElement Spacer = null;
		[SerializeField]
		private LayoutElement ParameterLayout = null;
		[SerializeField]
		private TooltipHandler NoteTooltip = null;

		private Color textColor = new Color(0.9921569f, 0.9921569f, 0.9960784f, 1f);
		private Color successColor = new Color(0.4117647f, 0.8470588f, 0.3098039f, 1f);
		private Color failColor = new Color(0.8980392f, 0f, 0f, 1f);
		private Color subColor = new Color(0.8470588f, 0.8627451f, 0.8392157f, 1f);
		private ContractState oldState;
		private IParameterSection parameterInterface;
		private CW_Note note;
		private List<CW_ParameterSection> parameters = new List<CW_ParameterSection>();
		private CW_ContractSection root;

		public void setParameter(IParameterSection section, CW_ContractSection c)
		{
			if (section == null)
				return;

			if (ParameterText == null || ParameterRewardText == null || ParameterPenaltyText == null)
				return;

			if (Spacer == null || ParameterLayout == null)
				return;

			if (c == null)
				return;

			root = c;

			parameterInterface = section;

			Spacer.minWidth = parameterInterface.ParamLayer * 5;

			ParameterLayout.minWidth -= parameterInterface.ParamLayer * 5;
			ParameterLayout.preferredWidth = ParameterLayout.minWidth;

			ParameterText.OnTextUpdate.Invoke(parameterInterface.TitleText);

			ParameterRewardText.OnTextUpdate.Invoke(parameterInterface.RewardText);

			ParameterPenaltyText.OnTextUpdate.Invoke(parameterInterface.PenaltyText);

			oldState = parameterInterface.ParameterState;

			if (!string.IsNullOrEmpty(parameterInterface.GetNote))
				setNote();
			else if (NoteToggle != null)
				NoteToggle.gameObject.SetActive(false);

			if (parameterInterface.ParamLayer < 4)
				CreateSubParameters(parameterInterface.GetSubParams);
		}

		public void UpdateParameter()
		{
			if (parameterInterface == null)
				return;

			if (parameterInterface.ParameterState != ContractState.Complete)
			{
				for (int i = parameters.Count - 1; i >= 0; i--)
				{
					CW_ParameterSection param = parameters[i];

					if (param == null)
						return;

					param.UpdateParameter();
				}
			}

			if (ParameterText == null || ParameterRewardText == null || ParameterPenaltyText == null)
				return;

			ParameterText.OnTextUpdate.Invoke(parameterInterface.TitleText);

			ParameterRewardText.OnTextUpdate.Invoke(parameterInterface.RewardText);

			ParameterPenaltyText.OnTextUpdate.Invoke(parameterInterface.PenaltyText);
		}

		public void ToggleSubParams(bool isOn)
		{
			if (parameterInterface == null)
				return;

			if (isOn && parameterInterface.ParameterState == ContractState.Complete)
				return;

			for (int i = parameters.Count - 1; i >= 0; i--)
			{
				CW_ParameterSection parameter = parameters[i];

				if (parameter == null)
					continue;

				parameter.ToggleSubParams(isOn);

				parameter.gameObject.SetActive(isOn);
			}
		}

		private void Update()
		{
			if (parameterInterface == null)
				return;

			if (oldState != parameterInterface.ParameterState)
			{
				oldState = parameterInterface.ParameterState;

				if (oldState != ContractState.Active)
					ToggleSubParams(false);
				else
					ToggleSubParams(true);
			}
			
			if (ParameterText != null)
				ParameterText.OnColorUpdate.Invoke(stateColor(oldState));
		}

		public void ToggleNote(bool isOn)
		{
			if (parameterInterface == null)
				return;

			if (note == null)
				return;

			note.gameObject.SetActive(isOn);

			if (NoteTooltip != null)
				NoteTooltip.SetNewText(isOn ? "Hide Note" : "Show Parameter Note");
		}

		private void setNote()
		{
			if (parameterInterface == null)
				return;

			if (NotePrefab == null || NoteTransform == null)
				return;

			GameObject obj = Instantiate(NotePrefab);

			if (obj == null)
				return;
			
			obj.transform.SetParent(NoteTransform, false);

			note = obj.GetComponent<CW_Note>();

			if (note == null)
				return;

			note.setNote(parameterInterface.GetNote);

			note.gameObject.SetActive(false);

			if (ParameterLayout == null)
				return;

			ParameterLayout.minWidth -= 10;
			ParameterLayout.preferredWidth -= 10;
		}

		private Color stateColor(ContractState state)
		{
			switch (state)
			{
				case ContractState.Active:
					if (parameterInterface.ParamLayer == 0)
						return textColor;
					else
						return subColor;
				case ContractState.Complete:
					return successColor;
				case ContractState.Fail:
					return failColor;
				default:
					return Color.yellow;
			}
		}

		private void CreateSubParameters(IList<IParameterSection> sections)
		{
			if (sections == null)
				return;

			if (parameterInterface == null)
				return;

			if (root == null || root.ParamPrefab == null || SubParamTransform == null)
				return;

			for (int i = 0; i < sections.Count; i++)
			{
				IParameterSection section = sections[i];

				if (section == null)
					continue;

				CreateSubParameter(section);
			}
		}

		private void CreateSubParameter(IParameterSection section)
		{
			GameObject obj = Instantiate(root.ParamPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(SubParamTransform, false);

			CW_ParameterSection paramObject = obj.GetComponent<CW_ParameterSection>();

			if (paramObject == null)
				return;

			paramObject.setParameter(section, root);

			parameters.Add(paramObject);

			paramObject.gameObject.SetActive(parameterInterface.ParameterState != ContractState.Complete);
		}
	}
}
