
using UnityEngine;

public class ApplyForce : MonoBehaviour
{
    public float force = 10f; // �������Ĵ�С
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
            rb.AddForce(ts.up * force, ForceMode.Impulse); // �ڰ��¿ո��ʱ������ʩ�����ϵ���
        }
        if (Input.GetKeyDown(KeyCode.Space) && isup==false)
        {
            rb.AddForce(-ts.up * force, ForceMode.Impulse); // �ڰ��¿ո��ʱ������ʩ�����ϵ���
        }
    }
}
