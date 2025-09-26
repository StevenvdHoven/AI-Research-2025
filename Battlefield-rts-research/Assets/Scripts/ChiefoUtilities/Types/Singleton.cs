using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChiefoUtilities
{
    namespace Types
    {
        public class Singleton<T> : MonoBehaviour
        {
            public static T Instance;

            protected void LoadSingelton(T _object)
            {
                if (Instance == null)
                {
                    Instance = _object;
                }
            }
        }
    }
}