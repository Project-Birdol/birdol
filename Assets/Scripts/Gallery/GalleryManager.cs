﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* 
Todo:
キャラごとの説明文はどう保持するか
戻るボタンなくね？　下にスペースを作る
 */

public class GalleryManager : MonoBehaviour
{
    private const int ARR_SIZE = 16;
    // 図鑑に表示されるバードルのリスト
    private CharacterModel[] characters = new CharacterModel[ARR_SIZE];
    private bool[] isUnlocked = new bool[ARR_SIZE];

    [SerializeField]
    private GameObject rowNodeOriginal;

    // 図鑑の初期化
    private void InitList()
    {
        // 仮データ用意
        for (int i = 0; i < ARR_SIZE; i++)
        {
            var tmp = new CharacterModel();
            tmp.id = i;
            tmp.name = "No Name";

            characters[i] = tmp;
        }
        characters[0].name = "シジュウカラ";
        isUnlocked[0] = true;
        isUnlocked[1] = true;

        rowNodeOriginal.SetActive(true);
        GameObject content = rowNodeOriginal.transform.parent.gameObject;

        // ceil(キャラの数/2)だけRowNodeをScroll ViewのContent配下に生成する
        int Nodes = (ARR_SIZE + 1) / 2;

        for (int i = 0; i < Nodes-1; i++)
        {
            GameObject row = Instantiate(rowNodeOriginal);
            row.transform.parent = rowNodeOriginal.transform.parent;
            row.transform.localScale = rowNodeOriginal.transform.localScale;
        }

        // 顔アイコンと名前を各Nodeにセット
        for (int i = 0; i < Nodes; i++) {
            GameObject row = content.transform.GetChild(i).gameObject;
            GameObject left = row.transform.Find("CharaNodeLeft").gameObject;
            GameObject right = row.transform.Find("CharaNodeRight").gameObject;

            // キャラ数が奇数なら一番下のRowNodeのCharaNodeRightをinvisibleに
            if (ARR_SIZE % 2 == 1 && i == Nodes-1) right.SetActive(false);

            SetCharacterData(left, 2*i);
            SetCharacterData(right, 2*i+1);
        }
    }

    private CharacterModel GetCharacterFromId(int id) {
        foreach (var c in characters) {
            if (id == c.id) return c;
        }
        return null;
    }

    private const string galleryPathBase = "Images/gallery/";
    private const string noImagePath = galleryPathBase + "face/noimage";  // face未実装キャラクター用
    private const string lockedPath = galleryPathBase + "face/locked";  // 未解禁キャラクター用
    private void SetCharacterData(GameObject node, int id) {
        GameObject faceObject = node.transform.Find("Panel/Face").gameObject;
        GameObject nameObject = node.transform.Find("Panel/Text").gameObject;

        CharacterModel model = GetCharacterFromId(id);

        Image faceImage = faceObject.GetComponent<Image>();
        Sprite faceSprite = Resources.Load<Sprite>("Images/gallery/face/" + id);
        Sprite lockedSprite = Resources.Load<Sprite>(lockedPath);

        Text nameText = nameObject.GetComponent<Text>();

        if (faceSprite == null) {
            faceSprite = Resources.Load<Sprite>(noImagePath);
        }

        if (!isUnlocked[id]) {
            faceImage.sprite = lockedSprite;
            nameText.text = "???";
        }
        else {
            faceImage.sprite = faceSprite;
            nameText.text = model.name;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
