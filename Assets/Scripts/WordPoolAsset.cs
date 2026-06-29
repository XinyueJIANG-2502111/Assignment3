using System.Collections.Generic;
using UnityEngine;

// 这个标签允许你在 Project 面板右键菜单中直接创建这个数据文件
// This attribute allows you to create the asset directly from the Project context menu
[CreateAssetMenu(fileName = "NewWordPool", menuName = "CustomData/Word Pool Asset")]
public class WordPoolAsset : ScriptableObject
{
    [Header("Universal Trash Pool")]
    [Tooltip("在这里填入所有用于生成的名词短语 / Enter all your phrases here")]
    public List<string> everythingTrashPool = new List<string>()
    {
        // 这里可以写一些默认值 / Default values
        "Fucking Assignment", "value1", "value2", "value3"
    };
}