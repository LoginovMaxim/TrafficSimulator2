using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCar : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<NormalCar>() != null)
        {
            StartCoroutine(SpeedIncrease(other.gameObject.GetComponent<NormalCar>().CarVelocity));
            return;
        }

        if (other.gameObject.GetComponent<TaxiCar>() != null)
        {
            StartCoroutine(SpeedIncrease(other.gameObject.GetComponent<TaxiCar>().CarVelocity));
            return;
        }

        if (other.gameObject.GetComponent<VeganCar>() != null)
        {
            StartCoroutine(SpeedIncrease(other.gameObject.GetComponent<VeganCar>().CarVelocity));
            return;
        }

        if (other.gameObject.GetComponent<AggressiveCar>() != null)
        {
            StartCoroutine(SpeedIncrease(other.gameObject.GetComponent<AggressiveCar>().CarVelocity));
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        gameObject.transform.parent.gameObject.GetComponent<AbstractCar>().isNearCar = false;
    }

    IEnumerator SpeedIncrease(float speed)
    {
        if (speed != 0)
            gameObject.transform.parent.gameObject.GetComponent<AbstractCar>().CarVelocity = speed - speed / 2f;
        else
            gameObject.transform.parent.gameObject.GetComponent<AbstractCar>().CarVelocity = 0;

        yield return new WaitForSeconds(2f);

        gameObject.transform.parent.gameObject.GetComponent<AbstractCar>().CarVelocity = 
            gameObject.transform.parent.gameObject.GetComponent<AbstractCar>().realVelocity;
    }
}
