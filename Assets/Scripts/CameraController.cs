using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotateSpeed = 60f;

    private void Update()
    {
        // WASD 이동 (카메라 로컬 방향 기준)
        float h = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        float v = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);

        Vector3 move = (transform.right * h + transform.forward * v) * (_moveSpeed * Time.deltaTime);
        move.y = 0f; // 수평 이동만
        transform.position += move;

        // 스페이스 / 컨트롤 고도
        float altitude = (Input.GetKey(KeyCode.Space) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftControl) ? 1f : 0f);
        transform.position += Vector3.up * (altitude * _moveSpeed * Time.deltaTime);

        // QE 좌우 회전 (Y축)
        float yaw = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
        transform.Rotate(Vector3.up, yaw * _rotateSpeed * Time.deltaTime, Space.World);

        // RF 상하 회전 (X축)
        float pitch = (Input.GetKey(KeyCode.F) ? 1f : 0f) - (Input.GetKey(KeyCode.R) ? 1f : 0f);
        transform.Rotate(Vector3.right, pitch * _rotateSpeed * Time.deltaTime, Space.Self);
    }
}
