using UnityEngine;

public class PlacerContoller : MonoBehaviour
{   
    [SerializeField] private float moveSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(h, 0, v);
        if (direction.magnitude > 0.1f)
        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;
      
    }
}
