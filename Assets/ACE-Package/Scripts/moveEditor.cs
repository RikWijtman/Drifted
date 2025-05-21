using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Moves))]
public class MovesDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty moveType = property.FindPropertyRelative("moveType");

        EditorGUILayout.PropertyField(property.FindPropertyRelative("moveName"));
        EditorGUILayout.PropertyField(moveType);
        EditorGUILayout.PropertyField(property.FindPropertyRelative("damage"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("cooldown"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("probability"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("baseProbability"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("moveRange"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("chargeTime"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("moveAnim"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("attackEffect"));

        Moves.AttackTypes type = (Moves.AttackTypes)moveType.enumValueIndex;

        switch (type)
        {
            case Moves.AttackTypes.Block:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("blockPercentage"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("blockTime"));
                break;

            case Moves.AttackTypes.Dash:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("dashSpeed"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("dashDuration"));
                break;

            case Moves.AttackTypes.Dodge:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("dodgeSpeed"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("dodgeDuration"));
                break;

            case Moves.AttackTypes.Healing:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("healingAmount"));
                break;

            case Moves.AttackTypes.Buffing:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("speedBuff"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("attackBuff"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("cooldownBuff"));
                break;

            case Moves.AttackTypes.Ranged:
                EditorGUILayout.PropertyField(property.FindPropertyRelative("projectile"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("projectileSpeed"));
                break;
        }

        EditorGUI.EndProperty();
    }
}
