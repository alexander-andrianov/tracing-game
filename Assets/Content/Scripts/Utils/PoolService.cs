using System;
using System.Collections.Generic;
using Content.Scripts.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Content.Scripts.Utils
{
    public class PoolService<T>
    {
        private const int DefaultPoolSize = 10;
        
        private readonly Dictionary<T, Pool<T>> pools;

        public PoolService()
        { 
           pools = new Dictionary<T, Pool<T>>();
        }

        private Pool<T> GetPool(T prefab, int poolSize = DefaultPoolSize)
        {
            try
            {
                if (prefab != null && pools!.ContainsKey(prefab) == false)
                {
                    pools[prefab] = new Pool<T>(prefab, poolSize);
                }
            
                return pools![prefab ?? throw new ArgumentNullException(nameof(prefab))];
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentNullException();
            }
        }

        public Pool<T> Preload(T prefab, Transform parent, int poolSize = DefaultPoolSize)
        {
            var objectPool = GetPool(prefab, poolSize);
            var objectArray = new T[poolSize];
            
            for (var i = 0; i < poolSize; i++)
            {
                objectArray[i] = Spawn(prefab, parent, Vector3.zero, Quaternion.identity);
            }

            for (var i = 0; i < poolSize; i++)
            {
                Despawn(objectArray[i]);
            }

            return objectPool;
        }

        private T Spawn(T prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            GetPool(prefab);

            return pools[prefab].Spawn(parent, position, rotation);
        }

        private void Despawn(T currentObject)
        {
            var currentTemp = currentObject as MonoBehaviour;
            
            if (currentTemp == null)
            {
                return;
            }
            
            var poolMember = currentTemp.gameObject.GetComponent<PoolMember>();
            
            if (poolMember == null)
            {
                Object.Destroy(currentTemp);
            }
            else
            {
                poolMember.Pool.Despawn((IView)currentObject);
            }
        }
        
        public void DespawnAll()
        {
            if (pools.Count == 0) return;

            foreach (var pair in pools)
            {
                pair.Value.DespawnAll();
            }
        }
    }
}