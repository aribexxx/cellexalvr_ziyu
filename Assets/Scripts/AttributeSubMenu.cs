﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeSubMenu : MonoBehaviour
{

    public ColorByAttributeButton buttonPrefab;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
    private Color[] colors;
    private List<ColorByAttributeButton> buttons;

    public void Init()
    {
        buttons = new List<ColorByAttributeButton>();
        colors = new Color[22];
        colors[0] = new Color(1, 0, 0);     // red
        colors[1] = new Color(0, 0, 1);     // blue
        colors[2] = new Color(0, 1, 0);     // green
        colors[3] = new Color(1, 1, 0);     // yellow
        colors[4] = new Color(0, 1, 1);     // cyan
        colors[5] = new Color(1, 0, 1);     // magenta
        colors[6] = new Color(1f, 153f / 255f, 204f / 255f);     // pink
        colors[7] = new Color(.6f, 1, .6f);     // lime green
        colors[8] = new Color(.4f, .2f, 1);     // brown
        colors[9] = new Color(1, .6f, .2f);     // orange
        colors[10] = new Color(.87f, 8f, .47f);     // some ugly sand color
        colors[11] = new Color(.3f, .3f, .3f);     // grey
        colors[12] = new Color(.18f, .69f, .54f);     // turquijioutyuoreyourse
        colors[13] = new Color(.84f, .36f, .15f);     // red panda red
        colors[14] = new Color(0, 1, 1);     // cyan
        colors[15] = new Color(1, 0, 1);     // magenta
        colors[16] = new Color(1f, 153f / 255f, 204f / 255f);     // pink
        colors[17] = new Color(.6f, 1, .6f);     // lime green
        colors[18] = new Color(.4f, .2f, 1);     // brown
        colors[19] = new Color(1, .6f, .2f);     // orange
        colors[20] = new Color(.87f, 8f, .47f);     // some ugly sand color
        colors[21] = new Color(.3f, .3f, .3f);     // grey
        gameObject.SetActive(false);
    }

    public void CreateAttributeButtons(string[] attributes)
    {
        if (colors == null)
        {
            Init();
        }
        foreach(ColorByAttributeButton button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-.39f, .77f, .282f);
        }
        for (int i = 0; i < attributes.Length; ++i)
        {
            string attribute = attributes[i];
            ColorByAttributeButton newButton = Instantiate(buttonPrefab, transform);
            newButton.gameObject.SetActive(true);
            newButton.transform.localPosition = buttonPos;
            newButton.SetAttribute(attribute, colors[i]);
            buttons.Add(newButton);
            if ((i + 1) % 4 == 0)
            {
                buttonPos -= buttonPosInc * 3;
                buttonPos += buttonPosNewRowInc;
            }
            else
            {
                buttonPos += buttonPosInc;
            }

        }
    }
}