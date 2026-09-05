//// WorldState.cs
//using System.Collections.Generic;
//using UnityEngine;

//public class WorldState : MonoBehaviour
//{
//    public WorldStateData Data { get; private set; } = new WorldStateData();
//}

//[System.Serializable]
//public class WorldStateData
//{
//    private Dictionary<string, bool> states = new Dictionary<string, bool>();

//    public void Set(string key, bool value) => states[key] = value;

//    public bool Get(string key)
//    {
//        states.TryGetValue(key, out bool val);
//        return val;
//    }

//    public bool Contains(string key) => states.ContainsKey(key);

//    // Kiểm tra xem một tập điều kiện có thỏa mãn không
//    public bool Satisfies(Dictionary<string, bool> conditions)
//    {
//        foreach (var kv in conditions)
//        {
//            if (!states.TryGetValue(kv.Key, out bool val) || val != kv.Value)
//                return false;
//        }
//        return true;
//    }

//    // Tạo bản sao để planner mô phỏng
//    public WorldStateData Clone()
//    {
//        var clone = new WorldStateData();
//        foreach (var kv in states)
//            clone.Set(kv.Key, kv.Value);
//        return clone;
//    }
//}


//---------------- NEW ------------------


// ============================================================
//  WorldState.cs
//  Gồm 2 class:
//    - WorldStateData  : plain C#, chứa toàn bộ logic & data
//    - WorldState      : MonoBehaviour wrapper, gắn vào GameObject
// ============================================================

using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ─────────────────────────────────────────────────────────────
//  WorldStateData  —  plain C# class, KHÔNG kế thừa gì
//  Lý do: Planner gọi new WorldStateData() liên tục để clone.
//         Nếu là MonoBehaviour thì Unity sẽ throw warning.
// ─────────────────────────────────────────────────────────────
[System.Serializable]
public class WorldStateData
{
    // Dictionary chính: string key → bool value
    // Dùng capacity nhỏ để tránh resize sớm (thường < 16 key)
    private Dictionary<string, bool> _states = new Dictionary<string, bool>(16);

    // ── Đọc / Ghi ───────────────────────────────────────────

    /// <summary>
    /// Thêm hoặc cập nhật một điều kiện.
    /// Nếu key chưa tồn tại → tạo mới.
    /// Nếu đã tồn tại → ghi đè.
    /// </summary>
    public void Set(string key, bool value)
    {
        _states[key] = value;
    }

    /// <summary>
    /// Đọc giá trị của một điều kiện.
    /// Trả về false nếu key chưa tồn tại (quy ước "chưa xảy ra").
    /// </summary>
    public bool Get(string key)
    {
        _states.TryGetValue(key, out bool val);
        return val;
    }

    /// <summary>
    /// Xóa một điều kiện khỏi bảng.
    /// Dùng khi điều kiện không còn ý nghĩa (ví dụ: load scene mới).
    /// </summary>
    public void Remove(string key)
    {
        _states.Remove(key);
    }

    /// <summary>
    /// Xóa toàn bộ điều kiện.
    /// Dùng khi NPC respawn hoặc bắt đầu game mới.
    /// </summary>
    public void ClearAll()
    {
        _states.Clear();
    }

    /// <summary>
    /// Kiểm tra key có tồn tại trong bảng không.
    /// </summary>
    public bool Contains(string key)
    {
        return _states.ContainsKey(key);
    }

    // ── Planner ──────────────────────────────────────────────

    /// <summary>
    /// Kiểm tra tập điều kiện có được thỏa mãn không.
    /// Dùng bởi Planner để kiểm tra Preconditions và Goal.
    /// </summary>
    public bool Satisfies(Dictionary<string, bool> conditions)
    {
        foreach (var kv in conditions)
        {
            if (!_states.TryGetValue(kv.Key, out bool val) || val != kv.Value)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Copy toàn bộ state vào một Dictionary có sẵn.
    /// Dùng bởi Planner để khởi tạo shared state mà không tạo object mới.
    /// </summary>
    public void CopyTo(Dictionary<string, bool> target)
    {
        target.Clear();
        foreach (var kv in _states)
            target[kv.Key] = kv.Value;
    }

    /// <summary>
    /// Reset về một tập trạng thái mặc định.
    /// Dùng khi NPC respawn để khôi phục trạng thái ban đầu.
    /// </summary>
    public void ResetToDefaults(Dictionary<string, bool> defaults)
    {
        _states.Clear();
        foreach (var kv in defaults)
            _states[kv.Key] = kv.Value;
    }

    // ── Debug ────────────────────────────────────────────────

    /// <summary>
    /// In toàn bộ state ra Unity Console.
    /// Gọi ws.PrintAll() khi cần debug AI behavior.
    /// </summary>
    public void PrintAll(string ownerName = "")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== WorldState [{ownerName}] ===");
        foreach (var kv in _states)
        {
            string icon = kv.Value ? "✓" : "✗";
            sb.AppendLine($"  {icon}  {kv.Key} = {kv.Value}");
        }
        Debug.Log(sb.ToString());
    }
}


// ─────────────────────────────────────────────────────────────
//  WorldState  —  MonoBehaviour wrapper
//  Gắn vào NPC GameObject.
//  Không chứa logic — chỉ expose WorldStateData ra Unity.
// ─────────────────────────────────────────────────────────────
public class WorldState : MonoBehaviour
{
    /// <summary>
    /// Data thực sự. Planner và Action làm việc trực tiếp với cái này.
    /// Các script Unity dùng wrapper methods bên dưới cho tiện.
    /// </summary>
    public WorldStateData Data { get; private set; }

    private void Awake()
    {
        Data = new WorldStateData();
    }

    // ── Wrapper methods ──────────────────────────────────────
    // Cho phép gọi ws.Set(...) thay vì ws.Data.Set(...)

    public void Set(string key, bool value) => Data.Set(key, value);
    public bool Get(string key) => Data.Get(key);
    public void Remove(string key) => Data.Remove(key);
    public void ClearAll() => Data.ClearAll();
    public bool Contains(string key) => Data.Contains(key);
    public bool Satisfies(Dictionary<string, bool> conditions) => Data.Satisfies(conditions);
    public void ResetToDefaults(Dictionary<string, bool> def) => Data.ResetToDefaults(def);
    public void PrintAll() => Data.PrintAll(gameObject.name);
}
