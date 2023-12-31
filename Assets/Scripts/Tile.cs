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

	[Header("Ghost")]
	public float ghostColorMultiplier = 0.5f;

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

	public void Fill(Color newColor, bool ghost = false)
	{
		if (!ghost) IsEmpty = false;

		if (sprite != null)
		{
			spriteRenderer.sprite = sprite;
		}

		if (ghost)
		{
			newColor *= ghostColorMultiplier;
			newColor.a = 1;
		}

		spriteRenderer.color = newColor;
		color = newColor;
	}

	public void CopyFrom(Tile tile, bool ghost = false)
	{
		if (tile.IsEmpty)
		{
			Clear();
		}
		else
		{
			color = tile.color;
			Fill(color, ghost);
		}
	}
}
