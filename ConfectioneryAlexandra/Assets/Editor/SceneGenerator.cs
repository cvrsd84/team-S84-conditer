using AlexandraBakery;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneGenerator
{
    const string Root="Assets/Resources/Atelier";
    public const string Kitchen="Assets/Scenes/Kitchen.unity";
    static Material MaterialAsset(string name,string shader){
        string path=Root+"/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material){material=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(material,path);}return material;
    }
    static Mesh SaveMesh(string id,AtelierMesh shape){
        string path=Root+"/"+id+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        var fresh=shape.Finish(id);if(!old){AssetDatabase.CreateAsset(fresh,path);return fresh;}
        EditorUtility.CopySerialized(fresh,old);Object.DestroyImmediate(fresh);EditorUtility.SetDirty(old);return old;
    }
    public static void BakeIngredientModels(){
        foreach(var id in new[]{"portion-cup","egg-yolk","milk-surface","cream-surface","cocoa-surface","sugar-pile","butter-piece","chocolate-chip","ganache-cake","custard-cake"})
            SaveMesh(id,Props.Make(id));
        AssetDatabase.SaveAssets();
    }
    [MenuItem("Кондитерская/Создать сцену кухни")]
    public static void CreateKitchen(){
        if(!AssetDatabase.IsValidFolder("Assets/Resources"))AssetDatabase.CreateFolder("Assets","Resources");
        if(!AssetDatabase.IsValidFolder(Root))AssetDatabase.CreateFolder("Assets/Resources","Atelier");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        Studio.surface=MaterialAsset("Glaze","Atelier/Porcelain");MaterialAsset("Rim","Atelier/Selection");
        Studio.Bake=SaveMesh;BakeIngredientModels();
        foreach(var list in new[]{Props.Pantry,Props.Cold,Props.Tools,Props.Extras,new[]{"sponge","icing","fruit","mat","rack","cupmat","plate","scale","book","small","large","cake","brownie","muffin","potato","hand","fill","spill","berrydot","bluedot","dust","cocoadot","parcel","portion","rosette","sprinkle-mark","stencil-mark","flour-pile","cocoa-pile","crumb-pile"}})
            foreach(var id in list)SaveMesh(id,Props.Make(id));
        var game=new GameObject("Кондитерская Александры").AddComponent<BakeryGame>();
        game.interfaceFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
        game.catalogue=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Recipes/recipe-library.json");
        game.GenerateScenePreview();Studio.Bake=null;EditorSceneManager.SaveScene(scene,Kitchen);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Kitchen,true)};
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.IL2CPP);
        PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
        PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(Kitchen);AssetDatabase.SaveAssets();
        Debug.Log("KITCHEN_SCENE_READY");
    }
    [MenuItem("Кондитерская/Запустить демо")]
    public static void RunDemo(){EditorSceneManager.OpenScene(Kitchen);EditorApplication.EnterPlaymode();}
    [MenuItem("Кондитерская/Создать и запустить демо")]
    public static void RebuildAndRun(){CreateKitchen();RunDemo();}
}
