using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoadGenerator : MonoBehaviour
{
    public static RoadGenerator Instance { get {return _instance;} }

    private static RoadGenerator _instance;

    public List<RoadGraphNode> roadGraphNodes;   // статический список узлов
    public List<GameObject> connectedRoads;   // статический список узлов
    public GameObject[] roadPrefabs;                    // массив префабов (дорожные объекты)

    private List<Vector3Int> _arrayA;                    // список позиций уже созданных векторов (дорог)
    private List<Vector3Int> _arrayB;                    // для проверки дальнейших соединений на пересечение

    // параметризация дорожной системы
    [Range(2, 41)] public int numberNodes;           // количество узлов
    [Range(1, 10)] public int sizeMap;               // размер карты

    public int scaleParameter = 36;                        // характерный размер (от объёма префаба дороги)

    [Range(1, 4)] public int maxConnections;          // максимально допустимое количество соединений дорог (ограничение на количество соседей любого узла)
    [Range(1, 10)] public int maxDistance;          // максимально допустимое количество соединений дорог (ограничение на количество соседей любого узла)

    public Transform plane;                     // позиция земли под дорогой (для ограничения по размеру карты на добавление новых узлов)
    private bool isCreatorRoad;                 // можно ли строить новые дороги?

    public GameObject particleSmoke;
    private void Awake()
    {
        if (_instance)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _instance = this;
    }
    void Start()
    {
        roadGraphNodes = new List<RoadGraphNode>();

        _arrayA = new List<Vector3Int>();
        _arrayB = new List<Vector3Int>();

        if (numberNodes > sizeMap * sizeMap)
            numberNodes = sizeMap * sizeMap;

        Generation();                        // генерация дорожной системы
        CreateVisibleBorders();

        isCreatorRoad = false;
    }

    public void SetCreatorRoad(bool state)
    {
        isCreatorRoad = state;
    }

    public void Generation()
    {
        for (int a = 0; a < numberNodes; a++)   // создание заданного количества дорожных узлов
        {
            int xx, zz;
            bool isSameNode;

            xx = Random.Range(0, sizeMap) * scaleParameter;     // задание рандомной позиции по х с учётом характерного масштаба
            zz = Random.Range(0, sizeMap) * scaleParameter;     // задание рандомной позиции по z с учётом характерного масштаба

            isSameNode = false;

            foreach (RoadGraphNode node in roadGraphNodes)  
            {
                // Проверка: является ли новая позиция - позицией уже существующего узла?
                if (node.nodePosition.x == xx && node.nodePosition.z == zz) 
                { 
                    isSameNode = true; 
                    a--; 
                    break; 
                }
            }

            if (isSameNode == false)
            {
                // Создание дорожного узла с начальной инициализацией уникального номера и позиции
                roadGraphNodes.Add(new RoadGraphNode(a, new Vector3Int(xx, 0, zz))); 
            }

        }

        foreach (RoadGraphNode node in roadGraphNodes)  // поиск соседей
        {
            SearchPathToNeighbors(node, false);
        }

        foreach (RoadGraphNode node in roadGraphNodes)  // создание префаба дороги, для каждого узла
        {
            CreateRoadType(node, false);
        }
        
        foreach (RoadGraphNode node in roadGraphNodes)  // соединение узлов прямой дорогой
        {
            CreateConnectedRoads(node);
        }

        foreach (RoadGraphNode node in roadGraphNodes)  // активация дорожной системы
        {
            node.isActive = true;
        }
        
        Info();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && isCreatorRoad)    
        {
            AddNewRoadSegment();    // Добавить дорожный сегмент
            //Info();
        }
        /*
        if (Input.GetMouseButtonDown(1) && isCreatorRoad)
        {
            DestroyRoadSegment();    // Удалить дорожный сегмент
            Info();
        }
        */
    }

    private void AddNewRoadSegment()
    {
        float correctionTileX;
        float correctionTileZ;

        bool isBusy = false;
        bool isOnMap;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        isOnMap = Physics.Raycast(ray, out hit);

        if (isOnMap)
        {

            GameObject hitObject = hit.collider.gameObject;

            Debug.Log(hitObject.name);

            correctionTileX = Mathf.Round(Mathf.Round(hit.point.x) / scaleParameter) * scaleParameter;
            correctionTileZ = Mathf.Round(Mathf.Round(hit.point.z) / scaleParameter) * scaleParameter;


            foreach (RoadGraphNode node in roadGraphNodes)
            {
                if (node.nodePosition.x == correctionTileX && node.nodePosition.z == correctionTileZ)
                {
                    isBusy = true;
                    break;
                }
            }

            if (isBusy == false)
            {
                roadGraphNodes.Add(new RoadGraphNode(numberNodes++, new Vector3Int(Mathf.RoundToInt(correctionTileX), 0, Mathf.RoundToInt(correctionTileZ))));

                if (hitObject.name != "TS-road-1(Clone)")
                {
                    int last = roadGraphNodes.Count - 1;

                    SearchPathToNeighbors(roadGraphNodes[last], true);
                    CreateRoadType(roadGraphNodes[last], false);
                    CreateConnectedRoads(roadGraphNodes[last]);
                    roadGraphNodes[last].isActive = true;
                }
                else
                {
                    GetTwoRGNbyOneRoad(hitObject);
                    Destroy(hitObject);
                }
            }

        }
    }

    private void DestroyRoadSegment()
    {
        float correctionTileX;
        float correctionTileZ;

        bool isOnMap;
        bool isGraphNode = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        isOnMap = Physics.Raycast(ray, out hit);

        if (isOnMap)
        {

            GameObject hitObject = hit.collider.gameObject;

            Debug.Log(hitObject.name);

            correctionTileX = Mathf.Round(Mathf.Round(hit.point.x) / scaleParameter) * scaleParameter;
            correctionTileZ = Mathf.Round(Mathf.Round(hit.point.z) / scaleParameter) * scaleParameter;


            RoadGraphNode destroyNode = default;

            foreach (RoadGraphNode node in roadGraphNodes)
            {
                if (node.nodePosition.x == correctionTileX && node.nodePosition.z == correctionTileZ)
                {
                    isGraphNode = true;
                    destroyNode = node;
                    break;
                }
            }

            if (isGraphNode)
            {
                for(int i = destroyNode.connectedRoadsToNeighbors.Count - 1; i >= 0; i--)
                {
                    Destroy(destroyNode.connectedRoadsToNeighbors[i]);


                    Debug.DrawLine(destroyNode.connectedRoadsToNeighbors[i].transform.position,
                        destroyNode.connectedRoadsToNeighbors[i].transform.position + new Vector3Int(2, 0, 2), Color.yellow, 6f, false);
                    Debug.DrawLine(destroyNode.connectedRoadsToNeighbors[i].transform.position,
                        destroyNode.connectedRoadsToNeighbors[i].transform.position + new Vector3Int(-2, 0, 2), Color.yellow, 6f, false);
                    Debug.DrawLine(destroyNode.connectedRoadsToNeighbors[i].transform.position,
                        destroyNode.connectedRoadsToNeighbors[i].transform.position + new Vector3Int(2, 0, -2), Color.yellow, 6f, false);
                    Debug.DrawLine(destroyNode.connectedRoadsToNeighbors[i].transform.position,
                        destroyNode.connectedRoadsToNeighbors[i].transform.position + new Vector3Int(-2, 0, -2), Color.yellow, 6f, false);
                }
                destroyNode.connectedRoadsToNeighbors.Clear();

                Destroy(hitObject);
                roadGraphNodes.Remove(destroyNode);
            }
        }
    }

    public void GetTwoRGNbyOneRoad(GameObject road)
    {
        RoadGraphNode phNeigbor1 = null;
        RoadGraphNode phNeigbor2 = null;

        Vector3Int pos = Vector3Int.FloorToInt(road.transform.position);
        pos.y = 0;

        Vector3Int path;
        Vector3Int delta = Vector3Int.zero;

        bool isVertDir;

        int last = roadGraphNodes.Count - 1;

        // Привязка к новым соседям и переприсваивание у них прошлых соседей
        if (road.transform.eulerAngles.y == 0 || road.transform.eulerAngles.y == 180)
            isVertDir = true;
        else
            isVertDir = false;
        
        for (int DIR = 0; DIR < 2; DIR++)
        {
            bool isNeighbor = false;
            path = pos;

            if (DIR == 0) 
            {
                if (isVertDir)
                    delta = new Vector3Int(0, 0, scaleParameter);
                else
                    delta = new Vector3Int(scaleParameter, 0, 0);
            }
            else if (DIR == 1)
            {
                if (isVertDir)
                    delta = new Vector3Int(0, 0, -scaleParameter);
                else
                    delta = new Vector3Int(-scaleParameter, 0, 0);
            }

            while (isNeighbor == false)
            {
                path += delta;

                if (path.x < 0 || path.x > sizeMap * scaleParameter || path.z < 0 || path.z > sizeMap * scaleParameter)
                    break;
                
                foreach (RoadGraphNode probublyNeighbor in roadGraphNodes)
                {
                    if (probublyNeighbor.isSamePosition(path))
                    {
                        if (phNeigbor1 == null)
                        {
                            phNeigbor1 = probublyNeighbor;
                            isNeighbor = true;
                            break;
                        }
                        else
                        {
                            phNeigbor2 = probublyNeighbor;
                            isNeighbor = true;
                            break;
                        }
                    }
                }
            }
        }

        foreach (RoadGraphNode nei1 in phNeigbor1.neighbors)
        {
            if (nei1.nodeIndex == phNeigbor2.nodeIndex)
            {
                phNeigbor1.neighbors.Remove(nei1);
                break;
            }
        }
        roadGraphNodes[last].neighbors.Add(phNeigbor1);
        phNeigbor1.neighbors.Add(roadGraphNodes[last]);
        
        foreach (RoadGraphNode nei2 in phNeigbor2.neighbors)
        {
            if (nei2.nodeIndex == phNeigbor1.nodeIndex)
            {
                phNeigbor2.neighbors.Remove(nei2);
                break;
            }
        }
        roadGraphNodes[last].neighbors.Add(phNeigbor2);
        phNeigbor2.neighbors.Add(roadGraphNodes[last]);
        
        // Нахождение новых соседей без учета выше устоявшихся
        for (int DIR = 0; DIR < 2; DIR++)
        {
            bool isNeighbor = false;
            path = pos;
            path.y = 0;

            if (DIR == 0)
            {
                if (!isVertDir)
                    delta = new Vector3Int(0, 0, scaleParameter);
                else
                    delta = new Vector3Int(scaleParameter, 0, 0);
            }
            else if (DIR == 1)
            {
                if (!isVertDir)
                    delta = new Vector3Int(0, 0, -scaleParameter);
                else
                    delta = new Vector3Int(-scaleParameter, 0, 0);
            }

            while (isNeighbor == false)
            {
                path += delta;

                if (Vector3.Distance(roadGraphNodes[last].nodePosition, path) / scaleParameter > maxDistance)
                    break;

                if (path.x < 0 || path.x > sizeMap * scaleParameter || path.z < 0 || path.z > sizeMap * scaleParameter)
                    break;

                foreach (RoadGraphNode probublyNeighbor in roadGraphNodes)
                {
                    if (probublyNeighbor.isSamePosition(path))
                    {
                        bool isCross = false;

                        if (_arrayA.Count > 0)
                        {
                            for (int i = 0; i < _arrayA.Count; i++)
                            {
                                if (roadGraphNodes[last].IsCrossing(roadGraphNodes[last].nodePosition, probublyNeighbor.nodePosition, _arrayA[i], _arrayB[i]) == true)
                                {
                                    isCross = true;
                                    break;
                                }
                            }
                        }

                        if (isCross == false)
                        {
                            roadGraphNodes[last].neighbors.Add(probublyNeighbor);

                            probublyNeighbor.neighbors.Add(roadGraphNodes[last]);
                            probublyNeighbor.CreateAllVectorsToNeighbors(true);
                            CreateRoadType(probublyNeighbor, true);

                            _arrayA.Add(roadGraphNodes[last].nodePosition);
                            _arrayB.Add(probublyNeighbor.nodePosition);

                            isNeighbor = true;
                            break;
                        }
                    }
                }
            }
        }

        CreateRoadType(roadGraphNodes[last], false);
        CreateConnectedRoads(roadGraphNodes[last]);
        roadGraphNodes[last].CreateAllVectorsToNeighbors(false);
        roadGraphNodes[last].isActive = true;
    }

    private void Info()
    {
        string str = string.Empty;

        foreach (RoadGraphNode node in roadGraphNodes)  // обозначение узлов
        {
            str += $"{node.nodeIndex} : (";
            foreach (RoadGraphNode neighbor in node.neighbors)  // обозначение узлов
            {
                str += $"{neighbor.nodeIndex}, ";
            }
            Debug.Log(str + ")");
            str = string.Empty;
        }
    }

    private void CreateVisibleBorders()
    {
        plane.position = new Vector3((sizeMap - 1) * 18f, -1f, (sizeMap - 1) * 18f);
        plane.localScale = new Vector3((sizeMap - 1) * 40f, 2f, (sizeMap - 1) * 40f);

        Camera.main.transform.position = new Vector3((sizeMap - 1) * 18f, 100f, (sizeMap - 1) * 18f);
        Camera.main.orthographicSize = (sizeMap - 1) * 22f;
    }

    private bool IsStreightNeighbor(Vector3 pos1, Vector3 pos2)
    {
        if ((pos1.x == pos2.x && pos1.z != pos2.z) ||
            (pos1.z == pos2.z && pos1.x != pos2.x))
            return true;
        else
            return false;
    }

    private void SearchPathToNeighbors(RoadGraphNode node, bool isUserAdd)
    {
        Vector3Int path;                        // вектор - копия позиции текущего узла (изменяемый в процессе)
        Vector3Int delta = Vector3Int.zero;     // вектор - шаг по пространству 

        for (int DIR = 0; DIR < 4; DIR++)   // поиск по четырём направлениям
        {
            // Проверка: может ли данный узел иметь ещё одного соседа?
            if (node.neighbors.Count < maxConnections)   
            {
                bool isNeighbor = false;    // логическая переменая: сосед?

                path = node.nodePosition;                                               // копирование позиции текущего узла
                if (DIR == 0) { delta = new Vector3Int(scaleParameter, 0, 0); }         // задание шага по х
                else if (DIR == 1) { delta = new Vector3Int(0, 0, scaleParameter); }    // задание шага по z
                else if (DIR == 2) { delta = new Vector3Int(-scaleParameter, 0, 0); }   // задание шага по х (в обратном направлении) 
                else if (DIR == 3) { delta = new Vector3Int(0, 0, -scaleParameter); }   // задание шага по z (в обратном направлении)

                while (isNeighbor == false)     // поиск соседа
                {
                    path += delta;  // смешение позиции на заданный шаг

                    // Проверка: позволяет ли заданная максимальная дистация иметь соседа на данной позиции ?
                    if (Vector3.Distance(node.nodePosition, path) / scaleParameter > maxDistance)
                        break;

                    // Проверка: ищем ли мы на пределами заданной карты ?
                    if (path.x < 0 || path.x > sizeMap * scaleParameter ||
                        path.z < 0 || path.z > sizeMap * scaleParameter)
                        break;

                    foreach (RoadGraphNode probublyNeighbor in roadGraphNodes)
                    {
                        // Проверка: есть ли на этом шаге узел и не является ли возможный узел текущим узлом?
                        if (probublyNeighbor.isSamePosition(path) &&
                            node.isSamePosition(probublyNeighbor.nodePosition) == false)
                        {
                            bool isCross = false;       // логическая переменнаяд для установления пересечения

                            if (_arrayA.Count > 0)      // Проверка: существует хотя бы один элемент массива уже созданных путей? 
                            {
                                for (int i = 0; i < _arrayA.Count; i++)
                                {
                                    // Проверка: пересекается ли путь от текущего узла до возможного с уже существующими путями
                                    if (node.IsCrossing(node.nodePosition, probublyNeighbor.nodePosition, _arrayA[i], _arrayB[i]) == true) 
                                    { 
                                        isCross = true; 
                                        break; 
                                    }
                                }
                            }

                            // Проверка: Если нет пересечений, то добавляет соседа в список
                            if (isCross == false)
                            {
                                // добавление нового соседа в список соседей
                                node.neighbors.Add(probublyNeighbor);

                                // Проверка: вызван ли данный метод генератором ? (если нет - то дорогу создаёт пользователь)
                                if (isUserAdd)
                                {
                                    // Если дорожный сегмент создал пользователь, то текущий узел добавляется в список принадлежащий соседу
                                    probublyNeighbor.neighbors.Add(node);
                                    probublyNeighbor.CreateAllVectorsToNeighbors(true);

                                    CreateRoadType(probublyNeighbor, true);
                                }

                                _arrayA.Add(node.nodePosition);                  // заносим в список новый путь
                                _arrayB.Add(probublyNeighbor.nodePosition);     
                                isNeighbor = true;                               // подтверждаем, что нашли нового соседа  
                                break;                                           // прекращаем поис других соседей в текущем направлении
                            }
                        }
                    }
                }
            }
        }
        node.CreateAllVectorsToNeighbors(false);    // Создание множества направлений от текущего узла до каждого найденного соседа
        
    }

    private void DestroyErrorRoad(RoadGraphNode node)
    {
        if (node.neighbors.Count == 1)
        {
            node.neighbors[0].neighbors.Remove(node);
            roadGraphNodes.Remove(node);
        }
        else if (node.neighbors.Count == 2)
        {
            if (node.isStraightConnect(node.nodePosition, node.neighbors[0].nodePosition, node.neighbors[1].nodePosition))
            {
                node.neighbors[0].neighbors.Remove(node);
                node.neighbors[1].neighbors.Remove(node);
                roadGraphNodes.Remove(node);
            }
        }
    }

    public void CreateRoadType(RoadGraphNode current, bool replacement)
    {
        Vector3 offsetUp = new Vector3(0, 1, 0);

        if (replacement)
            Destroy(current.roadType);

        if (current.neighbors.Count == 0)
        {
        }
        else if (current.neighbors.Count == 1)
        {
            current.roadType = Instantiate(roadPrefabs[0], 
                current.nodePosition + offsetUp, 
                Quaternion.Euler(0f, current.SetAngleForSingleRoad(current.nodePosition, current.neighbors[0].nodePosition), 0f));
        }
        else if (current.neighbors.Count == 2)
        {
            if (current.isStraightConnect(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition) == true)
            {
                current.roadType = Instantiate(roadPrefabs[0], 
                    current.nodePosition + offsetUp, 
                    Quaternion.Euler(0f, current.SetAngleForDoubleRoadStraight(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition), 0f));
            }
            else
            {
                current.roadType = Instantiate(roadPrefabs[1], 
                    current.nodePosition + offsetUp, 
                    Quaternion.Euler(0f, current.SetAngleForDoubleRoad(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition), 0f));
            }
        }
        else if (current.neighbors.Count == 3)
        {
            current.roadType = Instantiate(roadPrefabs[2], 
                current.nodePosition + offsetUp, 
                Quaternion.Euler(0f, 
                current.SetAngleForTripleRoad(current.nodePosition, current.neighbors[0].nodePosition, current.neighbors[1].nodePosition, current.neighbors[2].nodePosition), 
                0f));
        }
        else if (current.neighbors.Count == 4)
        {
            current.roadType = Instantiate(roadPrefabs[3], current.nodePosition + offsetUp, Quaternion.identity);
        }

    }

    private void CreateConnectedRoads(RoadGraphNode node)
    {
        Vector3 offsetUp = new Vector3(0, 1, 0);

        for (int n = 0; n < node.neighbors.Count; n++)
        {
            if (node.neighbors[n].isActive == true)
            {
                Vector3Int[] connectionPositions = node.CreateConnectedRoads(node.neighbors[n]);

                for (int c = 0; c < connectionPositions.Length; c++)
                {
                    connectedRoads.Add(Instantiate(roadPrefabs[0], 
                        connectionPositions[c] + offsetUp, 
                        Quaternion.Euler(0f, node.SetAngleForSingleRoad(node.nodePosition, node.neighbors[n].nodePosition), 0f)));

                    node.connectedRoadsToNeighbors.Add(connectedRoads[c]);
                }
            }
        }
        node.isActive = false;
    }

    private void FixedUpdate()
    {
        // отрисовка вспомогательных элементов

        foreach (RoadGraphNode node in roadGraphNodes)  // обозначение узлов
        {
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(2, 0, 2), Color.green, Time.fixedDeltaTime, true);
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(-2, 0, 2), Color.green, Time.fixedDeltaTime, true);
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(2, 0, -2), Color.green, Time.fixedDeltaTime, true);
            Debug.DrawLine(node.nodePosition, node.nodePosition + new Vector3Int(-2, 0, -2), Color.green, Time.fixedDeltaTime, true);
        }

        RoadGraphNode RGN = roadGraphNodes[roadGraphNodes.Count - 1];

        Debug.DrawLine(RGN.nodePosition, RGN.nodePosition + new Vector3Int(2, 0, 2), Color.red, Time.fixedDeltaTime, true);
        Debug.DrawLine(RGN.nodePosition, RGN.nodePosition + new Vector3Int(-2, 0, 2), Color.red, Time.fixedDeltaTime, true);
        Debug.DrawLine(RGN.nodePosition, RGN.nodePosition + new Vector3Int(2, 0, -2), Color.red, Time.fixedDeltaTime, true);
        Debug.DrawLine(RGN.nodePosition, RGN.nodePosition + new Vector3Int(-2, 0, -2), Color.red, Time.fixedDeltaTime, true);

        // обозрачение границ карты

        Debug.DrawLine(new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)),
            new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)), Color.white, Time.fixedDeltaTime, true);

        Debug.DrawLine(new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)),
            new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)), Color.white, Time.fixedDeltaTime, true);

        Debug.DrawLine(new Vector3((plane.localScale.x / 2 + plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)),
            new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)), Color.white, Time.fixedDeltaTime, true);

        Debug.DrawLine(new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, (plane.localScale.z / 2 + plane.position.z)),
            new Vector3(-(plane.localScale.x / 2 - plane.position.x), 0f, -(plane.localScale.z / 2 - plane.position.z)), Color.white, Time.fixedDeltaTime, true);
    }
}
