using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LiveController : MonoBehaviour
{
    public GameObject startbutton;
    public GameObject Background;
    public GameObject ResultUI;
    public GameObject sailium;
    public GameObject backlight;
    int dance = 0;
    int visual = 0;
    int vocal = 0;
    public static int selectedcharacter;
    public ProgressModel[] characters;

    HashSet<GameObject> sailiumcollections = new HashSet<GameObject>();
    public Image Achievement;
    public GameObject Achieved;
    public Text RemainingText;
    public float max;
    public float score = 0;
    public int remainingTurn = 5;
    WaitForSeconds wait = new WaitForSeconds(0.01f);
    float maxscore = 0;
    int maxindex = 0;

    public GameObject[] LiveCharacter=new GameObject[5];
    public GameObject[] CharacterList=new GameObject[5];

    public static bool executingSkills = false;

    void checkPos()
    {
        dance = 0;
        visual = 0;
        vocal = 0;
        bool active = false;
        GameObject[] objs = new GameObject[5];
        for (int i = 0; i < 5; i++) objs[i] = LiveCharacter[i];
        Array.Sort(objs, delegate (GameObject a1, GameObject a2) { return -1*a1.transform.parent.gameObject.GetComponent<RectTransform>().localPosition.y
            .CompareTo(a2.transform.parent.gameObject.GetComponent<RectTransform>().localPosition.y); });
        int depth = 0;
        for (int i=0;i<5;i++)
        {
            CharacterController c = objs[i].GetComponent<CharacterController>();
            string area = c.area;
            if (area == "dance") dance++;
            else if (area == "visual") visual++;
            else if (area == "vocal") vocal++;
            depth++;
            if (c.id == selectedcharacter)
            {
                c.light.SetActive(true);
                active = c.completedActiveSkill;
            }
            else
            {
                c.light.SetActive(false);
            }
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
        max = Common.GetLiveScoreMaxValue();
        executingSkills = false;
        selectedcharacter = 0;
        GameObject[] objs = LiveCharacter;
        listchilds = CharacterList;
        if (Common.characters == null) Common.initCharacters();//Delete On Pro
        for (int i = 0; i < 5; i++)
        {
            CharacterModel mainCharacter = Common.characters[Common.progresses[i].MainCharacterId];
            CharacterModel subCharacter = Common.characters[Common.progresses[i].SupportCharacterId];
            Common.progresses[i].ActiveSkillName = mainCharacter.skillname;
            Common.progresses[i].ActiveSkillParams = mainCharacter.activeparams;
            Common.progresses[i].ActiveSkillScore = mainCharacter.activeskillscore;
            Common.progresses[i].ActiveSkillType = mainCharacter.activetype;
            Common.progresses[i].ActiveSkillDescription = mainCharacter.activedescription;
            Common.progresses[i].BestSkill = mainCharacter.bestskill;
            Common.progresses[i].SupportSkillName = subCharacter.skillname;
            Common.progresses[i].PassiveSkillParams = subCharacter.passiveparams;
            Common.progresses[i].PassiveSkillScore = subCharacter.passiveskillscore;
            Common.progresses[i].PassiveSkillType = subCharacter.passivetype;
            Common.progresses[i].PassiveSkillDescription = subCharacter.passivedescription;
            CharacterController objk = objs[i].GetComponent<CharacterController>();
            objk.id = i;
            objk.characterInf = Common.progresses[i];
            objk.name.text = Common.progresses[i].Name;
            for (int j=0;j<6;j++)
            {
                //Change Here 
                objk.gifsprite.Add(Resources.Load<Sprite>("Images/Live/Gif/"+ characters[i].MainCharacterId+"/ch-"+j));
            }
            objk.initImage();
            if (i == 0) objk.SelectMe();
            //Change HEre
            listchilds[i].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/charactericon/" + characters[i].MainCharacterId);
            if (characters[i].BestSkill == "vocal") listchilds[i].transform.GetChild(2).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Live/Frame_Pink_Edge");
            else if (characters[i].BestSkill == "visual") listchilds[i].transform.GetChild(2).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Live/Frame_Yellow_Edge");
            else listchilds[i].transform.GetChild(2).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Live/Frame_Blue_Edge");
            objs[i].GetComponent<CharacterController>().listchild = listchilds[i];
            objs[i].GetComponent<CharacterController>().connectUI();
            objk.setParamsFont();
            

        }
        float baseheight = Screen.height + 220f;
        int xins = 100;
        float scale = 1.0f;
        int layer = 0;
        for (int y = 80; y < 520; y += 100)
        {
            for (int x = -270; x <= 270; x += xins)
            {
                GameObject instance = Instantiate(sailium, Background.transform);
                instance.transform.parent = Background.transform;
                instance.GetComponent<RectTransform>().anchoredPosition = new Vector2(x,y);
                instance.GetComponent<Sailium>().layer = layer;
                instance.transform.localScale *= scale;
                sailiumcollections.Add(instance);
            }
            layer++;
            xins -= 10;
            scale -= 0.1f;
        }
        backlight.transform.SetSiblingIndex(114514);
    }

    private IEnumerator updateScoreBar(float oldScore, float newScore)
    {
        System.Random random = new System.Random();
        float kb = (float)max / 60.0f;
        float k = 0;
        while (oldScore < newScore)
        {
            oldScore++;
            k++;
            if (k>=kb&& sailiumcollections.Count>0)
            {
                GameObject gameObject = sailiumcollections.ElementAt(random.Next(sailiumcollections.Count));
                gameObject.GetComponent<Sailium>().enableSailium();
                sailiumcollections.Remove(gameObject);
                k = 0;
            }
            Achievement.fillAmount = oldScore / max;
            yield return null;
        }
        if (newScore / max >= 1.0f && !Achieved.active)
        {
            Achieved.SetActive(true);
        }
    }


    private float ApplyActiveSkill(ProgressModel targetInf,ProgressModel characterInf)
    {
        float activescore = 0;
        if (characterInf.ActiveSkillParams == "all")
        {
            activescore += (targetInf.Visual + targetInf.Vocal + targetInf.Dance) * characterInf.ActiveSkillScore;
        }
        else
        {
            if (characterInf.ActiveSkillParams.Contains("visual"))
            {
                activescore += (targetInf.Visual * characterInf.ActiveSkillScore * characterInf.ActiveSkillLevel);

            }
            if (characterInf.ActiveSkillParams.Contains("vocal"))
            {
                activescore += (targetInf.Vocal * characterInf.ActiveSkillScore * characterInf.ActiveSkillLevel);
            }
            if (characterInf.ActiveSkillParams.Contains("dance"))
            {
                activescore += (targetInf.Dance * characterInf.ActiveSkillScore * characterInf.ActiveSkillLevel);
            }
        }
        return activescore;
    }
    private float execActiveSkill(CharacterController characterObj)
    {
        float activescore = 0;
        ProgressModel characterInf = characterObj.characterInf;
        int count = 0;
        if(characterInf.ActiveSkillType == "self")
        {
            activescore += ApplyActiveSkill(characterInf,characterInf);
        }
        else
        {
            GameObject[] objs = LiveCharacter;
            foreach(GameObject obj in objs)
            {
                CharacterController objinf = obj.GetComponent<CharacterController>();
                if (characterInf.ActiveSkillType=="samearea"&&characterObj.area != objinf.area)
                {
                    continue;
                }
                count++;
                activescore += ApplyActiveSkill(objinf.characterInf, characterInf);
            }
        }
        return activescore;
    }

    private void execPassiveSkill(CharacterController characterObj)
    {
        ProgressModel characterInf = characterObj.characterInf;
        GameObject[] objs = LiveCharacter;
        int count = 0;
        for(int i = 0; i < 5;i++)
        {
            CharacterController objcc = objs[i].GetComponent<CharacterController>();
            ProgressModel objinf = objcc.characterInf;
            if ((characterInf.PassiveSkillType.Contains("group")&&characterInf.Group!=objinf.Group) || 
                (characterInf.PassiveSkillType != "all" && !characterInf.PassiveSkillType.Contains("group") && objcc.area != characterObj.area))
            {
                continue;
            }
            count++;
            if (characterInf.PassiveSkillParams.Contains("visual") || characterInf.PassiveSkillParams == "all")
            {
                objinf.Visual *= (characterInf.PassiveSkillScore * characterInf.PassiveSkillLevel);
            }
            if (characterInf.PassiveSkillParams.Contains("vocal") || characterInf.PassiveSkillParams == "all")
            {
                objinf.Vocal *= (characterInf.PassiveSkillScore * characterInf.PassiveSkillLevel);
            }
            if (characterInf.PassiveSkillParams.Contains("dance") || characterInf.PassiveSkillParams == "all")
            {
                objinf.Dance *= (characterInf.PassiveSkillScore * characterInf.PassiveSkillLevel);
            }
            objcc.setParamsFont();
        }
        Debug.Log("Triggered:"+count);
    }

    private IEnumerator execSkillofOnePerson(CharacterController characterObj,int index,bool selected)
    {
        characterObj.executingSkill = true;
        float newscore = score;
        //パッシブスキルの発動
        ProgressModel characterInf = characterObj.characterInf;
        if (RandomArray.Probability(characterInf.PassiveSkillProbability*100.0f))
        {
            listchilds[index].transform.GetChild(1).gameObject.SetActive(true);
            execPassiveSkill(characterObj);
        }
        else
        {
            listchilds[index].transform.GetChild(1).gameObject.SetActive(false);
        }
        float parascore;
        float activeskillscore = 0;
        if (characterObj.area == "visual") parascore = characterInf.Visual;
        else if (characterObj.area == "vocal") parascore = characterInf.Vocal;
        else parascore = characterInf.Dance;
        //アクティブスキルの発動
        if (selected)
        {
            activeskillscore = execActiveSkill(characterObj);
            //testText.text = characterInf.Name + " の " + characterInf.ActiveSkillName;
            //listchilds[index].transform.GetChild(0).gameObject.SetActive(true);
        }
        //スコアの更新
        characterObj.score += parascore + activeskillscore;
        if (characterObj.score > maxscore)
        {
            maxscore = characterObj.score;
            maxindex = index;
        }
        newscore += (parascore+activeskillscore);
        yield return characterObj.jump();
        yield return updateScoreBar(score, newscore);
        score = newscore;
        //yield return new WaitForSeconds(1.0f);
        characterObj.executingSkill = false;
    }

    private IEnumerator execSkills()
    {
        GameObject[] objs = LiveCharacter;
        for (int i = 0; i < 5; i++)
        {
            yield return execSkillofOnePerson(objs[i].GetComponent<CharacterController>(),i,i==selectedcharacter);
        }
        remainingTurn--;
        objs[selectedcharacter].GetComponent<CharacterController>().finishSkill();
        if (remainingTurn > 0)
        {
            RemainingText.text = remainingTurn.ToString();
            executingSkills = false;
        }else if (score >= max)
        {
            //testText.text = "Mission Complete!";
            var resultchanger = ResultUI.GetComponent<UIchanger>();
            resultchanger.Judge_Image_num = 0;
            resultchanger.Chara_Image_num = LiveCharacter[maxindex].GetComponent<CharacterController>().characterInf.MainCharacterId;
            resultchanger.Score_num = (int)score;
            resultchanger.Achievement_num = score / max;
            ResultUI.SetActive(true);
        }
        else
        {
            //testText.text = "Mission Failed!";
            var resultchanger = ResultUI.GetComponent<UIchanger>();
            resultchanger.Judge_Image_num = 1;
            resultchanger.Chara_Image_num = LiveCharacter[maxindex].GetComponent<CharacterController>().characterInf.MainCharacterId;
            resultchanger.Score_num = (int)score;
            resultchanger.Achievement_num = score / max;
            ResultUI.SetActive(true);
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
