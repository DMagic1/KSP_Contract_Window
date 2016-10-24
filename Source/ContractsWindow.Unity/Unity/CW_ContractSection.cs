#region license
/*The MIT License (MIT)
CW_ContractSection - Controls the contract UI element

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
	public class CW_ContractSection : MonoBehaviour
	{
		[SerializeField]
		private TextHandler ContractTitle = null;
		[SerializeField]
		private Image Stars = null;
		[SerializeField]
		private TextHandler TimeRemaining = null;
		[SerializeField]
		private TextHandler ContractRewardText = null;
		[SerializeField]
		private TextHandler ContractPenaltyText = null;
		[SerializeField]
		private GameObject NoteContainer = null;
		[SerializeField]
		private TextHandler NoteText = null;
		[SerializeField]
		private GameObject ParameterSectionPrefab = null;
		[SerializeField]
		private Transform ParameterSectionTransform = null;
		[SerializeField]
		private Sprite Stars_One = null;
		[SerializeField]
		private Sprite Stars_Two = null;
		[SerializeField]
		private Sprite Stars_Three = null;
		[SerializeField]
		private TextHighlighter Highlighter = null;
		[SerializeField]
		private Toggle ContractNoteToggle = null;
		[SerializeField]
		private ToggleSpriteHandler EyesHandler = null;
		[SerializeField]
		private Toggle EyesToggle = null;
		[SerializeField]
		private Toggle PinToggle = null;
		[SerializeField]
		private TooltipHandler EyesTooltip = null;
		[SerializeField]
		private TooltipHandler PinTooltip = null;
		[SerializeField]
		private TooltipHandler NoteTooltip = null;

		private Color textColor = new Color(0.9411765f, 0.5137255f, 0.227451f, 1f);
		private Color successColor = new Color(0.4117647f, 0.8470588f, 0.3098039f, 1f);
		private Color failColor = new Color(0.8980392f, 0f, 0f, 1f);
		private Color timerWarningColor = new Color(0.7803922f, 0.7568628f, 0.04705882f, 1f);

		private IContractSection contractInterface;
		private List<CW_ParameterSection> parameters = new List<CW_ParameterSection>();
		private CW_MissionSection parent;
		private bool loaded;

		public IContractSection Interface
		{
			get { return contractInterface; }
		}

		public GameObject ParamPrefab
		{
			get { return ParameterSectionPrefab; }
		}

		public void setContract(IContractSection contract, CW_MissionSection mission)
		{
			if (contract == null)
				return;

			if (mission == null)
				return;

			if (ContractTitle == null || ContractRewardText == null || ContractPenaltyText == null)
				return;

			parent = mission;

			contractInterface = contract;

			ContractTitle.OnTextUpdate.Invoke(contract.ContractTitle);

			if (CW_Window.Window != null && CW_Window.Window.Scroll != null && Highlighter != null)
				Highlighter.setScroller(CW_Window.Window.Scroll);

			handleColors(stateColor(contract.ContractState));

			ContractRewardText.OnTextUpdate.Invoke(contract.RewardText);

			ContractPenaltyText.OnTextUpdate.Invoke(contract.PenaltyText);

			prepareHeader();

			CreateParameterSections(contract.GetParameters);

			loaded = true;
		}

		public void UpdateContract()
		{
			if (contractInterface == null)
				return;

			if (contractInterface.ShowParams)
			{
				for (int i = parameters.Count - 1; i >= 0; i--)
				{
					CW_ParameterSection param = parameters[i];

					if (param == null)
						return;

					param.UpdateParameter();
				}
			}

			if (ContractTitle == null || ContractRewardText == null || ContractPenaltyText == null)
				return;

			ContractTitle.OnTextUpdate.Invoke(contractInterface.ContractTitle);

			ContractRewardText.OnTextUpdate.Invoke(contractInterface.RewardText);

			ContractPenaltyText.OnTextUpdate.Invoke(contractInterface.PenaltyText);
		}

		private void Update()
		{
			if (contractInterface == null)
				return;

			if (parent == null)
				return;

			if (contractInterface.IsHidden && !parent.MissionInterface.ShowHidden)
				return;

			if (contractInterface.ContractState != ContractState.Active)
				ToggleToClose();

			if (ContractTitle != null && Highlighter != null && !Highlighter.Hover)
				handleColors(stateColor(contractInterface.ContractState));

			if (TimeRemaining != null)
			{
				TimeRemaining.OnTextUpdate.Invoke(contractInterface.TimeRemaining);

				TimeRemaining.OnColorUpdate.Invoke(timeColor(contractInterface.TimeState));
			}
		}

		public void RefreshParameters()
		{
			if (contractInterface == null)
				return;

			for (int i = parameters.Count - 1; i >= 0; i--)
			{
				CW_ParameterSection p = parameters[i];

				if (p == null)
					continue;

				p.gameObject.SetActive(false);

				Destroy(p.gameObject);
			}

			parameters.Clear();

			CreateParameterSections(contractInterface.GetParameters);
		}

		public void ShowAgent()
		{
			if (contractInterface == null)
				return;

			if (CW_Window.Window == null)
				return;

			CW_Window.Window.ShowAgentWindow(contractInterface);
		}

		public void ToggleToClose()
		{
			if (EyesHandler == null)
				return;

			EyesHandler.SetAlternate();

			if (EyesTooltip != null)
				EyesTooltip.SetNewText("Remove Contract");
		}

		public void ToggleHidden(bool isOn)
		{
			if (!loaded)
				return;

			if (contractInterface == null)
				return;

			if (parent == null)
				return;

			if (contractInterface.ContractState != ContractState.Active)
			{
				if (parent == null)
					return;

				parent.RemoveContract(contractInterface.ID);

				contractInterface.RemoveContractFromAll();
				return;
			}

			if (EyesTooltip != null)
				EyesTooltip.SetNewText(isOn ? "Show Contract" : "Hide Contract");

			parent.SwitchContract(contractInterface.ID, isOn);

			ShowParameters(!isOn);

			gameObject.SetActive(false);

			contractInterface.IsHidden = isOn;
		}

		public void TogglePinned(bool isOn)
		{
			if (!loaded)
				return;

			if (contractInterface == null)
				return;

			contractInterface.IsPinned = isOn;

			if (PinTooltip != null)
				PinTooltip.SetNewText(isOn ? "Un-Pin Contract" : "Pin Contract");
		}

		public void AddMission()
		{
			if (contractInterface == null)
				return;

			if (CW_Window.Window == null)
				return;

			CW_Window.Window.ShowMissionAddWindow(contractInterface);
		}

		public void ShowNote(bool isOn)
		{
			if (contractInterface == null)
				return;

			if (NoteContainer == null)
				return;

			NoteContainer.gameObject.SetActive(isOn);

			if (NoteTooltip != null)
				NoteTooltip.SetNewText(isOn ? "Hide Note" : "Show Contract Note");
		}

		public void ShowParameters(bool isOn)
		{
			if (contractInterface == null)
				return;

			contractInterface.ShowParams = isOn;

			for (int i = parameters.Count - 1; i >= 0; i--)
			{
				CW_ParameterSection parameter = parameters[i];

				if (parameter == null)
					continue;

				parameter.ToggleSubParams(isOn);

				parameter.gameObject.SetActive(isOn);
			}
		}

		private void prepareHeader()
		{
			if (Stars != null && Stars_One != null && Stars_Two != null || Stars_Three != null)
				Stars.sprite = GetStars(contractInterface.Difficulty);

			if (TimeRemaining != null)
			{
				TimeRemaining.OnTextUpdate.Invoke(contractInterface.TimeRemaining);

				TimeRemaining.OnColorUpdate.Invoke(timeColor(contractInterface.TimeState));
			}

			if (EyesToggle != null)
				EyesToggle.isOn = contractInterface.IsHidden;

			if (EyesTooltip != null)
				EyesTooltip.SetNewText(contractInterface.IsHidden ? "Show Contract" : "Hide Contract");

			if (PinToggle != null)
				PinToggle.isOn = contractInterface.Order != null;

			if (PinTooltip != null)
				PinTooltip.SetNewText(contractInterface.Order != null ? "Un-Pin Contract" : "Pin Contract");
				
			setNote();
		}

		private void handleColors(Color c)
		{
			ContractTitle.OnColorUpdate.Invoke(c);

			if (Highlighter != null)
				Highlighter.setNormalColor(c);
		}

		private Sprite GetStars(int stars)
		{
			switch (stars)
			{
				case 0:
					return Stars_One;
				case 1:
					return Stars_Two;
				case 2:
					return Stars_Three;
				default:
					return Stars_One;
			}
		}

		private void setNote()
		{
			if (contractInterface == null)
				return;

			if (NoteContainer == null || NoteText == null)
				return;

			if (string.IsNullOrEmpty(contractInterface.GetNote))
			{
				NoteContainer.gameObject.SetActive(false);

				if (ContractNoteToggle != null)
					ContractNoteToggle.gameObject.SetActive(false);

				return;
			}

			NoteText.OnTextUpdate.Invoke(contractInterface.GetNote);

			NoteContainer.gameObject.SetActive(false);
		}

		private void CreateParameterSections(IList<IParameterSection> sections)
		{
			if (sections == null)
				return;

			if (contractInterface == null)
				return;

			if (ParameterSectionPrefab == null || ParameterSectionTransform == null)
				return;

			for (int i = 0; i < sections.Count; i++)
			{
				IParameterSection section = sections[i];

				if (section == null)
					continue;

				CreateParameterSection(section);
			}
		}

		private void CreateParameterSection(IParameterSection section)
		{
			GameObject obj = Instantiate(ParameterSectionPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(ParameterSectionTransform, false);

			CW_ParameterSection paramObject = obj.GetComponent<CW_ParameterSection>();

			if (paramObject == null)
				return;

			paramObject.setParameter(section, this);

			parameters.Add(paramObject);
		}

		public void AddParameter(IParameterSection section)
		{
			if (section == null)
				return;

			CreateParameterSection(section);
		}

		private Color timeColor(int i)
		{
			switch (i)
			{
				case 0:
					return successColor;
				case 1:
					return timerWarningColor;
				case 2:
					return failColor;
				default:
					return failColor;
			}
		}

		private Color stateColor(ContractState state)
		{
			switch (state)
			{
				case ContractState.Active:
					return textColor;
				case ContractState.Complete:
					return successColor;
				case ContractState.Fail:
					return failColor;
				default:
					return textColor;
			}
		}
	}
}
