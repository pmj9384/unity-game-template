using UnityEngine.EventSystems;

// 가챠 결과 팝업 공통 — 팝업 아무 데나 탭하면 닫힌다.
// 클릭은 EventSystem에 맡긴다. 팝업 루트의 Image가 RaycastTarget이라 팝업 전체가 판정 영역이 되고,
// InputSystemUIInputModule이 UI 액션 맵의 Click을 받아 이 핸들러를 부른다.
// 예전에는 InputActionAsset을 직접 들고 FindActionMap("UI").FindAction("Click")으로 같은 액션을
// 한 번 더 잡았는데, 그 에셋이 리포에 없어 Awake에서 NullReferenceException이 났다.
public abstract class GachaResultPopupBase : UIPopup, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData) => Hide();
}
