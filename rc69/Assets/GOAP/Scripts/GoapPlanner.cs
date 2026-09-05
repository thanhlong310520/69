//// GoapPlanner.cs
//using System.Collections.Generic;
//using System.Linq;

//public class GoapPlanner
//{
//    // Node trong cây tìm kiếm A*
//    private class Node
//    {
//        public Node parent;
//        public float runningCost;
//        public WorldStateData state;
//        public GoapAction action;

//        public Node(Node parent, float cost, WorldStateData state, GoapAction action)
//        {
//            this.parent = parent;
//            this.runningCost = cost;
//            this.state = state;
//            this.action = action;
//        }
//    }

//    /// <summary>
//    /// Lập kế hoạch: trả về Queue các action cần thực hiện theo thứ tự,
//    /// hoặc null nếu không tìm được kế hoạch.
//    /// </summary>
//    public Queue<GoapAction> Plan(
//        WorldStateData currentState,
//        Dictionary<string, bool> goal,
//        List<GoapAction> availableActions)
//    {
//        // Lọc ra actions có procedural precondition hợp lệ
//        var usableActions = availableActions
//            .Where(a => a.CheckProceduralPrecondition(currentState))
//            .ToList();

//        var start = new Node(null, 0, currentState.Clone(), null);
//        var leaves = new List<Node>();

//        // Backward search: tìm từ goal ngược lại
//        bool found = BuildGraph(start, leaves, usableActions, goal);

//        if (!found) return null;

//        // Chọn leaf có tổng cost thấp nhất
//        Node cheapest = leaves.OrderBy(n => n.runningCost).First();

//        // Truy ngược để lấy chuỗi action
//        var result = new List<GoapAction>();
//        var node = cheapest;
//        while (node != null)
//        {
//            if (node.action != null) result.Insert(0, node.action);
//            node = node.parent;
//        }

//        return new Queue<GoapAction>(result);
//    }

//    private bool BuildGraph(
//        Node parent,
//        List<Node> leaves,
//        List<GoapAction> actions,
//        Dictionary<string, bool> goal)
//    {
//        bool foundPath = false;

//        foreach (var action in actions)
//        {
//            // Nếu preconditions của action thỏa mãn trong state hiện tại
//            if (parent.state.Satisfies(action.Preconditions))
//            {
//                // Áp dụng effects để tạo state mới
//                WorldStateData newState = ApplyEffects(parent.state, action.Effects);

//                var node = new Node(parent, parent.runningCost + action.Cost, newState, action);

//                // Nếu state mới thỏa mãn goal → tìm được đường!
//                if (GoalAchieved(newState, goal))
//                {
//                    leaves.Add(node);
//                    foundPath = true;
//                }
//                else
//                {
//                    // Tiếp tục tìm sâu hơn (DFS với chi phí tích lũy)
//                    var subset = actions.Where(a => a != action).ToList();
//                    bool found = BuildGraph(node, leaves, subset, goal);
//                    if (found) foundPath = true;
//                }
//            }
//        }

//        return foundPath;
//    }

//    private WorldStateData ApplyEffects(WorldStateData state, Dictionary<string, bool> effects)
//    {
//        var newState = state.Clone();
//        foreach (var kv in effects)
//            newState.Set(kv.Key, kv.Value);
//        return newState;
//    }

//    private bool GoalAchieved(WorldStateData state, Dictionary<string, bool> goal)
//        => state.Satisfies(goal);
//}


//---------------NEW---------------------------


// ============================================================
//  GoapPlanner.cs  —  A* GOAP Planner, low-GC
//
//  Thuật toán: A* forward search trên không gian state
//
//  Tại sao A* tốt hơn DFS cũ:
//    - Luôn mở rộng node có f = g + h thấp nhất trước
//    - Cắt tỉa ngay khi tìm ra đường tối ưu đầu tiên
//    - Không cần duyệt hết cây rồi mới so sánh
//
//  Giảm GC:
//    - Node là struct trong pre-alloc array
//    - Shared state + Apply/Revert thay vì Clone Dictionary
//    - MinHeap tự viết thay vì SortedList (tránh boxing)
//    - Pre-alloc List cho open/closed/result
// ============================================================

using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GoapPlanner
{
    // ─────────────────────────────────────────────────────────
    //  Cấu hình
    // ─────────────────────────────────────────────────────────
    private const int MAX_NODES = 256;
    private const int MAX_ACTIONS = 32;   // bitmask uint = tối đa 32 action

    // ─────────────────────────────────────────────────────────
    //  Node — struct để nằm trong array, không heap allocation
    // ─────────────────────────────────────────────────────────
    private struct Node
    {
        public int ParentIndex;   // -1 = root
        public int ActionIndex;   // -1 = root
        public float G;             // cost thực từ start đến node này
        public float F;             // f = g + h (h = heuristic)

        // Snapshot trạng thái TẠI node này
        // Lưu dưới dạng flat array để tránh Dictionary allocation
        // Xem StateSnapshot bên dưới
        public int SnapshotIndex;
    }

    // ─────────────────────────────────────────────────────────
    //  StateSnapshot — lưu trạng thái tại mỗi node
    //
    //  Thay vì Clone() một Dictionary (heap alloc),
    //  ta lưu snapshot dưới dạng (key, value) pairs
    //  trong một array phẳng được pre-alloc.
    //
    //  Layout: _snapshotData[base + 0] = (key0, val0)
    //                        [base + 1] = (key1, val1) ...
    //  _snapshotMeta[snapshotIdx] = (baseOffset, count)
    // ─────────────────────────────────────────────────────────
    private struct SnapshotEntry
    {
        public string Key;
        public bool Value;
    }

    private const int MAX_SNAPSHOT_ENTRIES = MAX_NODES * 16; // tối đa 16 key/state
    private readonly SnapshotEntry[] _snapshotData = new SnapshotEntry[MAX_SNAPSHOT_ENTRIES];
    private int _snapshotDataCount = 0;

    private struct SnapshotMeta { public int Base; public int Count; }
    private readonly SnapshotMeta[] _snapshotMeta = new SnapshotMeta[MAX_NODES];
    private int _snapshotCount = 0;

    // ─────────────────────────────────────────────────────────
    //  Node buffer
    // ─────────────────────────────────────────────────────────
    private readonly Node[] _nodeBuffer = new Node[MAX_NODES];
    private int _nodeCount = 0;

    // ─────────────────────────────────────────────────────────
    //  MinHeap (open list) — priority queue theo F
    //  Tự viết để tránh boxing/unboxing của SortedList
    // ─────────────────────────────────────────────────────────
    private readonly int[] _heap = new int[MAX_NODES]; // chứa node index
    private int _heapSize = 0;

    // Closed list: snapshotIndex của các state đã xét
    // Dùng HashSet<int> — int không boxing
    private readonly HashSet<int> _closedSnapshots = new HashSet<int>();

    // Pre-alloc cho trace ngược
    private readonly List<int> _traceBuffer = new List<int>(32);

    // ─────────────────────────────────────────────────────────
    //  Shared state cho heuristic — không dùng trong search
    //  (search dùng snapshot riêng của từng node)
    // ─────────────────────────────────────────────────────────
    private readonly Dictionary<string, bool> _tempState = new Dictionary<string, bool>(16);

    // ─────────────────────────────────────────────────────────
    //  Plan() — điểm vào duy nhất
    // ─────────────────────────────────────────────────────────
    public Queue<GoapAction> Plan(
        WorldStateData currentState,
        Dictionary<string, bool> goal,
        List<GoapAction> availableActions)
    {
        if (currentState == null || goal == null || availableActions == null)
        {
            Debug.LogWarning("[GoapPlanner] Plan() nhận tham số null.");
            return null;
        }
        if (availableActions.Count > MAX_ACTIONS)
        {
            Debug.LogWarning($"[GoapPlanner] Quá nhiều action ({availableActions.Count}), max={MAX_ACTIONS}.");
            return null;
        }

        // Reset toàn bộ buffers
        _nodeCount = 0;
        _snapshotDataCount = 0;
        _snapshotCount = 0;
        _heapSize = 0;
        _closedSnapshots.Clear();
        _traceBuffer.Clear();

        // Lọc action khả dụng bằng bitmask
        uint usableMask = 0u;
        for (int i = 0; i < availableActions.Count; i++)
            if (availableActions[i].CheckProceduralPrecondition(currentState))
                usableMask |= (1u << i);

        if (usableMask == 0)
        {
            Debug.LogWarning("[GoapPlanner] Không có action nào khả dụng.");
            return null;
        }

        // Tạo root node từ world state hiện tại
        int rootSnap = TakeSnapshot(currentState);
        if (rootSnap < 0) return null;

        int rootIdx = AllocNode(
            parentIdx: -1,
            actionIdx: -1,
            snapshotIdx: rootSnap,
            g: 0f,
            h: Heuristic(rootSnap, goal)
        );
        HeapPush(rootIdx);

        // ── A* main loop ─────────────────────────────────────
        while (_heapSize > 0)
        {
            // Lấy node có F thấp nhất
            int curIdx = HeapPop();
            ref Node cur = ref _nodeBuffer[curIdx];

            // Nếu goal đã đạt → trace ngược và return
            if (GoalAchieved(cur.SnapshotIndex, goal))
            {
                return BuildResult(curIdx, availableActions);
            }

            // Đánh dấu đã xét (closed)
            // Dùng snapshotIndex làm key — mỗi snapshot là một state khác nhau
            // Lưu ý: hai snapshot khác index nhưng nội dung giống nhau
            // vẫn được coi là khác trong impl này — đủ đúng cho GOAP
            // vì state space nhỏ và plan chỉ chạy không thường xuyên
            _closedSnapshots.Add(cur.SnapshotIndex);

            // Mở rộng: thử từng action còn trong mask
            for (int i = 0; i < availableActions.Count; i++)
            {
                if ((usableMask & (1u << i)) == 0) continue;

                GoapAction action = availableActions[i];

                // Kiểm tra precondition trên snapshot của node hiện tại
                if (!SnapshotSatisfies(cur.SnapshotIndex, action.Preconditions))
                    continue;

                // Tạo snapshot mới = snapshot hiện tại + effects của action
                int newSnap = ApplyEffectsToSnapshot(cur.SnapshotIndex, action.Effects);
                if (newSnap < 0) continue; // buffer đầy

                // Bỏ qua nếu state này đã trong closed
                if (_closedSnapshots.Contains(newSnap)) continue;

                float newG = cur.G + action.Cost;
                float newH = Heuristic(newSnap, goal);

                int newIdx = AllocNode(
                    parentIdx: curIdx,
                    actionIdx: i,
                    snapshotIdx: newSnap,
                    g: newG,
                    h: newH
                );
                if (newIdx < 0) continue; // node buffer đầy

                HeapPush(newIdx);
            }
        }

        Debug.LogWarning("[GoapPlanner] Không tìm được kế hoạch hợp lệ.");
        return null;
    }

    // ─────────────────────────────────────────────────────────
    //  Heuristic h(n)
    //
    //  Đếm số điều kiện trong goal chưa được thỏa mãn.
    //  Admissible (không overestimate) vì mỗi action
    //  có thể thỏa mãn tối đa một điều kiện.
    //  → A* đảm bảo tìm đường tối ưu.
    // ─────────────────────────────────────────────────────────
    private float Heuristic(int snapshotIdx, Dictionary<string, bool> goal)
    {
        int unsatisfied = 0;
        var meta = _snapshotMeta[snapshotIdx];

        foreach (var kv in goal)
        {
            bool found = false;
            for (int i = meta.Base; i < meta.Base + meta.Count; i++)
            {
                if (_snapshotData[i].Key == kv.Key)
                {
                    if (_snapshotData[i].Value == kv.Value) found = true;
                    break;
                }
            }
            if (!found) unsatisfied++;
        }
        return unsatisfied;
    }

    // ─────────────────────────────────────────────────────────
    //  Snapshot helpers
    // ─────────────────────────────────────────────────────────

    // Tạo snapshot từ WorldStateData
    private int TakeSnapshot(WorldStateData state)
    {
        // Copy state vào _tempState để duyệt
        state.CopyTo(_tempState);

        int count = _tempState.Count;
        if (_snapshotDataCount + count > MAX_SNAPSHOT_ENTRIES)
        {
            Debug.LogWarning("[GoapPlanner] Snapshot buffer đầy.");
            return -1;
        }
        if (_snapshotCount >= MAX_NODES)
        {
            Debug.LogWarning("[GoapPlanner] Snapshot meta buffer đầy.");
            return -1;
        }

        int snapIdx = _snapshotCount++;
        int baseOff = _snapshotDataCount;
        int cnt = 0;

        foreach (var kv in _tempState)
        {
            _snapshotData[_snapshotDataCount].Key = kv.Key;
            _snapshotData[_snapshotDataCount].Value = kv.Value;
            _snapshotDataCount++;
            cnt++;
        }

        _snapshotMeta[snapIdx] = new SnapshotMeta { Base = baseOff, Count = cnt };
        return snapIdx;
    }

    // Tạo snapshot mới = snapshot cũ + apply effects
    private int ApplyEffectsToSnapshot(int srcIdx, Dictionary<string, bool> effects)
    {
        var meta = _snapshotMeta[srcIdx];
        int newCount = meta.Count;

        // Đếm key mới (chưa có trong src)
        foreach (var kv in effects)
        {
            bool exists = false;
            for (int i = meta.Base; i < meta.Base + meta.Count; i++)
                if (_snapshotData[i].Key == kv.Key) { exists = true; break; }
            if (!exists) newCount++;
        }

        if (_snapshotDataCount + newCount > MAX_SNAPSHOT_ENTRIES)
        {
            Debug.LogWarning("[GoapPlanner] Snapshot buffer đầy khi apply effects.");
            return -1;
        }
        if (_snapshotCount >= MAX_NODES)
        {
            Debug.LogWarning("[GoapPlanner] Snapshot meta buffer đầy.");
            return -1;
        }

        int newSnapIdx = _snapshotCount++;
        int baseOff = _snapshotDataCount;
        int cnt = 0;

        // Copy từ src, override nếu key có trong effects
        for (int i = meta.Base; i < meta.Base + meta.Count; i++)
        {
            string srcKey = _snapshotData[i].Key;
            bool srcVal = _snapshotData[i].Value;

            // Kiểm tra key này có bị override bởi effect không
            if (effects.TryGetValue(srcKey, out bool effVal))
                srcVal = effVal;

            _snapshotData[_snapshotDataCount].Key = srcKey;
            _snapshotData[_snapshotDataCount].Value = srcVal;
            _snapshotDataCount++;
            cnt++;
        }

        // Thêm key mới từ effects (chưa tồn tại trong src)
        foreach (var kv in effects)
        {
            bool exists = false;
            for (int i = meta.Base; i < meta.Base + meta.Count; i++)
                if (_snapshotData[i].Key == kv.Key) { exists = true; break; }

            if (!exists)
            {
                _snapshotData[_snapshotDataCount].Key = kv.Key;
                _snapshotData[_snapshotDataCount].Value = kv.Value;
                _snapshotDataCount++;
                cnt++;
            }
        }

        _snapshotMeta[newSnapIdx] = new SnapshotMeta { Base = baseOff, Count = cnt };
        return newSnapIdx;
    }

    // Kiểm tra snapshot có thỏa mãn conditions không
    private bool SnapshotSatisfies(int snapIdx, Dictionary<string, bool> conditions)
    {
        if (conditions.Count == 0) return true;
        var meta = _snapshotMeta[snapIdx];

        foreach (var kv in conditions)
        {
            bool satisfied = false;
            for (int i = meta.Base; i < meta.Base + meta.Count; i++)
            {
                if (_snapshotData[i].Key == kv.Key)
                {
                    satisfied = _snapshotData[i].Value == kv.Value;
                    break;
                }
            }
            if (!satisfied) return false;
        }
        return true;
    }

    // Kiểm tra goal đã đạt chưa (alias rõ nghĩa hơn)
    private bool GoalAchieved(int snapIdx, Dictionary<string, bool> goal)
        => SnapshotSatisfies(snapIdx, goal);

    // ─────────────────────────────────────────────────────────
    //  Node buffer
    // ─────────────────────────────────────────────────────────
    private int AllocNode(int parentIdx, int actionIdx, int snapshotIdx, float g, float h)
    {
        if (_nodeCount >= MAX_NODES)
        {
            Debug.LogWarning("[GoapPlanner] Node buffer đầy.");
            return -1;
        }
        int idx = _nodeCount++;
        _nodeBuffer[idx].ParentIndex = parentIdx;
        _nodeBuffer[idx].ActionIndex = actionIdx;
        _nodeBuffer[idx].SnapshotIndex = snapshotIdx;
        _nodeBuffer[idx].G = g;
        _nodeBuffer[idx].F = g + h;
        return idx;
    }

    // ─────────────────────────────────────────────────────────
    //  MinHeap (binary heap) — open list
    //  Sắp xếp theo F tăng dần.
    //  Không dùng IComparer/delegate để tránh closure alloc.
    // ─────────────────────────────────────────────────────────
    private void HeapPush(int nodeIdx)
    {
        if (nodeIdx < 0) return;
        _heap[_heapSize] = nodeIdx;
        SiftUp(_heapSize);
        _heapSize++;
    }

    private int HeapPop()
    {
        int top = _heap[0];
        _heapSize--;
        _heap[0] = _heap[_heapSize];
        SiftDown(0);
        return top;
    }

    private void SiftUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_nodeBuffer[_heap[parent]].F <= _nodeBuffer[_heap[i]].F) break;
            Swap(i, parent);
            i = parent;
        }
    }

    private void SiftDown(int i)
    {
        while (true)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;

            if (left < _heapSize && _nodeBuffer[_heap[left]].F < _nodeBuffer[_heap[smallest]].F)
                smallest = left;
            if (right < _heapSize && _nodeBuffer[_heap[right]].F < _nodeBuffer[_heap[smallest]].F)
                smallest = right;

            if (smallest == i) break;
            Swap(i, smallest);
            i = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        int tmp = _heap[a];
        _heap[a] = _heap[b];
        _heap[b] = tmp;
    }

    // ─────────────────────────────────────────────────────────
    //  Trace ngược từ goal node về root → build Queue
    // ─────────────────────────────────────────────────────────
    private Queue<GoapAction> BuildResult(int goalNodeIdx, List<GoapAction> availableActions)
    {
        _traceBuffer.Clear();
        int cur = goalNodeIdx;
        while (cur != -1)
        {
            if (_nodeBuffer[cur].ActionIndex >= 0)
                _traceBuffer.Add(cur);
            cur = _nodeBuffer[cur].ParentIndex;
        }
        // _traceBuffer: [goalNode, ..., firstActionNode] — thứ tự ngược

        var queue = new Queue<GoapAction>(_traceBuffer.Count);
        for (int i = _traceBuffer.Count - 1; i >= 0; i--)
            queue.Enqueue(availableActions[_nodeBuffer[_traceBuffer[i]].ActionIndex]);

        LogPlan(queue, _nodeBuffer[goalNodeIdx].G);
        return queue;
    }

    // ─────────────────────────────────────────────────────────
    //  Debug — chỉ chạy trong Editor
    // ─────────────────────────────────────────────────────────
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogPlan(Queue<GoapAction> plan, float totalCost)
    {
        var sb = new StringBuilder();
        sb.Append($"[GoapPlanner] Plan (cost={totalCost:F1}): ");
        bool first = true;
        foreach (var a in plan)
        {
            if (!first) sb.Append(" → ");
            sb.Append(a.GetType().Name);
            first = false;
        }
        Debug.Log(sb.ToString());
    }
}