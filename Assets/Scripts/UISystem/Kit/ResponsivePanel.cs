using UnityEngine;

// 팝업 패널이 화면 방향에 맞는 크기를 갖게 한다(2026-10-29 표준안). 템플릿은 가로·세로 자동 회전이라 앵커 하나로는 한쪽이 납작해진다.
// 가로: 폭 60%·높이 80% / 세로: 폭 90%·높이 70%(조사 문서 부품 표). 부모(SafeArea) 크기가 바뀔 때마다 다시 잡는다
[RequireComponent(typeof(RectTransform))]
public class ResponsivePanel : MonoBehaviour
{
    [SerializeField] private Vector2 landscapeSize = new Vector2(0.6f, 0.8f);
    [SerializeField] private Vector2 portraitSize = new Vector2(0.9f, 0.7f);

    private RectTransform rect;

    private void Awake()
    {
        rect = (RectTransform)transform;
    }

    private void OnEnable()
    {
        Apply();
    }

    private void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    private void Apply()
    {
        if (rect == null) { return; }
        var parent = rect.parent as RectTransform;
        if (parent == null) { return; }

        Vector2 parentSize = parent.rect.size;
        bool landscape = parentSize.x >= parentSize.y;
        Vector2 size = landscape ? landscapeSize : portraitSize;
        rect.anchorMin = new Vector2(0.5f - size.x / 2f, 0.5f - size.y / 2f);
        rect.anchorMax = new Vector2(0.5f + size.x / 2f, 0.5f + size.y / 2f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
