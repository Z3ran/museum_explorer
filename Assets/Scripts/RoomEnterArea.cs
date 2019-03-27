using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnterArea : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Room room = this.transform.parent.parent.gameObject.GetComponent<Room>();

        if ( room )
        {
            room.onAreaEnter(other);
        }
    }
}
