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