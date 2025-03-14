using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;




public class PlayerManager : MonoBehaviour
{
    [Header("Basic")]
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


    //// 중력 관련 변수
    //public float gravity = -9.81f;
    //public float jumpHeight = 2.0f;

    [Header("Gravity")]
    private Vector3 velocity;
    private bool isGround;

    private Animator animator;
    private float horizontal;
    private float vertical;

    private bool isRunnning;
    private bool isAim = false;
    private bool isFire = false;

    [Header("Speed")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;

    [Header("Audio")]
    public AudioClip audioClipFire;
    public AudioClip audioClipWeaponChange;
    public AudioClip audioClipGetItem;
    private AudioSource audioSource;

    [Header("Gun Object")]
    public GameObject[] gunObj;

    private float mouseX;
    private float mouseY;

    private int animationSpeed = 1;
    private string currentAnimation = "Idle";

    [Header("Aim")]
    public Transform aimTarget;
    private float weaponMaxDistance = 100.0f;
    public LayerMask targetLayerMask;

    [Header("Animation Rig Control")]
    public MultiAimConstraint multiAimConstraint;

    [Header("PickUp")]
    public Vector3 boxSize = Vector3.one;
    public float castDistance = 5.0f;
    public LayerMask itemLayer;
    public Transform itemGetPos;

    [Header("Image")]
    public GameObject crossHairImage;
    public GameObject[] getItemImages;

    private bool[] isGetGunItem;
    private bool isUseWeapon = false;
    private bool isReloading = false;

    private int curBulletTotalCount = 0;
    private int curBulletCount = 0;

    [Header("Gun Data Info")]
    public ItemSO[] itemDatas;
    private Item curItem;
    [SerializeField]
    private GunType curGunType;
    private int prevGunIndex = 0;
    private int curGunIndex;
    public ParticleSystem shotEffect;
    private float currentDamage;

    private float fireDelay = 0.1f;

    [Header("GunInfoUI")]
    public Text gunNameText;
    public Text totalBulletText;
    public Text currentBulletText;
    public Text gunDamageText;

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
        crossHairImage.SetActive(false);

        isGetGunItem = new bool[5];
    }

    private void Update()
    {
        RotateCharacter();
        CheckGround();
        ChangePointOfView();
        MoveWalk();
        AimOn();
        AimOff();
        MoveRun();
        UpdateAnimation();
        Fire();
        ChangeWeapon();
        PickUp();
        Reload();
    }

    private void SetGunData(int index)
    {
        animator.SetFloat("Gun", index);
        fireDelay = itemDatas[index].fireDelay;
        weaponMaxDistance = itemDatas[index].maxWeaponDistance;
        curBulletTotalCount = itemDatas[index].bulletTotalCount;
        curBulletCount = itemDatas[index].bulletCurrentCount;
        currentDamage = itemDatas[index].damage;

        gunNameText.text = itemDatas[index].name;
        totalBulletText.text = $"Total: {curBulletTotalCount}";
        currentBulletText.text = $"Current: {curBulletCount}";
        gunDamageText.text = $"Damage: {itemDatas[index].damage}";

    }

    private void DebugBox(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[2], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[0], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[0], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);
        Debug.DrawRay(origin, direction * castDistance, Color.green, 3.0f);

    }

    private void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Gizmos.DrawRay(ray);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    public void PickUp()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (Input.GetKeyDown(KeyCode.E) && !stateInfo.IsName("PickUp") && !isReloading && !isFire)
        {
            animator.SetTrigger("PickUp");
        }
    }

    // pick up 애니메이션에서 실제로 아이템을 줍는 이벤트
    public void OnPickUp()
    {
        
        Vector3 origin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;
        DebugBox(origin, direction);
        RaycastHit[] hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);

        foreach (var hit in hits)
        {
            if (hit.collider.tag == "Item")
            {
                curItem = hit.collider.GetComponent<Item>();
                curGunType = curItem.gunType;
                curGunIndex = (int)curGunType;

                hit.collider.gameObject.SetActive(false);
                isGetGunItem[curGunIndex] = true;
                audioSource.PlayOneShot(audioClipGetItem);
            }
            Debug.Log($"Item : {hit.collider.name}");
            break;
        }
    }

    // 재장전
    private void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R) && isGetGunItem[curGunIndex] && isUseWeapon && curBulletTotalCount>0 && !isReloading && !isAim)
        {
            isReloading = true;
            curBulletCount = curBulletTotalCount;
            currentBulletText.text = $"Current: {curBulletCount}";
            animator.SetTrigger("Reload");
        }
    }

    public void ReloadOff()
    {
        isReloading = false;
    }

    // 마우스 입력을 받아 카메라와 플레이어 회전 처리
    public void RotateCharacter()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45, 45);
    }

    // 캐릭터가 Ground에 있을 때 조정
    public void CheckGround()
    {
        isGround = characterController.isGrounded;
        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    // V, F키를 통해 시점 변경 (1인칭, 3인칭)
    public void ChangePointOfView()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
        }

        if (!isFirstPerson && Input.GetKeyDown(KeyCode.F))
        {
            isRotateAroundPlayer = !isRotateAroundPlayer;
        }
    }

    // 시점에 따른 움직임
    public void MoveWalk()
    {
        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
        }
    }

    // lft shft 키를 통해 달릴 지 말 지를 결정
    public void MoveRun()
    {
        isRunnning = Input.GetKey(KeyCode.LeftShift);
        moveSpeed = isRunnning ? runSpeed : walkSpeed;
        animator.SetBool("isRun", isRunnning);
    }

    // 마우스 우클릭을 하면 줌 인
    public void AimOn()
    {
        // 우클릭 누르고 있는 동안
        if (Input.GetMouseButtonDown(1) && isGetGunItem[curGunIndex] && isUseWeapon && !isReloading)
        {
            isAim = true;
            crossHairImage.SetActive(true);
            multiAimConstraint.data.offset = new Vector3(-30, 0, 0);
            animator.SetLayerWeight(1, 1);
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
    }

    // 마우스 우클릭을 떼면 줌 아웃
    public void AimOff()
    {
        // 우클릭 해제 하면
        if (Input.GetMouseButtonUp(1) && isGetGunItem[curGunIndex] && isUseWeapon && !isReloading)
        {
            isAim = false;
            crossHairImage.SetActive(false);
            multiAimConstraint.data.offset = new Vector3(0, 0, 0);
            animator.SetLayerWeight(1, 0);
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
    }

    // 블렌드 트리를 통한 애니메이션 제어를 위한 파라미터 값 업데이트
    public void UpdateAnimation()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
    }

    // 총알 발사
    public void Fire()
    {
        // 마우스 왼쪽 버튼을 누르고 있으면 발사
        if (Input.GetMouseButton(0))
        {
            // 조준을 하고 있을 때/ 발사를 하고 있지 않을 때/ 현재 총알 수가 0보다 많을 때만
            if (isAim && !isFire && curBulletCount > 0)
            {
                // 발사 중으로 체크
                isFire = true;
                animator.SetTrigger("Fire");
                // 발사 딜레이 만큼 발사 못하게 딜레이 부여
                StartCoroutine(FireOff());

                // 메인 카메라에서 forward 방향으로 ray 발사
                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                // targetLayerMask에 해당하는 layer만 hits에 저장
                RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, targetLayerMask);


                //foreach (var hit in hits)
                //{
                //    Debug.Log($"{hit.collider.name}");
                //}

                // hits에 저장된 게 있을 때
                if (hits.Length > 0)
                {
                    // 좀비의 hp 감소 및 발사 궤도 draw(red)
                    foreach (var hit in hits)
                    {
                        ZombieManager zombie = hit.collider.GetComponent<ZombieManager>();
                        Debug.Log($"충돌: {hit.collider.name}, {zombie.HP}");
                        zombie.ChangeState(EZombieState.Damage, currentDamage);
                        Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                    }
                }
                // 발사 궤도 draw(green)
                else
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
                }
            }
        }
        //else
        //{
        //    isFire = false;
        //}
    }

    // 무기 장착 및 변경
    public void ChangeWeapon()
    {
        // 1번 키를 눌렀을 때/ 총을 가지고 있을 때만
        if (Input.GetKeyDown(KeyCode.Alpha1) && isGetGunItem[0])
        {
            gunObj[prevGunIndex].SetActive(false);
            getItemImages[prevGunIndex].SetActive(false);

            // 손에 미리 장착해 놓은 총 활성화
            gunObj[0].SetActive(true);
            getItemImages[0].SetActive(true);

            prevGunIndex = 0;

            SetGunData(0);

            animator.SetTrigger("onChangeWeapon");
            // 조준을 할 수 있도록 체크
            isUseWeapon = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && isGetGunItem[1])
        {
            gunObj[prevGunIndex].SetActive(false);
            getItemImages[prevGunIndex].SetActive(false);

            // 손에 미리 장착해 놓은 총 활성화
            gunObj[1].SetActive(true);
            getItemImages[1].SetActive(true);

            prevGunIndex = 1;

            SetGunData(1);

            animator.SetTrigger("onChangeWeapon");
            // 조준을 할 수 있도록 체크
            isUseWeapon = true;
        }
    }

    // 1인칭 일때의 움직임
    void FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

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

    // 3인칭 일때의 움직임
    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }

    // 카메라가 플레이어 시선을 따라갈 지 / 플레이어 주변을 회전할 지 결정
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

            UpdateAimTarget();
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

    // 3인칭 시점에서 조준할 때 카메라 줌 인
    IEnumerator ZoomCamera(float targetDistance)
    {
        // 현재 거리에서 목표 거리로 부드럽게 이동
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f)
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

    // 1인칭 시점에서 조준할 때 줌 인
    IEnumerator ZoomFieldOfView(float targetFov)
    {
        // 현재 Fov에서 목표 Fov로 부드럽게 조정
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        // 목표에 도달한 후 값을 고정
        mainCamera.fieldOfView = targetFov;
    }

    // 총알 발사에 딜레이를 주는 함수
    IEnumerator FireOff()
    {
        yield return new WaitForSeconds(fireDelay);
        isFire = false;
    }

    // 무기 장착 및 변경 애니메이션에서 audio play 이벤트
    public void OnChangeWeaponAudio()
    {
        audioSource.PlayOneShot(audioClipWeaponChange);
    }

    // 총알 발사 애니메이션에서 audio 및 발사 파티클 play 이벤트
    public void OnFireAudio()
    {
        // 총알 수 감소
        curBulletCount--;
        currentBulletText.text = $"Current: {curBulletCount}";
        audioSource.PlayOneShot(audioClipFire);
        shotEffect.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.tag == "PlayerDamage")
        //{
        //    //animationSpeed = 3;
        //    characterController.enabled = false;
        //    transform.position = Vector3.zero;
        //    characterController.enabled = true;
        //    OnChangeWeaponAudio();
        //    animator.SetTrigger("Hit"); // 강사님은 Damage
        //}
    }
}