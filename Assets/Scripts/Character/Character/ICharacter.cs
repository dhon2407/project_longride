using UnityEngine;

namespace Character.Character
{
    public interface ICharacter
    {
        Transform LeftFootMount { get; }
        Transform RightFootMount { get; }
        Transform LeftHandMount { get; }
        Transform RightHandMount { get; }
    }
}