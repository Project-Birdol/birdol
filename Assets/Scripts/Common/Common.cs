using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Common : MonoBehaviour
{
    public static CharacterModel[] characters;

    public static void initCharacters()
    {
        string json = Resources.Load<TextAsset>("common/characters").ToString();
        CommonCharacters result = JsonUtility.FromJson<CommonCharacters>(json);
        characters = result.characters;
    }

    //通信関連
    public const string protocol = "http"; //"http" や "https" など 
    public const string hostname = "localhost";
    public const string port = "80";
    public const bool allowAllCertification = true; //trueの場合、オレオレ証明書を含め全ての証明書を認証し通信する。httpsプロトコル使用時に注意。
    public const string salt = "Ll7Iy0r9zWslDniwgUXeS0KM9xke4zeg"; //固定ソルト

    void Start()
    {
        initCharacters();
    }
}