using System;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    [CustomPropertyDrawer(typeof(CylinderLayerData))]
    class CylinderLayerDataDrawer : LayerDataDrawer
    {
        class Styles
        {
            internal GUIContent centralAngleRadiansContent;
            internal GUIContent centralAngleDegreesContent;
            internal GUIStyle lockAspectRatioButton;

            internal Styles()
            {
                centralAngleRadiansContent = new GUIContent(k_CentralAngleNameRadians, k_CentralAngleRadiansToolTip);
                centralAngleDegreesContent = new GUIContent(k_CentralAngleNameDegrees, k_CentralAngleDegreesToolTip);

                lockAspectRatioButton = EditorStyles.iconButton;
                lockAspectRatioButton.contentOffset = new Vector2(2.2f, 2f);
                lockAspectRatioButton.margin = new RectOffset(0, 2, 2, 0);
            }
        }

        const string k_CentralAnglePropertyName = "m_CentralAngle";
        const string k_CentralAngleName = "Central Angle";
        const string k_ApplyTransformScaleName = "m_ApplyTransformScale";
        const string k_CentralAngleNameRadians = k_CentralAngleName + " (rad)";
        const string k_CentralAngleNameDegrees = k_CentralAngleName + " (deg)";
        const string k_CentralAngleRadiansToolTip = "The angle of the visible section of the cylinder, " +
            "based at 0 radians, in the range of [0, 2Pi). It grows symmetrically around the 0 radian angle. "
            + DisplayAnglePreferenceToolTip;
        const string k_CentralAngleDegreesToolTip = "The angle of the visible section of the cylinder, " +
            "based at 0 degress, in the range of [0, 360). It grows symmetrically around the 0 degress angle. "
            + DisplayAnglePreferenceToolTip;

        const string k_RadiusPropertyName = "m_Radius";
        const string k_AspectRatioPropertyName = "m_AspectRatio";
        const string k_MaintainAspectRatioPropertyName = "m_MaintainAspectRatio";
        const string k_MaintainAspectRatioToolTip = "|Toggles the option for locking and unlocking Radius and Aspect Ratio. " +
            "When Radius is locked, its value will be driven by the value of Height, and the Aspect Ratio value is unconstrained. When Aspect Ratio is locked, its value will be driven by the value of Height, and the Radius value is unconstrained.";

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
            if (propertyCopy.managedReferenceValue is not CylinderLayerData cylinderLayer)
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

                    if (currentProperty.name == k_CentralAnglePropertyName)
                    {
                        switch (CompositionLayersPreferences.Instance.DisplayAnglesAs)
                        {
                            case AngleDisplayType.Degrees:
                            {
                                float clampedCentralAngleInDegrees = Mathf.Clamp(cylinderLayer.CentralAngleInDegrees, 0f, 360f);
                                float roundedCentralAngleInDegrees = (float)Math.Round(clampedCentralAngleInDegrees, 2);

                                currentProperty.floatValue = EditorGUILayout.FloatField(
                                    styles.centralAngleDegreesContent,
                                    roundedCentralAngleInDegrees) * Mathf.Deg2Rad;
                                break;
                            }
                            case AngleDisplayType.Radians:
                            {
                                float clampedCentralAngle = Mathf.Clamp(cylinderLayer.CentralAngle, 0f, Mathf.PI * 2f);
                                float roundedCentralAngle = (float)Math.Round(clampedCentralAngle, 2);

                                currentProperty.floatValue = EditorGUILayout.FloatField(
                                    styles.centralAngleRadiansContent,
                                    roundedCentralAngle);
                                break;
                            }
                        }
                    }
                    else if (currentProperty.name == k_MaintainAspectRatioPropertyName)
                    {
                        // Skip rendering MaintainAspectRatio boolean
                    }
                    else if (currentProperty.name == k_RadiusPropertyName)
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(currentProperty);

                        // Render MaintainAspectRatio boolean as a button after Radius field

                        var lockIconContent = EditorGUIUtility.IconContent(cylinderLayer.MaintainAspectRatio == true ? "LockIcon-On" : "LockIcon", k_MaintainAspectRatioToolTip);

                        if (GUILayout.Button(lockIconContent, styles.lockAspectRatioButton, GUILayout.Width(18), GUILayout.Height(18)))
                            cylinderLayer.MaintainAspectRatio = !cylinderLayer.MaintainAspectRatio;

                        EditorGUILayout.EndHorizontal();
                    }
                    else if (currentProperty.name == k_AspectRatioPropertyName)
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(currentProperty);

                        // Render MaintainAspectRatio boolean as a button after AspectRatio field

                        var lockIconContent = EditorGUIUtility.IconContent(cylinderLayer.MaintainAspectRatio == true ? "LockIcon" : "LockIcon-On", k_MaintainAspectRatioToolTip);

                        if (GUILayout.Button(lockIconContent, styles.lockAspectRatioButton, GUILayout.Width(18), GUILayout.Height(18)))
                            cylinderLayer.MaintainAspectRatio = !cylinderLayer.MaintainAspectRatio;

                        EditorGUILayout.EndHorizontal();
                    }
                    else if (currentProperty.name == k_ApplyTransformScaleName)
                    {
                        // Draw height
                        var height = cylinderLayer.GetHeight();
                        height = EditorGUILayout.FloatField("Height", height);

                        if (change.changed)
                        {
                            if (cylinderLayer.MaintainAspectRatio == true)
                            {
                                var radius = cylinderLayer.CalculateRadiusFromHeight(height);
                                propertyCopy.FindPropertyRelative("m_Radius").floatValue = radius;
                                ApplyChangesWithReportStateChange(propertyCopy);
                            }
                            else
                            {
                                var aspectRatio = cylinderLayer.CalculateAspectRatioFromHeight(height);
                                propertyCopy.FindPropertyRelative("m_AspectRatio").floatValue = aspectRatio;
                                ApplyChangesWithReportStateChange(propertyCopy);
                            }
                        }

                        // Draw ApplyTransformScale
                        EditorGUILayout.PropertyField(currentProperty);
                    }
                    else if (currentProperty.name == k_BlendTypePropertyName)
                        BlendTypePropertyField(currentProperty);
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
