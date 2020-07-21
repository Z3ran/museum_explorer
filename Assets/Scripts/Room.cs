using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomConnectionEnum 
{
    TOP,
    RIGHT,
    BOTTOM,
    LEFT
}

public class Room : MonoBehaviour {

    private Random.State roomRandomState;

    private bool isCurrentRoom = false;
    private int roomIndex = -1;

    /// <summary>
    /// L'index de la 1er image dans le répertoire courant qu'on affiche dans la room
    /// </summary>
    private int _imageStartIndex;

    public int imageStartIndex
    {
        get { return _imageStartIndex; }
        set { _imageStartIndex = value; }
    }

    [SerializeField()]
    private List<GameObject> connectionsWall;

    /// <summary>
    /// Connections flag :
    /// top : 1
    /// right : 2
    /// bottom : 4
    /// left : 8
    /// </summary>
    [SerializeField()]
    private short connectionsFlag;

    private Room[] roomConnections = { null, null, null, null };

    public static RoomConnectionEnum GetReverseConnectionIndex( RoomConnectionEnum index )
    {
        return (RoomConnectionEnum)(((int)index + 2) % 4);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(int index, int imageIndex)
    {
        this.roomIndex = index;
        this._imageStartIndex = imageIndex;
    }

    public int GetIndex()
    {
        return this.roomIndex;
    }

    public void SetRoomConnectionOn( Room room, RoomConnectionEnum connectionEnum )
    {
        this.roomConnections[(int)connectionEnum] = room;

        if (this.connectionsWall[(int)connectionEnum] != null)
        {
            this.connectionsWall[(int)connectionEnum].SetActive(false);
        }
    }

    public Room RemoveRoomConnectionOn(RoomConnectionEnum connectionEnum)
    {
        Room room = this.roomConnections[(int)connectionEnum];

        this.roomConnections[(int)connectionEnum] = null;

        if (this.connectionsWall[(int)connectionEnum] != null)
        {
            this.connectionsWall[(int)connectionEnum].SetActive(true);
        }

        return room;
    }

    public Room GetRoomOnConnection(RoomConnectionEnum connectionEnum)
    {
        return this.roomConnections[(int)connectionEnum];
    }

    /// <summary>
    /// Recupère la prochaine room connecter a celle là en evitant la room de la connection en paramètre
    /// </summary>
    /// <param name="connectionIndex">l'index de la connexion à exclure</param>
    /// <returns>La prochaine room, l'index de connexion</returns>
    public (Room, RoomConnectionEnum?) GetNextRoomConnection(RoomConnectionEnum connectionIndex)
    {
        for (var i = 0; i <= (int)RoomConnectionEnum.LEFT; i++)
        {
            if (i == (int)connectionIndex) continue;

            if (this.roomConnections[i] != null)
            {
                return (this.roomConnections[i], (RoomConnectionEnum)i);
            }
        }

        return (null, null);
    }

    /// <summary>
    /// Retourne la prochaine connection disponible dans la room en excluant l'index donnée en paramêtre
    /// </summary>
    /// <param name="fromConnectionIndex">l'index à exclure</param>
    /// <returns>La connection disponible ou null si aucune de disponible</returns>
    public RoomConnectionEnum? GetNextConnectionIndex(RoomConnectionEnum? fromConnectionIndex)
    {
        for (var i = 0; i <= (int)RoomConnectionEnum.LEFT; i++)
        {
            if (fromConnectionIndex.HasValue && i == (int)fromConnectionIndex.Value) continue;

            if ( this.roomConnections[i] == null && ((this.connectionsFlag >> i) & 0x1) == 1 )
            {
                return (RoomConnectionEnum)i;
            }
        }

        return null;
    }

    public void onLeave( Room newRoom )
    {
        isCurrentRoom = false;
    }

    public void onAreaEnter( Collider other, int connectionIndex )
    {
        if (!this.isCurrentRoom)
        {
            this.isCurrentRoom = true;
            Galleries gallerie = this.transform.parent.GetComponent<Galleries>();

            if ( gallerie )
            {
                gallerie.onRoomEnter(this, connectionIndex);
            }

            Debug.Log("Enter Room");
        }
    }
}
