using Season.Character;
using Season.Input;
using Season.Manager;
using Season.Player;
using Season.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;

public class AnimTest : MonoBehaviour
{
    public string animName;
    private Animator _animator;
    private PlayerMovementControl _playerMovementControl;
    [SerializeField]
    private GameObject g1;
    [SerializeField]
    private GameObject g2;

    [FormerlySerializedAs("_timeLineDirector")] [SerializeField] private CutsceneDirector cutsceneDirector;
    [SerializeField] private GameObject timeLine;
    [SerializeField] private GameObject _camera;
    [SerializeField] private Animator _playerModel;
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerMovementControl = GetComponent<PlayerMovementControl>();
    }
    // Start is called before the first frame update
    void Start()
    {
        GameInputManager.FieldGameInputAction.Enable();
        GameEventManager.MainInstance.CallEvent("OnMainPlayerChangedOnField",gameObject);
        GameEventManager.MainInstance.CallEvent("OnMoveAbilityChanged",gameObject, MoveAbility.UNLIMITED, MoveAbility.AGENT_FOLLOWING);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.P)) {

            _playerMovementControl.IsBattleLock = !_playerMovementControl.IsBattleLock;
        }
        if (Input.GetKey(KeyCode.V))
        {
            timeLine.SetActive(true);
            _camera.SetActive(false);
            var transform1 = transform;
            var o = cutsceneDirector.gameObject;
            o.transform.position = transform1.position;
            o.transform.rotation = transform1.rotation;
            gameObject.SetActive(false);
        }
    }
    public void ShowPhantom()
    {
        g1.transform.position = gameObject.transform.position;
        g2.transform.position = gameObject.transform.position;
        g1.SetActive(true);
        g2.SetActive(true);
    }
    public void HidePhantom()
    {
        g1.SetActive(false);
        g2.SetActive(false);
    }
}
