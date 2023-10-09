using Sirenix.OdinInspector;
using UnityEngine;

namespace Bike
{
    [HideMonoScript]
    public class CrankController : MonoBehaviour
    {
        [SerializeField, Min(0)] private float rotateSpeed = 10;
        [TitleGroup("Pedal Mount")]
        [SerializeField] private Transform rightPedalMount;
        [TitleGroup("Pedal Mount")]
        [SerializeField] private Transform leftPedalMount;
        
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
            rightPedalMount.RotateAround(rightPedalMount.position, transform.right, angle);
            leftPedalMount.RotateAround(leftPedalMount.position, transform.right, angle);
        }
        
        public void SetFootMounts(Transform characterRightFootMount, Transform characterLeftFootMount)
        {
            characterRightFootMount.SetParent(rightPedalMount);
            characterRightFootMount.localPosition = Vector3.zero;
            
            characterLeftFootMount.SetParent(leftPedalMount);
            characterLeftFootMount.localPosition = Vector3.zero;
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