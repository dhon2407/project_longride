using Character.Character;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bike
{
    [HideMonoScript]
    public class BikeHandler : MonoBehaviour
    {
        [SerializeField] private Hero testRider;

        [SerializeField] private SteererController handleBar;
        [SerializeField] private CrankController crank;
        
        private void Awake()
        {
            Mount(testRider);
        }

        public void Mount(ICharacter character)
        {
            if (character == null)
                return;
            
            crank.SetFootMounts(character.RightFootMount, character.LeftFootMount);
            handleBar.SetHandMounts(character.RightHandMount, character.LeftHandMount);
        }
    }
}