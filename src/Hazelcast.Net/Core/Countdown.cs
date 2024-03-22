// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;

namespace Hazelcast.Core;

/// <summary>
/// Provides a set of methods and properties that you can use to run an accurate countdown.
/// </summary>
internal class Countdown
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private long _startMillis;
    private long _initialMillis;
    private long _remainingMillis;
    private bool _running;

    /// <summary>
    /// Initializes a new instance of the <see cref="Countdown"/> class.
    /// </summary>
    public Countdown()
    {
        Reset(0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Countdown"/> class.
    /// </summary>
    public Countdown(long millis)
    {
        Reset(millis);
    }

    public static Countdown StartNew(long millis) => new(millis);

    /// <summary>
    /// Gets the remaining time, in milliseconds.
    /// </summary>
    public long RemainingMilliseconds => _running ? Math.Max(0, _stopwatch.ElapsedMilliseconds - _startMillis) : _remainingMillis;

    /// <summary>
    /// Whether the countdown time has elapsed, i.e. the remaining time has reached zero.
    /// </summary>
    public bool Elapsed => (_running ? _stopwatch.ElapsedMilliseconds - _startMillis : _remainingMillis) <= 0;

    /// <summary>
    /// Resets the remaining time to the initial value and stops this instance.
    /// </summary>
    public void Reset() => Reset(_initialMillis);

    /// <summary>
    /// Resets the remaining time to the initial value and restarts this instance.
    /// </summary>
    public void Restart() => Restart(_initialMillis);

    /// <summary>
    /// Resets the remaining time to the specified value and stops this instance.
    /// </summary>
    /// <param name="millis">The new initial remaining time.</param>
    public void Reset(long millis)
    {
        _running = false;
        _remainingMillis = _initialMillis = millis;
    }

    /// <summary>
    /// Resets the remaining time to the specified value and restarts this instance.
    /// </summary>
    /// <param name="millis">The new initial remaining time.</param>
    public void Restart(long millis)
    {
        Reset(millis);
        Start();
    }

    /// <summary>
    /// Starts this instance.
    /// </summary>
    public void Start()
    {
        if (_running) return;
        _startMillis = _stopwatch.ElapsedMilliseconds;
        _running = true;
    }

    /// <summary>
    /// Stops this instance.
    /// </summary>
    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _remainingMillis -= _stopwatch.ElapsedMilliseconds - _startMillis;
    }
}
