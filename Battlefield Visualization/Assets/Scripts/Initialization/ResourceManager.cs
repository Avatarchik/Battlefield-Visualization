using UnityEngine;
using System.Collections;

public class ResourceManager : MonoBehaviour {

	//contains data that is to be passed
	public string blueFilePath;
	public string neutralFilePath;
	public string redFilePath;

	void Awake() 
	{
		// stops object from automatically destroyed on loading a scene
		DontDestroyOnLoad (this);
	}

}
