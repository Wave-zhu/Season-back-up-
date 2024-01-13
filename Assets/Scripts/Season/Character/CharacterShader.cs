using Season.Manager;
using System;
using UnityEngine;
namespace Season.Character
{
    public class CharacterShader : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

        private void CheckVisible(Collider[] colliders)
        {
            foreach (var collider in colliders) 
            {
                if (collider.transform == transform) 
                {
                    _skinnedMeshRenderer.enabled = false;
                    return;
                }
            }
            _skinnedMeshRenderer.enabled = true;
        }
        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener<Collider[]>("CheckCameraRender",CheckVisible);
        }
        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<Collider[]>("CheckCameraRender",CheckVisible);
        }
    }
}