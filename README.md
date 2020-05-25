# breakout

## Overview
`breakout` is a dotnet implementation of Michael Nygard's Circuit Breaker state machine, using the Gang of Four's STATE design pattern.

A circuit breaker can help you improve the stability of your application by protecting calls to third party services; e.g. a web service, a network resource, a database, or any other component which can intermittently fail.

Its a fundamental pattern for protecting your system from all manner of integration point problems. It is a way to fail fast while there is a known problem with an integration point.

The circuit breaker pattern was first described in detail by Michael Nygard in the Stability Patterns chapter of his book **"Release It!"** *Design and Deploy Production-Ready Software*.

This implementation is thread safe, lightweight and easily adapted into your existing codebase. Unlike other circuit breaker implementations, it leaves the responsibility for calling the third party service with your client code. Your code only needs to inform the circuit breaker of the success or failure of every call to the third party service, via `OperationSucceeded()` and `OperationFailed()`.

## Install

Available via [a nuget package](https://www.nuget.org/packages/Breakout.CircuitBreaker/)

PM> `Install-Package Breakout.CircuitBreaker -Version 1.0.2`

.NET CLI> `dotnet add package Breakout.CircuitBreaker --version 1.0.2`

## Example usage

```csharp
using Breakout.CircuitBreaker;

// Create the circuit breaker.
ICircuitBreaker cb = new CircuitBreaker(failureCountThreshold: 3, openTimeoutInSeconds: 5);

// Hook up to the events that inform you when the circuit breaker changes state.
cb.Closed += (source, eventArgs) => System.Console.WriteLine("Just changed to CLOSED");
cb.Open += (source, eventArgs) => System.Console.WriteLine("Just changed to OPEN");
cb.HalfOpen += (source, eventArgs) => System.Console.WriteLine("Just changed to HALF OPEN");

// Example of a long running process.
while (true)
{
    if (cb.IsOpen)
    {
        // The circuit breaker is OPEN.
        // Do not perform the call to the third party service.
        System.Console.WriteLine("The circuit breaker is OPEN. Did not perform call.");
    }
    else
    {
        // The circuit breaker is CLOSED or HALF OPEN.
        // Perform the call to the third party service.
        try
        {
            CallToThirdPartyServiceHere();

            // Success case.
            cb.OperationSucceeded();
        }
        catch
        {
            // Failure case.
            cb.OperationFailed();
        }
    }
}
```

## breakout state machine

![The Circuit Breaker state machine](/docs/circuit-breaker-state-machine.png)

Explanation:

<span style="color:blue">some *blue* text</span>
While in the CLOSED state, calls flow through as normal to the third party service.
If the operation succeeds, the failure count is reset.
If the operation fails, the failure count is incremented.
When the failure count threshold is reached, the trip breaker action is performed,
which transitions the state to OPEN.

While in the OPEN stats, no calls flow through to the third party service.
The caller just returns immediately, without performing the service call.
After the open timeout has passed, the attempt reset action is performed,
which transitions the state to HALF OPEN.

While in the HALF OPEN state, only one call is let through to the third party service.
If the operation succeeds, we reset the circuit breaker which transitions the state to CLOSED.
If the operation fails, the trip breaker action is performed, 
which transitions the state to OPEN.

## breakout UML design

The design uses the Gang of Four's STATE design pattern.

![The Circuit Breaker UML design](/docs/circuit-breaker-uml-design.png)
