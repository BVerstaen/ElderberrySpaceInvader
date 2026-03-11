#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static PLIbox.TransitionObject.TransitionObject;
#endif

namespace PLIbox.TransitionObject
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TransitionObject))]
    public class TransitionObjectEditor : PropertyDrawer
    {
        private PropertyField _dimension;
        private PropertyField _rect;
        private PropertyField _transform;

        //Position Serialized ref
        private PropertyField _transformToChange;
        private PropertyField _positionCurve;
        private PropertyField _beginPosition;
        private PropertyField _endPosition;

        //Rotation Serialized ref
        private PropertyField _rotationCurve;
        private PropertyField _beginRotation;
        private PropertyField _endRotation;
        private PropertyField _begin3DRotation;
        private PropertyField _end3DRotation;

        //Scale Serialized ref
        private PropertyField _scaleCurve;
        private PropertyField _beginScale;
        private PropertyField _endScale;

        private void SetupProperties(SerializedProperty property)
        {
            _dimension = new PropertyField(property.FindPropertyRelative("Dimension"));

            _rect = new PropertyField(property.FindPropertyRelative("Rect"));
            _transform = new PropertyField(property.FindPropertyRelative("Transform"));
            _transformToChange = new PropertyField(property.FindPropertyRelative("TransformToChange"));

            _positionCurve = new PropertyField(property.FindPropertyRelative("PositionCurve"));
            _beginPosition = new PropertyField(property.FindPropertyRelative("BeginPosition"));
            _endPosition = new PropertyField(property.FindPropertyRelative("EndPosition"));

            _rotationCurve = new PropertyField(property.FindPropertyRelative("RotationCurve"));
            _beginRotation = new PropertyField(property.FindPropertyRelative("BeginRotation"));
            _endRotation = new PropertyField(property.FindPropertyRelative("EndRotation"));
            _begin3DRotation = new PropertyField(property.FindPropertyRelative("Begin3DRotation"));
            _end3DRotation = new PropertyField(property.FindPropertyRelative("End3DRotation"));

            _scaleCurve = new PropertyField(property.FindPropertyRelative("ScaleCurve"));
            _beginScale = new PropertyField(property.FindPropertyRelative("BeginScale"));
            _endScale = new PropertyField(property.FindPropertyRelative("EndScale"));
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            //Setup & get infos
            var container = new VisualElement();
            SetupProperties(property);

            int index = property.displayName[property.displayName.Length - 1] - 48;
            GetCorrespondingTransitionAnimation(property, index, out bool Is3D);

            //Transform
            container.Add(_dimension);
            if (Is3D)
                container.Add(_transform);
            else
                container.Add(_rect);

            //Position
            container.Add(_transformToChange);
            container.Add(_positionCurve);
            container.Add(_beginPosition);
            container.Add(_endPosition);

            //Rotation
            container.Add(_rotationCurve);
            if (!Is3D)
            {
                container.Add(_beginRotation);
                container.Add(_endRotation);
            }
            else
            {
                container.Add(_begin3DRotation);
                container.Add(_end3DRotation);
            }

            //Scale
            container.Add(_scaleCurve);
            container.Add(_beginScale);
            container.Add(_endScale);
            return container;
        }

        private void GetCorrespondingTransitionAnimation(SerializedProperty property, int index, out bool Is3D)
        {
            string objectName = property.serializedObject.targetObject.name;
            GameObject correspondingObject = GameObject.Find(objectName);
            if (!correspondingObject)
            {
                //If can't find gameObject -> default to show everything with 2D
                Debug.LogWarning("Can't get transitionAnimation because " + objectName + " is disabled, show everything by default (and in 2D / UI) !");
                Is3D = false;
                return;
            }
            TransitionAnimation transitionAnimation = correspondingObject.GetComponent<TransitionAnimation>();
            if (!transitionAnimation)
            {
                //If can't find compoenent -> default to show everything with 2D
                Debug.LogWarning("Can't get transitionAnimation because you are in prefab mode, show everything by default (and in 2D / UI) !");
                Is3D = false;
                return;
            }

            Is3D = transitionAnimation.ObjectToAnimate[index].Dimension == DimensionType.ThreeD;
        }
    }
#endif
}

