using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

public static class SortChildren 
{
    [MenuItem("Tools/Wander Utils/Sort Children")]
    public static void SortChildrenMenuItem()
    {
        Transform root = Selection.activeTransform;
        sortChildren(root);
    }

    private static void sortChildren(Transform root)
    {
        Transform[] children = new Transform[root.childCount];
        for (int i = 0; i < root.childCount; i++)
        {
            children[i] = root.GetChild(i);
            sortChildren(children[i]);
        }

        Array.Sort(children, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        for (int i = 0; i < children.Length; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }

    [MenuItem("Tools/Wander Utils/Normalize Citywalk Name")]
    public static void NormalizeCitywalkNameMenuItem()
    {
        Transform root = Selection.activeTransform;
        normalizeCitywalkName(root);
    }

    private static void normalizeCitywalkName(Transform root)
    {
        Regex r = new Regex(@"(\w+)_region_(\d+)_(\d+)_(\d+)_(.+)");
        Match m = r.Match(root.name);
        if (m.Success)
        {
            string newName = string.Format("{0}_region_{1:D3}_{2:D3}_{3:D4}_{4}",
                m.Groups[1],
                int.Parse(m.Groups[3].Value),
                int.Parse(m.Groups[2].Value),
                int.Parse(m.Groups[4].Value),
                m.Groups[5]
            );

            root.name = newName;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            normalizeCitywalkName(root.GetChild(i));
        }
    }
}
