﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;


class HelperTool : MonoBehaviour
{

    public TextMeshPro textMesh;
    private int numColliders = 0;
    private Dictionary<string, string> descriptions = new Dictionary<string, string>();

    private void Start()
    {
        ReadDescriptionFile(Directory.GetCurrentDirectory() + "\\Assets\\descriptions.txt");
    }

    private void OnTriggerEnter(Collider other)
    {
        numColliders++;
        print(other.gameObject.name);
        if (descriptions.ContainsKey(other.tag))
        {
            textMesh.text = descriptions[other.tag];
        }
        else if (descriptions.ContainsKey(other.gameObject.name))
        {
            textMesh.text = descriptions[other.gameObject.name];
        }
    }

    private void ReadDescriptionFile(string filepath)
    {
        // The file format should be
        // [KEY]:[VALUE]
        // [KEY]:[VALUE]
        // ...

        string[] lines = File.ReadAllLines(filepath);
        if (lines.Length == 0)
        {
            Debug.LogWarning("No description file found at " + Directory.GetCurrentDirectory());
            return;
        }
        foreach (string line in lines)
        {
            if (line.Length == 0)
                continue;
            // comments in the file start with #
            if (line[0] != '#')
            {
                if (line.Substring(0, 4).Equals("TAG_", StringComparison.Ordinal))
                {
                    var colonIndex = line.IndexOf(":");
                    string tagName = line.Substring(4, colonIndex - 4);
                    descriptions[tagName] = line.Substring(colonIndex + 1);
                }
                else
                {
                    string[] splitString = line.Split(new char[] { ':' }, 2);
                    descriptions[splitString[0]] = splitString[1];
                }
            }
        }
    }

}
