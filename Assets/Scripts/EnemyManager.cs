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
        public bool IsDead;
        public Vector3 MovingTo;
        public float Speed;
        public Collider Col;
        public bool Rotate;
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
        MoveObjects();
        
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
        if (m_EnemyGenerationTimer.ElapsedMilliseconds > GameData.ObjectCreationIntervalMS)
        {
            CreateNewObject( Vector3.zero);
            m_EnemyGenerationTimer.Reset();
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
        tempData.Col = tempGameObject.GetComponent<Collider> ();
        //tempData.Col.isTrigger = false;
        ChooseStartPoint ( tempData.Col );
        //tempData.Col.isTrigger = true;
        tempData.ShieldLocations = new List<GameData.TouchLocation>();
        tempData.ShieldLocations = AddShields(tempData.Id,tempData.ThisType, tempGameObject);
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
            if (enemyObject.Value.Col.bounds.Intersects(col.bounds))
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

    public List<GameData.TouchLocation> AddShields(int id, GameData.ObjectType type, GameObject obj)
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

        foreach (var touchLocation in shieldLocs)
        {
            GameObject tempGameObject = GameObject.Instantiate ( ShieldPrefab , obj.transform.position , Quaternion.identity ) as GameObject;
            tempGameObject.GetComponent<Mono_Id>().Id = id;
            tempGameObject.GetComponent<Renderer>().material.color = Color.red;
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

        return shieldLocs;
    }

    public void MoveObjects()
    {
        foreach (var enemyObject in EnemyObjects)
        {
            enemyObject.Value.ThisObject.transform.Translate ( enemyObject.Value.MovingTo * enemyObject.Value.Speed);
        }
    }

    public void CheckColliders(int objOne, GameObject objTwo)
    {
        Debug.Log ( objOne.name + " " + objTwo.name );

        int idOne = -1;
        int idTwo = -1;

        if ( objOne.GetComponent<Mono_Id> () != null )
        {
            idOne = objOne.GetComponent<Mono_Id> ().Id;
            objOne.transform.Rotate ( new Vector3 ( 0 , 0 , 45 ) );
            //if ( EnemyObjects [ idOne ].IsDead )
            //{
            //    return;
            //}
        }
        if ( objTwo.GetComponent<Mono_Id> () != null )
        {
            idTwo = objTwo.GetComponent<Mono_Id> ().Id;
            objTwo.transform.Rotate ( new Vector3 ( 0 , 0 , 45 ) );
            //if ( EnemyObjects [ idTwo ].IsDead )
            //{
            //    return;
            //}
        }

        //if ( idOne != -1 )
        //{
        //    EnemyObjects [ idOne ].IsDead = true;
        //    //Debug.Log ( objOne.name + " One: Is dead " );
        //}
        //if ( idTwo != -1 )
        //{
        //    EnemyObjects [ idOne ].IsDead = true;
        //    //Debug.Log ( objTwo.name + " Two: Is dead " );
        //}
        CheckObjectExistence();
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
