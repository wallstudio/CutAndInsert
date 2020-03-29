using UnityEngine;


namespace CAI
{
    public class MoveAreaLimit : MonoBehaviour
    {   
        [SerializeField] BoxCollider area = null;
    
        Transform myTransform;

        void Start()
        {
            myTransform = GetComponent<Transform>();
        }

        void Update()
        {
            if(!area.bounds.Contains(myTransform.position))
            {
                transform.position = area.bounds.ClosestPoint(myTransform.position);
            }
        }
    }
}