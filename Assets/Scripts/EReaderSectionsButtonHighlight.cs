using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EReaderSectionsButtonHighlight : MonoBehaviour {
	public Text text;
	public Image sprite;
	public Color lit;
	public Color dark;
	public Sprite litSprite;
	public Sprite darkSprite;

	public void DeHighlight () {
		if (text != null) text.color = dark;
		if (sprite != null) sprite.overrideSprite = darkSprite;
	}
	public void Highlight () {
		if (text != null) text.color = lit;
		if (sprite != null) sprite.overrideSprite = litSprite;
	}
}
