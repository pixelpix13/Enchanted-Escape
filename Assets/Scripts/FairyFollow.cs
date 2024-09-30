using UnityEngine;

public class FairyFollow : MonoBehaviour
{
    public Transform fairyShoulderPoint;  // Reference to the shoulder point on PrincessSofie

    void Start()
    {
        if (fairyShoulderPoint == null)
        {
            // Automatically find the FairyShoulderPoint if not set in the Inspector
            GameObject princessObject = GameObject.Find("PrincessSofie");
            if (princessObject != null)
            {
                // Try to find the shoulder point as a child of PrincessSofie
                Transform shoulderPoint = princessObject.transform.Find("FairyShoulderPoint");
                if (shoulderPoint != null)
                {
                    fairyShoulderPoint = shoulderPoint;
                }
                else
                {
                    Debug.LogError("FairyShoulderPoint not found! Make sure the point exists as a child of PrincessSofie.");
                }
            }
            else
            {
                Debug.LogError("PrincessSofie not found! Make sure the name matches exactly in the Hierarchy.");
            }
        }
    }

    void Update()
    {
        if (fairyShoulderPoint != null)
        {
            // Stick the fairy to the shoulder point
            transform.position = fairyShoulderPoint.position;
        }
    }
}
