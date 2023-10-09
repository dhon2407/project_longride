using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Helpers;

namespace Bike
{
    [HideMonoScript]
    public class SteererController : MonoBehaviour
    {
        [SerializeField, Min(0)] private float rotateSpeed = 100;
        
        [TitleGroup("Hand Mount")]
        [SerializeField] private Transform rightHandMount;
        [TitleGroup("Hand Mount")]
        [SerializeField] private Transform leftHandMount;
        
        [TitleGroup("On Mount Hand Rotation Correction")] [SerializeField]
        private Vector3 rightHandCorrection = new (38.1023254f,214.422928f,7.72168016f);
        [TitleGroup("On Mount Hand Rotation Correction")] [SerializeField]
        private Vector3 leftHandCorrection = new (321.503021f,338.36673f,344.413727f);

        public event Action<float> OnChangeSteer; 

        private Transform _transform;
        private bool _active;

        private void Awake()
        {
            _transform = transform;
            _active = _transform != null;
        }

        public void Rotate(float angle)
        {
            if (!_active)
                return;

            _transform.RotateAround(_transform.position, _transform.up, angle);
            InvokeOnChangeSteer(angle);
        }
        
        public void SetHandMounts(Transform rightHand, Transform leftHand)
        {
            rightHand.SetParent(rightHandMount);
            rightHand.localPosition = Vector3.zero;
            
            leftHand.SetParent(leftHandMount);
            leftHand.localPosition = Vector3.zero;
                
            CallTiming.InvokeOnNextFrame(() =>
            {
                rightHand.localRotation = Quaternion.Euler(rightHandCorrection);
                leftHand.localRotation = Quaternion.Euler(leftHandCorrection);
            }, gameObject);
        }
        
        protected virtual void InvokeOnChangeSteer(float angle)
        {
            OnChangeSteer?.Invoke(angle);
        }

#if UNITY_EDITOR
        private void Update()
        {
            float input = Input.GetAxisRaw("Horizontal");
            if (input != 0)
                Rotate(-input * rotateSpeed * Time.deltaTime);

        }
#endif
    }
}