# FAQ

## Table of Contents

- [Basics](FAQ.md#basics)
    - [I downloaded toio SDK for Unity, but the samples don't work.](FAQ.md#i-downloaded-toio-sdk-for-unity-but-the-samples-dont-work)

- [Simulator](FAQ.md#simulator)
    - [I'm not sure which mats will work best for my development.](FAQ.md#im-not-sure-which-mats-will-work-best-for-my-development)
    - [Is the Stage Prefab required?](FAQ.md#is-the-stage-prefab-required)
    - [Is there anything I should be aware of if I don't use the Stage Prefab?](FAQ.md#is-there-anything-i-should-be-aware-of-if-i-dont-use-the-stage-prefab)

- [Web application related](FAQ.md#web-application-related)
    - [Web apps do not work well on Windows PC](FAQ.md#web-apps-do-not-work-well-on-windows-pc)
    - [Can't start or load apps made with Unity 2020](FAQ.md#Cant-start-or-load-apps-made-with-Unity-2020)

## Basics

### I downloaded toio SDK for Unity, but the samples don't work.

If you get the following error message and the program does not work, UniTask may not be installed.

```
Assets\toio-sdk-unity\Assets\toio-sdk\Scripts\Cube\Scanner\NearScanner.cs(54,22): error CS0246: The type or namespace name 'UniTask<>' could not be found (are you missing a using directive or an assembly reference?)
```

To run toio SDK for Unity, you need to install UniTask, see [here](download_sdk.md#install-unitask).

## Simulator

### I'm not sure which mats will work best for my development.
> Keywords: Mat Specifications

The only difference between the various mats is the appearance and the coordinate range. For specifications, please refer to [toio™ Core Cube Technical Specification 2.1.0](https://toio.github.io/toio-spec/en/docs/hardware_position_id) and ["Developer's Mat (tentative name)"](https://toio.io/blog/detail/20200423-1.html).

If you simply want to run it in Simulator, it doesn't matr which one you use.<br>
If you want to develop an application that actually runs Cube and not just complete it in Simulator, you should use the same type of mat that you actually use.

Mat class also provides coordinate range, center coordinate, and coordinate transformation functions depending on the selected mat type, so please refer to [document](usage_simulator.md#2-mat-prefab).

### Is the Stage Prefab required?
> Keywords: Stage

It is not required.

As described in [Documentation](usage_simulator.md#5-stage-prefab), the Stage Prefab and Cube Prefab are a set of mats, cameras, components necessary for operation, etc. If you put the Stage Prefab and Cube Prefab into your scene, you can quickly set up a basic development environment.

### Is there anything I should be aware of if I don't use the Stage Prefab?
> Keywords: Stage

Of course, you will not be able to use the Stage's exclusive focus and target pole functions.

You will need to add the EventSystem, etc. needed to operate Cube on your own. For details, please refer to the [Documentation](usage_simulator.md#45-manipulating-cube-objects-cubeinteraction).

## Web application related

### Web apps do not work well on Windows PC
> Keywords： Windows Bluetooth BLE

On Windows PCs, problems have been confirmed with multiple-unit connections. Please check [[here]](build_web.md#unstable-multi-unit-connection-using-windows-pc).

### Can't start or load apps made with Unity 2020
> Keywords: Build Startup

Go to [Edit] -> [Project Settings...] -> [Player] -> [Publishing Settings] and try setting the `Compression Format` to `Disabled` or checking the `Decompression Fallback`. Note that the file size will increase in the former case.

※Reference: [[SOLVED] Unity 2020 WebGL Doesn't work Uncaught SyntaxError: Invalid or unexpected token](https://forum.unity.com/threads/solved-unity-2020-webgl-doesnt-work-uncaught-syntaxerror-invalid-or-unexpected-token.872581/#post-6480523)