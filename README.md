# Hello Editor

> Unity Editor

## Custom Inspector

对 `Cube` 脚本自定义 `Inspector`，可随机改变方块颜色与重置。(`Scenes/CubeEditorSample`)

![Cube Editro](README/CubeEditor.png)

## 曲线编辑器

### 已实现的功能

- 创建曲线
- <kbd>Shift</kbd> + 左键 添加节点
- 右键 删除节点
- 移动节点，锚点跟随
- 调整锚点，对应锚点方向跟随
- 按住 <kbd>Ctrl</kbd> 自由调整单个锚点
- 曲线闭合，首尾相接
- 计算曲线的平均分割点
- `Mesh` 的创建（`RoadCreator`）

### 部分具体实现

#### 1. Path

曲线内部维护一个 `Vecotr2` 的 `List`（**`points`**），节点**按顺序**放置在 `points` 中。

![Path.cs](README/PathUML.png)

#### 2. 创建曲线

创建曲线的方法由 `PathCreator` 提供，`PathCreator` 可挂载在游戏物体上，创建曲线即将 `PathCreator` 内部包含的 `Path` 实例化，并传入游戏物体的坐标作为初始位置（`center`）。

创建的新曲线为一段，两个节点，共四个点。图中从左到右的顺序为 `0、1、2、3`，坐标分别为 `center` 的左侧一个单位的位置、左`0.5`上`0.5`单位的位置、右`0.5`下`0.5`单位的位置、右一个单位的位置。

```c#
public Path(Vector2 center) {
    points = new List<Vector2>() {
        center + Vector2.left,
        center + (Vector2.left + Vector2.up) * 0.5f,
        center + (Vector2.right + Vector2.down) * 0.5f,
        center + Vector2.right
    };
}
```

![New Curve](README/NewCurve.png)

#### 3. 添加节点

添加一个节点即添加一个曲线段需要添加 **3** 个点，图中（*除默认曲线的 4 个点外*）从左到右的顺序为 `4、5、6`。

这三个点的位置分别为 `points[2]` 相对于 `points[3]` 的对称点、`points[6]` 即添加节点位置与 `points[4]` 的中点、添加的节点位置。

```c#
public void AddSegment(Vector2 seg) {
    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
    points.Add((points[points.Count - 1] + seg) / 2);
    points.Add(seg);
}
```

![Add Segment](README/AddSegment.png)

#### 4. 删除节点

- 普通情况（删除中间某个节点）：只需删除节点左右两个点和节点本身，使用 `points.RemoveRange(index - 1, 3)` 即可。 
- 删除**首**节点：如果曲线未闭合则只需删除 `points` 的前三个点，即上图中的 `0、1、2`。如果曲线闭合则在删除这三个点之前将最后一个点的值改为 `points[2]` 即新的首节点的第一个锚点。
- 删除**尾**节点：如果未闭合则需删除 `points` 的最后三个点，即上图中的 `4、5、6` ，使新的尾节点只有一个锚点。如果曲线闭合，则不需删除新的尾节点的第二锚点，而需要删除最后一个点（首节点的一个锚点）。

```c#
public void DeleteSegment(int index) {
    if (NumSegments <= 2 && (isClosed || NumSegments <= 1))
        return;

    if (index == 0) {
        if (isClosed)
            points[points.Count - 1] = points[2];
        points.RemoveRange(0, 3);
    }
    else if (index == points.Count - 1 && !isClosed) {
        points.RemoveRange(index - 2, 3);
    }
    else {
        points.RemoveRange(index - 1, 3);
    }
}
```

#### 5. 移动节点

- 单纯移动点的部分只需将坐标赋值给对应点即可。
- 移动**节点**时还要将对应的两个**锚点**同时移动，使其保持相对位置。
- 移动**锚点**时如果没有按下 <kbd>Ctrl</kbd> 则要将所相对的锚点同时移动，保持方向相对，但对面锚点到节点的距离不变。

```c#
public void MovePoint(int i, Vector2 pos) {
    Vector2 offset = pos - points[i];
    points[i] = pos;

    switch (i % 3) {
        case 0:
            if (i - 1 >= 0 || isClosed)
                points[LoopIndex(i - 1)] += offset;
            if (i + 1 < NumPoints || isClosed)
                points[LoopIndex(i + 1)] += offset;
            break;
        case 1:
            if ((i - 2 >= 0 || isClosed) && !Event.current.control) {
                float dst = (points[LoopIndex(i - 1)] - points[LoopIndex(i - 2)]).magnitude;
                Vector2 dir = (points[LoopIndex(i - 1)] - pos).normalized;
                points[LoopIndex(i - 2)] = points[LoopIndex(i - 1)] + dir * dst;
            }
            break;
        case 2:
            if ((i + 2 < NumPoints || isClosed) && !Event.current.control) {
                float dst = (points[LoopIndex(i + 1)] - points[LoopIndex(i + 2)]).magnitude;
                Vector2 dir = (points[LoopIndex(i + 1)] - pos).normalized;
                points[LoopIndex(i + 2)] = points[LoopIndex(i + 1)] + dir * dst;
            }
            break;
        default:
            break;
    }
}
```

#### 6. 闭合曲线

闭合曲线通过一个按钮来控制闭合和开放，变量 `isClosed` 记录状态。闭合曲线需要在 `points` 最后增加两个点，即首尾节点的锚点，取消闭合则删除这两个点。

```c#
public void ToggleClose() {
    isClosed = !isClosed;

    if (isClosed) {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add(points[0] * 2 - points[1]);
    }
    else {
        points.RemoveRange(points.Count - 2, 2);
    }
}
```

#### 7. 曲线和锚点的绘制

曲线和锚点的绘制放在了一个 `Draw()` 函数中，`Draw()` 函数在 `PathEditor` 的 `OnSceneGUI()` 中调用。

- 锚点的绘制用到了 `Handles` 类中的 `FreeMoveHandle()` 方法来绘制了一个圆柱形手柄，手柄移动后的位置返回给 `pos` ，通过这个返回的 `pos` 来移动点。在移动之前用 `Undo` 类记录了当前状态，可以撤销这一步的移动。
- 曲线的绘制用到了 `Path` 类中的 `GetPointsInSegment(int)` 方法来获取某一段曲线的**四**个控制点，`0、1` 两个点连线，`2、3` 两个点连线，然后利用这四个点绘制一条从 `0` 到 `3` 的贝塞尔曲线。

```c#
private void Draw() {
    // 绘制锚点
    Handles.color = Color.red;
    for(int i = 0; i < path.NumPoints; i++) {
        Vector2 pos = Handles.FreeMoveHandle(path[i], Quaternion.identity, 0.1f, Vector3.zero, Handles.CylinderHandleCap);
        if (pos != path[i]) {
            Undo.RecordObject(creator, "移动锚点");
            path.MovePoint(i, pos);                        
        }
    }

    // 绘制曲线
    for(int i = 0; i < path.NumSegments; i++) {
        Vector2[] points = path.GetPointsInSegment(i);
        Handles.DrawLine(points[0], points[1]);
        Handles.DrawLine(points[2], points[3]);
        Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 4f);
    }
}
```

![New Curve](README/NewCurve.png)

#### 8. 鼠标点击控制

鼠标左键和右键分别有添加、删除节点的作用，`Editor` 中的鼠标输入需要用 `Event` 事件，如**左键点击**的判断： `guiEvent.type == EventType.MouseDown && guiEvent.button == 0`，而 <kbd>Shift</kbd> 的状态只需 `guiEvent.shift` 即可，其中 `guiEvent` 为 `Event.current`。

**鼠标位置**：`Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin`。

**右键删除节点**时需要遍历所有节点，比较鼠标位置与节点坐标距离，小于一定值才判断为点击的节点，再将其删除。

添加和删除节点都利用 `Undo` 类记录了状态，都可以 <kbd>Ctrl</kbd> + <kbd>Z</kbd> **撤销操作**。

#### 9. Inspector GUI

在`PathCreator` 的 `Inspector` 上添加了两个按钮：<kbd>创建新曲线</kbd> 和 <kbd>闭合/打开曲线</kbd> ，只需要将继承了 `Editor` 的 `PathEditor` 覆写 `OnInsepectorGUI()` 方法，在基类方法的基础上添加这两个按钮。点击按钮后通过 `SceneView` 类的 `RepaintAll()` 方法重新绘制。

```c#
public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    if (GUILayout.Button("创建新曲线")) {
        creator.CreatePath();
        path = creator.path;
        SceneView.RepaintAll();
    }

    if (GUILayout.Button("闭合/打开曲线")) {
        path.ToggleClose();
        SceneView.RepaintAll();
    }
}
```

#### 10. 用 Mesh 实现 RoadCreator

在实现 `Mesh` 之前要计算**曲线的平均分割点**，在 `Path` 中 `CalEvenSpacedPoints()` 实现，通过近似计算的方法逼近正确的平均分割点，这部分没怎么看明白，先拿过来用的。

获得的**平均分割点**就是曲线更细化的节点，创建 `Mesh` 就需要用这些节点来将 `Road` 分割成三角形。

先计算这些三角形的**顶点**，即下图中各个节点的左右两个点，同一节点的两个三角形顶点连线与红箭头方向垂直，而红箭头的方向由图可知：

- 中间节点的红箭头方向是**该节点到下一节点方向**与**上一节点到该节点方向**的向量和。
- 两端节点的红箭头方向则是其中仅有的一个方向的表示。

```c#
for (int i = 0; i < points.Length; i++) {
    Vector2 forward = Vector2.zero;

    if (i > 0)
        forward += points[i] - points[i - 1];
    if (i < points.Length - 1)
        forward += points[i + 1] - points[i];

    forward.Normalize();
}
```

得到了红箭头方向即可算得两侧顶点的坐标。

```c#
Vector2 left = new Vector2(-forward.y, forward.x);
verts.Add(points[i] + left * RoadWidth);
verts.Add(points[i] - left * RoadWidth);
```

![vertex](README/Verts.png)

`Mesh` 还需设置这些三角形，`mesh.triangles = tris`，即每个三角形对应的三个顶点坐标的集合。下图四个三角形中第一个三角形需从 `0-2-1` 的顺序，第二个三角形从 `1-2-3` 的顺序，顺序不对三角形会正反颠倒。

如果给这个 `Mesh` 加上贴图，则需要设置 `Mesh.uv` ，即对应贴图的 `UV` 坐标。原来的代码中没有第二行对 `percent` 的重新计算，实现的效果是一张贴图完整的贴满这条 `Road` ，但是闭合曲线后，首尾之间有一小段的 `UV` 坐标是完整的从`1`到`0`，导致图像压缩的很模糊。第二行代码使得整条 `Road` 的 `UV` 坐标变化变成了 `0-1-0` ，最后模糊的小段会变成`0`到`0`（变成最后一小段纯黑的路）。

```c#
float percent = i / (float)(points.Length - 1);
percent = 1 - Mathf.Abs(percent * 2 - 1);	// 1-|2x-1| 使 x 从原来的 0->1 变成了 0->1->0
uvs.Add(new Vector2(0, percent));
uvs.Add(new Vector2(1, percent));
```

![triangles](README/Tris.png)

计算得到 `Mesh` 的部分放在了 `RoadCreator` 类的 `CreateRoadMesh()` 中，与之对应的 `RoadEditor` 类在覆写的 `OnInspectorGUI()` 中调用 `CreateRoadMesh()` 刷新路径。

![Road](README/Road.png)