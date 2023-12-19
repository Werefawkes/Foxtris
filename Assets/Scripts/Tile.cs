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
		if (IsEmpty)
		{
			Clear();
		}
	}

	public void Clear()
	{
		IsEmpty = true;

		if (emptySprite != null)
		{
			spriteRenderer.sprite = emptySprite;
		}
		spriteRenderer.color = emptyColor;
	}

	public void Fill(Color color)
	{
		IsEmpty = false;

		if (sprite != null)
		{
			spriteRenderer.sprite = sprite;
		}
		spriteRenderer.color = color;
	}

	public void CopyFrom(Tile tile)
	{
		if (tile.IsEmpty)
		{
			Clear();
		}
		else
		{
			color = tile.color;
			Fill(color);
		}
	}
}
