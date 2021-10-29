using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class Common : MonoBehaviour
{
    public static CharacterModel[] characters;
    public static Sprite[] standImages = new Sprite[34];
    public static ProgressModel[] progresses = new ProgressModel[5];
    public static DendouModel teacher;
    public static string mainstoryid;
    public static int lessonCount = 5;
    public static int progressId;
    public static GameObject loadingCanvas;
    public static GameObject loadingGif;
    public static AudioSource bgmplayer;
    public static Text loadingTips;
    public static List<int> remainingSubstory = new List<int>(); 
    public static string mom = "ママ";
    private static readonly int[] liveScoreMaxValues = { 600, 900, 1200, 2000, 2400, 2800, 4000, 4500 };

    public static void initCharacters()
    {
        string json = Resources.Load<TextAsset>("Common/characters").ToString();
        characters = JsonUtility.FromJson<CommonCharacters>(json).characters;
        for (int i=0;i<32;i++)
        {
            standImages[i] = Resources.Load<Sprite>("Images/standimage/" + characters[i].id);
        }
    }

    public static void initProgress()
    {
        string json = Resources.Load<TextAsset>("Live/testdata").ToString();
        progresses = JsonUtility.FromJson<ProgressData>(json).progresses;
    }

    public static void syncProgress()
    {

    }

    /// <summary>
    /// </summary>
    /// <returns>現在のLiveの目標ハートスコア</returns>
    public static int GetLiveScoreMaxValue()
    {
        try
        {
            int chapter = mainstoryid[0]-'0';
            if (chapter >= 10 || chapter < 1) throw new Exception($"Unexpected mainstoryid: {mainstoryid}");
            Debug.Log($"chapter: {chapter}, ノルマ: {liveScoreMaxValues[chapter - 1]}");
            return liveScoreMaxValues[chapter-1];
        }catch(Exception e)
        {
            Debug.Log(e);
            return 0;
        }
    }

    //通信関連
    public const string api_version = "v2"; //"v1" or "v2" 
    public const string protocol = "https"; //"http" や "https" など 
    public const string hostname = "birdol.herokuapp.com";
    public const string port = "443";
    public const int timeout = 4; //通信タイムアウトの秒数 
    public const bool allowAllCertification = true; //trueの場合、オレオレ証明書を含め全ての証明書を認証し通信する。httpsプロトコル使用時に注意。
    public const string salt = "Ll7Iy0r9zWslDniwgUXeS0KM9xke4zeg"; //固定ソルト

    public static string playerName;
    private const string PLAYERPREFS_PLAYER_NAME = "PLAYER_NAME";
    public static string PlayerName
    {
        get
        {
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = PlayerPrefs.GetString(PLAYERPREFS_PLAYER_NAME);
            }
            return playerName;
        }
        set
        {
            if (playerName != value)
            {
                playerName = value;
                PlayerPrefs.SetString(PLAYERPREFS_PLAYER_NAME, playerName);
                PlayerPrefs.Save();
            }
        }
    }

    private const string TRIGGERED_SUB = "TRIGGERED_SUB";
    public static string TriggeredSubStory
    {
        get
        {
            
            return PlayerPrefs.GetString(TRIGGERED_SUB);
        }
        set
        {
            PlayerPrefs.SetString(TRIGGERED_SUB, value);
            PlayerPrefs.Save();
        }
    }

    public static int homeStandingId = -1;
    private const string PLAYERPREFS_HOMESTANDING_ID = "HOMESTANDING_ID";
    public static int HomeStandingId
    {
        get
        {
            if (homeStandingId == -1)
            {
                homeStandingId = PlayerPrefs.GetInt(PLAYERPREFS_HOMESTANDING_ID);
            }
            return homeStandingId;
        }
        set
        {
            if (homeStandingId != value)
            {
                homeStandingId = value;
                PlayerPrefs.SetInt(PLAYERPREFS_HOMESTANDING_ID, homeStandingId);
                PlayerPrefs.Save();
            }
        }
    }

    //PlayerPrefsに保存
    //ユーザID
    private const string PLAYERPREFS_USER_ID = "PLAYERPREFS_USER_ID";  
    private static uint userID=0;
    public static uint UserID
    {
        get
        {
            if (userID == 0)
            {
                userID = PlayerPrefs.GetUint(PLAYERPREFS_USER_ID);
            }
            return userID;
        }
        set
        {
            if (userID != value)
            {
                userID = value;
                PlayerPrefs.SetUint(PLAYERPREFS_USER_ID, value);
                PlayerPrefs.Save();
            }
        }
    }
    //アクセストークン 
    private const string PLAYERPREFS_ACCESS_TOKEN = "PLAYERPREFS_ACCESS_TOKEN"; 
    private static string accessToken;
    public static string AccessToken
    {
        get
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = PlayerPrefs.GetString(PLAYERPREFS_ACCESS_TOKEN);
            }
            return accessToken;
        }
        set
        {
            if (accessToken != value)
            {
                accessToken = value;
                PlayerPrefs.SetString(PLAYERPREFS_ACCESS_TOKEN, accessToken);
                PlayerPrefs.Save();
            }
        }
    }
    //リフレッシュトークン 
    private const string PLAYERPREFS_REFRESH_TOKEN = "PLAYERPREFS_REFRESH_TOKEN"; 
    private static string refreshToken;
    public static string RefreshToken
    {
        get
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                refreshToken = PlayerPrefs.GetString(PLAYERPREFS_REFRESH_TOKEN);
            }
            return refreshToken;
        }
        set
        {
            if (refreshToken != value)
            {
                refreshToken = value;
                PlayerPrefs.SetString(PLAYERPREFS_REFRESH_TOKEN, refreshToken);
                PlayerPrefs.Save();
            }
        }
    }
    //デバイスID 
    private const string PLAYERPREFS_UUID = "PLAYERPREFS_UUID";
    private static string uuid;
    public static string Uuid
    {
        get
        {
            if (string.IsNullOrEmpty(uuid))
            {
                uuid = PlayerPrefs.GetString(PLAYERPREFS_UUID);
            }
            return uuid;
        }
        set
        {
            if (uuid != value)
            {
                uuid = value;
                PlayerPrefs.SetString(PLAYERPREFS_UUID, uuid);
                PlayerPrefs.Save();
            }
        }
    }
    //セッションID 
    private const string PLAYERPREFS_SESSION_ID = "PLAYERPREFS_SESSION_ID"; 
    private static string sessionID;
    public static string SessionID
    {
        get
        {
            if (string.IsNullOrEmpty(sessionID))
            {
                sessionID = PlayerPrefs.GetString(PLAYERPREFS_SESSION_ID);
            }
            return sessionID;
        }
        set
        {
            if (sessionID != value)
            {
                sessionID = value;
                PlayerPrefs.SetString(PLAYERPREFS_SESSION_ID, sessionID);
                PlayerPrefs.Save();
            }
        }
    }
    //Signup時にサーバから受け取るアカウントID。手動で設定するアカウントIDをデバイスに保存することは現在想定していない。
    private const string PLAYERPREFS_DEFALT_ACCOUNT_ID = "PLAYERPREFS_DEFALT_ACCOUNT_ID";
    private static string defaultAccountID;
    public static string DefaultAccountID { 
        get
        {
            if (string.IsNullOrEmpty(defaultAccountID))
            {
                defaultAccountID = PlayerPrefs.GetString(PLAYERPREFS_DEFALT_ACCOUNT_ID);
            }
            return defaultAccountID;
        }
        set
        {
            defaultAccountID = value;
            PlayerPrefs.SetString(PLAYERPREFS_DEFALT_ACCOUNT_ID, defaultAccountID);
            PlayerPrefs.Save();
        }
    }

    //RSA Key Pair: 公開鍵と秘密鍵
    //private const string PLAYERPREFS_RSA_PUBLIC_KEY = "PLAYERPREFS_RSA_PUBLIC_KEY";  PrivateKeyにPublicKeyも含まれているため保存する必要ない
    private const string PLAYERPREFS_RSA_PRIVATE_KEY = "PLAYERPREFS_RSA_PRIVATE_KEY";
    private static (string privateKey, string publicKey) rsaKeyPair; //(privateKey: 秘密鍵, publicKey: 公開鍵)
    public static (string privateKey, string publicKey) RsaKeyPair
    {
        get
        {
            if (string.IsNullOrEmpty(rsaKeyPair.privateKey) )
            {
                rsaKeyPair.privateKey = PlayerPrefs.GetString(PLAYERPREFS_RSA_PRIVATE_KEY);
                //rsaKeyPair.publicKey = PlayerPrefs.GetString(PLAYERPREFS_RSA_PUBLIC_KEY);
            }
            return rsaKeyPair;
        }
        set
        {
            rsaKeyPair = value;
            PlayerPrefs.SetString(PLAYERPREFS_RSA_PRIVATE_KEY, rsaKeyPair.privateKey);
            //PlayerPrefs.SetString(PLAYERPREFS_RSA_PUBLIC_KEY, rsaKeyPair.publicKey);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// RSA 秘密鍵 公開鍵 生成 
    /// </summary>
    public static (string privateKey, string publicKey) CreateRsaKeyPair()
    {
        int size = 1024;
        RSACryptoServiceProvider csp = new RSACryptoServiceProvider(size);

        string publicKey = csp.ToXmlString(false);
        string privateKey = csp.ToXmlString(true);
        publicKey = StrToBase64Str(publicKey);
        privateKey= StrToBase64Str(privateKey);
        (string privateKey, string publicKey) keyPair = (privateKey: privateKey, publicKey: publicKey);

        Debug.Log($"private_key: { StrFromBase64Str(keyPair.privateKey) }");
        Debug.Log($"public_key: { StrFromBase64Str(keyPair.publicKey) }");

        return keyPair;
    }

    /// <summary> ランダム文字列の生成 </summary>
    /// <param name="len">文字列の長さ</param>
    /// <returns></returns>
    public static string GenerateRondomString(int len)
    {
        const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string t = "";
        for(int i = 0; i < len; i++)
        {
            t += characters[UnityEngine.Random.Range(0, characters.Length)];
        }
        return t;
    }

    public static string StrToBase64Str(string text)
    {
        string str = "";
        try
        {
            str = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return str;
    }
    public static string StrFromBase64Str(string text)
    {
        string str = "";
        try
        {
            str = Encoding.UTF8.GetString( Convert.FromBase64String(text) );
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return str;
    }

    void Start()
    {
        initCharacters();
    }
}
