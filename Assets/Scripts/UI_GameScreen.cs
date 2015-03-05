using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class UI_GameScreen  {

    #region singleton

    private static readonly UI_GameScreen instance = new UI_GameScreen ();

    public static UI_GameScreen Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    // rename singleton instance variable
    public MenuMasterControl.MasterMenuDelegate InitiateUIDel;

    private GameObject m_GamePageContainer;

    private GameObject m_GamePagePrefab;
    private GameObject m_ParticlePrefab;
    //private Dictionary<string , PanelData> m_TouchPanelDict = new Dictionary<string , PanelData> ();
    private Dictionary<TouchLocation, GameObject> m_ParticleDict = new Dictionary<TouchLocation, GameObject>(); 
    private List<ParticleSystem.Particle> ActiveParticles = new List<ParticleSystem.Particle>();
    private Vector3 TopLeft;
    private Vector3 BottomRight;
    private class PanelData
    {
        public Button PanelButton;
        public TouchLocation PanelLoc;
    }

    private class ParticleBurst
    {
        public int LeftToFire;
        public int TotalToFire;
        public int DeadParticles;
        public ParticleSystem ThisSystem;
        public ParticleSystem.Particle[] TheseParticles;
        public Stopwatch StopW = new Stopwatch();
        public bool Started;
        public bool ReadyForDisposal;
    }

    private List<ParticleBurst> m_ActiveParticleBursts = new List<ParticleBurst>();
    private List<ParticleBurst> m_SpentParticleBursts = new List<ParticleBurst> (); 

    public enum TouchLocation
    {
        Top,
        Left,
        Bottom,
        Right,
    }

    public void Update()
    {
        FireScheduledParticles();
        CheckParticleSystemDisposal();
        CheckForDeadParticles();
    }

    public void LoadUIAssets ()
    {
        m_GamePageContainer = new GameObject ( "GameScreen" );
        m_GamePageContainer.transform.SetParent ( MenuMasterControl.Instance.MenuContainer.transform );
        m_GamePagePrefab = ( GameObject ) GameObject.Instantiate ( Resources.Load<GameObject> ( "Prefabs/MainGameScreen" ) );
        m_GamePagePrefab.transform.SetParent ( m_GamePageContainer.transform );
        m_GamePagePrefab.GetComponent<Canvas>().worldCamera = SceneManager.Instance.MainCamera;

        InitiateUIDel = InitiateOrEmptyUI;

        MenuMasterControl.Instance.RegisterPage ( InitiateUIDel , MenuMasterControl.MenuPages.Game );

        MenuInfoSender tempScript = m_GamePagePrefab.GetComponent<MenuInfoSender>();
        tempScript.GetPrefabComponents();
        LoadPrefabVariables ( tempScript );

    }

    private void LoadPrefabVariables ( MenuInfoSender tempScript )
    {
        TopLeft = SceneManager.Instance.MainCamera.ViewportToWorldPoint ( new Vector3 ( 0 , 1 , SceneManager.Instance.MainCamera.nearClipPlane ) ) ;
        BottomRight = SceneManager.Instance.MainCamera.ViewportToWorldPoint ( new Vector3 ( 1 , 0 , SceneManager.Instance.MainCamera.nearClipPlane ) );
        
        foreach (var button in tempScript.ButtonList)
        {
            PanelData tempPanel = new PanelData();
           
            tempPanel.PanelButton = button;
            switch ( button.name )
            {
                case "LeftPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Left;
                        break;
                    }
                case "RightPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Right;
                        break;
                    }
                case "TopPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Top;
                        break;
                    }
                case "BottomPanel":
                    {
                        tempPanel.PanelLoc = TouchLocation.Bottom;
                        break;
                    }
            }

            AddListenerToButton ( tempPanel );
        }
        m_ParticlePrefab = Resources.Load<GameObject>("Particles/ParticleSystem");
    }
    private void AddListenerToButton ( PanelData panel )
    {
        panel.PanelButton.onClick.RemoveAllListeners ();
        //Debug.Log(panel.PanelButton.name);
        panel.PanelButton.onClick.AddListener ( () => PanelTouched ( panel ));
    }

    private void PanelTouched ( PanelData panel )
    {
        
        GameObject newParticle = GameObject.Instantiate(m_ParticlePrefab);

        newParticle.transform.SetParent ( SceneManager.Instance.MainCamera.transform );

        newParticle.transform.position = SceneManager.Instance.MainCamera.ScreenToWorldPoint ( new Vector3 ( Input.mousePosition.x , Input.mousePosition.y , 100 ) );

        ParticleSystem tempSys = newParticle.GetComponent<ParticleSystem> ();

        ParticleBurst tempBurst = new ParticleBurst ();

        tempBurst.ThisSystem = tempSys;

        ParticleSystem.Particle[] tempPart = new ParticleSystem.Particle[GameData.NumberOfParticlesPerShot];

        tempBurst.LeftToFire = tempPart.Count()-1;
        tempBurst.TotalToFire = tempBurst.LeftToFire;

        switch (panel.PanelLoc)
        {
            case TouchLocation.Left:
            {
                tempPart = ParticleDefiner(tempSys.transform.position, Vector3.right*GameData.ShotParticleVelocityMult, GameData.ShotParticleSize, GameData.ShotParticleLifetime, GameData.ShotParticleColour);
               
                newParticle.GetComponent<ParticleColliderMono> ().thisLocation = TouchLocation.Left;
                break;
            }
            case TouchLocation.Right:
            {
                tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.left * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );
               
                newParticle.GetComponent<ParticleColliderMono> ().thisLocation = TouchLocation.Right;
                break;
            }
            case TouchLocation.Top:
            {
                tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.down * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );
                
                newParticle.GetComponent<ParticleColliderMono> ().thisLocation = TouchLocation.Top;
                break;
            }
            case TouchLocation.Bottom:
            {
                tempPart = ParticleDefiner ( tempSys.transform.position , Vector3.up * GameData.ShotParticleVelocityMult , GameData.ShotParticleSize , GameData.ShotParticleLifetime , GameData.ShotParticleColour );
                
                newParticle.GetComponent<ParticleColliderMono> ().thisLocation = TouchLocation.Bottom;
                break;
            }

        }
        tempSys.Emit(tempPart[0]);
        tempBurst.TheseParticles = tempPart;
        m_ActiveParticleBursts.Add(tempBurst);
    }

    private void ParticleSystemHijack(ParticleSystem partSys)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[partSys.maxParticles];
        var partNumber = partSys.GetParticles(particles);

        Debug.Log(particles[0].velocity);
        Debug.Log ( particles [ 0 ].lifetime );
        Debug.Log ( particles [ 0 ].startLifetime );
        
    }

    private ParticleSystem.Particle[] ParticleDefiner( Vector3 pos, Vector3 vel, float size, float life, Color32 colour)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[GameData.NumberOfParticlesPerShot];

        for (int i = 0; i < particles.Count(); i++)
        {
            particles[i].position = pos;
            particles[i].velocity = vel;
            particles [ i ].size = size;
            particles [ i ].lifetime = life;
            particles [ i ].startLifetime = life;
            particles [ i ].color = colour;
        }
        
        return particles;
    }

    private void FireScheduledParticles()
    {
        foreach ( var particle in m_ActiveParticleBursts )
        {
            if (!particle.Started)
            {
                particle.StopW.Start();
            }
        }

        foreach (var mParticleBurst in m_ActiveParticleBursts)
        {
            //Debug.Log(mParticleBurst.StopW.ElapsedMilliseconds);
            if (mParticleBurst.StopW.ElapsedMilliseconds > GameData.ParticleShotIntervalMS && mParticleBurst.LeftToFire > 0)
            {
                mParticleBurst.StopW.Reset();
                mParticleBurst.StopW.Start();
                mParticleBurst.ThisSystem.Emit(mParticleBurst.TheseParticles[mParticleBurst.TotalToFire - mParticleBurst.LeftToFire+1]);
                mParticleBurst.LeftToFire --;
            }
            if (mParticleBurst.LeftToFire == 0)
            {
                mParticleBurst.StopW.Stop();
                //mParticleBurst.ReadyForDisposal = true;
            }
        }

    }

    private void CheckParticleSystemDisposal()
    {
        for ( int i = m_ActiveParticleBursts.Count-1; i > -1; i-- )
        {
            if ( m_ActiveParticleBursts [ i ].DeadParticles > m_ActiveParticleBursts[i].TotalToFire )
            {
                //m_SpentParticleBursts.Add ( m_ActiveParticleBursts [ i ] );
                m_ActiveParticleBursts[i].ThisSystem.gameObject.SetActive(false);
                m_ActiveParticleBursts.Remove(m_ActiveParticleBursts[i]);
            }
        }
    }

    private void CheckForDeadParticles()
    {
        foreach (var mActiveParticleBurst in m_ActiveParticleBursts)
        {
            ParticleSystem.Particle [] part = new ParticleSystem.Particle [ mActiveParticleBurst.ThisSystem.maxParticles];
            var partNo = mActiveParticleBurst.ThisSystem.GetParticles(part);
            for ( int i = 0; i < partNo; i++ )
            {
                
                if (CheckParticleStats(part[i]))
                {
                    part[i].lifetime = 0;
                    mActiveParticleBurst.DeadParticles++;
                }
            }
            mActiveParticleBurst.ThisSystem.SetParticles ( part , partNo );
        }
        
    }

    private bool CheckParticleStats(ParticleSystem.Particle particle)
    {
        if ( particle.velocity == Vector3.zero )
        {
            return true;
        }
        if ( particle.position.x > BottomRight.x || particle.position.x < TopLeft.x )
        {
            return true;
        }
        if ( particle.position.y > TopLeft.y || particle.position.y < BottomRight.y )
        {
            return true;
        }

        return false;
    }

    public void ColliderResult()
    {
        
    }

    public void InitiateOrEmptyUI ( bool on )
    {
        if ( on ) { InitiateUI (); }
        else { EmptyUI (); }
    }

    public void InitiateUI ()
    {
        m_GamePageContainer.SetActive ( true );
    }

    public void EmptyUI ()
    {
        m_GamePageContainer.SetActive ( false );
    }
}
