using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
    //private Dictionary<string , PanelData> m_TouchPanelDict = new Dictionary<string , PanelData> ();
    private Dictionary<TouchLocation, GameObject> m_ParticleDict = new Dictionary<TouchLocation, GameObject>(); 
    private List<GameObject> ActiveParticles = new List<GameObject>(); 

    private class PanelData
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

    }

    private void LoadPrefabVariables ( MenuInfoSender tempScript )
    {
        foreach (var button in tempScript.ButtonList)
        {
            PanelData tempPanel = new PanelData();
            //m_TouchPanelDict.Add ( button.name , tempPanel );
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

        m_ParticlePrefab = Resources.Load<GameObject>("Particles/ParticleSystem");
        //ParticleSystem tempPart = tempGameObj.GetComponent<ParticleSystem>();
        //tempPart = SetParticleVelocity(tempPart, Vector3.left);
        //m_ParticleDict.Add(TouchLocation.Right, tempGameObj);
        //tempPart = SetParticleVelocity ( tempPart , Vector3.right );
        //m_ParticleDict.Add ( TouchLocation.Left , tempGameObj );
        //tempPart = SetParticleVelocity ( tempPart , Vector3.up );
        //m_ParticleDict.Add ( TouchLocation.Bottom , tempGameObj );
        //tempPart = SetParticleVelocity ( tempPart , Vector3.down );
        //m_ParticleDict.Add ( TouchLocation.Top , tempGameObj );
    }

    //private ParticleSystem SetParticleVelocity(ParticleSystem partSys, Vector3 velocity)
    //{
    //    //part
        
    //    ParticleSystem.Particle [] p = new ParticleSystem.Particle [ 3 ];

    //    for (int i=0; i < p.Length; i++)
    //    {
    //        p[i].velocity = velocity*100;
    //        Debug.Log(p[i].velocity);
    //    }
    //    Debug.Log("done");
    //    partSys.SetParticles(p, p.Length);
    //    return partSys;
    //}

    private void AddListenerToButton ( PanelData panel )
    {
        panel.PanelButton.onClick.RemoveAllListeners ();
        //Debug.Log(panel.PanelButton.name);
        panel.PanelButton.onClick.AddListener ( () => PanelTouched ( panel ));
    }

    private void PanelTouched ( PanelData panel )
    {
        //Debug.Log("Panel "+panel.PanelLoc+ " " + Input.mousePosition);
        GameObject newParticle = GameObject.Instantiate(m_ParticlePrefab);
        ParticleSystem tempSys = newParticle.GetComponent<ParticleSystem>();
        ActiveParticles.Add ( newParticle );
        newParticle.transform.SetParent ( SceneManager.Instance.MainCamera.transform );
        newParticle.transform.position = SceneManager.Instance.MainCamera.ScreenToWorldPoint ( new Vector3 ( Input.mousePosition.x , Input.mousePosition.y , 100 ) );
        newParticle.GetComponent<ParticleColliderMono> ().thisLocation = TouchLocation.Right;

        switch (panel.PanelLoc)
        {
            case TouchLocation.Left:
            {
                tempSys.Emit(tempSys.transform.position, Vector3.right*100, 10f, 10, Color.red);
                break;
            }
            case TouchLocation.Right:
            {
                tempSys.Emit ( tempSys.transform.position , Vector3.left * 100 , 10f , 10 , Color.red );
                break;
            }
            case TouchLocation.Top:
            {
                tempSys.Emit ( tempSys.transform.position , Vector3.down * 100 , 10f , 10 , Color.red );
                break;
            }
            case TouchLocation.Bottom:
            {
                tempSys.Emit ( tempSys.transform.position , Vector3.up * 100 , 10f , 10 , Color.red );
                break;
            }

        }
        
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
