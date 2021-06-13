using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace OcUtility.Editor
{
public static class SceneViewController
{
    #region Add Rotation
    
    [Shortcut("Tools/SceneViewControl/SceneViewRotateLeft", KeyCode.Keypad4)]
    private static void SceneViewRotateLeft()
    {
        SceneViewRotate(new Vector3(0, 20, 0));
    }

    [Shortcut("Tools/SceneViewControl/SceneViewRotateRight", KeyCode.Keypad6)]
    private static void SceneViewRotateRight()
    {
        SceneViewRotate(new Vector3(0, -20, 0));
    }

    [Shortcut("Tools/SceneViewControl/SceneViewRotateAbove", KeyCode.Keypad8)]
    private static void SceneViewRotateAbove()
    {
        var angle = SceneView.lastActiveSceneView.rotation.eulerAngles.x;
        if (75 <= angle && angle <= 90) return;
        SceneViewRotate(new Vector3(15, 0, 0));
    }

    [Shortcut("Tools/SceneViewControl/SceneViewRotateBelow", KeyCode.Keypad2)]
    private static void SceneViewRotateBelow()
    {
        var angle = SceneView.lastActiveSceneView.rotation.eulerAngles.x;
        if (270 <= angle && angle <= 285) return;
        SceneViewRotate(new Vector3(-15, 0, 0));
    }

    #endregion

    #region Set Rotation

    [Shortcut("Tools/SceneViewControl/SetFront", KeyCode.Keypad1)]
    private static void SetFront()
    {
        SceneViewLookAt(Quaternion.Euler(0, 180, 0));
    }

    [Shortcut("Tools/SceneViewControl/SetBack", KeyCode.Keypad1, ShortcutModifiers.Action)]
    private static void SetBack()
    {
        SceneViewLookAt(Quaternion.Euler(0, 0, 0));
    }

    [Shortcut("Tools/SceneViewControl/SetLeft", KeyCode.Keypad3)]
    private static void SetLeft()
    {
        SceneViewLookAt(Quaternion.Euler(0, 90, 0));
    }

    [Shortcut("Tools/SceneViewControl/SetRight", KeyCode.Keypad3, ShortcutModifiers.Action)]
    private static void SetRight()
    {
        SceneViewLookAt(Quaternion.Euler(0, -90, 0));
    }
    
    [Shortcut("Tools/SceneViewControl/SetTop", KeyCode.Keypad7)]
    private static void SetTop()
    {
        SceneViewLookAt(Quaternion.Euler(90, 0, 0));
    }

    [Shortcut("Tools/SceneViewControl/SetBottom", KeyCode.Keypad7, ShortcutModifiers.Action)]
    private static void SetBottom()
    {
        SceneViewLookAt(Quaternion.Euler(-90, 0, 0));
    }

    [Shortcut("Tools/SceneViewControl/SetFree", KeyCode.Keypad0)]
    private static void SetFree()
    {
        SceneViewLookAt(Quaternion.Euler(45, -20, 0));
    }
    
    [Shortcut("Tools/SceneViewControl/InverseView", KeyCode.Keypad9)]
    private static void InverseView()
    {
        var q = SceneView.lastActiveSceneView.rotation * Quaternion.Euler(0, 180, 0);
        SceneViewLookAt(q);
    }

    #endregion

    #region Move
    
    [Shortcut("Tools/SceneViewControl/MoveFront", typeof(SceneView), KeyCode.Keypad8, ShortcutModifiers.Alt)]
    private static void MoveFront()
    {
        Move(-SceneView.lastActiveSceneView.size * 0.1f);
    }

    [Shortcut("Tools/SceneViewControl/MoveBackward", typeof(SceneView), KeyCode.Keypad2, ShortcutModifiers.Alt)]
    private static void MoveBackward()
    {
        Move(SceneView.lastActiveSceneView.size * 0.1f);
    }
    
    [Shortcut("Tools/SceneViewControl/MoveLeft", typeof(SceneView), KeyCode.Keypad4, ShortcutModifiers.Action)]
    private static void MoveLeft()
    {
        Move(SceneView.lastActiveSceneView.camera.transform.right * -1f);
    }

    [Shortcut("Tools/SceneViewControl/MoveRight", typeof(SceneView), KeyCode.Keypad6, ShortcutModifiers.Action)]
    private static void MoveRight()
    {
        Move(SceneView.lastActiveSceneView.camera.transform.right);
    }
    
    [Shortcut("Tools/SceneViewControl/MoveUp", typeof(SceneView), KeyCode.Keypad8, ShortcutModifiers.Action)]
    private static void MoveUp()
    {
        Move(Vector3.up * 0.5f);
    }

    [Shortcut("Tools/SceneViewControl/MoveDown", typeof(SceneView), KeyCode.Keypad2, ShortcutModifiers.Action)]
    private static void MoveDown()
    {
        Move(Vector3.down * 0.5f);
    }

    #endregion

    #region Others

    [Shortcut("Tools/SceneViewControl/ChangePerspective", typeof(SceneView), KeyCode.Keypad5)]
    private static void ChangePerspective()
    {
        SceneView.lastActiveSceneView.orthographic = !SceneView.lastActiveSceneView.orthographic;
    }
    
    [Shortcut("Tools/SceneViewControl/TimeScaleToZero", KeyCode.BackQuote, ShortcutModifiers.Alt)]
    private static void TimeScaleToZero()
    {
        if(!EditorApplication.isPlaying) return;
        var isSlowed = Time.timeScale < 1;
        Time.timeScale = isSlowed ? 1f : 0f;
        Debug.LogWarning($"[Alt + `]Time.timeScale => {Time.timeScale}");
    }
    
    [Shortcut("Tools/SceneViewControl/TimeScaleDown", KeyCode.KeypadMinus, ShortcutModifiers.Alt)]
    private static void TimeScaleDown()
    {
        if(!EditorApplication.isPlaying) return;
        Time.timeScale = Mathf.Clamp01(Time.timeScale - 0.2f);
        Debug.LogWarning($"[Alt + -]Time.timeScale => {Time.timeScale}");
    }
    [Shortcut("Tools/SceneViewControl/TimeScaleUp", KeyCode.KeypadPlus, ShortcutModifiers.Alt)]
    private static void TimeScaleUp()
    {
        if(!EditorApplication.isPlaying) return;
        Time.timeScale = Mathf.Clamp01(Time.timeScale + 0.2f);
        Debug.LogWarning($"[Alt + +]Time.timeScale => {Time.timeScale}");
    }
    #endregion


    static void Move(Vector3 pos)
    {
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.pivot += pos;
    }
    static void Move(float dir)
    {
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.size += dir;
    }

    static void SceneViewRotate(Vector3 eulerAngle)
    {
        var rot = SceneView.lastActiveSceneView.rotation.eulerAngles + eulerAngle;
        SceneViewLookAt(Quaternion.Euler(rot));
    }

    static void SceneViewLookAt(Quaternion rot)
    {
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.LookAt(sceneView.pivot, rot);
    }
    
}    
}
