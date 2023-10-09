using System;
using Character.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bike
{
    [HideMonoScript]
    public class BikeHandler : MonoBehaviour
    {
        [SerializeField] private Hero testRider;
        [SerializeField] private SteererController handleBar;
        [SerializeField] private CrankController crank;
        
        public event Action<float> OnSteerAngleChanged;

        private Hero _mountedRider;
        
        private void Awake()
        {
            Mount(testRider);

            handleBar.OnChangeSteer += InvokeSteerAngleChanged;
        }

        private void OnDestroy()
        {
            handleBar.OnChangeSteer -= InvokeSteerAngleChanged;
        }

        public void Mount(ICharacter character)
        {
            if (character == null)
                return;

            _mountedRider = character as Hero;
            crank.SetFootMounts(character.RightFootMount, character.LeftFootMount);
            handleBar.SetHandMounts(character.RightHandMount, character.LeftHandMount);
        }

        protected virtual void InvokeSteerAngleChanged(float angle)
        {
            OnSteerAngleChanged?.Invoke(angle);

            if (_mountedRider != null)
                _mountedRider.SteerTwist(angle);
        }
    }
}