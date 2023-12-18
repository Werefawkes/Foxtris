using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	public Vector2Int dimensions;
	public int[,] boardState;
	public float ticksPerSecond = 1;
	float nextTick;

	Tile[,] tiles;

	[Header("References")]
	public GameObject tilePrefab;

	void Start()
	{
		// Create the board
		tiles = new Tile[dimensions.x, dimensions.y];
		boardState = new int[dimensions.x, dimensions.y];

		for (int y = 0; y < dimensions.y; y++)
		{
			for (int x = 0; x < dimensions.x; x++)
			{
				// Center the pos on 0, 0
				Vector3 pos = new(x - dimensions.x / 2f + 0.5f, y - dimensions.y / 2f + 0.5f);

				Tile tile = Instantiate(tilePrefab, transform).GetComponent<Tile>();
				tile.transform.localPosition = pos;
				tile.SetEmpty(true);
				tile.name = $"Tile[{x},{y}]";
				tiles[x, y] = tile;

				boardState[x, y] = -1;
			}
		}

		SpawnPiece();
	}

	void Update()
	{
		if (Time.time >= nextTick)
		{
			nextTick = Time.time + ticksPerSecond;
			Tick();
		}
	}

	void Tick()
	{
		// Game stuff

		// Update display
		for (int y = 0; y < dimensions.y; y++)
		{
			for (int x = 0; x < dimensions.x; x++)
			{
				// Tile empty
				if (boardState[x, y] < 0)
				{
					tiles[x, y].SetEmpty(true);
				}
				else
				{
					tiles[x, y].color = Color.blue;
					tiles[x, y].SetEmpty(false);
				}
			}
		}
	}

	public void SpawnPiece()
	{
		Vector2Int pos = new(dimensions.x / 2, dimensions.y - 1);
		boardState[pos.x, pos.y] = 0;
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
