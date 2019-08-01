# GD Camera

**3C:** Camera, Control, Character

希望玩家看到怎样的画面。

如果移动过去会导致“穿帮”，则要限制玩家看到的画面。

游戏机位与电影机位有许多类似的地方，如果对电影有很多了解则更容易上手。需要靠视觉上的传达来“欺骗”玩家，达到震撼的效果。当触发了特定剧情后(关键节点)，机位会发生一定变化，如拉近镜头特写，改变重点等。用手柄体验相机。

**典型机位:** 从角色正后方一段距离往角色拍(如古墓丽影)；将主角放在屏幕中间偏左，提现移动的速度感(如战神)；鸟瞰(如刺客信条)；赛车游戏(第一人称更爽，第三人称更容易掌握道路状况，更容易)；FPS由两个相机组成(如战地，自己的枪被放大，其他人的枪正常比例)；RTS相机相对简单，地图上方俯视拍摄，难点在于鼠标移动在屏幕边缘时相应地移动相机。

**相机主要属性:**

FOV(Field of View, 视野):速度感

Projection(投影)

Movement(移动)

**第三人称相机:**

![Basic Setup](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/GD%20Camera/Basic%20Setup.png?raw=true)

![Vertical Follow](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/GD%20Camera/Vertical%20Follow.png?raw=true)

![Free Move Cylinder](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/GD%20Camera/Free%20Move%20Cylinder.png?raw=true)

![Standard Follow Mode](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/GD%20Camera/Standard%20Follow%20Mode.png?raw=true)

![Free Cam Controls](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/GD%20Camera/Free%20Cam%20Controls.png?raw=true)

![Free Cam Follow Mode](https://github.com/MilkyW/LearnUnityEveryday/blob/master/Pictures/GD%20Camera/Free%20Cam%20Follow%20Mode.png?raw=true)

```cs
    private void FixedUpdate()
    {
        AddjustFOV();
        Vector3 see = GetJoyStickRight();
        bool rerotate = false;

        // renew position and rotation
        if (System.Math.Abs(see.magnitude) < float.Epsilon)
        {
            if (!freeMoveCylinderSettings.enabled || OutOfFreeMoveCylinder())
                Basic();
        }
        else
        {
            FreeCameraControls(see);
            rerotate = true;
        }

        // interpolate position and rotation
        SmoothInterpolate(rerotate);

        CollisionFix();
    }
```
