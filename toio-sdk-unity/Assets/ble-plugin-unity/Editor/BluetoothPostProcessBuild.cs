#if UNITY_IOS
#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

using UnityEditor.Callbacks;

using UnityEditor.iOS.Xcode;

public class BluetoothPostProcessBuild
{
	[PostProcessBuild]
	public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
	{
		if (buildTarget == BuildTarget.iOS)
		{
			// Get plist
			string plistPath = pathToBuiltProject + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			// Get root
			PlistElementDict rootDict = plist.root;

            rootDict.SetString("NSBluetoothPeripheralUsageDescription", "Cubeと通信するためにBluetoothを使います。");
            rootDict.SetString("NSBluetoothAlwaysUsageDescription", "Cubeと通信するためにBluetoothを使います。");


            // Set encryption usage boolean
            string encryptKey = "ITSAppUsesNonExemptEncryption";
            rootDict.SetBoolean(encryptKey, false);

            // remove exit on suspend if it exists.
            string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if(rootDict.values.ContainsKey(exitsOnSuspendKey))
            {
                rootDict.values.Remove(exitsOnSuspendKey);
            }

			// Write to file
			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif
#endif
