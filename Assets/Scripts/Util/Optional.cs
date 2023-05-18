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

        public Optional()
        {
            hasValue = false;
        }

        public Optional(T value)
        {
            this.value = value;
            hasValue = true;
        }
    }
    
    [Serializable]
    public class OptionalInt : Optional<int> { }
}