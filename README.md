# Fintex

Fintex is organized as a two-app repository:

- `Frontend/nextjs`
- `Backend/aspnet-core`

## Layout

```text
Fintex/
  Frontend/
    nextjs/
  Backend/
    aspnet-core/
```

## Notes

- The Next.js frontend lives under `Frontend/nextjs`.
- The ASP.NET Core solution lives under `Backend/aspnet-core/Fintex.sln`.
- Existing screenshots and repo metadata stay at the repository root.
- CI/CD setup notes live in [docs/deployment-cicd.md](docs/deployment-cicd.md).
- Backend secret placeholders now live in tracked appsettings files, and the local env variable names are listed in [env.example](Backend/aspnet-core/src/Fintex.Web.Host/env.example).

## License

[MIT](LICENSE).
