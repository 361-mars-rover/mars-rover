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
// namespace PathFinding.BehaviorTrees
// {
//     public class IStrategy
//     {
        
//     }
//     public class Leaf : Node {
//         readonly IStrategy strategy;
//         public Leaf(string name, IStrategy strategy){
//             this.strategy = strategy;
//         }
//         public override Status Process() => strategy.Process();
//         public override void Reset() => strategy.Reset();
//     }
//     public class Node{
//         public enum Status {Success, Failure, Running}
//         public readonly string name;
//         public readonly List<Node> children = new();
//         protected int currentChild;
//         public Node(string name = "Node"){
//             this.name = name;
//         }
//         public void AddChild(Node child){
//             children.Add(child);
//         }

//         public virtual Status Process() =>  children[currentChild].Process();
//         public virtual void Reset(){
//             currentChild = 0;
//             foreach (var child in children)
//             {
//                 child.Reset();
//             }
//         }
//     }
// }