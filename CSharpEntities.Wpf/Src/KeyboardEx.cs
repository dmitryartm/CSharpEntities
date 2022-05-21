using System.Windows.Input;


namespace CSharpEntities.Wpf;


public static class KeyboardEx {
    public static bool CtrlOrShiftDown => Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ||
        Keyboard.Modifiers.HasFlag(ModifierKeys.Control);


    public static bool CtrlAndShiftUntouched => !CtrlOrShiftDown;
}