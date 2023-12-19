using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PieceSet", menuName = "Scriptable Objects/PieceSet")]
public class PieceSetSO : ScriptableObject
{
	public List<PieceSO> pieces = new();
}
