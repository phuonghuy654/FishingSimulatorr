using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(LineRenderer))]
public class FishingController : MonoBehaviour
{

    public enum FishingState
    {
        NotFishing,
        PowerCharge,
        WaitingForBite,
        Biting,
        MiniGame,
        Caught,
        Failed
    }

    [Header("State (Read Only)")]
    [SerializeField]
    private FishingState currentState = FishingState.NotFishing;

    [Header("System References (Drag In)")]
    public PlayerInventorySO playerInventory;
    public UIDocument inGameHUD;

    [Header("Casting Visuals")]
    [Tooltip("Drag the Bobber prefab here")]
    public GameObject bobberPrefab;
    [Tooltip("Minimum cast distance (at 0 power)")]
    public float minCastDistance = 2f;
    [Tooltip("Maximum cast distance (at 100 power)")]
    public float maxCastDistance = 10f;
    [Tooltip("Height of the arc when casting the bobber")]
    public float castArcHeight = 2.5f;

    [Header("Fishing Line (NEW)")]
    [Tooltip("Prefab with just a LineRenderer component. This script will add the component if missing.")]
    public Material lineMaterial;
    [Tooltip("The 'start' of the line (usually the rod tip)")]
    public Transform lineStartPoint;
    [Tooltip("How many points to use for the line curve")]
    public int lineSegments = 20;
    [Tooltip("How much the line sags (in meters) when idle")]
    public float maxLineSag = 1f;
    [Tooltip("Width of the fishing line")]
    public float lineWidth = 0.02f;
    [Tooltip("How fast the line sags or tightens")]
    public float lineSagSpeed = 5f;
    [Tooltip("How taut the line is when pulling in the minigame")]
    public float minigameLineTaut = 0f;
    [Tooltip("How slack the line is when releasing in the minigame")]
    public float minigameLineSag = 0.5f;

    [Tooltip("How much the line 'bounces' when reeling in a catch")]
    public float reelInTension = 0.1f;
    [Tooltip("How fast the line 'bounces' when reeling in")]
    public float reelInTensionSpeed = 25f;


    private LineRenderer _lineRenderer;
    private Transform _lineTarget;
    private float _currentLineSag = 0f;
    private float _targetLineSag = 0f;

    private Animator animator;
    private Transform playerTransform;

    private AudioManager audioManager;

    private VisualElement castingContainer, waitingIndicator, biteIndicator;
    private VisualElement minigameContainer, minigameProgress, fishZone, playerTarget;
    private VisualElement castingBarFill;

    [Header("Fishing State Settings")]
    [Tooltip("Speed of the power bar when charging (at 0%)")]
    public float minCastingSpeed = 40f;
    [Tooltip("Speed of the power bar when NEARLY full (at 100%)")]
    public float maxCastingSpeed = 120f;
    public float minWaitTime = 3.0f;
    public float maxWaitTime = 8.0f;
    public float biteWindow = 1.2f;

    [Header("Power Bar Colors")]
    [Tooltip("Color of the power bar at 0%")]
    public Color lowPowerColor = Color.red;
    [Tooltip("Color of the power bar at 100%")]
    public Color fullPowerColor = Color.green;

    [Header("Animation Correction")]
    [Tooltip("Drag the child object (e.g., base model, character hip) that tilts upward here.")]
    public Transform characterModelRoot;
    [Tooltip("Angle (X) to 'pitch' the model down to straighten it. Example: (10, 0, 0)")]
    public Vector3 animationTiltCorrection = new Vector3(10f, 0, 0);

    [Header("Positions (Transforms)")]
    [Tooltip("Assign an Empty Object as the bobber spawn point (usually at the rod tip or hand).")]
    public Transform bobberSpawnPoint;
    [Tooltip("Assign an Empty Object at the player's feet as the fish landing point.")]
    public Transform fishLandPoint;

    private const float INITIAL_PROGRESS_PERCENT = 0.33f;
    private const float PLAYER_TARGET_SIZE = 2f;

    private float currentProgress;
    private float currentFishThreshold;
    private float fishZonePosition;
    private float playerTargetPosition;
    private float fishZoneSize;
    private float progressGainRate;
    private float progressLossRate;
    private float playerTargetDescendSpeed;
    private float playerTargetAscendSpeed;
    private float fishZoneMoveIntervalMin;
    private float fishZoneMoveIntervalMax;
    private float fishZoneMoveTimer;
    private float fishZoneTargetPosition;
    private bool canFish = false;
    private FishingSpot currentSpot;
    private FishData currentFishData;
    private float waitTimer;
    private float biteTimer;
    private float castingPower;
    private int castingDirection = 1;
    private Coroutine biteIndicatorCoroutine;
    private Coroutine waitingAnimationCoroutine;
    private GameObject currentBobberInstance;
    private Vector3 bobberDestination;
    private float _stateCooldown;
    private float _inputCooldownTimer = 0f;
    private const float _powerChargeMinTime = 0.15f;
    private float _powerChargeTimer = 0f;
    private int animIsWaitingIdle;
    private int animIsReeling;
    private int animIsFishing;
    private int animStartCasting;
    private int animCatchFish;

    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null) Debug.LogError("[FishingController] AudioManager.instance not found!");

        animator = GetComponent<Animator>();
        playerTransform = transform;

        animIsWaitingIdle = Animator.StringToHash("IsWaitingIdle");
        animIsReeling = Animator.StringToHash("IsReeling");
        animIsFishing = Animator.StringToHash("isFishing");
        animStartCasting = Animator.StringToHash("StartCasting");
        animCatchFish = Animator.StringToHash("CatchFish");

        if (inGameHUD == null || inGameHUD.rootVisualElement == null)
        {
            Debug.LogError("FishingController: InGameHUD reference missing!");
            return;
        }

        if (bobberPrefab == null) Debug.LogWarning("[FishingController] Bobber Prefab is not assigned!");
        if (bobberSpawnPoint == null) Debug.LogWarning("[FishingController] 'bobberSpawnPoint' not assigned!");
        if (fishLandPoint == null) Debug.LogWarning("[FishingController] 'fishLandPoint' not assigned!");


        if (lineStartPoint == null)
        {
            Debug.LogWarning("[FishingController] 'Line Start Point' is not assigned! Defaulting to 'bobberSpawnPoint'.");
            lineStartPoint = bobberSpawnPoint;
        }

        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        if (lineMaterial != null)
        {
            _lineRenderer.material = lineMaterial;
        }
        else
        {
            Debug.LogError("[FishingController] 'Line Material' is not assigned! Line will be pink.");
        }

        _lineRenderer.positionCount = lineSegments;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.enabled = false;



        var root = inGameHUD.rootVisualElement;
        castingContainer = root.Q("casting-container");
        waitingIndicator = root.Q("waiting-indicator");
        biteIndicator = root.Q("bite-indicator");
        castingBarFill = root.Q("casting-bar-fill");
        minigameContainer = root.Q("minigame-container");
        minigameProgress = root.Q("minigame-progress");
        fishZone = root.Q("fish-zone");
        playerTarget = root.Q("player-target");

        castingContainer?.AddToClassList("hidden");
        waitingIndicator?.AddToClassList("hidden");
        biteIndicator?.AddToClassList("hidden");
        minigameContainer?.RemoveFromClassList("is-active");

        ResetAllFishingAnimStates();
    }

    void Update()
    {
        _inputCooldownTimer -= Time.deltaTime;


        if (currentState != FishingState.NotFishing && _lineRenderer != null && _lineRenderer.enabled)
        {
            UpdateLineRenderer();
        }


        switch (currentState)
        {
            case FishingState.NotFishing:
                if (canFish && Input.GetMouseButtonDown(0) && _inputCooldownTimer <= 0f)
                {
                    ChangeState(FishingState.PowerCharge);
                }
                break;

            case FishingState.PowerCharge:
                HandlePowerCharge();
                _powerChargeTimer -= Time.deltaTime;
                if (Input.GetMouseButtonUp(0) && _powerChargeTimer <= 0f)
                {
                    RaycastHit castHit;
                    if (TryCalculateCastHit(out castHit))
                    {
                        if (castHit.collider.CompareTag("Water"))
                        {
                            bobberDestination = castHit.point;
                            ChangeState(FishingState.WaitingForBite);
                        }
                        else
                        {
                            Debug.Log("Cast failed: Target is not 'Water'.");
                            ChangeState(FishingState.NotFishing);
                        }
                    }
                    else
                    {
                        Debug.Log("Cast failed: Raycast hit nothing.");
                        ChangeState(FishingState.NotFishing);
                    }
                }
                break;


            case FishingState.WaitingForBite:
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    ChangeState(FishingState.Biting);
                }
                break;

            case FishingState.Biting:
                biteTimer -= Time.deltaTime;
                if (Input.GetMouseButtonDown(0))
                {
                    ChangeState(FishingState.MiniGame);
                }
                if (biteTimer <= 0)
                {
                    ChangeState(FishingState.Failed);
                }
                break;

            case FishingState.MiniGame:
                HandlePlayerTargetMovement();
                HandleFishZoneMovement();
                HandleProgress();
                UpdateMinigameUI();
                break;

            case FishingState.Caught:

                break;

            case FishingState.Failed:
                _stateCooldown -= Time.deltaTime;
                if (_stateCooldown <= 0)
                {
                    _inputCooldownTimer = 0.5f;
                    ChangeState(FishingState.NotFishing);
                }
                break;
        }
    }


    private void ChangeState(FishingState newState)
    {
        if (currentState == newState) return;
        Debug.Log($"[FishingController] Changing State from {currentState} to {newState}");
        OnStateExit(currentState);
        currentState = newState;
        OnStateEnter(newState);
    }




    private void OnStateEnter(FishingState state)
    {
        if (state != FishingState.NotFishing)
        {
            if (animator != null) animator.SetBool(animIsFishing, true);
        }

        switch (state)
        {
            case FishingState.NotFishing:
                ResetAllFishingAnimStates();
                DestroyBobber();
                if (animator != null) animator.SetBool(animIsFishing, false);
                currentFishData = null;
                if (_lineRenderer != null) _lineRenderer.enabled = false;
                _lineTarget = null;
                _targetLineSag = 0f;
                _currentLineSag = 0f;
                break;

            case FishingState.PowerCharge:
                castingPower = 0f;
                castingDirection = 1;
                castingBarFill.style.width = new StyleLength(new Length(0, LengthUnit.Percent));
                castingContainer.RemoveFromClassList("hidden");
                audioManager?.PlaySFX("Click");
                _powerChargeTimer = _powerChargeMinTime;

                if (castingBarFill != null)
                {
                    castingBarFill.style.backgroundColor = lowPowerColor;
                }
                if (_lineRenderer != null) _lineRenderer.enabled = true;
                _lineTarget = null;
                _targetLineSag = 0f;
                break;

            case FishingState.WaitingForBite:
                currentFishData = GetRandomFishFromCurrentSpot();
                if (currentFishData == null) { ChangeState(FishingState.Failed); return; }
                playerInventory.CreateTemporaryFish(currentFishData);
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
                waitingIndicator.RemoveFromClassList("hidden");
                waitingAnimationCoroutine = StartCoroutine(WaitingIndicatorCoroutine());

                if (animator != null)
                {
                    animator.SetTrigger(animStartCasting);
                    animator.SetBool(animIsWaitingIdle, true);
                }
                _targetLineSag = maxLineSag;
                break;

            case FishingState.Biting:
                biteTimer = biteWindow;
                biteIndicator.RemoveFromClassList("hidden");
                biteIndicatorCoroutine = StartCoroutine(BiteIndicatorAnimationCoroutine());
                audioManager?.PlaySFX("DropWater");

                break;

            case FishingState.MiniGame:
                LoadFishDataForMinigame();
                currentProgress = INITIAL_PROGRESS_PERCENT * currentFishThreshold;
                playerTargetPosition = 50f - (PLAYER_TARGET_SIZE / 2f);
                fishZonePosition = Random.Range(0f, 100f - fishZoneSize);
                fishZoneTargetPosition = GetRandomZonePosition();
                fishZoneMoveTimer = Random.Range(fishZoneMoveIntervalMin, fishZoneMoveIntervalMax);
                minigameContainer.AddToClassList("is-active");
                if (animator != null) animator.SetBool(animIsReeling, true);
                audioManager?.PlayMusic("Retrieve");
                _targetLineSag = minigameLineSag;
                break;

            case FishingState.Caught:
                Debug.Log("Fish Caught!");
                audioManager?.PlaySFX("DropWater");
                if (animator != null)
                {
                    animator.SetTrigger(animCatchFish);
                    animator.SetBool(animIsWaitingIdle, false);
                    animator.SetBool(animIsReeling, false);
                }
                
                
                _targetLineSag = 0f;
                break;

            case FishingState.Failed:
                Debug.Log("Fish Got Away!");
                _stateCooldown = 0.3f;
                DestroyBobber();
                playerInventory.temporaryFish = null;
                ResetAllFishingAnimStates();
                if (_lineRenderer != null) _lineRenderer.enabled = false;
                _lineTarget = null;
                _targetLineSag = 0f;
                break;
        }
    }

    private void OnStateExit(FishingState state)
    {
        switch (state)
        {
            case FishingState.PowerCharge:
                castingContainer.AddToClassList("hidden");
                break;

            case FishingState.WaitingForBite:
                waitingIndicator.AddToClassList("hidden");
                if (waitingAnimationCoroutine != null) StopCoroutine(waitingAnimationCoroutine);
                if (currentState != FishingState.Biting && animator != null)
                {
                    animator.SetBool(animIsWaitingIdle, false);
                }
                break;

            case FishingState.Biting:
                biteIndicator.AddToClassList("hidden");
                if (biteIndicatorCoroutine != null) StopCoroutine(biteIndicatorCoroutine);
                if (animator != null)
                {
                    animator.SetBool(animIsWaitingIdle, false);
                }
                break;

            case FishingState.MiniGame:
                minigameContainer.RemoveFromClassList("is-active");
                audioManager?.PlayMusic("MainTheme");
                if (animator != null)
                {
                    animator.SetBool(animIsReeling, false);
                }
                break;

            case FishingState.Caught:
            case FishingState.Failed:
                _stateCooldown = 0f;
                break;
        }
    }



    
    
    
    private void UpdateLineRenderer()
    {
        if (lineStartPoint == null || _lineRenderer == null) return;

        Vector3 startPoint = lineStartPoint.position;
        Vector3 endPoint = (_lineTarget != null) ? _lineTarget.position : startPoint;


        _currentLineSag = Mathf.Lerp(_currentLineSag, _targetLineSag, Time.deltaTime * lineSagSpeed);

        
        float finalSagToShow = _currentLineSag;

        
        
        
        
        

        if (_lineRenderer.positionCount != lineSegments)
        {
            _lineRenderer.positionCount = lineSegments;
        }

        for (int i = 0; i < lineSegments; i++)
        {
            float t = (float)i / (float)(lineSegments - 1);
            Vector3 straightLinePos = Vector3.Lerp(startPoint, endPoint, t);


            float sag = Mathf.Sin(t * Mathf.PI) * finalSagToShow;

            Vector3 sagPos = straightLinePos + (Vector3.down * sag);

            _lineRenderer.SetPosition(i, sagPos);
        }
    }


    private bool TryCalculateCastHit(out RaycastHit hit)
    {
        float normalizedPower = castingPower / 100f;
        float distance = Mathf.Lerp(minCastDistance, maxCastDistance, normalizedPower);
        Vector3 destination = playerTransform.position + playerTransform.forward * distance;

        if (Physics.Raycast(destination + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            return true;
        }
        return false;
    }

    public void AnimationEvent_ThrowBobber()
    {
        if (currentState != FishingState.WaitingForBite) return;

        Debug.Log("[FishingController] Animation Event: ThrowBobber!");

        DestroyBobber();

        Vector3 startPos;
        if (bobberSpawnPoint != null)
        {
            startPos = bobberSpawnPoint.position;
        }
        else
        {
            startPos = playerTransform.position + playerTransform.forward * 0.5f + Vector3.up * 1f;
        }

        if (bobberPrefab != null)
        {
            currentBobberInstance = Instantiate(bobberPrefab, startPos, Quaternion.identity);
            _lineTarget = currentBobberInstance.transform;
        }
        else
        {
            return;
        }

        StartCoroutine(CastBobberRoutine(currentBobberInstance, bobberDestination, 0.7f));

        audioManager?.PlaySFX("CastRod");
    }

    private IEnumerator CastBobberRoutine(GameObject bobber, Vector3 destination, float duration)
    {
        Vector3 startPosition = bobber.transform.position;
        float timer = 0f;
        while (timer < duration)
        {
            float progress = timer / duration;
            Vector3 currentPos = Vector3.Lerp(startPosition, destination, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * castArcHeight;
            if (bobber != null)
            {
                bobber.transform.position = currentPos;
            }
            else
            {
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (bobber != null)
        {
            bobber.transform.position = destination;
            audioManager?.PlaySFX("DropWater");
        }
    }


    private IEnumerator ReelFishInRoutine(GameObject fish, Vector3 destination, float duration)
    {
        Vector3 startPosition = fish.transform.position;
        float timer = 0f;

        Vector3 lastPosition = startPosition;
        Vector3 lastVelocity = Vector3.zero;

        while (timer < duration)
        {
            if (fish == null)
            {
                _lineTarget = null;
                yield break;
            }

            float progress = timer / duration;
            Vector3 currentPos = Vector3.Lerp(startPosition, destination, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * castArcHeight;


            if (Time.deltaTime > 0f)
            {
                lastVelocity = (currentPos - lastPosition) / Time.deltaTime;
                lastPosition = currentPos;
            }
            else
            {
                lastVelocity = Vector3.zero;
            }

            fish.transform.position = currentPos;

            Vector3 lookDir = (destination - fish.transform.position).normalized;
            if (lookDir != Vector3.zero)
            {
                fish.transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.Euler(-90, 0, 0);
            }

            timer += Time.deltaTime;
            yield return null;
        }


        if (fish != null)
        {
            Collider fishCollider = fish.GetComponent<Collider>();
            if (fishCollider != null) fishCollider.enabled = true;

            Rigidbody fishRb = fish.GetComponent<Rigidbody>();
            if (fishRb != null)
            {
                fishRb.isKinematic = false;
                fishRb.velocity = lastVelocity;
            }

            _lineTarget = null;
        }
    }

    private void DestroyBobber()
    {
        if (currentBobberInstance != null)
        {
            Destroy(currentBobberInstance);
            currentBobberInstance = null;
        }
    }

    private void ResetAllFishingAnimStates()
    {
        if (animator == null) return;
        animator.SetBool(animIsWaitingIdle, false);
        animator.SetBool(animIsReeling, false);
    }

    private void HandlePowerCharge()
    {
        float normalizedPower = castingPower / 100f;
        float currentSpeed = Mathf.Lerp(minCastingSpeed, maxCastingSpeed, normalizedPower);

        castingPower += currentSpeed * castingDirection * Time.deltaTime;

        if (castingPower >= 100)
        {
            castingPower = 100;
            castingDirection = -1;
        }
        else if (castingPower <= 0)
        {
            castingPower = 0;
            castingDirection = 1;
        }

        castingBarFill.style.width = new StyleLength(new Length(castingPower, LengthUnit.Percent));

        if (castingBarFill != null)
        {
            float clampedNormalizedPower = castingPower / 100f;
            castingBarFill.style.backgroundColor = Color.Lerp(lowPowerColor, fullPowerColor, clampedNormalizedPower);
        }
    }

    
    
    
    public void AnimationEvent_SpawnAndReelFish()
    {
        if (playerInventory.temporaryFish == null)
        {
            DestroyBobber();
            _lineTarget = null;
            return;
        }
        FishInstance fishInstance = playerInventory.temporaryFish;
        GameObject fishPrefab = fishInstance.baseData.fishPrefab;
        if (fishPrefab == null)
        {
            Debug.LogError("FishData has no Prefab! Adding directly to inventory.");
            playerInventory.AddTemporaryFishToInventory();
            DestroyBobber();
            _lineTarget = null;
            return;
        }
        Vector3 spawnPos;
        if (currentBobberInstance != null)
        {
            spawnPos = currentBobberInstance.transform.position;
        }
        else
        {
            spawnPos = playerTransform.position + playerTransform.forward * 5f + Vector3.up * 0.5f;
            Debug.LogWarning("CurrentBobberInstance was null, spawning fish at fallback position.");
        }
        DestroyBobber();

        Vector3 destinationPos;
        if (fishLandPoint != null)
        {
            destinationPos = fishLandPoint.position;
        }
        else
        {
            destinationPos = playerTransform.position + playerTransform.forward * 0.3f + Vector3.up * 0.2f;
        }

        GameObject spawnedFish = Instantiate(fishPrefab, spawnPos, playerTransform.rotation);
        _lineTarget = spawnedFish.transform;

        
        
        
        _targetLineSag = maxLineSag;
        

        FishPickup pickupScript = spawnedFish.GetComponent<FishPickup>();
        if (pickupScript == null)
        {
            Debug.LogWarning("Fish Prefab missing FishPickup script! Adding automatically...");
            pickupScript = spawnedFish.AddComponent<FishPickup>();
        }

        pickupScript.Initialize(fishInstance, playerInventory);
        Collider fishCollider = spawnedFish.GetComponent<Collider>();
        if (fishCollider != null) fishCollider.enabled = false;
        Rigidbody fishRb = spawnedFish.GetComponent<Rigidbody>();
        if (fishRb != null) fishRb.isKinematic = true;

        StartCoroutine(ReelFishInRoutine(spawnedFish, destinationPos, 0.8f));
        playerInventory.temporaryFish = null;
    }




    public void AnimationEvent_CatchFinished()
    {
        Debug.Log("[FishingController] Animation Event: CatchFinished!");

        if (currentState == FishingState.Caught)
        {
            _inputCooldownTimer = 0.5f;
            ChangeState(FishingState.NotFishing);
        }
    }

    private FishData GetRandomFishFromCurrentSpot()
    {
        if (currentSpot == null) return null;
        float randomValue = Random.Range(0f, 1f);
        float cumulativeChance = 0f;
        foreach (var fishInfo in currentSpot.spotData.availableFish)
        {
            cumulativeChance += fishInfo.spawnChance;
            if (randomValue <= cumulativeChance) return fishInfo.fishData;
        }
        return null;
    }

    private IEnumerator WaitingIndicatorCoroutine()
    {
        if (waitingIndicator == null) yield break;
        List<Label> dots = waitingIndicator.Query<Label>().ToList();
        if (dots.Count < 3) yield break;
        dots[0].style.opacity = 0; dots[1].style.opacity = 0; dots[2].style.opacity = 0;
        while (true)
        {
            yield return new WaitForSeconds(0.3f); dots[0].style.opacity = 1;
            yield return new WaitForSeconds(0.3f); dots[1].style.opacity = 1;
            yield return new WaitForSeconds(0.3f); dots[2].style.opacity = 1;
            yield return new WaitForSeconds(0.5f);
            dots[0].style.opacity = 0; dots[1].style.opacity = 0; dots[2].style.opacity = 0;
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator BiteIndicatorAnimationCoroutine()
    {
        float shakeIntensity = 5f; float shakeDuration = 0.05f;
        while (currentState == FishingState.Biting)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity); float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
            biteIndicator.style.translate = new StyleTranslate(new Translate(new Length(-50f + offsetX, LengthUnit.Percent), new Length(-50f + offsetY, LengthUnit.Percent)));
            yield return new WaitForSeconds(shakeDuration);
            if (currentState == FishingState.Biting)
            {
                biteIndicator.style.translate = new StyleTranslate(new Translate(new Length(-50f, LengthUnit.Percent), new Length(-50f, LengthUnit.Percent)));
            }
        }
        biteIndicator.style.translate = new StyleTranslate(new Translate(new Length(-50f, LengthUnit.Percent), new Length(-50f, LengthUnit.Percent)));
    }

    private void LoadFishDataForMinigame()
    {
        if (currentFishData == null) { ChangeState(FishingState.Failed); return; }

        currentFishThreshold = currentFishData.baseProgressThreshold;
        if (currentFishThreshold <= 0)
        {
            Debug.LogWarning($"Fish '{currentFishData.fishName}' has baseProgressThreshold = 0! Defaulting to 100.");
            currentFishThreshold = 100f;
        }

        float rodPower = (playerInventory.currentRod != null) ? playerInventory.currentRod.pullPower : 1f;

        fishZoneSize = 50f - (currentFishData.rarity * 6f);
        fishZoneSize = Mathf.Max(fishZoneSize, 10f);

        playerTargetAscendSpeed = currentFishData.playerPullAgainst;
        playerTargetDescendSpeed = currentFishData.baseSlippage;

        fishZoneMoveIntervalMin = currentFishData.minTargetMoveInterval;
        fishZoneMoveIntervalMax = currentFishData.maxTargetMoveInterval;

        progressGainRate = rodPower;
        progressLossRate = currentFishData.baseProgressDrain;

        Debug.Log($"Loaded Fish: {currentFishData.fishName} | Threshold (HP): {currentFishThreshold}");
        Debug.Log($"Rod Power: {rodPower} (as Gain Rate: {progressGainRate} points/sec)");
        Debug.Log($"Fish Drain: {currentFishData.baseProgressDrain} (as Loss Rate: {progressLossRate} points/sec)");
        Debug.Log($"Target Speeds -> Ascend: {playerTargetAscendSpeed} | Descend: {playerTargetDescendSpeed}");
    }



    private void HandlePlayerTargetMovement()
    {
        if (Input.GetMouseButton(0))
        {
            playerTargetPosition += playerTargetAscendSpeed * Time.deltaTime;
            _targetLineSag = minigameLineTaut;
        }
        else
        {
            playerTargetPosition -= playerTargetDescendSpeed * Time.deltaTime;
            _targetLineSag = minigameLineSag;
        }
        playerTargetPosition = Mathf.Clamp(playerTargetPosition, 0f, 100f - PLAYER_TARGET_SIZE);
    }

    private void HandleFishZoneMovement()
    {
        fishZoneMoveTimer -= Time.deltaTime;
        if (fishZoneMoveTimer <= 0f)
        {
            fishZoneTargetPosition = GetRandomZonePosition();
            fishZoneMoveTimer = Random.Range(fishZoneMoveIntervalMin, fishZoneMoveIntervalMax);
        }
        fishZonePosition = Mathf.Lerp(fishZonePosition, fishZoneTargetPosition, Time.deltaTime * 2f);
    }

    private float GetRandomZonePosition() { return Random.Range(0, 100 - fishZoneSize); }

    private void HandleProgress()
    {
        if (IsTargetInZone()) currentProgress += progressGainRate * Time.deltaTime;
        else currentProgress -= progressLossRate * Time.deltaTime;

        currentProgress = Mathf.Clamp(currentProgress, 0f, currentFishThreshold);

        if (currentProgress >= currentFishThreshold) ChangeState(FishingState.Caught);
        else if (currentProgress <= 0f) ChangeState(FishingState.Failed);
    }

    private bool IsTargetInZone()
    {
        float playerTarget_min = playerTargetPosition; float playerTarget_max = playerTargetPosition + PLAYER_TARGET_SIZE;
        float fishZone_min = fishZonePosition; float fishZone_max = fishZonePosition + fishZoneSize;
        return playerTarget_min < fishZone_max && playerTarget_max > fishZone_min;
    }

    private void UpdateMinigameUI()
    {
        float normalizedProgress = 0f;
        if (currentFishThreshold > 0)
        {
            normalizedProgress = currentProgress / currentFishThreshold;
        }

        minigameProgress.style.width = new StyleLength(new Length(normalizedProgress * 100f, LengthUnit.Percent));
        fishZone.style.left = new StyleLength(new Length(fishZonePosition, LengthUnit.Percent));
        fishZone.style.width = new StyleLength(new Length(fishZoneSize, LengthUnit.Percent));
        playerTarget.style.left = new StyleLength(new Length(playerTargetPosition, LengthUnit.Percent));
        playerTarget.style.width = new StyleLength(new Length(PLAYER_TARGET_SIZE, LengthUnit.Percent));
    }

    public void EnterFishingSpot(FishingSpot spot)
    {
        if (currentState == FishingState.NotFishing)
        {
            Debug.Log("Player entered fishing spot: " + (spot?.spotData?.name ?? "Unknown"));
            canFish = true;
            currentSpot = spot;
        }
    }

    public void ExitFishingSpot()
    {
        Debug.Log("Player exited fishing spot.");
        canFish = false;
        currentSpot = null;
        if (currentState != FishingState.NotFishing)
        {
            DestroyBobber();
            ChangeState(FishingState.Failed);
        }
    }

    public void ApplyTiltCorrection()
    {
        if (characterModelRoot != null)
        {
            characterModelRoot.localEulerAngles = animationTiltCorrection;
        }
    }

    public void ResetTiltCorrection()
    {
        if (characterModelRoot != null)
        {
            characterModelRoot.localEulerAngles = Vector3.zero;
        }
    }
}