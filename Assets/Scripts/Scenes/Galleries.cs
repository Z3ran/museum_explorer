using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Galleries : MonoBehaviour {

    public GameObject roomPrefab;

    public GameObject mainCamera;

    public GameObject hud;

    public GameObject player;

    private List<GameObject> roomsList;

    private Room currentRoom;

    // Le nombre de piece mise en cache
    private const int ROOM_CACHE_NB = 10;

    // https://answers.unity.com/questions/139808/creating-a-plane-mesh-directly-from-code.html

	// Use this for initialization
	void Start () {
        string myPath = App.GetInstance().getPath();

        this.roomsList = new List<GameObject>();

        Debug.Log("Directory Path : " + myPath);

        DirectoryInfo dir = new DirectoryInfo(myPath);

        List<FileInfo> listOfImages = new List<FileInfo>();

        //FileInfo[] info = dir.GetFiles("*.png");
        //FileInfo[] infojpg = dir.GetFiles("*.jpg");

        listOfImages.AddRange( dir.GetFiles("*.png") );
        listOfImages.AddRange( dir.GetFiles("*.jpg") );
   
        // TODO sort images list by name         

        int loadedImage = 0;

        while ( loadedImage < listOfImages.Count )
        {
            GameObject room = this.createNewRoom();
            var roomFrames = room.transform.Find("frames");

            foreach (Transform frame in roomFrames.transform)
            {
                Transform image = frame.Find("Image");

                if (image)
                {
                    if (loadedImage < listOfImages.Count )
                    {
                        FileStream file = listOfImages[loadedImage].OpenRead();

                        byte[] fileData = new byte[file.Length];
                        file.Read(fileData, 0, (int)file.Length);

                        var tex = new Texture2D(2, 2);
                        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                        //Debug.Log("Image found : " + image + " Load Texture : " + listOfImages[loadedImage].Name);
                        image.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);

                        // Debug.Log(string.Format("Texture width {0}, height {1}", tex.width, tex.height));

                        //Get ratio :
                        var ratio = tex.width / tex.height;

                        // TODO Pour les images trop longue en hauteur ou en largeur les limités !!!
                        if (tex.width > tex.height)
                        {
                            image.localScale = new Vector3(image.localScale.x * ((float)tex.width / (float)tex.height), 1, image.localScale.z);
                        }
                        else if (tex.height > tex.width)
                        {
                            // Comme l'image a son encre au centre on doit remonter un peu l'image si on l'etire dans le sens de la hauteur. (10 egale le nombre d'unité par défaut)
                            image.localPosition = new Vector3(image.localPosition.x, ((image.localScale.z * ((float)tex.height / (float)tex.width) - (float)image.localScale.z) / 2) * 10, image.localPosition.z);
                            image.localScale = new Vector3(image.localScale.x, 1, image.localScale.z * ((float)tex.height / (float)tex.width));
                        }

                        //TODO gérer la position du texte en fonction du ratio de l'image
                        var plaqueText = frame.Find("Plaque/Text").GetComponent<Text>().text = listOfImages[loadedImage].Name;

                        loadedImage++;
                    }
                }
            }
        }

        Debug.Log( "All Image has been loaded" );

        // TODO Désactiver les tableaux en trop

        // positionne la caméra au centre des pieces : X Longueur, Y : Hauteur, Z : Largeur
        mainCamera.transform.position = new Vector3( 0, 4, 0 );

        // TODO Ajouter une animation au texte pour le faire disparaitre !
        this.hud.transform.Find("CurrentRoomText").GetComponent<Text>().text = "NEXUS";

        this.currentRoom = this.roomsList[0].GetComponent<Room>();
        this.currentRoom.onAreaEnter(null);
    }
	
	// Update is called once per frame
	void Update () {
        this.hud.transform.Find("DebugText").GetComponent<Text>().text = string.Format( "DEBUG - Position : x = {0} y = {1} z = {2}", this.mainCamera.transform.position.x, this.mainCamera.transform.position.y, this.mainCamera.transform.position.z );

        this.player.transform.position = new Vector3( this.mainCamera.transform.position.x, this.mainCamera.transform.position.y, this.mainCamera.transform.position.z );
    }

    private GameObject createNewRoom()
    {
        // 50 car de base le plane par défaut fait 10 unités et on la scale 5x donc 50 unités de large !
        var pos = new Vector3( 50 * this.roomsList.Count, 0, 0);

        this.roomsList.Add( Instantiate( this.roomPrefab, pos, Quaternion.identity, this.transform) );

        return this.roomsList[this.roomsList.Count - 1];
    }

    public void onRoomEnter( Room room )
    {
        this.currentRoom.onLeave( room );
        this.currentRoom = room;
    }
}
