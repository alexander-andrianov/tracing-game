using Content.Scripts.Interfaces;
using UnityEngine;

namespace Content.Scripts.GameLogic
{
    public class MaskView : MonoBehaviour, IView
    {
        public Transform Transform => transform;
    }
}
