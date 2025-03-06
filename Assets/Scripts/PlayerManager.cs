using System.Collections.Generic;
using UnityEngine;




public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5.0f;              // 플레이어 이동 속도
    public float mouseSensitivity = 100.0f;     // 마우스 감도
    public float thirdPersonDistance = 3.0f;    // 3인칭 모드에서 플레이어와 카메라의 거리
    public float zoomDistance = 1.0f;           // 3인칭 모드에서 카메라 줌일 때 거리
    public float zoomSpeed = 5.0f;              // 카메라 줌/줌해제 속도
    public float defaultFov = 60.0f;            // 기본 카메라 시야각
    public float zoomFov = 30.0f;               // 확대 시 카메라 시야각(1인칭 모드)

    public Transform cameraTransform;           // 카메라 transform
    public Transform playerHead;                // 플레이어 머리 위치(1인칭 모드)
    public Transform playerLookObj;             // 플레이어 시야 위치

    public CharacterController characterController;

    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0); // 3인칭 카메라 오프셋

    private float currentDistance;              // 현재 카메라와의 거리(3인칭)
    private float targetDistance;               // 목표 카메라 거리
    private float targetFov;                    // 목표 FOV
    private float pitch = 0.0f;                 // 위아래 회전 값
    private float yaw = 0.0f;                   // 좌우 회전 값

    private bool isZoomed = false;              // 확대 여부 확인
    private bool isFirstPerson = false;         // 1인칭 모드 여부
    private bool isRotateAroundPlayer = true;     // 카메라가 플레이어 주변 회전 여부

    private Coroutine zoomCoroutine;            // 코루틴을 사용하여 확대 축소 처리
    private Camera mainCamera;                  // 카메라 컴포넌트


    // 중력 관련 변수
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
        // 마우스 입력을 받아 카메라와 플레이어 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45, 45);

        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isFirstPerson ? "1인칭" : "3인칭");
        }

        if (!isFirstPerson && Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer = !isRotateAroundPlayer;
            Debug.Log(isRotateAroundPlayer ? "카메라가 주위를 회전" : "플레이어가 직접 회전");
        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }

    }

    void FirstPersonMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 현재 카메라의 회전값을 기준으로 움직임
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 카메라의 위치를 1인칭 위치로 이동
        cameraTransform.position = playerHead.position;
        // 카메라의 회전값을 마우스의 이동에 따라 변경
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // 카메라의 yaw값에 따라 플레이어도 회전
        transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0);
    }

    void ThirdPersonMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (isRotateAroundPlayer)
        {
            // 카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            // 카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            // 카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            // 플레이어가 직접 회전하는 모드
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
    }
}