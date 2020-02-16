using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class RoadGraphNode
{
    public int nodeIndex;               // Уникальный номер узла
    public Vector3Int nodePosition;     // Позиция в мировом пространстве

    public List<RoadGraphNode> neighbors;                                   // Список соседних узлов
    [HideInInspector] public List<Vector3> roadsToNeighbors;                // Список векторов до соседних узлов
    [HideInInspector] public List<GameObject> connectedRoadsToNeighbors;    // Список объектов соединяющих дорог

    [HideInInspector] public bool isActive;                   // Проверка: является ли улез активным? (правда - движение ничего не мешает, ложь - препятствие)
    [HideInInspector] public bool isGreenTrafficLight;        // Проверка: состояние светофора (правда - зелёный, ложь - красный)

    [HideInInspector] public GameObject roadType;             // Объект префаба дороги (от него удобно ссылаться на его детей (точки поворота / светофор))

    public RoadGraphNode(int nodeIndex, Vector3Int nodePosition)    // конструктор узла графа (инициализация объекта)
    {
        this.nodeIndex = nodeIndex;
        this.nodePosition = nodePosition;

        neighbors = new List<RoadGraphNode>();
        roadsToNeighbors = new List<Vector3>();
        connectedRoadsToNeighbors = new List<GameObject>();

        isActive = true;

        int randTrafficLightState = Random.Range(0, 2);

        //if(randTrafficLightState == 0)
        isGreenTrafficLight = true;
        //else
        //    isGreenTrafficLight = false;
    }

    public bool isSamePosition(Vector3 position)
    {
        return (nodePosition == position) ? true : false;
    }

    public void CreateAllVectorsToNeighbors(bool repeat)    // Создание множества направлений от узла до каждого соседнего узла
    {
        if (repeat == true)
            roadsToNeighbors.Clear();

        foreach (RoadGraphNode neighbor in neighbors)
        {
            roadsToNeighbors.Add(neighbor.nodePosition - nodePosition);
        }
    }

    public Vector3Int[] CreateConnectedRoads(RoadGraphNode neighbor)    // Заполнение прастранства между узлом и его соседями соеденительными дорогами
    {
        List<Vector3Int> connectionPos = new List<Vector3Int>();
        int steps;

        steps = Mathf.FloorToInt((neighbor.nodePosition - nodePosition).magnitude / RoadGenerator.Instance.scaleParameter);

        for (int step = 1; step < steps; step++)
        {
            Vector3 v3 = (neighbor.nodePosition - nodePosition);
            v3 = v3.normalized;
            connectionPos.Add(nodePosition + Vector3Int.FloorToInt(v3) * step * RoadGenerator.Instance.scaleParameter);
        }

        return connectionPos.ToArray();
    }
    public bool IsCrossing(Vector3Int A1, Vector3Int A2, Vector3Int B1, Vector3Int B2)  // Проверка на пересечение двух отрезков
    {
        A1.y = 0;
        A2.y = 0;
        B1.y = 0;
        B2.y = 0;

        float v1, v2, v3, v4;

        v1 = (B2.x - B1.x) * (A1.z - B1.z) - (B2.z - B1.z) * (A1.x - B1.x);
        v2 = (B2.x - B1.x) * (A2.z - B1.z) - (B2.z - B1.z) * (A2.x - B1.x);
        v3 = (A2.x - A1.x) * (B1.z - A1.z) - (A2.z - A1.z) * (B1.x - A1.x);
        v4 = (A2.x - A1.x) * (B2.z - A1.z) - (A2.z - A1.z) * (B2.x - A1.x);

        return (v1 * v2 < 0) && (v3 * v4 < 0);
    }

    public bool isStraightConnect(Vector3Int main, Vector3Int v1, Vector3Int v2)    // Проверка на отсутствие поворота узла (для выбора типа префаба дороги)
    {
        if (main.x == v1.x && main.z != v1.z && main.x == v2.x && main.z != v2.z) { return true; }
        if (main.x != v1.x && main.z == v1.z && main.x != v2.x && main.z == v2.z) { return true; }

        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return false; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return false; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return false; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return false; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return false; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return false; }

        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return false; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return false; }
        else { return false; }
    }
    public float SetAngleForDoubleRoadStraight(Vector3Int main, Vector3Int v1, Vector3Int v2)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
    {
        if (main.x == v1.x && main.z != v1.z && main.x == v2.x && main.z != v2.z) { return 0; }
        if (main.x != v1.x && main.z == v1.z && main.x != v2.x && main.z == v2.z) { return -90; }
        else { return 0; }
    }

    public float SetAngleForSingleRoad(Vector3Int main, Vector3Int v1)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
    {
        if (main.x == v1.x && main.z != v1.z) { return 0; }
        if (main.x != v1.x && main.z == v1.z) { return -90; }
        else { return 0; }
    }

    public float SetAngleForDoubleRoad(Vector3Int main, Vector3Int v1, Vector3Int v2)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
    {
        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return 0; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return 0; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z < v2.z) { return 90; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z < v1.z) { return 90; }

        if (main.x < v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return 180; }
        if (main.x < v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return 180; }

        if (main.x > v1.x && main.z == v1.z && main.x == v2.x && main.z > v2.z) { return 270; }
        if (main.x > v2.x && main.z == v2.z && main.x == v1.x && main.z > v1.z) { return 270; }
        else { return 0; }
    }

    public float SetAngleForTripleRoad(Vector3Int main, Vector3Int v1, Vector3Int v2, Vector3Int v3)  //Вызывается при создании префаба дорожного узла (возвращает значени угла поворота префаба)
    {
        if (main.x > v1.x && main.z == v1.z &&
            main.x == v2.x && main.z < v2.z &&
            main.x < v3.x && main.z == v3.z)
        { return 0; }

        if (main.x > v1.x && main.z == v1.z &&
            main.x == v3.x && main.z < v3.z &&
            main.x < v2.x && main.z == v2.z)
        { return 0; }

        if (main.x > v2.x && main.z == v2.z &&
            main.x == v1.x && main.z < v1.z &&
            main.x < v3.x && main.z == v3.z)
        { return 0; }

        if (main.x > v2.x && main.z == v2.z &&
            main.x == v3.x && main.z < v3.z &&
            main.x < v1.x && main.z == v1.z)
        { return 0; }

        if (main.x > v3.x && main.z == v3.z &&
            main.x == v1.x && main.z < v1.z &&
            main.x < v2.x && main.z == v2.z)
        { return 0; }

        if (main.x > v3.x && main.z == v3.z &&
            main.x == v2.x && main.z < v2.z &&
            main.x < v1.x && main.z == v1.z)
        { return 0; }


        if (main.x == v1.x && main.z < v1.z &&
            main.x < v2.x && main.z == v2.z &&
            main.x == v3.x && main.z > v3.z)
        { return 90; }

        if (main.x == v1.x && main.z < v1.z &&
            main.x < v3.x && main.z == v3.z &&
            main.x == v2.x && main.z > v2.z)
        { return 90; }

        if (main.x == v2.x && main.z < v2.z &&
            main.x < v1.x && main.z == v1.z &&
            main.x == v3.x && main.z > v3.z)
        { return 90; }

        if (main.x == v2.x && main.z < v2.z &&
            main.x < v3.x && main.z == v3.z &&
            main.x == v1.x && main.z > v1.z)
        { return 90; }

        if (main.x == v3.x && main.z < v3.z &&
            main.x < v1.x && main.z == v1.z &&
            main.x == v2.x && main.z > v2.z)
        { return 90; }

        if (main.x == v3.x && main.z < v3.z &&
            main.x < v2.x && main.z == v2.z &&
            main.x == v1.x && main.z > v1.z)
        { return 90; }


        if (main.x < v1.x && main.z == v1.z &&
            main.x == v2.x && main.z > v2.z &&
            main.x > v3.x && main.z == v3.z)
        { return 180; }

        if (main.x < v1.x && main.z == v1.z &&
            main.x == v3.x && main.z > v3.z &&
            main.x > v2.x && main.z == v2.z)
        { return 180; }

        if (main.x < v2.x && main.z == v2.z &&
            main.x == v1.x && main.z > v1.z &&
            main.x > v3.x && main.z == v3.z)
        { return 180; }

        if (main.x < v2.x && main.z == v2.z &&
            main.x == v3.x && main.z > v3.z &&
            main.x > v1.x && main.z == v1.z)
        { return 180; }

        if (main.x < v3.x && main.z == v3.z &&
            main.x == v1.x && main.z > v1.z &&
            main.x > v2.x && main.z == v2.z)
        { return 180; }

        if (main.x < v3.x && main.z == v3.z &&
            main.x == v2.x && main.z > v2.z &&
            main.x > v1.x && main.z == v1.z)
        { return 180; }


        if (main.x == v1.x && main.z > v1.z &&
            main.x > v2.x && main.z == v2.z &&
            main.x == v3.x && main.z < v3.z)
        { return 270; }

        if (main.x == v1.x && main.z > v1.z &&
            main.x > v3.x && main.z == v3.z &&
            main.x == v2.x && main.z < v2.z)
        { return 270; }

        if (main.x == v2.x && main.z > v2.z &&
            main.x > v1.x && main.z == v1.z &&
            main.x == v3.x && main.z < v3.z)
        { return 270; }

        if (main.x == v2.x && main.z > v2.z &&
            main.x > v3.x && main.z == v3.z &&
            main.x == v1.x && main.z < v1.z)
        { return 270; }

        if (main.x == v3.x && main.z > v3.z &&
            main.x > v1.x && main.z == v1.z &&
            main.x == v2.x && main.z < v2.z)
        { return 270; }

        if (main.x == v3.x && main.z > v3.z &&
            main.x > v2.x && main.z == v2.z &&
            main.x == v1.x && main.z < v1.z)
        { return 270; }
        else { return 0; }
    }
}
