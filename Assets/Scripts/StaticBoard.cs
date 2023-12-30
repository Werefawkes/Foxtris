using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBoard : Board
{
	public GameBoard board;

	public override void Start()
	{
		base.Start();
	}

	public void DisplayPiece(PieceSO piece)
	{
		ClearCurrentTiles();

		SpawnPiece(piece, true);
	}
}