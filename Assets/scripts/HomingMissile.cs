using UnityEngine;


public class HomingMissile : MonoBehaviour
{
    public float speed = 10f;
    public float turnRate = 2.5f;      
    public float lifetime = 5f;

  
    public float hitDistance = 1f;    

    private PlayerController target;
    private float age;

    public void Init(PlayerController player)
    {
        target = player;
        age = 0f;

       
        Vector3 toTarget = target.transform.position - transform.position;
        if (toTarget.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toTarget);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 toTarget = target.transform.position - transform.position;

       
        if (toTarget.sqrMagnitude <= hitDistance * hitDistance)
        {
            target.TakeHit();
            Destroy(gameObject);
            return;
        }

        
        Quaternion desired = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired, turnRate * Time.deltaTime);

        
        transform.position += transform.rotation * Vector3.forward * speed * Time.deltaTime;
    }
}
