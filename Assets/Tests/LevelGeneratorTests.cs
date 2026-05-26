//using system.collections;
//using nunit.framework;
//using unityengine;
//using unityengine.testtools;
//using unityengine.scenemanagement;

//public class levelgeneratortests
//{
//    [unitytest]
//    public ienumerator levelgenerator_spawnscorrectnumberofcrates()
//    {
//        // arrange
//        scenemanager.loadscene(0);

//        // wait for generator to build level
//        yield return null;

//        // act
//        levelgenerator generator = gameobject.findanyobjectbytype<levelgenerator>();
//        crate[] spawnedcrates = gameobject.findobjectsbytype<crate>(findobjectssortmode.none);

//        // assert
//        assert.isnotnull(generator, "levelgenerator not found in scene.");
//        assert.areequal(generator.cratestospawn, spawnedcrates.length, "spawned crates count does not match cratestospawn setting.");
//    }
//}