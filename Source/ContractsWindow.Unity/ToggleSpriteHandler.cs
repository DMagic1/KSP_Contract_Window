using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ContractsWindow.Unity
{
	[RequireComponent(typeof(Image))]
	public class ToggleSpriteHandler : MonoBehaviour
	{
		[SerializeField]
		private Sprite OnSprite = null;
		[SerializeField]
		private Sprite OffSprite = null;
		[SerializeField]
		private Sprite Alternate = null;
		[SerializeField]
		private Toggle ParentToggle = null;

		private Image image;
		private bool alternateSet;

		private void Awake()
		{
			if (ParentToggle == null || OnSprite == null || OffSprite == null)
				return;

			ParentToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(onToggle));

			image = GetComponent<Image>();
		}

		public void onToggle(bool isOn)
		{
			if (image == null || OnSprite == null || OffSprite == null)
				return;

			if (alternateSet)
				return;

			image.sprite = isOn ? OnSprite : OffSprite;
		}

		public void SetAlternate()
		{
			if (Alternate == null || image == null)
				return;

			alternateSet = true;

			image.sprite = Alternate;
		}
	}
}
