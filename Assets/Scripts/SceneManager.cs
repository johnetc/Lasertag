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
        Unpaused,
        GameOver ,
        Reset,
    }

    public InGameState CurrentInGameState;

    public GameState CurrentState;

    public GameData.ParticleShotType CurrentParticleShotType;

    public void Awake ()
    {
        TheEventSystem = GameObject.FindObjectOfType<EventSystem>();
    }

    public void Start ()
    {
        // load object data
        GameData.CalculateScreenDimensions();
        GameScreen.Instance.ResizeSpawnArea();
        
        EnemyManager.Instance.Preload();
        ParticleManager.Instance.Preload();
        BackgroundGenerator.Instance.Preload();
        ItemManager.Instance.Preload();

        // load ui data
        MenuMasterControl.Instance.LoadPageAssets();
        MenuMasterControl.Instance.UnloadPages();
        MenuMasterControl.Instance.SwitchToPage ( MenuMasterControl.MenuPages.Game );

        CurrentState = GameState.InGame;
        CurrentInGameState = InGameState.Initiate;
        CurrentParticleShotType = GameData.ParticleShotType.BubbleShot;
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
                ParticleManager.Instance.Initiate();
                BackgroundGenerator.Instance.Initiate();
                ItemManager.Instance.Initiate();
                ResetScores();
                CurrentInGameState = InGameState.Playing;
                break;

            case InGameState.Paused:
                GameScreen.Instance.Pause(); 
                EnemyManager.Instance.Pause();
                ParticleManager.Instance.Pause();
                ItemManager.Instance.Pause();
                break;

            case InGameState.Unpaused:
                GameScreen.Instance.Unpaused ();
                EnemyManager.Instance.Unpaused ();
                ParticleManager.Instance.Unpaused();
                ItemManager.Instance.Unpaused();
                CurrentInGameState = InGameState.Playing;
                break;

            case InGameState.Playing:
                GameScreen.Instance.Play(); 
                EnemyManager.Instance.Play(); 
                ParticleManager.Instance.Play();
                ItemManager.Instance.Play();
                BackgroundGenerator.Instance.Play();
                break;

            case InGameState.Reset:
                GameScreen.Instance.Reset(); 
                EnemyManager.Instance.Reset();
                ParticleManager.Instance.Reset();
                ItemManager.Instance.Reset();
                ResetGame();
                break;

            case InGameState.GameOver:
                GameScreen.Instance.GameOver(); 
                EnemyManager.Instance.GameOver(); 
                ItemManager.Instance.GameOver();
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

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void OnDrawGizmos() 
    {
       GameScreen.Instance.OnDrawGizmos();
    }
}
