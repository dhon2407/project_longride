using UnityEngine;

namespace Utils.Manager
{
    public abstract class SingletonManager<T> : MonoBehaviour where T : Component
    {
        protected abstract T Self { set; get; }
        protected virtual void Init() {}
        
        protected static T CreateNewInstance()
        {
            var newInstance = new GameObject(typeof(T).Name);
            return newInstance.AddComponent<T>();
        }

        private void Awake()
        {
            if (Self != null)
            {
                Destroy(gameObject);
                return;
            }
            
            var managerController = FindObjectOfType<ManagersController>();
            if (!managerController)
            {
                var newInstance = new GameObject(nameof(ManagersController));
                managerController = newInstance.AddComponent<ManagersController>();
            }

            managerController.AddManager(this);
            Self = this as T;
            Init();
        }
    }
}