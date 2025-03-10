using System.Collections;
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
        // ���콺 �Է��� �޾� ī�޶�� �÷��̾� ȸ�� ó��
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
            Debug.Log(isFirstPerson ? "1��Ī" : "3��Ī");
        }

        if (!isFirstPerson && Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer = !isRotateAroundPlayer;
            Debug.Log(isRotateAroundPlayer ? "ī�޶� ������ ȸ��" : "�÷��̾ ���� ȸ��");
        }

        // ������ ���� ������
        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }


        // ��Ŭ�� ������ �ִ� ����
        if (Input.GetMouseButtonDown(1))
        {
            isAim = true;
            animator.SetBool("isAim", isAim);
            // ���� �������� �ڷ�ƾ�� �����ϸ�(����/�ܾƿ��� �������̶��) �������� �ڷ�ƾ�� ����
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            // 1��Ī �����̶��
            if (isFirstPerson)
            {
                // targetFOV�� ���� ��
                SetTargetFOV(zoomFov);
                // FOV�� �ε巴�� �����ϰ� �ش� �ڷ�ƾ�� zoomCoroutine�� �Ҵ�
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            // 3��Ī �����̶��
            else
            {
                // targetDistance�� ���� ��
                SetTargetDistance(zoomDistance);
                // distance�� �ε巴�� �����ϰ� �ش� �ڷ�ƾ�� zoomCoroutine�� �Ҵ�
                zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        // ��Ŭ�� ���� �ϸ�
        if (Input.GetMouseButtonUp(1))
        {
            isAim = false;
            animator.SetBool("isAim", isAim);
            
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            // 1��Ī �����̶��
            if (isFirstPerson)
            {
                // targetFOV�� ���� ��
                SetTargetFOV(defaultFov);
                // FOV�� �ε巴�� �����ϰ� �ش� �ڷ�ƾ�� zoomCoroutine�� �Ҵ�
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
            }
            // 3��Ī �����̶��
            else
            {
                // targetDistance�� ���� ��
                SetTargetDistance(thirdPersonDistance);
                // distance�� �ε巴�� �����ϰ� �ش� �ڷ�ƾ�� zoomCoroutine�� �Ҵ�
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
        
        // ���� ī�޶��� ȸ������ �������� ������
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // ī�޶��� ��ġ�� 1��Ī ��ġ�� �̵�
        cameraTransform.position = playerHead.position;
        // ī�޶��� ȸ������ ���콺�� �̵��� ���� ����
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // ī�޶��� yaw���� ���� �÷��̾ ȸ��
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
        // ���� �Ÿ����� ��ǥ �Ÿ��� �ε巴�� �̵�
        while(Mathf.Abs(currentDistance - targetDistance) > 0.01f)
        {
            // ���� ������ �̿��Ͽ� Time.deltaTime*zoomSpeed ������ ����ؼ� �̵�
            // currentDistance�� �ش� ���� ����ؼ� �����ϹǷ� T�� �״�ο��� ��� x
            // �������� �̵��Ѵٰ� ���� ��
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        // ��ǥ �Ÿ��� ������ �� ���� ����
        currentDistance = targetDistance;
    }

    IEnumerator ZoomFieldOfView(float targetFov)
    {
        // ���� Fov���� ��ǥ Fov�� �ε巴�� ����
        while(Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        // ��ǥ�� ������ �� ���� ����
        mainCamera.fieldOfView = targetFov;
    }
}