using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
        public GameData.ShieldArrangement ShieldLocations;
        public List<BoxCollider> ShieldCols;
        public bool IsDead;
        public Vector3 MovingTo;
        public float Speed;
        public BoxCollider BoxCol;
        public bool Rotate;
        public Vector3 Facing;
        public Vector3 EndOfRotation;//
        public Stopwatch InvinceTimer = new Stopwatch();
        public Renderer MainRend;
        public List<Renderer> ShieldRendList; 
        public Color32 OrigColour;
    }

    public GameObject CubePrefab;
    public GameObject ShieldPrefab;

    public int CurrentId = 0;

    private Stopwatch m_EnemyGenerationTimer = new Stopwatch();

    public void Start ()
    {
        LoadAssets();
    }
    public void Initiate ()
    {
        Reset ();
    }
    public void Play()
    {
        CheckObjectCreation();
        //Debug.Log ( "Play loc" );
        CheckObjectLocation();
        CheckObjectExistence();
        RotateObjects();
        MoveObjects();
        CheckInvincibility();

    }

    public void Pause()
    {
        
    }

    public void Reset()
    {
        
    }

    public void GameOver()
    {
        //Debug.Log("GO");
        ResetEnemies ();
        //Debug.Log ( "GO chheck obj" );
        CheckObjectExistence ();
        //Debug.Log ( "GO reset var" );
        ResetVariables ();
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
        //Debug.Log ( EnemyObjects.Count );
        
        foreach ( var enemyObject in EnemyObjects.ToList() )
        {
            //Debug.Log(enemyObject.Value.Id);
            Vector3 tempPos = enemyObject.Value.ThisObject.transform.position;
            if (tempPos.x < GameData.TopLeftEnemyBorder.x || tempPos.x > GameData.BottomRightEnemyBorder.x)
            {
                SceneManager.Instance.ModifyLives ( -1 );
                enemyObject.Value.IsDead = true;
                ParticleManager.Instance.FireDeathExplosion ( enemyObject.Value.ThisObject.transform.position , GameData.DeathParticleBorder );
            }
            if ( tempPos.y < GameData.BottomRightEnemyBorder.y || tempPos.y > GameData.TopLeftEnemyBorder.y )
            {
                SceneManager.Instance.ModifyLives ( -1 );
                enemyObject.Value.IsDead = true;
                ParticleManager.Instance.FireDeathExplosion ( enemyObject.Value.ThisObject.transform.position , GameData.DeathParticleBorder );
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
	    tempData.MainRend = tempData.ThisObject.GetComponent<Renderer>();
	    tempData.OrigColour = tempData.MainRend.material.color;
        ChooseStartPoint ( tempData.BoxCol );
        tempData.ShieldLocations = new GameData.ShieldArrangement(GameData.ShieldArray[(int)tempData.ThisType]);
        ConvertBlueprintToShields ( tempData );
	    tempData.ThisMonoId.Id = CurrentId;
	    tempData.ThisObject.transform.localScale = Vector3.one * GameData.ObjectScale;
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
        Vector3 pos = new Vector3 ( x , y , GameData.CameraDepth );
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

    public void ConvertBlueprintToShields ( ObjectData tempData )
    {
        //GameData.ShieldArrangement tempShieldArray = new GameData.ShieldArrangement();

        GameData.ObjectType type = tempData.ThisType;
        int id = tempData.Id;
        GameObject obj = tempData.ThisObject;
        
        List<BoxCollider> tempColList = new List<BoxCollider>();

        tempData.ShieldRendList = new List<Renderer>();
        
        for (int i = 0; i < tempData.ShieldLocations.Locations.Count; i++)
        {
            tempColList.Add ( AddShield ( tempData.ShieldLocations.Locations [ i ] , tempData ) );
        }

        for ( int j = 0; j < tempData.ShieldLocations.Random; j++ )
        {
            int rand = Random.Range(0, 4);
            tempColList.Add (AddShield ( ( GameData.TouchLocation ) rand, tempData  ));
            tempData.ShieldLocations.Locations.Add ( ( GameData.TouchLocation ) rand );
        }

        tempData.ShieldCols = new List<BoxCollider>(tempColList);
    }

    private BoxCollider AddShield ( GameData.TouchLocation touchLocation , ObjectData tempData )
    {
        GameObject obj = tempData.ThisObject;
        int id = tempData.Id;

        GameObject tempGameObject = GameObject.Instantiate ( ShieldPrefab , obj.transform.position , Quaternion.identity ) as GameObject;
        tempGameObject.GetComponent<Mono_Id> ().Id = id;
        tempGameObject.GetComponent<Renderer> ().material.color = Color.red;
        tempData.ShieldRendList.Add ( tempGameObject.GetComponent<Renderer> ());
        BoxCollider col = tempGameObject.GetComponent<BoxCollider> ();

        switch ( touchLocation )
        {
            case GameData.TouchLocation.Top:
                tempGameObject.transform.localScale = new Vector3 ( tempGameObject.transform.lossyScale.x , 2 , 10 );
                tempGameObject.transform.position = new Vector3 ( obj.transform.position.x , obj.transform.position.y + 4f , GameData.ShieldDepth );
                break;
            case GameData.TouchLocation.Right:
                tempGameObject.transform.localScale = new Vector3 ( 2 , tempGameObject.transform.lossyScale.y , 10 );
                tempGameObject.transform.position = new Vector3 ( obj.transform.position.x + 4f , obj.transform.position.y , GameData.ShieldDepth );
                break;
            case GameData.TouchLocation.Bottom:
                tempGameObject.transform.localScale = new Vector3 ( tempGameObject.transform.lossyScale.x , 2 , 10 );
                tempGameObject.transform.position = new Vector3 ( obj.transform.position.x , obj.transform.position.y - 4f , GameData.ShieldDepth );
                break;
            case GameData.TouchLocation.Left:
                tempGameObject.transform.localScale = new Vector3 ( 2 , tempGameObject.transform.lossyScale.y , 10 );
                tempGameObject.transform.position = new Vector3 ( obj.transform.position.x - 4f , obj.transform.position.y , GameData.ShieldDepth );
                break;
        }

        tempGameObject.transform.SetParent ( obj.transform );
        return col;
    }

    public void RotateObjects()
    {
        foreach ( var enemyObject in EnemyObjects )
        {
            if (enemyObject.Value.Rotate)
            {
                enemyObject.Value.ThisObject.transform.Rotate(0,0,GameData.CollisionRotSpeed);
                
                float tempVal = enemyObject.Value.ThisObject.transform.rotation.eulerAngles.z ;

                if ( tempVal >  (enemyObject.Value.Facing.z+GameData.CollisionRotTotal) || tempVal < 1)
                {
                    enemyObject.Value.ThisObject.transform.eulerAngles = new Vector3 ( 0 , 0 , Mathf.FloorToInt ( enemyObject.Value.Facing.z + GameData.CollisionRotTotal ) );
                    enemyObject.Value.Facing = enemyObject.Value.ThisObject.transform.eulerAngles;
                    enemyObject.Value.Rotate = false;
                    enemyObject.Value.InvinceTimer.Start ();
                    RealignShields(enemyObject.Value);
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
            if (enemyObject.Value.InvinceTimer.IsRunning)
            {
                enemyObject.Value.MainRend.material.color = new Color ( enemyObject.Value.OrigColour.r,
                    enemyObject.Value.OrigColour.g,
                    enemyObject.Value.OrigColour.b,
                    Mathf.PingPong(Time.time * GameData.InvincibilityFlashSpeedMult, 0.5f));

                foreach (var shieldRend in enemyObject.Value.ShieldRendList)
                {
                   shieldRend.material.color = new Color ( shieldRend.material.color.r ,
                   shieldRend.material.color.g ,
                   shieldRend.material.color.b ,
                   Mathf.PingPong ( Time.time * GameData.InvincibilityFlashSpeedMult , 0.5f ) );
                }
            }
            
            if (enemyObject.Value.InvinceTimer.ElapsedMilliseconds > GameData.InvincibilityTimerMS)
            {
                enemyObject.Value.InvinceTimer.Reset();
                
                enemyObject.Value.MainRend.material.color = enemyObject.Value.OrigColour;
                foreach ( var shieldRend in enemyObject.Value.ShieldRendList )
                {
                    shieldRend.material.color = Color.red;
                }
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

    public void RealignShields(ObjectData tempObj)
    {
        GameData.ShieldArrangement tempArrange = new GameData.ShieldArrangement();

        int[] tempShieldCount = new [] {0, 0, 0, 0};

        tempObj.ThisMonoId.shieldlocs = new[] {0, 0, 0, 0};

        for (int i = 0; i < tempObj.ShieldLocations.Locations.Count; i++)
        {
            switch ( tempObj.ShieldLocations.Locations [ i ] )
            {
                    case GameData.TouchLocation.Top:
                    tempArrange.Locations.Add(GameData.TouchLocation.Left);
                    tempShieldCount[3] += 1;
                    break;
                    case GameData.TouchLocation.Right:
                    tempArrange.Locations.Add ( GameData.TouchLocation.Top );
                    tempShieldCount [ 0 ] += 1;
                    break;
                    case GameData.TouchLocation.Bottom:
                    tempArrange.Locations.Add ( GameData.TouchLocation.Right );
                    tempShieldCount [ 1 ] += 1;
                    break;
                    case GameData.TouchLocation.Left:
                    tempArrange.Locations.Add ( GameData.TouchLocation.Bottom );
                    tempShieldCount [ 2 ] += 1;
                    break;
            }
        }
        tempObj.ShieldLocations = tempArrange;
        tempObj.ThisMonoId.shieldlocs = tempShieldCount;
    }

    public void CheckHit(int id, GameData.ComponentType componentHit, GameData.TouchLocation location)
    {
        if (componentHit != GameData.ComponentType.MainBody)
        {
            return;
        }

        else
        {
            if (!EnemyObjects[id].ShieldLocations.Locations.Contains(location))

            {
                SceneManager.Instance.ModifyScore(1);
                EnemyObjects[id].IsDead = true;
                ParticleManager.Instance.FireDeathExplosion ( EnemyObjects [ id ].ThisObject.transform.position , GameData.DeathParticleShot );
            }
        }

    }

    public void ResetEnemies()
    {
        List<int> toRemove = new List<int> ();

        foreach ( var enemyObject in EnemyObjects )
        {
            enemyObject.Value.IsDead = true;
        }

        //Debug.Log("ResetDone");
    }

    public void ResetVariables()
    {
        EnemyObjects = new Dictionary<int , ObjectData> ();
    }
}
