using UnityEngine;
using UnityEngine.Events;

public class TutorialAction : MonoBehaviour
{
    [SerializeField] private TypeActionTutorial type;
    protected Transform target = null;
    public TypeActionTutorial GetTypeAction => type;
    bool isDone;

    protected UnityAction<Transform> actionSetup;
    public bool IsDone => isDone;

    public void SetIsDone(bool isDone)
    {
        this.isDone = isDone;
    }
    public virtual bool SetUp()
    {
        return true;
    }
    public virtual void Init(UnityAction<Transform> actionSetup)
    {
        target = null;
        this.actionSetup = actionSetup;
    }
}
