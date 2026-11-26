#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

// 暗号道具系统的开发者调试 + 装配自检工具。
// 菜单：Tools / 暗号道具
//
// 不依赖 MCP，纯 Editor 工具。也提供运行时调试命令（必须先 Play）。
public static class 暗号道具调试工具
{
    const string MENU_ROOT = "Tools/暗号道具/";

    // ============================================================
    // 装配自检（Editor 模式，扫描场景 + .asset 字段是否齐全）
    // ============================================================
    [MenuItem(MENU_ROOT + "1. 检查装配状态（打印到 Console）", priority = 100)]
    public static void 检查装配()
    {
        var sb = new StringBuilder();
        sb.AppendLine("===== 暗号道具系统装配自检 =====");

        // ---- 4 个物品 SO 字段检查 ----
        sb.AppendLine("\n[一] 4 个甲骨字物品 SO 检查（搜全工程 t:物品）：");
        var 道具GUIDs = AssetDatabase.FindAssets("t:物品 甲骨文-");
        if (道具GUIDs.Length == 0) sb.AppendLine("  ⚠ 没搜到任何 甲骨文-X 物品 SO");
        foreach (var g in 道具GUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var item = AssetDatabase.LoadAssetAtPath<物品>(path);
            if (item == null) continue;
            sb.Append("  - ").Append(item.name).Append(" @ ").Append(path).AppendLine();
            sb.Append("      itemImage=").Append(item.itemImage != null ? item.itemImage.name : "<空>");
            sb.Append("  图_甲骨=").Append(item.图_甲骨 != null ? "✓" : "✗");
            sb.Append("  图_秦篆=").Append(item.图_秦篆 != null ? "✓" : "✗");
            sb.Append("  图_唐楷=").Append(item.图_唐楷 != null ? "✓" : "✗").AppendLine();
            sb.Append("      info_甲骨=").Append(string.IsNullOrEmpty(item.info_甲骨) ? "✗" : "✓");
            sb.Append("  info_秦篆=").Append(string.IsNullOrEmpty(item.info_秦篆) ? "✗" : "✓");
            sb.Append("  info_唐楷=").Append(string.IsNullOrEmpty(item.info_唐楷) ? "✗" : "✓").AppendLine();
        }

        // ---- 王宫场景检查 ----
        sb.AppendLine("\n[二] 王宫场景（剧情系统 / 商王）：");
        检查场景(sb, "Assets/Scenes/商朝/王宫.scene", (scene) =>
        {
            var ang = FindByName(scene, "剧情系统");
            if (ang == null) { sb.AppendLine("  ✗ 没找到 GameObject「剧情系统」"); return; }
            var sys = ang.GetComponent<暗号道具系统>();
            if (sys == null) { sb.AppendLine("  ✗ 剧情系统 没挂 暗号道具系统 组件"); }
            else
            {
                sb.Append("  ✓ 暗号道具系统 在；暗号道具集长度=")
                  .Append(sys.暗号道具集 != null ? sys.暗号道具集.Length : 0)
                  .Append("；传送场景=").AppendLine(sys.赋予后传送场景);
                if (sys.暗号道具集 != null)
                    for (int i = 0; i < sys.暗号道具集.Length; i++)
                        sb.Append("      [").Append(i).Append("] ")
                          .AppendLine(sys.暗号道具集[i] != null ? sys.暗号道具集[i].name : "<空>");
            }
            var boot = ang.GetComponent<子场景Bootstrap>();
            sb.AppendLine(boot != null ? "  ✓ 子场景Bootstrap 在" : "  ⚠ 子场景Bootstrap 不在（直接 Play 王宫会缺单例）");

            var sw = FindByName(scene, "商王");
            if (sw == null) { sb.AppendLine("  ✗ 没找到 GameObject「商王」"); return; }
            var npc = sw.GetComponent<NPC对话>();
            if (npc == null) sb.AppendLine("  ✗ 商王 没挂 NPC对话 组件");
            else sb.Append("  ✓ NPC对话 在；startDialogue=")
                   .AppendLine(npc.startDialogue != null ? npc.startDialogue.name : "<空>");
        });

        // ---- 次元空间场景检查 ----
        sb.AppendLine("\n[三] 次元空间场景（次元空间Hub / 4 道门 / 演化UI）：");
        检查场景(sb, "Assets/Scenes/次元空间.scene", (scene) =>
        {
            var hub = FindComponent<次元空间Hub>(scene);
            if (hub == null) { sb.AppendLine("  ✗ 没找到 次元空间Hub 组件"); }
            else
            {
                sb.AppendLine(hub.演化UI != null ? "  ✓ 演化UI 已接" : "  ⚠ 演化UI 未接（无视觉演化）");
                sb.AppendLine(hub.回家门 != null ? "  ✓ 回家门 已接" : "  ⚠ 回家门 未接（唐朝通关后回家门不会显隐）");
                sb.AppendLine(hub.灵蝉对白_商朝后 != null ? "  ✓ 灵蝉对白_商朝后 已接" : "  ⚠ 灵蝉对白_商朝后 未接（SO 留空就不播）");
                sb.AppendLine(hub.灵蝉对白_秦朝后 != null ? "  ✓ 灵蝉对白_秦朝后 已接" : "  ⚠ 灵蝉对白_秦朝后 未接");
                sb.AppendLine(hub.灵蝉对白_唐朝后 != null ? "  ✓ 灵蝉对白_唐朝后 已接" : "  ⚠ 灵蝉对白_唐朝后 未接");
            }
            var doors = FindAllComponents<传送门>(scene);
            sb.Append("  传送门数量=").AppendLine(doors.Count.ToString());
            foreach (var d in doors)
                sb.Append("    gateIndex=").Append(d.gateIndex)
                  .Append("  nextScene=").Append(string.IsNullOrEmpty(d.nextScene) ? "<空>" : d.nextScene)
                  .Append("  go=").AppendLine(d.gameObject.name);
        });

        // ---- 太学藏书阁场景检查 ----
        sb.AppendLine("\n[四] 太学藏书阁场景（秦朝通关）：");
        检查场景(sb, "Assets/Scenes/秦朝/太学藏书阁.scene", (scene) =>
        {
            var q = FindComponent<秦朝通关>(scene);
            if (q == null) sb.AppendLine("  ✗ 没找到 秦朝通关 组件（救完竹简后不会自动跳次元空间）");
            else
                sb.Append("  ✓ 秦朝通关 在；通关救数目标=").Append(q.通关救数目标)
                  .Append("  下一场景=").AppendLine(q.下一场景);
        });

        // ---- 结局场景检查 ----
        sb.AppendLine("\n[五] 结局场景（暗号拼字）：");
        检查场景(sb, "Assets/Scenes/结局.scene", (scene) =>
        {
            var ui = FindComponent<暗号拼字>(scene);
            if (ui == null) sb.AppendLine("  ✗ 没找到 暗号拼字 组件");
            else
            {
                sb.Append("  ✓ 暗号拼字 在；字按钮容器=").Append(ui.字按钮容器 != null ? "✓" : "✗")
                  .Append("  空槽容器=").Append(ui.空槽容器 != null ? "✓" : "✗")
                  .Append("  读出按钮=").Append(ui.读出按钮 != null ? "✓" : "✗")
                  .Append("  小明读字面板=").Append(ui.小明读字面板 != null ? "✓" : "✗")
                  .Append("  回家场景=").AppendLine(ui.回家场景);
            }
        });

        sb.AppendLine("\n===== 自检结束 =====");
        Debug.Log(sb.ToString());
    }

    // ============================================================
    // 一键装配修复（Editor 模式，扫描 + 修补所有场景）
    // ============================================================
    [MenuItem(MENU_ROOT + "2. 一键装配修复（自动接线所有场景）", priority = 150)]
    public static void 一键装配()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("请先退出 Play 模式再装配"); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        { Debug.Log("用户取消"); return; }

        var sb = new StringBuilder();
        sb.AppendLine("===== 一键装配修复 =====");

        // 找 4 个甲骨字物品 SO（不论搬到哪个目录）
        var 道具集 = 加载4个道具(sb);

        // ----- 把 秦小篆 / 唐楷 PNG 接到 4 个物品 SO 的 图_秦篆 / 图_唐楷 字段 -----
        sb.AppendLine("\n[字形 sprite 装配] ");
        接字形Sprite(道具集, "秦小篆碎片", "图_秦篆", sb);
        接字形Sprite(道具集, "唐楷碎片", "图_唐楷", sb);

        // ----- 创建 3 个空灵蝉对白 SO（如果不存在），等用户填内容 -----
        sb.AppendLine("\n[灵蝉对白 SO] ");
        var 灵蝉 = AssetDatabase.LoadAssetAtPath<NPCSO>("Assets/SO/商朝/灵蝉.asset");
        if (灵蝉 == null) sb.AppendLine("  ⚠ 找不到 Assets/SO/商朝/灵蝉.asset");
        EnsureFolder("Assets/SO/次元空间");
        var 对白_商 = 确保对白SO("Assets/SO/次元空间/灵蝉对白_商朝后.asset", "（请填写：商朝通关后灵蝉对玩家说的话）", 灵蝉, sb);
        var 对白_秦 = 确保对白SO("Assets/SO/次元空间/灵蝉对白_秦朝后.asset", "（请填写：秦朝通关后灵蝉对玩家说的话）", 灵蝉, sb);
        var 对白_唐 = 确保对白SO("Assets/SO/次元空间/灵蝉对白_唐朝后.asset", "（请填写：唐朝通关后灵蝉对玩家说的话）", 灵蝉, sb);

        // ----- 王宫场景 -----
        sb.AppendLine("\n[王宫] ");
        修场景("Assets/Scenes/商朝/王宫.scene", sb, (scene) =>
        {
            var ang = GetOrCreate(scene, "剧情系统");
            var sys = ang.GetComponent<暗号道具系统>() ?? ang.AddComponent<暗号道具系统>();
            var boot = ang.GetComponent<子场景Bootstrap>() ?? ang.AddComponent<子场景Bootstrap>();

            // 接 4 道具引用
            if (道具集 != null && 道具集.Length == 4)
            {
                var so = new SerializedObject(sys);
                var arr = so.FindProperty("暗号道具集");
                arr.arraySize = 4;
                for (int i = 0; i < 4; i++)
                    arr.GetArrayElementAtIndex(i).objectReferenceValue = 道具集[i];
                so.FindProperty("赋予后传送场景").stringValue = "次元空间";
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(sys);
                sb.AppendLine("  ✓ 剧情系统 / 暗号道具系统 接好 4 道具");
            }
            sb.AppendLine(boot != null ? "  ✓ 子场景Bootstrap 已挂" : "");
        });

        // ----- 次元空间场景 -----
        sb.AppendLine("\n[次元空间] ");
        修场景("Assets/Scenes/次元空间.scene", sb, (scene) =>
        {
            var hub = FindComponent<次元空间Hub>(scene);
            if (hub == null) { sb.AppendLine("  ⚠ 没找到 次元空间Hub，跳过"); return; }
            var huiJia = FindByName(scene, "传送门4-回家");
            var soHub = new SerializedObject(hub);
            if (huiJia != null && hub.回家门 == null)
            {
                soHub.FindProperty("回家门").objectReferenceValue = huiJia;
                sb.AppendLine("  ✓ 次元空间Hub.回家门 已接 传送门4-回家");
            }
            // 接 3 个灵蝉对白 SO（若 Hub 上还为空）
            if (hub.灵蝉对白_商朝后 == null && 对白_商 != null) { soHub.FindProperty("灵蝉对白_商朝后").objectReferenceValue = 对白_商; sb.AppendLine("  ✓ Hub.灵蝉对白_商朝后 已接"); }
            if (hub.灵蝉对白_秦朝后 == null && 对白_秦 != null) { soHub.FindProperty("灵蝉对白_秦朝后").objectReferenceValue = 对白_秦; sb.AppendLine("  ✓ Hub.灵蝉对白_秦朝后 已接"); }
            if (hub.灵蝉对白_唐朝后 == null && 对白_唐 != null) { soHub.FindProperty("灵蝉对白_唐朝后").objectReferenceValue = 对白_唐; sb.AppendLine("  ✓ Hub.灵蝉对白_唐朝后 已接"); }
            soHub.ApplyModifiedProperties();
            EditorUtility.SetDirty(hub);
            // 验证 4 门 nextScene 配置
            string[] 应有 = { "商朝", "咸阳街市", "长安诗会", "结局" };
            var doors = FindAllComponents<传送门>(scene);
            doors.Sort((a, b) => a.gateIndex.CompareTo(b.gateIndex));
            for (int i = 0; i < doors.Count && i < 应有.Length; i++)
            {
                if (doors[i].nextScene != 应有[i])
                {
                    var so = new SerializedObject(doors[i]);
                    so.FindProperty("nextScene").stringValue = 应有[i];
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(doors[i]);
                    sb.Append("  ✓ 修复门").Append(doors[i].gateIndex).Append(" nextScene → ").AppendLine(应有[i]);
                }
            }
        });

        // ----- 太学藏书阁场景 -----
        sb.AppendLine("\n[太学藏书阁] ");
        修场景("Assets/Scenes/秦朝/太学藏书阁.scene", sb, (scene) =>
        {
            var rs = FindComponent<RescueScore>(scene);
            if (rs == null) { sb.AppendLine("  ⚠ 没找到 RescueScore"); return; }
            var qq = rs.GetComponent<秦朝通关>();
            if (qq == null)
            {
                qq = rs.gameObject.AddComponent<秦朝通关>();
                EditorUtility.SetDirty(rs.gameObject);
                sb.AppendLine("  ✓ 给 RescueScore GameObject 挂上 秦朝通关");
            }
            else sb.AppendLine("  ✓ 秦朝通关 已挂");
            var so = new SerializedObject(qq);
            so.FindProperty("通关救数目标").intValue = Mathf.Max(1, so.FindProperty("通关救数目标").intValue);
            if (string.IsNullOrEmpty(so.FindProperty("下一场景").stringValue))
                so.FindProperty("下一场景").stringValue = "次元空间";
            so.ApplyModifiedProperties();

            // 失败 UI 装配：找 / 建 RescueGameOver + 失败面板
            var rgoComp = FindComponent<RescueGameOver>(scene);
            if (rgoComp == null)
            {
                rgoComp = rs.gameObject.AddComponent<RescueGameOver>();
                sb.AppendLine("  ✓ 给 RescueScore GameObject 挂上 RescueGameOver");
            }
            if (rgoComp.结束面板 == null)
            {
                var panel = 搭建失败UI(scene);
                var soRgo = new SerializedObject(rgoComp);
                soRgo.FindProperty("结束面板").objectReferenceValue = panel;
                soRgo.ApplyModifiedProperties();
                EditorUtility.SetDirty(rgoComp);
                sb.AppendLine("  ✓ 搭建失败 Canvas + 面板 + 重新开始按钮，并接到 RescueGameOver.结束面板");
            }
            else sb.AppendLine("  ✓ RescueGameOver.结束面板 已接");
        });

        // ----- 结局场景：主角自白 SO + 接到 结局触发器.告别对白 -----
        sb.AppendLine("\n[结局 - 主角自白] ");
        var 小明NPC = AssetDatabase.LoadAssetAtPath<NPCSO>("Assets/SO/商朝/小明.asset");
        EnsureFolder("Assets/SO/结局");
        var 主角自白SO = 确保对白SO("Assets/SO/结局/主角自白.asset",
            "（请填写：小明回到家后的自白。可以写多句，在 dialogueContent 数组里加。）", 小明NPC, sb);
        修场景("Assets/Scenes/结局.scene", sb, (scene) =>
        {
            var trig = FindComponent<结局触发器>(scene);
            if (trig == null) { sb.AppendLine("  ⚠ 没找到 结局触发器 组件"); return; }
            if (trig.告别对白 == null && 主角自白SO != null)
            {
                var s = new SerializedObject(trig);
                s.FindProperty("告别对白").objectReferenceValue = 主角自白SO;
                s.FindProperty("结束后场景").stringValue = ""; // 空 = 退出游戏
                s.ApplyModifiedProperties();
                EditorUtility.SetDirty(trig);
                sb.AppendLine("  ✓ 结局触发器.告别对白 已接 " + AssetDatabase.GetAssetPath(主角自白SO));
            }
            else if (trig.告别对白 != null) sb.AppendLine("  ✓ 结局触发器.告别对白 已有");
        });

        sb.AppendLine("\n===== 装配修复完成 =====");
        Debug.Log(sb.ToString());
    }

    static GameObject 搭建失败UI(Scene scene)
    {
        // Canvas
        var canvasGo = new GameObject("失败UI");
        SceneManager.MoveGameObjectToScene(canvasGo, scene);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // 在 HUD 之上
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // 失败面板（全屏半透黑底，默认隐藏）
        var panel = new GameObject("失败面板");
        panel.transform.SetParent(canvasGo.transform, false);
        var prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        var pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0, 0, 0, 0.78f);

        // 标题
        var t1 = new GameObject("标题");
        t1.transform.SetParent(panel.transform, false);
        var t1rt = t1.AddComponent<RectTransform>();
        t1rt.anchorMin = new Vector2(0.5f, 0.66f); t1rt.anchorMax = new Vector2(0.5f, 0.66f);
        t1rt.sizeDelta = new Vector2(1200, 140); t1rt.anchoredPosition = Vector2.zero;
        var t1tmp = t1.AddComponent<TextMeshProUGUI>();
        t1tmp.text = "时辰已到！";
        t1tmp.fontSize = 96; t1tmp.alignment = TextAlignmentOptions.Center;
        t1tmp.color = new Color(0.95f, 0.5f, 0.3f, 1f);

        // 副标题
        var t2 = new GameObject("副标题");
        t2.transform.SetParent(panel.transform, false);
        var t2rt = t2.AddComponent<RectTransform>();
        t2rt.anchorMin = new Vector2(0.5f, 0.52f); t2rt.anchorMax = new Vector2(0.5f, 0.52f);
        t2rt.sizeDelta = new Vector2(1200, 80); t2rt.anchoredPosition = Vector2.zero;
        var t2tmp = t2.AddComponent<TextMeshProUGUI>();
        t2tmp.text = "竹简未能尽数救下……";
        t2tmp.fontSize = 48; t2tmp.alignment = TextAlignmentOptions.Center;
        t2tmp.color = new Color(0.9f, 0.9f, 0.85f, 1f);

        // 重新开始按钮
        var btn = new GameObject("重新开始按钮");
        btn.transform.SetParent(panel.transform, false);
        var brt = btn.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0.28f); brt.anchorMax = new Vector2(0.5f, 0.28f);
        brt.sizeDelta = new Vector2(400, 110); brt.anchoredPosition = Vector2.zero;
        var bImg = btn.AddComponent<Image>();
        bImg.color = new Color(0.85f, 0.7f, 0.4f, 1f);
        var bBtn = btn.AddComponent<Button>();
        bBtn.targetGraphic = bImg;
        var ub = btn.AddComponent<UIButton>();
        ub.type = UIButton.BtnType.重新开始;

        // 按钮文字
        var bt = new GameObject("文字");
        bt.transform.SetParent(btn.transform, false);
        var btrt = bt.AddComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one;
        btrt.offsetMin = Vector2.zero; btrt.offsetMax = Vector2.zero;
        var bttmp = bt.AddComponent<TextMeshProUGUI>();
        bttmp.text = "重新开始";
        bttmp.fontSize = 48; bttmp.alignment = TextAlignmentOptions.Center;
        bttmp.color = Color.black;

        panel.SetActive(false);
        return panel;
    }

    static void 接字形Sprite(物品[] 道具集, string 文件夹, string 字段名, StringBuilder sb)
    {
        if (道具集 == null) return;
        string[] 字 = { "之", "马", "开", "门" };
        for (int i = 0; i < 4; i++)
        {
            if (道具集[i] == null) continue;
            string path = "Assets/美术资源/道具/" + 文件夹 + "/" + 字[i] + ".png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) { sb.Append("  ✗ 找不到 ").AppendLine(path); continue; }
            var so = new SerializedObject(道具集[i]);
            var p = so.FindProperty(字段名);
            if (p.objectReferenceValue != sprite)
            {
                p.objectReferenceValue = sprite;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(道具集[i]);
                sb.Append("  ✓ ").Append(道具集[i].name).Append(".").Append(字段名).Append(" ← ").AppendLine(path);
            }
        }
    }

    static void EnsureFolder(string assetFolder)
    {
        if (AssetDatabase.IsValidFolder(assetFolder)) return;
        string parent = System.IO.Path.GetDirectoryName(assetFolder).Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(assetFolder);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    static DialogueSO 确保对白SO(string path, string 占位文本, NPCSO 灵蝉, StringBuilder sb)
    {
        var so = AssetDatabase.LoadAssetAtPath<DialogueSO>(path);
        if (so != null) { sb.Append("  ✓ 已存在 ").AppendLine(path); return so; }
        so = ScriptableObject.CreateInstance<DialogueSO>();
        so.npcData = 灵蝉;
        so.dialogueContent = new string[] { 占位文本 };
        so.dialogueOptions = new DialogueSO.DialogueOption[0];
        so.callbacks = new DialogueSO.DialogueCallback[0];
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        sb.Append("  ✓ 新建 ").AppendLine(path);
        return so;
    }

    static 物品[] 加载4个道具(StringBuilder sb)
    {
        var arr = new 物品[4];
        string[] 字 = { "之", "马", "开", "门" };
        for (int i = 0; i < 4; i++)
        {
            var guids = AssetDatabase.FindAssets("t:物品 甲骨文-" + 字[i]);
            if (guids.Length == 0) { sb.AppendLine("  ✗ 没找到 甲骨文-" + 字[i] + " 物品 SO"); arr[i] = null; }
            else arr[i] = AssetDatabase.LoadAssetAtPath<物品>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        return arr;
    }

    static GameObject GetOrCreate(Scene scene, string name)
    {
        var go = FindByName(scene, name);
        if (go == null)
        {
            go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
        }
        return go;
    }

    static void 修场景(string path, StringBuilder sb, System.Action<Scene> 操作)
    {
        var scene = EditorSceneManager.GetSceneByPath(path);
        bool 我打开的 = !(scene.IsValid() && scene.isLoaded);
        if (我打开的) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        try
        {
            操作(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            sb.AppendLine("  ↳ 已保存场景");
        }
        catch (System.Exception e) { sb.Append("  [异常] ").AppendLine(e.Message); }
    }

    // ============================================================
    // 调试 - 运行时命令（必须 Play 模式）
    // ============================================================
    [MenuItem(MENU_ROOT + "调试/赋予 4 个暗号道具（仅 Play）", priority = 200)]
    public static void 调试_赋予道具()
    {
        if (!EnsurePlaying()) return;
        if (暗号道具系统.Instance == null) { Debug.LogError("暗号道具系统.Instance 为空，先到王宫场景或让 Bootstrap 跑一次"); return; }
        暗号道具系统.Instance.赋予暗号道具();
    }

    [MenuItem(MENU_ROOT + "调试/演化字形 → 甲骨（仅 Play）", priority = 210)]
    public static void 调试_演化_甲骨() => 调试演化(物品.字形阶段.甲骨);

    [MenuItem(MENU_ROOT + "调试/演化字形 → 秦篆（仅 Play）", priority = 211)]
    public static void 调试_演化_秦篆() => 调试演化(物品.字形阶段.秦篆);

    [MenuItem(MENU_ROOT + "调试/演化字形 → 唐楷（仅 Play）", priority = 212)]
    public static void 调试_演化_唐楷() => 调试演化(物品.字形阶段.唐楷);

    static void 调试演化(物品.字形阶段 阶段)
    {
        if (!EnsurePlaying()) return;
        if (暗号道具系统.Instance == null) { Debug.LogError("暗号道具系统.Instance 为空"); return; }
        if (GameManager.Instance != null) GameManager.Instance.暗号字形阶段 = (int)阶段;
        暗号道具系统.Instance.演化到(阶段);
    }

    [MenuItem(MENU_ROOT + "调试/解锁到下一关 (unlockedGateIndex++)（仅 Play）", priority = 220)]
    public static void 调试_解锁下一关()
    {
        if (!EnsurePlaying()) return;
        if (GameManager.Instance == null) { Debug.LogError("GameManager.Instance 为空"); return; }
        GameManager.Instance.UnlockNextGate();
        Debug.Log("unlockedGateIndex → " + GameManager.Instance.unlockedGateIndex);
    }

    [MenuItem(MENU_ROOT + "调试/清空背包（仅 Play）", priority = 230)]
    public static void 调试_清空背包()
    {
        if (!EnsurePlaying()) return;
        if (InventoryManager.instance == null || InventoryManager.instance.myBag == null) { Debug.LogError("背包为空"); return; }
        InventoryManager.instance.myBag.itemList.Clear();
        InventoryManager.强制刷新UI();
        Debug.Log("背包已清空");
    }

    [MenuItem(MENU_ROOT + "调试/重置全部演化&对白进度（仅 Play）", priority = 240)]
    public static void 调试_重置进度()
    {
        if (!EnsurePlaying()) return;
        if (GameManager.Instance == null) { Debug.LogError("GameManager.Instance 为空"); return; }
        GameManager.Instance.暗号字形阶段 = 0;
        GameManager.Instance.灵蝉对白_商朝后已播 = false;
        GameManager.Instance.灵蝉对白_秦朝后已播 = false;
        GameManager.Instance.灵蝉对白_唐朝后已播 = false;
        Debug.Log("演化阶段和灵蝉对白播放标记已重置");
    }

    [MenuItem(MENU_ROOT + "调试/删除磁盘存档（Editor 任意时刻可点）", priority = 250)]
    public static void 调试_删存档()
    {
        string path = Application.persistentDataPath + "/game_SaveData/saveData.txt";
        if (System.IO.File.Exists(path)) { System.IO.File.Delete(path); Debug.Log("已删除：" + path); }
        else Debug.Log("无存档：" + path);
    }

    [MenuItem(MENU_ROOT + "调试/打开存档目录（Editor）", priority = 251)]
    public static void 调试_打开存档目录()
    {
        string path = Application.persistentDataPath + "/game_SaveData";
        if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
    }

    // ============================================================
    // 工具方法
    // ============================================================
    static bool EnsurePlaying()
    {
        if (EditorApplication.isPlaying) return true;
        Debug.LogWarning("该命令需要先进入 Play 模式");
        return false;
    }

    delegate void 场景检查回调(Scene scene);

    static void 检查场景(StringBuilder sb, string path, 场景检查回调 cb)
    {
        var scene = EditorSceneManager.GetSceneByPath(path);
        bool 已加载 = scene.IsValid() && scene.isLoaded;
        bool 我加载的 = false;
        if (!已加载)
        {
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            我加载的 = true;
        }
        try { cb(scene); }
        catch (System.Exception e) { sb.Append("  [异常] ").AppendLine(e.Message); }
        finally
        {
            if (我加载的 && scene.IsValid())
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    static GameObject FindByName(Scene scene, string name)
    {
        if (!scene.IsValid()) return null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            var t = root.transform.Find(name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    static T FindComponent<T>(Scene scene) where T : Component
    {
        if (!scene.IsValid()) return null;
        foreach (var root in scene.GetRootGameObjects())
        {
            var c = root.GetComponentInChildren<T>(true);
            if (c != null) return c;
        }
        return null;
    }

    static List<T> FindAllComponents<T>(Scene scene) where T : Component
    {
        var list = new List<T>();
        if (!scene.IsValid()) return list;
        foreach (var root in scene.GetRootGameObjects())
            list.AddRange(root.GetComponentsInChildren<T>(true));
        return list;
    }
}
#endif
