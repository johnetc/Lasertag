using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class EnemyManager {

    public List<ObjectData> EnemyObjects = new List<ObjectData> ();

    public class ObjectData
    {
        public GameObject ThisObject;
        public ObjectType ThisType;
        public HitDetector ThisHitDetector;
    }

    public GameObject CubePrefab;

    public enum ObjectType
    {
        TwoShieldStandard,
    }

    private Stopwatch m_EnemyGenerationTimer = new Stopwatch();

    public EnemyManager ()
    {
        LoadAssets();
    }

    public void LoadAssets()
    {
        CubePrefab = Resources.Load<GameObject>("Prefabs/Cube");
    }

    public void CheckObjectCreation()
    {
        if (!m_EnemyGenerationTimer.IsRunning)
        {
            m_EnemyGenerationTimer.Start();
        }
        if (m_EnemyGenerationTimer.ElapsedMilliseconds > GameData.ObjectCreationIntervalMS)
        {
            CreateNewObject(ObjectType.TwoShieldStandard, new Vector3(474, 360, 100));
            m_EnemyGenerationTimer.Reset();
        }
    }

    public void CheckObjectExistence()
    {
        for ( int i = EnemyObjects.Count-1; i > -1; i-- )
        {
            if (EnemyObjects[i].ThisHitDetector.IsHit)
            {
                Object.Destroy(EnemyObjects[i].ThisObject);
                EnemyObjects.RemoveAt(i);
            }
        }
    }

	public void CreateNewObject(ObjectType type, Vector3 pos)
	{
		ObjectData tempData = new ObjectData();
        EnemyObjects.Add ( tempData );

        GameObject tempGameObject = GameObject.Instantiate(CubePrefab, pos, Quaternion.identity) as GameObject;
	    tempGameObject.name = type + " " + EnemyObjects.Count;
	    tempData.ThisObject = tempGameObject;
	    tempData.ThisType = type;
	    tempData.ThisHitDetector = tempGameObject.GetComponent<HitDetector>();
        tempData.ThisHitDetector.ShieldLocations.Add(GameScreen.TouchLocation.Top);
	}

    public void ObjectMovement()
    {
        for (int i = 0; i < EnemyObjects.Count; i++)
        {
            Vector3 tempPos = EnemyObjects[i].ThisObject.transform.position;
            tempPos.x = tempPos.x + Random.Range(-1, 2);
            tempPos.y = tempPos.y + Random.Range ( -1 , 2 );
            EnemyObjects[i].ThisObject.transform.position = tempPos;
        }
    }
}
