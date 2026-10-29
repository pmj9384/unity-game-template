using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour, IUIManager
{
    private readonly Dictionary<Type, UIScreen> screens = new();
    private readonly Dictionary<Type, UIPopup> popups = new();
    private readonly List<UIPopup> openPopups = new();   // 연 순서. 뒤로가기는 맨 뒤(가장 최근)부터 닫는다

    // 안드로이드 뒤로가기는 Input System에서 Escape 키로 들어온다고 알려져 있다(2026-10-29 UI 키트. 문서로는 못 찾았고 실기기 확인 필요).
    // 열린 팝업이 없으면 아무것도 안 한다(화면 뒤로가기는 게임 몫). 인게임 UIElement 패널은 이 스택에 없어 뒤로가기로 안 닫힌다
    private void Update()
    {
        if (openPopups.Count == 0) { return; }
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) { return; }
        if (!keyboard.escapeKey.wasPressedThisFrame) { return; }
        openPopups[openPopups.Count - 1].Hide();
    }

    protected void Setup()
    {
        foreach (var screen in FindObjectsByType<UIScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            screens[screen.GetType()] = screen;
            screen.SetUIManager(this);
            screen.Close();
        }

        foreach (var popup in FindObjectsByType<UIPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            popups[popup.GetType()] = popup;
            popup.SetUIManager(this);
            popup.Hide();
        }

        foreach (var panel in FindObjectsByType<UIPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            panel.SetUIManager(this);
        }
    }

    public void OpenScreen<T>() where T : UIScreen
    {
        foreach (var kvp in screens)
        {
            if (kvp.Key == typeof(T))
                kvp.Value.Open();
            else
                kvp.Value.Close();
        }
    }

    public void ShowPopup<T>() where T : UIPopup
    {
        if (popups.TryGetValue(typeof(T), out var popup))
            popup.Show();
    }

    public void HidePopup<T>() where T : UIPopup
    {
        if (popups.TryGetValue(typeof(T), out var popup))
            popup.Hide();
    }

    public T GetPopup<T>() where T : UIPopup
    {
        if (popups.TryGetValue(typeof(T), out var popup)) { return (T)popup; }
        Debug.LogError($"[UIManager] 등록되지 않은 팝업: {typeof(T).Name}. 씬에 그 팝업이 있는지 확인한다");
        return null;
    }

    public void NotifyPopupShown(UIPopup popup)
    {
        openPopups.Remove(popup);
        openPopups.Add(popup);
    }

    public void NotifyPopupHidden(UIPopup popup)
    {
        openPopups.Remove(popup);
    }

    public void OpenScreen(UIScreen screen)
    {
        foreach (var entry in screens)
        {
            if (entry.Value == screen)
                entry.Value.Open();
            else
                entry.Value.Close();
        }
    }
}
