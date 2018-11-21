using HoloIslandVis.Automaton;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEngine.TestTools;
using NUnit.Framework;
#endif

public class Tests_Automaton {
#if UNITY_EDITOR
    [Test]
    public void TestCommandEquality() {
        Command command1 = new Command(GestureType.Invariant, KeywordType.Hide, InteractableType.Island);
        Command command2 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Island);
        Command command3 = new Command(GestureType.OneHandDoubleTap, KeywordType.Find, InteractableType.ImportDock);
        Command command4 = new Command(GestureType.ManipulationUpdate, KeywordType.Invariant, InteractableType.Invariant);
        Command command5 = new Command(GestureType.ManipulationUpdate, KeywordType.Invariant, InteractableType.Island);

        Assert.That(command1.Equals(command2) == true);
        Assert.That(command2.Equals(command3) == false);
        Assert.That(command5.Equals(command4) == true);
        Assert.That(command4.Equals(command5) == true);
    }
#endif
}
