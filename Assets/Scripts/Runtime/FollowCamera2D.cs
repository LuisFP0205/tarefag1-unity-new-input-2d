using UnityEngine;

namespace Aula0.Runtime
{
    public sealed class FollowCamera2D : MonoBehaviour
    {
        [SerializeField] private bool keepFixed = true;

        private Vector3 _fixedPosition;

        private void Awake()
        {
            _fixedPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (keepFixed)
            {
                transform.position = _fixedPosition;
            }
        }
    }
}
