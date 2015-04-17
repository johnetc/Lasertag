using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

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
        public float VisibleTime;
        public Stopwatch StopWTime;
        public Renderer MainRend;
        public Color32 OrigColour;

        public ItemInfo(GameObject obj, BaseItemTypes itemClass, float visibleTime, Renderer mainRend, Color32 col)
        {
            Obj = obj;
            ItemClass = itemClass;
            VisibleTime = visibleTime;
            StopWTime = new Stopwatch();
            MainRend = mainRend;
            OrigColour = col;
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
        CheckItemLife();
        CheckItemFlash();
        RemoveDeadItems();
    }

    public void Pause ()
    {
        foreach (var itemInfo in _ActiveItemDict)
        {
            if (itemInfo.Value.StopWTime.IsRunning)
            {
                Debug.Log("Stop");
                itemInfo.Value.StopWTime.Stop();
            }
        }
    }

    public void Unpaused ()
    {
        foreach ( var itemInfo in _ActiveItemDict )
        {
            if ( itemInfo.Value.StopWTime.ElapsedMilliseconds>0 )
            {
                Debug.Log ( "Start" );
                itemInfo.Value.StopWTime.Start();
            }
        }
    }

    public void GameOver()
    {
        foreach (var itemInfo in _ActiveItemDict)
        {
            itemInfo.Value.Obj.SetActive(false);
        }
        RemoveDeadItems();
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
            case GameData.ItemTypes.SpreadShotItem:
            {
                tempItemClass = new SpreadShotItem();

            }
            break;
        }

        GameObject tempObj = GameObject.Instantiate(_ItemPrefabDict[tempItemClass.ItemName], pos, Quaternion.identity) as GameObject;

        tempObj.GetComponent<Mono_Id>().Id = _CurrentItemNumber;
        tempObj.GetComponent<Mono_Id>().ThisObjType = GameData.ComponentType.Item;
        tempObj.transform.SetParent(_ItemContainer.transform);
        tempObj.transform.localScale = new Vector3(tempItemClass.ItemSize,tempItemClass.ItemSize,tempItemClass.ItemSize );

        ItemInfo tempItemInfo = new ItemInfo ( tempObj , tempItemClass , tempItemClass.VisibleTimeMS , tempObj.GetComponent<Renderer> () , tempObj.GetComponent<Renderer> ().material.color );
        tempItemInfo.StopWTime.Start();

        _ActiveItemDict.Add ( _CurrentItemNumber , tempItemInfo );

        _CurrentItemNumber++;
    }

    private void CheckItemLife()
    {
        foreach (var itemInfo in _ActiveItemDict)
        {
            if (itemInfo.Value.StopWTime.ElapsedMilliseconds > itemInfo.Value.VisibleTime)
            {
                itemInfo.Value.Obj.SetActive ( false );
            }
        }
    }

    private void CheckItemFlash()
    {
        foreach ( var itemInfo in _ActiveItemDict )
        {
            if ( itemInfo.Value.StopWTime.IsRunning )
            {
                itemInfo.Value.MainRend.material.color = new Color ( itemInfo.Value.OrigColour.r ,
                    itemInfo.Value.OrigColour.g ,
                    itemInfo.Value.OrigColour.b ,
                    Mathf.PingPong ( Time.time * GameData.InvincibilityFlashSpeedMult , 0.5f ) );
               
            }

        }
    }

    private void RemoveDeadItems()
    {
        //Debug.Log ( "start" + _ActiveItemDict.Count );
        
        Dictionary<int, ItemInfo> tempdict = new Dictionary<int, ItemInfo>();

        foreach (var itemInfo in _ActiveItemDict)
        {
            if (itemInfo.Value.Obj.gameObject.activeInHierarchy)
            {
                tempdict.Add(itemInfo.Key, itemInfo.Value);
            }
            else
            {
                Object.Destroy ( itemInfo.Value.Obj );
            }
        }

        _ActiveItemDict = new Dictionary<int, ItemInfo>();

        _ActiveItemDict = tempdict;
        
        //Debug.Log("end"+_ActiveItemDict.Count);
    }

    public void CheckItemPickup(int id)
    {
        //Debug.Log(id);
        _ActiveItemDict[id].Obj.SetActive(false);
        _ActiveItemDict [ id ].ItemClass.ActivateItem();

    }
    
}
