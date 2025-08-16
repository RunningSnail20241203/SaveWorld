using Runtime.Logic.Event;
using SaveWorld;
using UnityEngine;
using UnityEngine.UI;
public class LoginUI : UGuiForm
{
    [SerializeField] private Button loginButton;
    private void Awake()
    {
        loginButton.onClick.AddListener(OnLoginBtnClick);
    }

    private void OnDestroy()
    {
        loginButton.onClick.RemoveListener(OnLoginBtnClick);
    }

    private void OnLoginBtnClick()
    {
        GameEntry.Event.Fire(UIClickLoginBtnEventArgs.EventID, new UIClickLoginBtnEventArgs());
    }
}
