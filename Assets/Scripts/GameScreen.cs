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

    private ParticleManager m_ParticleManager;
    
    //public Vector3 TopLeft;
    //public Vector3 BottomRight;

    public class PanelData
    {
        public Button PanelButton;
        public GameData.TouchLocation PanelLoc;
    }

    public void Start()
    {
        ResizeSpawnArea();
    }

    public void Update()
    {
        m_ParticleManager.Update();
        
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

    private void LoadPrefabVariables ( MenuInfoSender tempScript )
    {
        foreach (var button in tempScript.ButtonList)
        {
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

    public void ResizeSpawnArea ()
    {
        Vector3 tempSpawnTopLeft = (GameData.TopLeftPoint - GameData.MidPoint);
        tempSpawnTopLeft = tempSpawnTopLeft.normalized;
        float distance = Vector3.Distance(GameData.TopLeftPoint, GameData.MidPoint);
        GameData.TopLeftSpawnArea = (GameData.MidPoint + ((GameData.SpawnScreenFraction*distance)*tempSpawnTopLeft));
        GameData.TopLeftSpawnArea = new Vector3(GameData.TopLeftSpawnArea.x,GameData.TopLeftSpawnArea.y,100);

        Vector3 tempSpawnBottomRight = ( GameData.BottomRightPoint - GameData.MidPoint );
        tempSpawnBottomRight = tempSpawnBottomRight.normalized;
        distance = Vector3.Distance ( GameData.BottomRightPoint , GameData.MidPoint );
        GameData.BottomRightSpawnArea = ( GameData.MidPoint + ( ( GameData.SpawnScreenFraction * distance ) * tempSpawnBottomRight ) );
        GameData.BottomRightSpawnArea = new Vector3(GameData.BottomRightSpawnArea.x,GameData.BottomRightSpawnArea.y,100);
        //show points

        GameObject sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        sphere.transform.position = GameData.TopLeftSpawnArea;
        sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        sphere.transform.position = new Vector3 ( GameData.TopLeftSpawnArea.x , GameData.BottomRightSpawnArea.y , 100 );
        sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        sphere.transform.position = GameData.BottomRightSpawnArea;
        sphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere );
        sphere.transform.position = new Vector3 ( GameData.BottomRightSpawnArea.x , GameData.TopLeftSpawnArea.y , 100 );
        
    }

    public void InitiateOrEmptyUI ( bool on )
    {
        if ( on ) { InitiateUI (); }
        else { EmptyUI (); }
    }

    public void InitiateUI ()
    {
        m_GamePageContainer.SetActive ( true );
    }

    public void EmptyUI ()
    {
        m_GamePageContainer.SetActive ( false );
    }
}
