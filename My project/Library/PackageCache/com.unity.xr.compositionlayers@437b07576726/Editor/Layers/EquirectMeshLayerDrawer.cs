using System;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    [CustomPropertyDrawer(typeof(EquirectMeshLayerData))]
    class EquirectMeshLayerDrawer : LayerDataDrawer
    {
        class Styles
        {
            internal GUIContent centralHorizontalAngleRadiansContent;
            internal GUIContent centralHorizontalAngleDegreesContent;
            internal GUIContent upperVerticalAngleRadiansContent;
            internal GUIContent upperVerticalAngleDegreesContent;
            internal GUIContent lowerVerticalAngleRadiansContent;
            internal GUIContent lowerVerticalAngleDegreesContent;

            internal Styles()
            {
                centralHorizontalAngleRadiansContent = new GUIContent(k_CentralHorizontalAngleNameRadians, k_CentralHorizontalAngleRadiansToolTip);
                centralHorizontalAngleDegreesContent = new GUIContent(k_CentralHorizontalAngleNameDegrees, k_CentralHorizontalAngleDegreesToolTip);
                upperVerticalAngleRadiansContent = new GUIContent(k_UpperVerticalAngleNameRadians, k_UpperVerticalAngleRadiansToolTip);
                upperVerticalAngleDegreesContent = new GUIContent(k_UpperVerticalAngleNameDegrees, k_UpperVerticalAngleDegreesToolTip);
                lowerVerticalAngleRadiansContent = new GUIContent(k_LowerVerticalAngleNameRadians, k_LowerVerticalAngleRadiansToolTip);
                lowerVerticalAngleDegreesContent = new GUIContent(k_LowerVerticalAngleNameDegrees, k_LowerVerticalAngleDegreesToolTip);
            }
        }

        const string k_AngleRadians = " (rad)";
        const string k_AngleDegrees = " (deg)";

        const float k_HalfPi = Mathf.PI * 0.5f;
        const float k_NegativeHalfPi = -k_HalfPi;
        const float k_TwoPi = Mathf.PI * 2f;

        const string k_CentralHorizontalAnglePropertyName = "m_CentralHorizontalAngle";
        const string k_UpperVerticalAnglePropertyName = "m_UpperVerticalAngle";
        const string k_LowerVerticalAnglePropertyName = "m_LowerVerticalAngle";
        const string k_RadiusPropertyName = "m_Radius";

        const string k_CentralHorizontalAngleName = "Central Hori. Angle";
        const string k_CentralHorizontalAngleNameRadians = k_CentralHorizontalAngleName + k_AngleRadians;
        const string k_CentralHorizontalAngleNameDegrees = k_CentralHorizontalAngleName + k_AngleDegrees;
        const string k_CentralHorizontalAngleRadiansToolTip = "The visible horizontal angle of the sphere, based at 0 " +
            "radians, in the range of [0, 2Pi]. It grows symmetrically around the 0 radian angle. " + DisplayAnglePreferenceToolTip;
        const string k_CentralHorizontalAngleDegreesToolTip = "The visible horizontal angle of the sphere, based at 0 " +
            "degrees, in the range of [0, 360]. It grows symmetrically around the 0 degree angle. " + DisplayAnglePreferenceToolTip;

        const string k_UpperVerticalAngleName = "Upper Vert. Angle";
        const string k_UpperVerticalAngleNameRadians = k_UpperVerticalAngleName + k_AngleRadians;
        const string k_UpperVerticalAngleNameDegrees = k_UpperVerticalAngleName + k_AngleDegrees;
        const string k_UpperVerticalAngleRadiansToolTip = "The upper vertical angle of the visible portion of the " +
            "sphere, in the range of [-Pi/2, Pi/2]. " + DisplayAnglePreferenceToolTip;
        const string k_UpperVerticalAngleDegreesToolTip = "The upper vertical angle of the visible portion of the " +
            "sphere, in the range of [-90, 90]. " + DisplayAnglePreferenceToolTip;

        const string k_LowerVerticalAngleName = "Lower Vert. Angle";
        const string k_LowerVerticalAngleNameRadians = k_LowerVerticalAngleName + k_AngleRadians;
        const string k_LowerVerticalAngleNameDegrees = k_LowerVerticalAngleName + k_AngleDegrees;
        const string k_LowerVerticalAngleRadiansToolTip = "The lower vertical angle of the visible portion of the " +
            "sphere, in the range of [-Pi/2, Pi/2]. " + DisplayAnglePreferenceToolTip;
        const string k_LowerVerticalAngleDegreesToolTip = "The lower vertical angle of the visible portion of the " +
            "sphere, in the range of [-90, 90]. " + DisplayAnglePreferenceToolTip;

        static Styles s_Styles;
        static Styles styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new Styles();

                return s_Styles;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || property.propertyType != SerializedPropertyType.ManagedReference || property.managedReferenceValue == null)
                return; // Fix crash on obsolete properties.

            var propertyCopy = property.Copy();
            if (propertyCopy.managedReferenceValue is not EquirectMeshLayerData equirectMeshLayerData)
                return;

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.LabelField(position, label);
                EditorGUI.indentLevel++;

                var enumerator = property.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var currentProperty = enumerator.Current as SerializedProperty;
                    if (currentProperty == null)
                        continue;

                    if (currentProperty.name is k_CentralHorizontalAnglePropertyName or k_UpperVerticalAnglePropertyName or k_LowerVerticalAnglePropertyName)
                    {
                        switch (CompositionLayersPreferences.Instance.DisplayAnglesAs)
                        {
                            case AngleDisplayType.Degrees:
                                {
                                    if (currentProperty.name == k_CentralHorizontalAnglePropertyName)
                                    {
                                        currentProperty.floatValue = EditorGUILayout.Slider(
                                            styles.centralHorizontalAngleDegreesContent,
                                            equirectMeshLayerData.CentralHorizontalAngleInDegrees,
                                            0f,
                                            360f) * Mathf.Deg2Rad;
                                    }
                                    else if (currentProperty.name == k_UpperVerticalAnglePropertyName)
                                    {
                                        currentProperty.floatValue = EditorGUILayout.Slider(
                                            styles.upperVerticalAngleDegreesContent,
                                            equirectMeshLayerData.UpperVerticalAngleInDegrees,
                                            -90f,
                                            90f) * Mathf.Deg2Rad;
                                    }
                                    else
                                    {
                                        currentProperty.floatValue = EditorGUILayout.Slider(
                                            styles.lowerVerticalAngleDegreesContent,
                                            equirectMeshLayerData.LowerVerticalAngleInDegrees,
                                            -90f,
                                            90f) * Mathf.Deg2Rad;
                                    }
                                    break;
                                }
                            case AngleDisplayType.Radians:
                                {
                                    if (currentProperty.name == k_CentralHorizontalAnglePropertyName)
                                    {
                                        currentProperty.floatValue = EditorGUILayout.Slider(
                                            styles.centralHorizontalAngleRadiansContent,
                                            equirectMeshLayerData.CentralHorizontalAngle,
                                            0f,
                                            k_TwoPi);
                                    }
                                    else if (currentProperty.name == k_UpperVerticalAnglePropertyName)
                                    {
                                        currentProperty.floatValue = EditorGUILayout.Slider(
                                            styles.upperVerticalAngleRadiansContent,
                                            equirectMeshLayerData.UpperVerticalAngle,
                                            k_NegativeHalfPi,
                                            k_HalfPi);
                                    }
                                    else
                                    {
                                        currentProperty.floatValue = EditorGUILayout.Slider(
                                            styles.lowerVerticalAngleRadiansContent,
                                            equirectMeshLayerData.LowerVerticalAngle,
                                            k_NegativeHalfPi,
                                            k_HalfPi);
                                    }
                                    break;
                                }
                        }
                    }
                    else if (currentProperty.name is k_RadiusPropertyName)
                    {
                        // Limit radius to positive values
                        currentProperty.floatValue = Mathf.Max(0f,
                            EditorGUILayout.DelayedFloatField(
                                new GUIContent(currentProperty.displayName, currentProperty.tooltip),
                                currentProperty.floatValue));
                    }
                    else if (currentProperty.name is k_BlendTypePropertyName)
                    {
                        BlendTypePropertyField(currentProperty);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(currentProperty);
                    }
                }

                if (change.changed)
                    ApplyChangesWithReportStateChange(property);
            }

            EditorGUI.EndProperty();
            EditorGUI.indentLevel--;
        }
    }
}
