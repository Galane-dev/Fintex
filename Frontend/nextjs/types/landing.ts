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

export interface SubscriptionPlan {
  key: "starter" | "pro" | "elite";
  title: string;
  price: string;
  cadence: string;
  description: string;
  bullets: string[];
}

export interface FooterLink {
  label: string;
  href: string;
}

export interface FooterGroup {
  title: string;
  links: FooterLink[];
}
