using System;
using UnityEditor;
using UnityEngine;
using WanderUtils;
using ParticleCities;

namespace WanderUtils.Editor
{
    [CustomEditor(typeof(InputManagerSteamVR))]
    public class InputManagerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();

            if (!EditorApplication.isPlaying)
            {
                return;
            }

            InputManager inputManager = (InputManager)target;
            EditorGUILayout.BeginVertical();

            forBothHand((handType) =>
            {
                EditorGUILayout.FloatField("Trigger", inputManager.GetTriggerValue(handType));
                EditorGUILayout.FloatField("Grab", inputManager.GetGrabValue(handType));
            });

            forBothHand((handType) =>
            {
                bool touchpadPressed;
                Vector2 touchpad = inputManager.GetTouchpadValue(handType, out touchpadPressed);
                EditorGUILayout.Toggle("Touchpad Pressed", touchpadPressed);
                EditorGUILayout.Vector2Field("Touchpad", touchpad);
            });

            EditorGUILayout.EndVertical();
        }

        void OnEnable()
        {
            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        void Update()
        {
            Repaint();
        }

        private void forBothHand(Action<HandType> action)
        {
            EditorGUILayout.LabelField("Left Hand");
            action(HandType.Left);
            EditorGUILayout.LabelField("Right Hand");
            action(HandType.Right);
            EditorGUILayout.Separator();
        }
    }
}
