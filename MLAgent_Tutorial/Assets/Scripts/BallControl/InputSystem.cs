using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public string UDAxisName = "Vertical";
    public string LRAxisName = "Horizontal";

    public float UpDown { get; private set; }
    public float LeftRight { get; private set; }

    private void Update()
    {
        UpDown = Input.GetAxis(UDAxisName);
        LeftRight = Input.GetAxis(LRAxisName);
    }
}