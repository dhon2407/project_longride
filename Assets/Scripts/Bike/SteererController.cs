using Sirenix.OdinInspector;
using UnityEngine;

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
        
        public void SetHandMounts(Transform rightHand, Transform leftHand)
        {
            rightHand.SetParent(rightHandMount);
            rightHand.localPosition = Vector3.zero;
            
            leftHand.SetParent(leftHandMount);
            leftHand.localPosition = Vector3.zero;
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