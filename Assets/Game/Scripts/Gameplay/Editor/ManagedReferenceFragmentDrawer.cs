#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public abstract class ManagedReferenceFragmentDrawer<TFragment> :
        PropertyDrawer
        where TFragment : class
    {
        private static readonly Type[] FragmentTypes =
            TypeCache.GetTypesDerivedFrom<TFragment>()
                .Where(type =>
                    !type.IsAbstract &&
                    !type.IsGenericType &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.Name)
                .ToArray();

        private static readonly string[] FragmentNames =
            new[] { "None" }
                .Concat(
                    FragmentTypes.Select(type =>
                        ObjectNames.NicifyVariableName(type.Name)))
                .ToArray();

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(
                position,
                label,
                property);

            DrawHeader(position, property, label);

            if (property.isExpanded &&
                property.managedReferenceValue != null)
            {
                DrawChildren(position, property);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded ||
                property.managedReferenceValue == null)
            {
                return height;
            }

            var child = property.Copy();
            var end = child.GetEndProperty();
            var enterChildren = true;

            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                height +=
                    EditorGUIUtility.standardVerticalSpacing +
                    EditorGUI.GetPropertyHeight(
                        child,
                        includeChildren: true);

                enterChildren = false;
            }

            return height;
        }

        private static void DrawHeader(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            var line = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            var labelRect = new Rect(
                line.x,
                line.y,
                EditorGUIUtility.labelWidth,
                line.height);

            var popupRect = new Rect(
                labelRect.xMax,
                line.y,
                line.width - labelRect.width,
                line.height);

            if (property.managedReferenceValue != null)
            {
                property.isExpanded = EditorGUI.Foldout(
                    labelRect,
                    property.isExpanded,
                    label,
                    toggleOnLabelClick: true);
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            var currentType =
                property.managedReferenceValue?.GetType();

            var currentIndex = currentType == null
                ? 0
                : Array.IndexOf(FragmentTypes, currentType) + 1;

            EditorGUI.BeginChangeCheck();

            var selectedIndex = EditorGUI.Popup(
                popupRect,
                currentIndex,
                FragmentNames);

            if (!EditorGUI.EndChangeCheck())
                return;

            property.managedReferenceValue =
                selectedIndex == 0
                    ? null
                    : Activator.CreateInstance(
                        FragmentTypes[selectedIndex - 1]);

            property.isExpanded = selectedIndex != 0;
        }

        private static void DrawChildren(
            Rect position,
            SerializedProperty property)
        {
            var child = property.Copy();
            var end = child.GetEndProperty();
            var enterChildren = true;

            var y =
                position.y +
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;

            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                var height = EditorGUI.GetPropertyHeight(
                    child,
                    includeChildren: true);

                var childRect = new Rect(
                    position.x,
                    y,
                    position.width,
                    height);

                EditorGUI.PropertyField(
                    childRect,
                    child,
                    includeChildren: true);

                y +=
                    height +
                    EditorGUIUtility.standardVerticalSpacing;

                enterChildren = false;
            }

            EditorGUI.indentLevel--;
        }
    }
}

#endif