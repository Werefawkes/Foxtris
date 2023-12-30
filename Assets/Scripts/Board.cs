using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	[Header("Board")]
	public Vector2Int dimensions;
	public int buffer = 2;

	public Tile[,] tiles;
	public GameObject tilePrefab;

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

	// Update is called once per frame
	void Update()
	{
		
	}
}
