using Season.AssetLibrary;
using Season.SceneBehaviors;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Season.BattleSubSystem
{
    public class BattleFieldInteractiveSystem : MonoBehaviour
    {
        private GameObject _squareEffect;
        private GameObject _circleEffect;

        private readonly float _fieldRadius = 15f;
        private readonly float _textureFactor = 1.6f;
        private Vector3 _fieldCenter;
        public Vector3 FieldCenter => _fieldCenter;

        public Vector3 ClampDisplacementByField(Vector3 pos)
        {
            var temp = _fieldCenter;
            temp.y = 0f;
            pos.y = 0f;
            var direction = pos - temp;
            var scale = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.z));
            if (scale.x <= _fieldRadius && scale.y <= _fieldRadius) {
                return pos;
            } else {
                return temp + direction * _fieldRadius / Mathf.Max(scale.x, scale.y);
            }
        }

        public void SetBattleField(Vector3 center)
        {
            _fieldCenter = center;
            _fieldCenter.y = 0.01f;
            _squareEffect.SetActive(true);
            _squareEffect.transform.position = _fieldCenter;
            _squareEffect.transform.localScale = new Vector3(
                _fieldRadius * _textureFactor,
                _fieldRadius * _textureFactor,
                _fieldRadius * _textureFactor);
        }
        public void SetMovement(Vector3 center, float radius)
        {
            _circleEffect.SetActive(true);
            radius /= 5;
            center.y = 0.01f;
            _circleEffect.transform.position = center;
            _circleEffect.transform.localScale = new Vector3(radius, radius, radius);
        }

        private async void RegisterEffects()
        {
            if (!_squareEffect)
            {
                _squareEffect = await Addressables.InstantiateAsync(BattleScene.SquareFieldEffect, transform, true).Task;
            }

            if (!_circleEffect)
            {
                _circleEffect = await Addressables.InstantiateAsync(BattleScene.CircleMovementEffect, transform, true).Task;
            }
        }

        protected void Awake()
        {
            RegisterEffects();
        }

    }
}