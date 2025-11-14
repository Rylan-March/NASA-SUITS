using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.XR.Hands.Capture
{
    static class EditorUIUtils
    {
        const int k_DefaultMargin = 5;
        const int k_DefaultFontSize = 12;

        internal static ObjectField CreateObjectField(string label, Type type)
        {
            var field = new ObjectField(label);
            field.style.marginBottom = k_DefaultMargin;
            field.style.marginRight = k_DefaultMargin;
            field.objectType = type;
            return field;
        }

        internal static Foldout CreateFoldout(string title)
        {
            var foldout = new Foldout { text = title };
            foldout.style.marginTop = k_DefaultMargin;
            foldout.style.marginBottom = k_DefaultMargin;

            // Make the title bolder and bigger
            var titleLabel = foldout.Q<Toggle>().Q<Label>();
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = k_DefaultFontSize;

            return foldout;
        }

        internal static Button CreateButton(string text, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.style.marginTop = k_DefaultMargin;
            button.style.marginBottom = k_DefaultMargin;
            button.style.marginRight = k_DefaultMargin;
            return button;
        }

        internal static Label CreateDescriptionLabel(string text)
        {
            var label = new Label(text);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = k_DefaultFontSize;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = k_DefaultMargin;
            label.style.marginBottom = k_DefaultMargin;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.flexWrap = Wrap.Wrap;
            label.style.overflow = Overflow.Hidden;
            return label;
        }
    }
}
