using System;
using Character.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bike
{
    [HideMonoScript]
    public class BikeHandler : MonoBehaviour
    {
        [FoldoutGroup("References")] [SerializeField]
        private SteererController handleBar;
        [FoldoutGroup("References")] [SerializeField]
        private CrankController crank;
        
        [FoldoutGroup("Parameters")] [SerializeField]
        private float maxTurnAngle = 20f;
        [FoldoutGroup("Parameters")] [SerializeField]
        private uint turnSpeed = 90U;
        
        public event Action<float> OnSteerAngleChanged;

        private Hero _mountedRider;
        private float _currentTurnAngle;
        
        private void Awake()
        {
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
            
            _mountedRider.transform.SetParent(transform);
        }

        public void SetPower(float power)
        {
            transform.Translate(transform.forward.normalized * power * Time.deltaTime);
        }

        public void Steer(float steerValue)
        {
            _currentTurnAngle = Mathf.Clamp(_currentTurnAngle + (turnSpeed * Time.deltaTime * steerValue), -maxTurnAngle,
                maxTurnAngle);
            
            handleBar.Rotate(_currentTurnAngle);
        }

        protected virtual void InvokeSteerAngleChanged(float angle)
        {
            OnSteerAngleChanged?.Invoke(angle);

            if (_mountedRider != null)
                _mountedRider.SteerTwist(angle);
        }
    }
}