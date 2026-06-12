using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Z_DesignStyle;
using Z_Code;

/// <summary>
/// 检查 Form 表之间的外键引用完整性
/// 例如：CharacterProductForm.Data.avatarTex 引用 TexAssetForm，检查该值是否存在于 TexAssetForm.DataById 中
/// 同时检查 EventProgramData 中每个项的代码编译是否正确
/// </summary>
public class CheckController : Z_Controller<ModManager>
{
    public CheckController(ModManager super) : base(super)
    {
    }

    /// <summary>
    /// 唯一入口方法：检查所有表的外键引用和代码编译，找出丢失的索引和编译错误，返回报告字符串
    /// </summary>
    public string Check()
    {
        var sb = new StringBuilder();
        sb.AppendLine("========== 数据检查报告 ==========");
        sb.AppendLine();

        // 1. 检查外键引用
        sb.AppendLine("【一、外键引用检查】");
        sb.AppendLine();
        var foreignKeyMappings = GetForeignKeyMappings();
        foreach (var mapping in foreignKeyMappings)
        {
            CheckForeignKey(mapping, sb);
        }

        // 2. 检查代码编译
        sb.AppendLine("【二、代码编译检查】");
        sb.AppendLine();
        CheckEventProgramCode(sb);

        sb.AppendLine("========== 检查完毕 ==========");
        return sb.ToString();
    }

    /// <summary>
    /// 检查 EventProgramData 中每个项的代码编译
    /// </summary>
    private void CheckEventProgramCode(StringBuilder sb)
    {
        try
        {
            var compiler = new Compiler();
            int totalCount = 0;
            int errorCount = 0;

            foreach (var kvp in Form.EventProgramDataForm.DataByUid)
            {
                var data = kvp.Value;
                if (string.IsNullOrEmpty(data.code))
                    continue;

                totalCount++;
                var zl = compiler.Compile(data.code, out var syntaxs, out var paramCount, out var ret, out var zCodeMap, out var errors);

                if (errors.Count > 0)
                {
                    errorCount++;
                    sb.AppendLine($"【程序: {data.name} (uid={data.uid}, category={data.category}, type={data.type})】");
                    sb.AppendLine($"  代码: {data.code}");
                    sb.AppendLine($"  编译错误 ({errors.Count}个):");
                    foreach (var error in errors)
                    {
                        sb.AppendLine($"    - {error}");
                    }
                    sb.AppendLine();
                }
            }

            if (totalCount == 0)
            {
                sb.AppendLine("  没有需要编译的代码");
            }
            else if (errorCount == 0)
            {
                sb.AppendLine($"  全部 {totalCount} 个程序代码编译通过");
            }
            else
            {
                sb.AppendLine($"  共 {totalCount} 个程序，{errorCount} 个存在编译错误");
            }
            sb.AppendLine();
        }
        catch (Exception ex)
        {
            sb.AppendLine($"[错误] 检查代码编译时异常: {ex.Message}");
            sb.AppendLine();
        }
    }

    /// <summary>
    /// 外键映射定义
    /// </summary>
    private class ForeignKeyMapping
    {
        public string SourceFormName;      // 源表名
        public string FieldName;           // 字段名
        public string TargetFormName;      // 目标表名
        public string TargetIndexName;     // 目标索引名 (DataById/DataByUid)
        public Type FieldType;             // 字段类型 (int, List<int>, Dictionary<K,int> 等)
        
        // 纯显示用参数（可选，为空则使用原名）
        public string SourceDisplayName;   // 源表显示名
        public string FieldDisplayName;    // 字段显示名
        public string TargetDisplayName;   // 目标表显示名
        public string SourceDisplayFieldName; // 源表用于显示的字段名（如 "name"，用于输出时标识具体记录）
    }

    /// <summary>
    /// 获取所有外键映射关系
    /// </summary>
    private static List<ForeignKeyMapping> GetForeignKeyMappings()
    {
        var list = new List<ForeignKeyMapping>();

        // ========== CharacterProductForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "CharacterProductForm", FieldName = "avatarTex", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "角色", FieldDisplayName = "头像", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "CharacterProductForm", FieldName = "illustration", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "角色", FieldDisplayName = "立绘", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "CharacterProductForm", FieldName = "minimapIcon", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "角色", FieldDisplayName = "小地图图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "CharacterProductForm", FieldName = "skill", TargetFormName = "SkillProductForm", TargetIndexName = "DataByUid", FieldType = typeof(Dictionary<,>), SourceDisplayName = "角色", FieldDisplayName = "技能", TargetDisplayName = "技能", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "CharacterProductForm", FieldName = "equips", TargetFormName = "ItemProductForm", TargetIndexName = "DataByUid", FieldType = typeof(Dictionary<,>), SourceDisplayName = "角色", FieldDisplayName = "装备", TargetDisplayName = "道具", SourceDisplayFieldName = "name" });

        // ========== ItemProductForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "ItemProductForm", FieldName = "iconTexName", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "道具", FieldDisplayName = "图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "ItemProductForm", FieldName = "minimapIcon", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "道具", FieldDisplayName = "小地图图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "ItemProductForm", FieldName = "model", TargetFormName = "MapModelForm", TargetIndexName = "DataById", FieldType = typeof(object), SourceDisplayName = "道具", FieldDisplayName = "模型", TargetDisplayName = "模型", SourceDisplayFieldName = "name" });

        list.Add(new ForeignKeyMapping { SourceFormName = "ItemProductForm", FieldName = "styleTex", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(Dictionary<,>), SourceDisplayName = "道具", FieldDisplayName = "样式", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });

        // ========== SkillProductForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "SkillProductForm", FieldName = "icon", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "技能", FieldDisplayName = "图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "SkillProductForm", FieldName = "characterUid", TargetFormName = "CharacterProductForm", TargetIndexName = "DataByUid", FieldType = typeof(int), SourceDisplayName = "技能", FieldDisplayName = "装备角色", TargetDisplayName = "角色", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "SkillProductForm", FieldName = "triggerConditionUid", TargetFormName = "EventTriggerForm", TargetIndexName = "DataByUid", FieldType = typeof(int), SourceDisplayName = "技能", FieldDisplayName = "触发条件ID", TargetDisplayName = "事件触发器表", SourceDisplayFieldName = "name" });

        // ========== MapObjectForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "MapObjectForm", FieldName = "icon", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "地图景物", FieldDisplayName = "图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "MapObjectForm", FieldName = "minimapIcon", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "地图景物", FieldDisplayName = "小地图图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });

        // ========== MapTextureForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "MapTextureForm", FieldName = "icon", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "地图图片", FieldDisplayName = "图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "MapTextureForm", FieldName = "texs", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(List<int>), SourceDisplayName = "地图图片", FieldDisplayName = "纹理列表", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });

        // ========== MapMaskForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "MapMaskForm", FieldName = "icon", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "地图遮罩", FieldDisplayName = "图标", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });
        list.Add(new ForeignKeyMapping { SourceFormName = "MapMaskForm", FieldName = "texsName", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(List<int>), SourceDisplayName = "地图遮罩", FieldDisplayName = "纹理列表", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });

        // ========== SceneForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "SceneForm", FieldName = "miniMap", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "场景", FieldDisplayName = "小地图", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });

        // ========== ProgressForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "ProgressForm", FieldName = "sceneId", TargetFormName = "SceneForm", TargetIndexName = "DataByUid", FieldType = typeof(int), SourceDisplayName = "进度", FieldDisplayName = "场景", TargetDisplayName = "场景", SourceDisplayFieldName = "uid" });
        list.Add(new ForeignKeyMapping { SourceFormName = "ProgressForm", FieldName = "characterUid", TargetFormName = "CharacterProductForm", TargetIndexName = "DataByUid", FieldType = typeof(int), SourceDisplayName = "进度", FieldDisplayName = "角色ID", TargetDisplayName = "角色", SourceDisplayFieldName = "uid" });
        list.Add(new ForeignKeyMapping { SourceFormName = "ProgressForm", FieldName = "largeMap", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "进度", FieldDisplayName = "大地图", TargetDisplayName = "图片", SourceDisplayFieldName = "uid" });
        list.Add(new ForeignKeyMapping { SourceFormName = "ProgressForm", FieldName = "bag", TargetFormName = "ItemProductForm", TargetIndexName = "DataByUid", FieldType = typeof(List<int>), SourceDisplayName = "进度", FieldDisplayName = "背包", TargetDisplayName = "道具", SourceDisplayFieldName = "uid" });
        list.Add(new ForeignKeyMapping { SourceFormName = "ProgressForm", FieldName = "team", TargetFormName = "CharacterProductForm", TargetIndexName = "DataByUid", FieldType = typeof(List<int>), SourceDisplayName = "进度", FieldDisplayName = "队伍", TargetDisplayName = "角色", SourceDisplayFieldName = "uid" });
        list.Add(new ForeignKeyMapping { SourceFormName = "ProgressForm", FieldName = "teamActive", TargetFormName = "CharacterProductForm", TargetIndexName = "DataByUid", FieldType = typeof(List<int>), SourceDisplayName = "进度", FieldDisplayName = "活动队伍", TargetDisplayName = "角色", SourceDisplayFieldName = "uid" });

        // ========== ImageUiItemForm ==========
        list.Add(new ForeignKeyMapping { SourceFormName = "ImageUiItemForm", FieldName = "tex", TargetFormName = "TexAssetForm", TargetIndexName = "DataById", FieldType = typeof(int), SourceDisplayName = "弹出图片", FieldDisplayName = "图片", TargetDisplayName = "图片", SourceDisplayFieldName = "name" });

        return list;
    }

    /// <summary>
    /// 检查单个外键映射
    /// </summary>
    private static void CheckForeignKey(ForeignKeyMapping mapping, StringBuilder sb)
    {
        try
        {
            // 获取源 Form 类型
            var sourceFormType = GetFormType(mapping.SourceFormName);
            if (sourceFormType == null)
            {
                sb.AppendLine($"[警告] 未找到源表: {mapping.SourceFormName}");
                return;
            }

            // 获取目标 Form 类型
            var targetFormType = GetFormType(mapping.TargetFormName);
            if (targetFormType == null)
            {
                sb.AppendLine($"[警告] 未找到目标表: {mapping.TargetFormName}");
                return;
            }

            // 获取源 Form 的 Data 集合 (DataByUid 或 DataById)
            var sourceDataDict = GetFormDataDictionary(sourceFormType);
            if (sourceDataDict == null)
            {
                sb.AppendLine($"[警告] {mapping.SourceFormName} 没有 DataByUid/DataById 索引");
                return;
            }

            // 获取目标 Form 的索引字典
            var targetIndexDict = GetFormIndexDictionary(targetFormType, mapping.TargetIndexName);
            if (targetIndexDict == null)
            {
                sb.AppendLine($"[警告] {mapping.TargetFormName} 没有 {mapping.TargetIndexName} 索引");
                return;
            }

            // 获取目标索引的 Keys（用于检查）
            var targetKeys = new HashSet<int>(targetIndexDict.Keys.Cast<int>());

            // 获取 Data 类的属性
            var dataType = sourceFormType.GetNestedType("Data", BindingFlags.Public);
            if (dataType == null) return;

            var fieldProp = dataType.GetProperty(mapping.FieldName, BindingFlags.Public | BindingFlags.Instance);
            if (fieldProp == null)
            {
                // 字段可能在父类中
                fieldProp = dataType.GetProperty(mapping.FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (fieldProp == null)
                {
                    sb.AppendLine($"[警告] {mapping.SourceFormName}.Data 没有 {mapping.FieldName} 字段");
                    return;
                }
            }

            // 遍历源表所有 Data，检查外键引用
            var missingRefs = new List<string>();

            // 获取用于显示的字段属性
            PropertyInfo displayProp = null;
            if (!string.IsNullOrEmpty(mapping.SourceDisplayFieldName))
            {
                displayProp = dataType.GetProperty(mapping.SourceDisplayFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            }

            foreach (DictionaryEntry entry in sourceDataDict)
            {
                var dataKey = entry.Key;
                var data = entry.Value;

                var fieldValue = fieldProp.GetValue(data);
                if (fieldValue == null) continue;

                // 获取显示字段的值
                var displayValue = displayProp?.GetValue(data)?.ToString() ?? dataKey.ToString();

                // 根据字段类型提取 int 值
                var intValues = ExtractIntValues(fieldValue, fieldProp.PropertyType);
                foreach (var intVal in intValues)
                {
                    if (intVal == 0 || intVal == -1) continue; // 跳过默认值
                    if (!targetKeys.Contains(intVal))
                    {
                        var srcName = mapping.SourceDisplayName ?? mapping.SourceFormName;
                        var fldName = mapping.FieldDisplayName ?? mapping.FieldName;
                        var tgtName = mapping.TargetDisplayName ?? mapping.TargetFormName;
                        missingRefs.Add($"{srcName}[{displayValue}]的{fldName}字段值[{intVal}]在{tgtName}中不存在");
                    }
                }
            }

            if (missingRefs.Count > 0)
            {
                var srcName = mapping.SourceDisplayName ?? mapping.SourceFormName;
                var fldName = mapping.FieldDisplayName ?? mapping.FieldName;
                var tgtName = mapping.TargetDisplayName ?? mapping.TargetFormName;
                sb.AppendLine($"【{srcName}.{fldName} → {tgtName}】");
                sb.AppendLine($"  缺失引用 ({missingRefs.Count}个):");
                foreach (var refInfo in missingRefs.Take(20)) // 限制输出数量
                {
                    sb.AppendLine($"    - {refInfo}");
                }
                if (missingRefs.Count > 20)
                {
                    sb.AppendLine($"    ... 还有 {missingRefs.Count - 20} 个");
                }
                sb.AppendLine();
            }
        }
        catch (Exception ex)
        {
            var srcName = mapping.SourceDisplayName ?? mapping.SourceFormName;
            var fldName = mapping.FieldDisplayName ?? mapping.FieldName;
            sb.AppendLine($"[错误] 检查 {srcName}.{fldName} 时异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 从字段值中提取 int 值（处理 int、List<int>、Dictionary<K,int> 等类型）
    /// </summary>
    private static List<int> ExtractIntValues(object value, Type type)
    {
        var result = new List<int>();

        if (value == null) return result;

        // 直接是 int
        if (value is int intVal)
        {
            result.Add(intVal);
            return result;
        }

        // List<int>
        if (value is IList list)
        {
            foreach (var item in list)
            {
                if (item is int i) result.Add(i);
            }
            return result;
        }

        // Dictionary<K,int> - 提取 Values 中的 int
        if (value is IDictionary dict)
        {
            foreach (DictionaryEntry entry in dict)
            {
                if (entry.Value is int i) result.Add(i);
            }
            return result;
        }

        // MapModelForm.Data 等复杂类型 - 尝试获取 id 或 uid 属性
        var valueType = value.GetType();
        var idProp = valueType.GetProperty("id") ?? valueType.GetProperty("uid");
        if (idProp != null)
        {
            var idVal = idProp.GetValue(value);
            if (idVal is int i) result.Add(i);
        }

        return result;
    }

    /// <summary>
    /// 获取 Form 类型的 DataByUid 或 DataById 字典
    /// </summary>
    private static IDictionary GetFormDataDictionary(Type formType)
    {
        // 尝试 DataByUid
        var uidProp = formType.GetProperty("DataByUid", BindingFlags.Public | BindingFlags.Static);
        if (uidProp != null) return uidProp.GetValue(null) as IDictionary;

        // 尝试 DataById
        var idProp = formType.GetProperty("DataById", BindingFlags.Public | BindingFlags.Static);
        if (idProp != null) return idProp.GetValue(null) as IDictionary;

        return null;
    }

    /// <summary>
    /// 获取 Form 类型的指定索引字典
    /// </summary>
    private static IDictionary GetFormIndexDictionary(Type formType, string indexName)
    {
        var prop = formType.GetProperty(indexName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (prop != null) return prop.GetValue(null) as IDictionary;
        return null;
    }

    /// <summary>
        /// 根据 Form 名称获取 Type（支持跨命名空间/程序集）
        /// </summary>
        private static Type GetFormType(string formName)
        {
            // 按优先级尝试不同命名空间
            var namespaces = new[]
            {
                "Form",                    // GameSample 层
                "Z_DataSystem.Form",       // Z_Level2 层（TexAssetForm 等）
            };

            foreach (var ns in namespaces)
            {
                var fullName = $"{ns}.{formName}";
                // 先尝试当前程序集
                var formType = Type.GetType(fullName);
                if (formType != null) return formType;

                // 遍历所有已加载程序集查找
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    formType = asm.GetType(fullName);
                    if (formType != null) return formType;
                }
            }

            return null;
        }
}