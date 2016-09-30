#region license
/*The MIT License (MIT)
CW_Style - Unity element for marking UI styles

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

using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_Style : MonoBehaviour
	{
		public enum ElementTypes
		{
			None,
			Window,
			Box,
			Button,
			ToggleButton,
			ToggleAlwaysOn,
			Label,
			VertScroll,
			Slider,
			SliderBackground,
			Text,
			TextUpdated
		}

		[SerializeField]
		private ElementTypes elementType = ElementTypes.None;

		public ElementTypes ElementType
		{
			get { return elementType; }
		}

		private void setSelectable(Styles style, Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setText(style, GetComponentInChildren<Text>());

			Selectable select = GetComponent<Selectable>();

			if (select == null)
				return;

			select.image.sprite = normal;
			select.image.type = Image.Type.Sliced;
			select.transition = Selectable.Transition.SpriteSwap;

			SpriteState spriteState = select.spriteState;
			spriteState.highlightedSprite = highlight;
			spriteState.pressedSprite = active;
			spriteState.disabledSprite = inactive;
			select.spriteState = spriteState;
		}

		private void setText(Styles style, Text text)
		{
			if (style == null)
				return;

			if (text == null)
				return;

			if (style.Font != null)
				text.font = style.Font;

			text.fontSize = style.Size;
			text.fontStyle = style.Style;
			text.color = style.Color;
		}

		public void setText(Styles style)
		{
			setText(style, GetComponent<Text>());
		}

		public void setScrollbar(Sprite background, Sprite thumb)
		{
			Image back = GetComponent<Image>();

			if (back == null)
				return;

			back.sprite = background;

			Scrollbar scroll = GetComponent<Scrollbar>();

			if (scroll == null)
				return;

			if (scroll.targetGraphic == null)
				return;

			Image scrollThumb = scroll.targetGraphic.GetComponent<Image>();

			if (scrollThumb == null)
				return;

			scrollThumb.sprite = thumb;
		}

		public void setImage(Sprite sprite, Image.Type type)
		{
			Image image = GetComponent<Image>();

			if (image == null)
				return;

			image.sprite = sprite;
			image.type = type;
		}

		public void setButton(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setSelectable(null, normal, highlight, active, inactive);
		}

		public void setToggle(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
		{
			setSelectable(null, normal, highlight, active, inactive);

			Toggle toggle = GetComponent<Toggle>();

			if (toggle == null)
				return;

			Image toggleImage = toggle.graphic as Image;

			if (toggleImage == null)
				return;

			toggleImage.sprite = active;
			toggleImage.type = Image.Type.Sliced;
		}

		public void setSlider(Sprite background, Sprite foreground, Color backColor, Color foreColor)
		{
			if (background == null || foreground == null)
				return;

			Slider slider = GetComponent<Slider>();

			if (slider == null)
				return;

			Image back = slider.GetComponentInChildren<Image>();

			if (back == null)
				return;

			back.sprite = background;
			back.color = backColor;
			back.type = Image.Type.Sliced;

			RectTransform fill = slider.fillRect;

			if (fill == null)
				return;

			Image front = fill.GetComponentInChildren<Image>();

			if (front == null)
				return;

			front.sprite = foreground;
			front.color = foreColor;
			front.type = Image.Type.Sliced;
		}

		public void setSlider(Sprite foreground, Color foreColor)
		{
			if (foreground == null)
				return;

			Slider slider = GetComponent<Slider>();

			if (slider == null)
				return;

			RectTransform fill = slider.fillRect;

			if (fill == null)
				return;

			Image front = fill.GetComponentInChildren<Image>();

			if (front == null)
				return;

			front.sprite = foreground;
			front.color = foreColor;
			front.type = Image.Type.Sliced;
		}

	}
}
