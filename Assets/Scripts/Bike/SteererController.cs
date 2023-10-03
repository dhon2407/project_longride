using Sirenix.OdinInspector;
using UnityEngine;

namespace Bike
{
    [HideMonoScript]
    public class SteererController : MonoBehaviour
    {
        [SerializeField, Min(0)] private float rotateSpeed = 100;

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
        }

#if UNITY_EDITOR
        private void Update()
        {
            float input = Input.GetAxisRaw("Horizontal");
            if (input != 0)
                Rotate(input * rotateSpeed * Time.deltaTime);

        }
#endif
    }
}