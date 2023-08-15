using System.Collections.Generic;
using Content.Scripts.Interfaces;
using UnityEngine;

namespace Content.Scripts.Utils
{
    public class Pool<T>
    {
        private readonly Stack<T> inactive;
        private readonly List<T> active;
        private readonly T prefab;

        public Pool(T prefab, int initialQty)
        {
            this.prefab = prefab;
            inactive = new Stack<T>(initialQty);
            active = new List<T>();
        }

        public T Spawn(Transform parent, Vector3 position, Quaternion rotation)
        {
            T currentObject;
            GameObject currentGameObject;

            if (inactive.Count == 0)
            {
                var currentMonoBehaviour = Object.Instantiate(prefab as MonoBehaviour, position, rotation, parent);

                currentGameObject = currentMonoBehaviour.gameObject;
                currentObject = currentGameObject.GetComponent<T>();

                var poolMember = currentMonoBehaviour.gameObject.AddComponent<PoolMember>();
                poolMember.Pool = this as Pool<IView>;
            }
            else
            {
                currentObject = inactive.Pop();
                active.Add(currentObject);

                currentGameObject = (currentObject as MonoBehaviour)!.gameObject;

                if (currentObject == null)
                {
                    return Spawn(parent, position, rotation);
                }
            }

            currentGameObject!.transform.position = position;
            currentGameObject!.transform.rotation = rotation;
            currentGameObject!.SetActive(true);

            return currentObject;
        }

        public void Despawn(T currentObject)
        {
            var currentGameObject = currentObject as MonoBehaviour;

            if (currentGameObject == null)
            {
                return;
            }

            currentGameObject.gameObject.SetActive(false);
            inactive.Push(currentObject);
            active.Remove(currentObject);
        }

        public void DespawnAll()
        {
            foreach (var poolMember in active.ToArray())
            {
                Despawn(poolMember);
            }
        }
    }
}