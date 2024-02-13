using System;
using System.Collections.Generic;

// Interface for behavior tree nodes
public interface IBehaviourNode
{
    NodeState Tick();
}

// Enumeration for node states
public enum NodeState
{
    Running,
    Success,
    Failure
}

// Selector node: executes its children in sequence until one succeeds
public class Selector : IBehaviourNode
{
    private List<IBehaviourNode> children;

    public Selector(params IBehaviourNode[] nodes)
    {
        children = new List<IBehaviourNode>(nodes);
    }

    public NodeState Tick()
    {
        foreach (var child in children)
        {
            var result = child.Tick();
            if (result == NodeState.Success)
            {
                return NodeState.Success;
            }
            if (result == NodeState.Running)
            {
                return NodeState.Running;
            }
        }
        return NodeState.Failure;
    }
}

// Sequence node: executes its children in sequence until one fails
public class Sequence : IBehaviourNode
{
    private List<IBehaviourNode> children;

    public Sequence(params IBehaviourNode[] nodes)
    {
        children = new List<IBehaviourNode>(nodes);
    }

    public NodeState Tick()
    {
        foreach (var child in children)
        {
            var result = child.Tick();
            if (result == NodeState.Failure)
            {
                return NodeState.Failure;
            }
            if (result == NodeState.Running)
            {
                return NodeState.Running;
            }
        }
        return NodeState.Success;
    }
}

// Condition node: checks a condition and returns success or failure
public class Condition : IBehaviourNode
{
    private Func<bool> condition;

    public Condition(Func<bool> condition)
    {
        this.condition = condition;
    }

    public NodeState Tick()
    {
        return condition() ? NodeState.Success : NodeState.Failure;
    }
}

// Action node: performs an action

public class Action : IBehaviourNode
{
    private ActionNodeDelegate action;

    public Action(ActionNodeDelegate action)
    {
        this.action = action;
    }

    public NodeState Tick()
    {
        action();
        return NodeState.Success;
    }
}

// Define the delegate type for action nodes
public delegate void ActionNodeDelegate();


public class BehaviourTree
{
    private IBehaviourNode rootNode;

    public BehaviourTree(IBehaviourNode rootNode)
    {
        this.rootNode = rootNode;
    }

    public void Tick()
    {
        rootNode.Tick();
    }
}
