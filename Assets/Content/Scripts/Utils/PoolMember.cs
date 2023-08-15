using Content.Scripts.Interfaces;
using UnityEngine;

namespace Content.Scripts.Utils
{
    public class PoolMember : MonoBehaviour
    {
        private Pool<IView> pool;

        public Pool<IView> Pool
        {
            get => pool;
            set => pool = value;
        }
    }
}