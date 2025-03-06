using System.Collections.Generic;
using UnityEngine;




public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5.0f;              // �÷��̾� �̵� �ӵ�
    public float mouseSensitivity = 100.0f;     // ���콺 ����
    public float thirdPersonDistance = 3.0f;    // 3��Ī ��忡�� �÷��̾�� ī�޶��� �Ÿ�
    public float zoomDistance = 1.0f;           // 3��Ī ��忡�� ī�޶� ���� �� �Ÿ�
    public float zoomSpeed = 5.0f;              // ī�޶� ��/������ �ӵ�
    public float defaultFov = 60.0f;            // �⺻ ī�޶� �þ߰�
    public float zoomFov = 30.0f;               // Ȯ�� �� ī�޶� �þ߰�(1��Ī ���)

    public Transform cameraTransform;           // ī�޶� transform
    public Transform playerHead;                // �÷��̾� �Ӹ� ��ġ(1��Ī ���)
    public Transform playerLookObj;             // �÷��̾� �þ� ��ġ

    public CharacterController characterController;

    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0); // 3��Ī ī�޶� ������

    private float currentDistance;              // ���� ī�޶���� �Ÿ�(3��Ī)
    private float targetDistance;               // ��ǥ ī�޶� �Ÿ�
    private float targetFov;                    // ��ǥ FOV
    private float pitch = 0.0f;                 // ���Ʒ� ȸ�� ��
    private float yaw = 0.0f;                   // �¿� ȸ�� ��

    private bool isZoomed = false;              // Ȯ�� ���� Ȯ��
    private bool isFirstPerson = false;         // 1��Ī ��� ����
    private bool isRotateAroundPlayer = true;     // ī�޶� �÷��̾� �ֺ� ȸ�� ����
    
    private Coroutine zoomCoroutine;            // �ڷ�ƾ�� ����Ͽ� Ȯ�� ��� ó��
    private Camera mainCamera;                  // ī�޶� ������Ʈ


    // �߷� ���� ����
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;

    private Vector3 velocity;
    private bool isGround;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;

    }

    private void Update()
    {
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (isRotateAroundPlayer)
        {
            // ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            // ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            // ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            // �÷��̾ ���� ȸ���ϴ� ���
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
    }
}