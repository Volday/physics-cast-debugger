using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using UnityEditorInternal;

namespace PhysicsCastDebugger
{
    public class PhysicsCastDebuggerWindow : EditorWindow
    {
        private List<PhysicsCastInfo> physicsCastInfos = new List<PhysicsCastInfo>();
        private List<TableRow> tableRows = new List<TableRow>();

        private List<PhysicsCastInfo> selectedPhysicsCastInfo = new List<PhysicsCastInfo>();

        private bool isReceivingPhysicsCastInfo;
        [SerializeField] private bool isCollapsed;
        private string searchLine = "";
        private readonly static List<Type> supportedCastTypes = new List<Type>
        {
            typeof(RaycastInfo),
            typeof(SphereCastInfo),
            typeof(CapsuleCastInfo),
            typeof(BoxCastInfo),
        };
        [SerializeField] private bool[] disabledTypes = new bool[supportedCastTypes.Count];

        private static float singleLineHeight = EditorGUIUtility.singleLineHeight;
        private float stackTraceHeight = 100;
        private float splitterHandleThickness = 4;
        private bool splitterDragStarted;

        private const string prefsKey = "PhysicsCastDebuggerWindow";

        [SerializeField] private GizmosSettings gizmosSettings = new GizmosSettings();

        [SerializeField] private List<string> activeColumnNames = new List<string>()
        {
            "Call source",
            "Origin",
            "Layer mask",
            "Is collided",
            "Hit point",
            "Hit distance",
            "Hit object",
        };

        [MenuItem("Window/Analysis/Physics Cast Debugger", false, 904)]
        private static void Init()
        {
            var win = GetWindow<PhysicsCastDebuggerWindow>("Physics Cast Debugger");

            win.minSize = new Vector2(300, 300);
            win.isReceivingPhysicsCastInfo = true;
        }

        private void OnEnable()
        {
            var json = EditorJsonUtility.ToJson(this);
            json = EditorPrefs.GetString(prefsKey, json);
            EditorJsonUtility.FromJsonOverwrite(json, this);

            stickScrollToBottom = true;

            RaycastCallData.onPhysicsCastCall += OnPhysicsCast;
            SceneView.duringSceneGui += OnSceneGUI;

            InitializeTable();
        }

        private void OnDisable()
        {
            RaycastCallData.onPhysicsCastCall -= OnPhysicsCast;
            SceneView.duringSceneGui -= OnSceneGUI;

            var json = EditorJsonUtility.ToJson(this);
            EditorPrefs.SetString(prefsKey, json);
        }

        private void OnPhysicsCast(PhysicsCastInfo physicsCastInfo)
        {
            if (!isReceivingPhysicsCastInfo) return;

            physicsCastInfos.Add(physicsCastInfo);

            AddTableRow(physicsCastInfo);

            Repaint();

            SceneView.RepaintAll();
        }

        private void AddTableRow(PhysicsCastInfo physicsCastInfo)
        {
            var typeIndex = supportedCastTypes.IndexOf(physicsCastInfo.GetType());

            if (typeIndex == -1 || disabledTypes[typeIndex]) return;

            if (searchLine != "" && !physicsCastInfo.stackTrace.ToLower().Contains(searchLine.ToLower())) return;

            if (isCollapsed)
            {
                var tableRow = tableRows
                    .Where(t => t.physicsCastInfo.stackTrace == physicsCastInfo.stackTrace)
                    .FirstOrDefault();

                if (tableRow is null)
                {
                    tableRows.Add(new TableRow(physicsCastInfo));
                }
                else
                {
                    int index = tableRows.IndexOf(tableRow);
                    tableRows[index] = new TableRow(physicsCastInfo);
                    tableRows[index].collapsedItemsCount = tableRow.collapsedItemsCount + 1;
                }
            }
            else
            {
                tableRows.Add(new TableRow(physicsCastInfo));
            }
        }

        private void RefreshTableRows()
        {
            tableRows.Clear();
            for (int i = 0; i < physicsCastInfos.Count; i++)
            {
                AddTableRow(physicsCastInfos[i]);
            }
        }


        #region GUI

        class TableRow
        {
            public PhysicsCastInfo physicsCastInfo;
            public bool isRaycastHitFoldout;
            public int collapsedItemsCount = 1;

            public TableRow(PhysicsCastInfo physicsCastInfo)
            {
                this.physicsCastInfo = physicsCastInfo;
            }

            public float GetRowHeight()
            {
                if (isRaycastHitFoldout && physicsCastInfo.raycastHits.Length > 1)
                {
                    return singleLineHeight * physicsCastInfo.raycastHits.Length;
                }

                return singleLineHeight;
            }
        }

        class Column
        {
            public readonly MultiColumnHeaderState.Column columnHeader;
            public readonly Action<Rect, int> onGUI;
            public readonly Action sort;

            public Column(MultiColumnHeaderState.Column columnHeader, Action<Rect, int> onGUI, Action sort)
            {
                this.columnHeader = columnHeader;
                this.onGUI = onGUI;
                this.sort = sort;
            }
        }

        private MultiColumnHeaderState multiColumnHeaderState;
        private MultiColumnHeader multiColumnHeader;
        private Column[] columns;
        private void InitializeTable()
        {
            columns = new Column[]
            {
            new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Call source"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.LabelField(cellPosition, tableRows[index].physicsCastInfo.callSourceName);
                    EditorGUI.indentLevel--;

                    if(isCollapsed)
                    {
                        GUIStyle countBadge = "CN CountBadge";
                        GUIContent collapsedItemsCount = new GUIContent(tableRows[index].collapsedItemsCount.ToString());
                        var badgeSize = countBadge.CalcSize(collapsedItemsCount);
                        var badgeRect = new Rect(cellPosition)
                        {
                            x = cellPosition.x + cellPosition.width - badgeSize.x,
                            width = badgeSize.x,
                        };
                        GUI.Label(badgeRect, collapsedItemsCount, countBadge);
                    }
                },
                () => {}
            ),

            new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Origin"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    GUI.Label(cellPosition, tableRows[index].physicsCastInfo.origin.ToString());

                    if(Event.current.type == EventType.MouseUp && Event.current.button == 0 && cellPosition.Contains(Event.current.mousePosition))
                    {
                        var bounds = new Bounds(tableRows[index].physicsCastInfo.origin, Vector3.one * 2);
                        SceneView.lastActiveSceneView.Frame(bounds, false);
                    }
                },
                () => {}
            ),

            //new Column
            //(
            //    new MultiColumnHeaderState.Column()
            //    {
            //        allowToggleVisibility = true,
            //        autoResize = true,
            //        canSort = false,
            //        headerContent = new GUIContent("Max distance"),
            //        headerTextAlignment = TextAlignment.Center,
            //    },
            //    (Rect cellPosition, int index) =>
            //    {
            //        GUI.Label(cellPosition, tableRows[index].physicsCastInfo.maxDistance.ToString());
            //    },
            //    () => {}
            //),

            new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Layer mask"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    EditorGUI.MaskField(cellPosition, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(tableRows[index].physicsCastInfo.layerMask), InternalEditorUtility.layers);
                },
                () => {}
            ),

            new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Trigger interaction"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    GUI.Label(cellPosition, tableRows[index].physicsCastInfo.queryTriggerInteraction.ToString());
                },
                () => {}
            ),

            new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Is collided"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    if(tableRows[index].physicsCastInfo.raycastHits.Length > 1)
                    {
                        var label = $"{tableRows[index].physicsCastInfo.isCollided.ToString()}";
                        tableRows[index].isRaycastHitFoldout = EditorGUI.Foldout(cellPosition, tableRows[index].isRaycastHitFoldout, label);

                        if(!tableRows[index].isRaycastHitFoldout)
                        {
                            GUIStyle countBadge = "CN CountBadge";
                            GUIContent hitCount = new GUIContent(tableRows[index].physicsCastInfo.raycastHits.Length.ToString());
                            var badgeSize = countBadge.CalcSize(hitCount);
                            var badgeRect = new Rect(cellPosition)
                            {
                                x = cellPosition.x + cellPosition.width - badgeSize.x,
                                width = badgeSize.x,
                            };
                            GUI.Label(badgeRect, hitCount, countBadge);
                        }
                    }
                    else
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.LabelField(cellPosition, tableRows[index].physicsCastInfo.isCollided.ToString());
                        EditorGUI.indentLevel--;
                    }
                },
                () => {}
            ),

            new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Hit point"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    if(tableRows[index].isRaycastHitFoldout)
                    {
                        for (int i = 0; i < tableRows[index].physicsCastInfo.raycastHits.Length; i++)
                        {
                            GUI.Label(cellPosition, tableRows[index].physicsCastInfo.raycastHits[i].point.ToString());
                            
                            if(Event.current.type == EventType.MouseUp && Event.current.button == 0 && cellPosition.Contains(Event.current.mousePosition))
                            {
                                var bounds = new Bounds(tableRows[index].physicsCastInfo.raycastHits[i].point, Vector3.one * 2);
                                SceneView.lastActiveSceneView.Frame(bounds);
                            }

                            cellPosition.y += singleLineHeight;

                        }
                    }
                    else if(tableRows[index].physicsCastInfo.raycastHits.Length > 0)
                    {
                        GUI.Label(cellPosition, tableRows[index].physicsCastInfo.raycastHits[0].point.ToString());

                        if(Event.current.type == EventType.MouseUp && Event.current.button == 0 && cellPosition.Contains(Event.current.mousePosition))
                        {
                            var bounds = new Bounds(tableRows[index].physicsCastInfo.raycastHits[0].point, Vector3.one * 2);
                            SceneView.lastActiveSceneView.Frame(bounds, false);
                        }
                    }
                },
                () => {}
            ),

             new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Hit distance"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    if(tableRows[index].isRaycastHitFoldout)
                    {
                        for (int i = 0; i < tableRows[index].physicsCastInfo.raycastHits.Length; i++)
                        {
                            GUI.Label(cellPosition, tableRows[index].physicsCastInfo.raycastHits[i].distance.ToString());
                            cellPosition.y += singleLineHeight;
                        }
                    }
                    else if(tableRows[index].physicsCastInfo.raycastHits.Length > 0)
                    {
                        GUI.Label(cellPosition, tableRows[index].physicsCastInfo.raycastHits[0].distance.ToString());
                    }
                },
                () => {}
            ),

            new Column
            (
                new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = true,
                    autoResize = true,
                    canSort = false,
                    headerContent = new GUIContent("Hit object"),
                    headerTextAlignment = TextAlignment.Center,
                },
                (Rect cellPosition, int index) =>
                {
                    Collider[] colliders;
                    if(tableRows[index].physicsCastInfo.raycastHits.Length > 0)
                    {
                        colliders = tableRows[index].physicsCastInfo.raycastHits
                            .Select(t => t.collider)
                            .ToArray();
                    }
                    else if (tableRows[index].physicsCastInfo.colliders?.Length > 0)
                    {
                        colliders = tableRows[index].physicsCastInfo.colliders;
                    }
                    else
                    {
                        colliders = new Collider[0];
                    }

                    if(tableRows[index].isRaycastHitFoldout)
                    {
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            GUI.Label(cellPosition, colliders[i].name.ToString());

                            if(Event.current.type == EventType.MouseUp && Event.current.button == 0 && cellPosition.Contains(Event.current.mousePosition))
                            {
                                Selection.activeGameObject = colliders[i].gameObject;
                                SceneView.lastActiveSceneView.FrameSelected();
                            }

                            cellPosition.y += singleLineHeight;
                        }
                    }
                    else if(colliders.Length > 0)
                    {
                        GUI.Label(cellPosition, colliders[0].name.ToString());

                        if(Event.current.type == EventType.MouseUp && Event.current.button == 0 && cellPosition.Contains(Event.current.mousePosition))
                        {
                            Selection.activeGameObject = colliders[0].gameObject;
                            SceneView.lastActiveSceneView.FrameSelected();
                        }
                    }
                },
                () => {}
            ),
            };

            multiColumnHeaderState = new MultiColumnHeaderState(columns.Select(t => t.columnHeader).ToArray());
            multiColumnHeaderState.visibleColumns = GetVisibleColumns();
            multiColumnHeader = new MultiColumnHeader(multiColumnHeaderState);

            multiColumnHeader.visibleColumnsChanged += (multiColumnHeader) => multiColumnHeader.ResizeToFit();
            multiColumnHeader.visibleColumnsChanged += UpdateActiveColumnNames;
            multiColumnHeader.ResizeToFit();

            int[] GetVisibleColumns()
            {
                List<int> visibleColumns = new List<int>();
                for (int i = 0; i < columns.Length; i++)
                {
                    if (activeColumnNames.Contains(columns[i].columnHeader.headerContent.text))
                    {
                        visibleColumns.Add(i);
                    }
                }
                return visibleColumns.ToArray();
            }

            void UpdateActiveColumnNames(MultiColumnHeader multiColumnHeader)
            {
                activeColumnNames = multiColumnHeaderState.visibleColumns
                    .Select(t => columns[t].columnHeader.headerContent.text)
                    .ToList();
            }
        }

        private void OnGUI()
        {
            var windowRect = new Rect(position)
            {
                x = 0,
                y = 0,
            };

            var toolbarRect = new Rect(windowRect)
            {
                height = singleLineHeight + 1,
            };
            DrawToolbar(toolbarRect);

            var tableRect = new Rect(windowRect)
            {
                y = toolbarRect.y + toolbarRect.height,
                height = windowRect.height - (stackTraceHeight + toolbarRect.height),
            };

            var splitterHandleRect = new Rect(tableRect)
            {
                y = tableRect.y + tableRect.height - splitterHandleThickness,
                height = splitterHandleThickness * 2,
            };
            EditorGUIUtility.AddCursorRect(splitterHandleRect, MouseCursor.ResizeVertical);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && splitterHandleRect.Contains(Event.current.mousePosition))
            {
                splitterDragStarted = true;
                Event.current.Use();
            }

            if (splitterDragStarted && Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                splitterDragStarted = false;
                Event.current.Use();
            }

            if (splitterDragStarted && !windowRect.Contains(Event.current.mousePosition))
            {
                splitterDragStarted = false;
            }

            if (splitterDragStarted)
            {
                stackTraceHeight = windowRect.height - Event.current.mousePosition.y;
                stackTraceHeight = Mathf.Clamp(stackTraceHeight, singleLineHeight * 2, windowRect.height - toolbarRect.height - singleLineHeight * 3);
                Repaint();
            }

            tableRect.height = windowRect.height - (stackTraceHeight + toolbarRect.height);

            var splitterRect = new Rect(tableRect)
            {
                y = tableRect.y + tableRect.height,
                height = 1,
            };
            EditorGUI.DrawRect(splitterRect, Color.black);

            var stackTraceRect = new Rect(tableRect)
            {
                y = tableRect.y + tableRect.height + splitterHandleThickness,
                height = stackTraceHeight - splitterHandleThickness,
            };

            DrawTable(tableRect);
            DrawStackTrace(stackTraceRect);
        }

        private void DrawToolbar(Rect rect)
        {
            var toolbarLeftElementRect = new Rect(rect)
            {
                width = 80,
            };

            GUIContent stopStart = new GUIContent(
                isReceivingPhysicsCastInfo ? "Stop" : "Start",
                isReceivingPhysicsCastInfo ? "Press to stop receiving physics cas info" : "Press to start receiving physics cas info");
            isReceivingPhysicsCastInfo = GUI.Toggle(toolbarLeftElementRect, isReceivingPhysicsCastInfo, stopStart, "ToolbarButton");
            toolbarLeftElementRect.x += 80;

            GUIContent clear = new GUIContent("Clear");
            if (GUI.Button(toolbarLeftElementRect, clear, "ToolbarButton"))
            {
                physicsCastInfos.Clear();
                RefreshTableRows();
                selectedPhysicsCastInfo.Clear();
                stickScrollToBottom = true;
            }
            toolbarLeftElementRect.x += 80;

            GUIContent collapse = new GUIContent("Collapse");
            EditorGUI.BeginChangeCheck();
            isCollapsed = GUI.Toggle(toolbarLeftElementRect, isCollapsed, collapse, "ToolbarButton");
            if (EditorGUI.EndChangeCheck())
            {
                RefreshTableRows();
            }
            toolbarLeftElementRect.x += 80;

            GUIContent gizmos = new GUIContent("Gizmos");
            var gizmosButtonStyle = EditorStyles.toolbarDropDown;
            gizmosButtonStyle.alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(toolbarLeftElementRect, gizmos, gizmosButtonStyle))
            {
                GizmosSettingsWindow.ShowAtPosition(toolbarLeftElementRect, gizmosSettings);
            }
            toolbarLeftElementRect.x += 80;

            EditorGUI.BeginChangeCheck();

            var toolbarRightElementRect = new Rect(rect)
            {
                x = rect.x + rect.width - 30,
                width = 30,
            };
            GUIContent capsuleCastIcon = EditorGUIUtility.IconContent("d_CapsuleCollider Icon");
            var capsuleCastTypeIndex = supportedCastTypes.IndexOf(typeof(CapsuleCastInfo));
            disabledTypes[capsuleCastTypeIndex] = !GUI.Toggle(toolbarRightElementRect, !disabledTypes[capsuleCastTypeIndex], capsuleCastIcon, "ToolbarButton");

            toolbarRightElementRect = new Rect(toolbarRightElementRect)
            {
                x = toolbarRightElementRect.x - 30,
            };
            GUIContent boxCastIcon = EditorGUIUtility.IconContent("d_BoxCollider Icon");
            var boxCastTypeIndex = supportedCastTypes.IndexOf(typeof(BoxCastInfo));
            disabledTypes[boxCastTypeIndex] = !GUI.Toggle(toolbarRightElementRect, !disabledTypes[boxCastTypeIndex], boxCastIcon, "ToolbarButton");

            toolbarRightElementRect = new Rect(toolbarRightElementRect)
            {
                x = toolbarRightElementRect.x - 30,
            };
            GUIContent sphereCastIcon = EditorGUIUtility.IconContent("d_SphereCollider Icon");
            var sphereCastTypeIndex = supportedCastTypes.IndexOf(typeof(SphereCastInfo));
            disabledTypes[sphereCastTypeIndex] = !GUI.Toggle(toolbarRightElementRect, !disabledTypes[sphereCastTypeIndex], sphereCastIcon, "ToolbarButton");

            toolbarRightElementRect = new Rect(toolbarRightElementRect)
            {
                x = toolbarRightElementRect.x - 30,
            };
            GUIContent raycastIcon = EditorGUIUtility.IconContent("d_RaycastCollider Icon");
            var raycastTypeIndex = supportedCastTypes.IndexOf(typeof(RaycastInfo));
            disabledTypes[raycastTypeIndex] = !GUI.Toggle(toolbarRightElementRect, !disabledTypes[raycastTypeIndex], raycastIcon, "ToolbarButton");

            const float searchLineRectMaxWidth = 300f;
            const float searchLineRectMinWidth = 100f;
            float searchLineRectWidth = Mathf.Min(toolbarRightElementRect.x - toolbarLeftElementRect.x, searchLineRectMaxWidth);
            var searchLineRect = new Rect(toolbarLeftElementRect)
            {
                y = toolbarLeftElementRect.y + 1,
                x = toolbarRightElementRect.x - searchLineRectWidth + 1,
                width = searchLineRectWidth - 3,
            };
            if (Event.current.type == EventType.MouseUp && !searchLineRect.Contains(Event.current.mousePosition) || Event.current.type == EventType.Used)
            {
                if (GUI.GetNameOfFocusedControl() == "searchLine")
                {
                    EditorGUI.FocusTextInControl(null);
                    Repaint();
                }
            }

            if (searchLineRectWidth >= searchLineRectMinWidth)
            {
                GUI.SetNextControlName("searchLine");
                searchLine = EditorGUI.TextField(searchLineRect, searchLine, new GUIStyle("SearchTextField"));
            }

            if (EditorGUI.EndChangeCheck())
            {
                RefreshTableRows();
            }

            var underlineRect = new Rect(rect)
            {
                y = rect.y + rect.height - 1,
                height = 1,
            };
            EditorGUI.DrawRect(underlineRect, Color.black);
        }

        private readonly Color lighterColor = Color.white * 0.3f;
        private readonly Color darkerColor = Color.white * 0.1f;
        private Vector2 scrollPosition;
        private bool stickScrollToBottom = true;
        private void DrawTable(Rect rect)
        {
            Rect tableRect = rect;

            Rect headerRect = new Rect(tableRect)
            {
                height = singleLineHeight,
                width = tableRect.width + scrollPosition.x,
                x = tableRect.x - scrollPosition.x
            };

            float lastColumnBottomY = headerRect.y + headerRect.height;

            multiColumnHeader.OnGUI(headerRect, 0.0f);

            Rect positionalRectAreaOfScrollView = new Rect(tableRect)
            {
                y = lastColumnBottomY,
                height = tableRect.height - headerRect.height
            };

            Rect viewRect = new Rect(tableRect)
            {
                yMin = lastColumnBottomY,
                xMax = multiColumnHeaderState.widthOfAllVisibleColumns,
                height = GetTableViewRectHeight(),
            };

            if (stickScrollToBottom)
            {
                scrollPosition.y = viewRect.height - positionalRectAreaOfScrollView.height;
            }

            EditorGUI.BeginChangeCheck();
            scrollPosition = GUI.BeginScrollView(
                positionalRectAreaOfScrollView,
                scrollPosition,
                viewRect,
                false,
                false
            );
            if (EditorGUI.EndChangeCheck() || Event.current.isScrollWheel)
            {
                stickScrollToBottom = (int)(viewRect.height - positionalRectAreaOfScrollView.height) == (int)(scrollPosition.y);
            }

            for (int i = 0; i < tableRows.Count; i++)
            {
                Rect rowRect = new Rect(tableRect)
                {
                    height = tableRows[i].GetRowHeight(),
                    width = tableRect.width + scrollPosition.x,
                    y = lastColumnBottomY
                };
                lastColumnBottomY = rowRect.y + rowRect.height;

                if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && rowRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.shift)
                    {
                        var lastSelected = selectedPhysicsCastInfo.LastOrDefault();
                        var lastSelectedTableRow = tableRows
                                .Where(t => t.physicsCastInfo == lastSelected)
                                .FirstOrDefault();
                        if (lastSelectedTableRow is null)
                        {
                            selectedPhysicsCastInfo.Clear();
                            selectedPhysicsCastInfo.Add(tableRows[i].physicsCastInfo);
                        }
                        else
                        {
                            var lastSelectedTableRowIndex = tableRows.IndexOf(lastSelectedTableRow);
                            var upIndex = Mathf.Min(i, lastSelectedTableRowIndex);
                            var bottomIndex = Mathf.Max(i, lastSelectedTableRowIndex);
                            if (selectedPhysicsCastInfo.Contains(tableRows[i].physicsCastInfo))
                            {
                                for (int j = upIndex; j <= bottomIndex; j++)
                                {
                                    if (selectedPhysicsCastInfo.Contains(tableRows[j].physicsCastInfo))
                                        selectedPhysicsCastInfo.Remove(tableRows[j].physicsCastInfo);
                                }
                            }
                            else
                            {
                                for (int j = upIndex; j <= bottomIndex; j++)
                                {
                                    if (!selectedPhysicsCastInfo.Contains(tableRows[j].physicsCastInfo))
                                        selectedPhysicsCastInfo.Add(tableRows[j].physicsCastInfo);
                                }
                            }

                            // Remove and add clicked element to make them the last added elments
                            if (selectedPhysicsCastInfo.Contains(tableRows[i].physicsCastInfo))
                                selectedPhysicsCastInfo.Remove(tableRows[i].physicsCastInfo);
                            selectedPhysicsCastInfo.Add(tableRows[i].physicsCastInfo);
                        }
                        Event.current.Use();
                    }
                    else if (Event.current.control)
                    {
                        if (selectedPhysicsCastInfo.Contains(tableRows[i].physicsCastInfo))
                        {
                            selectedPhysicsCastInfo.Remove(tableRows[i].physicsCastInfo);
                        }
                        else
                        {
                            selectedPhysicsCastInfo.Add(tableRows[i].physicsCastInfo);
                        }
                        Event.current.Use();
                    }
                    else
                    {
                        selectedPhysicsCastInfo.Clear();
                        selectedPhysicsCastInfo.Add(tableRows[i].physicsCastInfo);
                    }

                    stickScrollToBottom = false;

                    Repaint();
                    SceneView.RepaintAll();
                }

                if (rowRect.y + rowRect.height < scrollPosition.y)
                {
                    continue;
                }

                if (rowRect.y > positionalRectAreaOfScrollView.y + positionalRectAreaOfScrollView.height + scrollPosition.y)
                {
                    break;
                }

                if (selectedPhysicsCastInfo.Contains(tableRows[i].physicsCastInfo))
                {
                    EditorGUI.DrawRect(rowRect, GUI.skin.settings.selectionColor);
                }
                else
                {
                    if (i % 2 == 0)
                        EditorGUI.DrawRect(rowRect, darkerColor);
                    else
                        EditorGUI.DrawRect(rowRect, lighterColor);
                }

                for (int t = 0; t < columns.Length; t++)
                {
                    if (multiColumnHeader.IsColumnVisible(t))
                    {
                        int visibleColumnIndex = multiColumnHeader.GetVisibleColumnIndex(t);

                        Rect columnRect = multiColumnHeader.GetColumnRect(visibleColumnIndex);

                        columnRect.y = rowRect.y;

                        Rect cellPosition = multiColumnHeader.GetCellRect(visibleColumnIndex, columnRect);
                        cellPosition.x += tableRect.x;

                        columns[t].onGUI(cellPosition, i);
                    }
                }
            }

            GUI.EndScrollView(handleScrollWheel: true);

            float GetTableViewRectHeight()
            {
                float viewRectHeight = 0;

                foreach (var tableRow in tableRows)
                {
                    viewRectHeight += tableRow.GetRowHeight();
                }

                return viewRectHeight;
            }
        }

        private Vector2 stackTraceScrollPos;
        private void DrawStackTrace(Rect rect)
        {
            GUILayout.BeginArea(rect);
            stackTraceScrollPos = GUILayout.BeginScrollView(stackTraceScrollPos);
            GUIStyle stile = "CN Message";
            string stackWithHyperlinks = selectedPhysicsCastInfo.LastOrDefault()?.stackTrace;
            if (stackWithHyperlinks is null) stackWithHyperlinks = "";
            float height = stile.CalcHeight(new GUIContent(stackWithHyperlinks), rect.width);
            EditorGUILayout.SelectableLabel(stackWithHyperlinks, stile, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(height + 10));
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        #endregion

        #region Gizmos

        void OnSceneGUI(SceneView sceneView)
        {
            var originalColor = Handles.color;

            if (gizmosSettings.drawSelected)
            {
                var selectedPhysicsCast = tableRows
                    .Where(t => selectedPhysicsCastInfo.Contains(t.physicsCastInfo))
                    .Select(t => t.physicsCastInfo)
                    .ToList();
                DrawGizmos(selectedPhysicsCastInfo);
            }

            if (gizmosSettings.drawNewCast)
            {
                var currentTime = DateTime.UtcNow;
                var lifeTimeMilliseconds = gizmosSettings.gizmosLifeTime * 1000;
                var newPhysicsCastInfos = tableRows
                    .Where(t => (currentTime - t.physicsCastInfo.castTime).TotalMilliseconds < lifeTimeMilliseconds)
                    .Select(t => t.physicsCastInfo)
                    .ToList();

                DrawGizmos(newPhysicsCastInfos, gizmosSettings.usefadeEffect);

                if(newPhysicsCastInfos.Count > 0)
                    SceneView.RepaintAll();
            }

            Handles.color = originalColor;
        }

        private void DrawGizmos(List<PhysicsCastInfo> physicsCasts, bool fadeEffect = false)
        {
            if (gizmosSettings.drawSelected || gizmosSettings.drawNewCast)
                DrawPhysicsCasts(physicsCasts, fadeEffect);
        }

        private void DrawPhysicsCasts(List<PhysicsCastInfo> physicsCasts, bool fadeEffect = false)
        {
            Handles.color = gizmosSettings.castColor;

            var raycasts = physicsCasts
                .Where(t => t.GetType() == typeof(RaycastInfo))
                .Select(t => t as RaycastInfo)
                .ToList();
            var sphereCasts = physicsCasts
                .Where(t => t.GetType() == typeof(SphereCastInfo))
                .Select(t => t as SphereCastInfo)
                .ToList();
            var boxCasts = physicsCasts
               .Where(t => t.GetType() == typeof(BoxCastInfo))
               .Select(t => t as BoxCastInfo)
               .ToList();
            var capsuleCasts = physicsCasts
               .Where(t => t.GetType() == typeof(CapsuleCastInfo))
               .Select(t => t as CapsuleCastInfo)
               .ToList();

            DrawRays(physicsCasts, fadeEffect);

            DrawCollidedSphere(sphereCasts, fadeEffect);
            DrawCollidedBox(boxCasts, fadeEffect);
            DrawCollidedCapsules(capsuleCasts, fadeEffect);

            if (gizmosSettings.drawOrigin)
                DrawOrigins(physicsCasts, fadeEffect);

            if (gizmosSettings.drawHitPoint)
                DrawHitPoints(physicsCasts, fadeEffect);
        }

        private void DrawOrigins(List<PhysicsCastInfo> physicsCasts, bool fadeEffect = false)
        {
            Handles.color = gizmosSettings.originPointColor;
            foreach (var physicsCast in physicsCasts)
            {
                if (fadeEffect)
                {
                    Handles.color = gizmosSettings.originPointColor * GetFadeCoeff(physicsCast);
                }
                DrawPoint(physicsCast.origin);
            }
        }

        private void DrawHitPoints(List<PhysicsCastInfo> physicsCasts, bool fadeEffect = false)
        {
            Handles.color = gizmosSettings.hitPointColor;
            foreach (var physicsCast in physicsCasts)
            {
                foreach (var hit in physicsCast.raycastHits)
                {
                    if (fadeEffect)
                    {
                        Handles.color = gizmosSettings.hitPointColor * GetFadeCoeff(physicsCast);
                    }
                    DrawPoint(hit.point);
                }
            }
        }

        private void DrawRays(List<PhysicsCastInfo> physicsCasts, bool fadeEffect = false)
        {
            const float maxRayDrawDistance = float.MaxValue / 5f;
            foreach (var physicsCast in physicsCasts)
            {
                if (fadeEffect)
                {
                    Handles.color = gizmosSettings.castColor * GetFadeCoeff(physicsCast);
                }
                Handles.DrawLine(physicsCast.origin, physicsCast.origin + physicsCast.direction.normalized * Mathf.Min(physicsCast.maxDistance, maxRayDrawDistance));
            }
        }

        private void DrawCollidedSphere(List<SphereCastInfo> sphereCasts, bool fadeEffect = false)
        {
            foreach (var sphereCast in sphereCasts)
            {
                if (fadeEffect)
                {
                    Handles.color = gizmosSettings.castColor * GetFadeCoeff(sphereCast);
                }

                if (sphereCast.maxDistance == 0 && sphereCast.raycastHits.Length == 0)
                {
                    DrawSphere(sphereCast.origin, sphereCast.radius);
                }
                else
                {
                    foreach (var hit in sphereCast.raycastHits)
                    {
                        var pointOnRay = Vector3.Project(hit.point - sphereCast.origin, sphereCast.direction.normalized) + sphereCast.origin;
                        var sqrDistanceToRay = (hit.point - pointOnRay).sqrMagnitude;
                        var distanceFromPointOnRayToSphereCenter = Mathf.Sqrt(sphereCast.radius * sphereCast.radius - sqrDistanceToRay);
                        var distanceFromOriginToPointOnRay = (pointOnRay - sphereCast.origin).magnitude;
                        var distanceFromOriginToSphereCenter = distanceFromOriginToPointOnRay - distanceFromPointOnRayToSphereCenter;
                        var sphereCenter = sphereCast.origin + sphereCast.direction.normalized * distanceFromOriginToSphereCenter;
                        DrawSphere(sphereCenter, sphereCast.radius);
                    }
                }
            }
        }

        private void DrawCollidedBox(List<BoxCastInfo> boxCasts, bool fadeEffect = false)
        {
            foreach (var boxCast in boxCasts)
            {
                if (fadeEffect)
                {
                    Handles.color = gizmosSettings.castColor * GetFadeCoeff(boxCast);
                }

                if (boxCast.maxDistance == 0 && boxCast.raycastHits.Length == 0)
                {
                    DrawBox(boxCast.origin, boxCast.halfExtents, boxCast.orientation);
                }
                else
                {
                    // halfExtents was multiplied by 1.002f to fix float inaccuracy problem
                    var loaclPoints = GetBoxLocalPoints(boxCast.halfExtents * 1.002f, boxCast.orientation);
                    var points = loaclPoints
                        .Select(t => t + boxCast.origin)
                        .ToArray();

                    foreach (var hit in boxCast.raycastHits)
                    {
                        float hitDistance;
                        var ray = new Ray(hit.point, -boxCast.direction.normalized);

                        var topPlane = new Plane(points[0], points[2], points[1]);
                        if (topPlane.Raycast(ray, out hitDistance) && Vector3.Dot(-boxCast.direction.normalized, topPlane.normal) < 0)
                        {
                            if (MathUtils.IntersectRayTriangle(ray, points[0], points[2], points[1], true) != null ||
                                MathUtils.IntersectRayTriangle(ray, points[0], points[3], points[2], true) != null)
                            {
                                DrawBox(boxCast.origin + boxCast.direction.normalized * hitDistance, boxCast.halfExtents, boxCast.orientation);
                                continue;
                            }
                        }

                        var bottomPlane = new Plane(points[4], points[5], points[6]);
                        if (bottomPlane.Raycast(ray, out hitDistance) && Vector3.Dot(-boxCast.direction.normalized, bottomPlane.normal) < 0)
                        {
                            if (MathUtils.IntersectRayTriangle(ray, points[4], points[5], points[6], true) != null ||
                                MathUtils.IntersectRayTriangle(ray, points[4], points[6], points[7], true) != null)
                            {
                                DrawBox(boxCast.origin + boxCast.direction.normalized * hitDistance, boxCast.halfExtents, boxCast.orientation);
                                continue;
                            }
                        }

                        var frontPlane = new Plane(points[0], points[5], points[4]);
                        if (frontPlane.Raycast(ray, out hitDistance) && Vector3.Dot(-boxCast.direction.normalized, frontPlane.normal) < 0)
                        {
                            if (MathUtils.IntersectRayTriangle(ray, points[0], points[5], points[4], true) != null ||
                                MathUtils.IntersectRayTriangle(ray, points[0], points[1], points[5], true) != null)
                            {
                                DrawBox(boxCast.origin + boxCast.direction.normalized * hitDistance, boxCast.halfExtents, boxCast.orientation);
                                continue;
                            }
                        }

                        var backPlane = new Plane(points[2], points[7], points[6]);
                        if (backPlane.Raycast(ray, out hitDistance) && Vector3.Dot(-boxCast.direction.normalized, backPlane.normal) < 0)
                        {
                            if (MathUtils.IntersectRayTriangle(ray, points[2], points[7], points[6], true) != null ||
                                MathUtils.IntersectRayTriangle(ray, points[2], points[3], points[7], true) != null)
                            {
                                DrawBox(boxCast.origin + boxCast.direction.normalized * hitDistance, boxCast.halfExtents, boxCast.orientation);
                                continue;
                            }
                        }

                        var rightPlane = new Plane(points[0], points[4], points[7]);
                        if (rightPlane.Raycast(ray, out hitDistance) && Vector3.Dot(-boxCast.direction.normalized, rightPlane.normal) < 0)
                        {
                            if (MathUtils.IntersectRayTriangle(ray, points[0], points[4], points[7], true) != null ||
                                MathUtils.IntersectRayTriangle(ray, points[0], points[7], points[3], true) != null)
                            {
                                DrawBox(boxCast.origin + boxCast.direction.normalized * hitDistance, boxCast.halfExtents, boxCast.orientation);
                                continue;
                            }
                        }

                        var leftPlane = new Plane(points[1], points[6], points[5]);
                        if (leftPlane.Raycast(ray, out hitDistance) && Vector3.Dot(-boxCast.direction.normalized, leftPlane.normal) < 0)
                        {
                            if (MathUtils.IntersectRayTriangle(ray, points[1], points[6], points[5], true) != null ||
                                MathUtils.IntersectRayTriangle(ray, points[1], points[2], points[6], true) != null)
                            {
                                DrawBox(boxCast.origin + boxCast.direction.normalized * hitDistance, boxCast.halfExtents, boxCast.orientation);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        private void DrawCollidedCapsules(List<CapsuleCastInfo> capsuleCasts, bool fadeEffect = false)
        {
            //TO DO
        }

        private void DrawPoint(Vector3 point)
        {
            var size = HandleUtility.GetHandleSize(point) * gizmosSettings.pointSize;
            Handles.DrawLine(-Vector3.forward * size + point, Vector3.forward * size + point, 1f);
            Handles.DrawLine(-Vector3.right * size + point, Vector3.right * size + point, 1f);
            Handles.DrawLine(-Vector3.up * size + point, Vector3.up * size + point, 1f);
        }

        private void DrawSphere(Vector3 position, float radius)
        {
            Handles.DrawWireDisc(position, Vector3.right, radius);
            Handles.DrawWireDisc(position, Vector3.up, radius);
            Handles.DrawWireDisc(position, Vector3.forward, radius);

            if (Camera.current.orthographic)
            {
                Vector3 normal = position - Handles.inverseMatrix.MultiplyVector(Camera.current.transform.forward);
                float sqrMagnitude = normal.sqrMagnitude;
                float num0 = radius * radius;
                Handles.DrawWireDisc(position - num0 * normal / sqrMagnitude, normal, radius);
            }
            else
            {
                Vector3 normal = position - Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position);
                float sqrMagnitude = normal.sqrMagnitude;
                float num0 = radius * radius;
                float num1 = num0 * num0 / sqrMagnitude;
                float num2 = Mathf.Sqrt(num0 - num1);
                Handles.DrawWireDisc(position - num0 * normal / sqrMagnitude, normal, num2);
            }
        }

        private void DrawCapsule(Vector3 point1, Vector3 point2, float radius)
        {
            //TO DO
        }

        private void DrawBox(Vector3 position, Vector3 halfExtents, Quaternion orientation)
        {
            var loaclPoints = GetBoxLocalPoints(halfExtents, orientation);
            var points = loaclPoints
                .Select(t => t + position)
                .ToArray();

            // Top
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[1], points[2]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawLine(points[3], points[0]);

            // Bottom
            Handles.DrawLine(points[4], points[5]);
            Handles.DrawLine(points[5], points[6]);
            Handles.DrawLine(points[6], points[7]);
            Handles.DrawLine(points[7], points[4]);

            // Sides
            Handles.DrawLine(points[0], points[4]);
            Handles.DrawLine(points[1], points[5]);
            Handles.DrawLine(points[2], points[6]);
            Handles.DrawLine(points[3], points[7]);
        }

        private Vector3[] GetBoxLocalPoints(Vector3 halfExtents, Quaternion orientation)
        {
            var points = new Vector3[]
            {
                new Vector3(halfExtents.x, halfExtents.y, halfExtents.z),
                new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z),
                new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z),
                new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z),
                new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z),
                new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z),
                new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
                new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z),
            };

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = orientation * points[i];
            }

            return points;
        }

        private float GetFadeCoeff(PhysicsCastInfo physicsCastInfo)
        {
            const float fadeMaxDuration = 2f;
            float fadeDuration = Mathf.Min(gizmosSettings.gizmosLifeTime, fadeMaxDuration);
            float expirationTime = gizmosSettings.gizmosLifeTime - ((float)(DateTime.UtcNow - physicsCastInfo.castTime).TotalMilliseconds / 1000f);
            float fadeCoeff = Mathf.Min(expirationTime / fadeDuration, 1);
            return fadeCoeff;
        }

        #endregion
    }
}
