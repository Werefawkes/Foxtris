using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Foxthorne.Utilities;
using Foxthorne.FoxScreens;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public VersionNumber version = new(VersionNumber.VersionPhase.Alpha, 0, 0, 1);

	[Header("References")]
	public TMPro.TMP_Text text;

	private void OnValidate()
	{
		text.text = version.ToString();
	}
}
