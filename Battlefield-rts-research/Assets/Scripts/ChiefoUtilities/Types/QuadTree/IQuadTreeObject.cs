using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChiefoUtilities
{
    namespace Types
    {
        public interface IQuadTreeObject<T>
        {
            public Vector3 Position { get; set; }
        }
    }
}