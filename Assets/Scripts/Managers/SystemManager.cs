using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Manager;

namespace Managers
{
    [HideMonoScript]
    public class SystemManager : SingletonManager<SystemManager>
    {
        private static SystemManager Instance =>
            _instance ? _instance : throw new UnityException($"No instance of {nameof(SystemManager)}");
        
        private static SystemManager _instance;

        protected override SystemManager Self
        {
            set => _instance = value;
            get => _instance;
        }
    }
}
