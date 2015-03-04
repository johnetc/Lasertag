using UnityEngine;
using System.Collections;

public class ParticleColliderMono : MonoBehaviour
{

    public UI_GameScreen.TouchLocation thisLocation;
    private ParticleCollisionEvent [] collisionEvents = new ParticleCollisionEvent [ 16 ];
    
    public void OnParticleCollision(GameObject other)
    {
        ParticleSystem particleSystem;
        particleSystem = gameObject.GetComponent<ParticleSystem> ();
        int safeLength = particleSystem.GetSafeCollisionEventSize();
        if ( collisionEvents.Length < safeLength )
            collisionEvents = new ParticleCollisionEvent [ safeLength ];

        int numCollisionEvents = particleSystem.GetCollisionEvents ( other , collisionEvents );
        int i = 0;
        while ( i < numCollisionEvents )
        {
            if ( other.GetComponent<Rigidbody>() )
            {
                Vector3 pos = collisionEvents [ i ].intersection;
                Vector3 force = collisionEvents [ i ].velocity * 1;
                other.GetComponent<Rigidbody> ().AddForceAtPosition( force, pos );
                Destroy(this.gameObject);
            }
            i++;
        }
        
    }

}
