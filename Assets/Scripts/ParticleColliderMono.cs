using UnityEngine;
using System.Collections;

public class ParticleColliderMono : MonoBehaviour
{

    public UI_GameScreen.TouchLocation thisLocation;
    private ParticleSystem.CollisionEvent[] collisionEvents = new ParticleSystem.CollisionEvent[16];

    public void OnParticleCollision(GameObject other)
    {
        ParticleSystem particleSystem;
        particleSystem = gameObject.GetComponent<ParticleSystem> ();
        int safeLength = particleSystem.safeCollisionEventSize;
        if ( collisionEvents.Length < safeLength )
            collisionEvents = new ParticleSystem.CollisionEvent [ safeLength ];

        int numCollisionEvents = particleSystem.GetCollisionEvents ( other , collisionEvents );
        int i = 0;
        while (i < numCollisionEvents)
        {
            if ( other.rigidbody )
            {
                Vector3 pos = collisionEvents[i].intersection;
                Vector3 force = collisionEvents[i].velocity*1;
                other.rigidbody.AddForce ( force );
            }
            i++;
            //Debug.Log(other.name);
        }
    }

}
