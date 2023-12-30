using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	[Header("Board")]
	public Vector2Int dimensions;
	public int buffer = 2;
	// TODO: Move current palette to player prefs
	public PaletteSO palette;

	public Tile[,] tiles;
	public GameObject tilePrefab;

	public List<Tile> currentTiles;


	public virtual void Start()
	{
		// Create the board
		tiles = new Tile[dimensions.x, dimensions.y + buffer];

		for (int y = 0; y < dimensions.y + buffer; y++)
		{
			for (int x = 0; x < dimensions.x; x++)
			{
				// Center the pos on 0, 0
				Vector3 pos = new(x - dimensions.x / 2f + 0.5f, y - dimensions.y / 2f + 0.5f);

				Tile tile = Instantiate(tilePrefab, transform).GetComponent<Tile>();
				tile.transform.localPosition = pos;
				tile.Clear();
				tile.name = $"Tile[{x},{y}]";
				tile.index = new(x, y);
				tiles[x, y] = tile;
			}
		}
	}

	public bool SpawnPiece(PieceSO piece, bool centered = false)
	{
		Vector2Int center = new(dimensions.x / 2, dimensions.y - 1);

		if (centered)
		{
			center = dimensions / 2;
		}

		currentTiles = new();
		foreach (Vector2Int p in piece.tiles)
		{
			Vector2Int tPos = center + p;
			Tile tile = tiles[tPos.x, tPos.y];
			if (tile.IsEmpty)
			{
				currentTiles.Add(tile);
				tile.Fill(palette.colors[piece.colorIndex]);
			}
			else
			{
				currentTiles = new();
				return false;
			}
		}

		return true;
	}


	public virtual void OnDrawGizmos()
	{
		Vector2 dim = (Vector2)dimensions / 2;
		dim.x *= transform.localScale.x;
		dim.y *= transform.localScale.y;
		Vector3[] points = new Vector3[4]
		{
			new(-dim.x, -dim.y),
			new(dim.x, -dim.y),
			new(dim.x, dim.y),
			new(-dim.x, dim.y)
		};
		for (int i = 0; i < points.Length; i++)
		{
			points[i] += transform.position;
		}

		Gizmos.DrawLineStrip(points, true);
	}

}
