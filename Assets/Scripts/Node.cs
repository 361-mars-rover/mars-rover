using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public enum Status { Success, Failure, Running }

public abstract class Node{
    public abstract Status Tick();
}

public class SequenceNode : Node{
    private List<Node> children;
    public SequenceNode(List<Node> children)
    {
        this.children = children;
    }
    public override Status Tick()
    {
        foreach (Node child in children)
        {
            Status status = child.Tick();
            if (status != Status.Success){
                return status;
            }
        }
        return Status.Success;
    }
}
public class SelectorNode : Node{
    private List<Node> children;
    public SelectorNode(List<Node> children)
    {
        this.children = children;
    }
    public override Status Tick()
    {
        foreach (Node child in children)
        {
            Status status = child.Tick();
            if (status == Status.Success){
                return Status.Success;
            }
        }
        return Status.Failure;
    }
}
public class ActionNode : Node{
    private System.Func<Status> action;
    public ActionNode(System.Func<Status> action)
    {
        this.action = action;
    }
    public override Status Tick()
    {
        return action.Invoke();
    }
}