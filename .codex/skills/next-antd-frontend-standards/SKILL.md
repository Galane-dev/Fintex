---
name: next-antd-frontend-standards
description: Build or refactor frontend features in this repo's Next.js app using Ant Design and the existing app patterns. Use when working in the `nextjs` folder on pages, layouts, components, providers, hooks, tables, charts, dashboards, filters, or other UI flows that should follow the team's Next.js, React hooks, TypeScript, naming, and modularity standards.
---

# Next Antd Frontend Standards

## Overview

Follow these standards whenever building or refactoring frontend code in the `nextjs` app.
Prefer clean, modular, composable React code that matches the repo's existing `antd` and `antd-style` patterns.

## Core stack

- Use Next.js conventions and preserve the app's existing structure.
- Use Ant Design for UI primitives and `antd-style` when the surrounding code already uses it.
- Use ES6 functional code and React function components.
- Follow React hook rules strictly.
- Keep folder names lowercase.
- Use `PascalCase` for React component file names.
- Use `kebab-case` for non-component file names.
- Name exported components and functions before exporting them.

## Architecture rules

- Keep code functional rather than class-based unless there is no clean alternative.
- Prefer pure functions and small utilities that are easy to test.
- Avoid mutation and prefer `const`.
- Prefer `map`, `filter`, `reduce`, `some`, and `every` over mutable loops.
- Use early returns to reduce nesting.
- Avoid duplication; extract reusable logic into `components/`, `hooks/`, `utils/`, `constants/`, and `types/`.
- Keep state close to where it is used.
- Keep components composable and focused on one UI responsibility.
- Keep contexts logically separated and avoid placing too many changing values into one provider.
- Separate state and actions in context when practical.

## Next.js standards

- Respect client and server boundaries.
- Add `"use client";` only where interactivity or hooks require it.
- Keep page-level files aligned with Next.js routing conventions already used in the app.
- Prefer built-in Next.js capabilities before adding new dependencies.
- Use dynamic imports or code splitting only when there is a clear payoff.
- Use SSR or SSG intentionally; do not add them by habit.

## React standards

- Prefer presentational, shareable components with minimal props.
- Keep business logic in hooks, providers, or small helper functions instead of large UI components.
- Use `useMemo` mainly for expensive calculations.
- Use `useCallback` and `React.memo` only when they clearly reduce unnecessary work.
- Do not optimize blindly; prefer readability unless there is measured performance pain.
- Avoid large stateful modules.

## TypeScript standards

- Avoid `any`.
- Prefer explicit types for props, DTOs, hook returns, and shared models.
- Use `unknown` instead of `any` when input is uncertain.
- Use non-null assertions only when the value is guaranteed at that point.

## Styling standards

- Follow the repo's existing visual and structural patterns.
- Prefer `antd-style` for scoped styling when that is the established pattern in nearby code.
- Avoid scattered inline styles for layout and reusable styling.
- Keep styles modular and close to the component or page they support.
- Avoid hard-coded design tokens when reusable theme values or shared styling patterns already exist.
- Preserve responsiveness for desktop and mobile.

## UI and accessibility standards

- Build for real screens and real states: loading, empty, error, success, and permission-restricted states.
- Keep accessibility in mind with semantic structure, keyboard usage, focus behavior, and readable contrast.
- Use clear boolean prop names like `isLoading`, `isDisabled`, and `hasError`.
- Prefer clear, practical APIs over overly generic component abstractions.

## File and folder placement

- Put shared UI in `components/`.
- Put shared hooks in `hooks/`.
- Put reusable functions in `utils/`.
- Put shared constants and labels in `constants/`.
- Put shared types and interfaces in `types/`.
- Keep page-specific pieces near the page unless they are reused elsewhere.

## Dependency rules

- Avoid adding third-party packages unless they are clearly justified.
- Prefer existing project libraries and patterns over introducing parallel solutions.
- Skip trivial packages that can be implemented cleanly in-house.
- Consider maintenance, community support, bundle size, and consistency before introducing a dependency.

## Existing repo pattern

When the surrounding code already uses patterns like:

- `antd`
- `antd-style`
- provider-based state
- `withAuth`
- `Can`
- custom hooks for page behavior

preserve and extend those patterns instead of replacing them with a different architecture.

## Delivery expectations

- Write clean and modular code.
- Align with Next.js standards.
- Follow hooks rules.
- Use ES6 functions.
- Keep folder names lowercase.
- Match the existing `antd` coding and styling approach shown by the user unless explicitly told to introduce a different pattern.




## ANTD usage
---
name: frontend-next-antd-design
description: Design and implement frontend UI using Next.js App Router with Ant Design and antd-style, emphasizing server components, strong typing, and modular structure.
---

## When to use this skill

Use this skill whenever you are:
- Designing or implementing **new frontend pages or components**.
- Refactoring **existing UI** to align with the project’s conventions.
- Reviewing frontend code for **consistency, type safety, and structure**.

The goal is to produce a clean, modular, responsive,strongly-typed UI that matches the project’s visual language (white and blue theme) and uses **Next.js App Router + Ant Design + antd-style**.

Before write code, come up with a plan, and let me approve it first. You should include how you are going to implement it, files you are going to add or modify and why the plan works

---

## Tech Stack & Global Constraints

1. **Framework**
   - Use **Next.js `16.1.6`**.
   - Use the **App Router** (`app/` directory) for routing.

2. **Component Model**
   - **Prefer Server Components** by default.
   - Only use **Client Components** when required (e.g., hooks like `useState`, `useEffect`, `useRouter`, `localStorage`, event handlers that depend on browser APIs, etc.).
   - Mark client components explicitly with `"use client"` at the top of the file.

3. **UI Library**
   - Use **Ant Design** components for UI building blocks.
   - Reference: https://ant.design/components/overview/

   Example pattern:
   ```tsx
   import { Button } from "antd";
   import { UserOutlined } from "@ant-design/icons";
   import { useRouter } from "next/navigation";

   "use client";

   const ProfileButton: React.FC = () => {
     const router = useRouter();

     return (
       <Button
         type="default"
         shape="circle"
         icon={<UserOutlined />}
         onClick={() => router.push("/profile")}
         aria-label="Go to profile"
       />
     );
   };

   export default ProfileButton;

   Styling

Use antd-style for styling.

Define styles in a dedicated styles.ts (or style.ts) file per component/page.

Import via: import { useStyles } from "./style/styles";

Example styles.ts:

import { createStyles, css } from "antd-style";

export const useStyles = createStyles(({ token }) => ({
  pageWrapper: css`
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: ${token.colorBgLayout};
  `,

  card: css`
    width: 420px;
    border-radius: 16px;
    box-shadow: 0 8px 40px rgba(0, 0, 0, 0.1);
    padding: 12px 8px;
    background: ${token.colorBgContainer};
  `,

  header: css`
    text-align: center;
    margin-bottom: 32px;
  `,

  title: css`
    margin-bottom: 4px;
  `,
}));

Example usage in a component:

import { Typography } from "antd";
import { useStyles } from "./style/styles";

const { Title, Text } = Typography;

const LoginHeader: React.FC = () => {
  const { styles } = useStyles();

  return (
    <div className={styles.header}>
      <Title level={3} className={styles.title}>
        Welcome back
      </Title>
      <Text type="secondary" style={{ fontSize: 14 }}>
        Sign in to your account to continue
      </Text>
    </div>
  );
};

export default LoginHeader;

Color Palette

Primary theme: white and blue.

Use Ant Design tokens / theme overrides rather than hard-coded colors where possible.

When hard-coding is necessary, prefer subtle blues for primary actions and keep backgrounds predominantly white/light.

Architecture & Code Organization

Routing

Use Next.js App Router:

Pages live under app/.

Use nested routes and layouts (app/(segment)/layout.tsx, app/(segment)/page.tsx).

Use Link and useRouter from next/navigation for navigation.

Component Decomposition

Break pages into small, focused components:

Example for a page:

app/(auth)/login/page.tsx – page entry (server component).

app/(auth)/login/LoginForm.tsx – client component with form logic.

app/(auth)/login/LoginHeader.tsx – UI-only header.

app/(auth)/login/style/styles.ts – styles.

Keep components single-responsibility:

Layout-only vs. data-fetching vs. form-logic components.

TypeScript & Types

Everything must be typed:

Use interface or type aliases for props and data models.

Avoid any unless absolutely unavoidable; prefer proper domain types.

Example:

export interface LoginFormValues {
  email: string;
  password: string;
}

interface LoginFormProps {
  onSubmit: (values: LoginFormValues) => Promise<void>;
  isLoading: boolean;
}
const LoginForm: React.FC<LoginFormProps> = ({ onSubmit, isLoading }) => {
  // ...
};

Data Fetching & Suspense

For server components, use async functions to fetch data directly from the server.

Wrap data-dependent UI in <Suspense> when appropriate:

For slower data calls.

For sections that can load independently.

Example:

import { Suspense } from "react";
import UsersTable from "./UsersTable";

const UsersPage = async () => {
  return (
    <Suspense fallback={<div>Loading users...</div>}>
      <UsersTable />
    </Suspense>
  );
};

export default UsersPage;

When using Suspense in client components, ensure data fetching supports it (e.g., React cache, appropriate hooks, or server components providing data).

Implementation Checklist

When generating or refactoring code with this skill, always ensure:

Next.js App Router

The file is placed correctly under app/ with appropriate page.tsx, layout.tsx, or segment structure.

Server vs Client

Default to server components.

Add "use client" only where needed (hooks, browser APIs, event-heavy UI).

Ant Design Usage

Use Ant Design components for layout, forms, modals, tables, etc.

Avoid reinventing standard UI elements that already exist in Ant Design.

antd-style

Styles defined in styles.ts (or similar) using createStyles.

Components import and use useStyles() and styles.* classNames.

Types & Interfaces

All props and domain objects have clear TypeScript types/interfaces.

No untyped/loosely-typed props.

Suspense

Use <Suspense> for sections depending on asynchronous data where it improves UX.

Provide simple, non-flashy fallbacks that respect the white/blue theme.

Visual Consistency

Main colors: white + blue.

Keep the look clean, modern, and consistent:

Adequate spacing

Rounded corners for cards where appropriate

Minimal inline styles (prefer antd-style and Ant Design theming)


Leverage Server-Side Rendering Wisely
While server-side rendering enhances SEO and initial page load times, it may not be necessary for all pages. Identify pages that require SSR, such as dynamic or content-heavy pages, and use Next.js’s “getServerSideProps” or “getInitialProps” functions selectively for optimal performance.

Embrace Static Site Generation (SSG)
Static Site Generation offers better performance and scalability compared to SSR for pages with static content. For pages that do not require real-time data, leverage SSG with “getStaticProps” to generate static HTML files at build time and serve them directly to users, reducing server load.

Optimize Image Loading
Images can significantly impact page load times. Use Next.js’s “Image” component, which automatically optimizes images and provides lazy loading, ensuring faster rendering and improved performance.

Code Splitting and Dynamic Imports
Take advantage of Next.js’s built-in code splitting feature to divide your application code into smaller, more manageable chunks. Use dynamic imports to load non-essential components only when needed, reducing the initial bundle size and improving overall page load times.

Minimize Third-Party Dependencies
Be cautious when adding third-party libraries and packages to your project, as they can increase the bundle size and affect performance. Opt for lightweight alternatives or, where feasible, write custom solutions to reduce dependencies.

Manage State Effectively
Select the appropriate state management solution for your project, such as React’s built-in “useState” and “useReducer” hooks or external libraries like Redux or Zustand. Keep the global state minimal, and prefer passing data between components using props whenever possible.

Opt for TypeScript Integration
Integrating TypeScript in your Next.js project provides static type-checking, reducing the chances of runtime errors and enhancing code reliability. Embrace TypeScript to improve code maintainability and collaboration within your team.

Properly Handle Error States
Handle error states gracefully by implementing custom error pages using Next.js’s “ErrorBoundary” or the “getStaticProps” and “getServerSideProps” functions. Providing users with informative error messages enhances user experience and helps identify and resolve issues quickly.

Implement Caching Strategies
Leverage caching techniques, such as HTTP caching and data caching, to reduce server requests and enhance performance. Caching can significantly improve response times for frequently requested resources.


---
name: create-provider-context
description: Scaffold a typed React Context provider with actions and reducer for CRUD operations on a specific entity, following the Recipe provider pattern (actions.tsx, context.tsx, reducer.tsx, index.tsx).
---

## When to use this skill

Use this skill whenever the user wants to create a **new Provider + Context + Actions + Reducer** for a given domain entity (e.g. Recipe, User, Product) that follows this structure:

- `actions.tsx`
- `context.tsx`
- `reducer.tsx`
- `index.tsx`

The goal is to generate a complete, strongly-typed CRUD state management layer that mirrors the **Recipe** example:

- Uses **TypeScript**
- Uses **React Context + useReducer**
- Uses **redux-actions** (`createAction`, `handleActions`)
- Talks to an API via an axios instance (`getAxiosInstace` utility)
- Exposes **custom hooks** for state and actions

---

## Overall conventions

When using this skill, follow these conventions:

1. **Entity Naming**
   - Let the entity name be `Entity` (e.g. `Recipe`, `User`, `Product`).
   - Use the **singular** for types: `IEntity`, `IEntityStateContext`, `IEntityActionContext`.
   - Use the **plural** form for arrays and collection state: `entities`, `recipes`, `users`, etc.
   - Action enums should be prefixed with the entity name, e.g. `RecipeActionEnums`, `UserActionEnums`.

2. **Files to create**

For an entity `Recipe`, create:

- `actions.tsx`
- `context.tsx`
- `reducer.tsx`
- `index.tsx`

in the target directory (e.g. `src/providers/recipe` or similar location chosen by the user).

3. **Libraries and types**
   - Use **TypeScript**.
   - Import `createAction` and `handleActions` from **redux-actions**.
   - Use `React.ReactNode` for the `children` prop of the provider.
   - Do **not** use `any`. Always create proper interfaces/types.

4. **API access**
   - Use an existing utility like `getAxiosInstace` imported from `../../utils/axiosInstance` (or whatever path matches the repo).
   - Define a `BASE_URL` constant:
     - Prefer reusing the existing pattern in the project (e.g. `import.meta.env.Backend_API_URL` or `import.meta.env.VITE_<ENTITY>_API_URL`).
   - CRUD handlers use `async` functions and `try/catch` or `.then/.catch` to call the API and dispatch actions.

---

## context.tsx – Define types and React contexts

Create a `context.tsx` file that:

1. **Defines the entity interface**

For `Recipe` (example):

```ts
export interface IRecipe {
  id: number;
  name: string;
  ingredients: string[];
  instructions: string[];
  image: string;
  rating: number;
}

For other entities, the fields must match the domain model described by the user.

Defines state context

export interface IRecipeStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  recipe?: IRecipe;
  recipes?: IRecipe[];
}

General pattern:

isPending, isSuccess, isError flags.

Optional entity and entities (or recipe / recipes) properties.

Defines actions context

export interface IRecipeActionContext {
  getRecipe: (id: number) => void;
  getRecipes: () => void;
  createRecipe: (recipe: Omit<IRecipe, "id">) => void;
  updateRecipe: (id: number, recipe: Partial<IRecipe>) => void;
  deleteRecipe: (id: number) => void;
}

Patterns:

get<Entity>(id: IdType)

get<Entities>()

create<Entity>(payloadWithoutId)

update<Entity>(id, partialPayload)

delete<Entity>(id)

Adjust types (e.g. string id) if the user specifies.

Initial states

export const INITIAL_STATE: IRecipeStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const INITIAL_ACTION_STATE: IRecipeActionContext = {
  getRecipe: () => {},
  getRecipes: () => {},
  createRecipe: () => {},
  updateRecipe: () => {},
  deleteRecipe: () => {},
};

Create contexts

import { createContext } from "react";

export const RecipeStateContext =
  createContext<IRecipeStateContext>(INITIAL_STATE);

export const RecipeActionContext =
  createContext<IRecipeActionContext | undefined>(undefined);
actions.tsx – Define action enums and action creators

Create an actions.tsx file that:

Imports

import { createAction } from "redux-actions";
import type { IRecipe, IRecipeStateContext } from "./context";

Defines the action enum

Follow this pattern for each CRUD operation:

export enum RecipeActionEnums {
  getRecipePending = "GET_RECIPE_PENDING",
  getRecipeSuccess = "GET_RECIPE_SUCCESS",
  getRecipeError = "GET_RECIPE_ERROR",

  getRecipesPending = "GET_RECIPES_PENDING",
  getRecipesSuccess = "GET_RECIPES_SUCCESS",
  getRecipesError = "GET_RECIPES_ERROR",

  createRecipePending = "CREATE_RECIPE_PENDING",
  createRecipeSuccess = "CREATE_RECIPE_SUCCESS",
  createRecipeError = "CREATE_RECIPE_ERROR",

  updateRecipePending = "UPDATE_RECIPE_PENDING",
  updateRecipeSuccess = "UPDATE_RECIPE_SUCCESS",
  updateRecipeError = "UPDATE_RECIPE_ERROR",

  deleteRecipePending = "DELETE_RECIPE_PENDING",
  deleteRecipeSuccess = "DELETE_RECIPE_SUCCESS",
  deleteRecipeError = "DELETE_RECIPE_ERROR",
}

For other entities, always adjust names consistently.

Action creators pattern

Pending actions: set flags (isPending, isSuccess, isError) accordingly.

Success actions: set flags and include entity or collection.

Error actions: set error flag.

Example (single entity):

export const getRecipePending = createAction<IRecipeStateContext>(
  RecipeActionEnums.getRecipePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getRecipeSuccess = createAction<IRecipeStateContext, IRecipe>(
  RecipeActionEnums.getRecipeSuccess,
  (recipe: IRecipe) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    recipe,
  })
);

export const getRecipeError = createAction<IRecipeStateContext>(
  RecipeActionEnums.getRecipeError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

Example (collection):

export const getRecipesPending = createAction<IRecipeStateContext>(
  RecipeActionEnums.getRecipesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getRecipesSuccess = createAction<IRecipeStateContext, IRecipe[]>(
  RecipeActionEnums.getRecipesSuccess,
  (recipes: IRecipe[]) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    recipes,
  })
);

export const getRecipesError = createAction<IRecipeStateContext>(
  RecipeActionEnums.getRecipesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

Repeat the same structure for create, update, delete.

reducer.tsx – Implement the reducer using handleActions

Create a reducer.tsx file that:

Imports

import { handleActions } from "redux-actions";
import { INITIAL_STATE } from "./context";
import type { IRecipeStateContext } from "./context";
import { RecipeActionEnums } from "./actions";

Defines reducer pattern

Use handleActions<IRecipeStateContext, IRecipeStateContext> and for each case:

Spread existing state.

Spread action.payload.

For update and delete success, adjust collection (recipes) appropriately.

Example:

export const RecipeReducer = handleActions<IRecipeStateContext, IRecipeStateContext>(
  {
    // Single Recipe
    [RecipeActionEnums.getRecipePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [RecipeActionEnums.getRecipeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [RecipeActionEnums.getRecipeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    // All Recipes
    [RecipeActionEnums.getRecipesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [RecipeActionEnums.getRecipesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [RecipeActionEnums.getRecipesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    // Create Recipe
    [RecipeActionEnums.createRecipePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [RecipeActionEnums.createRecipeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
      // Optionally push into existing collection if present
      recipes: state.recipes
        ? [...state.recipes, action.payload.recipe!]
        : action.payload.recipes ?? state.recipes,
    }),
    [RecipeActionEnums.createRecipeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    // Update Recipe
    [RecipeActionEnums.updateRecipePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [RecipeActionEnums.updateRecipeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
      recipes: state.recipes?.map((r) =>
        r.id === action.payload.recipe!.id ? action.payload.recipe! : r
      ),
    }),
    [RecipeActionEnums.updateRecipeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    // Delete Recipe
    [RecipeActionEnums.deleteRecipePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [RecipeActionEnums.deleteRecipeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
      // Optionally remove from collection & clear single
      recipes: state.recipes?.filter(
        (r) => r.id !== (action as any).meta?.id // or include id in payload/meta
      ),
      recipe: undefined,
    }),
    [RecipeActionEnums.deleteRecipeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);

For other entities, keep the same pattern but adjust property names (recipes → users, etc.) and id field type.

index.tsx – Implement the Provider and hooks

Create an index.tsx file that:

Imports

import { useContext, useReducer } from "react";
import { getAxiosInstace } from "../../utils/axiosInstance";

import {
  createRecipePending,
  createRecipeSuccess,
  createRecipeError,
  deleteRecipePending,
  deleteRecipeSuccess,
  deleteRecipeError,
  getRecipeError,
  getRecipePending,
  getRecipeSuccess,
  getRecipesError,
  getRecipesPending,
  getRecipesSuccess,
  updateRecipePending,
  updateRecipeSuccess,
  updateRecipeError,
} from "./actions";

import type { IRecipe } from "./context";
import {
  INITIAL_STATE,
  RecipeActionContext,
  RecipeStateContext,
} from "./context";
import { RecipeReducer } from "./reducer";

Define BASE_URL

Use the project’s env convention, e.g.:

const BASE_URL = import.meta.env.Backend_API_URL;
// or const BASE_URL = import.meta.env.VITE_RECIPE_API_URL;

Define the Provider component

Pattern:

export const RecipeProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(RecipeReducer, INITIAL_STATE);
  const instance = getAxiosInstace();

  const getRecipe = async (id: number) => {
    dispatch(getRecipePending());
    try {
      const response = await instance.get(`${BASE_URL}/${id}`);
      dispatch(getRecipeSuccess(response.data));
    } catch (error) {
      console.error(error);
      dispatch(getRecipeError());
    }
  };

  const getRecipes = async () => {
    dispatch(getRecipesPending());
    try {
      const response = await instance.get(BASE_URL);
      dispatch(getRecipesSuccess(response.data.recipes));
    } catch (error) {
      console.error(error);
      dispatch(getRecipesError());
    }
  };

  const createRecipe = async (recipe: Omit<IRecipe, "id">) => {
    dispatch(createRecipePending());
    try {
      const response = await instance.post(BASE_URL, recipe);
      dispatch(createRecipeSuccess(response.data));
    } catch (error) {
      console.error(error);
      dispatch(createRecipeError());
    }
  };

  const updateRecipe = async (id: number, recipe: Partial<IRecipe>) => {
    dispatch(updateRecipePending());
    try {
      const response = await instance.put(`${BASE_URL}/${id}`, recipe);
      dispatch(updateRecipeSuccess(response.data));
    } catch (error) {
      console.error(error);
      dispatch(updateRecipeError());
    }
  };

  const deleteRecipe = async (id: number) => {
    dispatch(deleteRecipePending());
    try {
      await instance.delete(`${BASE_URL}/${id}`);
      dispatch(deleteRecipeSuccess());
    } catch (error) {
      console.error(error);
      dispatch(deleteRecipeError());
    }
  };

  return (
    <RecipeStateContext.Provider value={state}>
      <RecipeActionContext.Provider
        value={{ getRecipe, getRecipes, createRecipe, updateRecipe, deleteRecipe }}
      >
        {children}
      </RecipeActionContext.Provider>
    </RecipeStateContext.Provider>
  );
};

Custom hooks

Always generate the two hooks with error checks:

export const useRecipeState = () => {
  const context = useContext(RecipeStateContext);
  if (!context) {
    throw new Error("useRecipeState must be used within a RecipeProvider");
  }
  return context;
};

export const useRecipeActions = () => {
  const context = useContext(RecipeActionContext);
  if (!context) {
    throw new Error("useRecipeActions must be used within a RecipeProvider");
  }
  return context;
};

For other entities, rename appropriately.