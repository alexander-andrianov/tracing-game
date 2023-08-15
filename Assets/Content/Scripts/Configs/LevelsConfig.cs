using System.Collections.Generic;
using Content.Scripts.Data;
using Content.Scripts.Interfaces;
using UnityEngine;

namespace Content.Scripts.Configs
{
    [CreateAssetMenu(menuName = "Levels/Level config", fileName = "Level")]
    public class LevelsConfig : ScriptableObject, ILevelsConfig
    {
        [SerializeField] private List<LevelData> levels;
        [SerializeField] private GameData gameData;

        public LevelData GetCurrentLevelData()
        {
            return levels[0];
        }

        public GameData GetGameData()
        {
            return gameData;
        }
    }
}