using SharpDX.DirectInput;
using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        var directInput = new DirectInput();

        // ゲームパッドまたはジョイスティックの取得
        var joystickGuid = Guid.Empty;

        foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
        {
            joystickGuid = deviceInstance.InstanceGuid;
            break;
        }

        if (joystickGuid == Guid.Empty)
        {
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
            {
                joystickGuid = deviceInstance.InstanceGuid;
                break;
            }
        }

        if (joystickGuid == Guid.Empty)
        {
            Console.WriteLine("ジョイスティックやゲームパッドが見つかりません。");
            return;
        }

        var joystick = new Joystick(directInput, joystickGuid);
        Console.WriteLine($"使用中のデバイス: {joystick.Properties.InstanceName}");

        joystick.Properties.BufferSize = 128;
        joystick.Acquire();

        Console.WriteLine("入力を監視中（Ctrl+Cで終了）...");

        while (true)
        {
            joystick.Poll();
            var state = joystick.GetCurrentState();
            if (state == null) continue;

            Console.Clear();

            // 軸表示
            Console.WriteLine("Axes:");
            Console.WriteLine($"  X : {state.X}");
            Console.WriteLine($"  Y : {state.Y}");
            Console.WriteLine($"  Z : {state.Z}");
            Console.WriteLine($"  Rx: {state.RotationX}");
            Console.WriteLine($"  Ry: {state.RotationY}");
            Console.WriteLine($"  Rz: {state.RotationZ}");

            // スライダー
            var sliders = state.Sliders;
            for (int i = 0; i < sliders.Length; i++)
                Console.WriteLine($"  Slider[{i}]: {sliders[i]}");

            // POV（十字キー）
            Console.WriteLine("POVs (十字キー):");
            foreach (var pov in state.PointOfViewControllers)
            {
                string direction = PovToDirection(pov);
                Console.WriteLine($"  POV: {pov} ({direction})");
            }

            // ボタン
            Console.WriteLine("Buttons:");
            var buttons = state.Buttons;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i])
                    Console.WriteLine($"  Button {i + 1}: Pressed");
            }

            Thread.Sleep(10);
        }
    }

    // POV角度を方向に変換するヘルパー関数
    static string PovToDirection(int pov)
    {
        if (pov == -1) return "Centered";

        return pov switch
        {
            0 => "Up",
            4500 => "Up-Right",
            9000 => "Right",
            13500 => "Down-Right",
            18000 => "Down",
            22500 => "Down-Left",
            27000 => "Left",
            31500 => "Up-Left",
            _ => $"Unknown ({pov})"
        };
    }
}
