using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PunSingleton<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{

	private static T _instance;
	private static readonly object _instanceLock = new object();
	private static bool _quitting = false;

	public static T instance
	{
		get => _instance;
		protected set => _instance = value;
	}

	protected virtual void Awake()
	{
		CheckSingleton();
	}

	protected virtual void OnApplicationQuit()
	{
		_quitting = true;
	}


	protected void CheckSingleton()
    {
		lock (_instanceLock)
		{
			if (instance == null && !_quitting)
			{

				instance = GameObject.FindObjectOfType<T>();
				if (instance == null)
				{
					instance = gameObject.AddComponent<T>();
				}
			}
		}
	}
}
