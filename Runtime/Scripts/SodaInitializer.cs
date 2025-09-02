using System;
using UnityEngine;

namespace Soda.Runtime
{
    public class SodaInitializer : MonoBehaviour
    {
        [SerializeField] private InitializationType initializationType;
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        private void Awake()
        {
            if (initializationType == InitializationType.Awake && !SodaSDK.IsInitialized)
            {
                Initialize();
            }
        }

        private void Start()
        {
            if (initializationType == InitializationType.Start && !SodaSDK.IsInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            SodaSDK.Initialize();
                
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    public enum InitializationType
    {
        Awake = 0,
        Start = 1,
    }
}