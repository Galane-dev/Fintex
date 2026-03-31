---
name: fintex-backend-refactor
description: Refactor custom Fintex ASP.NET Core backend code under `Backend/aspnet-core/src` into a cleaner DDD-style structure with smaller files, focused domain folders, service subfolders, readable comments, and no ABP scaffold churn. Use when reorganizing or splitting custom Investments, brokers, market-data, news, trading, profiles, repositories, or related Web.Host workers.
---

# Fintex Backend Refactor

## Overview

Refactor only the custom backend code added for Fintex trading features. Keep ABP defaults intact unless a custom feature directly depends on them.

## Structure Rules

- Keep domain entities and enums under `Fintex.Core/Investments/...` in clear subfolders such as:
  - `market-data/`
  - `paper-trading/`
  - `brokers/`
  - `news/`
  - `trading/`
  - `profiles/`
  - `events/`
- Keep application logic under `Fintex.Application/Investments/...` in service-oriented folders with supporting files nearby, for example:
  - `paper-trading/services/`
  - `paper-trading/dto/`
  - `market-data/services/`
  - `market-data/dto/`
- Keep infrastructure and EF repository code under `Fintex.EntityFrameworkCore/EntityFrameworkCore/...` in matching subfolders when custom repositories are involved.
- Keep Web.Host custom runtime code under focused folders such as:
  - `BackgroundWorkers/`
  - `Brokers/`
  - `MarketData/`
  - `Realtime/`

## File Size Rules

- Target under `300` lines per file.
- If a file grows past `300`, split by responsibility:
  - orchestration service
  - factory/builder
  - evaluator/scorer
  - mapper/normalizer
  - query helper
  - constants/options
- Prefer small helpers over deep inheritance.

## Domain Rules

- Keep entities readable and mostly state-focused.
- Move multi-step business rules into domain services or focused application helpers.
- Use descriptive method names over long inline logic.
- Add brief comments only where the intent is not obvious.

## Application Rules

- Keep app services thin orchestration layers.
- Push heavy calculations, recommendation logic, scoring, and sync/import logic into focused collaborators.
- Keep DTOs close to their feature folders.
- Prefer one interface per top-level app service.

## Refactor Workflow

1. Identify only custom feature files in the target area.
2. Measure file sizes and choose the biggest, most coupled files first.
3. Split by responsibility without changing behavior.
4. Preserve DI-friendly constructor injection and existing ABP conventions.
5. Keep folder names lower-case and dashed when adding new folders.
6. Run project builds after each major slice.

## Preferred Outcomes

- A senior backend engineer should quickly see:
  - where entities live
  - where orchestration lives
  - where calculations and scoring live
  - where infrastructure adapters live
- Files should read top-to-bottom with minimal mental jumping.
