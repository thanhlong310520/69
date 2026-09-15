#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Raccoon.Data;
using UnityEditor;
using UnityEngine;

namespace Raccoon.EditorTool
{
    // Paste a spreadsheet block (Name / Rarity / Level / GrowTime / Speed / Cost, tab-separated,
    // exactly like copy-pasting straight out of Google Sheets/Excel) and apply it in bulk to the
    // EggDataSO assets of a zone. Level and Speed columns are read but ignored (no matching field
    // on EggDataSO).
    public class EggDataBulkImporterWindow : EditorWindow
    {
        private const string DatasRoot = "Assets/1_MyAssets/Datas/Egg";

        private ZoneType _zone = ZoneType.Forest;
        private string _pastedText = "";
        private Vector2 _inputScroll;
        private Vector2 _previewScroll;
        private readonly List<ParsedRow> _rows = new List<ParsedRow>();
        private string _lastLog = "";

        private class ParsedRow
        {
            public string RawName;
            public string RarityText;
            public string GrowTimeText;
            public string CostText;

            public bool RarityOk;
            public RarityType Rarity;

            public bool CostOk;
            public long Cost;

            public int Hours, Minutes, Seconds;

            public EggDataSO Match;
            public List<string> Candidates = new List<string>();
            public string Error;
        }

        [MenuItem("Raccoon/Egg/Bulk Data Importer")]
        public static void Open()
        {
            GetWindow<EggDataBulkImporterWindow>("Egg Data Importer");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Egg Data Bulk Importer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Paste rows copied from a sheet. Columns (tab-separated): Name, Rarity, Level (ignored), GrowTime, Speed (ignored), Cost.\n" +
                "Also accepts a 4-column format: Name, Rarity, GrowTime, Cost.\n" +
                "Rarity must match an existing RarityType enum value (Common, Rare, Epic, Legendary, Mythic, Godly, Secret, Uncommon, Cosmic, Eternal...).",
                MessageType.Info);

            _zone = (ZoneType)EditorGUILayout.EnumPopup("Zone", _zone);

            EditorGUILayout.LabelField("Paste data here:");
            _inputScroll = EditorGUILayout.BeginScrollView(_inputScroll, GUILayout.Height(160));
            _pastedText = EditorGUILayout.TextArea(_pastedText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Parse", GUILayout.Height(28)))
            {
                ParseInput();
            }
            GUI.enabled = _rows.Count > 0;
            if (GUILayout.Button("Apply To Assets", GUILayout.Height(28)))
            {
                ApplyRows();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (_rows.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Preview ({_rows.Count} rows)", EditorStyles.boldLabel);
                _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, GUILayout.Height(260));
                foreach (var row in _rows)
                {
                    DrawRow(row);
                }
                EditorGUILayout.EndScrollView();
            }

            if (!string.IsNullOrEmpty(_lastLog))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(_lastLog, MessageType.None);
            }
        }

        private void DrawRow(ParsedRow row)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            string status;
            MessageType type;
            if (!string.IsNullOrEmpty(row.Error))
            {
                status = row.Error;
                type = MessageType.Error;
            }
            else if (row.Match == null)
            {
                status = row.Candidates.Count > 1
                    ? $"AMBIGUOUS match: {string.Join(", ", row.Candidates)}"
                    : "NOT FOUND: no matching EggDataSO in this zone";
                type = MessageType.Warning;
            }
            else if (!row.RarityOk)
            {
                status = $"Unknown rarity '{row.RarityText}' - add it to RarityType enum first. Other fields will still be applied.";
                type = MessageType.Warning;
            }
            else if (!row.CostOk)
            {
                status = $"Could not parse cost '{row.CostText}'.";
                type = MessageType.Warning;
            }
            else
            {
                status = $"-> {AssetDatabase.GetAssetPath(row.Match)}";
                type = MessageType.None;
            }

            EditorGUILayout.LabelField($"{row.RawName}   |  rarity: {row.RarityText}  |  grow: {row.GrowTimeText}  |  cost: {row.CostText}");
            EditorGUILayout.HelpBox(status, type);

            EditorGUILayout.EndVertical();
        }

        private void ParseInput()
        {
            _rows.Clear();
            _lastLog = "";

            var zoneDir = $"{DatasRoot}/{_zone}";
            var eggAssets = new List<EggDataSO>();
            if (Directory.Exists(zoneDir))
            {
                var guids = AssetDatabase.FindAssets("t:EggDataSO", new[] { zoneDir });
                foreach (var guid in guids)
                {
                    var asset = AssetDatabase.LoadAssetAtPath<EggDataSO>(AssetDatabase.GUIDToAssetPath(guid));
                    if (asset != null) eggAssets.Add(asset);
                }
            }

            var table = ParseDelimited(_pastedText);
            foreach (var fields in table)
            {
                if (fields.Length == 0 || fields.All(string.IsNullOrWhiteSpace)) continue;

                var row = new ParsedRow();

                string name, rarity, growTime, cost;
                if (fields.Length >= 6)
                {
                    name = fields[0]; rarity = fields[1]; growTime = fields[3]; cost = fields[5];
                }
                else if (fields.Length == 4)
                {
                    name = fields[0]; rarity = fields[1]; growTime = fields[2]; cost = fields[3];
                }
                else
                {
                    row.RawName = string.Join(" | ", fields);
                    row.Error = $"Unexpected column count ({fields.Length}), expected 4 or 6+.";
                    _rows.Add(row);
                    continue;
                }

                row.RawName = Clean(name);
                row.RarityText = Clean(rarity);
                row.GrowTimeText = Clean(growTime);
                row.CostText = Clean(cost);

                if (string.IsNullOrEmpty(row.RawName))
                {
                    row.Error = "Empty name, skipped.";
                    _rows.Add(row);
                    continue;
                }

                row.RarityOk = Enum.TryParse(row.RarityText, true, out RarityType rarityValue);
                row.Rarity = rarityValue;

                row.CostOk = TryParseCost(row.CostText, out long costValue);
                row.Cost = costValue;

                var (h, m, s) = ParseGrowTime(row.GrowTimeText);
                row.Hours = h; row.Minutes = m; row.Seconds = s;

                row.Match = FindMatch(eggAssets, row.RawName, row.Candidates);

                _rows.Add(row);
            }

            Repaint();
        }

        private void ApplyRows()
        {
            int applied = 0, skipped = 0;
            var issues = new List<string>();

            foreach (var row in _rows)
            {
                if (!string.IsNullOrEmpty(row.Error) || row.Match == null)
                {
                    skipped++;
                    if (row.Match == null && string.IsNullOrEmpty(row.Error))
                        issues.Add($"{row.RawName}: no matching asset");
                    continue;
                }

                var so = new SerializedObject(row.Match);
                so.FindProperty("itemName").stringValue = row.RawName;

                if (row.RarityOk)
                {
                    so.FindProperty("rarityType").intValue = (int)row.Rarity;
                }
                else
                {
                    issues.Add($"{row.RawName}: unknown rarity '{row.RarityText}' left unchanged");
                }

                if (row.CostOk)
                {
                    so.FindProperty("cost").longValue = row.Cost;
                }
                else
                {
                    issues.Add($"{row.RawName}: could not parse cost '{row.CostText}' left unchanged");
                }

                var growTimeProp = so.FindProperty("growTime");
                growTimeProp.FindPropertyRelative("hours").intValue = row.Hours;
                growTimeProp.FindPropertyRelative("minutes").intValue = row.Minutes;
                growTimeProp.FindPropertyRelative("seconds").intValue = row.Seconds;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(row.Match);
                applied++;
            }

            AssetDatabase.SaveAssets();

            var sb = new StringBuilder();
            sb.AppendLine($"Applied: {applied}. Skipped: {skipped}.");
            if (issues.Count > 0)
            {
                sb.AppendLine("Issues:");
                foreach (var i in issues) sb.AppendLine(" - " + i);
            }
            _lastLog = sb.ToString();
            Debug.Log($"[EggDataBulkImporter] {_lastLog}");
        }

        private static EggDataSO FindMatch(List<EggDataSO> eggAssets, string itemName, List<string> candidatesOut)
        {
            var itemKey = Normalize(itemName);

            foreach (var asset in eggAssets)
            {
                if (Normalize(StripEggPrefix(asset.name)) == itemKey)
                    return asset;
            }

            var candidates = eggAssets
                .Where(a =>
                {
                    var fileKey = Normalize(StripEggPrefix(a.name));
                    return itemKey.Contains(fileKey) || fileKey.Contains(itemKey);
                })
                .ToList();

            if (candidates.Count == 1) return candidates[0];

            foreach (var c in candidates) candidatesOut.Add(c.name);
            return null;
        }

        private static string StripEggPrefix(string assetName)
        {
            return assetName.StartsWith("Egg") ? assetName.Substring(3) : assetName;
        }

        private static string Normalize(string s)
        {
            return new string((s ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }

        private static string Clean(string s)
        {
            if (s == null) return "";
            return s.Replace("\r", "").Trim().Trim('\n').Trim();
        }

        private static (int h, int m, int s) ParseGrowTime(string text)
        {
            int h = 0, m = 0, s = 0;
            if (string.IsNullOrEmpty(text)) return (0, 0, 0);

            var mh = Regex.Match(text, @"(\d+)\s*h");
            var mm = Regex.Match(text, @"(\d+)\s*m(?!s)");
            var ms = Regex.Match(text, @"(\d+)\s*s");

            if (mh.Success) h = int.Parse(mh.Groups[1].Value);
            if (mm.Success) m = int.Parse(mm.Groups[1].Value);
            if (ms.Success) s = int.Parse(ms.Groups[1].Value);

            return (h, m, s);
        }

        private static bool TryParseCost(string raw, out long cost)
        {
            cost = 0;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            var match = Regex.Match(raw.Trim(), @"^([\d.,]+)\s*([kKmMbB]?)");
            if (!match.Success) return false;

            var numPart = match.Groups[1].Value;
            var suffix = match.Groups[2].Value.ToLowerInvariant();

            double multiplier = suffix switch
            {
                "k" => 1e3,
                "m" => 1e6,
                "b" => 1e9,
                _ => 1
            };

            string normalized = multiplier != 1
                ? numPart.Replace(",", ".")
                : numPart.Replace(",", "").Replace(".", "");

            if (!double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                return false;

            cost = (long)Math.Round(value * multiplier);
            return true;
        }

        // Minimal TSV parser: tab-separated fields, double-quoted fields may contain
        // embedded newlines/commas/tabs (as produced by copy-pasting from Sheets/Excel).
        private static List<string[]> ParseDelimited(string text)
        {
            var rows = new List<string[]>();
            var fields = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            int i = 0;

            while (i < text.Length)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { sb.Append('"'); i += 2; continue; }
                        inQuotes = false; i++; continue;
                    }
                    sb.Append(c); i++; continue;
                }

                if (c == '"') { inQuotes = true; i++; continue; }
                if (c == '\t') { fields.Add(sb.ToString()); sb.Clear(); i++; continue; }
                if (c == '\r') { i++; continue; }
                if (c == '\n')
                {
                    fields.Add(sb.ToString()); sb.Clear();
                    rows.Add(fields.ToArray()); fields.Clear();
                    i++; continue;
                }
                sb.Append(c); i++;
            }

            if (sb.Length > 0 || fields.Count > 0)
            {
                fields.Add(sb.ToString());
                rows.Add(fields.ToArray());
            }

            return rows;
        }
    }
}
#endif
