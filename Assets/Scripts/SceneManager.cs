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
        Menu,
        InGame
    }

    public enum InGameState
    {
        Initiate,
        Playing ,
        Paused ,
        GameOver ,
        Reset,
    }

    public InGameState CurrentInGameState;

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

        CurrentState = GameState.InGame;
        CurrentInGameState = InGameState.Initiate;
    }
    

    public void Update ()
    {
        MenuMasterControl.Instance.Update();

        switch (CurrentState)
        {
            case GameState.InGame:
                CheckInGameState();
            break;
            case GameState.Menu:
            //GameScreen.Instance.Play();
            break;
        }

        
    }

    public void CheckInGameState()
    {
        //Debug.Log(CurrentInGameState);
        switch ( CurrentInGameState )
        {
            case InGameState.Initiate:
                GameScreen.Instance.Initiate(); 
                EnemyManager.Instance.Initiate(); 
                ResetScores();
                CurrentInGameState = InGameState.Playing;
                break;
            case InGameState.Paused:
                GameScreen.Instance.Pause(); 
                EnemyManager.Instance.Pause(); 
                break;
            case InGameState.Playing:
                GameScreen.Instance.Play(); 
                EnemyManager.Instance.Play(); 
                break;
            case InGameState.Reset:
                GameScreen.Instance.Reset(); 
                EnemyManager.Instance.Reset(); 
                ResetGame();
                break;
            case InGameState.GameOver:
                //GameScreen.Instance.GameOver(); 
                //EnemyManager.Instance.GameOver(); 
                break;
        }
    }

    public void ModifyScore ( int scoreToAdd )
    {
        //Debug.Log("score mod" + scoreToAdd);
        GameData.CurrentScore += scoreToAdd;
        GameScreen.Instance.ModifyUIText ( GameScreen.TypeOfUIElement.Text , "ScoreText" , GameData.CurrentScore.ToString () );
        
    }

    public void ModifyLives ( int livesToAdd )
    {
        //Debug.Log ( "life mod" + livesToAdd );
        GameData.CurrentLives += livesToAdd;
        GameScreen.Instance.ModifyUIText ( GameScreen.TypeOfUIElement.Text , "LivesText" , GameData.CurrentLives.ToString () );
        
        if ( GameData.CurrentLives < 1 )
        {
            CurrentInGameState = InGameState.GameOver;
            GameOver();
        }
    }

    public void GameOver()
    {
        GameScreen.Instance.GameOver();
        EnemyManager.Instance.GameOver();
    }

    public void ResetGame ()
    {
        ResetScores ();
        
        CurrentInGameState = InGameState.Playing;

    }

    public void ResetScores ()
    {
        GameData.CurrentScore = GameData.StartScore;
        GameData.CurrentLives = GameData.StartLives;
        ModifyScore ( 0 );
        ModifyLives ( 0 );
    }

    public void OnDrawGizmos() 
    {
       GameScreen.Instance.OnDrawGizmos();
    }
}
