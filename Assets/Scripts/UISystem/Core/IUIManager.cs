public interface IUIManager
{
    void OpenScreen<T>() where T : UIScreen;
    void ShowPopup<T>() where T : UIPopup;
    void HidePopup<T>() where T : UIPopup;
    T GetPopup<T>() where T : UIPopup;   // 인자를 넘겨 여는 팝업(ConfirmPopup.Open 등)

    void OpenScreen(UIScreen screen);

    void NotifyPopupShown(UIPopup popup);
    void NotifyPopupHidden(UIPopup popup);
}
