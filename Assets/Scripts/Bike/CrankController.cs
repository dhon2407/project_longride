using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bike
{
    [HideMonoScript]
    public class CrankController : MonoBehaviour
    {
        [SerializeField, Min(0)] private float rotateSpeed = 10;
        
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
            
            /* Rotate the whole crank clockwise(+angle) */
            _transform.RotateAround(_transform.position, -_transform.right, angle);
        }

#if UNITY_EDITOR
        private void Update()
        {
            float input = Input.GetAxisRaw("Vertical");
            if (input != 0)
                Rotate(input * rotateSpeed * Time.deltaTime);
        }
#endif
    }
}