
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ContractsWindow.Unity
{
	[RequireComponent(typeof(Text))]
	public class TextHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
	{
		[SerializeField]
		private Color NormalColor = Color.white;
		[SerializeField]
		private Color HighlightColor = Color.yellow;

		private ScrollRect scroller;
		private Text AttachedText;

		private void Awake()
		{
			AttachedText = GetComponent<Text>();
		}

		public void setScroller(ScrollRect s)
		{
			scroller = s;
		}

		public void setNormalColor(Color c)
		{
			NormalColor = c;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (AttachedText == null)
				return;

			AttachedText.color = HighlightColor;

			eventData.Reset();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (AttachedText == null)
				return;

			AttachedText.color = NormalColor;
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (scroller == null)
				return;

			scroller.OnScroll((PointerEventData)eventData);
		}

	}
}
