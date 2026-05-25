using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ExplosionTests
{
    [UnityTest]
    public IEnumerator Explosion_DestroysItself_AfterLifetime()
    {
        // Arrange
        GameObject explosionObject = new GameObject("TestExplosion");
        Explosion explosionScript = explosionObject.AddComponent<Explosion>();

        // Act: Wait slightly longer than lifetime (0.5s) to ensure destruction
        yield return new WaitForSeconds(0.6f);

        // Assert
        Assert.IsTrue(explosionObject == null, "Explosion should be destroyed after lifetime expires.");
    }
}