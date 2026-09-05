using Raccoon;
using Raccoon.Controller;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public static TutorialController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public LineRenderer line;
    public Vector3 offset;
    [SerializeField] List<TutorialAction> actions;
    bool done;
    bool isInited = false;
    int numberActionDone = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        numberActionDone = 0;
        done = false;
        foreach (var ac in actions)
        {
            ac.Init(SetUpLine);

            bool isDone = PlayerData.Get.HaveActionTutDone(ac.GetTypeAction.ToString());
            ac.SetIsDone(isDone);
            if(isDone) ActionDone();
        }
        line.enabled = false;
        isInited = false;
        StartCoroutine(DelayTuto());
    }
    IEnumerator DelayTuto()
    {
        yield return new WaitForSeconds(0.1f);
        isInited = true;
    }

    // Update is called once per frame
    bool haveAction = false;
    void Update()
    {
        if (done) return;
        if (!isInited) return;
        haveAction = false;
        foreach (var a in actions)
        {
            if (!a.IsDone)
            {
                if (a.SetUp())
                {
                    haveAction = true;
                    break;
                }

            }
        }
        line.enabled = haveAction;

        if (haveAction)
        {
            line.SetPosition(0, (PlayerController.instance.transform.position + Vector3.up * 0.5f));
        }
    }

    public void SetComplete(TypeActionTutorial type)
    {
        line.enabled = false;

        if (done) return;
        if (PlayerData.Get.HaveActionTutDone(type.ToString())) return;
        print("complete ");
        actions.FirstOrDefault(a => a.GetTypeAction == type)?.SetIsDone(true);
        PlayerData.Get.AddActionTutDone(type.ToString());
        ActionDone();
    }
    void ActionDone()
    {
        numberActionDone++;
        if (numberActionDone >= actions.Count) done = true;
    }
    void SetUpLine(Transform target)
    {
       if(target != null) line.SetPosition(1, (target.position + offset + Vector3.up * 0.5f));
    }
}

public enum TypeActionTutorial
{
    LootItem, PushBrainrot, Upgrade,
}
