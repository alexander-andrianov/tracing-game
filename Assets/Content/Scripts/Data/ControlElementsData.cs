using System;
using UnityEngine;

namespace Content.Scripts.Data
{
    [Serializable]
    public class ControlElementsData
    {
        [SerializeField] private Transform transform;
        [SerializeField] private Collider2D elementCollider;
        [SerializeField] private int direction;

        private int currentIndex = 0;
        
        private bool isInitialized = false;

        public Transform Transform => transform;
        public Collider2D Collider => elementCollider;
        public int Direction => direction;

        public int CurrentIndex
        {
            get => currentIndex;
            set => currentIndex = value;
        }
        
        public bool IsInitialized
        {
            get => isInitialized;
            set => isInitialized = value;
        }
    }
}
