using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class Dakimakurator : UdonSharpBehaviour
{
    public GameObject ui;
    public GameObject failUi;
    public VRCObjectPool pool;
    public Transform spawnPoint;

    [Header("Camera Settings")]
    public int textureWidth = 2048;
    public int textureHeight = 2048;
    private readonly float cameraDistance = 2f;
    private float cameraHeight = 1f;

    // Assign a camera in the inspector. Do not create a new GameObject at runtime (not supported by Udon).
    public Camera captureCamera;

    [Header("Effects")]
    public AudioSource audioSource;
    public AudioClip soundFx;
    public AudioClip[] voiceLines;
    public AudioClip failVoiceLine;
    public Material emissionMat;

    [Header("Emission Settings")]
    public float emissionBumpIntensity = 2f;
    public float emissionTransitionDuration = 1f;

    // Store the player currently in the trigger zone
    private VRCPlayerApi currentPlayer;

    // Emission animation state
    private float emissionStartIntensity = 1f;
    private float emissionTargetIntensity = 1f;
    private float emissionTransitionTime = 0f;
    private bool isAnimatingEmission = false;

    // RenderTextures are created automatically at Start(). Hidden from inspector.
    [System.NonSerialized]
    public RenderTexture frontRT;

    [System.NonSerialized]
    public RenderTexture backRT;

    // Synced variables for texture updates
    [UdonSynced]
    private int spawnedDakimakuraIndex = -1;

    void Start()
    {
        // Do NOT use Camera.main here because it's not exposed in Udon.
        // The user must assign a Camera in the inspector to `captureCamera`.
        if (captureCamera != null)
        {
            captureCamera.enabled = false;
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
        }
        else
        {
            Debug.LogWarning(
                "Dakimakurator: No captureCamera assigned. Please assign a Camera in the inspector. Capture functions will be disabled."
            );
        }

        // Create render textures for front and back captures
        frontRT = new RenderTexture(textureWidth / 2, textureHeight, 24);
        backRT = new RenderTexture(textureWidth / 2, textureHeight, 24);

        // Store initial emission intensity
        if (emissionMat != null)
        {
            emissionStartIntensity = emissionMat.GetFloat("_EmissionStrength");
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            ui.SetActive(true);
        }
        // Store whoever enters the trigger - they're the one to be scanned
        currentPlayer = player;

        float height = player.GetAvatarEyeHeightAsMeters();
        captureCamera.orthographicSize = (height / 2) + 0.5f;
        cameraHeight = height / 2f;
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            ui.SetActive(false);
        }
    }

    public void CreateDakimakura()
    {
        SendCustomEventDelayedSeconds(nameof(FinishDakimakura), 4f);
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayAnimation));
    }

    public void PlayAnimation()
    {
        // Disable UI
        ui.SetActive(false);

        // Play sound FX
        audioSource.PlayOneShot(soundFx);

        // Start smooth emission bump animation
        if (emissionMat != null)
        {
            emissionStartIntensity = emissionMat.GetFloat("_EmissionStrength");
            emissionTargetIntensity = emissionStartIntensity * emissionBumpIntensity;
            emissionTransitionTime = 0f;
            isAnimatingEmission = true;
        }
    }

    void Update()
    {
        if (isAnimatingEmission && emissionMat != null)
        {
            emissionTransitionTime += Time.deltaTime;
            float t = Mathf.Clamp01(emissionTransitionTime / emissionTransitionDuration);

            // Smooth interpolation
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentIntensity = Mathf.Lerp(
                emissionStartIntensity,
                emissionTargetIntensity,
                smoothT
            );

            emissionMat.SetFloat("_EmissionStrength", currentIntensity);

            if (t >= 1f)
            {
                isAnimatingEmission = false;
            }
        }
    }

    public void HideFailUi()
    {
        failUi.SetActive(false);
    }

    [NetworkCallable]
    public void PlayVoiceline(int index)
    {
        audioSource.PlayOneShot(voiceLines[index]);
    }

    public void FinishDakimakura()
    {
        // Re-enable UI
        ui.SetActive(true);

        // Capture whoever is currently in the trigger zone
        if (currentPlayer == null)
            return;

        // Start smooth emission return animation
        if (emissionMat != null)
        {
            emissionStartIntensity = emissionMat.GetFloat("_EmissionStrength");
            emissionTargetIntensity = 1f; // Return to normal intensity
            emissionTransitionTime = 0f;
            isAnimatingEmission = true;
        }

        // Create new RenderTextures for this specific dakimakura
        RenderTexture newFrontRT = new RenderTexture(textureWidth / 2, textureHeight, 24);
        RenderTexture newBackRT = new RenderTexture(textureWidth / 2, textureHeight, 24);

        // Capture front and back into the new RenderTextures
        CapturePlayerTexture(currentPlayer, true, newFrontRT);
        CapturePlayerTexture(currentPlayer, false, newBackRT);

        if (newFrontRT == null || newBackRT == null)
        {
            Debug.LogWarning(
                "Dakimakurator: Failed to capture front or back RenderTexture. Ensure a captureCamera is assigned in the inspector."
            );
            return;
        }

        Networking.SetOwner(Networking.LocalPlayer, pool.gameObject);
        GameObject newDakimakura = pool.TryToSpawn();

        if (newDakimakura == null)
        {
            // Play fail voice line if pool is exhausted
            audioSource.PlayOneShot(failVoiceLine);
            failUi.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(HideFailUi), 3f);
            return;
        }

        // Spawn the dakimakura prefab
        int randomIndex = Random.Range(0, voiceLines.Length);
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayVoiceline), randomIndex);

        Vector3 spawnPosition =
            spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 2f;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        newDakimakura.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        // Apply textures locally first
        ApplyTextureToObject(newDakimakura, newFrontRT, newBackRT);

        // Sync which object was spawned to other players
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        spawnedDakimakuraIndex = GetPoolObjectIndex(newDakimakura);
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        // When synced variables update, apply textures on remote clients
        if (!Networking.IsOwner(gameObject) && spawnedDakimakuraIndex >= 0)
        {
            GameObject dakimakura = GetPoolObjectByIndex(spawnedDakimakuraIndex);
            if (dakimakura != null && currentPlayer != null)
            {
                // Re-capture the player on remote clients
                RenderTexture remoteFrontRT = new RenderTexture(
                    textureWidth / 2,
                    textureHeight,
                    24
                );
                RenderTexture remoteBackRT = new RenderTexture(textureWidth / 2, textureHeight, 24);

                CapturePlayerTexture(currentPlayer, true, remoteFrontRT);
                CapturePlayerTexture(currentPlayer, false, remoteBackRT);

                ApplyTextureToObject(dakimakura, remoteFrontRT, remoteBackRT);
            }
        }
    }

    private void ApplyTextureToObject(GameObject obj, RenderTexture frontTex, RenderTexture backTex)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.materials.Length >= 2)
        {
            Material[] materials = renderer.materials;
            materials[0].mainTexture = frontTex;
            materials[1].mainTexture = backTex;
            renderer.materials = materials;

            Debug.Log("Dakimakurator: Assigned front texture to slot 0, back texture to slot 1");
        }
    }

    private int GetPoolObjectIndex(GameObject obj)
    {
        // Find the index of this object in the pool
        Transform poolParent = pool.transform;
        for (int i = 0; i < poolParent.childCount; i++)
        {
            if (poolParent.GetChild(i).gameObject == obj)
            {
                return i;
            }
        }
        return -1;
    }

    private GameObject GetPoolObjectByIndex(int index)
    {
        Transform poolParent = pool.transform;
        if (index >= 0 && index < poolParent.childCount)
        {
            return poolParent.GetChild(index).gameObject;
        }
        return null;
    }

    private RenderTexture CapturePlayerTexture(
        VRCPlayerApi player,
        bool isFront,
        RenderTexture targetRT
    )
    {
        if (captureCamera == null || player == null || targetRT == null)
            return null;

        // Get player position and rotation
        Vector3 playerPos = player.GetPosition();
        Quaternion playerRot = player.GetRotation();

        // Calculate camera position
        Vector3 direction = isFront ? playerRot * Vector3.forward : playerRot * Vector3.back;
        Vector3 camPos = playerPos + direction * cameraDistance + Vector3.up * cameraHeight;

        // Position and orient camera
        captureCamera.transform.position = camPos;
        captureCamera.transform.LookAt(playerPos + Vector3.up * cameraHeight);

        // Set target render texture
        captureCamera.targetTexture = targetRT;

        // Render the scene into the RenderTexture. We avoid ReadPixels/RenderTexture.active because they're not exposed to Udon.
        captureCamera.Render();

        // Clear the camera's target to prevent continuous rendering
        captureCamera.targetTexture = null;

        // Return the RenderTexture reference for direct assignment to material properties.
        return targetRT;
    }
}
