using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LessonController : MonoBehaviour
{
    public GameObject startbutton;
    public GameObject Background;
    public GameObject backlight;
    int dance = 0;
    int visual = 0;
    int vocal = 0;
    public static int selectedcharacter;
    public ProgressModel[] characters;
    public Text remainingText;
    WaitForSeconds wait = new WaitForSeconds(0.01f);
    public static bool executingSkills = false;
    TeacherController teacher;
    public GameObject[] LiveCharacter = new GameObject[6];
    public GameObject[] CharacterList = new GameObject[5];

    void checkPos()
    {
        dance = 0;
        visual = 0;
        vocal = 0;
        bool active = false;
        GameObject[] objs= new GameObject[6];
        for (int i = 0; i < 6; i++) objs[i] = LiveCharacter[i];
        Array.Sort(objs, delegate (GameObject a1, GameObject a2) { return -1*a1.transform.parent.gameObject.GetComponent<RectTransform>().localPosition.y
            .CompareTo(a2.transform.parent.gameObject.GetComponent<RectTransform>().localPosition.y); });
        int depth = 0;
        for (int i=0;i<6;i++)
        {
            if (objs[i].name=="TeacherCharacter") continue;
            string area = objs[i].GetComponent<LessonCharacterController>().area;
            if (area == "dance") dance++;
            else if (area == "visual") visual++;
            else if (area == "vocal") vocal++;
            depth++;
            objs[i].transform.parent.gameObject.transform.SetSiblingIndex(i);
        }
        if ((dance <= 2 && visual <= 2 && vocal <= 2)&&!active)
        {
            startbutton.SetActive(true);
        }
        else
        {
            startbutton.SetActive(false);
        }
    }

    private GameObject[] listchilds;


    void initLiveStage()
    {
        executingSkills = false;
        selectedcharacter = 0;
        remainingText.text = Common.lessonCount.ToString();
        GameObject[] objs = LiveCharacter;
        listchilds = CharacterList;
        if (Common.characters == null) Common.initCharacters();//Delete On Pro
        for (int i = 0; i < 5; i++)
        {
            CharacterModel mainCharacter = Common.characters[characters[i].MainCharacterId];
            CharacterModel subCharacter = Common.characters[characters[i].SupportCharacterId];
            LessonCharacterController objk = objs[i].GetComponent<LessonCharacterController>();
            objk.id = i;
            objk.name.text = Common.progresses[i].Name;
            for (int j=0;j<6;j++)
            {
                objk.gifsprite.Add(Resources.Load<Sprite>("Images/Live/Gif/"+mainCharacter.id+"/ch-"+j));
            }
            objk.initImage();
            if (i == 0) objk.SelectMe();
            listchilds[i].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/charactericon/" + characters[i].MainCharacterId);
            if (characters[i].BestSkill == "vocal") listchilds[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Live/Frame_Pink_Edge");
            else if (characters[i].BestSkill == "visual") listchilds[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Live/Frame_Yellow_Edge");
            else listchilds[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Live/Frame_Blue_Edge");
            objk.connectUI();
            objk.setParams();
        }
        //For Test
        DendouModel dendouModel = new DendouModel();
        dendouModel.MainCharacterId = 4;
        dendouModel.SupportCharacterId = 4;
        dendouModel.Name = Common.characters[4].name;
        dendouModel.Vocal = Common.characters[4].vocal;
        dendouModel.Visual = Common.characters[4].visual;
        dendouModel.Dance = Common.characters[4].dance;
        //Init teacher
        teacher = objs[5].GetComponent<TeacherController>();
        teacher.name.text = dendouModel.Name;
        teacher.initPos();
        for (int j = 0; j < 6; j++)
        {
            teacher.gifsprite.Add(Resources.Load<Sprite>("Images/Live/Gif/" + dendouModel.MainCharacterId + "/ch-" + j));
        }
        teacher.initImage();
        backlight.transform.SetSiblingIndex(114514);
    }

    

    private IEnumerator execSkillofOnePerson(LessonCharacterController characterObj,int index)
    {
        characterObj.executingSkill = true;
        int score = 0;
        if(characterObj.area==teacher.area)score = RandomArray.GetRandom(new int[] { 0,1,1,1,2,2,2,3,3,3 });
        else score = RandomArray.GetRandom(new int[] { 0, 1, 2 });
        if (characterObj.area == "visual") Common.progresses[index].Visual+=score;
        else if (characterObj.area == "vocal") Common.progresses[index].Vocal += score;
        else Common.progresses[index].Dance += score;
        characterObj.setParams();
        characterObj.executingSkill = false;
        yield return characterObj.jump();
    }

    private IEnumerator execSkills()
    {
        GameObject[] objs = LiveCharacter;
        for (int i = 0; i < 5; i++)
        {
            yield return execSkillofOnePerson(objs[i].GetComponent<LessonCharacterController>(),i);
        }
        Common.lessonCount--;
        remainingText.text = Common.lessonCount.ToString();
        UpdateCharacterWebClient characterWebClient = new UpdateCharacterWebClient(WebClient.HttpRequestMethod.Put, $"/api/{Common.api_version}/gamedata/character");
        characterWebClient.SetData();
        UpdateMainStoryWebClient storyWebClient = new UpdateMainStoryWebClient(WebClient.HttpRequestMethod.Put, $"/api/{Common.api_version}/gamedata/story");
        yield return characterWebClient.Send();
        if (Common.lessonCount > 0)
        {
            executingSkills = false;
            storyWebClient.SetData(Common.mainstoryid, Common.lessonCount);
            yield return storyWebClient.Send();
        }
        else
        {
            //Change Scene
            Common.loadingCanvas.SetActive(true);
            Common.mainstoryid = Common.mainstoryid.Replace("a", "b");
            storyWebClient.SetData(Common.mainstoryid, 5);
            storyWebClient.sceneid = (int)gamestate.Story;
            yield return storyWebClient.Send();
        }
    }

    public void onStartButtonClick()
    {
        executingSkills = true;
        startbutton.SetActive(false);
        StartCoroutine(execSkills());
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        if (Common.characters == null) Common.initCharacters();//Test Only
        initLiveStage();
        checkPos();
    }

    void Update()
    {
        if(!executingSkills)checkPos();
    }
}