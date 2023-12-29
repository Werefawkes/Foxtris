using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Palette", menuName = "Scriptable Objects/Palette")]
public class PaletteSO : ScriptableObject
{
	public List<Color> colors = new();
}
