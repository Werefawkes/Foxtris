using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Foxthorne.FoxScreens;

public class GameBoard : Board
{
	[Header("Game")]
	public float ticksPerSecond = 1;
	public PieceSetSO pieceSet;
	float nextTickTime;
	bool holdSpent = false;

	[Header("Scoring")]
	public int linesPerLevel = 10;

	int level = 1;
	int score = 0;
	int linesClearedTotal = 0;
	int linesClearedThisTick = 0;


	[Header("Input"), Tooltip("The time in seconds it takes to repeat the move while holding the button down.")]
	public float moveRepeatTime = 0.1f;
	[Tooltip("The time in seconds it takes to begin repeating the move when holding the button down.")]
	public float moveRepeatDelay = 0.2f;
	public float dropRepeatTime = 0.1f;
	float nextMoveTime = -1;
	float nextDropTime = -1;
	float moveInput;

	readonly List<PieceSO> pieceBag = new();

	List<Tile> ghostTiles = new();

	[Header("References")]
	public StaticBoard holdBoard;
	public StaticBoard previewBoard;
	public TMPro.TMP_Text text;

	public override void Start()
	{
		base.Start();

		SpawnPiece(GetNextPiece());
		previewBoard.DisplayPiece(GetNextPiece());
	}

	void Update()
	{
		if (UIManager.IsUIClear)
		{
			Time.timeScale = 1;
		}
		else
		{
			Time.timeScale = 0;
		}

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

		// Score display
		string newText = $"Score\n{score}\nLevel\n{level}\nLines\n{linesClearedTotal}";
		text.text = newText;
	}

	void Tick()
	{
		// Fall
		if (!TryMoveDown())
		{
			linesClearedThisTick = 0;

			// Piece lands
			currentTiles = new();
			ghostTiles = new();
			CheckLines();
			ScoreLines();
			TrySpawnNext();
			holdSpent = false;
		}
	}

	#region Piece Spawning
	bool TrySpawnNext()
	{
		if (!SpawnPiece(previewBoard.CurrentPiece))
		{
			// Game over
			Debug.Log("Game over");
			return false;
		}
		else
		{
			previewBoard.DisplayPiece(GetNextPiece());
			CreateGhost();
			return true;
		}
	}

	public PieceSO GetNextPiece()
	{
		// 7 bag
		if (pieceBag.Count == 0)
		{
			pieceBag.AddRange(pieceSet.pieces);
		}
		// Draw a random piece
		PieceSO next = pieceBag[Random.Range(0, pieceBag.Count)];
		pieceBag.Remove(next);

		return next;
	}
	#endregion

	#region Line Checking
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
				linesClearedThisTick++;
			}
		}
	}

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

		// Iterate upwards to shift the entire board down
		if (rowIndex + 1 < dimensions.y)
		{
			ClearLine(rowIndex + 1);
		}
	}
	#endregion

	#region Scoring
	public void ScoreLines()
	{
		int lineMultiplier = 0;
		if (linesClearedThisTick == 1)
		{
			lineMultiplier = 100;
		}
		else if (linesClearedThisTick == 2)
		{
			lineMultiplier = 300;
		}
		else if (linesClearedThisTick == 3)
		{
			lineMultiplier = 500;
		}
		else if (linesClearedThisTick == 4)
		{
			lineMultiplier = 800;
		}

		score += lineMultiplier * level;
		linesClearedTotal += linesClearedThisTick;
		level = (linesClearedTotal / linesPerLevel) + 1;
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
		CreateGhost();
		return true;
	}

	public void CreateGhost()
	{
		if (currentTiles == null || currentTiles.Count == 0) return;

		List<Tile> oldTiles = new(currentTiles);
		List<Tile> newTiles = new();
		bool didMove = true;
		while (didMove)
		{
			foreach (Tile tile in oldTiles)
			{
				Vector2Int targetIndex = tile.index;
				targetIndex += Vector2Int.down;

				// Fail if the target would be off the board
				if (targetIndex.x < 0 || targetIndex.x >= dimensions.x || targetIndex.y < 0)
				{
					didMove = false;
					break;
				}

				Tile target = tiles[targetIndex.x, targetIndex.y];

				if (target.IsEmpty || oldTiles.Contains(target))
				{
					newTiles.Add(target);
				}
				else
				{
					didMove = false;
					break;
				}
			}

			if (didMove)
			{
				oldTiles = newTiles;
				newTiles = new();
			}
		}

		foreach (Tile t in ghostTiles) 
		{
			if (!currentTiles.Contains(t))
			{
				t.Clear();
			}
		}

		for (int i = 0; i < oldTiles.Count; i++)
		{
			// Don't overwrite the actual piece
			if (!currentTiles.Contains(oldTiles[i]))
			{
				oldTiles[i].CopyFrom(currentTiles[i], true);
			}
		}

		ghostTiles = oldTiles;
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
		CreateGhost();
		return true;
	}
	#endregion

	#region Input Methods
	bool CanMove()
	{
		return UIManager.IsUIClear;
	}

	void OnMove(InputValue value)
	{
		if (!CanMove()) return;

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
		if (!CanMove()) return;

		float v = value.Get<float>();
		if (v == 0) return;

		TryRotate(v > 0);
	}

	void OnSoftDrop(InputValue value)
	{
		if (!CanMove()) return;

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
		if (!CanMove()) return;

		bool moved;
		do
		{
			moved = TryMoveDown();
		} 
		while (moved);

		// Make next tick happen instantly
		nextTickTime = Time.time;
	}

	void OnHold()
	{
		if (holdSpent) return;

		PieceSO held = holdBoard.CurrentPiece;
		holdBoard.DisplayPiece(CurrentPiece);
		ClearCurrentTiles();

		if (held == null)
		{
			TrySpawnNext();
		}
		else
		{
			SpawnPiece(held);
			CreateGhost();
		}

		holdSpent = true;
	}

	void OnPause()
	{
		if (UIManager.IsUIClear)
		{
			UIManager.Instance.OpenScreen("PauseMenu");
		}
		else
		{
			UIManager.Instance.CloseAllScreens();
		}
	}
	#endregion

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();

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
