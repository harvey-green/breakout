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

## The Circuit Breaker state machine

![The Circuit Breaker state machine](/docs/circuit-breaker-state-machine.png)

## The Circuit Breaker UML design

The design uses the Gang of Four's STATE design pattern.

![The Circuit Breaker UML design](/docs/circuit-breaker-uml-design.png)
