using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Camera
{
    [HideMonoScript]
    public class CameraTarget : MonoBehaviour
    {
        [TitleGroup("References")] [SerializeField]
        private Transform mainPlayer;

        [SerializeField]
        private float distanceFromPlayer = 0;

        private Vector3 _adjustedPosition;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            _adjustedPosition = mainPlayer.position;
            _adjustedPosition.z += distanceFromPlayer;
            _transform.position = _adjustedPosition;
        }
    }
}