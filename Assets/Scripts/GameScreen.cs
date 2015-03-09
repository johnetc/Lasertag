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
    private EnemyManager m_EnemyManager;
    
    public Vector3 TopLeft;
    public Vector3 BottomRight;

    public class PanelData
    {
        public Button PanelButton;
        public TouchLocation PanelLoc;
    }

    public enum TouchLocation
    {
        Top,
        Left,
        Bottom,
        Right,
    }

    public void Update()
    {
        m_ParticleManager.Update();
        m_EnemyManager.CheckObjectExistence();
        m_EnemyManager.CheckObjectCreation();
        m_EnemyManager.ObjectMovement();
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
        m_EnemyManager = new EnemyManager();
    }

    private void LoadPrefabVariables ( MenuInfoSender tempScript )
    {
        TopLeft = SceneManager.Instance.MainCamera.ViewportToWorldPoint ( new Vector3 ( 0 , 1 , SceneManager.Instance.MainCamera.nearClipPlane ) ) ;
        BottomRight = SceneManager.Instance.MainCamera.ViewportToWorldPoint ( new Vector3 ( 1 , 0 , SceneManager.Instance.MainCamera.nearClipPlane ) );
        
        foreach (var button in tempScript.ButtonList)
        {
            PanelData tempPanel = new PanelData();
           
            tempPanel.PanelButton = button;
            switch ( button.name )
            {
                case "LeftPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Left;
                        break;
                    }
                case "RightPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Right;
                        break;
                    }
                case "TopPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Top;
                        break;
                    }
                case "BottomPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Bottom;
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
   
    public void ColliderResult()
    {
        
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
