using System;
using Bike;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Character
{
    [HideMonoScript]
    public class Hero : MonoBehaviour, ICharacter
    {
        [TitleGroup("Body Mounts")]
        [SerializeField] private Transform rightFootMount;
        [SerializeField] private Transform leftFootMount;
        [TitleGroup("Body Mounts")]
        [SerializeField] private Transform rightHandMount;
        [SerializeField] private Transform leftHandMount;

        [TitleGroup("Body Parts")]
        [SerializeField] private Transform twistSpine;

        [SerializeField] private float twistRatio = 0.01f;

        [SerializeField] private BikeHandler targetBike;

        private bool _mounted;

        public Transform LeftFootMount => leftFootMount;
        public Transform RightFootMount => rightFootMount;
        public Transform LeftHandMount => leftHandMount;
        public Transform RightHandMount => rightHandMount;

        private Quaternion _twistingSpineOriginalRotation;

        private void Awake()
        {
            _twistingSpineOriginalRotation = twistSpine.localRotation;
        }

        private void Start()
        {
            MountCurrentTarget();
        }

        public void SteerTwist(float angle)
        {
            twistSpine.localRotation = _twistingSpineOriginalRotation * Quaternion.Euler(0, angle * twistRatio, 0);
        }
        
        [Button]
        private void MountCurrentTarget()
        {
            if (targetBike != null)
            {
                targetBike.Mount(this);
                _mounted = true;
            }
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            if (!_mounted)
                return;
            
            float input = Input.GetAxisRaw("Horizontal");
            if (input != 0)
                targetBike.Steer(-input);

            input = Input.GetAxisRaw("Vertical");
            if (input != 0)
                targetBike.SetPower(-input * 10f);
            

        }
#endif
    }
}