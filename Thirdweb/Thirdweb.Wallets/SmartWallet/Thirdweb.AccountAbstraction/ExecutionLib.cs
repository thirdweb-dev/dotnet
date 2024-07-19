using System;
using System.Linq;

namespace Thirdweb.AccountAbstraction
{
    public static class ExecutionLib
    {
        public struct Execution
        {
            public byte[] Target { get; set; } // Address is 20 bytes
            public ulong Value { get; set; } // Assuming 64-bit
            public byte[] CallData { get; set; }

            public Execution(byte[] target, ulong value, byte[] callData)
            {
                if (target.Length != 20)
                    throw new ArgumentException("Target must be 20 bytes.");
                Target = target;
                Value = value;
                CallData = callData;
            }
        }

        public static Execution[] DecodeBatch(byte[] callData)
        {
            int offset = BitConverter.ToInt32(callData.Take(4).ToArray(), 0);
            int length = BitConverter.ToInt32(callData.Skip(offset).Take(4).ToArray(), 0);
            Execution[] executionBatch = new Execution[length];

            int currentIndex = offset + 4;
            for (int i = 0; i < length; i++)
            {
                byte[] target = callData.Skip(currentIndex).Take(20).ToArray();
                ulong value = BitConverter.ToUInt64(callData.Skip(currentIndex + 20).Take(8).ToArray(), 0);
                byte[] executionCallData = callData.Skip(currentIndex + 28).ToArray();

                executionBatch[i] = new Execution(target, value, executionCallData);
                currentIndex += 28 + executionCallData.Length;
            }

            return executionBatch;
        }

        public static byte[] EncodeBatch(Execution[] executions)
        {
            var batchData = executions.SelectMany(execution => execution.Target.Concat(BitConverter.GetBytes(execution.Value)).Concat(execution.CallData)).ToArray();

            byte[] callData = BitConverter.GetBytes(batchData.Length).Concat(batchData).ToArray();
            return callData;
        }

        public static (byte[] target, ulong value, byte[] callData) DecodeSingle(byte[] executionCalldata)
        {
            byte[] target = executionCalldata.Take(20).ToArray();
            ulong value = BitConverter.ToUInt64(executionCalldata.Skip(20).Take(8).ToArray(), 0);
            byte[] callData = executionCalldata.Skip(28).ToArray();

            return (target, value, callData);
        }

        public static byte[] EncodeSingle(byte[] target, ulong value, byte[] callData)
        {
            if (target.Length != 20)
                throw new ArgumentException("Target must be 20 bytes.");

            return target.Concat(BitConverter.GetBytes(value)).Concat(callData).ToArray();
        }
    }
}
