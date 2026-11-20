using Form;
using System;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class GetCharacterParameterCmd : SynchronousQueryCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetCharacterParameterCmd());
        }
        public override string GetName() => "GetCharacterParameter";
        protected override BoxDataForm.Data Query(ReadOnlySpan<BoxDataForm.Data> prm)
        {
            var parameters = CharacterProductForm.ResolveReference(prm[0].str)?.paramDic;
            return parameters != null && parameters.TryGetValue(prm[1].str, out var parameter) && parameter != null
                ? parameter.GetValue().DeepCopy()
                : CodeHelper.CreateBox();
        }
    }
}
