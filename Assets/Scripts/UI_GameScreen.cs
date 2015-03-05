using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class UI_GameScreen  {

    #region singleton

    private static readonly UI_GameScreen instance = new UI_GameScreen ();

    public static UI_GameScreen Instance
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
    private GameObject m_ParticlePrefab;

    private ParticleManager m_ParticleManager;
    
    private List<ParticleSystem.Particle> ActiveParticles = new List<ParticleSystem.Particle>();
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
