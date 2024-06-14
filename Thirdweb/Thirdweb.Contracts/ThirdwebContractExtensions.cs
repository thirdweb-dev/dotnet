using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public static class ThirdwebContractExtensions
    {
        #region Common

        public static async Task<BigInteger> GetBalance(this ThirdwebContract contract, ThirdwebClient client)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var rpc = ThirdwebRPC.GetRpcInstance(client, contract.Chain);
            var balanceHex = await rpc.SendRequestAsync<string>("eth_getBalance", contract.Address, "latest");
            return new HexBigInteger(balanceHex).Value;
        }

        #endregion

        #region ERC20

        // Check the balance of a specific address
        public static async Task<BigInteger> ERC20_BalanceOf(this ThirdwebContract contract, string ownerAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress);
        }

        // Get the total supply of the token
        public static async Task<BigInteger> ERC20_TotalSupply(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "totalSupply");
        }

        // Get the number of decimals used by the token
        public static async Task<int> ERC20_Decimals(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<int>(contract, "decimals");
        }

        // Get the symbol of the token
        public static async Task<string> ERC20_Symbol(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "symbol");
        }

        // Get the name of the token
        public static async Task<string> ERC20_Name(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "name");
        }

        // Get the allowance of a spender for a specific owner
        public static async Task<BigInteger> ERC20_Allowance(this ThirdwebContract contract, string ownerAddress, string spenderAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            if (string.IsNullOrEmpty(spenderAddress))
            {
                throw new ArgumentException("Spender address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "allowance", ownerAddress, spenderAddress);
        }

        // Approve a spender to spend a specific amount of tokens
        public static async Task<ThirdwebTransactionReceipt> ERC20_Approve(this ThirdwebContract contract, IThirdwebWallet wallet, string spenderAddress, BigInteger amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(spenderAddress))
            {
                throw new ArgumentException("Spender address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "approve", 0, spenderAddress, amount);
        }

        // Transfer tokens to a specific address
        public static async Task<ThirdwebTransactionReceipt> ERC20_Transfer(this ThirdwebContract contract, IThirdwebWallet wallet, string toAddress, BigInteger amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "transfer", 0, toAddress, amount);
        }

        // Transfer tokens from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC20_TransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "transferFrom", 0, fromAddress, toAddress, amount);
        }

        #endregion

        #region ERC721

        // Check the balance of a specific address
        public static async Task<BigInteger> ERC721_BalanceOf(this ThirdwebContract contract, string ownerAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress);
        }

        // Get the owner of a specific token
        public static async Task<string> ERC721_OwnerOf(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "ownerOf", tokenId);
        }

        // Get the name of the token
        public static async Task<string> ERC721_Name(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "name");
        }

        // Get the symbol of the token
        public static async Task<string> ERC721_Symbol(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "symbol");
        }

        // Get the URI of a specific token
        public static async Task<string> ERC721_TokenURI(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "tokenURI", tokenId);
        }

        // Approve a specific address to transfer a specific token
        public static async Task<ThirdwebTransactionReceipt> ERC721_Approve(this ThirdwebContract contract, IThirdwebWallet wallet, string toAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "approve", 0, toAddress, tokenId);
        }

        // Get the approved address for a specific token
        public static async Task<string> ERC721_GetApproved(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "getApproved", tokenId);
        }

        // Check if an address is an operator for another address
        public static async Task<bool> ERC721_IsApprovedForAll(this ThirdwebContract contract, string ownerAddress, string operatorAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Read<bool>(contract, "isApprovedForAll", ownerAddress, operatorAddress);
        }

        // Set or unset an operator for an owner
        public static async Task<ThirdwebTransactionReceipt> ERC721_SetApprovalForAll(this ThirdwebContract contract, IThirdwebWallet wallet, string operatorAddress, bool approved)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "setApprovalForAll", 0, operatorAddress, approved);
        }

        // Transfer a specific token from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC721_TransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "transferFrom", 0, fromAddress, toAddress, tokenId);
        }

        // Safe transfer a specific token from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC721_SafeTransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "safeTransferFrom", 0, fromAddress, toAddress, tokenId);
        }

        #endregion

        #region ERC1155

        // Check the balance of a specific token for a specific address
        public static async Task<BigInteger> ERC1155_BalanceOf(this ThirdwebContract contract, string ownerAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress, tokenId);
        }

        // Check the balance of multiple tokens for multiple addresses
        public static async Task<List<BigInteger>> ERC1155_BalanceOfBatch(this ThirdwebContract contract, string[] ownerAddresses, BigInteger[] tokenIds)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (ownerAddresses == null || tokenIds == null)
            {
                throw new ArgumentException("Owner addresses and token IDs must be provided");
            }

            return await ThirdwebContract.Read<List<BigInteger>>(contract, "balanceOfBatch", ownerAddresses, tokenIds);
        }

        // Approve a specific address to transfer specific tokens
        public static async Task<ThirdwebTransactionReceipt> ERC1155_SetApprovalForAll(this ThirdwebContract contract, IThirdwebWallet wallet, string operatorAddress, bool approved)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "setApprovalForAll", 0, operatorAddress, approved);
        }

        // Check if an address is approved to transfer specific tokens
        public static async Task<bool> ERC1155_IsApprovedForAll(this ThirdwebContract contract, string ownerAddress, string operatorAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Read<bool>(contract, "isApprovedForAll", ownerAddress, operatorAddress);
        }

        // Transfer specific tokens from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC1155_SafeTransferFrom(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            string fromAddress,
            string toAddress,
            BigInteger tokenId,
            BigInteger amount,
            byte[] data
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "safeTransferFrom", 0, fromAddress, toAddress, tokenId, amount, data);
        }

        // Transfer multiple tokens from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC1155_SafeBatchTransferFrom(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            string fromAddress,
            string toAddress,
            BigInteger[] tokenIds,
            BigInteger[] amounts,
            byte[] data
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            if (tokenIds == null || amounts == null)
            {
                throw new ArgumentException("Token IDs and amounts must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "safeBatchTransferFrom", 0, fromAddress, toAddress, tokenIds, amounts, data);
        }

        // Get the URI for a specific token
        public static async Task<string> ERC1155_URI(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "uri", tokenId);
        }

        #endregion
    }
}
