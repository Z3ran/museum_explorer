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

    /// <summary>
    /// Pour l'instant garde en mémoire la liste des images dans le répertoire à parcourir
    /// </summary>
    List<FileInfo> listOfImages;

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

        listOfImages = new List<FileInfo>();

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
                room = this.createNewRoom(loadedImage, previousRoom, (int)previousConnection.Value);
            }
            else
            {
                room = this.createNewRoom(loadedImage);
            }

            currentRoom = room.GetComponent<Room>();
            currentRoom.LoadImages(listOfImages);

            loadedImage += currentRoom.GetNbFrames();
            previousRoom = currentRoom;
        }

        Debug.Log( "All Image has been loaded" );

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

    private GameObject createNewRoom(int imageIndex, Room fromRoom = null, int? connectionIndex = null)
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
        room.GetComponent<Room>().Init(this.roomsList.Count - 1, imageIndex);

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
                var nbFrames = lastRoomFound.GetNbFrames();
                var newConnectionIndex = lastRoomFound.GetNextConnectionIndex(Room.GetReverseConnectionIndex(lastRoomConnectionIndex.Value));

                int newRoomImageIndex = lastRoomFound.imageStartIndex + nbFrames * ((lastRoomFound.imageStartIndex > room.imageStartIndex) ? 1 : -1);

                // TODO gérer le nombre de chambre
                if (newRoomImageIndex < 0) return;
                
                var newRoomGameObject = this.createNewRoom(newRoomImageIndex, lastRoomFound, (int)newConnectionIndex.Value);
                var newRoom = newRoomGameObject.GetComponent<Room>();
                // TODO mettre à jour les tableaux !
                newRoom.LoadImages(this.listOfImages);
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
                var previousRoom = lastRoomFound.GetRoomOnConnection(Room.GetReverseConnectionIndex(lastRoomConnectionIndex.Value));
                previousRoom.RemoveRoomConnectionOn(lastRoomConnectionIndex.Value);

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
