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

    //private ParticleManager m_ParticleManager;
    public enum TypeOfUIElement
    {
        Panel ,
        Button ,
        Text
    }

    public class PanelData
    {
        public Button PanelButton;
        public GameData.TouchLocation PanelLoc;
        public Stopwatch TimeSincePressMS;
        public int ConsecutiveTouches;
    }

    public Dictionary<GameData.TouchLocation, PanelData> PanelList; 

    //public void Update ()
    //{
        

    //}

    public void Initiate()
    {
        Reset ();
    }

    public void Play ()
    {
        //Debug.Log ( "screen play" );
        ParticleManager.Instance.Update ();
        CheckTouchTime();

    }

    public void Pause ()
    {

    }

    public void Reset ()
    {
        Debug.Log("screen reset");
        LoadActiveScreenPositions ();
        ResetUI ();
    }

    public void GameOver ()
    {
        Debug.Log ( "screen game over" );
        m_UIButtonDict["ResetButton"].gameObject.SetActive(true);

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

        //m_ParticleManager = new ParticleManager();
    }

    public void LoadActiveScreenPositions()
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
        PanelList = new Dictionary<GameData.TouchLocation, PanelData>();
        
        foreach (var button in tempScript.ButtonList)
        {
            m_UIButtonDict.Add(button.gameObject.name, button);

            if (button.gameObject.name.Contains("Panel"))
            {
                AddPanel(button);
            }
            else
            {
                AddListenerToButton(button);
            }
        }

        foreach (var text in tempScript.TextList)
        {
            m_UITextDict.Add(text.gameObject.name, text);
        }

        m_GamePagePrefabBorderParent = tempScript.MenuInfoGroup[1].gameObject;
        //Debug.Log(m_GamePagePrefabBorderParent.name);
    }

    private void AddListenerToButton ( Button button )
    {
        button.onClick.RemoveAllListeners ();
        button.onClick.AddListener ( () => ButtonTouched( button.gameObject.name ) );
    }

    private void AddPanel(Button button)
    {
        PanelData tempPanel = new PanelData ();
        tempPanel.TimeSincePressMS = new Stopwatch();
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

        AddListenerToPanel ( tempPanel );

        PanelList.Add(tempPanel.PanelLoc, tempPanel);
    }

    private void AddListenerToPanel ( PanelData panel )
    {
        panel.PanelButton.onClick.RemoveAllListeners ();
        panel.PanelButton.onClick.AddListener ( () => PanelTouched ( panel ));
    }

    private void PanelTouched ( PanelData panel )
    {
        if (!CheckTouchCount(panel))
        {
            return;
        };
        
        switch (SceneManager.Instance.CurrentInGameState)
        {
            case SceneManager.InGameState.Playing:
            { 
                ParticleManager.Instance.NewShotSystem ( panel );
            }
            break;
        }
        
    }

    private void ButtonTouched(string name)
    {
        switch (name)
        {
            case "ResetButton":
            {
                SceneManager.Instance.ResetGame();
            }
            break;
        }
    }

    private bool CheckTouchCount(PanelData panel)
    {
        bool isIt = false;
        
        if (panel.TimeSincePressMS.ElapsedMilliseconds > GameData.PanelCooldownMS)
        {
            panel.TimeSincePressMS.Reset ();
            panel.TimeSincePressMS.Start ();
            isIt = true;
        }
        if ( panel.TimeSincePressMS.ElapsedMilliseconds < GameData.PanelCooldownMS && panel.ConsecutiveTouches < GameData.MaxTapsOnPanel )
        {
            panel.TimeSincePressMS.Reset ();
            panel.TimeSincePressMS.Start ();
            panel.ConsecutiveTouches++;
            isIt = true;
        }
        if ( panel.TimeSincePressMS.ElapsedMilliseconds < GameData.PanelCooldownMS && panel.ConsecutiveTouches >= GameData.MaxTapsOnPanel )
        {
            isIt = false;
        }

        return isIt;

    }

    private void CheckTouchTime()
    {
        foreach (var panelData in PanelList)
        {
            //Debug.Log ( panelData.Value.PanelLoc + " " + panelData.Value.TimeSincePressMS.ElapsedMilliseconds );
            if ( panelData.Value.TimeSincePressMS.ElapsedMilliseconds > GameData.PanelCooldownMS && panelData.Value.ConsecutiveTouches == 0 )
            {
                panelData.Value.TimeSincePressMS.Reset ();

            }
            if ( panelData.Value.TimeSincePressMS.ElapsedMilliseconds > GameData.PanelCooldownMS && panelData.Value.ConsecutiveTouches > 0)
            {
                panelData.Value.ConsecutiveTouches--;
                panelData.Value.TimeSincePressMS.Reset();
                panelData.Value.TimeSincePressMS.Start();
            }
            
        }

        // debug
        m_UITextDict["LPText"].text = PanelList[GameData.TouchLocation.Left].ConsecutiveTouches.ToString();
        m_UITextDict [ "RPText" ].text = PanelList [ GameData.TouchLocation.Right ].ConsecutiveTouches.ToString ();
        m_UITextDict [ "TPText" ].text = PanelList [ GameData.TouchLocation.Top ].ConsecutiveTouches.ToString ();
        m_UITextDict [ "BPText" ].text = PanelList [ GameData.TouchLocation.Bottom ].ConsecutiveTouches.ToString ();
    }

    public void ModifyUIText(TypeOfUIElement type, string textToChange, string newText)
    {
        switch (type)
        {
            case TypeOfUIElement.Text:
            m_UITextDict[textToChange].text = newText;
            break;
        }
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

    private void ResetUI()
    {
        m_UIButtonDict [ "ResetButton" ].gameObject.SetActive ( false );
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
