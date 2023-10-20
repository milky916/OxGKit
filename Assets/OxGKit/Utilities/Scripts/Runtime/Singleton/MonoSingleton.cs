﻿using UnityEngine;

namespace OxGKit.Utilities.Singleton
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public bool dontDestoryOnLoad = false;

        public static bool isCreated { get; private set; }
        public static bool isReleased { get; private set; }

        private static readonly object _locker = new object();
        private static T _instance;

        /// <summary>
        /// If need a DontDestroyOnLoad MonoSingleton set true once.
        /// </summary>
        /// <param name="dontDestroyOnLoad"></param>
        /// <returns></returns>
        public static T GetInstance(bool dontDestroyOnLoad = false)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (Application.isPlaying)
                    {
                        _instance = _FindExistingInstance();
                        if (_instance == null) _instance = _CreateNewInstance();
                        if (dontDestroyOnLoad) DontDestroyOnLoad(_instance);
                    }
                }
            }
            return _instance;
        }

        private static T _FindExistingInstance()
        {
            T[] existingInstances = FindObjectsOfType<T>();

            // No instance found
            if (existingInstances == null || existingInstances.Length == 0) return null;

            return existingInstances[0];
        }

        private static T _CreateNewInstance()
        {
            return new GameObject(typeof(T).Name).AddComponent<T>();
        }

        /// <summary>
        /// Call by Awake
        /// <para>
        /// Note: Except Awake() and OnDestroy(), other Unity methods can be override.
        /// </para>
        /// </summary>
        protected abstract void OnCreate();

        /// <summary>
        /// Call by OnDestroy
        /// <para>
        /// Note: Except Awake() and OnDestroy(), other Unity methods can be override.
        /// </para>
        /// </summary>
        protected abstract void OnRelease();

        /// <summary>
        /// Destroy singleton instance
        /// </summary>
        /// <param name="gameObjectIncluded"></param>
        public static void DestroyInstance(bool gameObjectIncluded = false)
        {
            GameObject go = null;
            if (gameObjectIncluded) go = _instance?.gameObject;
            Component.Destroy(_instance);
            _instance = null;
            GameObject.Destroy(go);
        }

        #region Unity Methods
        /// <summary>
        /// Don't override (Except Awake() and OnDestroy(), other Unity methods can be override.)
        /// </summary>
        private void Awake()
        {
            // Reset destroy flag
            isReleased = false;

            // If MonoSingleton already in the scene
            if (_instance == null)
            {
                _instance = _FindExistingInstance();
                if (this.dontDestoryOnLoad) DontDestroyOnLoad(_instance);
            }

            if (!isCreated)
            {
                isCreated = true;
                this.OnCreate();
            }
        }

        /// <summary>
        /// Don't override (Except Awake() and OnDestroy(), other Unity methods can be override.)
        /// </summary>
        private void OnDestroy()
        {
            isCreated = false;
            isReleased = true;
            this.OnRelease();
        }
        #endregion
    }
}