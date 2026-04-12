using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshFilter))]
public class GoalZone : MonoBehaviour
{
    [Header("오버레이")]
    [SerializeField] private Renderer _overlayRenderer;

    [Header("UI")]
    [SerializeField] private TextMeshPro _countText;
    [SerializeField] private float _textHeightOffset = 0.8f;

    private Camera _cam;
    private BoxCollider _col;
    private Vector2 _meshLocalSize; // 메시의 로컬 XZ 크기

    private void Awake()
    {
        _cam = Camera.main;
        _col = GetComponent<BoxCollider>();

        // 메시 로컬 바운드에서 XZ 크기 캐싱 (Plane=10, Quad=1 등 자동 대응)
        var mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            var b = mf.sharedMesh.bounds;
            _meshLocalSize = new Vector2(b.size.x, b.size.z);
        }
        else
        {
            _meshLocalSize = Vector2.one; // fallback
        }

        // CountText를 GoalZone 자식에서 분리 → 스케일 영향 차단
        if (_countText != null)
            _countText.transform.SetParent(null, worldPositionStays: true);
    }

    private void LateUpdate()
    {
        if (_countText == null || _cam == null) return;

        // GoalZone 월드 위치를 따라가되 스케일은 독립 유지
        _countText.transform.position = transform.position + Vector3.up * _textHeightOffset;
        _countText.transform.forward = _cam.transform.forward;
    }

    private static readonly Color RedColor = new(1f, 0f, 0f, 0.4f);
    private static readonly Color GreenColor = new(0f, 1f, 0f, 0.4f);

    private int _requiredCount;
    private readonly HashSet<GameObject> _unitsInside = new();

    public bool IsCompleted => _unitsInside.Count >= _requiredCount;
    public int UnitsInside => _unitsInside.Count;
    public int UnitsNeeded => Mathf.Max(0, _requiredCount - _unitsInside.Count);
    public Vector3 Center => transform.position;
    public bool IsUnitInside(Unit unit) => _unitsInside.Contains(unit.gameObject);

    // CommanderAgent가 에피소드마다 호출
    public void Initialize(int requiredCount, Vector3 position, Vector3 scale)
    {
        _requiredCount = requiredCount;
        transform.position = position;
        transform.localScale = scale;
        _unitsInside.Clear();

        // 메시 전체를 덮도록 XZ는 메시 로컬 크기, Y는 월드 4m (겹친 유닛 포함)
        float worldHeight = 4f;
        float localH = worldHeight / Mathf.Max(scale.y, 0.001f);
        _col.size   = new Vector3(_meshLocalSize.x, localH, _meshLocalSize.y);
        // center를 위쪽으로 올려서 바닥~4m 하늘 영역 커버
        _col.center = new Vector3(0f, localH * 0.5f, 0f);

        // 즉시 물리 동기화 → 이전 유닛의 Exit 이벤트가 지연 발생하는 것 방지
        Physics.SyncTransforms();

        UpdateVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Unit")) return;
        _unitsInside.Add(other.gameObject);
        UpdateVisual();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Unit")) return;
        _unitsInside.Remove(other.gameObject);
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        _countText.text = _requiredCount - _unitsInside.Count > 0 ? $"{_requiredCount}({_unitsInside.Count})" : "OK!";

        // .material 로 인스턴스 복사본을 수정 → 공유 머티리얼 오염 없음
        _overlayRenderer.material.color = IsCompleted ? GreenColor : RedColor;
    }
}
