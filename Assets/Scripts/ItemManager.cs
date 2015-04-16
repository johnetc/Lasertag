using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ItemManager
{

    #region Singleton

    private static readonly ItemManager instance = new ItemManager();

    public static ItemManager Instance
    {
        get { return instance; }
    }

    #endregion

    private GameObject _ItemContainer;

    private Dictionary<string, GameObject> _ItemPrefabDict;

    private Dictionary<string, Material> _MaterialDict;

    private Dictionary<int, ItemInfo> _ActiveItemDict;
    
    private int _CurrentItemNumber;

    private class ItemInfo
    {
        public GameObject Obj;
        public BaseItemTypes ItemClass;

        public ItemInfo(GameObject obj, BaseItemTypes itemClass)
        {
            Obj = obj;
            ItemClass = itemClass;
        }
    }

    public void Preload()
    {
        LoadAssets();
    }

    public void Initiate ()
    {
        Reset ();
    }

    public void Play ()
    {
        
    }

    public void Pause ()
    {
        
    }

    public void Unpaused ()
    {
        
    }

    public void GameOver()
    {
        
    }

    public void Reset()
    {
        _ActiveItemDict = new Dictionary<int, ItemInfo>();
        
    }

    public void LoadAssets()
    {
        _ItemPrefabDict = new Dictionary<string , GameObject> ();

        _MaterialDict = new Dictionary<string , Material> ();

        var resources = Resources.LoadAll ( "Items" );

        foreach ( var resource in resources )
        {
            if ( resource is GameObject )
            {
                _ItemPrefabDict.Add ( resource.name , ( GameObject ) resource );
            }
            if ( resource is Material )
            {
                _MaterialDict.Add ( resource.name , ( Material ) resource );
            }
        }

        _ItemContainer = new GameObject("Item_Container");
    }

    public void CreateItem(GameData.ItemTypes itemType, Vector3 pos)
    {
        BaseItemTypes tempItemClass = new LaserShotItem();
        
        switch (itemType)
        {
            case GameData.ItemTypes.LaserShotItem:
            {
                tempItemClass = new LaserShotItem();
            }
            break;
            case GameData.ItemTypes.BubbleShotItem:
            {
                tempItemClass = new BubbleShotItem ();
                
            }
            break;
        }

        GameObject tempObj = GameObject.Instantiate(_ItemPrefabDict[tempItemClass.ItemName], pos, Quaternion.identity) as GameObject;

        tempObj.GetComponent<Mono_Id>().Id = _CurrentItemNumber;
        tempObj.GetComponent<Mono_Id>().ThisObjType = GameData.ComponentType.Item;
        tempObj.transform.SetParent(_ItemContainer.transform);
        tempObj.transform.localScale = new Vector3(tempItemClass.ItemSize,tempItemClass.ItemSize,tempItemClass.ItemSize );

        ItemInfo tempItemInfo = new ItemInfo(tempObj, tempItemClass);
        _ActiveItemDict.Add ( _CurrentItemNumber , tempItemInfo );

        _CurrentItemNumber++;
    }

    public void CheckItemPickup(int id)
    {
        //Debug.Log(id);
        _ActiveItemDict[id].Obj.SetActive(false);
        _ActiveItemDict [ id ].ItemClass.ActivateItem();

    }
    
}
