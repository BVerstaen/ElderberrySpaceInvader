using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLIbox.TransitionObject
{
    //Parameters for one object to animate
    [Serializable]
    public class TransitionObject
    {
        public enum TransformType
        {
            Position,
            LocalPosition,
            AnchoredPosition
        }

        public enum DimensionType
        {
            UI,
            ThreeD
        }

        public DimensionType Dimension;
        public RectTransform Rect;
        public Transform Transform;

        [Header("Position")]
        public TransformType TransformToChange;
        public AnimationCurve PositionCurve;
        public Vector3 BeginPosition;
        public Vector3 EndPosition;

        [Space(15)]
        [Header("Rotation")]
        public AnimationCurve RotationCurve;
        public float BeginRotation = 0.0f;
        public float EndRotation = 0.0f;
        //3D Rotation
        [Header("3D Rotation")]
        public Vector3 Begin3DRotation;
        public Vector3 End3DRotation;

        [Space(15)]
        [Header("Scale")]
        public AnimationCurve ScaleCurve;
        public float BeginScale = 1.0f;
        public float EndScale = 1.0f;

        public Vector3 LerpPosition(float t)
        {
            return new Vector3
            (
                Mathf.Lerp(BeginPosition.x, EndPosition.x, PositionCurve.Evaluate(t)),
                Mathf.Lerp(BeginPosition.y, EndPosition.y, PositionCurve.Evaluate(t)),
                Mathf.Lerp(BeginPosition.z, EndPosition.z, PositionCurve.Evaluate(t))
            );
        }

        public Vector3 LerpRotation(float t)
        {
            if (Dimension == DimensionType.ThreeD)
            {
                return new Vector3
                (
                    Mathf.Lerp(Begin3DRotation.x, End3DRotation.x, RotationCurve.Evaluate(t)),
                    Mathf.Lerp(Begin3DRotation.y, End3DRotation.y, RotationCurve.Evaluate(t)),
                    Mathf.Lerp(Begin3DRotation.z, End3DRotation.z, RotationCurve.Evaluate(t))
                );
            }
            else
            {
                return new Vector3
                (
                    0,
                    0,
                    Mathf.Lerp(BeginRotation, EndRotation, RotationCurve.Evaluate(t))
                );
            }
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            //check if animation curve isn't empty
            if (IsAnimationCurveEmpty(PositionCurve))
                return;

            switch (TransformToChange)
            {
                case TransformType.Position:
                    if (Dimension == DimensionType.UI)
                        Rect.transform.position = newPosition;
                    else
                        Transform.position = newPosition;
                    break;

                case TransformType.LocalPosition:
                    if (Dimension == DimensionType.UI)
                        Rect.transform.localPosition = newPosition;
                    else
                        Transform.localPosition = newPosition;
                    break;

                case TransformType.AnchoredPosition:
                    if (Dimension == DimensionType.ThreeD)
                    {
                        Debug.LogError("You can't have \"ThreeD\" with anchored position !");
                        return;
                    }
                    Vector2 newVector2Position = newPosition; //Ignore Z axis
                    Rect.anchoredPosition = newVector2Position;
                    break;
            }
        }

        public void UpdateRotation(Vector3 newRotation)
        {
            //check if animation curve isn't empty
            if (IsAnimationCurveEmpty(RotationCurve))
                return;

            switch (Dimension)
            {
                case DimensionType.UI:
                    Rect.transform.eulerAngles = newRotation;
                    break;

                case DimensionType.ThreeD:
                    Transform.eulerAngles = newRotation;
                    break;
            }

        }

        public void UpdateScale(float newScale)
        {
            //check if animation curve isn't empty
            if (IsAnimationCurveEmpty(ScaleCurve))
                return;

            if (Dimension == DimensionType.UI)
                Rect.transform.localScale = new Vector2(newScale, newScale);
            else
                Transform.localScale = new Vector3(newScale, newScale, newScale);
        }

        public bool IsAnimationCurveEmpty(AnimationCurve curveToTest) => curveToTest.keys.Length < 2;
    }


    public class TransitionAnimation : MonoBehaviour
    {
        private enum AnimationDirection
        {
            Forward,
            Backward
        }

        [Header("Parameters")]
        [SerializeField] private float _animationDuration;
        [SerializeField] private bool _DEBUGPrintPositionAtStartup;
        [SerializeField] private bool _bypassPlayingGuard;

        [Header("Animation")]
        [SerializeField] private List<TransitionObject> _objectToAnimate;

        [Header("Events")]
        [SerializeField] private UnityEvent _onFinishAnimation;
        [SerializeField] private UnityEvent _onFinishAnimationReversed;

        private Coroutine _animationCoroutine;
        private bool _isPlaying;
        private float _animationProgress;

        private AnimationDirection _lastDirection = AnimationDirection.Backward;

        public bool IsPlaying { get => _isPlaying; }

        public List<TransitionObject> ObjectToAnimate { get => _objectToAnimate; }

        public void OnDisable()
        {
            StopAnimation(false);
        }

        public void PlayAnimation()
        {
            if (!AssertAnimation())
                return;

            //print default position if needed
            if (_DEBUGPrintPositionAtStartup)
            {
                foreach (TransitionObject animated in _objectToAnimate)
                {
                    if (animated.Dimension == TransitionObject.DimensionType.UI)
                        Debug.Log(animated.Rect.gameObject.name + " anchored position is : " + animated.Rect.anchoredPosition);
                    else
                        Debug.Log(animated.Transform.gameObject.name + "is in 3D, so can't get the anchoredPosition");
                }
            }

            _animationProgress = 0.0f;
            _animationCoroutine = StartCoroutine(TransitionAnimationRoutine(AnimationDirection.Forward));
        }

        public void PlayAnimationReversed()
        {
            if (!AssertAnimation())
                return;

            _animationProgress = 1.0f;
            _animationCoroutine = StartCoroutine(TransitionAnimationRoutine(AnimationDirection.Backward));
        }

        public void ToggleAnimation()
        {
            switch (_lastDirection)
            {
                case AnimationDirection.Forward:
                    PlayAnimationReversed();
                    break;
                case AnimationDirection.Backward:
                    PlayAnimation();
                    break;
            }
        }

        public void SetStartState(bool callEvents)
        {
            //Set animation to first state
            _animationProgress = 0.0f;
            UpdateAnimations();

            //Launch events if needed
            if (callEvents)
            {
                _lastDirection = AnimationDirection.Forward;
                InvokeAnimationEvent();
            }
        }

        public void SetEndState(bool callEvents)
        {
            _animationProgress = 1.0f;
            UpdateAnimations();


            //Launch events if needed
            if (callEvents)
            {
                _lastDirection = AnimationDirection.Backward;
                InvokeAnimationEvent();
            }
        }

        public void StopAnimation(bool finishAnimation)
        {
            //For safety, stop playing the coroutine
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
            _isPlaying = false;

            if (finishAnimation)
            {
                //Need to switch because lastDirection != the current one
                _lastDirection = _lastDirection == AnimationDirection.Forward ? AnimationDirection.Backward : AnimationDirection.Forward;

                //Set State to their final destination & stop playing
                _animationProgress = (_lastDirection == AnimationDirection.Forward ? 1 : 0);
                UpdateAnimations();

                //Invoke animation event
                InvokeAnimationEvent();
            }
        }

        private IEnumerator TransitionAnimationRoutine(AnimationDirection direction)
        {
            _isPlaying = true;
            float timeElapsed = 0.0f;
            float progress = 0.0f;

            while (timeElapsed <= _animationDuration)
            {
                progress = timeElapsed / _animationDuration;
                _animationProgress = direction == AnimationDirection.Forward ? progress : 1 - progress;
                UpdateAnimations();

                timeElapsed += Time.deltaTime;
                yield return null;
            }
            _isPlaying = false;

            _animationProgress = direction == AnimationDirection.Forward ? 1 : 0;
            UpdateAnimations();

            //Decide which event to invoke based on the direction
            _lastDirection = direction;
            InvokeAnimationEvent();
        }

        private void UpdateAnimations()
        {
            foreach (TransitionObject animated in _objectToAnimate)
            {
                animated.UpdatePosition(animated.LerpPosition(_animationProgress));
                animated.UpdateRotation(animated.LerpRotation(_animationProgress));
                animated.UpdateScale(Mathf.Lerp(animated.BeginScale, animated.EndScale, animated.ScaleCurve.Evaluate(_animationProgress)));
            }
        }

        private void InvokeAnimationEvent()
        {
            //Decide which event to invoke based on the direction
            switch (_lastDirection)
            {
                case AnimationDirection.Forward:
                    _onFinishAnimation?.Invoke();
                    break;

                case AnimationDirection.Backward:
                    _onFinishAnimationReversed?.Invoke();
                    break;
            }
        }

        private bool AssertAnimation()
        {
            if (_isPlaying && !_bypassPlayingGuard)
            {
                Debug.LogWarning("Animation is already playing");
                return false;
            }

            if (_animationDuration <= 0)
            {
                Debug.LogWarning("No animation duration");
                return false;
            }

            if (_objectToAnimate.Count <= 0)
            {
                Debug.LogError("No Object to animate");
                return false;
            }

            return true;
        }
    }
}