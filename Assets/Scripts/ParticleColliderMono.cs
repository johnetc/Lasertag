using System.Xml.Serialization;
using UnityEngine;
using System.Collections;

public class ParticleColliderMono : MonoBehaviour
{

    public GameData.TouchLocation thisLocation;
    private ParticleCollisionEvent [] collisionEvents = new ParticleCollisionEvent [ 16 ];

    public void Update()
    {
        
    }

    public void OnParticleCollision(GameObject other)
    {
        ParticleSystem particleSystem;
        particleSystem = gameObject.GetComponent<ParticleSystem> ();
        int safeLength = particleSystem.GetSafeCollisionEventSize();
        if ( collisionEvents.Length < safeLength )
            collisionEvents = new ParticleCollisionEvent [ safeLength ];

        int numCollisionEvents = particleSystem.GetCollisionEvents ( other , collisionEvents );
        int i = 0;
        foreach (var particleCollisionEvent in collisionEvents)
        {
            
        //}
        //( i < numCollisionEvents )
        //{
            if ( other.GetComponent<Rigidbody>() )
            {
                //Debug.Log ( "Hit from the... " + thisLocation );
                //Vector3 pos = collisionEvents [ i ].intersection;
                ////Debug.Log ( other.transform.position + " " + collisionEvents [ i ].intersection );
                //Vector3 force = collisionEvents [ i ].velocity * 1;
                ////other.GetComponent<Rigidbody> ().AddForceAtPosition( force, pos );
                EnemyManager.Instance.CheckHit ( other.GetComponent<Mono_Id> ().Id , other.GetComponent<Mono_Id> ().ThisObjType , thisLocation );
                
                ////Debug.Log(collisionEvents[i].collider.name);
                ////Destroy(this.gameObject);
            }
            //i++;
        }
        
    }

}
