﻿using CellexalVR.AnalysisLogic.H5reader;
using CellexalVR.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// Class that handles the reading of attribute/meta files input.
    /// </summary>
    public class AttributeReader : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        //summertwerk
        /// <summary>
        /// Reads all attributes from current h5 file
        /// </summary>
        public IEnumerator H5ReadAttributeFilesCoroutine(H5Reader h5Reader)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            List<string> available_attributes = new List<string>();


            foreach (string attr in h5Reader.attributes)
            {
                print("reading attribute " + attr);

                while (h5Reader.busy)
                    yield return null;

                StartCoroutine(h5Reader.GetAttributes(attr));

                while (h5Reader.busy)
                    yield return null;


                string[] attrs = h5Reader._attrResult;
                string[] cellNames = h5Reader.index2cellname;

                for (int j = 0; j < cellNames.Length; j++)
                {
                    string cellName = cellNames[j];

                    string attribute_name = attr + "@" + attrs[j];
                    int index_of_attribute;
                    if (!available_attributes.Contains(attribute_name))
                    {
                        available_attributes.Add(attribute_name);
                        index_of_attribute = available_attributes.Count - 1;
                    }
                    else
                    {
                        index_of_attribute = available_attributes.IndexOf(attribute_name);
                    }


                    referenceManager.cellManager.AddAttribute(cellName, attribute_name,
                        index_of_attribute % CellexalConfig.Config.SelectionToolColors.Length);
                    if (j % 500 == 0)
                    {
                        yield return null;
                    }
                }
            }

            referenceManager.attributeSubMenu.CreateButtons(available_attributes.ToArray());

            referenceManager.cellManager.Attributes = available_attributes;
            for (int i = CellexalConfig.Config.SelectionToolColors.Length;
                i < referenceManager.cellManager.Attributes.Count;
                i++)
            {
                referenceManager.settingsMenu.AddSelectionColor();
            }

            referenceManager.settingsMenu.unsavedChanges = false;
            stopwatch.Stop();
            referenceManager.inputReader.attributeFileRead = true;
            CellexalLog.Log("h5 read attributes in " + stopwatch.Elapsed.ToString());
        }


        /// <summary>
        /// Reads an attribute file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public IEnumerator ReadAttributeFilesCoroutine(string path)
        {
            // Read the each .meta.cell file
            // The file format should be
            //              TYPE_1  TYPE_2  ...
            //  CELLNAME_1  [0,1]   [0,1]
            //  CELLNAME_2  [0,1]   [0,1]
            // ...
            yield return null;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            string[] metaCellFiles = Directory.GetFiles(path, "*.meta.cell");
            foreach (string metaCellFile in metaCellFiles)
            {
                FileStream metaCellFileStream = new FileStream(metaCellFile, FileMode.Open);
                StreamReader metaCellStreamReader = new StreamReader(metaCellFileStream);

                // first line is a header line
                string header = metaCellStreamReader.ReadLine();
                if (header != null)
                {
                    string[] attributeTypes = header.Split('\t');
                    string[] actualAttributeTypes = new string[attributeTypes.Length - 1];
                    for (int i = 1; i < attributeTypes.Length; ++i)
                    {
                        actualAttributeTypes[i - 1] = attributeTypes[i];
                    }

                    while (!metaCellStreamReader.EndOfStream)
                    {
                        string line = metaCellStreamReader.ReadLine();
                        if (line == "")
                            continue;

                        if (line != null)
                        {
                            string[] words = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            string cellName = words[0];
                            for (int j = 1; j < words.Length; ++j)
                            {
                                if (words[j] == "1")
                                    referenceManager.cellManager.AddAttribute(cellName, attributeTypes[j],
                                        (j - 1) % CellexalConfig.Config.SelectionToolColors.Length);
                            }
                        }
                    }

                    metaCellStreamReader.Close();
                    metaCellFileStream.Close();
                    referenceManager.cellManager.Attributes = new List<string>(); //actualAttributeTypes.ToList();
                    referenceManager.attributeSubMenu.CreateButtons(actualAttributeTypes);
                }

                int nrOfAttributes = referenceManager.cellManager.Attributes.Count;
                int nrOfSelToolColors = CellexalConfig.Config.SelectionToolColors.Length;
                if (nrOfAttributes > nrOfSelToolColors)
                {
                    referenceManager.settingsMenu.AddSelectionColors(nrOfAttributes - nrOfSelToolColors);
                    referenceManager.settingsMenu.unsavedChanges = false;
                }
            }

            stopwatch.Stop();
            referenceManager.inputReader.attributeFileRead = true;
            CellexalLog.Log("read attributes in " + stopwatch.Elapsed.ToString());

        }
    }
}
