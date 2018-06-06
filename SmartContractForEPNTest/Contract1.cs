using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    public class ICO_Template : Framework.SmartContract
    {
        //Token Settings
        public static string Name() => "EPN2";
        public static string Symbol() => "epn2";
        public static readonly byte[] Owner = "AYJPcufRXYxYRUHFkQiHsD27wWW7RifG61".ToScriptHash();
        public static readonly byte[] Owner2 = "AUkADJRkBy3Fz4wf2ek9q93t6mCTarYi2A".ToScriptHash();
        public static readonly byte[] Owner3 = "AV98PwVvT2bSLwxLBuCYeCMtWnBYeWkAM1".ToScriptHash();
        //public static readonly byte[] Owner4 = "ALvu9jT4NJCED8MvzxTgpvepnrkYyoStEx".ToScriptHash();
        public static byte Decimals() => 8;
        private const ulong factor = 100000000;
        //private const ulong neo_decimals = 100000000;


        private const ulong pre_ico_cap  = 300000000 * factor;
        private const ulong pre_ico_cap2 = 100000000 * factor;
        private const ulong pre_ico_cap3 = 100000000 * factor;
       // private const ulong pre_ico_cap4 = 100 * factor;


        [DisplayName("transfer")]
        public static event Action<byte[], byte[], BigInteger> Transferred;



        public static Object Main(string operation, params object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                if (Owner.Length == 20)
                {
                    // if param Owner is script hash
                    return Runtime.CheckWitness(Owner);
                }
                else if (Owner.Length == 33)
                {
                    // if param Owner is public key
                    byte[] signature = operation.AsByteArray();
                    return VerifySignature(signature, Owner);
                }
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                if (operation == "deploy") return Deploy();
                //if (operation == "mintTokens") return MintTokens();
                if (operation == "totalSupply") return TotalSupply();
                if (operation == "name") return Name();
                if (operation == "symbol") return Symbol();
                if (operation == "transfer")
                {
                    if (args.Length != 3) return false;
                    byte[] from = (byte[])args[0];
                    byte[] to = (byte[])args[1];
                    BigInteger value = (BigInteger)args[2];
                    return Transfer(from, to, value);
                }
                if (operation == "balanceOf")
                {
                    if (args.Length != 1) return 0;
                    byte[] account = (byte[])args[0];
                    return BalanceOf(account);
                }
                if (operation == "decimals") return Decimals();
            }


            return false;
        }

        // initialization parameters, only once
        // 初始化参数
        public static bool Deploy()
        {
            byte[] total_supply = Storage.Get(Storage.CurrentContext, "totalSupply");
            if (total_supply.Length != 0) return false;
            Storage.Put(Storage.CurrentContext, Owner, pre_ico_cap);
            Storage.Put(Storage.CurrentContext, Owner2, pre_ico_cap2);
            Storage.Put(Storage.CurrentContext, Owner3, pre_ico_cap3);
           // Storage.Put(Storage.CurrentContext, Owner4, pre_ico_cap4);

            Storage.Put(Storage.CurrentContext, "totalSupply", pre_ico_cap + pre_ico_cap2 + pre_ico_cap3);
            Transferred(null, Owner, pre_ico_cap);
            Transferred(null, Owner2, pre_ico_cap2);
            Transferred(null, Owner3, pre_ico_cap3);
            //Transferred(null, Owner4, pre_ico_cap4);
            return true;
        }

        // get the total token supply
        // 获取已发行token总量
        public static BigInteger TotalSupply()
        {
            return Storage.Get(Storage.CurrentContext, "totalSupply").AsBigInteger();
        }

        // function that is always called when someone wants to transfer tokens.
        // 流转token调用

        public static bool Transfer(byte[] from, byte[] to, BigInteger value)
        {
            if (value <= 0) return false;
            if (!Runtime.CheckWitness(from)) return false;
            if (from == to) return true;
            BigInteger from_value = Storage.Get(Storage.CurrentContext, from).AsBigInteger();
            if (from_value < value) return false;
            if (from_value == value)
                Storage.Delete(Storage.CurrentContext, from);
            else
                Storage.Put(Storage.CurrentContext, from, from_value - value);
            BigInteger to_value = Storage.Get(Storage.CurrentContext, to).AsBigInteger();
            Storage.Put(Storage.CurrentContext, to, to_value + value);
            Transferred(from, to, value);
            return true;
        }

        // get the account balance of another account with address
        // 根据地址获取token的余额
        public static BigInteger BalanceOf(byte[] address)
        {
            return Storage.Get(Storage.CurrentContext, address).AsBigInteger();
        }

        /*
        // get smart contract script hash
        private static byte[] GetReceiver()
        {
            return ExecutionEngine.ExecutingScriptHash;
        }
        */
    }
}