#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PLIbox.TransitionObject
{
    [CustomEditor(typeof(TransitionAnimation))]
    public class TransitionAnimationEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            SerializedProperty animationDuration = serializedObject.FindProperty("_animationDuration");
            SerializedProperty debugPrint = serializedObject.FindProperty("_DEBUGPrintPositionAtStartup");
            SerializedProperty bypassIsPlaying = serializedObject.FindProperty("_bypassPlayingGuard");
            SerializedProperty listProp = serializedObject.FindProperty("_objectToAnimate");
            SerializedProperty FinishAnimationEvent = serializedObject.FindProperty("_onFinishAnimation");
            SerializedProperty FinishAnimationReversedEvent = serializedObject.FindProperty("_onFinishAnimationReversed");

            PropertyField animationDurationField = new PropertyField(animationDuration);
            PropertyField debugPrintField = new PropertyField(debugPrint);
            PropertyField bypassIsPlayingField = new PropertyField(bypassIsPlaying);
            PropertyField listField = new PropertyField(listProp);
            PropertyField FinishAnimationEventField = new PropertyField(FinishAnimationEvent);
            PropertyField FinishAnimationReversedEventField = new PropertyField(FinishAnimationReversedEvent);

            animationDurationField.Bind(serializedObject);
            debugPrintField.Bind(serializedObject);
            bypassIsPlayingField.Bind(serializedObject);
            listField.Bind(serializedObject);
            FinishAnimationEventField.Bind(serializedObject);
            FinishAnimationReversedEventField.Bind(serializedObject);

            container.Add(animationDurationField);
            container.Add(debugPrintField);
            container.Add(bypassIsPlayingField);
            container.Add(listField);
            container.Add(FinishAnimationEventField);
            container.Add(FinishAnimationReversedEventField);

            return container;
        }
    }
}
#endif