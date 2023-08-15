using System.Collections.Generic;
using Content.Scripts.GameLogic;
using PathCreation;
using UnityEngine;

namespace Content.Scripts.Data
{
    public class LevelData : MonoBehaviour
    {
        [SerializeField] private PathCreator path;
        [SerializeField] private MaskView mask;
        [SerializeField] private List<ControlElementsData> controlElementsData;

        public PathCreator Path => path;
        public MaskView Mask => mask;
        public List<ControlElementsData> ControlElementsData => controlElementsData;
    }
}