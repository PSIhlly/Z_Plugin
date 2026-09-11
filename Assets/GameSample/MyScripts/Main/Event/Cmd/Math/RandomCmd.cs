using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class RandomCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new RandomCmd());
        }

        public override string GetName() => "Random";
        public override CmdBase GetNew() => new RandomCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var story = GameManager.instance.curStory;
            var progress = GameManager.instance.curProgress;
            if (story == null)
                throw new InvalidOperationException("当前故事不存在");
            if (progress == null)
                throw new InvalidOperationException("当前游戏进度不存在");

            var num1 = prm[0].num;
            var num2 = prm[1].num;
            if (float.IsNaN(num1) || float.IsInfinity(num1) ||
                float.IsNaN(num2) || float.IsInfinity(num2))
            {
                throw new ArgumentOutOfRangeException(nameof(prm), "随机数范围必须是有限数值");
            }

            var min = Math.Min(num1, num2);
            var max = Math.Max(num1, num2);
            var integerMin = (long)Math.Ceiling(min);
            var integerMax = (long)Math.Floor(max);
            if (integerMin < int.MinValue || integerMax > int.MaxValue || integerMin > integerMax)
                throw new ArgumentOutOfRangeException(nameof(prm), "随机数范围内必须包含有效整数");

            var seed = string.IsNullOrEmpty(story.randomSeed)
                ? story.id.ToString(CultureInfo.InvariantCulture)
                : story.randomSeed;
            var result = GetDeterministicInteger(seed, progress.seconds, (int)integerMin, (int)integerMax);
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(result) };
            return true;
        }

        internal static int GetDeterministicInteger(string seed, float seconds, int min, int max)
        {
            if (min == max)
                return min;

            var seedBytes = Encoding.UTF8.GetBytes(seed ?? string.Empty);
            var input = new byte[seedBytes.Length + sizeof(int)];
            Buffer.BlockCopy(seedBytes, 0, input, 0, seedBytes.Length);

            var secondsBits = BitConverter.ToInt32(BitConverter.GetBytes(seconds), 0);
            var inputOffset = seedBytes.Length;
            input[inputOffset] = (byte)(secondsBits >> 24);
            input[inputOffset + 1] = (byte)(secondsBits >> 16);
            input[inputOffset + 2] = (byte)(secondsBits >> 8);
            input[inputOffset + 3] = (byte)secondsBits;

            byte[] hash;
            using (var sha256 = SHA256.Create())
                hash = sha256.ComputeHash(input);

            var sample = ((uint)hash[0] << 24) |
                         ((uint)hash[1] << 16) |
                         ((uint)hash[2] << 8) |
                         hash[3];
            var range = (ulong)((long)max - min) + 1UL;
            var resultOffset = (ulong)Math.Floor(sample / 4294967296.0 * range);
            return (int)((long)min + (long)resultOffset);
        }
    }
}
