using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Board : MonoBehaviour
{
	[Header("Board")]
	public Vector2Int dimensions;
	public int buffer = 2;

	[Header("Game")]
	public float ticksPerSecond = 1;
	public PieceSetSO pieceSet;
	float nextTickTime;

	List<Tile> currentTiles;

	Tile[,] tiles;

	[Header("Input"), Tooltip("The time in seconds it takes to repeat the move while holding the button down.")]
	public float moveRepeatTime = 0.1f;
	[Tooltip("The time in seconds it takes to begin repeating the move when holding the button down.")]
	public float moveRepeatDelay = 0.2f;
	public float dropRepeatTime = 0.1f;
	float nextMoveTime = -1;
	float nextDropTime = -1;
	float moveInput;

	[Header("References")]
	public GameObject tilePrefab;

	void Start()
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

		SpawnPiece();
	}

	void Update()
	{
		if (Time.time >= nextTickTime)
		{
			nextTickTime = Time.time + (1 / ticksPerSecond);
			Tick();
		}

		// Input repeating
		if (nextMoveTime > 0 && Time.time >= nextMoveTime)
		{
			if (moveInput < 0)
			{
				TryMoveLeft();
			}
			else
			{
				TryMoveRight();
			}

			nextMoveTime = Time.time + moveRepeatTime;
		}

		// Soft drops
		if (nextDropTime > 0 && Time.time >= nextDropTime)
		{
			TryMoveDown();
			nextDropTime = Time.time + dropRepeatTime;
			// Postpone next tick
			nextTickTime = Time.time + (1 / ticksPerSecond);
		}
	}

	void Tick()
	{
		// Fall
		if (!TryMoveDown())
		{
			currentTiles = new();
			CheckLines();
			SpawnPiece();
		}
	}

	public void SpawnPiece()
	{
		PieceSO piece = GetNextPiece();

		Vector2Int center = new(dimensions.x / 2, dimensions.y - 2);

		currentTiles = new();
		foreach (Vector2Int p in piece.tiles)
		{
			Vector2Int tPos = center + p;
			Tile tile = tiles[tPos.x, tPos.y];
			if (tile.IsEmpty)
			{
				currentTiles.Add(tile);
				tile.Fill(Color.red);
			}
			else
			{
				// Game over
				Debug.Log("Game over");
				currentTiles = new();
				return;
			}
		}
	}

	public PieceSO GetNextPiece()
	{
		return pieceSet.pieces[Random.Range(0, pieceSet.pieces.Count)];
	}


	#region Line Checking
	public void ClearLine(int rowIndex)
	{
		for (int x = 0; x < dimensions.x; x++)
		{
			// If there's a tile above
			if (rowIndex + 1 < dimensions.y)
			{
				tiles[x, rowIndex].CopyFrom(tiles[x, rowIndex + 1]);
			}
			else
			{
				tiles[x, rowIndex].Clear();
			}
		}

		if (rowIndex + 1 < dimensions.y)
		{
			ClearLine(rowIndex + 1);
		}
	}

	public void CheckLines()
	{
		// Check from the top down
		for (int y = dimensions.y - 1; y >= 0; y--)
		{
			bool full = true;
			for (int x = 0; x < dimensions.x; x++)
			{
				if (tiles[x, y].IsEmpty || currentTiles.Contains(tiles[x, y]))
				{
					full = false;
					break;
				}
			}

			if (full)
			{
				ClearLine(y);
			}
		}
	}
	#endregion

	#region Movement
	public bool TryMove(Vector2Int direction)
	{
		if (currentTiles == null || currentTiles.Count == 0) return false;

		List<Tile> newTiles = new();
		foreach (Tile tile in currentTiles)
		{
			Vector2Int targetIndex = tile.index;
			targetIndex += direction;

			// Fail if the target would be off the board
			if (targetIndex.x < 0 || targetIndex.x >= dimensions.x || targetIndex.y < 0)
			{
				return false;
			}

			Tile target = tiles[targetIndex.x, targetIndex.y];

			if (target.IsEmpty || currentTiles.Contains(target))
			{
				newTiles.Add(target);
			}
			else
			{
				return false;
			}
		}

		for (int i = 0; i < newTiles.Count; i++)
		{
			newTiles[i].CopyFrom(currentTiles[i]);
			if (!newTiles.Contains(currentTiles[i]))
			{
				currentTiles[i].Clear();
			}
		}

		currentTiles = newTiles;
		return true;
	}

	public bool TryMoveDown()
	{
		return TryMove(Vector2Int.down);
	}

	public bool TryMoveLeft()
	{
		return TryMove(Vector2Int.left);
	}

	public bool TryMoveRight()
	{
		return TryMove(Vector2Int.right);
	}

	public bool TryRotate(bool clockwise)
	{
		Vector2 center = Vector2.zero;
		foreach (Tile t in currentTiles)
		{
			center += t.index;
		}
		center /= currentTiles.Count;
		Debug.Log(center);
		Vector2Int centerInt = new(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y));

		List<Tile> newTiles = new();
		foreach (Tile tile in currentTiles)
		{
			Vector2Int dif = centerInt - tile.index;
			Vector2Int rDif = new(dif.y, dif.x);
			if (clockwise)
			{
				rDif.x = -rDif.x;
			}
			else
			{
				rDif.y = -rDif.y;
			}

			Vector2Int targetIndex = rDif + centerInt;

			// Fail if the target would be off the board
			if (targetIndex.x < 0 || targetIndex.x >= dimensions.x || targetIndex.y < 0 || targetIndex.y >= dimensions.y + buffer)
			{
				return false;
			}

			Tile target = tiles[targetIndex.x, targetIndex.y];

			if (target.IsEmpty || currentTiles.Contains(target))
			{
				newTiles.Add(target);
			}
			else
			{
				return false;
			}
		}

		for (int i = 0; i < newTiles.Count; i++)
		{
			newTiles[i].CopyFrom(currentTiles[i]);
			if (!newTiles.Contains(currentTiles[i]))
			{
				currentTiles[i].Clear();
			}
		}

		currentTiles = newTiles;
		return true;
	}
	#endregion

	#region Input Methods
	void OnMove(InputValue value)
	{
		float v = value.Get<float>();
		// Don't move if v == 0
		if (v == 0)
		{
			nextMoveTime = -1;
		}
		else
		{
			nextMoveTime = Time.time + moveRepeatDelay;
			moveInput = v;
		}

		if (v < 0)
		{
			TryMoveLeft();
		}
		else if (v > 0)
		{
			TryMoveRight();
		}
	}

	void OnRotate(InputValue value)
	{
		float v = value.Get<float>();
		if (v == 0) return;

		TryRotate(v > 0);
	}

	void OnSoftDrop(InputValue value)
	{
		if (value.Get<float>() == 0)
		{
			nextDropTime = -1;
		}
		else
		{
			TryMoveDown();
			nextDropTime = Time.time + dropRepeatTime;
		}

	}

	void OnHardDrop()
	{
		bool moved;
		do
		{
			moved = TryMoveDown();
		} 
		while (moved);

		// Make next tick happen instantly
		nextTickTime = Time.time;
	}
	#endregion

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

		if (currentTiles == null) return;
		Vector2 center = Vector2.zero;
		foreach (Tile t in currentTiles)
		{
			center += t.index;
		}
		center /= currentTiles.Count;
		Vector2Int centerInt = new(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y));
		Vector2 offset = new(0.5f, 0.5f);

		Gizmos.color = Color.blue;
		center -= dimensions / 2;
		center += offset;
		Gizmos.DrawSphere(center * transform.localScale, 0.5f * transform.localScale.x);

		Gizmos.color = Color.yellow;
		Vector2 ci = new(centerInt.x - dimensions.x / 2, centerInt.y - dimensions.y / 2);
		ci += offset;
		Gizmos.DrawSphere(ci * transform.localScale, 0.5f * transform.localScale.x);
	}
}
