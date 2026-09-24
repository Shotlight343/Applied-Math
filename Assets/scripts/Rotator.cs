using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
     if (target == null) return;

     var dir = target.position - this.transform.position;
     var angle = Mathf.Atan2(dir.x, dir.z)*Mathf.Rad2Deg;
     this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(0,angle,0),Time.deltaTime * rotationSpeed);
     Debug.DrawLine(this.transform.position, target.position, Color.red);
    }
}
