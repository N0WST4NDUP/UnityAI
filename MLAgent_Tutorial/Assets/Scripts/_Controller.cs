using UnityEngine;

public class _Controller : MonoBehaviour
{
    public float Speed = 10f;
    public Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(Speed * Vector3.left);

        if (Input.GetKey(KeyCode.D))
            rb.AddForce(Speed * Vector3.right);
    }
}
