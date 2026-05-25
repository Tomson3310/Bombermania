using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class LevelGeneratorTests
{
    [UnityTest]
    public IEnumerator LevelGenerator_SpawnsCorrectNumberOfCrates()
    {
        // Arrange
        SceneManager.LoadScene(0);

        // Wait for generator to build level
        yield return null;

        // Act
        LevelGenerator generator = GameObject.FindAnyObjectByType<LevelGenerator>();
        Crate[] spawnedCrates = GameObject.FindObjectsByType<Crate>(FindObjectsSortMode.None);

        // Assert
        Assert.IsNotNull(generator, "LevelGenerator not found in scene.");
        Assert.AreEqual(generator.CratesToSpawn, spawnedCrates.Length, "Spawned crates count does not match CratesToSpawn setting.");
    }
}