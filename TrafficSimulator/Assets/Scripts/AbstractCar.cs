using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractCar : MonoBehaviour
{
    private protected Vector3 _targetDir;
    private protected Vector3 offsetCenterNode;

    [SerializeField] private protected float _carVelocity;
    public float realVelocity;

    private protected RoadGraphNode _currentNode;
    private protected RoadGraphNode _targetNode;

    public float CarVelocity
    {
        get
        {
            return _carVelocity;
        }
        set
        {
            _carVelocity = value;
        }
    }

    public RoadGraphNode CurrentNode
    {
        get
        {
            return _currentNode;
        }
        set
        {
            _currentNode = value;
        } 
    }

    private protected Stack<RoadGraphNode> _pathFromAtoB;

    private protected bool _isOnStraightRoad;
    [SerializeField] private protected bool _isOnCrossRoad;
    private protected bool _isOutRoad;
    private protected bool _isRightMovement;
    [SerializeField] private protected bool _isChoosedNewPath;

    public bool isNearCar;

    private protected int _targetNodeIndex;

    private protected float signRot;
    private protected int seed;

    private void Start()
    {
        _pathFromAtoB = new Stack<RoadGraphNode>();

        _isOnStraightRoad = false;
        _isOnCrossRoad = false;
        _isOutRoad = false;
        _isChoosedNewPath = false;

        _isOnCrossRoad = false;
        isNearCar = false;

        _targetNodeIndex = 0;
        _targetNode = _currentNode.neighbors[_targetNodeIndex];

        transform.position += GetRightOffset(_currentNode.roadsToNeighbors[_targetNodeIndex], 3.0f);

        _targetDir= _currentNode.roadsToNeighbors[_targetNodeIndex];

        RotateCar(_targetDir);

        seed = default;

        realVelocity = _carVelocity;
    }


    void Update()
    {
        if (_isOnCrossRoad)
        {
            transform.RotateAround(offsetCenterNode, Vector3.up, 2f * _targetDir.normalized.magnitude * Time.deltaTime * signRot * _carVelocity);
        }
        else
        {
            if (!isNearCar)
                transform.Translate(_targetDir.normalized * Time.deltaTime * _carVelocity, Space.World);
        }

        Debug.DrawLine(transform.position, _currentNode.nodePosition, Color.blue, Time.deltaTime, true);
        Debug.DrawLine(transform.position, _targetNode.nodePosition, Color.yellow, Time.deltaTime, true);

        if (transform.position.y < -10)
            Destroy(this);

        if (Input.GetKeyDown("i"))
        {
            Info();
        }
    }

    private void Info()
    {
        Debug.Log("======================================");
        Debug.Log("_isOnCrossRoad " + _isOnCrossRoad);
        Debug.Log("_isChoosedNewPath " + _isChoosedNewPath);
        Debug.Log("======================================");
    }

    private void RotateCar(Vector3 targetV)
    {
        if (targetV.x == 0 && targetV.z > 0)
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        else if (targetV.x == 0 && targetV.z < 0)
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        else if (targetV.x > 0 && targetV.z == 0)
            transform.eulerAngles = new Vector3(0f, -90f, 0f);
        else if (targetV.x < 0 && targetV.z == 0)
            transform.eulerAngles = new Vector3(0f, -270f, 0f);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        string name = other.gameObject.name;

        if (other.gameObject.GetComponent<Points>())
        {
            GameObject parent = other.gameObject.transform.parent.gameObject;

            if (_isOnCrossRoad == false && (
                (name == "point0") ||
                (name == "point2") ||
                (name == "point4") ||
                (name == "point6")))
            {
                DriveIntoIntersection(_targetNode.neighbors.Count, name, parent);
            }
            else if (_isOnCrossRoad == true)
            {
                DriveFromIntersection(_targetNode.neighbors.Count, name, parent);
            }

            if (_isChoosedNewPath && _isOnCrossRoad == false)
            {
                DriveFromIntersection(_targetNode.neighbors.Count, name, parent);
            }
        }
    }
    
    private void DriveIntoIntersection(int index, string name, GameObject currendRoad)
    {
        if (_isChoosedNewPath == false)
        {
            if (index == 2)
            {
                ChooseOtherPath();

                if (name == "point0")
                {
                    RotateInRoad(0, 1, true, currendRoad, index);
                }
                else if (name == "point2")
                {
                    RotateInRoad(2, 3, false, currendRoad, index);
                }
            }
            else if (index == 3)
            {
                Vector3 oldTargetDir= _targetDir;
                ChooseOtherPath();

                if (name == "point0")
                {
                    if (Vector3.Dot(oldTargetDir, _targetDir) == 0)
                        RotateInRoad(0, 3, true, currendRoad, index);
                }
                else if (name == "point2")
                {
                    if (Vector3.Dot(oldTargetDir, _targetDir) == 0)
                        RotateInRoad(2, 3, false, currendRoad, index);
                }
                else if (name == "point4")
                {
                    Vector3 sumDir = oldTargetDir + _targetDir;

                    if ((currendRoad.transform.eulerAngles.y == 0 && sumDir.x < 0) ||
                        (currendRoad.transform.eulerAngles.y == 90 && sumDir.z > 0) ||
                        (currendRoad.transform.eulerAngles.y == 180 && sumDir.x > 0) ||
                        (currendRoad.transform.eulerAngles.y == 270 && sumDir.z < 0))
                    {
                        RotateInRoad(4, 5, false, currendRoad, index);
                    }
                    else
                    {
                        RotateInRoad(4, 1, true, currendRoad, index);
                    }
                }
            }
            else if (index == 4)
            {
                Vector3 oldTargetDir = _targetDir;
                ChooseOtherPath();

                Vector3 sumDir = oldTargetDir + _targetDir;

                Debug.Log(sumDir);

                if (name == "point0")
                {
                    if ((sumDir.x > 0) && (sumDir.z < 0))
                    {
                        RotateInRoad(0, 1, false, currendRoad, index);
                    }
                    else if ((sumDir.x > 0) && (sumDir.z > 0))
                    {
                        RotateInRoad(0, 5, true, currendRoad, index);
                    }

                }
                else if (name == "point2")
                {
                    if ((sumDir.x > 0) && (sumDir.z > 0))
                    {
                        RotateInRoad(2, 3, true, currendRoad, index);
                    }
                    else if ((sumDir.x < 0) && (sumDir.z > 0))
                    {
                        RotateInRoad(2, 7, false, currendRoad, index);
                    }
                }
                else if (name == "point4")
                {
                    if ((sumDir.x < 0) && (sumDir.z > 0))
                    {
                        RotateInRoad(4, 5, false, currendRoad, index);
                    }
                    else if ((sumDir.x < 0) && (sumDir.z < 0))
                    {
                        RotateInRoad(4, 1, true, currendRoad, index);
                    }
                }
                else if (name == "point6")
                {
                    if ((sumDir.x < 0) && (sumDir.z < 0))
                    {
                        RotateInRoad(6, 7, false, currendRoad, index);
                    }
                    else if ((sumDir.x > 0) && (sumDir.z < 0))
                    {
                        RotateInRoad(6, 3, true, currendRoad, index);
                    }
                }
            }
        }
    }

    private void DriveFromIntersection(int index, string name, GameObject currendRoad)
    {
        if (name == "point1" || name == "point3" || name == "point5" || name == "point7")
        {
            Debug.Log(name);

            if (currendRoad.transform.eulerAngles.y == 0 || currendRoad.transform.eulerAngles.y == 180)
            {
                if (_isRightMovement)
                    transform.position = new Vector3(currendRoad.transform.FindChild(name).transform.position.x, transform.position.y, transform.position.z);
                else
                    transform.position = new Vector3(transform.position.x, transform.position.y, currendRoad.transform.FindChild(name).transform.position.z);
            }
            else
            {
                if (_isRightMovement)
                    transform.position = new Vector3(transform.position.x, transform.position.y, currendRoad.transform.FindChild(name).transform.position.z);
                else
                    transform.position = new Vector3(currendRoad.transform.FindChild(name).transform.position.x, transform.position.y, transform.position.z);
            }

            _isOnCrossRoad = false;
            _isChoosedNewPath = false;

            RotateCar(_targetDir);
        }
    }

    private void RotateInRoad(int childThis, int child, bool isRight, GameObject parent, int index)
    {
        _isOnCrossRoad = true;
        this._isRightMovement = isRight;

        float coeffRot = 0f;
        Vector3 dir;

        if (index == 2)
        {
            if (isRight) { coeffRot = 8.485f; signRot = -1; }
            else { coeffRot = 16.97f; signRot = 1; }
        }
        else if (index == 3)
        {
            if (isRight) { coeffRot = 16.97f; signRot = -1; }
            else { coeffRot = 16.97f; signRot = 1; }
        }


        dir = parent.transform.GetChild(child).gameObject.transform.position - parent.transform.GetChild(childThis).transform.position;

        offsetCenterNode = parent.transform.position + GetRightOffset(dir, coeffRot * signRot);
    }

    private void ChooseOtherPath()
    {
        int currentIndexNode;
        _isChoosedNewPath = true;

        currentIndexNode = _currentNode.nodeIndex;
        _currentNode = _targetNode;

        if (_currentNode.neighbors.Count == 1)
        {
            _targetNode = _currentNode.neighbors[0];
        }
        else if (_currentNode.neighbors.Count == 2)
        {
            _targetNodeIndex = 0;
            if (_currentNode.neighbors[_targetNodeIndex].nodeIndex == currentIndexNode)
            {
                _targetNodeIndex = 1;
                _targetNode = _currentNode.neighbors[_targetNodeIndex];
            }
            else
            {
                _targetNode = _currentNode.neighbors[_targetNodeIndex];
            }
        }
        else
        {
            while (true)
            {
                seed++;
                Random.InitState(seed);
                _targetNodeIndex = Random.Range(0, _currentNode.neighbors.Count);
                _targetNode = _currentNode.neighbors[_targetNodeIndex];

                if (_targetNode.nodeIndex != currentIndexNode) { break; }
            }
        }

        _targetDir= _currentNode.roadsToNeighbors[_targetNodeIndex];
    }

    public Vector3 GetRightOffset(Vector3 target, float offset)
    {
        Vector3 offsetVector;
        offsetVector.x = -target.z * (-1);
        offsetVector.z = target.x * (-1);
        offsetVector.y = 0f;

        return offsetVector.normalized * offset;
    }
}
