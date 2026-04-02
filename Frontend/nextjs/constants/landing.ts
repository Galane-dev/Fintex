import type {
  FooterGroup,
  HeroMetric,
  LandingFeature,
  MarketSnapshot,
  NavLink,
  SubscriptionPlan,
} from "@/types/landing";

export const navLinks: NavLink[] = [
  { label: "Markets", href: "#markets" },
  { label: "Features", href: "#features" },
  { label: "Plans", href: "#plans" },
  { label: "Security", href: "#security" },
];

export const heroMetrics: HeroMetric[] = [
  { label: "Daily Volume", value: "$2.4B+" },
  { label: "Active Traders", value: "1M+" },
  { label: "Markets", value: "150+" },
];

export const landingFeatures: LandingFeature[] = [
  {
    key: "execution",
    title: "Lightning-fast execution",
    description: "Route orders through a low-latency stack built for decisive entries and exits in volatile markets.",
    icon: "FlashFilled",
  },
  {
    key: "analytics",
    title: "Advanced analytics",
    description: "Monitor trend score, confidence, momentum, MACD, RSI, and live verdicts from one workflow.",
    icon: "LineChartOutlined",
  },
  {
    key: "security",
    title: "Bank-grade security",
    description: "Protect accounts with layered access controls, session awareness, and hardened auth flows.",
    icon: "SafetyCertificateOutlined",
  },
  {
    key: "markets",
    title: "Global multi-asset access",
    description: "Track forex, crypto, metals, and synthetic watchlists inside a single unified experience.",
    icon: "GlobalOutlined",
  },
  {
    key: "automation",
    title: "Smarter trading tools",
    description: "Surface the right data faster with modular dashboards, live calculations, and role-based views.",
    icon: "RobotOutlined",
  },
  {
    key: "compliance",
    title: "Built for trust",
    description: "Clear risk communication, consistent UI feedback, and predictable auth boundaries across protected pages.",
    icon: "LockOutlined",
  },
];

export const marketSnapshots: MarketSnapshot[] = [
  { symbol: "EUR/USD", price: "1.0847", change: "+0.24%", verdict: "Buy" },
  { symbol: "GBP/USD", price: "1.2634", change: "+0.18%", verdict: "Buy" },
  { symbol: "BTC/USD", price: "67,234", change: "+2.45%", verdict: "Buy" },
  { symbol: "ETH/USD", price: "3,421", change: "+1.87%", verdict: "Buy" },
  { symbol: "GOLD", price: "2,185", change: "-0.34%", verdict: "Hold" },
  { symbol: "USD/JPY", price: "149.82", change: "-0.12%", verdict: "Sell" },
];

export const subscriptionPlans: SubscriptionPlan[] = [
  {
    key: "starter",
    title: "Starter",
    price: "$0",
    cadence: "/month",
    description: "A demo-friendly entry plan for exploring FinteX workflows and UI.",
    bullets: ["Watchlists and snapshots", "Landing + dashboard preview", "Community support"],
  },
  {
    key: "pro",
    title: "Pro",
    price: "$29",
    cadence: "/month",
    description: "For active traders who want richer analytics and automation-ready workflows.",
    bullets: ["Advanced analytics cards", "Rule-based automation", "Priority feature releases"],
  },
  {
    key: "elite",
    title: "Elite",
    price: "$99",
    cadence: "/month",
    description: "A premium plan for teams and power users who need top-tier execution visibility.",
    bullets: ["Multi-account oversight", "Automation + risk desk tooling", "Dedicated support channel"],
  },
];

export const footerGroups: FooterGroup[] = [
  {
    title: "Platform",
    links: [
      { label: "Markets", href: "#markets" },
      { label: "Features", href: "#features" },
      { label: "Plans", href: "#plans" },
      { label: "Security", href: "#security" },
    ],
  },
  {
    title: "Account",
    links: [
      { label: "Sign in", href: "/auth/sign-in" },
      { label: "Create account", href: "/auth/sign-up" },
      { label: "Dashboard", href: "/dashboard" },
    ],
  },
];
