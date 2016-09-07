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
		private Text ContractTitle = null;
		[SerializeField]
		private Image Stars = null;
		[SerializeField]
		private Text TimeRemaining = null;
		[SerializeField]
		private Text ContractRewardText = null;
		[SerializeField]
		private Text ContractPenaltyText = null;
		[SerializeField]
		private GameObject ContractNotePrefab = null;
		[SerializeField]
		private Transform ContractNoteTransform = null;
		[SerializeField]
		private GameObject ParamaterSectionPrefab = null;
		[SerializeField]
		private Transform ParamaterSectionTransform = null;
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

		private Color textColor = new Color(0.9411765f, 0.5137255f, 0.227451f, 1f);
		private Color successColor = new Color(0.4117647f, 0.8470588f, 0.3098039f, 1f);
		private Color failColor = new Color(0.8980392f, 0f, 0f, 1f);
		private Color timerWarningColor = new Color(0.7803922f, 0.7568628f, 0.04705882f, 1f);

		private IContractSection contractInterface;
		private List<CW_ParameterSection> parameters = new List<CW_ParameterSection>();
		private CW_Note note;
		private CW_Window window;
		private CW_MissionSection parent;
		private bool loaded;

		public IContractSection Interface
		{
			get { return contractInterface; }
		}

		public void setContract(IContractSection contract, CW_Window win, CW_MissionSection mission)
		{
			if (contract == null)
				return;

			if (win == null)
				return;

			if (mission == null)
				return;

			if (ContractTitle == null || ContractRewardText == null || ContractPenaltyText == null)
				return;

			window = win;

			parent = mission;

			contractInterface = contract;

			ContractTitle.text = contract.ContractTitle;

			handleColors(stateColor(contract.ContractState));

			ContractRewardText.text = contract.RewardText;

			ContractPenaltyText.text = contract.PenaltyText;

			prepareHeader();

			CreateParameterSections(contract.GetParameters);

			loaded = true;
		}

		private void Update()
		{
			if (contractInterface == null)
				return;

			if (contractInterface.ContractState != ContractState.Active)
				ToggleToClose();

			if (ContractTitle!= null)
			{
				ContractTitle.text = contractInterface.ContractTitle;

				handleColors(stateColor(contractInterface.ContractState));
			}

			if (ContractRewardText != null)
				ContractRewardText.text = contractInterface.RewardText;

			if (ContractPenaltyText != null)
				ContractPenaltyText.text = contractInterface.PenaltyText;

			if (TimeRemaining != null)
			{
				TimeRemaining.text = contractInterface.TimeRemaining;

				TimeRemaining.color = timeColor(contractInterface.TimeState);
			}
		}

		public void ShowAgent()
		{
			if (contractInterface == null)
				return;

			if (window == null)
				return;

			window.ShowAgentWindow(contractInterface);
		}

		public void ToggleToClose()
		{
			if (EyesHandler == null)
				return;

			EyesHandler.SetAlternate();
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

			parent.SwitchContract(contractInterface.ID, isOn);

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
		}

		public void AddMission()
		{
			if (contractInterface == null)
				return;

			if (window == null)
				return;

			window.ShowMissionAddWindow(contractInterface);
		}

		public void ShowNote(bool isOn)
		{
			if (contractInterface == null)
				return;

			if (note == null)
				return;

			note.gameObject.SetActive(isOn);
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
				TimeRemaining.text = contractInterface.TimeRemaining;

				TimeRemaining.color = timeColor(contractInterface.TimeState);
			}

			if (EyesToggle != null)
				EyesToggle.isOn = contractInterface.IsHidden;

			if (PinToggle != null)
				PinToggle.isOn = contractInterface.Order != null;
				
			if (!string.IsNullOrEmpty(contractInterface.GetNote))
				setNote();
			else if (ContractNoteToggle != null)
				ContractNoteToggle.gameObject.SetActive(false);
		}

		private void handleColors(Color c)
		{
			ContractTitle.color = c;

			if (Highlighter != null)
				Highlighter.setNormalColor(c);
		}

		private Sprite GetStars(int stars)
		{
			switch (stars)
			{
				case 1:
					return Stars_One;
				case 2:
					return Stars_Two;
				case 3:
					return Stars_Three;
				default:
					return Stars_One;
			}
		}

		private void setNote()
		{
			if (contractInterface == null)
				return;

			if (ContractNotePrefab == null || ContractNoteTransform == null)
				return;

			GameObject obj = Instantiate(ContractNotePrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(ContractNoteTransform, false);

			note = obj.GetComponent<CW_Note>();

			if (note == null)
				return;

			note.setNote(contractInterface.GetNote);

			note.gameObject.SetActive(false);
		}

		private void CreateParameterSections(IList<IParameterSection> sections)
		{
			if (sections == null)
				return;

			if (contractInterface == null)
				return;

			if (ParamaterSectionPrefab == null || ParamaterSectionTransform == null)
				return;

			for (int i = sections.Count - 1; i >= 0; i--)
			{
				IParameterSection section = sections[i];

				if (section == null)
					continue;

				CreateParameterSection(section);
			}
		}

		private void CreateParameterSection(IParameterSection section)
		{
			GameObject obj = Instantiate(ParamaterSectionPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(ParamaterSectionTransform, false);

			CW_ParameterSection paramObject = obj.GetComponent<CW_ParameterSection>();

			if (paramObject == null)
				return;

			paramObject.setParameter(section);

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
