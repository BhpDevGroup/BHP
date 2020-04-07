﻿using Bhp.SmartContract.Framework;
using Bhp.SmartContract.Framework.Services.Bhp;
using System.Numerics;
using Helper = Bhp.SmartContract.Framework.Helper;

namespace BHPNFT
{
    public partial class BHPNFT : SmartContract
    {
        //超级管理員
        static readonly byte[] superAdmin = Helper.ToScriptHash("AWWx2F1Ph9oJtbU8H2mcJGDgDeFDH8geWs");

        #region Storage key prefixes
        static readonly string StoragePrefixSysState = "sysState";
        static readonly string StoragePrefixAsset = "asset";
        static readonly string StoragePrefixAddrList = "addrNFTlist";
        static readonly string StoragePrefixApprovedAddrList = "approvedAddrNFTlist";
        static readonly string StoragePrefixPledgedAddrList = "pledgedAddrNFTlist";
        static readonly string StoragePrefixMintAddr = "addrApproveMintAddrs";
        #endregion

        public static object Main(string operation, object[] args)
        {
            //UTXO转账转入转出都不允许
            if (Runtime.Trigger == TriggerType.Verification || Runtime.Trigger == TriggerType.VerificationR)
            {
                return false;
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                #region 基础方法
                //无入参只读类 
                if (operation == "name")
                {
                    return Name();
                }
                if (operation == "symbol")
                {
                    return Symbol();
                }
                if (operation == "decimals")
                {
                    return Decimals();
                }
                if (operation == "supportedStandards")
                {
                    return SupportedStandards();
                }
                if (operation == "totalSupply")
                {
                    return TotalSupply();
                }
                #endregion

                #region 转账
                if (operation == "transfer")
                {
                    if (args.Length != 2) return false;
                    return Transfer((byte[])args[0], (BigInteger)args[1]);
                }
                if (operation == "transferFrom")
                {
                    if (args.Length != 2) return false;
                    return TransferFrom((byte[])args[0], (BigInteger)args[1]);
                }
                if (operation == "transferTo")
                {
                    if (args.Length != 2) return false;
                    return TransferTo((byte[])args[0], (BigInteger)args[1]);
                }
                #endregion

                #region 授权
                if (operation == "approve")
                {
                    if (args.Length != 2 && args.Length != 3) return false;
                    if (args.Length == 2) return Approve((byte[])args[0], (BigInteger)args[1], false);
                    return Approve((byte[])args[0], (BigInteger)args[1], (bool)args[2]);
                }

                #endregion

                #region 质押、解质押

                if (operation == "pledger")
                {
                    if (args.Length != 3) return false;
                    return PledgerNFT((byte[])args[0], (BigInteger)args[1], (bool)args[2]);
                }
                if (operation == "unpledger")
                {
                    if (args.Length != 1) return false;
                    return UnPledge((BigInteger)args[0]);
                }

                #endregion

                #region 查询
                //单token_id只读类
                if (operation == "allowance")
                {
                    if (args.Length != 1) return false;
                    return Allowance((BigInteger)args[0]);
                }
                if (operation == "ownerOf")
                {
                    if (args.Length != 1) return false;
                    return OwnerOf((BigInteger)args[0]);
                }

                if (operation == "token")
                {
                    if (args.Length != 1) return false;
                    return GetAsset((BigInteger)args[0]);
                }
                //所有权类
                if (operation == "balanceOf")
                {
                    if (args.Length != 1) return false;
                    return BalanceOf((byte[])args[0]);
                }
                if (operation == "tokenIDsOfOwner")
                {
                    if (args.Length != 1) return false;
                    return TokenIDsOfOwner((byte[])args[0]);
                }
                if (operation == "tokenIDsOfApproved")
                {
                    if (args.Length != 1) return false;
                    return TokenIDsOfApproved((byte[])args[0]);
                }
                if (operation == "tokenIDsOfPledged")
                {
                    if (args.Length != 1) return false;
                    return TokenIDsOfPledged((byte[])args[0]);
                }
                #endregion

                #region 铸币
                //代币合约所有者操作(superAdmin)
                if (operation == "mintToken")
                {
                    if (args.Length != 15) return false;
                    return MintToken((byte[])args[0], (byte[])args[1], (BigInteger)args[2], (BigInteger)args[3],
                        (BigInteger)args[4], (BigInteger)args[5], (BigInteger)args[6], (BigInteger)args[7], (BigInteger)args[8],
                        (BigInteger)args[9], (BigInteger)args[10], (BigInteger)args[11], (BigInteger)args[12], (BigInteger)args[13], (BigInteger)args[14]);
                }
                #endregion

                #region 修改NFT属性

                if (operation == "modifyNFTattribute")
                {
                    if (args.Length != 3) return false;
                    return ModifyNFTattribute((BigInteger)args[0], (string)args[1], (object)args[2]);
                }
                #endregion

                #region 修改合约属性
                //设置操作（仅superAdmin）
                if (operation == "setName")
                {
                    if (args.Length != 1) return false;
                    return SetName((string)args[0]);
                }
                if (operation == "setSymbol")
                {
                    if (args.Length != 1) return false;
                    return SetSymbol((string)args[0]);
                }
                if (operation == "setSupportedStandards")
                {
                    if (args.Length != 1) return false;
                    return SetSupportedStandards((string[])args[0]);
                }

                #endregion

                #region 合约升级

                if (operation == "migrate")
                {
                    return MigrateContract(args);
                }

                #endregion

                #region 授权地址操作

                //获取发行地址
                if (operation == "getApproveMintAddr")
                {
                    return GetApproveMintAddr();
                }
                //增加发行地址（仅超级管理员）
                if (operation == "approveMintAddrAdd")
                {
                    if (args.Length != 1) return false;
                    return ApproveMintAddrAdd((byte[])args[0]);
                }
                //删除发行地址（仅超级管理员）
                if (operation == "approveMintAddrRemove")
                {
                    if (args.Length != 1) return false;
                    return ApproveMintAddrRemove((byte[])args[0]);
                }
                //判断地址是否为发行地址
                if (operation == "isMintAddress")
                {
                    if (args.Length != 1) return false;
                    return IsMintAddress((byte[])args[0]);
                }

                #endregion

            }
            return false;
        }

    }
}