using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class EnemyManager {

    #region singleton

    private static readonly EnemyManager instance = new EnemyManager ();

    public static EnemyManager Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    public GameObject UnitContainer = new GameObject("Unit_container");

    public Dictionary<int , ObjectData> EnemyObjects = new Dictionary<int , ObjectData> ();

    public class ObjectData
    {
        public int Id;
        public GameObject ThisObject;
        public GameData.ObjectType ThisType;
        public Mono_Id ThisMonoId;
        public List<GameData.TouchLocation> ShieldLocations;
        public List<BoxCollider> ShieldCols;
        public bool IsDead;
        public Vector3 MovingTo;
        public float Speed;
        public BoxCollider BoxCol;
        public bool Rotate;
        public Vector3 Facing;
        public Stopwatch InvinceTimer = new Stopwatch();
    }

    public GameObject CubePrefab;
    public GameObject ShieldPrefab;

    public int CurrentId = 0;

    private Stopwatch m_EnemyGenerationTimer = new Stopwatch();

    public void Start ()
    {
        LoadAssets();
    }

    public void Update()
    {
        CheckObjectCreation();
        CheckObjectLocation();
        CheckObjectExistence();
        RotateObjects();
        MoveObjects();
        CheckInvincibility();

    }

    public void LoadAssets()
    {
        CubePrefab = Resources.Load<GameObject>("Prefabs/Cube");
        ShieldPrefab = Resources.Load<GameObject> ( "Prefabs/Shield" );
    }

    public void CheckObjectCreation()
    {
        if (!m_EnemyGenerationTimer.IsRunning)
        {
            m_EnemyGenerationTimer.Start();
        }
        //if (m_EnemyGenerationTimer.ElapsedMilliseconds > GameData.ObjectCreationIntervalMS)
        //{
        //    CreateNewObject( Vector3.zero);
        //    m_EnemyGenerationTimer.Reset();
        //}
        if ( m_EnemyGenerationTimer.ElapsedMilliseconds > GameData.ObjectCreationIntervalMS && EnemyObjects.Count < GameData.MaxEnemyObjects)
        {
            CreateNewObject ( Vector3.zero );
            m_EnemyGenerationTimer.Reset ();
        }
    }

    public void CheckObjectLocation()
    {
        foreach ( var enemyObject in EnemyObjects )
        {
            Vector3 tempPos = enemyObject.Value.ThisObject.transform.position;
            if (tempPos.x < GameData.TopLeftEnemyBorder.x || tempPos.x > GameData.BottomRightEnemyBorder.x)
            {
                enemyObject.Value.IsDead = true;
            }
            if ( tempPos.y < GameData.BottomRightEnemyBorder.y || tempPos.y > GameData.TopLeftEnemyBorder.y )
            {
                enemyObject.Value.IsDead = true;
            }
        }
    }

    public void CheckObjectExistence()
    {
        List<int> toRemove = new List<int>();
        
        foreach (var enemyObject in EnemyObjects)
        {
            if (enemyObject.Value.IsDead)
            {
                Object.Destroy(enemyObject.Value.ThisObject);
                toRemove.Add(enemyObject.Value.Id);
                
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            EnemyObjects.Remove ( toRemove[i] );
        }
    }

    public void CreateNewObject(Vector3 pos)
    {
        int randType = Random.Range(0, (int) GameData.ObjectType.count);
        CreateNewObject((GameData.ObjectType)randType, pos);
    }

	public void CreateNewObject(GameData.ObjectType type, Vector3 pos)
	{
		ObjectData tempData = new ObjectData();
        GameObject tempGameObject = GameObject.Instantiate(CubePrefab, pos, Quaternion.identity) as GameObject;
        tempData.Id = CurrentId;

        tempGameObject.transform.SetParent(UnitContainer.transform);
	    tempGameObject.name = type + " " + CurrentId;
	    tempData.ThisObject = tempGameObject;
	    tempData.ThisType = type;
	    tempData.MovingTo = ChooseMovementEndpoint();
	    tempData.Speed = GameData.StartSpeed;
	    tempData.ThisMonoId = tempGameObject.GetComponent<Mono_Id>();
        tempData.BoxCol = tempGameObject.GetComponent<BoxCollider> ();
        //tempData.BoxCol.isTrigger = false;
        ChooseStartPoint ( tempData.BoxCol );
        //tempData.BoxCol.isTrigger = true;
        //tempData.ShieldLocations = new List<GameData.TouchLocation>();
        //tempData.ShieldLocations = AddShields(tempData.Id,tempData.ThisType, tempGameObject);
        tempData.ShieldCols = new List<BoxCollider>();
        tempData.ShieldCols = AddShields ( tempData.Id , tempData.ThisType , tempGameObject );
	    tempData.ThisMonoId.Id = CurrentId;
	    
        EnemyObjects.Add ( CurrentId,tempData );
	    CurrentId++;
	}

    public void ChooseStartPoint (Collider col)
    {
        //Debug.Log("in");
        
        Vector3 pos = CreatePoint();
        col.gameObject.transform.position = pos;
        //Debug.Log ( pos );
        bool inObject = false;
        foreach (var enemyObject in EnemyObjects)
        {
            if (enemyObject.Value.BoxCol.bounds.Intersects(col.bounds))
            {
                inObject = true;
            }
        }

        if (inObject == true)
        {
            ChooseStartPoint ( col );
        }
        else
        {
            
        }
        
    }

    private Vector3 CreatePoint()
    {
        float x = Random.Range ( GameData.TopLeftSpawnArea.x , GameData.BottomRightSpawnArea.x + 1 );
        float y = Random.Range ( GameData.TopLeftSpawnArea.y , GameData.BottomRightSpawnArea.y + 1 );
        Vector3 pos = new Vector3 ( x , y , 90 );
        return pos;
    }

    public Vector3 ChooseMovementEndpoint ( )
    {
        int direction = Random.Range(0, 4);

        Vector3 pos = new Vector3 ();

        switch (direction)
        {
            case 0:
                pos = Vector3.up;
                break;
            case 1:
                pos = Vector3.right;
                break;
            case 2:
                pos = Vector3.down;
                break;
            case 3:
                pos = Vector3.left;
                break;

        }
        //pos = Vector3.zero;
        return pos;
    }

    public List<BoxCollider> AddShields(int id, GameData.ObjectType type, GameObject obj)
    {
        GameData.ShieldArrangement tempShieldArray = new GameData.ShieldArrangement();

        tempShieldArray = GameData.ShieldArray[(int) type];
        
        List<GameData.TouchLocation> shieldLocs = new List<GameData.TouchLocation>();

        for (int i = 0; i < tempShieldArray.Top; i++)
        {
            shieldLocs.Add(GameData.TouchLocation.Top);
        }
        for ( int i = 0; i < tempShieldArray.Bottom; i++ )
        {
            shieldLocs.Add(GameData.TouchLocation.Bottom);
        }
        for ( int i = 0; i < tempShieldArray.Left; i++ )
        {
            shieldLocs.Add(GameData.TouchLocation.Left);
        }
        for ( int i = 0; i < tempShieldArray.Right; i++ )
        {
            shieldLocs.Add(GameData.TouchLocation.Right);
        }
        for ( int i = 0; i < tempShieldArray.Random; i++ )
        {
            int rand = Random.Range(0, 4);
            shieldLocs.Add((GameData.TouchLocation)rand);
        }

        List<BoxCollider> colList = new List<BoxCollider>();

        foreach (var touchLocation in shieldLocs)
        {
            GameObject tempGameObject = GameObject.Instantiate ( ShieldPrefab , obj.transform.position , Quaternion.identity ) as GameObject;
            tempGameObject.GetComponent<Mono_Id>().Id = id;
            tempGameObject.GetComponent<Renderer>().material.color = Color.red;
            colList.Add(tempGameObject.GetComponent<BoxCollider> ());
            switch (touchLocation)
            {
                    case GameData.TouchLocation.Top:
                    tempGameObject.transform.localScale = new Vector3(tempGameObject.transform.lossyScale.x, 2, 10);
                    tempGameObject.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y+4f, 80);
                    tempGameObject.transform.SetParent(obj.transform);
                    break;
                    case GameData.TouchLocation.Right:
                    tempGameObject.transform.localScale = new Vector3 ( 2, tempGameObject.transform.lossyScale.y , 10 );
                    tempGameObject.transform.position = new Vector3 ( obj.transform.position.x + 4f , obj.transform.position.y , 80 );
                    tempGameObject.transform.SetParent(obj.transform);
                    break;
                    case GameData.TouchLocation.Bottom:
                    tempGameObject.transform.localScale = new Vector3(tempGameObject.transform.lossyScale.x, 2, 10);
                    tempGameObject.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y-4f, 80);
                    tempGameObject.transform.SetParent(obj.transform);
                    break;
                    case GameData.TouchLocation.Left:
                    tempGameObject.transform.localScale = new Vector3 ( 2 , tempGameObject.transform.lossyScale.y , 10 );
                    tempGameObject.transform.position = new Vector3(obj.transform.position.x-4f, obj.transform.position.y, 80);
                    tempGameObject.transform.SetParent(obj.transform);
                    break;
            }
        }

        return colList;
    }

    public void RotateObjects()
    {
        foreach ( var enemyObject in EnemyObjects )
        {
            if (enemyObject.Value.Rotate)
            {
                enemyObject.Value.ThisObject.transform.Rotate(0,0,GameData.CollisionRotSpeed);
                //int tempVal = Mathf.CeilToInt ( enemyObject.Value.ThisObject.transform.rotation.eulerAngles.z );
                float tempVal = enemyObject.Value.ThisObject.transform.rotation.eulerAngles.z ;
                ////Debug.Log(tempVal);
                //Debug.Log ( enemyObject.Value.ThisObject.name + " " + enemyObject.Value.ThisObject.transform.rotation.eulerAngles.z );
                if ( tempVal >  (enemyObject.Value.Facing.z+GameData.CollisionRotTotal) )
                {
                    //Debug.Log(enemyObject.Value.ThisObject.name);
                    enemyObject.Value.ThisObject.transform.eulerAngles = new Vector3 ( 0 , 0 , enemyObject.Value.Facing.z + GameData.CollisionRotTotal );
                    enemyObject.Value.Rotate = false;
                    enemyObject.Value.InvinceTimer.Start();

                }
            }
        }
    }

    public void MoveObjects()
    {
        foreach (var enemyObject in EnemyObjects)
        {
            if (!enemyObject.Value.Rotate)
            {
                enemyObject.Value.ThisObject.transform.Translate(enemyObject.Value.MovingTo*enemyObject.Value.Speed);
            }
            //else
            //{
            //    enemyObject.Value.BoxCol.enabled = true;
            //}
        }
    }

    public void CheckInvincibility()
    {
        foreach (var enemyObject in EnemyObjects)
        {
            if (enemyObject.Value.InvinceTimer.ElapsedMilliseconds > GameData.InvincibilityTimerMS)
            {
                enemyObject.Value.InvinceTimer.Reset();
                //enemyObject.Value.BoxCol.enabled = true;
                //foreach ( var shieldCol in enemyObject.Value.ShieldCols )
                //{
                //    shieldCol.enabled = true;
                //}
            }
        }
    }

    public void CheckColliders ( int objOne , GameData.ComponentType objOnetype , int objTwo , GameData.ComponentType objTwotype )
    {

        if (EnemyObjects[objOne].Rotate || EnemyObjects[objTwo].Rotate)
        {
            return;
        }
        if ( EnemyObjects [ objOne ].InvinceTimer.IsRunning || EnemyObjects [ objTwo ].InvinceTimer.IsRunning )
        {
            return;
        }

        //Debug.Log ( objOne + " " + objTwo );
        EnemyObjects[objOne].Rotate = true;
        EnemyObjects[objOne].Facing = EnemyObjects[objOne].ThisObject.transform.rotation.eulerAngles;
        EnemyObjects [ objTwo ].Rotate = true;
        EnemyObjects [ objTwo ].Facing = EnemyObjects [ objTwo ].ThisObject.transform.rotation.eulerAngles;

    }

    public void CheckHit(int id, GameData.ComponentType componentHit, GameData.TouchLocation location)
    {
        if (componentHit != GameData.ComponentType.MainBody)
        {
            return;
        }
        
        if ( !EnemyObjects [ id ].ShieldLocations.Contains ( location ) )
        {
            EnemyObjects [ id ].IsDead = true;
        }

    }
}
