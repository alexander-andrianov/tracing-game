using Content.Scripts.Data;

namespace Content.Scripts.Interfaces
{
    public interface ILevelsConfig
    {
        LevelData GetCurrentLevelData();
        GameData GetGameData();
    }
}