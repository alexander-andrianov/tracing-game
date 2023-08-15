using System;
using UnityEngine;

namespace Content.Scripts.Data
{
    [Serializable]
    public class GameData
    {
        [SerializeField] private float refreshInterval = 0.1f;
        [SerializeField] private float masksInterval = 0.2f;
        [SerializeField] private float minControlDistance = 0.6f;
        [SerializeField] private float maxControlDistance = 0.75f;
        [SerializeField] private float elementMovementDuration = 0.1f;
        [SerializeField] private float finishMovementDuration = 0.5f;
        [SerializeField] private float finishScaleDuration = 0.4f;
        [SerializeField] private float finishScaleUpValue = 1.4f;
        [SerializeField] private int hugeDifferenceValue = 250;
        [SerializeField] private int masksPreloadValue = 50;
        [SerializeField] private int firstElementSpawnIndex = 15;
        [SerializeField] private int secondElementSpawnIndex = 5;

        public float RefreshInterval => refreshInterval;
        public float MasksInterval => masksInterval;
        public float MinControlDistance => minControlDistance;
        public float MaxControlDistance => maxControlDistance;
        public float ElementMovementDuration => elementMovementDuration;
        public float FinishMovementDuration => finishMovementDuration;
        public float FinishScaleDuration => finishScaleDuration;
        public float FinishScaleUpValue => finishScaleUpValue;
        public int HugeDifferenceValue => hugeDifferenceValue;
        public int MasksPreloadValue => masksPreloadValue;
        public int FirstElementSpawnIndex => firstElementSpawnIndex;
        public int SecondElementSpawnIndex => secondElementSpawnIndex;
    }
}
