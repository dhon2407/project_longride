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
        

        public Transform LeftFootMount => leftFootMount;
        public Transform RightFootMount => rightFootMount;
        public Transform LeftHandMount => leftHandMount;
        public Transform RightHandMount => rightHandMount;

        public void SteerTwist(float angle)
        {
            twistSpine.Rotate(twistSpine.up, angle * twistRatio, Space.World);
        }
    }
}