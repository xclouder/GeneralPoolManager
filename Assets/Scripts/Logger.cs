using UnityEngine;
using System.Collections;

public class Logger {

	public static void Debug(string msg)
	{
		UnityEngine.Debug.Log(msg);
	}

	public static void Warning(string msg)
	{
		UnityEngine.Debug.LogWarning(msg);
	}

	public static void Error(string msg)
	{
		UnityEngine.Debug.LogError(msg);
	}

}
