using System;
using System.Net.Mail;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoginWebClient: WebClient 
{
    [Header("Login Information")]
    [SerializeField] protected LoginRequestData loginRequestData;

    public bool isLoginSuccess { get; private set; } //ログインが成功したか否か。通信成功の後にチェックする。 

    /// <summary>
    /// Login Request Data: send to Server
    /// </summary>
    [Serializable]
    public struct LoginRequestData
    {
        [SerializeField] public string email;
        [SerializeField] public string password;
        [SerializeField] public string device_id;

        /// <summary>
        /// COnstructor
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="device_id"></param>
        public LoginRequestData(string email, string password, string device_id)
        {
            this.email = email;
            this.password = password;
            this.device_id = device_id;
        }
    }

    /// <summary>
    /// Login Response Data: receive from Server
    /// </summary>
    [Serializable]
    public struct LoginResponseData
    {
        [SerializeField] public string error;
        [SerializeField] public string result;
        [SerializeField] public int user_id;
        [SerializeField] public string access_token;
    }
    [Serializable]
    public class Auth
    {
        [SerializeField] public string user_id;
        [SerializeField] public string access_token;
        [SerializeField] public string device_id;
    }

    /// <summary>
    /// Constructor:  
    /// </summary>
    /// <param name="requestMethod"></param>
    /// <param name="path">default "/"</param>
    public LoginWebClient(HttpRequestMethod requestMethod, string loginPath) : base(requestMethod, loginPath)
    {
    }

    /// <summary>
    /// Constructor: requestMethod to $"(hostname}:{port}{loginPath}" with loginRequestData 
    /// </summary>
    /// <param name="loginRequestData"></param>
    /// <param name="requestMethod"></param>
    /// <param name="path">default "/"</param>
    public LoginWebClient(LoginRequestData loginRequestData, HttpRequestMethod requestMethod, string loginPath): base(requestMethod, loginPath)
    {
        this.loginRequestData = loginRequestData;
    }

    /// <summary>
    /// Constructor: requestMethod to $"(hostname}:{port}{loginPath}" with loginRequestData 
    /// </summary>
    /// <param name="requestMethod"></param>
    /// <param name="hostname"></param>
    /// <param name="port"></param>
    /// <param name="path">default "/"</param>
    public LoginWebClient( string email, string password, string device_id, HttpRequestMethod requestMethod, string loginPath) : base(requestMethod, loginPath)
    {
        SetData(email, password, device_id);
    }

    /// <summary>
    /// Setdata 
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="device_id"></param>
    public void SetData(string email, string password, string device_id)
    {
        this.loginRequestData = new LoginRequestData(email, password, device_id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lrd"></param>
    /// <returns>true if response data is correctry parsed</returns>
    protected bool CheckResponseData(LoginResponseData lrd)
    {
        return !string.IsNullOrEmpty(lrd.result) || !string.IsNullOrEmpty(lrd.error) || !string.IsNullOrEmpty(lrd.access_token);
    }

    /// <summary>
    /// </summary>
    /// <returns>if request data is appropriate or not</returns>
    public override bool CheckRequestData()
    {
        bool ok = true;
        if (this.loginRequestData.email.Length > ConnectionModel.EMAIL_LENGTH_MAX || this.loginRequestData.email.Length< ConnectionModel.EMAIL_LENGTH_MIN)
        {
            ok = false;
            this.message = $"不適切なメールアドレスです。\n{ConnectionModel.EMAIL_LENGTH_MIN}文字から{ConnectionModel.EMAIL_LENGTH_MAX}文字で入力してください。";
        }else if (this.loginRequestData.password.Length > ConnectionModel.PASSWORD_LENGTH_MAX || this.loginRequestData.password.Length< ConnectionModel.PASSWORD_LENGTH_MIN)
        {
            ok = false;
            this.message = $"不適切なパスワードです。\n{ConnectionModel.PASSWORD_LENGTH_MIN}文字から{ConnectionModel.PASSWORD_LENGTH_MAX}文字で入力してください。";
        }
        else
        {
            try
            {
                new MailAddress(this.loginRequestData.email);
            }
            catch
            {
                ok = false;
                this.message = "不適切なメールアドレスです。\n間違っていないか確認してください。";
            }
        }

        return ok;
    }

    /// <summary>
    /// Setup Web Request Data 
    /// </summary>
    /// <returns></returns>
    protected override void HandleSetupWebRequestData(UnityWebRequest www)
    {
        isLoginSuccess = false;
        try
        {
            this.loginRequestData.password = Hash(this.loginRequestData.password);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            this.message = "このパスワードは使用できません。";
            throw;
        }
        byte[] postData = System.Text.Encoding.UTF8.GetBytes( JsonUtility.ToJson(this.loginRequestData) + "}");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(postData);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
    }


    /// <summary>
    /// HandleSuccessData: 通信に成功した時にLoginクライアントが行う処理
    /// dataに値を保存 
    /// </summary>
    /// <param name="response">received data</param>
    /// <returns></returns>
    protected override void HandleSuccessData(string response)
    {
        this.data = JsonUtility.FromJson<LoginResponseData>(response);
        LoginResponseData lrd = (LoginResponseData)this.data;
        if (CheckResponseData(lrd)!=true)
        {
            this.message = "サーバーから不適切な値が送信されました。";
            this.isSuccess = false;
        }
        else
        {
            if (lrd.result == "success")
            {
                this.message = "ログインに成功しました。";
                OnLoginSuccess(lrd.user_id,lrd.access_token);
            }
            else
            {
                if (!string.IsNullOrEmpty(lrd.error)) this.message = lrd.error;
            }
        }
    }

    /// <summary>
    /// HandleErrorData: 通信に失敗した時にLoginクライアントが行う処理
    /// </summary>
    protected override void HandleErrorData(string error)
    {
        this.message = $"通信に失敗しました。";
        Debug.Log($"error: \n{error}");
    }

    /// <summary>
    /// HandleInProgressData: 通信に途中だった時にLoginクライアントが行う処理 
    /// </summary>
    protected override void HandleInProgressData()
    {
        this.message = "通信中です。"; 
        Debug.LogError("Unexpected UnityWebRequest Result");
    }


    /// <summary>
    /// ログイン成功した時の動作。クライアント側としてデバイスへのデータ保存などを行う。
    /// </summary>
    /// <param name="user_id"></param>
    /// <param name="access_token"></param>
    private void OnLoginSuccess(int user_id, string access_token)
    {
        isLoginSuccess = true;
        Debug.Log($"user_id: {user_id}, access_token: {access_token}\n<color=\"red\">TO DO: デバイスに保存する。</color>");
    }
}
