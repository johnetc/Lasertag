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
        public HitDetector ThisHitDetector;
        public List<GameData.TouchLocation> ShieldLocations;
        public bool IsDead;
        public Vector3 MovingTo;
        public float Speed;
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
            CreateNewObject( ChooseStartPoint());
            m_EnemyGenerationTimer.Reset();
        }
    }

    public void CheckObjectLocation()
    {
        foreach (var enemyObject in EnemyObjects)
        {
            Vector3 tempPos = enemyObject.Value.ThisObject.transform.position;
            if (tempPos.x > )
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
        tempGameObject.transform.SetParent(UnitContainer.transform);
	    tempGameObject.name = type + " " + EnemyObjects.Count;
	    tempData.ThisObject = tempGameObject;
	    tempData.ThisType = type;
	    tempData.MovingTo = ChooseMovementEndpoint();
	    tempData.Speed = GameData.StartSpeed;
	    tempData.ThisHitDetector = tempGameObject.GetComponent<HitDetector>();
        tempData.ShieldLocations = new List<GameData.TouchLocation>();
        tempData.ShieldLocations = AddShields(tempData.ThisType, tempGameObject);
	    tempData.ThisHitDetector.Id = CurrentId;
	    tempData.Id = CurrentId;
        EnemyObjects.Add ( CurrentId,tempData );
	    CurrentId++;
	}

    public Vector3 ChooseStartPoint ()
    {
        float x = Random.Range(GameData.TopLeftSpawnArea.x, GameData.BottomRightSpawnArea.x + 1);
        float y = Random.Range ( GameData.TopLeftSpawnArea.y , GameData.BottomRightSpawnArea.y + 1 );
        Vector3 pos = new Vector3(x,y,90);
        return pos;
    }

    public Vector3 ChooseMovementEndpoint ( )
    {
        int direction = Random.Range(0, 4);

        Vector3 pos = new Vector3 ();

        switch (direction)
        {
            case 0:
                pos = new Vector3(0, 1, 0);
                break;
            case 1:
                pos = new Vector3 ( 1 , 0 , 0);
                break;
            case 2:
                pos = new Vector3 ( 0 , -1 , 0 );
                break;
            case 3:
                pos = new Vector3 ( -1 , 0 , 0 );
                break;

        }
        return pos;
    }

    public List<GameData.TouchLocation> AddShields(GameData.ObjectType type, GameObject obj)
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
            
            switch (touchLocation)
            {
                    case GameData.TouchLocation.Top:
                    tempGameObject.transform.localScale = new Vector3(tempGameObject.transform.lossyScale.x, 2, 10);
                    tempGameObject.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y+7f, 90);
                    tempGameObject.transform.SetParent(obj.transform);
                    break;
                    case GameData.TouchLocation.Right:
                    tempGameObject.transform.localScale = new Vector3 ( 2, tempGameObject.transform.lossyScale.y , 10 );
                    tempGameObject.transform.position = new Vector3 ( obj.transform.position.x + 7f , obj.transform.position.y , 90 );
                    tempGameObject.transform.SetParent(obj.transform);
                    break;
                    case GameData.TouchLocation.Bottom:
                    tempGameObject.transform.localScale = new Vector3(tempGameObject.transform.lossyScale.x, 2, 10);
                    tempGameObject.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y-7f, 90);
                    tempGameObject.transform.SetParent(obj.transform);
                    break;
                    case GameData.TouchLocation.Left:
                    tempGameObject.transform.localScale = new Vector3 ( 2 , tempGameObject.transform.lossyScale.y , 10 );
                    tempGameObject.transform.position = new Vector3(obj.transform.position.x-7f, obj.transform.position.y, 90);
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

    public void CheckHit(int id, GameData.TouchLocation location)
    {
        if (!EnemyObjects[id].ShieldLocations.Contains(location))
        {
            EnemyObjects[id].IsDead = true;
        }
    }
}
