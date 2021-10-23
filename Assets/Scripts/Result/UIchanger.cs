using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIchanger : MonoBehaviour{
    public GameObject Score_Text; //スコアテキストのUI指定
    public int Score_num; //スコアに表示する値(6桁以下の整数)
    public int characterId;
    public GameObject Judge_Image; //判定用画像のUI指定
    public GameObject Chara_Image; //立ち絵画像のUI指定
    public Sprite[] Judge_Sprites; //判定用の画像の配列
    public Sprite[] Chara_Sprites; //立ち絵の画像の配列
    public int Judge_Image_num; //表示する判定用画像の配列のインデックスを設定
    public int Chara_Image_num; //表示する立ち絵画像の配列のインデックスを設定
    public GameObject Achievement_Image; //到達度ゲージ用画像のUI指定
    public GameObject Achievement_Text; //到達度テキストのUI指定
    public float Achievement_num; //到達度(0~1)
    public GameObject Score_Image; //スコア用画像のUI指定
    public Sprite[] Score_Sprites; //スコア用画像の配列

    void Start(){ //Start時にUIを全て変更
        Score_Text.GetComponent<Text>().text = Score_num.ToString("000,000");
        Judge_Image.GetComponent<Image>().sprite = Judge_Sprites[Judge_Image_num];
        //Chara_Image.GetComponent<Image>().sprite = Chara_Sprites[Chara_Image_num];
        Chara_Image.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/standimage/" + Chara_Image_num);
        Achievement_Image.GetComponent<Image>().fillAmount = Achievement_num;
        Achievement_Text.GetComponent<Text>().text = Achievement_num.ToString("P0");
        Score_Image.GetComponent<Image>().sprite = Score_Sprites[Judge_Image_num];
    }

    public void onClick(){
        Common.loadingCanvas.SetActive(true);
        Common.loadingGif.GetComponent<GifPlayer>().index = 0;
        Common.loadingGif.GetComponent<GifPlayer>().StartGif();
        if (Judge_Image_num==0)
        {   //Success
            Common.mainstoryid = Common.mainstoryid.Replace("b", "c");
            UpdateMainStoryWebClient webClient = new UpdateMainStoryWebClient(WebClient.HttpRequestMethod.Put, $"/api/{Common.api_version}/gamedata/story");
            Common.lessonCount = 5;
            webClient.SetData(Common.mainstoryid, Common.lessonCount);
            webClient.sceneid = (int)gamestate.Story;
            StartCoroutine(webClient.Send());
        }
        else
        {
            //Failed
            FinishProgressWebClient webClient = new FinishProgressWebClient(WebClient.HttpRequestMethod.Put, $"/api/{Common.api_version}/gamedata/complete");
            webClient.SetData();
            webClient.sceneid = (int)gamestate.Failed;
            StartCoroutine(webClient.Send());
        }
    }

   
}
