using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class KitchenWindows
{
    [MenuItem("Кондитерская/Собрать игру для Windows")]
    public static void BuildPlayer(){
        Build(ScriptingImplementation.IL2CPP,"Builds/Windows-IL2CPP");
    }
    public static void BuildPortable(){
        var target=UnityEditor.Build.NamedBuildTarget.Standalone;
        var backend=PlayerSettings.GetScriptingBackend(target);
        try{Build(ScriptingImplementation.Mono2x,"Builds/Windows");}
        finally{PlayerSettings.SetScriptingBackend(target,backend);AssetDatabase.SaveAssets();}
    }
    static void Build(ScriptingImplementation backend,string folder){
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,backend);AssetDatabase.SaveAssets();
        Debug.Log("KITCHEN_WINDOWS_BACKEND "+PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone));
        Directory.CreateDirectory(folder);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=new[]{SceneGenerator.Kitchen},locationPathName=folder+"/ConfectioneryAlexandra.exe",
            target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Сборка Windows: "+report.summary.result);
        Debug.Log("KITCHEN_WINDOWS_READY");
    }
    public static void Launch(){
        var gameView=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.maximized=true;gameView.Focus();SceneGenerator.RunDemo();
    }
    public static void Capture(Camera cam,string name){
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        var rt=RenderTexture.GetTemporary(1280,720,24);
        var old=cam.targetTexture;var oldActive=RenderTexture.active;
        try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
            tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine("Logs",name+".png"),tex.EncodeToPNG());Object.DestroyImmediate(tex);}
        finally{cam.targetTexture=old;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(rt);}
    }
}
