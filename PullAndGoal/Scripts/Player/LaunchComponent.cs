using System;
using UnityEngine;

public class LaunchComponent : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera cam;
    private PlayerState playerState;
    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;


    [Header("Launch Info")]
    public float power = 1000f;
    private int _remainingLaunches;
    private int remainingLaunches
    {
        get { return _remainingLaunches; }
        set
        {
            _remainingLaunches = Mathf.Clamp(value, 0, 3);
            SetSpriteColorByLaunches();
        }
    }
    [Space]

    [Header("Drag Info")]
    [SerializeField] private float minDragDist = 50f;
    [SerializeField] private float maxDragDist = 200f;
    [Space]

    [Header("Aim Info")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int numOfDots;
    [SerializeField] private float distanceOfDots;
    private GameObject[] dots;

    [Space]
    private Vector2 dragStartPos;

    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();

        GenerateDots();

        playerState = GetComponent<PlayerState>();
        playerState.OnLanded += LandedRefillLaunches;
        playerState.OnTakenOff += TakenOffReduceLaunches;
        playerState.OnGetLaunchPack += AddLaunch;

        SetSpriteColorByLaunches();
    }

    private void Update()
    {
        if (playerState.dead)
        {
            return;
        }

        if (playerState._isGrounded)
        {
            remainingLaunches = 1;
        }

        if (Input.GetMouseButtonDown(0))
        {
            DragStart();
        }
        else if (Input.GetMouseButton(0))
        {
            Dragging();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            DragRelease();
        }
    }

    #region Drag
    private void DragStart()
    {
        dragStartPos = Input.mousePosition;
    }

    private void Dragging()
    {
        ShowTrajectory();
        DrawLine();
    }

    private void DrawLine()
    {
        SetLineEnable(true);

        Vector2 curPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 startPos = cam.ScreenToWorldPoint(dragStartPos);

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, curPos);
    }

    private void DragRelease()
    {
        SetDotsActive(false);
        SetLineEnable(false);
        Launch();
    }

    private void SetLineEnable(bool enable)
    {
        lineRenderer.enabled = enable;
    }

    #endregion

    #region AimDots
    private void GenerateDots()
    {
        dots = new GameObject[numOfDots];

        for (int i = 0; i < numOfDots; i++)
        {
            dots[i] = Instantiate(dotPrefab, transform.position, Quaternion.identity);
            dots[i].SetActive(false);
        }
    }

    private void SetDotsActive(bool active)
    {
        for (int i = 0; i < numOfDots; i++)
        {
            dots[i].SetActive(active);
        }
    }

    private void ShowTrajectory()
    {
        Vector2 velocity = GetLaunchVelocity();
        if (velocity != Vector2.zero && dots[0].activeSelf == false)
        {
            SetDotsActive(true);
        }
        else if (velocity == Vector2.zero && dots[0].activeSelf == true)
        {
            SetDotsActive(false);
        }

        for (int i = 0; i < numOfDots; i++)
        {
            float t = i * distanceOfDots;
            Vector2 pos = (Vector2)transform.position + 
                                    velocity * t + 
                                    0.5f * (Physics2D.gravity * rb.gravityScale) * (t * t);

            dots[i].transform.position = pos;
        }
    }

    #endregion

    #region Launch
    void LandedRefillLaunches()
    {
        remainingLaunches = Mathf.Max(remainingLaunches, 1);
        AudioManager.Instance.PlaySFX("RefillLaunches");
    }

    void TakenOffReduceLaunches()
    {
        ReduceLaunch();
    }

    private void AddLaunch()
    {
        remainingLaunches = Mathf.Min(remainingLaunches + 1, 3);
        AudioManager.Instance.PlaySFX("RefillLaunches");
    }

    private void ReduceLaunch()
    {
        remainingLaunches = Mathf.Max(remainingLaunches - 1, 0);
    }


    void Launch()
    {
        Vector2 velocity = GetLaunchVelocity();

        if (remainingLaunches > 0 && velocity != Vector2.zero)
        {
            ReduceLaunch();
            rb.linearVelocity = GetLaunchVelocity();
            AudioManager.Instance.PlaySFX("Launch");
        }
    }

    Vector2 GetLaunchForce()
    {
        Vector2 dragReleasePos = Input.mousePosition;

        float dragDist = Vector2.Distance(dragStartPos, dragReleasePos);

        if (dragDist < minDragDist)
        {
            return Vector2.zero;
        }

        return (Vector2.ClampMagnitude(dragStartPos - dragReleasePos, maxDragDist) / maxDragDist) * power;
    }

    Vector2 GetLaunchVelocity()
    {
        return (GetLaunchForce() / rb.mass) * Time.fixedDeltaTime;
    }
    #endregion

    #region Visual

    void SetSpriteColorByLaunches()
    {
        Color[] colors = { Color.grey, Color.white, Color.yellow, Color.green };
        spriteRenderer.color = colors[remainingLaunches];
    }

    #endregion
}
