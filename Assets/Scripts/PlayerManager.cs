using System.Collections;
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

    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool isRunnning;
    private bool isAim = false;
    private bool isFire = false;

    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;

    public AudioClip audioClipFire;
    public AudioClip audioClipWeaponChange;
    private AudioSource audioSource;

    public GameObject rifleM4Obj;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFov = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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

        // 시점에 따른 움직임
        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }


        // 우클릭 누르고 있는 동안
        if (Input.GetMouseButtonDown(1))
        {
            isAim = true;
            animator.SetBool("isAim", isAim);
            // 현재 진행중인 코루틴이 존재하면(줌인/줌아웃이 진행중이라면) 진행중인 코루틴을 멈춤
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            // 1인칭 시점이라면
            if (isFirstPerson)
            {
                // targetFOV를 설정 후
                SetTargetFOV(zoomFov);
                // FOV를 부드럽게 조정하고 해당 코루틴을 zoomCoroutine에 할당
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            // 3인칭 시점이라면
            else
            {
                // targetDistance를 설정 후
                SetTargetDistance(zoomDistance);
                // distance를 부드럽게 조정하고 해당 코루틴을 zoomCoroutine에 할당
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        // 우클릭 해제 하면
        if (Input.GetMouseButtonUp(1))
        {
            isAim = false;
            animator.SetBool("isAim", isAim);
            
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            // 1인칭 시점이라면
            if (isFirstPerson)
            {
                // targetFOV를 설정 후
                SetTargetFOV(defaultFov);
                // FOV를 부드럽게 조정하고 해당 코루틴을 zoomCoroutine에 할당
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            // 3인칭 시점이라면
            else
            {
                // targetDistance를 설정 후
                SetTargetDistance(thirdPersonDistance);
                // distance를 부드럽게 조정하고 해당 코루틴을 zoomCoroutine에 할당
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        isRunnning = Input.GetKey(KeyCode.LeftShift);
        moveSpeed = isRunnning ? runSpeed : walkSpeed;
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("isRun", isRunnning);



        if (Input.GetMouseButton(0))
        {
            if (isAim)
            {
                isFire = true;
                animator.SetBool("isFire", isFire);
                audioSource.PlayOneShot(audioClipFire);
            }            
        }
        else
        {
            isFire = false;
            animator.SetBool("isFire", isFire);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.PlayOneShot(audioClipWeaponChange);
            rifleM4Obj.SetActive(true);
            animator.SetTrigger("onChangeWeapon");
        }
    }

    void FirstPersonMovement()
    {
        horizontal = isAim ? 0 : Input.GetAxis("Horizontal");
        vertical = isAim ? 0 : Input.GetAxis("Vertical");
        
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
        horizontal = isAim ? 0 : Input.GetAxis("Horizontal");
        vertical = isAim ? 0 : Input.GetAxis("Vertical");

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

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetFOV(float FOV)
    {
        targetFov = FOV;
    }

    IEnumerator ZoomCamera(float targetDistance)
    {
        // 현재 거리에서 목표 거리로 부드럽게 이동
        while(Mathf.Abs(currentDistance - targetDistance) > 0.01f)
        {
            // 선형 보간을 이용하여 Time.deltaTime*zoomSpeed 값으로 계속해서 이동
            // currentDistance에 해당 값을 계속해서 대입하므로 T는 그대로여도 상관 x
            // 시작점이 이동한다고 보면 됨
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        // 목표 거리에 도달한 후 값을 고정
        currentDistance = targetDistance;
    }

    IEnumerator ZoomFieldOfView(float targetFov)
    {
        // 현재 Fov에서 목표 Fov로 부드럽게 조정
        while(Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        // 목표에 도달한 후 값을 고정
        mainCamera.fieldOfView = targetFov;
    }
}