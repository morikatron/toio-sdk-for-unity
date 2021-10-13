# Preparation

This section introduces what you need to use toio SDK for Unity. Specifically, you will need the following three pieces of software

- Unity (2020.3.17f1 LTS)
- Xcode (11 or later)
- git

## Install Unity

For more information about Unity installation and operating environment, please refer to the official documentation below.

- [Installing Unity \- Unity Manual](https://docs.unity3d.com/2020.3/Documentation/Manual/GettingStartedInstallingUnity.html)
(*Japanese. Click the right arrow on the linked page to see the system requirements and installation details.)

We recommend using Unity Hub as it allows you to install multiple Unity versions of different versions.
Unity version should be **Unity 2020.3.17f1 (LTS)**. (We have tested with Unity 2020.3.17f1 LTS. If you want to use a newer version of Unity, please try it at your own risk)

You can also add **iOS Build Support**, **Android Build Support** or **WebGL Build Support** modules on demand during installation to enable building on supported platforms (modules can also be added later using Unity Hub).

> If you use a proxy, you may not be able to install the module.

## Install Xcode

To run your application on iOS, you will need Xcode (macOS app). Get the latest version of Xcode from the link below.

- [‎Xcode on Mac App Store](https://apps.apple.com/app/xcode/id497799835)

## About git

When you install UniTask in [Install toio SDK for Unity](download_sdk.md), you will need to have the git command installed.

#### macOS

"git" may be already installed on macOS, but if you want to check it, type the following command in a terminal app

```
$ git --version
```

If git is already installed, the version number will be displayed like this.

```
git version 2.21.1 (Apple Git-122.3)
```

If you don't have git, you will be prompted to install the command line developer tools, click the "Install" button on the dialog to do so.

#### Windows

On Windows, you can check if git is installed by launching a command prompt and running the following command.

```
> git --version
```

If you have already installed git, you will see the following text.

```
git version 2.21.0.windows.1
```

If you don't have the git command yet, download the installer from the [git for windows official website](https://gitforwindows.org/) and install it.



This completes the preliminary preparations. If you want to install toio SDK for Unity, please refer to [Installing toio SDK for Unity](download_sdk.md).