using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameUIRaceProgressController))]
public class GameUIRaceProgressControllerInspector : Editor
{
    private float _progress;
    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        // if(target == null)
        // {
        //     return;
        // }

        // var ctrl = target as GameUIRaceProgressController;

        // var curProgress = EditorGUILayout.Slider(_progress, 0, 1);

        // if(_progress != curProgress)
        // {
        //     _progress = curProgress;
        //     ctrl.UpdatePlayerTranEditor(_progress);
        // }
    }
}
