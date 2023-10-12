using System;
using Character.Character;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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
        private uint steeringSpeed = 90U;
        [FoldoutGroup("Parameters")] [SerializeField]
        private uint turningSpeed = 30U;
        [FoldoutGroup("Parameters")] [SerializeField]
        private float steerNeutralReturnSmoothTime = 0.5f;

        public bool IsMounted => _mounted;
        public event Action<float> OnSteerAngleChanged;

        private Hero _mountedRider;
        private float _currentTurnAngle;
        private bool _mounted;
        private Vector3 _moveVelocity;
        private Vector3 _moveDirection;
        private Transform _transform;
        private float _currentPower;
        private float _lastSteerInput;
        private float _steerNeutralCorrectionVelocity;

        private void Awake()
        {
            _transform = transform;
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
            
            crank.SetFootMounts(character.RightFootMount, character.LeftFootMount);
            handleBar.SetHandMounts(character.RightHandMount, character.LeftHandMount);
            
            _mountedRider = character as Hero;
            if (_mountedRider == null)
                return;
            
            _mountedRider.transform.SetParent(_transform);
        }

        public void SetPower(float power)
        {
            _currentPower = power;
        }

        public void Steer(float steerValue)
        {
            _lastSteerInput = steerValue;
            _currentTurnAngle = Mathf.Clamp(_currentTurnAngle + (steeringSpeed * Time.deltaTime * steerValue), -maxTurnAngle,
                maxTurnAngle);
            _moveDirection = Quaternion.AngleAxis(-_currentTurnAngle, _transform.up) * _transform.forward * (turningSpeed * Time.deltaTime);

            handleBar.Rotate(_currentTurnAngle);
        }

        private void LateUpdate()
        {
            /* transform movement happens on late update when all inputs are processed */
            _transform.Translate((_transform.forward + _moveDirection).normalized * (_currentPower * Time.deltaTime));

            ResetMoveVelocity();
            ResetSteeringInput();
        }

        private void ResetSteeringInput()
        {
            /* If no input from last frame bring back the steering to center */
            if (_lastSteerInput == 0)
            {
                _currentTurnAngle = Mathf.SmoothDamp(_currentTurnAngle, 0, ref _steerNeutralCorrectionVelocity,
                    steerNeutralReturnSmoothTime);
                handleBar.Rotate(_currentTurnAngle);
            }
            
            _lastSteerInput = 0;
        }

        private void ResetMoveVelocity()
        {
            _moveDirection = Vector3.zero;
            _currentPower = 0;
        }

        protected virtual void InvokeSteerAngleChanged(float angle)
        {
            OnSteerAngleChanged?.Invoke(angle);

            if (IsMounted)
                _mountedRider.SteerTwist(angle);
        }
    }
}