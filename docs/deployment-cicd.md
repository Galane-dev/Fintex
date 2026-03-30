# CI/CD Setup

This repository now uses GitHub Actions for automatic deployments:

- `Backend/aspnet-core` deploys to Azure as a Docker container
- `Frontend/nextjs` deploys to Vercel through the Vercel CLI

## Backend to Azure

Workflow: `.github/workflows/deploy-backend-azure.yml`

### Required GitHub secrets

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

These are used by `azure/login` with OpenID Connect.

### Required GitHub variables

- `AZURE_ACR_NAME`
- `AZURE_WEBAPP_NAME`

### Azure requirements

Before the workflow runs successfully:

1. Create or reuse an Azure Container Registry.
2. Grant the GitHub OIDC principal permission to push to that registry.
3. Ensure the target Azure Web App is configured to run a custom container.
4. Ensure the Web App can pull from the registry, ideally with managed identity or pre-configured registry credentials.

The workflow builds the image from:

- `Backend/aspnet-core/src/Fintex.Web.Host/Dockerfile`

and pushes:

- `fintex-backend:${GITHUB_SHA}`
- `fintex-backend:latest`

## Frontend to Vercel

Workflow: `.github/workflows/deploy-frontend-vercel.yml`

### Required GitHub secrets

- `VERCEL_TOKEN`
- `VERCEL_ORG_ID`
- `VERCEL_PROJECT_ID`

### Deployment flow

1. Install dependencies
2. Lint the frontend
3. Validate the frontend Docker image build
4. Pull Vercel environment settings
5. Build with `vercel build`
6. Deploy with `vercel deploy --prebuilt --prod`

## Why Docker is different between Azure and Vercel

Azure Web App for Containers deploys a Docker image directly, so the backend workflow publishes a container image.

Vercel deploys the build output of the app rather than a Docker image. The frontend still has a Dockerfile for consistent local and CI validation, but the actual Vercel deployment is performed with the official Vercel CLI.
