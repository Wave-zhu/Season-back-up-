using Season.Manager;
using System.Collections;
using System.Collections.Generic;
using Season.SceneBehaviors;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Season.Render
{
    public class FragmentAnim : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Explodable _explodable;
        private SpriteAdapter _spriteAdapter;
        private BoxCollider2D _box;
        public UnityEngine.Camera camera;

        private List<GameObject> effects = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _explodable = GetComponent<Explodable>();
            _spriteAdapter = GetComponent<SpriteAdapter>();
            _box = GetComponent<BoxCollider2D>();

            _box.size = new Vector2(camera.orthographicSize * 2 * camera.aspect, camera.orthographicSize * 2);
        }

        public void Capture()
        {
            StartCoroutine(CaptureByCamera());
        }

        private IEnumerator CaptureByCamera()
        {
            yield return new WaitForEndOfFrame();

            RenderTexture mRender = new RenderTexture(Screen.width, Screen.height, 0);
            //设置相机的渲染目标  
            camera.targetTexture = mRender;
            //开始渲染  
            camera.Render();

            // 创建一个临时Texture2D来复制RenderTexture的内容
            Texture2D tempTexture = new Texture2D(mRender.width, mRender.height, TextureFormat.RGB24, false);
            RenderTexture.active = mRender;
            tempTexture.ReadPixels(new Rect(0, 0, mRender.width, mRender.height), 0, 0);
            tempTexture.Apply();
            RenderTexture.active = null;

            Sprite newSprite = Sprite.Create(tempTexture, new Rect(0, 0, tempTexture.width, tempTexture.height),
                new Vector2(0.5f, 0.5f));
            _spriteRenderer.sprite = newSprite;
            camera.targetTexture = null;
            GameObject.Destroy(mRender);
            SceneAssets.CameraSubSystem.CameraControl.SetActive(false);
            SceneAssets.CameraSubSystem.EffectCamera.gameObject.SetActive(true);
            //释放相机，销毁渲染贴图
            //渲染到当前Sprite上
            _spriteAdapter.AdaptSpriteRender();
            _explodable.explode(false, false);
            _spriteRenderer.sprite = null;
            effects = _explodable.fragments;
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].layer = 9;
                Rigidbody2D rb2D = effects[i].GetComponent<Rigidbody2D>();
                rb2D.GetComponent<Collider2D>().isTrigger = true;
                rb2D.gravityScale = 0;
                rb2D.freezeRotation = true;
                rb2D.transform.localEulerAngles = new Vector3(0, 0, Random.Range(-10f, 10f));
                rb2D.transform.localPosition += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                rb2D.gravityScale = 2;
                rb2D.AddForce(new Vector2(Random.Range(-3f, 3f), Random.Range(5f, 10f)), ForceMode2D.Impulse);
            }
        }

        public void CleanFragments()
        {
            StopAllCoroutines();
            for (int i = 0; i < _explodable.fragments.Count; i++)
            {
                Destroy(_explodable.fragments[i]);
            }

            _explodable.fragments.Clear();
        }

        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener("PlayFrag", Capture);
            GameEventManager.MainInstance.AddEventListener("CleanFrag", CleanFragments);
        }

        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent("PlayFrag", Capture);
            GameEventManager.MainInstance.RemoveEvent("CleanFrag", CleanFragments);
        }
    }
}
    