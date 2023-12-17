using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	public Vector2Int dimensions;
	public int[,] boardState;

	Tile[,] tiles;

	[Header("References")]
	public GameObject tilePrefab;

	void Start()
	{
		tiles = new Tile[dimensions.x, dimensions.y];

		for (int y = 0; y < dimensions.y; y++)
		{
			for (int x = 0; x < dimensions.x; x++)
			{
				// Center the pos on 0, 0
				Vector3 pos = new(x - dimensions.x / 2f + 0.5f, y - dimensions.y / 2f + 0.5f);

				Tile tile = Instantiate(tilePrefab, transform).GetComponent<Tile>();
				tile.transform.localPosition = pos;
				tile.IsEmpty = true;
				tile.name = $"Tile[{x},{y}]";
				tiles[x, y] = tile;
			}
		}
	}

	void Update()
	{
		
	}

	private void OnDrawGizmos()
	{
		Vector2 dim = dimensions / 2;
		dim.x *= transform.localScale.x;
		dim.y *= transform.localScale.y;
		Vector3[] points = new Vector3[4]
		{
			new(-dim.x, -dim.y),
			new(dim.x, -dim.y),
			new(dim.x, dim.y),
			new(-dim.x, dim.y)
		};
		Gizmos.DrawLineStrip(points, true);
	}
}
