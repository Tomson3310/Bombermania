using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{
    [UnityTest]
    public IEnumerator PlayerDies_WhenDieMethodIsCalled_ObjectIsDestroyed()
    {
        // Arrange
        GameObject playerObject = new GameObject("TestPlayer");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();

        // Act
        playerMovement.Die();

        // Unity's Destroy() doesn't delete immediately; object is removed at end of frame
        yield return null;

        // Assert
        Assert.IsTrue(playerObject == null, "Player object should be destroyed after Die() is called.");
    }
}