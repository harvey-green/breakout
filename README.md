# breakout

## Overview
`breakout` is a dotnet implementation of Michael Nygard's Circuit Breaker state machine, using the Gang of Four's STATE pattern.

A circuit breaker can help you improve the stability of your application by protecting calls to third party services; e.g. a web service, a network resource, a database, or any other component which can intermittently fail.

Its a fundamental pattern for protecting your system from all manner of integration point problems. It is a way to fail fast while there is a known problem with an integration point.

The circuit breaker pattern was first described in detail by Michael Nygard in the Stability Patterns chapter of his book **"Release It!"** *Design and Deploy Production-Ready Software*.

## Install

Available via [a nuget package](https://www.nuget.org/packages/Breakout.CircuitBreaker/)

PM> `Install-Package Breakout.CircuitBreaker -Version 1.0.2`

.NET CLI> `dotnet add package Breakout.CircuitBreaker --version 1.0.2`

