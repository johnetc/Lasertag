using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class MenuMasterControl {

    #region singleton

    private static readonly MenuMasterControl instance = new MenuMasterControl ();

    public static MenuMasterControl Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    public delegate void MasterMenuDelegate (bool on);

    public GameObject MenuContainer;

    public enum MenuPages
    {
        Game,
        none
    }

    public MenuPages CurrentMenuPage = MenuPages.none;

    public Dictionary<MenuPages , Delegate> MenupageDict = new Dictionary<MenuPages , Delegate> ();

    public void LoadPageAssets()
    {
        MenuContainer = new GameObject("Menu_Container");
        UI_GameScreen.Instance.LoadUIAssets();
    }

    public void UnloadPages()
    {
        UI_GameScreen.Instance.EmptyUI();
    }

    public void RegisterPage ( Delegate page , MenuPages pageType )
    {
        MenupageDict.Add(pageType, page);
        
    }

    public void SwitchToPage(MenuPages pageType)
    {
        //Debug.Log(pageType);
        if (CurrentMenuPage == pageType)
        {
            return;
        }
        
        ClearAllNormalPages();
        

        switch (pageType)
        {
            case MenuPages.Game:
            {
                MenupageDict[MenuPages.Game].DynamicInvoke(true);
                break;
            }
        }

        CurrentMenuPage = pageType;

    }

    public void ClearAllNormalPages()
    {
        foreach (var page in MenupageDict)
        {
            page.Value.DynamicInvoke(false);
        }
    }

    public void Update()
    {
        switch (CurrentMenuPage)
        {
            
        }
    }
}
