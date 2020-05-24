using System;
using System.Timers;
using System.Threading;

namespace Breakout.CircuitBreaker
{
    public interface ICircuitBreaker
    {
        void OperationSucceeded();
        void OperationFailed();
        bool IsClosed { get; }
        bool IsOpen { get; }
        bool IsHalfOpen { get; }
        event EventHandler Closed;
        event EventHandler Open;
        event EventHandler HalfOpen;
    }

    public class CircuitBreaker : ICircuitBreaker
    {
        // Configuration settings.
        private readonly long failureThreshold;
        private readonly long openTimeoutSeconds;

        // Member variables.
        // The number of consecutive failures.
        private long failureCount;

        // The timer used to determine when the open timedout event occurs.
        private System.Timers.Timer timer;

        // The counter used to determine when the open timedout event occurs.
        private long timerSecondsElapsed;

        // The three possible states: Closed, Open, HalfOpen.
        private static ICircuitBreakerState closedState = new CircuitBreakerClosedState();
        private static ICircuitBreakerState openState = new CircuitBreakerOpenState();
        private static ICircuitBreakerState halfOpenState = new CircuitBreakerHalfOpenState();

        // The current state.
        private ICircuitBreakerState state;

        // Public C# events.
        public event EventHandler Closed;
        public event EventHandler Open;
        public event EventHandler HalfOpen;

        // Locked object to prevent concurrent assignments of the current state.
        private object StateTransitionLockerObject = new object();

        // The constructor.
        public CircuitBreaker(int failureCountThreshold, int openTimeoutInSeconds)
        {
            state = closedState;
            failureCount = 0L;
            failureThreshold = failureCountThreshold;
            openTimeoutSeconds = openTimeoutInSeconds;
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += TimerElapsed;
            timerSecondsElapsed = 0L;
        }

        // The public properties.
        public bool IsClosed => this.state == closedState;
        public bool IsOpen => this.state == openState;
        public bool IsHalfOpen => this.state == halfOpenState;

        private void RestartTimer()
        {
            timer.Stop();
            Interlocked.Exchange(ref timerSecondsElapsed, 0L);
            timer.Start();
        }

        private void StopTimer()
        {
            timer.Stop();
        }

        private void TimerElapsed(object source, ElapsedEventArgs e)
        {
            long elapsedSeconds = Interlocked.Increment(ref timerSecondsElapsed);
            if (IsOpen)
            {
                if (elapsedSeconds >= openTimeoutSeconds)
                {
                    OpenTimedout();
                }
            }
        }

        // Public FSM Event - OperationSucceeded.
        // The caller is indicating to the FSM that an operation has succeeded.
        public void OperationSucceeded()
        {
            state.OperationSucceeded(this);
        }

        // Public FSM Event - OperationFailed.
        // The caller is indicating to the FSM that an operation has failed.
        public void OperationFailed()
        {
            state.OperationFailed(this);
        }

        // Private FSM Event - ReachedThreshold.
        // We have reached the threshold of failure events while in the closed state, 
        // and so we need to transition to the open state.
        private void ReachedThreshold()
        {            
            state.ReachedThreshold(this);
        }

        // Private FSM Event - OpenTimedout.
        // The open state has timed out, and so we need to transition to half-open.
        private void OpenTimedout()
        {            
            state.OpenTimedOut(this);
        }

        private void ResetCount()
        {
            Interlocked.Exchange(ref failureCount, 0L);
        }

        private void IncrementCount()
        {
            if (Interlocked.Increment(ref failureCount) >= failureThreshold)
            {
                this.ReachedThreshold();
            }
        }

        private void Reset()
        {
            lock (StateTransitionLockerObject)
            {
                // Must be from half-open.
                if (state == halfOpenState)
                {
                    Interlocked.Exchange(ref failureCount, 0L);
                    state = closedState;
                    Closed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void TripBreaker()
        {
            lock (StateTransitionLockerObject)
            {
                // Must be from closed or half-open.
                if (state != openState)
                {
                    Interlocked.Exchange(ref failureCount, 0L);
                    state = openState;
                    Open?.Invoke(this, EventArgs.Empty);
                    RestartTimer();
                }
            }
        }

        private void AttemptReset()
        {
            lock (StateTransitionLockerObject)
            {
                // Must be from open.
                if (state == openState)
                {
                    StopTimer();
                    state = halfOpenState;
                    HalfOpen?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private interface ICircuitBreakerState
        {
            void OperationSucceeded(CircuitBreaker cb);
            void OperationFailed(CircuitBreaker cb);
            void ReachedThreshold(CircuitBreaker cb);
            void OpenTimedOut(CircuitBreaker cb);
        }

        private class CircuitBreakerClosedState : ICircuitBreakerState
        {
            public void OperationSucceeded(CircuitBreaker cb)
            {
                cb.ResetCount();
            }
            public void OperationFailed(CircuitBreaker cb)
            {
                cb.IncrementCount();
            }
            public void ReachedThreshold(CircuitBreaker cb)
            {
                cb.TripBreaker();
            }
            public void OpenTimedOut(CircuitBreaker cb)
            {
            }
        }

        private class CircuitBreakerOpenState : ICircuitBreakerState
        {
            public void OperationSucceeded(CircuitBreaker cb)
            {
            }
            public void OperationFailed(CircuitBreaker cb)
            {
            }
            public void ReachedThreshold(CircuitBreaker cb)
            {
            }
            public void OpenTimedOut(CircuitBreaker cb)
            {
                cb.AttemptReset();
            }
        }

        private class CircuitBreakerHalfOpenState : ICircuitBreakerState
        {
            public void OperationSucceeded(CircuitBreaker cb)
            {
                cb.Reset();
            }
            public void OperationFailed(CircuitBreaker cb)
            {
                cb.TripBreaker();
            }
            public void ReachedThreshold(CircuitBreaker cb)
            {
            }
            public void OpenTimedOut(CircuitBreaker cb)
            {
            }
        }
    }
}
