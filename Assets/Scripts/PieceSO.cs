using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu(menuName: "Piece")]
public class PieceSO : ScriptableObject
{
	public Tile[,] tiles;
}
