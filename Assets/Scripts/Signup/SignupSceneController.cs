using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SignupWebClient))]
public class SignupSceneController : SceneVisor
{
    [Header("SignUp Web Client")]
    [SerializeField] private SignupWebClient signupWebClient;

    [Header("Input")]
    [SerializeField] InputField usernameInputField;
    [SerializeField] Button signupButton;

    [Header("Display")]
    [SerializeField] GameObject AlertUI;
    [SerializeField] Text AlertText;

    private bool isConnectionInProgress = false;


    private void Start()
    {
        /*TODO; here or elsewhere
        ①TokenAuthorize to update session: if success, jumpt to game. 
        ②AccountLogin to update accessToken: if success, try ① once. 
        ③If Failed ① or ②, the user is new to this game. The user need to signup! 
        */

        SetUpButtonEvent();
    }

    private void SetUpButtonEvent()
    {
        //Signup
        signupButton.onClick.AddListener(() => {
            OnLoginButtonClicked();
        });
        //username 中の文字としてふさわしくなさそうなものを削除する。 
        usernameInputField.onEndEdit.AddListener((s) =>
        {
            usernameInputField.text = System.Text.RegularExpressions.Regex.Replace(usernameInputField.text, @"\n|\r|\s|\t|\v", string.Empty); 
        });
    }

    private void OnLoginButtonClicked()
    {
        if (isConnectionInProgress) return;
        StartCoroutine(Login());
    }

    /// <summary>
    /// Login Request 
    /// </summary>
    /// <returns></returns>
    private IEnumerator Login()
    {
        isConnectionInProgress = true;
        string username = usernameInputField.text;
        string password = Common.GenerateRondomString(64);
        string _uuid = GenerateGUID();
        signupWebClient.SetData(username, password, _uuid);

        //データチェックをサーバへ送信する前に行う。
        if (signupWebClient.CheckRequestData()==false)
        {
            AlertText.text = signupWebClient.message;
            Debug.Log(signupWebClient.message);
            yield return StartCoroutine(ShowForWhileCoroutine(2.0f, AlertUI));
            isConnectionInProgress = false;
            yield break;
        }

        AlertUI.SetActive(true);
        AlertText.text = "通信中..."; 
        float conn_start = Time.time;
        yield return StartCoroutine(signupWebClient.Send());
        float conn_end = Time.time;
        if (conn_end - conn_start > 0) yield return new WaitForSeconds(0.5f); //ユーザ側視点としては、通信時間としてに必ず最低0.5秒はかかるとする。さもなくば「通信中...」の表示がフラッシュみたいになって気持ち悪い気がする。
        AlertUI.SetActive(false);

        //処理
        if (signupWebClient.isSuccess == true && signupWebClient.isInProgress == false)
        {
            //成功した時
            SignupWebClient.SignupResponseData lrd = (SignupWebClient.SignupResponseData)signupWebClient.data;
            Debug.Log("ParsedResponseData: \n" + lrd.ToString());
            if (signupWebClient.isSignupSuccess)
            {
                Common.DefaultPassword = password;
                Common.Uuid = _uuid;
                Debug.Log($"Playerprefs Saved.\nPassword: {password}, UUID: {_uuid}");
                
                AlertText.text = signupWebClient.message;
                yield return StartCoroutine(ShowForWhileCoroutine(2.0f, AlertUI));                
                OnSignupSuccess();
            }
            else
            {
                AlertText.text = signupWebClient.message;
                yield return StartCoroutine(ShowForWhileCoroutine(2.0f, AlertUI));
            }
        }
        else
        {
            //失敗した時
            AlertText.text = $"<color=\"red\">{signupWebClient.message}</color>";
            yield return StartCoroutine(ShowForWhileCoroutine(2.0f, AlertUI));
        }

        isConnectionInProgress = false;
        yield break;
    }

    //Show (GameObject)gm for (float)duration seconds
    private IEnumerator ShowForWhileCoroutine(float duration, GameObject gm)
    {
        gm.SetActive(true);
        yield return new WaitForSeconds(duration);
        gm.SetActive(false);
        yield break;
    }

    /// <summary>
    /// アカウント登録に成功したときの動作。例えば、Gameシーンへの遷移など。
    /// </summary>
    private void OnSignupSuccess()
    {

    }

    /// <summary>
    /// Generate UUID
    /// </summary>
    private string GenerateGUID(){
        System.Guid guid = System.Guid.NewGuid();
        return guid.ToString();
    }
}
