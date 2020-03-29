using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CAI
{
    [RequireComponent(typeof(Paper))]
    public class Finger : MonoBehaviour
    {
        enum State
        {
            None,
            Scissors,
            MovePaper,
            MoveCamera,
            Zoom,
        }

        [SerializeField] Transform cameraLeader = null;
        [SerializeField] float cameraSensitive = 0.1f;
        [SerializeField] float cameraZoomSensitive = 0.1f;
        [SerializeField] float sensitive = 0.01f;
        [SerializeField] Material track = null;

        State _state = State.None;
        State state { get => _state; set { Debug.Log($"{_state} => {value}"); _state = value; } }

        public LineRenderer lineRenderer { get; private set; }

        Vector2 startPos;
        Vector3 startWorldPos;
        Paper paper;
        List<Vector2> cutting = new List<Vector2>();
        List<Vector2> cuttingUv = new List<Vector2>();
        Transform grabing = null;


        void Start()
        {
            paper = GetComponent<Paper>();
        }

        void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (RayCast(ray, out RaycastHit hit)
                    && hit.collider.tag == "Paper")
                {
                    startPos = hit.textureCoord;
                    startWorldPos = hit.point;
                    state = State.Scissors;
                    cutting.Add(new Vector2(startWorldPos.x, startWorldPos.z));
                    cuttingUv.Add(startPos);
                    lineRenderer = new GameObject().AddComponent<LineRenderer>();
                    lineRenderer.widthCurve = new AnimationCurve(new Keyframe(0, 0.01f), new Keyframe(1, 0.01f));
                }
            }
            else if(Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(RayCast(ray, out RaycastHit hit)
                    && hit.collider.tag == "Paper"
                    && state == State.Scissors)
                {
                    Vector2 currentPos = hit.textureCoord;
                    if((currentPos - startPos).magnitude > sensitive)
                    {
                        // Cut!
                        // Debug.DrawLine(startWorldPos, hit.point, Color.red, 3);
                        startPos = hit.textureCoord;
                        startWorldPos = hit.point;
                        cutting.Add(new Vector2(hit.point.x, hit.point.z));
                        cuttingUv.Add(hit.textureCoord);
                        lineRenderer.positionCount = cutting.Count;
                        lineRenderer.SetPositions(cutting.Select(v => new Vector3(v.x, 0.1f, v.y)).ToArray());
                        lineRenderer.material = track;
                    }
                }
            }
            else if(Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (RayCast(ray, out RaycastHit hit)
                    && hit.collider.tag == "PaperPiece")
                {
                    grabing = hit.collider.transform;
                    startPos = Input.mousePosition;
                    startWorldPos = hit.point;
                    state = State.MovePaper;
                }
                else
                {
                    startPos = Input.mousePosition;
                    state = State.MoveCamera;
                }
            }
            else if(Input.GetMouseButton(1))
            {
                if(state == State.MovePaper)
                {
                    grabing.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (RayCast(ray, out RaycastHit hit))
                    {
                        grabing.position = hit.point;
                    }
                    grabing.gameObject.layer = LayerMask.NameToLayer("Piece");
                }
                else if(state == State.MoveCamera)
                {
                    Vector2 delta = (Vector2)Input.mousePosition - startPos;
                    cameraLeader.Translate(new Vector3(-delta.x, 0, -delta.y) * cameraSensitive, Space.World);
                    startPos = Input.mousePosition;
                }
            }
            else if(Input.GetMouseButtonDown(2))
            {
                startPos = Input.mousePosition;
                state = State.Zoom;
            }
            else if(Input.GetMouseButton(2))
            {
                float delta = Input.mousePosition.y - startPos.y;
                cameraLeader.Translate(Vector3.forward * delta * cameraZoomSensitive, Space.Self);
                startPos = Input.mousePosition;
            }
            else if(Input.GetMouseButtonUp(0)
                || Input.GetMouseButtonUp(1)
                || Input.GetMouseButtonUp(2))
            {
                if(state == State.Scissors)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (RayCast(ray, out RaycastHit hit)
                        && hit.collider.tag == "Paper")
                    {
                        startWorldPos = hit.point;
                        cutting.Add(new Vector2(startWorldPos.x, startWorldPos.z));
                        cuttingUv.Add(hit.textureCoord);
                    }
                    paper.Cut(cutting, cuttingUv);
                    cutting.Clear();
                    cuttingUv.Clear();
                    Destroy(lineRenderer.gameObject);
                }

                state = State.None;
            }
        }

        static bool RayCast(Ray ray, out RaycastHit hit, int layerMask = ~0)
        {
            bool isHit = Physics.Raycast(ray, out hit, 100, layerMask);
            #if UNITY_EDITOR
            Debug.DrawLine(ray.origin, isHit ? hit.point : (ray.origin + ray.direction * float.MaxValue), Color.blue, 0);
            #endif
            return isHit;
        }
    }
}