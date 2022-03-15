namespace SharedCounter
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using MQTTnet;
    using MQTTnet.Client.Options;
    using MQTTnet.Extensions.ManagedClient;
    using SharpDeck.Connectivity;

    /// <summary>
    /// Provides methods for monitoring and persisting the shared count.
    /// </summary>
    public class Counter : IHostedService
    {
        /// <summary>
        /// The synchronization root.
        /// </summary>
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="Counter"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public Counter(IStreamDeckConnection connection)
            => Connection = connection;

        /// <summary>
        /// Occurs when the value has changed.
        /// </summary>
        public event EventHandler<int> Changed;

        /// <summary>
        /// Gets the connection to the Stream Deck.
        /// </summary>
        private IStreamDeckConnection Connection { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        private int Value { get; set; }

        /// <summary>
        /// Gets the task responsible for indicating this instance is ready.
        /// </summary>
        private TaskCompletionSource<object> Started { get; } = new TaskCompletionSource<object>();

        /// <summary>
        /// Gets the value asynchronously.
        /// </summary>
        /// <returns>The value of the counter.</returns>
        public async ValueTask<int> GetValueAsync()
        {
            await Started.Task;
            return Value;
        }

        /// <summary>
        /// Increments the count asynchronously.
        /// </summary>
        public ValueTask IncrementAsync()
            => UpdateAsync(() => Value+=3);

        /// <summary>
        /// Resets the count asynchronously.
        /// </summary>
        public ValueTask ResetAsync()
            => UpdateAsync(() => Value = 0);

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var settings = await Connection.GetGlobalSettingsAsync<GlobalSettings>();


            Value = settings.Count;
            Changed?.Invoke(this, Value);

            Started.TrySetResult(true);
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Updates the value asynchronous.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        private async ValueTask UpdateAsync(Action action)
        {
            await Started.Task;

            try
            {
                await _syncRoot.WaitAsync();

                action();
                await Connection.SetGlobalSettingsAsync(new GlobalSettings
                {
                    Count = Value
                });
            }
            finally
            {
                _syncRoot.Release();
            }

            Changed?.Invoke(this, Value);
        }
    }
}
