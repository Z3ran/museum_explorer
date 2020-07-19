using Assets.Scripts;
using System;
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
    private int roomIndexCount = 0;

    // Le nombre de piece mise en cache
    private const int ROOM_CACHE_NB = 5;


    private int SortFileByCreationDate(FileInfo file1, FileInfo file2)
    {
        if (file1.CreationTimeUtc > file2.CreationTimeUtc)
        {
            return 1;
        }
        else
        {
            return -1;
        }

        return 0;
    }

	// Use this for initialization
	void Start () {
        string myPath = App.GetInstance().getPath();

        this.roomsList = new List<GameObject>();

        Debug.Log("Directory Path : " + myPath);

        DirectoryInfo dir = new DirectoryInfo(myPath);

        List<FileInfo> listOfImages = new List<FileInfo>();

        listOfImages.AddRange( dir.GetFiles("*.png") );
        listOfImages.AddRange( dir.GetFiles("*.jpg") );

        listOfImages.Sort(SortFileByCreationDate);   

        int loadedImage = 0;
        int nbMaxToLoad = Math.Min(40, listOfImages.Count);

        Room previousRoom = null;
        Room currentRoom = null;
        RoomConnectionEnum? previousConnection = null;

        while ( loadedImage < nbMaxToLoad)
        {
            GameObject room = null;

            if (previousRoom != null)
            {
                RoomConnectionEnum? reverseConnection = null;

                if (previousConnection.HasValue)
                {
                    reverseConnection = Room.GetReverseConnectionIndex(previousConnection.Value);
                }

                previousConnection = previousRoom.GetNextConnectionIndex(reverseConnection);
                room = this.createNewRoom(previousRoom, (int)previousConnection.Value);
            }
            else
            {
                room = this.createNewRoom();
            }

            currentRoom = room.GetComponent<Room>();

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

            previousRoom = currentRoom;
        }

        Debug.Log( "All Image has been loaded" );

        // TODO Désactiver les tableaux en trop

        // TODO Ajouter une animation au texte pour le faire disparaitre !
        this.UpdateGUIPositionText( "NEXUS" );

        // Choisi la piece du millieu
        int currentRoomIndex = (int)Math.Floor(this.roomsList.Count / 2.0f);

        this.currentRoom = this.roomsList[currentRoomIndex].GetComponent<Room>();
        this.currentRoom.onAreaEnter(null, -1);

        // positionne le joueur au centre de la piece principale : X Longueur, Y : Hauteur, Z : Largeur
        player.transform.position = new Vector3(this.currentRoom.gameObject.transform.position.x, 5, this.currentRoom.gameObject.transform.position.z);
    }
	
	// Update is called once per frame
	void Update () {
        this.hud.transform.Find("DebugText").GetComponent<Text>().text = string.Format( "DEBUG - Position : x = {0} y = {1} z = {2}", this.mainCamera.transform.position.x, this.mainCamera.transform.position.y, this.mainCamera.transform.position.z );
    }

    private GameObject createNewRoom(Room fromRoom = null, int? connectionIndex = null)
    {
        Vector3 pos;

        if (fromRoom && connectionIndex.HasValue)
        {
            float x = 0;
            float z = 0;

            if (connectionIndex.Value % 2 == 0)
            {
                z = 25f * (connectionIndex.Value - 1) * -1;
            }
            else
            {
                x = 50f * (connectionIndex.Value - 2) * -1;
            }

            pos = new Vector3(fromRoom.gameObject.transform.position.x + x, 0, fromRoom.gameObject.transform.position.z + z);
        }
        else
        {
            // 50 car de base le plane par défaut fait 10 unités et on la scale 5x donc 50 unités de large !
            pos = new Vector3(50 * this.roomsList.Count, 0, 0);
        }


        this.roomsList.Add( Instantiate( this.roomPrefab, pos, Quaternion.identity, this.transform) );
        var room = this.roomsList[this.roomsList.Count - 1];
        room.GetComponent<Room>().Init(this.roomsList.Count - 1);

        if (fromRoom && connectionIndex.HasValue)
        {
            fromRoom.SetRoomConnectionOn(room.GetComponent<Room>(), (RoomConnectionEnum)connectionIndex.Value);
            room.GetComponent<Room>().SetRoomConnectionOn(fromRoom, Room.GetReverseConnectionIndex((RoomConnectionEnum)connectionIndex.Value));
        }

        room.name = string.Format("ROOM({0})", this.roomIndexCount);
        this.roomIndexCount++;

        return room;
    }

    public void onRoomEnter( Room room, int connectionIndex )
    {
        if (this.currentRoom == room) return;

        this.currentRoom.onLeave( room );
        this.currentRoom = room;

        Debug.LogFormat("ENTER ROOM : {0}", room.gameObject.name);

        UpdateGUIPositionText(string.Format("Room #{0}", room.GetIndex()));

        if (connectionIndex != -1)
        {
            // Crée la room suivante
            Room currentRoom = room;
            Room lastRoomFound = null;
            RoomConnectionEnum? lastRoomConnectionIndex = null;
            RoomConnectionEnum? currentRoomConnectionIndex = (RoomConnectionEnum)connectionIndex;

            int safeLoop = 0;

            do
            {
                (currentRoom, currentRoomConnectionIndex) = currentRoom.GetNextRoomConnection(Room.GetReverseConnectionIndex(currentRoomConnectionIndex.Value));
                
                if (currentRoom != null)
                {
                    lastRoomConnectionIndex = currentRoomConnectionIndex;
                    lastRoomFound = currentRoom;
                }

                safeLoop++;
            }
            while (currentRoom != null && safeLoop < 100);
            
            if (lastRoomFound != null)
            {
                var newConnectionIndex = lastRoomFound.GetNextConnectionIndex(Room.GetReverseConnectionIndex(lastRoomConnectionIndex.Value));

                var newRoomGameObject = this.createNewRoom(lastRoomFound, (int)newConnectionIndex.Value);
                var newRoom = newRoomGameObject.GetComponent<Room>();
                // TODO mettre à jour les tableaux !
                Debug.LogFormat("CREATE ROOM {0}", newRoomGameObject.name);
            }

            // Supprime la room à l'extreme opposé
            currentRoomConnectionIndex = Room.GetReverseConnectionIndex((RoomConnectionEnum)connectionIndex);
            currentRoom = room;
            lastRoomFound = null;
            lastRoomConnectionIndex = null;

            safeLoop = 0;

            do
            {
                (currentRoom, currentRoomConnectionIndex) = currentRoom.GetNextRoomConnection(Room.GetReverseConnectionIndex(currentRoomConnectionIndex.Value));

                if (currentRoom != null)
                {
                    lastRoomConnectionIndex = currentRoomConnectionIndex;
                    lastRoomFound = currentRoom;
                }

                safeLoop++;
            }
            while (currentRoom != null && safeLoop < 100);

            if (lastRoomFound != null)
            {
                this.roomsList.Remove(lastRoomFound.gameObject);

                Debug.LogFormat("DELETE ROOM {0}", lastRoomFound.name);

                Destroy(lastRoomFound.gameObject);
            }
        }
    }

    private void UpdateGUIPositionText(string text)
    {
        this.hud.transform.Find("CurrentRoomText").GetComponent<Text>().text = text;
    }
}
