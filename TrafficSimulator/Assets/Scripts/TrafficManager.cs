using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum carTypes
{
    NORMAL = 0,
    TAXI = 1,
    VEGAN = 2,
    AGGRESSIVE = 3
}

public class TrafficManager : MonoBehaviour
{
    public static TrafficManager Instance { get { return _instance; } }

    private static TrafficManager _instance = null;

    public GameObject[] carPrefabs;             // префаб машины
    public List<GameObject> cars;               // список созданных машин 

    [Range(6, 36)] public float TTL_timer;      // таймер работы светофоров

    private carTypes typeCar;
    private void Awake()
    {
        if (_instance)
        {
            DestroyImmediate(gameObject);
            return;
        }

        _instance = this;

        //InvokeRepeating("TTL", TTL_timer, TTL_timer);
    }

    public void DestroyAllCars()
    {
        if (cars.Count > 0)
        {
            foreach (GameObject car in cars)
            {
                Destroy(car);
            }

            cars.Clear();
        }
    }

    public void CreateNewCar(int index)
    {
        Vector3 offset = new Vector3(0, 4f, 0);

        RoadGraphNode randomPositionForCar;
        do
        {
            randomPositionForCar = RoadGenerator.Instance.roadGraphNodes[Random.Range(0, RoadGenerator.Instance.roadGraphNodes.Count)];
        }
        while (randomPositionForCar.neighbors.Count == 0);

        switch ((carTypes)index)
        {
            case carTypes.NORMAL:
                cars.Add(Instantiate(carPrefabs[(int)carTypes.NORMAL], 
                    randomPositionForCar.nodePosition + offset, Quaternion.identity));
                cars[cars.Count - 1].GetComponent<NormalCar>().CurrentNode = randomPositionForCar;
                break;
            case carTypes.TAXI:
                cars.Add(Instantiate(carPrefabs[(int)carTypes.TAXI], 
                    randomPositionForCar.nodePosition + offset, Quaternion.identity));
                cars[cars.Count - 1].GetComponent<TaxiCar>().CurrentNode = randomPositionForCar;
                break;
            case carTypes.VEGAN:
                cars.Add(Instantiate(carPrefabs[(int)carTypes.VEGAN], 
                    randomPositionForCar.nodePosition + offset, Quaternion.identity));
                cars[cars.Count - 1].GetComponent<VeganCar>().CurrentNode = randomPositionForCar;
                break;
            case carTypes.AGGRESSIVE:
                cars.Add(Instantiate(carPrefabs[(int)carTypes.AGGRESSIVE], 
                    randomPositionForCar.nodePosition + offset, Quaternion.identity));
                cars[cars.Count - 1].GetComponent<AggressiveCar>().CurrentNode = randomPositionForCar;
                break;
        }
        
    }

    private void TTL() // TimerTrafficLight
    {
        foreach (RoadGraphNode node in RoadGenerator.Instance.roadGraphNodes)
        {
            if (node.roadType.name == "TS-road-1" || node.roadType.name == "TS-road-1(Clone)")
            {
                node.isGreenTrafficLight = true;
            }
            else if (node.roadType.name == "2-path" || node.roadType.name == "2-path(Clone)")
            {
                node.isGreenTrafficLight = true;
            }
            else if (node.roadType.name == "3-path" || node.roadType.name == "3-path(Clone)")
            {
                node.isGreenTrafficLight = !node.isGreenTrafficLight;
                if (node.isGreenTrafficLight == true)
                {
                    node.roadType.transform.GetChild(6).transform.GetChild(0).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(6).transform.GetChild(1).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(7).transform.GetChild(0).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(7).transform.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    node.roadType.transform.GetChild(6).transform.GetChild(0).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(6).transform.GetChild(1).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(7).transform.GetChild(0).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(7).transform.GetChild(1).gameObject.SetActive(true);
                }
            }
            else if (node.roadType.name == "4-path" || node.roadType.name == "4-path(Clone)")
            {
                node.isGreenTrafficLight = !node.isGreenTrafficLight;
                if (node.isGreenTrafficLight == true)
                {
                    node.roadType.transform.GetChild(8).transform.GetChild(0).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(8).transform.GetChild(1).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(9).transform.GetChild(0).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(9).transform.GetChild(1).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(10).transform.GetChild(0).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(10).transform.GetChild(1).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(11).transform.GetChild(0).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(11).transform.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    node.roadType.transform.GetChild(8).transform.GetChild(0).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(8).transform.GetChild(1).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(9).transform.GetChild(0).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(9).transform.GetChild(1).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(10).transform.GetChild(0).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(10).transform.GetChild(1).gameObject.SetActive(true);
                    node.roadType.transform.GetChild(11).transform.GetChild(0).gameObject.SetActive(false);
                    node.roadType.transform.GetChild(11).transform.GetChild(1).gameObject.SetActive(true);
                }
            }
        }
    }

}
