using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : Node {

    public BehaviourTree() {                        // Constructors

        name = "Tree";
    }

    public BehaviourTree(string n) {

        name = n;
    }

    struct NodeLevel {                          // Hold node level (used to indent tree structure)

        public int level;
        public Node node;
    }

    public override Status Process() {

        return children[currentChild].Process();
    }

    public void PrintTree() {                   // Traverse through graph and print
                                                // without recursion.

        string treePrintOut = "";               // Empty to start
        Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();  // Using NodeLevel (not  node, is a level)
        Node currentNode = this;
        nodeStack.Push(new NodeLevel { level = 0, node = currentNode }); // Push current node level
                                                                         // on to stack

        while (nodeStack.Count != 0) {

            NodeLevel nextNode = nodeStack.Pop();                          // Pop parents and print
            treePrintOut += new string('-', nextNode.level) + nextNode.node.name + "\n"; // Print dash

            for (int i = nextNode.node.children.Count - 1; i >= 0; --i) { // Push children of parent just popped off and add to nodeStack

                nodeStack.Push(new NodeLevel { level = nextNode.level + 1, node = nextNode.node.children[i] });
            }
        }
        Debug.Log(treePrintOut);
    }
}