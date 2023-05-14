using System;
using UnityEngine;

namespace Util
{
    [Serializable]
    public class Optional<T>
    {
        [SerializeField] private bool hasValue;
        [SerializeField] private T value;
        
        public bool TryGetValue(out T outValue)
        {
            outValue = value;
            return hasValue;
        }
    }
    
    [Serializable]
    public class OptionalInt : Optional<int> { }
}