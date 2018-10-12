using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

public class GameLogic : MonoBehaviour {

	// Use this for initialization
	void Start () {
        NetworkManager.Instance.InstantiatePlayer(position: new Vector3(0, 5, 0));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
