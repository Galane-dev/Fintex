# CI/CD Setup

This repository now uses:

- Azure DevOps YAML pipeline for the backend container build and Azure deploy
- Vercel native Git integration for the frontend

## Backend to Azure with Azure DevOps

Pipeline file:

- `azure-pipelines.yml`

### What the backend pipeline does

1. Triggers on backend changes to `master` or `main`
2. Restores, builds, and tests the ASP.NET Core solution
3. Builds a Docker image from `Backend/aspnet-core/src/Fintex.Web.Host/Dockerfile`
4. Pushes the image to Azure Container Registry
5. Updates the Azure Web App for Containers to the new image tag

### Azure DevOps service connections you need

- Azure Resource Manager service connection
  - example variable value: `fintex-azure-rm-sc`
- Azure Container Registry service connection
  - example variable value: `fintex-acr-sc`

### Azure DevOps pipeline variables you need

- `azureServiceConnection`
- `acrServiceConnection`
- `acrLoginServer`
- `webAppName`

You can set these in the pipeline UI or a variable group. They do not need to be committed to the repository.

### Azure requirements

Before the pipeline can deploy successfully:

1. Create or reuse an Azure Container Registry.
2. Create or reuse an Azure Web App for Containers.
3. Ensure the Web App can pull from your ACR image, ideally with managed identity or preconfigured registry access.
4. Add the backend runtime settings in Azure App Service `Environment variables`.

## Frontend to Vercel

Use Vercel native Git deployment instead of GitHub Actions.

### Recommended setup

1. Import the GitHub repository into Vercel
2. Set the project root to `Frontend/nextjs`
3. Add `NEXT_PUBLIC_API_BASE_URL`
4. Let Vercel deploy automatically on pushes to your chosen production branch

## Why this setup works well

- Azure DevOps gives you a real YAML CI/CD pipeline to present
- Vercel gives you simple, reliable frontend auto-deploys without GitHub Actions billing
- Docker is still part of the backend delivery path, so your deployment remains containerized
