//
// Copyright (c) 2020-present, Sony Interactive Entertainment Inc.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//

#if UNITY_IOS
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class XcodeModuleEnabling {
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        if (target != BuildTarget.iOS) {
            return;
        }

        string projPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj/project.pbxproj");
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

#if UNITY_2019_3_OR_NEWER
        var appTarget = proj.GetUnityFrameworkTargetGuid();
#else
        var appTarget = proj.TargetGuidByName("Unity-iPhone");
#endif
        proj.SetBuildProperty (appTarget, "CLANG_ENABLE_MODULES", "YES");
        File.WriteAllText(projPath, proj.WriteToString());
    }
}
#endif
#endif
