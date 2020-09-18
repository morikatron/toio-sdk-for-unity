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
using System.Diagnostics;
using System;


public class PodInstallation {
#if UNITY_2019_3_OR_NEWER
    private static string PODFILE_CONTENT = @"# Uncomment the next line to define a global platform for your project
platform :ios, '10.0'
target 'Unity-iPhone' do
  use_frameworks!
  pod 'MultiplatformBleAdapter', '~> 0.1.5'
  target 'Unity-iPhone Tests' do
    inherit! :search_paths
  end
end
target 'UnityFramework' do
  use_frameworks!
  pod 'MultiplatformBleAdapter', '~> 0.1.5'
end
";

#else
    private static string PODFILE_CONTENT = @"# Uncomment the next line to define a global platform for your project
platform :ios, '10.0'
target 'Unity-iPhone' do
  use_frameworks!
  pod 'MultiplatformBleAdapter', '~> 0.1.5'
  target 'Unity-iPhone Tests' do
    inherit! :search_paths
  end
end
";
#endif

    [PostProcessBuildAttribute(3)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        if (target != BuildTarget.iOS) {
            return;
        }

        // Add usual ruby runtime manager path to process.
        ShellCommand.AddPossibleRubySearchPaths();

        var podExisting = ShellCommand.Run("which", "pod");
        if (string.IsNullOrEmpty(podExisting)) {
            var text = @"toio-plugin-unity integrating failed. Building toio-plugin-unity for iOS target requires CocoaPods, but it is not installed. Please run ""sudo gem install cocoapods"" and try again.";
            UnityEngine.Debug.LogError(text);
            var clicked = EditorUtility.DisplayDialog("CocoaPods not found", text, "More", "Cancel");
            if (clicked) {
                Application.OpenURL("https://cocoapods.org");
            }
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var podfileLocation = Path.Combine(pathToBuiltProject, "Podfile");

        if (File.Exists(podfileLocation)) {
            var text = @"A Podfile is already existing under Xcode project root. Skipping copying of toio-plugin-unity's Podfile. Make sure you have setup Podfile correctly if you are using another package also requires CocoaPods.";
            UnityEngine.Debug.Log(text);
        } else {
            using (StreamWriter stream = File.CreateText(podfileLocation)) {
                stream.Write(PODFILE_CONTENT);
            }
        }

        Directory.SetCurrentDirectory(pathToBuiltProject);
        var log = ShellCommand.Run("pod", "install");
        UnityEngine.Debug.Log(log);
        Directory.SetCurrentDirectory(currentDirectory);
    }
}
#endif
#endif
