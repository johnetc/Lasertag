using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class GameScreen  {

    #region singleton

    private static readonly GameScreen instance = new GameScreen ();

    public static GameScreen Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    // rename singleton instance variable
    public MenuMasterControl.MasterMenuDelegate InitiateUIDel;

    private GameObject m_GamePageContainer;
    private GameObject m_GamePagePrefab;
    private GameObject m_GamePagePrefabBorderParent;
    private Dictionary<string, Text> m_UITextDict = new Dictionary<string, Text>();
    private Dictionary<string , Button> m_UIButtonDict = new Dictionary<string , Button> ();

    private ParticleManager m_ParticleManager;
    
    //public Vector3 TopLeft;
    //public Vector3 BottomRight;

    public class PanelData
    {
        public Button PanelButton;
        public GameData.TouchLocation PanelLoc;
    }

    public void Update ()
    {
        m_ParticleManager.Update ();

    }

    public void Start()
    {
        ResizeSpawnArea();
    }
    
    public void ResizeSpawnArea ()
    {
        Vector3 tempSpawnTopLeft = ( GameData.TopLeftPoint - GameData.MidPoint );
        tempSpawnTopLeft = tempSpawnTopLeft.normalized;
        float distance = Vector3.Distance ( GameData.TopLeftPoint , GameData.MidPoint );
        GameData.TopLeftSpawnArea = ( GameData.MidPoint + ( ( GameData.SpawnScreenFraction * distance ) * tempSpawnTopLeft ) );
        GameData.TopLeftSpawnArea = new Vector3 ( GameData.TopLeftSpawnArea.x , GameData.TopLeftSpawnArea.y , GameData.CameraDepth );

        Vector3 tempSpawnBottomRight = ( GameData.BottomRightPoint - GameData.MidPoint );
        tempSpawnBottomRight = tempSpawnBottomRight.normalized;
        distance = Vector3.Distance ( GameData.BottomRightPoint , GameData.MidPoint );
        GameData.BottomRightSpawnArea = ( GameData.MidPoint + ( ( GameData.SpawnScreenFraction * distance ) * tempSpawnBottomRight ) );
        GameData.BottomRightSpawnArea = new Vector3 ( GameData.BottomRightSpawnArea.x , GameData.BottomRightSpawnArea.y , GameData.CameraDepth );
        //show points

        //GameObject sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        //sphere.transform.position = GameData.TopLeftSpawnArea;
        //sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        //sphere.transform.position = new Vector3 ( GameData.TopLeftSpawnArea.x , GameData.BottomRightSpawnArea.y , GameData.CameraDepth );
        //sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        //sphere.transform.position = GameData.BottomRightSpawnArea;
        //sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        //sphere.transform.position = new Vector3 ( GameData.BottomRightSpawnArea.x , GameData.TopLeftSpawnArea.y , GameData.CameraDepth );

    }


    public void LoadUIAssets ()
    {
        m_GamePageContainer = new GameObject ( "GameScreen" );
        m_GamePageContainer.transform.SetParent ( MenuMasterControl.Instance.MenuContainer.transform );
        m_GamePagePrefab = ( GameObject ) GameObject.Instantiate ( Resources.Load<GameObject> ( "Prefabs/MainGameScreen" ) );
        m_GamePagePrefab.transform.SetParent ( m_GamePageContainer.transform );
        m_GamePagePrefab.GetComponent<Canvas>().worldCamera = SceneManager.Instance.MainCamera;

        InitiateUIDel = InitiateOrEmptyUI;

        MenuMasterControl.Instance.RegisterPage ( InitiateUIDel , MenuMasterControl.MenuPages.Game );

        MenuInfoSender tempScript = m_GamePagePrefab.GetComponent<MenuInfoSender>();
        tempScript.GetPrefabComponents();
        LoadPrefabVariables ( tempScript );

        m_ParticleManager = new ParticleManager();
    }

    private void LoadActiveScreenPositions()
    {
        //Debug.Log("Load screen pos");
        RectTransform [] ids = m_GamePagePrefabBorderParent.GetComponentsInChildren<RectTransform> ();

        float leftX = ids [ 0 ].gameObject.transform.position.x;
        float rightX = ids [ 0 ].gameObject.transform.position.x; 
        float topY = ids [ 0 ].gameObject.transform.position.y; 
        float bottomY = ids [ 0 ].gameObject.transform.position.y; 
        
        foreach (var monoId in ids)
        {
            //Debug.Log(monoId.gameObject.transform.position);
            if (monoId.gameObject.transform.position.x > leftX)
            {
                rightX = monoId.gameObject.transform.position.x;
            }
            if (monoId.gameObject.transform.position.x < leftX)
            {
                leftX = monoId.gameObject.transform.position.x;
            }
            if ( monoId.gameObject.transform.position.y < topY )
            {
                bottomY = monoId.gameObject.transform.position.y;
            }
            if ( monoId.gameObject.transform.position.y > topY )
            {
                topY = monoId.gameObject.transform.position.y;
            }
        }

        GameData.TopLeftEnemyBorder = new Vector3 ( leftX , topY , GameData.CameraDepth );
        GameData.BottomRightEnemyBorder = new Vector3 ( rightX , bottomY , GameData.CameraDepth );
    }

    private void LoadPrefabVariables ( MenuInfoSender tempScript )
    {
        foreach (var button in tempScript.ButtonList)
        {
            m_UIButtonDict.Add(button.gameObject.name, button);
            
            PanelData tempPanel = new PanelData();
           
            tempPanel.PanelButton = button;
            switch ( button.name )
            {
                case "LeftPanel":
                    {
                        tempPanel.PanelLoc = GameData.TouchLocation.Left;
                        //Debug.Log ( SceneManager.Instance.MainCamera.WorldToViewportPoint(button.GetComponent<RectTransform> ().anchoredPosition ));
                        break;
                    }
                case "RightPanel":
                    {
                        tempPanel.PanelLoc = GameData.TouchLocation.Right;
                        break;
                    }
                case "TopPanel":
                    {
                        tempPanel.PanelLoc = GameData.TouchLocation.Top;
                        break;
                    }
                case "BottomPanel":
                    {
                        tempPanel.PanelLoc = GameData.TouchLocation.Bottom;
                        break;
                    }
            }

            AddListenerToButton ( tempPanel );
        }

        foreach (var text in tempScript.TextList)
        {
            m_UITextDict.Add(text.gameObject.name, text);
        }

        m_GamePagePrefabBorderParent = tempScript.MenuInfoGroup[1].gameObject;
        //Debug.Log(m_GamePagePrefabBorderParent.name);
    }
    private void AddListenerToButton ( PanelData panel )
    {
        panel.PanelButton.onClick.RemoveAllListeners ();
        panel.PanelButton.onClick.AddListener ( () => PanelTouched ( panel ));
    }

    private void PanelTouched ( PanelData panel )
    {
        m_ParticleManager.NewShotSystem(panel);
    }

    public void ModifyScore(int scoreToAdd)
    {
        GameData.CurrentScore += scoreToAdd;
        m_UITextDict["ScoreText"].text = GameData.CurrentScore.ToString();
    }

    public void ModifyLives(int livesToAdd)
    {
        GameData.CurrentLives += livesToAdd;
        m_UITextDict [ "LivesText" ].text = GameData.CurrentLives.ToString ();
        if (GameData.CurrentLives < 1)
        {
            SceneManager.Instance.CurrentState = SceneManager.GameState.Stop; 
        }
    }

    public void ResetScores()
    {
        GameData.CurrentScore = GameData.StartScore;
        GameData.CurrentLives = GameData.StartLives;
        ModifyScore(0);
        ModifyLives(0);
    }

    public void InitiateOrEmptyUI ( bool on )
    {
        if ( on ) { InitiateUI (); }
        else { EmptyUI (); }
    }

    public void InitiateUI ()
    {
        m_GamePageContainer.SetActive ( true );
        LoadActiveScreenPositions ();
        ResetScores();
        SceneManager.Instance.CurrentState = SceneManager.GameState.Play;
    }

    public void EmptyUI ()
    {
        m_GamePageContainer.SetActive ( false );
    }

    public void OnDrawGizmos ()
    {
        //if ( target != null )
        //{
        //Debug.Log("draw");    
        Gizmos.color = Color.blue;
            Gizmos.DrawLine ( GameData.TopLeftEnemyBorder , GameData.BottomRightEnemyBorder );
        //}
    }
}
