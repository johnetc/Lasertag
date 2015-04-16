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

    //private GameObject m_GamePageContainer;
    //private GameObject m_GamePagePrefab;
    //private GameObject m_GamePagePrefabBorderParent;
    private Dictionary<string, GameObject> _PrefabBlueprintDict;
    private Dictionary<string , GameObject> _GameobjUIDict; 

    // UI element reference dictionaries
    private Dictionary<string, Text> m_UITextDict = new Dictionary<string, Text>();
    private Dictionary<string , Button> m_UIButtonDict = new Dictionary<string , Button> ();
    private Dictionary<string , HorizontalOrVerticalLayoutGroup> m_UILayoutGroupDict = new Dictionary<string , HorizontalOrVerticalLayoutGroup> ();
    private Dictionary<string , LayoutElement> m_UILayoutElementDict = new Dictionary<string, LayoutElement>(); 

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
        public List<GameObject> UIShotRemainingObjList = new List<GameObject> (); 
    }

    public Dictionary<GameData.TouchLocation, PanelData> PanelDict;
    

    public void Initiate()
    {
        Reset ();
    }

    public void Play ()
    {
        CheckTouchTime();
    }

    public void Pause ()
    {
        PanelInteractivityOn(false);
    }

    public void Unpaused ()
    {
        PanelInteractivityOn ( true );
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

    public void LoadUIAssets ()
    {
        _PrefabBlueprintDict = new Dictionary<string, GameObject>();
        _GameobjUIDict = new Dictionary<string , GameObject> ();

        var resources = Resources.LoadAll ( "UI" );

        foreach ( var resource in resources )
        {
            if ( resource is GameObject )
            {
                _PrefabBlueprintDict.Add ( resource.name , ( GameObject ) resource );
            }
        }

        _GameobjUIDict.Add ( GameData.MainScreenContainer , new GameObject ( GameData.MainScreenContainer ) );
        _GameobjUIDict.Add ( GameData.MainScreen , GameObject.Instantiate ( _PrefabBlueprintDict [ GameData.MainScreen ] ) );

        _GameobjUIDict [ GameData.MainScreenContainer ].transform.SetParent ( MenuMasterControl.Instance.MenuContainer.transform );
        _GameobjUIDict [ GameData.MainScreen ].transform.SetParent ( _GameobjUIDict [ GameData.MainScreenContainer ].transform.transform );

        _GameobjUIDict [ GameData.MainScreen ].GetComponent<Canvas> ().worldCamera = SceneManager.Instance.MainCamera;

        InitiateUIDel = InitiateOrEmptyUI;

        MenuMasterControl.Instance.RegisterPage ( InitiateUIDel , MenuMasterControl.MenuPages.Game );

        MenuInfoSender tempScript = _GameobjUIDict [ GameData.MainScreen ].GetComponent<MenuInfoSender> ();
        tempScript.GetPrefabComponents();
        LoadPrefabVariables ( tempScript );

        //m_ParticleManager = new ParticleManager();
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
    public void LoadActiveScreenPositions()
    {
        //Debug.Log("Load screen pos");
        RectTransform [] ids = _GameobjUIDict["GamePagePrefabBorderParent"].GetComponentsInChildren<RectTransform> ();

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
        PanelDict = new Dictionary<GameData.TouchLocation, PanelData>();
        // load all UI prefabs into dictionaries
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
            if (!m_UITextDict.ContainsKey(text.gameObject.name))
            {
                m_UITextDict.Add(text.gameObject.name, text);
            }
        }

        foreach ( var layoutObj in tempScript.LayoutGroupList )
        {
            if ( !m_UILayoutGroupDict.ContainsKey ( layoutObj.gameObject.name ) )
            {
                m_UILayoutGroupDict.Add ( layoutObj.gameObject.name , layoutObj );
            }
        }

        foreach ( var layoutElement in tempScript.LayoutElements )
        {
            if ( !m_UILayoutElementDict.ContainsKey ( layoutElement.gameObject.name ) )
            {
                m_UILayoutElementDict.Add ( layoutElement.gameObject.name , layoutElement );
            }
        }
        _GameobjUIDict.Add( "GamePagePrefabBorderParent",   tempScript.MenuInfoGroup [ 1 ].gameObject);
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

        PanelDict.Add(tempPanel.PanelLoc, tempPanel);
    }

    private void AddListenerToPanel ( PanelData panel )
    {
        panel.PanelButton.onClick.RemoveAllListeners ();
        panel.PanelButton.onClick.AddListener ( () => PanelTouched ( panel ));
    }

    private void PanelTouched ( PanelData panel )
    {
        
        switch (SceneManager.Instance.CurrentInGameState)
        {
            case SceneManager.InGameState.Playing:
            {
                if ( !CheckTouchCount ( panel ) )
                {
                    return;
                }
                ParticleManager.Instance.NewShot( panel.PanelLoc );
            }
            break;
        }
        
    }

    private void ButtonTouched(string name)
    {
        //Debug.Log(name);
        switch (name)
        {
            case "ResetButton":
            {
                SceneManager.Instance.ResetGame();
            }
            break;

            case "PauseButton":
            {
                switch ( SceneManager.Instance.CurrentInGameState )
                {
                    case SceneManager.InGameState.Paused:
                        SceneManager.Instance.CurrentInGameState = SceneManager.InGameState.Unpaused;
                    break;
                    case SceneManager.InGameState.Playing:
                        SceneManager.Instance.CurrentInGameState = SceneManager.InGameState.Paused;
                    break;
                }
            }
            break;

            case "ExitButton":
            {
                SceneManager.Instance.QuitApplication();
            }
            break;
        }
    }

    private void PanelInteractivityOn(bool on)
    {
        foreach ( var panelData in PanelDict )
        {
            panelData.Value.PanelButton.interactable = on;
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
            ChangeUIShotCount ( panel , -1 );
            isIt = true;
        }
        if ( panel.TimeSincePressMS.ElapsedMilliseconds < GameData.PanelCooldownMS && panel.ConsecutiveTouches >= GameData.MaxTapsOnPanel+1 )
        {
            isIt = false;
        }

        return isIt;

    }

    private void CheckTouchTime()
    {
        foreach (var panelData in PanelDict)
        {
            //Debug.Log ( panelData.Value.PanelLoc + " " + panelData.Value.TimeSincePressMS.ElapsedMilliseconds );
            if ( panelData.Value.TimeSincePressMS.ElapsedMilliseconds > GameData.PanelCooldownMS && panelData.Value.ConsecutiveTouches == 0 )
            {
                panelData.Value.TimeSincePressMS.Reset ();

            }
            if ( panelData.Value.TimeSincePressMS.ElapsedMilliseconds > GameData.PanelCooldownMS && panelData.Value.ConsecutiveTouches > 0)
            {
                panelData.Value.ConsecutiveTouches--;
                ChangeUIShotCount(panelData.Value, 1);
                panelData.Value.TimeSincePressMS.Reset();
                panelData.Value.TimeSincePressMS.Start();
            }
            
        }

        // debug
        //m_UITextDict["LPText"].text = PanelDict[GameData.TouchLocation.Left].ConsecutiveTouches.ToString();
        //m_UITextDict [ "RPText" ].text = PanelDict [ GameData.TouchLocation.Right ].ConsecutiveTouches.ToString ();
        //m_UITextDict [ "TPText" ].text = PanelDict [ GameData.TouchLocation.Top ].ConsecutiveTouches.ToString ();
        //m_UITextDict [ "BPText" ].text = PanelDict [ GameData.TouchLocation.Bottom ].ConsecutiveTouches.ToString ();
    }

    private void ChangeUIShotCount(PanelData panel, int toAdd)
    {
        if (toAdd == 1)
        {
            panel.UIShotRemainingObjList[GameData.MaxTapsOnPanel - panel.ConsecutiveTouches-1].GetComponent<Image>().enabled = true;
        }
        else
        {
            panel.UIShotRemainingObjList [ GameData.MaxTapsOnPanel - panel.ConsecutiveTouches ].GetComponent<Image> ().enabled = false;
        }
    }

    public void ModifyUIText(TypeOfUIElement type, string textToChange, string newText)
    {
        switch (type)
        {
            case TypeOfUIElement.Text:
            {
                m_UITextDict[textToChange].text = newText;
            }
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
        _GameobjUIDict [ GameData.MainScreenContainer ].SetActive ( true );
    }

    private void ResetUI()
    {
        m_UIButtonDict [ "ResetButton" ].gameObject.SetActive ( false );
        ResetShotPanels();
        SetShotNumber ( GameData.MaxTapsOnPanel );
    }

    public void SetShotNumber(int number)
    {
        SetShotNumber(number,number,number,number);
    }

    public void SetShotNumber(int topNumber, int rightNumber,int bottomNumber,int leftNumber)
    {
        SetShotDisplay ( GameData.TouchLocation.Top , topNumber );
        SetShotDisplay ( GameData.TouchLocation.Right , rightNumber );
        SetShotDisplay ( GameData.TouchLocation.Bottom , bottomNumber );
        SetShotDisplay ( GameData.TouchLocation.Left , leftNumber );
    }

    private void ResetShotPanels()
    {
        foreach (var panelData in PanelDict)
        {
            List<GameObject> tempList = panelData.Value.UIShotRemainingObjList;
            for (int i = 1; i < tempList.Count; i++)
            {
                Object.Destroy(tempList[i]);
            }
            panelData.Value.UIShotRemainingObjList.Clear();
        }
    }

    private void SetShotDisplay(GameData.TouchLocation locationEnum, int number)
    {
        string location = locationEnum.ToString(); 
        
        List<GameObject> tempElements = new List<GameObject> ();

        PanelDict[locationEnum].UIShotRemainingObjList = tempElements;

        tempElements.Add ( m_UILayoutElementDict [ location + "SingleShot" ].gameObject );

        for (int i = 0; i < number-1; i++)
        {
            GameObject tempObj = GameObject.Instantiate ( m_UILayoutElementDict [ location+"SingleShot" ].gameObject );
            tempObj.transform.SetParent ( m_UILayoutGroupDict [ location+"ShotRemaining" ].gameObject.transform , false );
            tempElements.Add(tempObj);
        }

        m_UILayoutGroupDict[location + "ShotRemaining"].spacing = GameData.RemainingShotSpacing;

    }

    public void EmptyUI ()
    {
        _GameobjUIDict [ GameData.MainScreenContainer ].SetActive ( false );
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
