using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class TestBehavior : MonoBehaviour
{
    private static float updateInterval = 10.0f;
    private Vessel lastVessel;
    private float lastUpdate;
    private float lastLateUpdate;
    private float lastFixedUpdate;
    private float lastOnGui;

    public class TestTest : KSP.Testing.UnitTest
    {
        public TestTest()
        {
            Debug.Log("TAC Test [][" + Time.time + "]: Test creation");
            GameObject ghost = new GameObject("TacTest", typeof(TestBehavior));
            GameObject.DontDestroyOnLoad(ghost);
        }
    }

    void Awake()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Awake");
        lastVessel = null;
        lastUpdate = 0;
        lastLateUpdate = 0;
        lastFixedUpdate = 0;
        lastOnGui = 0;
    }

    void OnEnable()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnEnable");
    }

    void Start()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Start");
    }





    void Update()
    {
        if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != lastVessel)
        {
            lastVessel = FlightGlobals.ActiveVessel;
            Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Update: " + lastVessel);
        }
        else if ((Time.time - lastUpdate) > updateInterval)
        {
            lastUpdate = Time.time;
            Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: Update");
        }
    }

    void LateUpdate()
    {
        if ((Time.time - lastLateUpdate) > updateInterval)
        {
            lastLateUpdate = Time.time;
            Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: LateUpdate");
        }
    }

    void FixedUpdate()
    {
        if ((Time.time - lastFixedUpdate) > updateInterval)
        {
            lastFixedUpdate = Time.time;
            Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: FixedUpdate");
        }
    }

    void OnGUI()
    {
        if ((Time.time - lastOnGui) > updateInterval)
        {
            lastOnGui = Time.time;
            Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnGUI");
        }
    }






    void OnLevelWasLoaded(int level)
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnLevelWasLoaded " + level
            + " flight=" + HighLogic.LoadedSceneIsFlight + " editor=" + HighLogic.LoadedSceneIsEditor
            + " scene=" + HighLogic.LoadedScene.ToString());
    }

    void OnApplicationPause()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnApplicationPause");
    }

    void OnApplicationFocus()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnApplicationFocus");
    }






    void OnApplicationQuit()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnApplicationQuit");
    }

    void OnDisable()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDisable");
    }

    void OnDestroy()
    {
        Debug.Log("TAC Test [" + this.GetInstanceID().ToString("X") + "][" + Time.time + "]: OnDestroy");
    }
}
