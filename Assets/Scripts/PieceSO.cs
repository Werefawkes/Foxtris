using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Piece", menuName = "Scriptable Objects/Piece")]
public class PieceSO : ScriptableObject
{
	public Tile[] tiles;
}
