using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class ParticleManager

{
    public GameObject ParticleContainer;
    
    private List<ParticleSystem.Particle> ActiveParticles = new List<ParticleSystem.Particle> ();

    private class ParticleBurst
    {
        public int LeftToFire;
        public int TotalToFire;
        public int DeadParticles;
        public ParticleSystem ThisSystem;
        public ParticleSystem.Particle [] TheseParticles;
        public Stopwatch StopW = new Stopwatch ();
        public bool Started;
        public bool ReadyForDisposal;
    }

    private List<ParticleBurst> m_ActiveParticleBursts = new List<ParticleBurst> ();
    private List<ParticleBurst> m_SpentParticleBursts = new List<ParticleBurst> ();

    private GameObject m_ParticlePrefab;

    public ParticleManager()
    {
        LoadAssets();
    }

    public void Update()
    {
        FireScheduledParticles ();
        CheckParticleSystemDisposal ();
        CheckForDeadParticles ();
    }

    public void LoadAssets()
    {
        m_ParticlePrefab = Resources.Load<GameObject> ( "Particles/ParticleSystem" );
        ParticleContainer = new GameObject("Particle_Container");
        ParticleContainer.transform.position = SceneManager.Instance.MainCamera.transform.position;
    }

    public void NewShotSystem(GameScreen.PanelData panel)
    {
        GameObject newParticle = GameObject.Instantiate ( m_ParticlePrefab );

        newParticle.transform.SetParent ( ParticleContainer.transform );

        newParticle.transform.position = SceneManager.Instance.MainCamera.ScreenToWorldPoint ( new Vector3 ( Input.mousePosition.x , Input.mousePosition.y , 100 ) );

        ParticleSystem tempSys = newParticle.GetComponent<ParticleSystem> ();

        ParticleBurst tempBurst = new ParticleBurst ();

        tempBurst.ThisSystem = tempSys;

        ParticleSystem.Particle [] tempPart = new ParticleSystem.Particle [ GameData.NumberOfParticlesPerShot ];

        tempBurst.LeftToFire = tempPart.Count () - 1;
        tempBurst.TotalToFire = tempBurst.LeftToFire;

        switch ( panel.PanelLoc )
        {
            case GameData.TouchLocation.Left:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.right * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Left;
                    break;
                }
            case GameData.TouchLocation.Right:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.left * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Right;
                    break;
                }
            case GameData.TouchLocation.Top:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.down * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Top;
                    break;
                }
            case GameData.TouchLocation.Bottom:
                {
                    tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.up * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );

                    newParticle.GetComponent<ParticleColliderMono> ().thisLocation = GameData.TouchLocation.Bottom;
                    break;
                }

        }
        tempSys.Emit ( tempPart [ 0 ] );
        tempBurst.TheseParticles = tempPart;
        m_ActiveParticleBursts.Add ( tempBurst );
    }

    public void ParticleSystemDebug ( ParticleSystem partSys )
    {
        ParticleSystem.Particle [] particles = new ParticleSystem.Particle [ partSys.maxParticles ];
        var partNumber = partSys.GetParticles ( particles );

        Debug.Log ( particles [ 0 ].velocity );
        Debug.Log ( particles [ 0 ].lifetime );
        Debug.Log ( particles [ 0 ].startLifetime );

    }

    public ParticleSystem.Particle [] ParticleDefiner ( Vector3 pos , Vector3 vel , float size , float life , Color32 colour )
    {
        ParticleSystem.Particle [] particles = new ParticleSystem.Particle [ GameData.NumberOfParticlesPerShot ];

        for ( int i = 0; i < particles.Count (); i++ )
        {
            particles [ i ].position = pos;
            particles [ i ].velocity = vel;
            particles [ i ].size = size;
            particles [ i ].lifetime = life;
            particles [ i ].startLifetime = life;
            particles [ i ].color = colour;
        }

        return particles;
    }

    public void FireScheduledParticles ()
    {
        foreach ( var particle in m_ActiveParticleBursts )
        {
            if ( !particle.Started )
            {
                particle.StopW.Start ();
            }
        }

        foreach ( var mParticleBurst in m_ActiveParticleBursts )
        {
            //Debug.Log(mParticleBurst.StopW.ElapsedMilliseconds);
            if ( mParticleBurst.StopW.ElapsedMilliseconds > GameData.ParticleShotIntervalMS && mParticleBurst.LeftToFire > 0 )
            {
                mParticleBurst.StopW.Reset ();
                mParticleBurst.StopW.Start ();
                mParticleBurst.ThisSystem.Emit ( mParticleBurst.TheseParticles [ mParticleBurst.TotalToFire - mParticleBurst.LeftToFire + 1 ] );
                mParticleBurst.LeftToFire--;
            }
            if ( mParticleBurst.LeftToFire < 1 )
            {
                mParticleBurst.StopW.Stop ();
                //mParticleBurst.ReadyForDisposal = true;
            }
        }

    }

    public void CheckParticleSystemDisposal ()
    {
        for ( int i = m_ActiveParticleBursts.Count - 1; i > -1; i-- )
        {
            if ( m_ActiveParticleBursts [ i ].DeadParticles > m_ActiveParticleBursts [ i ].TotalToFire )
            {
                //m_SpentParticleBursts.Add ( m_ActiveParticleBursts [ i ] );
                m_ActiveParticleBursts [ i ].ThisSystem.gameObject.SetActive ( false );
                Object.Destroy(m_ActiveParticleBursts[i].ThisSystem.gameObject);
                m_ActiveParticleBursts.Remove ( m_ActiveParticleBursts [ i ] );
            }
        }
    }

    public void CheckForDeadParticles ()
    {
        foreach ( var mActiveParticleBurst in m_ActiveParticleBursts )
        {
            ParticleSystem.Particle [] part = new ParticleSystem.Particle [ mActiveParticleBurst.ThisSystem.maxParticles ];
            var partNo = mActiveParticleBurst.ThisSystem.GetParticles ( part );
            for ( int i = 0; i < partNo; i++ )
            {
                if ( CheckParticleStats ( part [ i ] ) )
                {
                    part [ i ].lifetime = 0;
                    mActiveParticleBurst.DeadParticles++;
                }
            }
            mActiveParticleBurst.ThisSystem.SetParticles ( part , partNo );
        }
    }

    public bool CheckParticleStats ( ParticleSystem.Particle particle )
    {
        if ( particle.velocity.magnitude < GameData.ShotParticleVelocityMult )
        {
            return true;
        }
        if ( particle.position.x > GameData.BottomRightPoint.x || particle.position.x < GameData.TopLeftPoint.x )
        {
            return true;
        }
        if ( particle.position.y > GameData.TopLeftPoint.y || particle.position.y < GameData.BottomRightPoint.y )
        {
            return true;
        }

        return false;
    }
}
