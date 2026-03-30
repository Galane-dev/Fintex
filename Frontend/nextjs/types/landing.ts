export interface NavLink {
  label: string;
  href: string;
}

export interface HeroMetric {
  label: string;
  value: string;
}

export interface LandingFeature {
  key: string;
  title: string;
  description: string;
  icon:
    | "FlashFilled"
    | "LineChartOutlined"
    | "SafetyCertificateOutlined"
    | "GlobalOutlined"
    | "RobotOutlined"
    | "LockOutlined";
}

export interface MarketSnapshot {
  symbol: string;
  price: string;
  change: string;
  verdict: "Buy" | "Sell" | "Hold";
}

export interface PlatformShowcase {
  key: "web" | "mobile" | "api";
  title: string;
  description: string;
  bullets: string[];
  accent: string;
}

export interface FooterGroup {
  title: string;
  links: string[];
}
