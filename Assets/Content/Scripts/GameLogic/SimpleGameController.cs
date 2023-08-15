using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.Scripts.Data;
using Content.Scripts.Interfaces;
using Content.Scripts.Utils;
using DG.Tweening;
using Lean.Touch;
using PathCreation;
using UniRx;
using UnityEngine;

namespace Content.Scripts.GameLogic
{
    public class SimpleGameController : MonoBehaviour, IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private Camera gameCamera;
        private Transform levelsRoot;
        private Transform masksRoot;
        private List<ControlElementsData> currentElementsData;
        private ControlElementsData currentElementData;
        private LevelData levelData;
        private GameData gameData;
        private MaskView maskPrefab;
        private VertexPath currentPath;
        private PoolService<IView> poolService;
        private Pool<IView> masksPool;

        private ILevelsConfig levelsConfig;
        private IView lastMaskView;

        private bool touchStarted;

        public void Initialize(ILevelsConfig config, Camera localCamera, Transform levelRoot, Transform maskRoot)
        {
            levelsConfig = config;
            gameData = levelsConfig.GetGameData();
            levelData = Instantiate(levelsConfig.GetCurrentLevelData(), levelRoot);

            currentPath = levelData.Path.path;
            currentElementsData = levelData.ControlElementsData;
            maskPrefab = levelData.Mask;
            gameCamera = localCamera;
            levelsRoot = levelRoot;
            masksRoot = maskRoot;

            PreloadMasks();
            BuildElementsData();
            lastMaskView = masksPool.Spawn(masksRoot, currentPath.GetPoint(0), Quaternion.identity);

            SubscribeOnGameEvents();
        }

        private void SubscribeOnGameEvents()
        {
            Observable.Timer(TimeSpan.FromSeconds(gameData.RefreshInterval)).Repeat().Subscribe(_ =>
            {
                var distanceBetweenControls = GetDistanceBetweenControls();

                if (distanceBetweenControls >= gameData.MaxControlDistance)
                {
                    disposables?.Clear();
                    Observable.Timer(TimeSpan.FromSeconds(gameData.RefreshInterval)).Repeat()
                        .Subscribe(_ => TryRestartLevel()).AddTo(disposables);
                }
            }).AddTo(disposables);

            LeanTouch.OnFingerDown += HandleFingerDown;
            LeanTouch.OnFingerUpdate += HandleFingerUpdate;
            LeanTouch.OnFingerUp += HandleFingerUp;
        }

        private void UnsubscribeFromGameEvents()
        {
            LeanTouch.OnFingerDown -= HandleFingerDown;
            LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
            LeanTouch.OnFingerUp -= HandleFingerUp;
        }

        private void HandleFingerDown(LeanFinger finger)
        {
            if (touchStarted)
            {
                return;
            }

            var fingerPosition = gameCamera.ScreenToWorldPoint(finger.ScreenPosition);
            var interactiveCollider = Physics2D.OverlapPoint(fingerPosition, LayerMask.GetMask("Default"));

            if (interactiveCollider != null)
            {
                SetCurrentElementData(interactiveCollider);
            }

            touchStarted = true;
        }

        private void HandleFingerUpdate(LeanFinger finger)
        {
            if (!touchStarted || currentElementData is null)
            {
                return;
            }

            var fingerPosition = gameCamera.ScreenToWorldPoint(finger.ScreenPosition);
            var interactiveCollider = Physics2D.OverlapPoint(fingerPosition);

            if (interactiveCollider is null)
            {
                return;
            }

            var currentTransform = currentElementData.Transform;
            var nearestPoint = currentPath.GetClosestPointAndIndexOnPath(fingerPosition);
            var nearestPosition = new Vector3(nearestPoint.Item1.x, nearestPoint.Item1.y, 0f);
            var isRightDirection = IsRightDirection(currentElementData.Direction, currentElementData.CurrentIndex,
                nearestPoint.Item2);

            if (!isRightDirection && currentElementData.IsInitialized)
            {
                return;
            }

            if (!IsNextElement(nearestPoint.Item2))
            {
                return;
            }

            currentTransform.DOMove(nearestPosition, gameData.ElementMovementDuration);
            currentElementData.CurrentIndex = nearestPoint.Item2;

            if (Vector3.Distance(lastMaskView.Transform.position, currentTransform.position) >= gameData.MasksInterval)
            {
                lastMaskView = masksPool.Spawn(masksRoot, currentTransform.position, Quaternion.identity);
            }

            currentElementData.IsInitialized = true;
        }

        private void HandleFingerUp(LeanFinger finger)
        {
            currentElementData = null;
            touchStarted = false;
        }

        private bool IsRightDirection(int direction, int currentIndex, int nextIndex)
        {
            return direction switch
            {
                > 0 => currentIndex < nextIndex,
                < 0 => currentIndex > nextIndex,
                _ => false
            };
        }

        private bool IsNextElement(int nextIndex)
        {
            var indexesDifference = Mathf.Abs(currentElementData.CurrentIndex - nextIndex);
            var isInversedDirection = currentElementData.Direction < 0;

            if (indexesDifference >= gameData.HugeDifferenceValue)
            {
                return false;
            }

            switch (isInversedDirection)
            {
                case true when currentElementData.CurrentIndex < nextIndex:
                case false when currentElementData.CurrentIndex > nextIndex:
                    return false;
                default:
                    return true;
            }
        }

        private void SetCurrentElementData(Collider2D localCollider)
        {
            currentElementData = localCollider == currentElementsData[0].Collider
                ? currentElementsData[0]
                : currentElementsData[1];
        }

        private void BuildElementsData()
        {
            var firstPointIndex = gameData.FirstElementSpawnIndex;
            var lastPointIndex = currentPath.NumPoints - gameData.SecondElementSpawnIndex;

            currentElementsData[0].CurrentIndex = firstPointIndex;
            currentElementsData[0].IsInitialized = false;

            currentElementsData[1].CurrentIndex = lastPointIndex;
            currentElementsData[1].IsInitialized = false;

            currentElementsData[0].Transform.position = currentPath.GetPoint(firstPointIndex);
            currentElementsData[1].Transform.position = currentPath.GetPoint(lastPointIndex);
        }

        private void PreloadMasks()
        {
            poolService = new PoolService<IView>();
            masksPool = poolService.Preload(maskPrefab, masksRoot, gameData.MasksPreloadValue);
        }

        private float GetDistanceBetweenControls()
        {
            var firstControlPosition = currentElementsData[0].Transform.position;
            var secondControlPosition = currentElementsData[1].Transform.position;

            return Vector3.Distance(firstControlPosition, secondControlPosition);
        }

        private Vector3 GetPointBetweenControls()
        {
            var firstControlPosition = currentElementsData[0].Transform.position;
            var secondControlPosition = currentElementsData[1].Transform.position;

            return (firstControlPosition + secondControlPosition) / 2f;
        }

        private void SpawnEndMasks()
        {
            masksPool.Spawn(masksRoot, currentElementsData[0].Transform.position, Quaternion.identity);
            masksPool.Spawn(masksRoot, currentElementsData[1].Transform.position, Quaternion.identity);
        }

        private async Task PlayFinishAnimation()
        {
            var sequence = DOTween.Sequence();
            var pointBetweenControls = GetPointBetweenControls();
            var cachedScale = currentElementsData[0].Transform.localScale.x;

            sequence.Append(currentElementsData[0].Transform
                .DOMove(pointBetweenControls, gameData.FinishMovementDuration));
            sequence.Join(
                currentElementsData[1].Transform.DOMove(pointBetweenControls, gameData.FinishMovementDuration));
            sequence.Append(currentElementsData[0].Transform.DOScale(gameData.FinishScaleUpValue, gameData.FinishScaleDuration));
            sequence.Join(currentElementsData[1].Transform.DOScale(gameData.FinishScaleUpValue, gameData.FinishScaleDuration));
            sequence.Append(currentElementsData[0].Transform.DOScale(cachedScale, gameData.FinishScaleDuration / 2f));
            sequence.Join(currentElementsData[1].Transform.DOScale(cachedScale, gameData.FinishScaleDuration / 2f));

            await sequence.Play().AsyncWaitForCompletion();
        }

        private async void TryRestartLevel()
        {
            var distanceBetweenControls = GetDistanceBetweenControls();

            if (distanceBetweenControls <= gameData.MinControlDistance)
            {
                currentElementData = null;
                touchStarted = false;

                disposables?.Clear();
                UnsubscribeFromGameEvents();
                SpawnEndMasks();

                await PlayFinishAnimation();

                masksPool.DespawnAll();

                Destroy(levelData.gameObject);
                Initialize(levelsConfig, gameCamera, levelsRoot, masksRoot);
            }
        }

        public void Dispose()
        {
            disposables?.Dispose();
        }
    }
}