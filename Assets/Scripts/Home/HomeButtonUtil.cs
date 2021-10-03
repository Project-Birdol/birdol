using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HomeButtonUtil : MonoBehaviour
{

    public class DialogText
    {
        public string[] text;
    }

    public GameObject Gallery;
    public GameObject Ikusei;
    public GameObject CharacterImage;
    public GameObject Dialog;
    public GameObject Back;
    public GameObject Option;
    int PrevIndex = -1;
    public Text dialogtext;
    public Image CharacterImageSplite;
    public CharacterModel charactermodel;
    public DialogText DialogTextData;
    public int tempteacherid;


    private void Start()
    {
        Dialog.SetActive(false);
        CharacterImage.SetActive(true);

        //ホーム画面に表示するキャラの取得
        //HomeData/HomeData.jsonにデータは保存
        string json_tmp = Resources.Load<TextAsset>("HomeData/HomeData").ToString();
        charactermodel = JsonUtility.FromJson<CharacterModel>(json_tmp);
        CharacterImageSplite.sprite = Resources.Load<Sprite>("Images/Home/" + charactermodel.name);
        Debug.Log(charactermodel.name);

        //Dialogに表示するテキストの取得
        json_tmp = Resources.Load<TextAsset>("HomeData/CharaText/" + charactermodel.name).ToString();
        Debug.Log(json_tmp);
        DialogTextData = JsonUtility.FromJson<DialogText>(json_tmp);
        Debug.Log(DialogTextData.text[0]);

    }

    public void onButtonPressedGallery()
    {
        Debug.Log("Pushed Gallery");
        Common.loadingCanvas.SetActive(true);
        Manager.manager.StateQueue((int)gamestate.Gallery);

    }

    public void onButtonPressedDendou()
    {
        Debug.Log("Pushed Gallery");
        Common.loadingCanvas.SetActive(true);
        Manager.manager.StateQueue((int)gamestate.CompletedCharacters);

    }

    public void onButtonPressedIkusei()
    {
        Debug.Log("Pushed Ikusei");
        Common.loadingCanvas.SetActive(true);
        Manager.manager.StateQueue((int)gamestate.Story);

    }

    public void onButtonPressedCharacterImage()
    {
        Debug.Log("Pushed CharaImage");
        CancelInvoke();
        DialogTextChanger();
        Dialog.SetActive(true);
        Invoke("DialogCloser", 6);
    }
        
    public void onButtonPressedDialog()
    {
        Debug.Log("Pushed Dialog");
        DialogCloser();


    }
    public void onButtonPressedBack()
    {
        Debug.Log("Pushed Back");

    }

    public void onButtonPressedOption()
    {
        Debug.Log("Pushed Option");

    }

    void DialogTextChanger()
    {
        int rand_num, TextMaxSize;
        do
        {
            TextMaxSize = DialogTextData.text.Length;
            rand_num = (int)(UnityEngine.Random.value * TextMaxSize);
        } while (rand_num == PrevIndex || rand_num >= TextMaxSize);

        PrevIndex = rand_num;
        dialogtext.text = DialogTextData.text[rand_num];
    }

    void DialogCloser ()
    {
        Dialog.SetActive(false);
    }
}
