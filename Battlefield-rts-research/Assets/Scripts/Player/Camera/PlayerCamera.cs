using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float m_CameraSpeed = 10;
    [SerializeField] private float m_MinSize = 2f;
    [SerializeField] private float m_MaxSize = 20f;

    [Header("Reference")]
    [SerializeField] private Camera m_Camera;

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float scrollValue = context.ReadValue<Vector2>().y;
            m_Camera.orthographicSize -= scrollValue;
            m_Camera.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, m_MinSize, m_MaxSize);
        }
    }

    private void Start()
    {
        if(!m_Camera)
            m_Camera = Camera.main;
    }

    private void LateUpdate()
    {
        MouseMovement();
    }

    private void MouseMovement()
    {
        Vector2 mousePosition = Input.mousePosition;

        Vector2 dir = Vector2.zero;
        if(mousePosition.x <= 0) dir.x = -1f;
        else if (mousePosition.x >= Screen.width) dir.x = 1f;

        if (mousePosition.y <= 0) dir.y = -1f;
        else if (mousePosition.y >= Screen.height) dir.y = 1f;

        if(dir != Vector2.zero)
        {
            MoveCamera(dir);
        }
    }

    private void MoveCamera(Vector2 dir)
    {
        var halfMapSize = GameManager.Instance.MapSize * 0.5f;


        var futurePos = m_Camera.transform.position + new Vector3(dir.x, dir.y, 0f) * m_CameraSpeed * Time.deltaTime;
        if(futurePos.x < -halfMapSize.x || futurePos.x > halfMapSize.x ||
           futurePos.y < -halfMapSize.y || futurePos.y > halfMapSize.y)
        {
            return; // Prevent camera from moving out of bounds
        }
        m_Camera.transform.position += new Vector3(dir.x, dir.y, 0f) * m_CameraSpeed * Time.deltaTime;


    }

    
}
