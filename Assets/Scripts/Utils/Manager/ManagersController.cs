using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils.Manager
{
    [HideMonoScript]
    public class ManagersController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private List<Component> activeManagers;
        
        private static ManagersController _instance;
        // private static DevSettings Dev => GameSettings.Dev;
        
        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            // DOTween.SetTweensCapacity(Dev.TweenCapacity.capacity, Dev.TweenCapacity.sequences);
            _instance = this;
            DontDestroyOnLoad(this);
        }

        public void AddManager<T>(SingletonManager<T> manager) where T : Component
        {
            activeManagers.Add(manager);
            manager.transform.SetParent(transform);
        }
    }
}
