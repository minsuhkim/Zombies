using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;




public class PlayerManager : MonoBehaviour
{
    [Header("Basic")]
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


    //// �߷� ���� ����
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

    // pick up �ִϸ��̼ǿ��� ������ �������� �ݴ� �̺�Ʈ
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

    // ������
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

    // ���콺 �Է��� �޾� ī�޶�� �÷��̾� ȸ�� ó��
    public void RotateCharacter()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45, 45);
    }

    // ĳ���Ͱ� Ground�� ���� �� ����
    public void CheckGround()
    {
        isGround = characterController.isGrounded;
        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    // V, FŰ�� ���� ���� ���� (1��Ī, 3��Ī)
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

    // ������ ���� ������
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

    // lft shft Ű�� ���� �޸� �� �� ���� ����
    public void MoveRun()
    {
        isRunnning = Input.GetKey(KeyCode.LeftShift);
        moveSpeed = isRunnning ? runSpeed : walkSpeed;
        animator.SetBool("isRun", isRunnning);
    }

    // ���콺 ��Ŭ���� �ϸ� �� ��
    public void AimOn()
    {
        // ��Ŭ�� ������ �ִ� ����
        if (Input.GetMouseButtonDown(1) && isGetGunItem[curGunIndex] && isUseWeapon && !isReloading)
        {
            isAim = true;
            crossHairImage.SetActive(true);
            multiAimConstraint.data.offset = new Vector3(-30, 0, 0);
            animator.SetLayerWeight(1, 1);
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
    }

    // ���콺 ��Ŭ���� ���� �� �ƿ�
    public void AimOff()
    {
        // ��Ŭ�� ���� �ϸ�
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
    }

    // ���� Ʈ���� ���� �ִϸ��̼� ��� ���� �Ķ���� �� ������Ʈ
    public void UpdateAnimation()
    {
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
    }

    // �Ѿ� �߻�
    public void Fire()
    {
        // ���콺 ���� ��ư�� ������ ������ �߻�
        if (Input.GetMouseButton(0))
        {
            // ������ �ϰ� ���� ��/ �߻縦 �ϰ� ���� ���� ��/ ���� �Ѿ� ���� 0���� ���� ����
            if (isAim && !isFire && curBulletCount > 0)
            {
                // �߻� ������ üũ
                isFire = true;
                animator.SetTrigger("Fire");
                // �߻� ������ ��ŭ �߻� ���ϰ� ������ �ο�
                StartCoroutine(FireOff());

                // ���� ī�޶󿡼� forward �������� ray �߻�
                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                // targetLayerMask�� �ش��ϴ� layer�� hits�� ����
                RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, targetLayerMask);


                //foreach (var hit in hits)
                //{
                //    Debug.Log($"{hit.collider.name}");
                //}

                // hits�� ����� �� ���� ��
                if (hits.Length > 0)
                {
                    // ������ hp ���� �� �߻� �˵� draw(red)
                    foreach (var hit in hits)
                    {
                        ZombieManager zombie = hit.collider.GetComponent<ZombieManager>();
                        Debug.Log($"�浹: {hit.collider.name}, {zombie.HP}");
                        zombie.ChangeState(EZombieState.Damage, currentDamage);
                        Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                    }
                }
                // �߻� �˵� draw(green)
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

    // ���� ���� �� ����
    public void ChangeWeapon()
    {
        // 1�� Ű�� ������ ��/ ���� ������ ���� ����
        if (Input.GetKeyDown(KeyCode.Alpha1) && isGetGunItem[0])
        {
            gunObj[prevGunIndex].SetActive(false);
            getItemImages[prevGunIndex].SetActive(false);

            // �տ� �̸� ������ ���� �� Ȱ��ȭ
            gunObj[0].SetActive(true);
            getItemImages[0].SetActive(true);

            prevGunIndex = 0;

            SetGunData(0);

            animator.SetTrigger("onChangeWeapon");
            // ������ �� �� �ֵ��� üũ
            isUseWeapon = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && isGetGunItem[1])
        {
            gunObj[prevGunIndex].SetActive(false);
            getItemImages[prevGunIndex].SetActive(false);

            // �տ� �̸� ������ ���� �� Ȱ��ȭ
            gunObj[1].SetActive(true);
            getItemImages[1].SetActive(true);

            prevGunIndex = 1;

            SetGunData(1);

            animator.SetTrigger("onChangeWeapon");
            // ������ �� �� �ֵ��� üũ
            isUseWeapon = true;
        }
    }

    // 1��Ī �϶��� ������
    void FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

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

    // 3��Ī �϶��� ������
    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }

    // ī�޶� �÷��̾� �ü��� ���� �� / �÷��̾� �ֺ��� ȸ���� �� ����
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

    // 3��Ī �������� ������ �� ī�޶� �� ��
    IEnumerator ZoomCamera(float targetDistance)
    {
        // ���� �Ÿ����� ��ǥ �Ÿ��� �ε巴�� �̵�
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f)
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

    // 1��Ī �������� ������ �� �� ��
    IEnumerator ZoomFieldOfView(float targetFov)
    {
        // ���� Fov���� ��ǥ Fov�� �ε巴�� ����
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        // ��ǥ�� ������ �� ���� ����
        mainCamera.fieldOfView = targetFov;
    }

    // �Ѿ� �߻翡 �����̸� �ִ� �Լ�
    IEnumerator FireOff()
    {
        yield return new WaitForSeconds(fireDelay);
        isFire = false;
    }

    // ���� ���� �� ���� �ִϸ��̼ǿ��� audio play �̺�Ʈ
    public void OnChangeWeaponAudio()
    {
        audioSource.PlayOneShot(audioClipWeaponChange);
    }

    // �Ѿ� �߻� �ִϸ��̼ǿ��� audio �� �߻� ��ƼŬ play �̺�Ʈ
    public void OnFireAudio()
    {
        // �Ѿ� �� ����
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
        //    animator.SetTrigger("Hit"); // ������� Damage
        //}
    }
}