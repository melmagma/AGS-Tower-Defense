using UnityEngine;
using UnityEngine.InputSystem;
using static System.TimeZoneInfo;
using static UnityEditor.Profiling.HierarchyFrameDataView;

public enum ViewMode
{
    ThreeQuarter,
    TopDown
}

[DefaultExecutionOrder(100)]
public class CameraRig : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to use Camera.main")]
    public Transform cameraTransform;

    [Header("View Modes")]
    public ViewMode currentMode = ViewMode.ThreeQuarter;
    public Key toggleKey = Key.T;

    [Header("Top-Down Mode")]
    [Range(60f, 90f)] public float topDownPitch = 90f;
    public float topDownMinDistance = 12f;
    public float topDownMaxDistance = 60f;
    [Tooltip("Allow Q/E and middle-drag rotation while in top-down")]
    public bool topDownAllowRotation = false;
    [Tooltip("Snap yaw to the nearest Snap Angle when entering top-down so a grid lines up with the screen")]
    public bool topDownSnapYaw = true;

    [Header("Mode Transition")]
    [Tooltip("Smoothing used while switching modes. Lower = slower, more cinematic")]
    public float transitionSmoothing = 4f;
    [Tooltip("How long (seconds) the slower transition smoothing is used")]
    public float transitionDuration = 1f;

    /// <summary>Fired whenever the mode changes (hide fog, show grid overlay, update UI, etc.)</summary>
    public event System.Action<ViewMode> ModeChanged;

    [Header("Orbit (the circular track)")]
    public float startYaw = 45f;
    public float rotateKeySpeed = 90f;      // degrees / second for Q/E
    public float rotateDragSpeed = 4f;      // degrees per mouse unit
    [Tooltip("Angle for snap rotation (SnapRotate method), e.g. 90 for 4 fixed views")]
    public float snapAngle = 90f;

    [Header("Pitch (3/4 view angle)")]
    [Range(10f, 89f)] public float pitch = 55f;
    [Tooltip("Let the player tilt with middle mouse drag")]
    public bool allowManualPitch = false;
    public float minPitch = 35f;
    public float maxPitch = 75f;
    public float pitchDragSpeed = 2.5f;
    [Tooltip("Zoomed out = steeper (more top-down), zoomed in = flatter")]
    public bool tiltWithZoom = true;
    [Range(10f, 89f)] public float pitchWhenZoomedIn = 40f;
    [Range(10f, 89f)] public float pitchWhenZoomedOut = 65f;

    [Header("Zoom (track radius)")]
    public float startDistance = 18f;
    public float minDistance = 8f;
    public float maxDistance = 40f;
    public float zoomSpeed = 4f;

    [Header("Pan")]
    public float panSpeed = 1.2f;           // multiplied by distance so zoomed-out feels right
    public float dragPanSpeed = 0.06f;
    public bool edgeScrolling = false;
    public float edgeSize = 15f;            // pixels from screen edge

    [Header("Bounds (XZ, world space)")]
    public bool clampToBounds = true;
    public Vector2 boundsCenter = Vector2.zero;
    public Vector2 boundsSize = new Vector2(60f, 60f);

    [Header("Smoothing")]
    [Tooltip("Higher = snappier. 0 = no smoothing")]
    public float smoothing = 12f;

    // Targets (where we want to be) and current (what we're showing)
    Vector3 _targetFocus, _focus;
    float _targetYaw, _yaw;
    float _targetPitch, _pitch;
    float _targetDistance, _distance;
    float _transitionTimer;

    // NEW: distance limits depend on the active mode
    float ActiveMinDistance => currentMode == ViewMode.TopDown ? topDownMinDistance : minDistance;
    float ActiveMaxDistance => currentMode == ViewMode.TopDown ? topDownMaxDistance : maxDistance;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        _targetFocus = _focus = transform.position;
        _targetYaw = _yaw = startYaw;
        _targetPitch = _pitch = pitch;
        _targetDistance = _distance = Mathf.Clamp(startDistance, minDistance, maxDistance);

        ApplyTransform();
    }

    void LateUpdate()
    {
        // unscaledDeltaTime so the camera still works while the game is paused
        float dt = Time.unscaledDeltaTime;

        HandleModeInput();
        HandleRotation(dt);
        HandleZoom();
        HandlePan(dt);
        Smooth(dt);
        ApplyTransform();
    }

    // ---------------------------------------------------------------- input

    void HandleModeInput()
    {
        var kb = Keyboard.current;
        if (kb != null && kb[toggleKey].wasPressedThisFrame)
            ToggleMode();
    }
    void HandleRotation(float dt)
    {
        bool canRotate = currentMode == ViewMode.ThreeQuarter || topDownAllowRotation;

        if (Input.GetKey(KeyCode.Q)) _targetYaw -= rotateKeySpeed * dt;
        if (Input.GetKey(KeyCode.E)) _targetYaw += rotateKeySpeed * dt;

        if (Input.GetMouseButton(2))
        {
            _targetYaw += Input.GetAxis("Mouse X") * rotateDragSpeed;
            if (allowManualPitch && currentMode == ViewMode.ThreeQuarter)
            {
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * pitchDragSpeed, minPitch, maxPitch);
            }
        }
    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
            _targetDistance = Mathf.Clamp(_targetDistance - scroll * zoomSpeed, minDistance, maxDistance);

        if (currentMode == ViewMode.TopDown)
        {
            _targetPitch = topDownPitch;
        }
        else if (tiltWithZoom && !allowManualPitch)
        {
            float t = Mathf.InverseLerp(minDistance, maxDistance, _targetDistance);
            _targetPitch = Mathf.Lerp(pitchWhenZoomedIn, pitchWhenZoomedOut, t);
        }
        else
        {
            _targetPitch = pitch;
        }
    }

    void HandlePan(float dt)
    {
        // Flatten the camera's facing onto the ground plane
        Quaternion yawRot = Quaternion.Euler(0f, _targetYaw, 0f);
        Vector3 right = yawRot * Vector3.right;
        Vector3 forward = yawRot * Vector3.forward;

        Vector3 move = Vector3.zero;

        // Keyboard
        move += right * Input.GetAxisRaw("Horizontal");
        move += forward * Input.GetAxisRaw("Vertical");

        // Edge scrolling
        if (edgeScrolling && Application.isFocused)
        {
            Vector3 m = Input.mousePosition;
            if (m.x <= edgeSize) move -= right;
            else if (m.x >= Screen.width - edgeSize) move += right;
            if (m.y <= edgeSize) move -= forward;
            else if (m.y >= Screen.height - edgeSize) move += forward;
        }

        if (move.sqrMagnitude > 1f) move.Normalize();
        _targetFocus += move * (panSpeed * _targetDistance * dt);

        // Right-mouse drag ("grab the ground")
        if (Input.GetMouseButton(1))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            _targetFocus -= (right * dx + forward * dy) * (dragPanSpeed * _targetDistance);
        }

        if (clampToBounds)
        {
            float hx = boundsSize.x * 0.5f;
            float hz = boundsSize.y * 0.5f;
            _targetFocus.x = Mathf.Clamp(_targetFocus.x, boundsCenter.x - hx, boundsCenter.x + hx);
            _targetFocus.z = Mathf.Clamp(_targetFocus.z, boundsCenter.y - hz, boundsCenter.y + hz);
        }
    }

    // ------------------------------------------------------------- smoothing

    void Smooth(float dt)
    {
        float s = smoothing;
        if (_transitionTimer > 0f)
        {
            s = transitionSmoothing;
            _transitionTimer -= dt;
        }

        float k = s <= 0f ? 1f : 1f - Mathf.Exp(-s * dt);

        _focus = Vector3.Lerp(_focus, _targetFocus, k);
        _yaw = Mathf.LerpAngle(_yaw, _targetYaw, k);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, k);
        _distance = Mathf.Lerp(_distance, _targetDistance, k);
    }

    // -------------------------------------------------------------- apply

    void ApplyTransform()
    {
        if (cameraTransform == null) return;

        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        // Point on the circular track, looking at the focus point
        cameraTransform.position = _focus + rot * new Vector3(0f, 0f, -_distance);
        cameraTransform.rotation = rot;

        transform.position = _focus; // keep rig object at the focus for easy debugging
    }

    // ------------------------------------------------------- public helpers

    /// <summary>Smoothly move the focus to a world position (e.g. selected tower, base, new wave spawn).</summary>
    public void FocusOn(Vector3 worldPos)
    {
        _targetFocus = new Vector3(worldPos.x, _targetFocus.y, worldPos.z);
    }

    /// <summary>Rotate the track position by +/- snapAngle (hook up to UI buttons).</summary>
    public void SnapRotate(int direction)
    {
        _targetYaw += Mathf.Sign(direction) * snapAngle;
    }

    // NEW ------------------------------------------------------------------

    /// <summary>Switch between 3/4 and top-down. Hook this to a UI button.</summary>
    public void ToggleMode()
    {
        SetMode(currentMode == ViewMode.ThreeQuarter ? ViewMode.TopDown : ViewMode.ThreeQuarter);
    }

    // Parameterless versions so UI Button OnClick events can call them directly
    public void SetThreeQuarter() => SetMode(ViewMode.ThreeQuarter);
    public void SetTopDown() => SetMode(ViewMode.TopDown);

    public void SetMode(ViewMode mode)
    {
        if (mode == currentMode) return;

        float oldMin = ActiveMinDistance;
        float oldMax = ActiveMaxDistance;

        currentMode = mode;

        // Keep the same relative zoom level in the new mode's distance range
        float t = Mathf.InverseLerp(oldMin, oldMax, _targetDistance);
        _targetDistance = Mathf.Lerp(ActiveMinDistance, ActiveMaxDistance, t);

        // Line the view up with the grid when going top-down
        if (currentMode == ViewMode.TopDown && topDownSnapYaw && snapAngle > 0f)
            _targetYaw = Mathf.Round(_targetYaw / snapAngle) * snapAngle;

        _transitionTimer = transitionDuration;
        ModeChanged?.Invoke(currentMode);
    }

    // --------------------------------------------------------------- gizmos

    void OnDrawGizmosSelected()
    {
        // Draw the "dolly track" circle and the playable bounds
        Vector3 c = Application.isPlaying ? _focus : transform.position;
        float r = Application.isPlaying ? _distance : startDistance;
        float h = Mathf.Sin((Application.isPlaying ? _pitch : pitch) * Mathf.Deg2Rad) * r;
        float flatR = Mathf.Cos((Application.isPlaying ? _pitch : pitch) * Mathf.Deg2Rad) * r;

        Gizmos.color = Color.cyan;
        Vector3 prev = c + new Vector3(flatR, h, 0f);
        for (int i = 1; i <= 64; i++)
        {
            float a = i / 64f * Mathf.PI * 2f;
            Vector3 next = c + new Vector3(Mathf.Cos(a) * flatR, h, Mathf.Sin(a) * flatR);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        if (clampToBounds)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(boundsCenter.x, c.y, boundsCenter.y),
                                new Vector3(boundsSize.x, 0.1f, boundsSize.y));
        }
    }
}
