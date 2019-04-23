using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelMover : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
		transform.Rotate(2 * Vector3.right);
    }
}
