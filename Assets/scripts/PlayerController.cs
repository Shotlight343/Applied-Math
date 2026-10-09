using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
   
    public float forwardSpeed = 8f;
    public float turnSpeed = 60f;         
    public float maxBankAngle = 25f;       
    public float bankSmoothing = 6f;

   
    public int maxHits = 5;

    public int HitCount { get; private set; }

    private float yaw;                     
    private float currentBank;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        float input = Input.GetAxisRaw("Horizontal"); 

       
        yaw += input * turnSpeed * Time.deltaTime;

       
        float targetBank = -input * maxBankAngle;
        currentBank = Mathf.Lerp(currentBank, targetBank, bankSmoothing * Time.deltaTime);

        
        Quaternion yawRot = Quaternion.AngleAxis(yaw, Vector3.up);
        Quaternion bankRot = Quaternion.AngleAxis(currentBank, Vector3.forward);
        transform.rotation = yawRot * bankRot;

     
        Vector3 heading = yawRot * Vector3.forward;
        transform.position += heading * forwardSpeed * Time.deltaTime;
    }

    
    public void TakeHit()
    {
        HitCount++;
        Debug.Log($"Player hit! {HitCount}/{maxHits}");

        if (HitCount >= maxHits)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
