using System.Collections;
using System.Collections.Generic;
using Season.UI;
using UnityEngine;

namespace Season.Animation
{
    public class IconMath : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Vector2 _startPos;
        private Vector2 _endPos;
        private Queue<IEnumerator> _movementsQueue;
        private bool _isMoving;
        private ActionIcon _actionIcon;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _movementsQueue = new();
            _actionIcon = GetComponent<ActionIcon>();
        }
        public void StartLine(Vector2 end, float duration)
        {
            if(end == _rectTransform.anchoredPosition) return;
            _movementsQueue.Enqueue(MoveLine(end, duration));
            if (!_isMoving)
            {
                StartCoroutine(StartNextMovement());
            }
        }
    
        public void StartParabola(Vector2 end, float duration)
        { 
            if(end == _rectTransform.anchoredPosition) return;
            _movementsQueue.Enqueue(MoveParabola(end, duration));
            if (!_isMoving)
            {
                StartCoroutine(StartNextMovement());
            }
        }
        public void StartParabolaFade(Vector2 end, float duration)
        {
            if(end == _rectTransform.anchoredPosition) return;
            _movementsQueue.Enqueue(MoveParabolaFade(end, duration));
            if (!_isMoving)
            {
                StartCoroutine(StartNextMovement());
            }
        }

        private Vector2 EvaluateLine(float t)
        {
            return Vector2.Lerp(_startPos, _endPos, t);
        }

        private Vector2 EvaluateParabola(float t)
        {
            float p  = (_endPos.y - _startPos.y) / ((_endPos.x - _startPos.x) * (_endPos.x - _startPos.x));
            float x = Mathf.Lerp(_startPos.x, _endPos.x, t);
            float y = p * (x - _startPos.x) * (x - _startPos.x) + _startPos.y;

            return new Vector2(x, y);
        }

        private IEnumerator StartNextMovement()
        {
            while (_movementsQueue.Count > 0)
            {
                IEnumerator currentMovement = _movementsQueue.Dequeue();
                _isMoving = true;
                yield return StartCoroutine(currentMovement);
            }
        }

        private IEnumerator MoveLine(Vector2 end, float duration)
        {
            _startPos = _rectTransform.anchoredPosition;
            _endPos = end;
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                Vector2 nextPos = EvaluateLine(t);
                _rectTransform.anchoredPosition = nextPos;
                yield return null;
            }
            _rectTransform.anchoredPosition = _endPos;
            _isMoving = false;
        }

        private IEnumerator MoveParabola(Vector2 end,float duration)
        {
            _startPos = _rectTransform.anchoredPosition;
            _endPos = end;
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                Vector2 nextPos = EvaluateParabola(t);
                _rectTransform.anchoredPosition = nextPos;
                yield return null;
            }
            _rectTransform.anchoredPosition = _endPos;
            _isMoving = false;
        }

        private IEnumerator MoveParabolaFade(Vector2 end,float duration)
        {
            _startPos = _rectTransform.anchoredPosition;
            _endPos = end;
            float elapsedTime = 0.0f;
        
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                _actionIcon.SetTransparency(1 - t);
                Vector2 nextPos = EvaluateParabola(t);
                _rectTransform.anchoredPosition = nextPos;
            
                yield return null;
            }
            _rectTransform.anchoredPosition = _endPos;
            _isMoving = false;
            gameObject.SetActive(false);
        }
    }
}

