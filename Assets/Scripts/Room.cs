using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    private Random.State roomRandomState;

    private bool isCurrentRoom = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void onLeave( Room newRoom )
    {
        isCurrentRoom = false;
    }

    public void onAreaEnter( Collider other )
    {
        if (!this.isCurrentRoom)
        {
            this.isCurrentRoom = true;
            Galleries gallerie = this.transform.parent.GetComponent<Galleries>();

            if ( gallerie )
            {
                gallerie.onRoomEnter(this);
            }

            Debug.Log("Enter Room");
        }
    }
}
