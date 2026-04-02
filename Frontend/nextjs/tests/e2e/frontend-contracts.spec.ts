import { expect, test } from "@playwright/test";
import { ROUTES } from "../../constants/routes";
import {
  footerGroups,
  heroMetrics,
  marketSnapshots,
  navLinks,
  subscriptionPlans,
} from "../../constants/landing";
import { getApiBaseUrl } from "../../utils/api-config";

test.describe("Frontend Contracts", () => {
  test("routes are unique and absolute", () => {
    const routeValues = Object.values(ROUTES);

    expect(routeValues.length).toBeGreaterThan(0);
    expect(new Set(routeValues).size).toBe(routeValues.length);

    for (const route of routeValues) {
      expect(route.startsWith("/")).toBeTruthy();
    }

    expect(ROUTES.signIn).toBe("/auth/sign-in");
    expect(ROUTES.signUp).toBe("/auth/sign-up");
    expect(ROUTES.dashboard).toBe("/dashboard");
  });

  test("subscription plans and nav/footer links are well-formed", () => {
    expect(subscriptionPlans.length).toBeGreaterThanOrEqual(3);
    expect(new Set(subscriptionPlans.map((plan) => plan.key)).size).toBe(
      subscriptionPlans.length,
    );

    for (const plan of subscriptionPlans) {
      expect(plan.title.length).toBeGreaterThan(0);
      expect(plan.price.startsWith("$")).toBeTruthy();
      expect(plan.bullets.length).toBeGreaterThanOrEqual(3);
    }

    expect(navLinks.length).toBeGreaterThanOrEqual(3);
    for (const link of navLinks) {
      expect(link.label.length).toBeGreaterThan(0);
      expect(link.href.length).toBeGreaterThan(0);
    }

    expect(footerGroups.length).toBeGreaterThanOrEqual(2);
    for (const group of footerGroups) {
      expect(group.title.length).toBeGreaterThan(0);
      expect(group.links.length).toBeGreaterThan(0);
    }
  });

  test("hero and market snapshot datasets are populated", () => {
    expect(heroMetrics.length).toBeGreaterThanOrEqual(3);
    for (const metric of heroMetrics) {
      expect(metric.label.length).toBeGreaterThan(0);
      expect(metric.value.length).toBeGreaterThan(0);
    }

    expect(marketSnapshots.length).toBeGreaterThanOrEqual(3);
    for (const snapshot of marketSnapshots) {
      expect(snapshot.symbol.length).toBeGreaterThan(0);
      expect(snapshot.price.length).toBeGreaterThan(0);
      expect(["Buy", "Hold", "Sell"]).toContain(snapshot.verdict);
    }
  });

  test("api base URL falls back and trims trailing slashes", () => {
    const previous = process.env.NEXT_PUBLIC_API_BASE_URL;

    try {
      delete process.env.NEXT_PUBLIC_API_BASE_URL;
      expect(getApiBaseUrl()).toBe("https://localhost:44311");

      process.env.NEXT_PUBLIC_API_BASE_URL = "https://fintex.azurewebsites.net///";
      expect(getApiBaseUrl()).toBe("https://fintex.azurewebsites.net");
    } finally {
      if (previous === undefined) {
        delete process.env.NEXT_PUBLIC_API_BASE_URL;
      } else {
        process.env.NEXT_PUBLIC_API_BASE_URL = previous;
      }
    }
  });
});
