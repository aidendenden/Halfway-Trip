
using UnityEngine;

public class ApplyForce : MonoBehaviour
{
    public float force = 10f; // 定义力的大小
    public bool isup = true;

    private Rigidbody rb;
    private Transform ts;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ts = GetComponent<Transform>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)&&isup)
        {
            rb.AddForce(ts.up * force, ForceMode.Impulse); // 在按下空格键时给物体施加向上的力
        }
        if (Input.GetKeyDown(KeyCode.Space) && isup==false)
        {
            rb.AddForce(-ts.up * force, ForceMode.Impulse); // 在按下空格键时给物体施加向上的力
        }
    }
}
