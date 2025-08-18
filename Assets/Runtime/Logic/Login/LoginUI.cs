using Runtime.Logic.Event;
using SaveWorld;
using UnityEngine;
using UnityEngine.UI;
public class LoginUI : UGuiForm
{
    [SerializeField] private Button loginButton;
    private ProcedurePreLogin procedurePreLogin;
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
        procedurePreLogin.StartLogin();
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        
        procedurePreLogin = userData as ProcedurePreLogin;
    }
}
