using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SceneManager  {

    #region singleton

    private static readonly SceneManager instance = new SceneManager ();

    public static SceneManager Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    public Camera MainCamera;
    public EventSystem TheEventSystem;

    public void Awake ()
    {
        TheEventSystem = GameObject.FindObjectOfType<EventSystem>();
    }

    public void Start ()
    {
        MenuMasterControl.Instance.LoadPageAssets();
        MenuMasterControl.Instance.UnloadPages();
        MenuMasterControl.Instance.SwitchToPage ( MenuMasterControl.MenuPages.Game );
    }
    

    public void Update ()
    {
        MenuMasterControl.Instance.Update();
    }
}
