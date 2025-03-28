using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeController : MonoBehaviour
{
    public CarControl carControl;
    private Node rootNode;
    void Start()
    {
        ActionNode circleScan = new ActionNode(() => {
            carControl.CircleAIUpdate();
            return Status.Running;
        });

        ActionNode checkForGem = new ActionNode(() => {
            bool gemFound = false; // e.g., carControl.HasDetectedGem();, for now it is hardcoded as false
            return gemFound ? Status.Success : Status.Failure;
        });

        ActionNode handleGem = new ActionNode(() => {
            //create the function in carControl.cs so it checks the little circle thoroughly...
            return Status.Success;
        });

        ActionNode checkNavMode = new ActionNode(() => {
            //create the function in carControl.cs so it checks the little circle thoroughly...
            return carControl.navigateToTarget ? Status.Success : Status.Failure;
        });

        ActionNode planPath = new ActionNode(() => {
            //MAKE PLAN PATH IN CAR CONTROLER
            return Status.Success;
        });

        ActionNode executeNav = new ActionNode(() => {
            //carControl.NavigateToTarget();
            // Return Running until target is reached, then Success.
            return /**carControl.TargetReached()**/false ? Status.Success : Status.Running;
        });

        SequenceNode gemSequence = new SequenceNode(new List<Node> { checkForGem, handleGem });
        SelectorNode gemSelector = new SelectorNode(new List<Node> { gemSequence, circleScan });
        SequenceNode navigationSequence = new SequenceNode(new List<Node> { planPath, executeNav });
        SelectorNode algorithmSelector = new SelectorNode(new List<Node> {
            new SequenceNode(new List<Node> {checkNavMode, navigationSequence}),
            gemSelector
        });

        rootNode = algorithmSelector;
    }
    void Update()
    {
        if (rootNode != null && carControl.useAI)
        {
            rootNode.Tick();
        }
    }
}