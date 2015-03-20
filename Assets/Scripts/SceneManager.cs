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

    public enum GameState
    {
        Play,
        Stop
    }

    public GameState CurrentState;

    public void Awake ()
    {
        TheEventSystem = GameObject.FindObjectOfType<EventSystem>();
    }

    public void Start ()
    {
        GameData.CalculateScreenDimensions();
        GameScreen.Instance.Start();
        EnemyManager.Instance.Start();

        MenuMasterControl.Instance.LoadPageAssets();
        MenuMasterControl.Instance.UnloadPages();
        MenuMasterControl.Instance.SwitchToPage ( MenuMasterControl.MenuPages.Game );
    }
    

    public void Update ()
    {
        MenuMasterControl.Instance.Update();

        switch (CurrentState)
        {
                case GameState.Play:
                GameScreen.Instance.Update();
                EnemyManager.Instance.Update();
                break;
                case GameState.Stop:
                GameScreen.Instance.Update();
                break;
        }

        
    }

    public void OnDrawGizmos() 
    {
       GameScreen.Instance.OnDrawGizmos();
    }
}
