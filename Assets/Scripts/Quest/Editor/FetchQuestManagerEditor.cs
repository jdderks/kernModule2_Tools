﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Events;

//TODO: Look up GUIContent flexible width



[CustomEditor(typeof(FetchQuestManager))]
public class FetchQuestManagerEditor : Editor
{
    FetchQuestManager manager;

    SerializedProperty quest;


    ReorderableList questList;

    static Dictionary<int, int> heights = new Dictionary<int, int>();


    private void OnEnable()
    {
        manager = (FetchQuestManager)target;


        quest = serializedObject.FindProperty("Quests");
        questList = new ReorderableList(serializedObject, quest, true, true, true, true);

        questList.drawElementCallback = DrawQuestListItems;
        questList.drawHeaderCallback = DrawQuestHeader;
        questList.elementHeightCallback = DrawHeight;
    }
    public override void OnInspectorGUI()
    {
        //This doesn't work yet :( so you're going to have to still use the field from the base.OnInspectorGUI for now...
        manager.Player = (GameObject)EditorGUILayout.ObjectField("The player gameobject", manager.Player, typeof(GameObject), true);


        base.OnInspectorGUI();

        serializedObject.Update();
        questList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    void DrawQuestListItems(Rect rect, int index, bool isActive, bool isFocused)
    {

        SerializedProperty element = questList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty destList = element.FindPropertyRelative("destinations");
        SerializedProperty homeDest = element.FindPropertyRelative("homeDestination");

        SerializedProperty windBool = element.FindPropertyRelative("isLookingForWind");
        SerializedProperty waterBool = element.FindPropertyRelative("isLookingForWater");
        SerializedProperty sunBool = element.FindPropertyRelative("isLookingForSunlight");

        SerializedProperty questEvent = element.FindPropertyRelative("finishedEvent");
        //The Quest name
        EditorGUI.PropertyField(
            new Rect(rect.x, rect.y, 160, EditorGUIUtility.singleLineHeight * 10),
            element.FindPropertyRelative("questName"),
            GUIContent.none);

        //The Locations enum
        EditorGUI.PropertyField(
            new Rect(rect.x + 165, rect.y, 120, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("types"),
            GUIContent.none);

        if (manager.Quests[index].types == FetchQuest.QuestTypes.Travel)
        {
            //The text part of the Destination part
            //EditorGUI.LabelField(new Rect(rect.x + 290, rect.y, 200, EditorGUIUtility.singleLineHeight), "Destination Number:");

            //The text input field of the Destination part
            //EditorGUI.PropertyField(
            //    new Rect(rect.x + 415, rect.y, 20, EditorGUIUtility.singleLineHeight),
            //    element.FindPropertyRelative("currentDestinationNumber"),
            //    GUIContent.none
            //);

            //Dropdown for each destination in a quest
            if (EditorGUI.PropertyField(new Rect(rect.x + 300, rect.y, 20, EditorGUIUtility.singleLineHeight), destList))
            {
                for (int i = 0; i < destList.arraySize; ++i)
                {
                    SerializedProperty destination = destList.GetArrayElementAtIndex(i);

                    SerializedProperty destName = destination.FindPropertyRelative("destinationName");

                    SerializedProperty onEvent = destination.FindPropertyRelative("destinationEvent");

                    Destination d = destination.objectReferenceValue as Destination;

                    if (d != null)
                    {
                        //TODO: Element spacing (replace 20 + 20 + 20 + 20 + 20 with something less arbitrary)
                        EditorGUI.LabelField(new Rect(rect.x + 300, rect.y + 20 + 20 * i, 100f, EditorGUIUtility.singleLineHeight), d.destinationName);
                        if (GUI.Button(new Rect(rect.x + 375, rect.y + 20 + 20 * i, 15f, EditorGUIUtility.singleLineHeight), "-"))
                        {
                            var elementProperty = destList.GetArrayElementAtIndex(i);
                            if (destList.GetArrayElementAtIndex(i).objectReferenceValue != null)
                            {
                                destList.GetArrayElementAtIndex(i).objectReferenceValue = null;
                            }
                            destList.DeleteArrayElementAtIndex(i);
                        }
                    }


                    //EditorGUI.PropertyField(new Rect(rect.x + 700, rect.y, 120, EditorGUIUtility.singleLineHeight), onEvent, GUIContent.none);
                    if (GUI.Button(new Rect(rect.x + 395, rect.y + 20 + 20 * i, 100f, EditorGUIUtility.singleLineHeight), "Show Object"))
                    {
                        UnityEditor.EditorGUIUtility.PingObject(d);
                    }
                }

                heights[index] = destList.arraySize * 20;

                if (destList.arraySize <= 0)
                {
                    EditorGUI.LabelField(new Rect(rect.x + 455, rect.y + 20, 250f, EditorGUIUtility.singleLineHeight), "You have not assigned any destinations");
                }
            }
            else
            {
                heights[index] = 0;
            }
        }
        else if (manager.Quests[index].types == FetchQuest.QuestTypes.GoToCondition)
        {
            EditorGUI.LabelField(new Rect(rect.x + 290, rect.y, 120, EditorGUIUtility.singleLineHeight), "Resources: ");

            EditorGUI.LabelField(new Rect(rect.x + 370, rect.y, 120, EditorGUIUtility.singleLineHeight), "Wind: ");
            EditorGUI.PropertyField(new Rect(rect.x + 405, rect.y, 20, EditorGUIUtility.singleLineHeight), windBool, GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x + 440, rect.y, 120, EditorGUIUtility.singleLineHeight), "Water: ");
            EditorGUI.PropertyField(new Rect(rect.x + 480, rect.y, 20, EditorGUIUtility.singleLineHeight), waterBool, GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x + 510, rect.y, 120, EditorGUIUtility.singleLineHeight), "Sun: ");
            EditorGUI.PropertyField(new Rect(rect.x + 540, rect.y, 120, EditorGUIUtility.singleLineHeight), sunBool, GUIContent.none);
        }

        EditorGUI.PropertyField(new Rect(rect.width - 275, rect.y, 200, EditorGUIUtility.singleLineHeight), questEvent, GUIContent.none);
        heights[index] = 180;

            EditorGUI.LabelField(new Rect(rect.x + rect.width - 75, rect.y, 60, EditorGUIUtility.singleLineHeight), "Finished:");
        GUI.enabled = false;
        EditorGUI.Toggle(
            new Rect(rect.x + rect.width - 20, rect.y, 20, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("isDone").boolValue
        );
        GUI.enabled = true;
    }

    void DrawQuestHeader(Rect rect)
    {
        string name = "Quests";
        EditorGUI.LabelField(rect, name);
    }

    public float DrawHeight(int index)
    {

        if (heights.ContainsKey(index))
        {
            return 20 + heights[index];
        }
        else return 20;
    }
}
