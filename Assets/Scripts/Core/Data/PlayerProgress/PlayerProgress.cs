using System;
using System.Collections.Generic;
using System.Linq;
using Core.Player;
using Core.Player.InitialData;
using Core.Player.PlayerSO;
using Core.Skins;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Data.PlayerProgress
{
    [Serializable]
    public class PlayerProgress
    {
        public event Action Changed;
    
        [SerializeField] [Min(0)]
        private long _money;
        [SerializeField] 
        private List<LevelProgress> _levelsProgress;
        [SerializeField]
        private List<WheelProgress> _wheelsProgress;
        [SerializeField]
        private List<WheelWrapperProgress> _wheelWrappersProgress;
        [SerializeField]
        private CharacterProgress _characterProgress;
        [SerializeField]
        private List<PurchasableSkinProgress> _skinsProgress;
        [SerializeField]
        private List<PurchasableSkinWrapperProgress> _skinWrappersProgress;

        private WheelsLibrary _wheelsLibrary;
        private SkinsLibrary _skinsLibrary;

        [SerializeField][HideInInspector]
        private int _notConsideredCharacterStatPoints;

        public long Money => _money;

        public CharacterModifiersPoints CharacterModifiersPoints => _characterProgress.ModifierPoints;
        public IEnumerable<PurchasableSkinWrapperProgress> SkinWrappersProgress => _skinWrappersProgress;

        public PlayerProgress(InitialWheelsData initialWheelsData, InitialCharacterStatsData initialCharacterStatsData, List<PurchasableSkin> purchasableSkins, int moneyStartValue)
        {
            SetupMoney(moneyStartValue);
            SetupWheelProgress(initialWheelsData);
            //SetupViewProgress(initialData);
            SetupCharacterInitialProgress(initialCharacterStatsData);
            SetupPurchasableSkinsProgress(purchasableSkins);
            SetupLevelProgress();
        }

        public void SetupWheelWrappersProgressFromLibrary(WheelsLibrary library)
        {
            _wheelWrappersProgress = new List<WheelWrapperProgress>();
            _wheelsLibrary = library;

            _wheelsProgress.ForEach(progress =>
            {
                var prefab = library.GetWheelByID(progress.WheelID);
                _wheelWrappersProgress.Add(new WheelWrapperProgress() {Wheel = prefab, Progress = progress});
            });
        }
        
        public void SetupSkinWrappersProgressFromLibrary(SkinsLibrary library)
        {
            _skinWrappersProgress = new List<PurchasableSkinWrapperProgress>();
            _skinsLibrary = library;

            _skinsProgress.ForEach(progress =>
            {
                var skin = library.GetSkinByGroupID(progress.SkinGroupID);
                _skinWrappersProgress.Add(new PurchasableSkinWrapperProgress() {Skin = skin, Progress = progress});
            
                Debug.Log($"== skin add [{progress.SkinGroupID}] {skin.Name}");
            });
        }

        public void UpdateCharacterProgress(CharacterProgress characterProgress)
        {
            _characterProgress = characterProgress;
            Changed?.Invoke();
        }

        private void SetupCharacterInitialProgress(InitialCharacterStatsData characterStatsData)
        {
            _characterProgress = characterStatsData.CharacterProgress;
            _notConsideredCharacterStatPoints = _characterProgress.ModifierPoints.GetPointsTotal();
        }

        private void SetupLevelProgress()
        {
            _levelsProgress = new List<LevelProgress>();
        }

        public void AddMoney(int addedMoney)
        {
            if (addedMoney <= 0)
                throw new InvalidOperationException();
            
            _money += addedMoney;
            Changed?.Invoke();
        }

        private void SetupMoney(int moneyStartValue)
        {
            _money = moneyStartValue;
        }

        private void SetupWheelProgress(InitialWheelsData initialWheelsData)
        {
            _wheelsProgress = new List<WheelProgress>();

            foreach (var data in initialWheelsData.WheelsData)
            {
                _wheelsProgress.Add(new WheelProgress(initialWheelsData.GetWheelID(data.Wheel), data.IsCurrent, data.IsPurchased));
            }
        }

        public void AddLevelProgress(LevelProgress levelProgress)
        {
            _levelsProgress.Add(levelProgress);
            Changed?.Invoke();
        }

        public void RemoveLevelProgress(LevelProgress levelProgress)
        {
            _levelsProgress.Remove(levelProgress);
            Changed?.Invoke();
        }

        public int GetUnlockedCharacterStatsPoints()
        {
            var count = 0;
            _levelsProgress.ForEach(p => { count += p.GetAchievedCharacterStatsPoints();});

            return count;
        }

        public int GetSpendedCharacterStatsPoints() 
        {
            return _characterProgress.ModifierPoints.GetPointsTotal() - _notConsideredCharacterStatPoints;
        }

        public int GetFreeCharacterStatsPoints()
        {
            return GetUnlockedCharacterStatsPoints() - GetSpendedCharacterStatsPoints();
        }

        public bool HasLevelProgress(string sceneName)
        {
            var result = _levelsProgress.Count(p => p.SceneName == sceneName) > 0;
        
            return result;
        }

        public LevelProgress GetLevelProgressBySceneName(string sceneName)
        {
            return _levelsProgress.First(p => p.SceneName == sceneName);
        }
        
        private void SetupPurchasableSkinsProgress(List<PurchasableSkin> purchasableSkins)
        {
            if (purchasableSkins == null)
                throw new ArgumentNullException();

            _skinsProgress = purchasableSkins
                .Select(purchasableSkin => new PurchasableSkinProgress(purchasableSkin.SkinGroupID, purchasableSkin.IsDefault, purchasableSkin.IsDefault))
                .ToList();
        }
        
        public void UpdatePurchasableSkinProgress(PurchasableSkinProgress purchasableSkinProgress)
        {
            var index = _skinsProgress.FindIndex(progress => progress.SkinGroupID == purchasableSkinProgress.SkinGroupID);
            if (index == -1)
                throw new InvalidOperationException("Skin progress that you want change is not found in PlayerProgress");
            
            _skinsProgress[index] = purchasableSkinProgress;
            
            Changed?.Invoke();
        }

        public List<MonoWheel> GetPurchasedWheels()
        {
            return _wheelWrappersProgress.Where(wp => wp.Progress.IsPurchased)
                .Select(wp => wp.Wheel)
                .ToList();
        }

        public List<MonoWheel> GetAllWheels()
        {
            return _wheelWrappersProgress.Select(wp => wp.Wheel).ToList();
        }

        public MonoWheel GetCurrentWheel()
        {
            var progressForCurrentWheel = _wheelWrappersProgress.First(wp => wp.Progress.IsCurrent);
            return progressForCurrentWheel.Wheel;
        }

        public void ChangeCurrentWheel(MonoWheel wheel)
        {
            if (wheel == null)
                throw new NullReferenceException();

            var isPurchased = IsPurchasedWheel(wheel);
            
            if (isPurchased == false)
                throw new InvalidOperationException($"Non purchased wheel can't be selected as current!");

            ChangeCurrentWheelByID(_wheelsLibrary.GetIDByWheel(wheel));
        }

        public bool IsPurchasedWheel(MonoWheel wheel)
        {
            var wheelID = _wheelsLibrary.GetIDByWheel(wheel);
            var foundedProgress = _wheelsProgress.First(progress => progress.WheelID == wheelID);
            
            return foundedProgress.IsPurchased;
        }

        public void PurchaseWheel(MonoWheel wheel)
        {
            var wheelID = _wheelsLibrary.GetIDByWheel(wheel);
            var foundedProgressIndex = _wheelsProgress.FindIndex(progress => progress.WheelID == wheelID);
            var foundedPrefabProgressIndex = _wheelWrappersProgress.FindIndex(progress => progress.Progress.WheelID == wheelID);
            var foundedProgress = _wheelsProgress[foundedProgressIndex];
            
            if (foundedProgress.IsPurchased)
                throw new InvalidOperationException($"Wheel {wheel} is already purchased!");
            
            var newWheelProgress = new WheelProgress(foundedProgress.WheelID, foundedProgress.IsCurrent, true);
            _wheelsProgress[foundedProgressIndex] = newWheelProgress;

            _wheelWrappersProgress[foundedPrefabProgressIndex] = new WheelWrapperProgress()
            {
                Wheel = _wheelWrappersProgress[foundedPrefabProgressIndex].Wheel,
                Progress = newWheelProgress
            };

            var newMoneyValue = Money - wheel.Parameters.ShopParameters.Cost;
            if(newMoneyValue < 0)
                throw new InvalidOperationException($"Wheel {wheel} can't be purchased while money is not enough!");

            _money = newMoneyValue;
            
            Changed?.Invoke();
        }

        private void ChangeCurrentWheelByID(string wheelID)
        {
            var prefabProgressForOldCurrentWheelIndex = _wheelWrappersProgress.FindIndex(wp => wp.Progress.IsCurrent);
            var prefabProgressForNewCurrentWheelIndex = _wheelWrappersProgress.FindIndex(wp => wp.Progress.WheelID == wheelID);
            var progressForOldCurrentWheelIndex = _wheelsProgress.FindIndex(wp => wp.IsCurrent);
            var progressForNewCurrentWheelIndex = _wheelsProgress.FindIndex(wp => wp.WheelID == wheelID);
        
            var oldCurrentProgress = _wheelWrappersProgress[prefabProgressForOldCurrentWheelIndex];
            _wheelWrappersProgress[prefabProgressForOldCurrentWheelIndex] = new WheelWrapperProgress()
            {
                Wheel = oldCurrentProgress.Wheel,
                Progress = new WheelProgress(oldCurrentProgress.Progress.WheelID, false, oldCurrentProgress.Progress.IsPurchased)
            };
        
            var newCurrentProgress = _wheelWrappersProgress[prefabProgressForNewCurrentWheelIndex];
            _wheelWrappersProgress[prefabProgressForNewCurrentWheelIndex] = new WheelWrapperProgress()
            {
                Wheel = newCurrentProgress.Wheel,
                Progress = new WheelProgress(newCurrentProgress.Progress.WheelID, true, newCurrentProgress.Progress.IsPurchased)
            };

            _wheelsProgress[progressForOldCurrentWheelIndex] = _wheelWrappersProgress[prefabProgressForOldCurrentWheelIndex].Progress;
            _wheelsProgress[progressForNewCurrentWheelIndex] = _wheelWrappersProgress[prefabProgressForNewCurrentWheelIndex].Progress;

            Changed?.Invoke();
        }
    }
}