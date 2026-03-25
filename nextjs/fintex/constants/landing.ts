import type {
  FooterGroup,
  HeroMetric,
  LandingFeature,
  MarketSnapshot,
  NavLink,
  PlatformShowcase,
} from "@/types/landing";

export const navLinks: NavLink[] = [
  { label: "Markets", href: "#markets" },
  { label: "Features", href: "#features" },
  { label: "Platforms", href: "#platforms" },
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

export const platformShowcases: PlatformShowcase[] = [
  {
    key: "web",
    title: "Web platform",
    description: "A desktop-grade trading surface in the browser with deep charting, watchlists, and instant execution.",
    bullets: ["Workspace layouts", "Live indicator cards", "Zero-install access"],
    accent: "linear-gradient(135deg, rgba(52, 245, 197, 0.22), rgba(50, 91, 255, 0.18))",
  },
  {
    key: "mobile",
    title: "Mobile app flow",
    description: "Stay in sync with alerts, quick order tickets, and biometric-ready access patterns for trading on the move.",
    bullets: ["Push alerts", "Compact trade tickets", "Fast re-entry"],
    accent: "linear-gradient(135deg, rgba(113, 143, 255, 0.28), rgba(52, 245, 197, 0.12))",
  },
  {
    key: "api",
    title: "API and automation",
    description: "Connect real-time feeds and strategy tooling through a modular backend and future-ready trading workflows.",
    bullets: ["Market streaming", "Signal consumption", "Strategy extensibility"],
    accent: "linear-gradient(135deg, rgba(52, 245, 197, 0.16), rgba(113, 143, 255, 0.24))",
  },
];

export const footerGroups: FooterGroup[] = [
  {
    title: "Platform",
    links: ["Markets", "Trade Center", "Signals", "Research"],
  },
  {
    title: "Company",
    links: ["About", "Security", "Careers", "Contact"],
  },
  {
    title: "Resources",
    links: ["Help Center", "Trading Guide", "API Docs", "Status"],
  },
  {
    title: "Legal",
    links: ["Terms", "Privacy", "Risk Disclosure", "Compliance"],
  },
];
