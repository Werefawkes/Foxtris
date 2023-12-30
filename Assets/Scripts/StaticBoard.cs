using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBoard : Board
{
	public GameBoard board;

	public PieceSO CurrentPiece { get; private set; }

	public override void Start()
	{
		base.Start();
	}

	public void DisplayPiece(PieceSO piece)
	{
		CurrentPiece = piece;
		Clear();

		SpawnPiece(piece, true);
	}

	public void Clear()
	{
		foreach (Tile t in tiles)
		{
			t.Clear();
		}
	}
}
