using UnityEngine;


namespace CAI
{
    public class TrackCamera : MonoBehaviour
    {
        [SerializeField] Transform target = null;
        [SerializeField] float rate = 0.9f;
        [SerializeField] float maxLimit = 0.1f;
        Transform myTransform;

        void Start()
        {
            myTransform = GetComponent<Transform>();
        }

        void Update()
        {
            Vector3 delta = target.position - myTransform.position;
            Vector3 move = delta * rate;
            if(move.magnitude > maxLimit)
            {
                move = move.normalized * maxLimit;
            }
            myTransform.Translate(move, Space.World);
        }
    }
}