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
		private Image ShowHide = null;
		[SerializeField]
		private Image Pin = null;
		[SerializeField]
		private Image ContractNoteImage = null;
		[SerializeField]
		private Toggle ContractNoteToggle = null;
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
		private Sprite Show = null;
		[SerializeField]
		private Sprite Hide = null;
		[SerializeField]
		private Sprite Close = null;
		[SerializeField]
		private Sprite Pin_Sprite = null;
		[SerializeField]
		private Sprite UnPin = null;
		[SerializeField]
		private Sprite NoteOn = null;
		[SerializeField]
		private Sprite NoteOff = null;

		private Color textColor = new Color();
		private Color successColor = new Color();
		private Color failColor = new Color();
		private Color timerWarningColor = new Color();

		private IContractSection contractInterface;
		private List<CW_ParameterSection> parameters = new List<CW_ParameterSection>();
		private CW_Note note;
		private CW_Window window;
		private CW_MissionSection parent;

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

			ContractTitle.color = stateColor(contract.ContractState);

			ContractRewardText.text = contract.RewardText;

			ContractPenaltyText.text = contract.PenaltyText;

			prepareHeader();

			CreateParameterSections(contract.GetParameters());
		}

		private void Update()
		{
			if (contractInterface == null)
				return;

			if (!contractInterface.IsVisible)
				return;

			contractInterface.Update();

			if (ContractTitle!= null)
			{
				ContractTitle.text = contractInterface.ContractTitle;

				ContractTitle.color = stateColor(contractInterface.ContractState);
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

		public void ToggleHidden(bool isOn)
		{
			if (contractInterface == null)
				return;

			if (contractInterface.ContractState != ContractState.Active)
			{
				contractInterface.RemoveContract();

				if (parent == null)
					return;

				parent.DestroyChild(gameObject);
				return;
			}

			contractInterface.IsHidden = isOn;

			if (ShowHide != null && Show != null && Hide != null)
				ShowHide.sprite = GetEyes(isOn);
		}

		public void TogglePinned(bool isOn)
		{
			if (contractInterface == null)
				return;

			contractInterface.IsPinned = isOn;

			if (Pin != null && Pin_Sprite != null && UnPin != null)
				Pin.sprite = GetPin(isOn);
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

			if (ContractNoteImage == null || NoteOn == null || NoteOff == null)
				return;

			if (note == null)
				return;

			note.gameObject.SetActive(isOn);

			ContractNoteImage.sprite = isOn ? NoteOff : NoteOn;
		}

		public void ShowParameters(bool isOn)
		{
			if (contractInterface == null)
				return;

			for (int i = parameters.Count - 1; i >= 0; i--)
			{
				CW_ParameterSection parameter = parameters[i];

				if (parameter == null)
					continue;

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

			if (ShowHide != null && Show != null && Hide != null)
				ShowHide.sprite = GetEyes(contractInterface.IsHidden);

			if (Pin != null && Pin_Sprite != null && UnPin != null)
				Pin.sprite = GetPin(contractInterface.IsPinned);

			if (contractInterface.HasNote)
				setNote();
			else if (ContractNoteToggle != null)
				ContractNoteToggle.gameObject.SetActive(false);
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

		private Sprite GetEyes(bool hidden)
		{
			if (hidden)
				return Hide;
			else
				return Show;
		}

		private Sprite GetPin(bool pinned)
		{
			if (pinned)
				return UnPin;
			else
				return Pin_Sprite;
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

			contractInterface.ProcessStyle(obj);

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

			contractInterface.ProcessStyle(obj);

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
					return textColor;
				case 1:
					return timerWarningColor;
				case 2:
					return failColor;
				default:
					return textColor;
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
