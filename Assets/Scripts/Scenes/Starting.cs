using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts;

public class Starting : MonoBehaviour
{
    public InputField path;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickLaunch()
    {
        App.GetInstance().setPath(path.text);

        SceneManager.LoadSceneAsync("Galleries");
    }
}
