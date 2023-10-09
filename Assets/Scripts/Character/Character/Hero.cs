using Sirenix.OdinInspector;
using UnityEngine;

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

        public Transform LeftFootMount => leftFootMount;
        public Transform RightFootMount => rightFootMount;
        public Transform LeftHandMount => leftHandMount;
        public Transform RightHandMount => rightHandMount;
    }
}