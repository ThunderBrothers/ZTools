using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;

namespace ZTools.EditorExample
{
    [System.Serializable]
    public struct MinMaxFloat
    {
        public float min;
        public float max;

        public float GetRandomValue()
        {
            return GetRangedValue(Randomizer.zeroToOne);
        }

        public float GetRangedValue(float v)
        {
            return v * (max - min) + min;
        }

        public float GetClampedValue(float v)
        {
            return Mathf.Clamp(v, min, max);
        }
    }
    public static class Randomizer
    {
        public const float denominator = 1f / (float)0x80000000;

        public static int seed;

        public static int next { get { return seed = (seed + 35757) * 31313; } }
        public static float plusMinusOne { get { return (float)next * denominator; } }
        public static float zeroToOne { get { return (plusMinusOne + 1f) * 0.5f; } }
    }
    public enum BulletType : int
    {
        Real = 0,
        Track = 1,
    }
    public class MinMaxAttribute : PropertyAttribute
    {
        public float min;
        public float max;
        public bool colorize;

        public MinMaxAttribute(float mv, float nv)
        {
            min = mv;
            max = nv;
        }
    }
    [Serializable]
    public class CABullet : ScriptableObject
    {
        public BulletType Type = BulletType.Real;
        public int Speed;
        public int Damage;
        public GameObject eff;
        [MinMax(0, 22000)]
        public MinMaxFloat highPassRange = new MinMaxFloat
        {
            min = 0,
            max = 1100,
        };
    }
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            var mp = prop.FindPropertyRelative("min");
            var np = prop.FindPropertyRelative("max");
            var mm = attribute as MinMaxAttribute;

            if (mp != null && np != null && mm != null)
            {
                var oldColor = GUI.color;
                if (mm.colorize)
                    GUI.color = ColorizeDrawer.GetColor(prop.propertyPath);

                int i = EditorGUI.indentLevel;

                float mv = mp.floatValue;
                float nv = np.floatValue;

                float dx1 = EditorGUIUtility.fieldWidth * 2 + Mathf.Clamp01(i) * 9;
                float dx2 = EditorGUIUtility.fieldWidth * 2 + (i - 1) * 9;

                Rect r = pos;
                r.width = r.width - dx1;
                EditorGUI.MinMaxSlider(
                   new GUIContent(ObjectNames.NicifyVariableName(prop.name)), r,
                    ref mv, ref nv, mm.min, mm.max);

                EditorGUI.indentLevel = 0;

                r.x = pos.width - dx2 + i * 9 + 3;
                r.width = EditorGUIUtility.fieldWidth;
                var s = new GUIStyle(EditorStyles.numberField);
                s.fixedWidth = EditorGUIUtility.fieldWidth;
                mv = EditorGUI.DelayedFloatField(r, mv, s);
                r.x += EditorGUIUtility.fieldWidth + 2;
                nv = EditorGUI.DelayedFloatField(r, nv, s);

                mp.floatValue = Mathf.Min(Mathf.Max(mv, mm.min), Mathf.Min(nv, mm.max));
                np.floatValue = Mathf.Max(Mathf.Max(mv, mm.min), Mathf.Min(nv, mm.max));

                EditorGUI.indentLevel = i;

                if (mm.colorize)
                    GUI.color = oldColor;
            }
        }
    }
    public class ColorizeAttribute : PropertyAttribute
    {
    }
    [CustomPropertyDrawer(typeof(ColorizeAttribute))]
    public class ColorizeDrawer : PropertyDrawer
    {
        static Dictionary<object, int> _lookup = new Dictionary<object, int>();

        static readonly Color _disabledColor = new Color(0.75f, 0.75f, 0.75f);

        static readonly Color[] _colors = new Color[] {
        new Color(0.85f, 1.00f, 1.00f),
        new Color(0.85f, 1.00f, 0.85f),
        new Color(0.95f, 1.00f, 0.75f),
        new Color(1.00f, 0.75f, 0.65f),
        new Color(1.00f, 0.75f, 0.95f),
        new Color(0.75f, 0.75f, 1.00f),
        new Color(0.75f, 0.85f, 1.00f)
    };

        static int _index;

        public static Color disabledColor
        {
            get { return _disabledColor; }
        }

        public static void Reset()
        {
            _lookup.Clear();
            _index = 0;
        }

        public static Color GetColor(int _index)
        {
            return _colors[_index % _colors.Length];
        }

        public static Color GetColor(string path)
        {
            int i, j;
            while ((i = path.IndexOf('[')) > 0)
            {
                j = path.IndexOf(']');
                path = path.Substring(0, i) + path.Substring(j + 1);
            }
            if (!_lookup.TryGetValue(path, out i))
            {
                i = _index;
                _lookup[path] = i;
                _index = i + 1;
            }
            return GetColor(i);
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(prop, label, true);
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            var oldColor = GUI.color;
            GUI.color = GetColor(prop.propertyPath);
            EditorGUI.PropertyField(pos, prop, label, true);
            GUI.color = oldColor;
        }
    }
}
