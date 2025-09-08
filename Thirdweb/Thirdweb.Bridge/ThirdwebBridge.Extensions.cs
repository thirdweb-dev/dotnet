using System.Numerics;

namespace Thirdweb;

public static class ThirdwebBridgeExtensions
{
    #region Execution

    /// <summary>
    /// Executes buy transaction(s) and handles status polling.
    /// </summary>
    /// <param name="bridge">The Thirdweb bridge.</param>
    /// <param name="executor">The executor wallet.</param>
    /// <param name="preparedBuy">The buy data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction receipts as a list of <see cref="ThirdwebTransactionReceipt"/>.</returns>
    public static async Task<List<ThirdwebTransactionReceipt>> Execute(this ThirdwebBridge bridge, IThirdwebWallet executor, BuyPrepareData preparedBuy, CancellationToken cancellationToken = default)
    {
        return await ExecuteInternal(bridge, executor, preparedBuy.Steps, cancellationToken);
    }

    /// <summary>
    /// Executes sell transaction(s) and handles status polling.
    /// </summary>
    /// <param name="bridge">The Thirdweb bridge.</param>
    /// <param name="executor">The executor wallet.</param>
    /// <param name="preparedSell">The prepared sell data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction receipts as a list of <see cref="ThirdwebTransactionReceipt"/>.</returns>
    public static async Task<List<ThirdwebTransactionReceipt>> Execute(
        this ThirdwebBridge bridge,
        IThirdwebWallet executor,
        SellPrepareData preparedSell,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteInternal(bridge, executor, preparedSell.Steps, cancellationToken);
    }

    /// <summary>
    /// Executes a transfer transaction and handles status polling.
    /// </summary>
    /// <param name="bridge">The Thirdweb bridge.</param>
    /// <param name="executor">The executor wallet.</param>
    /// <param name="preparedTransfer">The prepared transfer data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction receipts as a list of <see cref="ThirdwebTransactionReceipt"/>.</returns>
    public static Task<List<ThirdwebTransactionReceipt>> Execute(
        this ThirdwebBridge bridge,
        IThirdwebWallet executor,
        TransferPrepareData preparedTransfer,
        CancellationToken cancellationToken = default
    )
    {
        var steps = new List<Step>() { new() { Transactions = preparedTransfer.Transactions } };
        return ExecuteInternal(bridge, executor, steps, cancellationToken);
    }

    /// <summary>
    /// Executes a set of post-onramp transactions and handles status polling.
    /// </summary>
    /// <param name="bridge">The Thirdweb bridge.</param>
    /// <param name="executor">The executor wallet.</param>
    /// <param name="preparedOnRamp">The prepared onramp data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction receipts as a list of <see cref="ThirdwebTransactionReceipt"/>.</returns>
    /// <remarks>Note: This method is used for executing transactions after an onramp process.</remarks>
    public static Task<List<ThirdwebTransactionReceipt>> Execute(this ThirdwebBridge bridge, IThirdwebWallet executor, OnrampPrepareData preparedOnRamp, CancellationToken cancellationToken = default)
    {
        return ExecuteInternal(bridge, executor, preparedOnRamp.Steps, cancellationToken);
    }

    /// <summary>
    /// Executes a set of transactions and handles status polling.
    /// </summary>
    /// /// <param name="bridge">The Thirdweb bridge.</param>
    /// <param name="executor">The executor wallet.</param>
    /// <param name="steps">The steps containing transactions to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static Task<List<ThirdwebTransactionReceipt>> Execute(this ThirdwebBridge bridge, IThirdwebWallet executor, List<Step> steps, CancellationToken cancellationToken = default)
    {
        return ExecuteInternal(bridge, executor, steps, cancellationToken);
    }

    private static async Task<List<ThirdwebTransactionReceipt>> ExecuteInternal(this ThirdwebBridge bridge, IThirdwebWallet executor, List<Step> steps, CancellationToken cancellationToken = default)
    {
        var receipts = new List<ThirdwebTransactionReceipt>();
        foreach (var step in steps)
        {
            foreach (var tx in step.Transactions)
            {
                var thirdwebTx = await tx.ToThirdwebTransaction(executor);
                var hash = await ThirdwebTransaction.Send(thirdwebTx);
                receipts.Add(await ThirdwebTransaction.WaitForTransactionReceipt(executor.Client, tx.ChainId, hash, cancellationToken));
                _ = await bridge.WaitForStatusCompletion(hash, tx.ChainId, cancellationToken);
            }
        }
        return receipts;
    }

    #endregion

    #region Helpers

    public static async Task<ThirdwebTransaction> ToThirdwebTransaction(this Transaction transaction, IThirdwebWallet executor)
    {
        return await ThirdwebTransaction.Create(
            executor,
            new ThirdwebTransactionInput(
                chainId: transaction.ChainId,
                to: transaction.To,
                value: BigInteger.Parse(string.IsNullOrEmpty(transaction.Value) ? "0" : transaction.Value),
                data: string.IsNullOrEmpty(transaction.Data) ? "0x" : transaction.Data
            )
        );
    }

    public static async Task<StatusData> WaitForStatusCompletion(this ThirdwebBridge bridge, string hash, BigInteger chainId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(hash))
        {
            throw new ArgumentNullException(nameof(hash));
        }

        if (chainId == 0)
        {
            throw new ArgumentNullException(nameof(chainId));
        }

        var status = await bridge.Status(hash, chainId);
        while (status.StatusType is StatusType.PENDING or StatusType.NOT_FOUND)
        {
            await ThirdwebTask.Delay(500, cancellationToken);
            status = await bridge.Status(hash, chainId);
        }

        if (status.StatusType is StatusType.FAILED)
        {
            throw new Exception($"Transaction with hash {hash} failed.");
        }

        return status;
    }

    public static bool IsSwapRequiredPostOnramp(this OnrampPrepareData preparedOnramp)
    {
        return preparedOnramp.Steps == null || preparedOnramp.Steps.Count == 0 || !preparedOnramp.Steps.Any(step => step.Transactions?.Count > 0);
    }

    #endregion
}
