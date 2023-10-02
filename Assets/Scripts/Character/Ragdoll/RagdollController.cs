using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.Ragdoll
{
    [HideMonoScript]
    public class RagdollController : MonoBehaviour
    {
        private bool _activated = false;
        
        [ShowInInspector]
        private List<ConfigurableJoint> _allJoints;
        private List<Rigidbody> _allRB;
        
        [Button]
        private void FetchParts()
        {
            _allJoints = transform.GetComponentsInChildren<ConfigurableJoint>(true).ToList();
            _allRB = transform.GetComponentsInChildren<Rigidbody>(true).ToList();
        }

        [Button(ButtonSizes.Large), ShowIf("_activated")]
        private void DeactivateJoints()
        {
            SetRB(false);
            SetJoints(false);
            _activated = false;
        }
        
        [Button(ButtonSizes.Large), HideIf("_activated")]
        private void ActivateJoints()
        {
            SetRB(true);
            SetJoints(true);
            _activated = true;
        }

        private void SetJoints(bool active)
        {
            foreach (ConfigurableJoint joint in _allJoints)
            {
                Rigidbody rb = joint.transform.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = !active;
                
                Collider col = joint.transform.GetComponent<Collider>();
                if (col != null)
                    col.enabled = active;
            } 
        }

        private void SetRB(bool active)
        {
            foreach (Rigidbody rb in _allRB)
                rb.isKinematic = !active;
        }

    }
}
