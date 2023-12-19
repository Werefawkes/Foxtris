using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public Vector2Int index;
	public Color color = Color.red;
	public Sprite sprite;

	[Header("Empty")]
	public Color emptyColor = Color.black;
	public Sprite emptySprite;
	public bool IsEmpty { get; private set; }

	[Header("References")]
	public SpriteRenderer spriteRenderer;

	private void Start()
	{
		SetEmpty(IsEmpty);
	}

	public void SetEmpty(bool isEmpty = true)
	{
		IsEmpty = isEmpty;

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

	public void CopyFrom(Tile tile)
	{
		SetEmpty(tile.IsEmpty);
		color = tile.color;
	}
}
