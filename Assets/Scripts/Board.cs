using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Board : MonoBehaviour
{
	public Vector2Int dimensions;
	public float ticksPerSecond = 1;
	float nextTickTime;

	Tile currentTile;

	Tile[,] tiles;

	[Header("Input"), Tooltip("The time in seconds it takes to repeat the move when holding the button down.")]
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
		tiles = new Tile[dimensions.x, dimensions.y];

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
			SpawnPiece();
		}


	}

	public void SpawnPiece()
	{
		// TODO: Check that the piece can be spawned
		Vector2Int pos = new(dimensions.x / 2, dimensions.y - 1);
		currentTile = tiles[pos.x, pos.y];
		currentTile.SetEmpty(false);
	}

	#region Movement
	public bool TryMove(Vector2Int direction)
	{
		Vector2Int targetIndex = currentTile.index;
		targetIndex += direction;

		// Fail if the target would be off the board
		if (targetIndex.x < 0 || targetIndex.x >= dimensions.x || targetIndex.y < 0)
		{
			return false;
		}

		Tile target = tiles[targetIndex.x, targetIndex.y];

		if (target.IsEmpty)
		{
			target.SetEmpty(false);
			currentTile.SetEmpty(true);
			currentTile = target;
			return true;
		}
		else
		{
			// Lock piece in
			return false;
		}
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

	#endregion

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
