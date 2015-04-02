using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{

    void Awake()
    {
        SceneManager.Instance.MainCamera = gameObject.GetComponent<Camera>();
        SceneManager.Instance.Awake();
    }
	void Start () 
    {
        SceneManager.Instance.Start();
	}
	
	// Play is called once per frame
	void Update () 
    {
        SceneManager.Instance.Update();
	}

    void OnDrawGizmos ()
    {
        SceneManager.Instance.OnDrawGizmos ();
    }
}
