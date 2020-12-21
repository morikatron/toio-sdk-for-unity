#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools;
using UnityEngine;

[assembly:TestPlayerBuildModifier(typeof(TestBuildScript))]
public class TestBuildScript : ITestPlayerBuildModifier
{
    public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
    {
        if (BuildTarget.WebGL == playerOptions.target)
        {
            playerOptions.options &= ~(BuildOptions.Development);
        }
        return playerOptions;
    }
}
#endif