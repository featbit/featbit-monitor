# featbit-monitor
FeatBit Service Health Monitoring System

A monitoring system that ensures FeatBit is running correctly by periodically modifying feature flag toggles and verifying that services respond properly.

## Overview

FeatBit SaaS already utilizes various Azure services for monitoring, alerting, and notifications at the infrastructure level. This project serves as an **external monitoring solution** that complements the existing infrastructure by testing FeatBit SaaS from the user's perspective. It provides randomized end-to-end testing and alerts for potential issues that might affect actual usage of the FeatBit SaaS service.

## Components

### asp-net-monitor
An ASP.NET service that integrates the FeatBit .NET Server SDK. This service is used to evaluate feature flag values.

**Key Features:**
- Integrates FeatBit .NET Server SDK
- Provides API endpoints to retrieve feature flag values
- Serves as the target service for monitoring

**Deployment:**
- Deployed on Azure

### flag-change-trigger
A scheduled Cloudflare Worker that automatically modifies feature flag toggles and checks whether asp-net-monitor correctly evaluates them.

**Key Features:**
- Automatically modifies feature flag configurations on schedule
- Calls asp-net-monitor API to verify flag values
- Detects if asp-net-monitor responds correctly to configuration changes
- Sends monitoring result notifications

**Deployment:**
- Deployed on Cloudflare Workers
