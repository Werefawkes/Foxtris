using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewBoard : Board
{
	public GameBoard board;

	public PieceSO CurrentPiece { get; private set; }

	public override void Start()
	{
		base.Start();
	}

	public void DisplayPiece(PieceSO piece)
	{

	}

	public void Clear()
	{
		
	}
}
