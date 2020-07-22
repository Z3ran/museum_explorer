using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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

    private void OnDestroy()
    {
        StopAllCoroutines();

        Caching.ClearCache();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
#else
        Resources.UnloadUnusedAssets();
#endif
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

    /// <summary>
    /// Retourne le nombre de cadre dans la room
    /// </summary>
    /// <returns></returns>
    public int GetNbFrames()
    {
        return this.transform.Find("frames").childCount;
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

    public void LoadImages(List<System.IO.FileInfo> images)
    {
        StartCoroutine(LoadImagesCoroutines(images));
    }

    public IEnumerator LoadImagesCoroutines(List<System.IO.FileInfo> images)
    { 
        var roomFrames = this.transform.Find("frames");
        var nbLoaded = 0;

        foreach (Transform frame in roomFrames.transform)
        {
            if (this._imageStartIndex + nbLoaded >= images.Count)
            {   // Désactive les tableaux
                frame.gameObject.SetActive(false);
                continue;
            }

            System.IO.FileInfo imageFile = images[this._imageStartIndex + nbLoaded];
            Transform image = frame.Find("Image");

            if (image)
            {
                yield return LoadFrameImageRoutine(imageFile, image.gameObject);

                //TODO gérer la position du texte en fonction du ratio de l'image
                var plaqueText = frame.Find("Plaque/Text").GetComponent<Text>().text = imageFile.Name;
            }

            nbLoaded++;
            yield return new WaitForSeconds(0.2f);
        }
    }

    [System.ObsoleteAttribute("Prend trop de temps utiliser plutot LoadFrameImageRoutine")]
    private async Task LoadFrameImage(System.IO.FileInfo imageFile, GameObject frameImage)
    {
        Debug.LogFormat("#1 LoadFrameImage {2} - t: {0}, dt: {1}", Time.unscaledTime, Time.deltaTime, imageFile.Name);
        System.IO.FileStream file = imageFile.OpenRead();

        byte[] fileData = new byte[file.Length];

        Debug.LogFormat("#2 LoadFrameImage {2} - t: {0}, dt: {1}", Time.unscaledTime, Time.deltaTime, imageFile.Name);
        await file.ReadAsync(fileData, 0, (int)file.Length);

        Debug.LogFormat("#3 LoadFrameImage {2} - t: {0}, dt: {1}", Time.unscaledTime, Time.deltaTime, imageFile.Name);
        
        // Load Image prend trop de temps et bloque le jeu.
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

        Debug.LogFormat("#4 LoadFrameImage {2} - t: {0}, dt: {1}", Time.unscaledTime, Time.deltaTime, imageFile.Name);

        //Debug.Log("Image found : " + image + " Load Texture : " + listOfImages[loadedImage].Name);
        frameImage.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);

        // Debug.Log(string.Format("Texture width {0}, height {1}", tex.width, tex.height));
        Debug.LogFormat("#5 LoadFrameImage {2} - t: {0}, dt: {1}", Time.unscaledTime, Time.deltaTime, imageFile.Name);
        //Get ratio :
        var ratio = tex.width / tex.height;

        // TODO Pour les images trop longue en hauteur ou en largeur les limités !!!
        if (tex.width > tex.height)
        {
            frameImage.transform.localScale = new Vector3(frameImage.transform.localScale.x * ((float)tex.width / (float)tex.height), 1, frameImage.transform.localScale.z);
        }
        else if (tex.height > tex.width)
        {
            // Comme l'image a son ancre au centre on doit remonter un peu l'image si on l'etire dans le sens de la hauteur. (10 egale le nombre d'unité par défaut)
            frameImage.transform.localPosition = new Vector3(
                frameImage.transform.localPosition.x, 
                ((frameImage.transform.localScale.z * ((float)tex.height / (float)tex.width) - (float)frameImage.transform.localScale.z) / 2) * 10, 
                frameImage.transform.localPosition.z);

            frameImage.transform.localScale = new Vector3(frameImage.transform.localScale.x, 1, frameImage.transform.localScale.z * ((float)tex.height / (float)tex.width));
        }
    }

    private IEnumerator LoadFrameImageRoutine(System.IO.FileInfo imageFile, GameObject frameImage)
    {
        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(@"file://" + imageFile.FullName))
        {
            yield return uwr.SendWebRequest();

            if (string.IsNullOrEmpty(uwr.error))
            {
                // Get downloaded asset bundle
                var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);

                //Debug.Log("Image found : " + image + " Load Texture : " + listOfImages[loadedImage].Name);
                frameImage.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);

                // Debug.Log(string.Format("Texture width {0}, height {1}", tex.width, tex.height));

                //Get ratio :
                var ratio = tex.width / tex.height;

                // TODO Pour les images trop longue en hauteur ou en largeur les limités !!!
                if (tex.width > tex.height)
                {
                    frameImage.transform.localScale = new Vector3(frameImage.transform.localScale.x * ((float)tex.width / (float)tex.height), 1, frameImage.transform.localScale.z);
                }
                else if (tex.height > tex.width)
                {
                    // Comme l'image a son ancre au centre on doit remonter un peu l'image si on l'etire dans le sens de la hauteur. (10 egale le nombre d'unité par défaut)
                    frameImage.transform.localPosition = new Vector3(
                        frameImage.transform.localPosition.x,
                        ((frameImage.transform.localScale.z * ((float)tex.height / (float)tex.width) - (float)frameImage.transform.localScale.z) / 2) * 10,
                        frameImage.transform.localPosition.z);

                    frameImage.transform.localScale = new Vector3(frameImage.transform.localScale.x, 1, frameImage.transform.localScale.z * ((float)tex.height / (float)tex.width));
                }
            }
            else
            {
                Debug.Log(uwr.error);
            }

        }
    }
}
