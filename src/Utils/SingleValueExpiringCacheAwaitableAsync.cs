using System;
using System.Threading;
using System.Threading.Tasks;

namespace SwarmUI.Utils;

/// <summary>
/// Micro-utility to cache a single value in an async-safe way.
/// Guarantees that calc-value will only be called once per expiration.
/// </summary>
public class SingleValueExpiringCacheAwaitableAsync<TValue> where TValue : class
{
    /// <summary>The time the value was last updated (<see cref="Environment.TickCount64"/>).</summary>
    public long TimeValueUpdated = 0;

    /// <summary>The current cache value (if any).</summary>
    public volatile TValue Value;

    /// <summary>After how much time delay should the value be considered expired.</summary>
    public TimeSpan ExpireTime;

    /// <summary>Function that calculates the new value asynchronously.</summary>
    public Func<Task<TValue>> CalculateValueFunc;

    /// <summary>Lock to ensure stable access for calculating the value.</summary>
    public SemaphoreSlim Lock = new(1, 1);

    public SingleValueExpiringCacheAwaitableAsync(Func<Task<TValue>> calculateValueFunc, TimeSpan expireTime)
    {
        CalculateValueFunc = calculateValueFunc;
        ExpireTime = expireTime;
    }

    /// <summary>Forces the value to immediately be considered expired.</summary>
    public void ForceExpire()
    {
        Interlocked.Exchange(ref TimeValueUpdated, 0);
    }

    /// <summary>Get the current value, either from cache or a fresh calculation.</summary>
    public async Task<TValue> GetValue()
    {
        // Try a direct read to get
        long read = Interlocked.Read(ref TimeValueUpdated);
        if (read != 0 && Environment.TickCount64 - read < ExpireTime.TotalMilliseconds)
        {
            return Value;
        }

        // But if the direct read is a cache miss, need to potentially write
        await Lock.WaitAsync();
        try
        {
            // Re-check key inside the lock to avoid unwanted recalculation
            read = Interlocked.Read(ref TimeValueUpdated);
            if (read != 0 && Environment.TickCount64 - read < ExpireTime.TotalMilliseconds)
            {
                return Value;
            }
            Value = await CalculateValueFunc();
            Interlocked.Exchange(ref TimeValueUpdated, Environment.TickCount64);
            return Value;
        }
        finally
        {
            Lock.Release();
        }
    }
}
