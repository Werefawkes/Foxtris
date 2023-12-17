using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public Color color = Color.red;
	public Sprite sprite;

	[Header("Empty")]
	public bool IsEmpty;
	public Color emptyColor = Color.black;
	public Sprite emptySprite;

	[Header("References")]
	public SpriteRenderer spriteRenderer;

	private void Start()
	{
		TileUpdate();
	}

	private void TileUpdate()
	{
		if (IsEmpty)
		{
			if (emptySprite != null)
			{
				spriteRenderer.sprite = emptySprite;
			}
			spriteRenderer.color = emptyColor;
		}
		else
		{
			if (sprite != null)
			{
				spriteRenderer.sprite = sprite;
			}
			spriteRenderer.color = color;
		}
	}
}
