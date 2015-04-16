using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BackgroundGenerator 
{

    #region singleton

    private static readonly BackgroundGenerator instance = new BackgroundGenerator ();

    public static BackgroundGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    private GameObject _BGPillar;
    private float _HeightOfPillar;
    public GameObject BGContainer;
    private Dictionary< Vector2, GameObject> _PillarDict; 
    public void Preload ()
    {
        LoadAssets ();
    }
    
    public void LoadAssets ()
    {
        _BGPillar = Resources.Load<GameObject> ( "Prefabs/BGPillar" );
    }

    public void Initiate()
    {
        //PlacePillars();
    }

    public void PlacePillars()
    {
        BGContainer = new GameObject("BG_Container");

        float height = GameData.TopLeftPoint.y - GameData.BottomRightPoint.y;
        float width = GameData.BottomRightPoint.x - GameData.TopLeftPoint.x;
        _HeightOfPillar = height / GameData.BackgroundPillarYNumber;
        float numberOfPillarsXFloat = width/_HeightOfPillar;
        int numberOfPillarsX = Mathf.CeilToInt(numberOfPillarsXFloat);

        _PillarDict = new Dictionary< Vector2, GameObject>();

        int k = 0;

        for (int i = 0; i < GameData.BackgroundPillarYNumber; i++)
        {
            for (int j = 0; j < numberOfPillarsX; j++)
            {
                if (i >= j)
                {
                    k = i;
                }
                else
                {
                    k = j;
                }
                Vector3 tempPos = new Vector3 ( GameData.TopLeftPoint.x + (_HeightOfPillar*j), 
                    GameData.BottomRightPoint.y + (_HeightOfPillar*i), 
                    //k*(_HeightOfPillar*0.5f) );
                    0 );
                GameObject tempGameObject = GameObject.Instantiate ( _BGPillar , tempPos , Quaternion.identity ) as GameObject;
                tempGameObject.transform.localScale = new Vector3(_HeightOfPillar, _HeightOfPillar, _HeightOfPillar*2);
                tempGameObject.transform.SetParent(BGContainer.transform);
                _PillarDict.Add ( new Vector2 ( j , i ) , tempGameObject );
            }
            
        }

        //BGContainer.transform.eulerAngles = new Vector3 ( 45 , 0 , 0 );

        BGContainer.transform.position = new Vector3 ( _HeightOfPillar * 0.5f , _HeightOfPillar * 0.5f , GameData.CameraDepth + GameData.BackgroundDepthModifier );


    }

    public void Play ()
    {
        //MovePillars();

    }

    public void Pause ()
    {

    }

    public void Reset ()
    {
        
    }

    public void GameOver ()
    {
        

    }

    public void MovePillars()
    {
        foreach (var obj in _PillarDict)
        {
            obj.Value.transform.localPosition = new Vector3 ( obj.Value.transform.localPosition.x , 
                obj.Value.transform.localPosition.y , 
                Mathf.PingPong ( ( Time.time * 10 ) + ( (obj.Key.x + obj.Key.y)/2 ) * 10 , ( _HeightOfPillar * 2 ) )  //current z pos
                + GameData.CameraDepth + GameData.BackgroundDepthModifier ); // base z pos modifier
        }
        //BGContainer.transform.Rotate(new Vector3(1,0));
    }
}
