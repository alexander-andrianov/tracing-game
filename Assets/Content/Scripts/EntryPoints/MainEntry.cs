using Content.Scripts.Configs;
using Content.Scripts.GameLogic;
using UnityEngine;

namespace Content.Scripts.EntryPoints
{
    public class MainEntry : MonoBehaviour
    {
        [SerializeField] private SimpleGameController gameController;
        [SerializeField] private LevelsConfig levelConfigs;
        [SerializeField] private Camera localCamera;
        [SerializeField] private Transform levelsRoot;
        [SerializeField] private Transform masksRoot;
        
        private void Start()
        {
            gameController.Initialize(levelConfigs, localCamera, levelsRoot, masksRoot);
        }
    }
}
