using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        var door = this.transform.Find("door");

        door.gameObject.SetActive(!isOpen);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

