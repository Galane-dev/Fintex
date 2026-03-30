"use client";

import { Footer } from "@/components/landing/Footer";
import { Features } from "@/components/landing/Features";
import { Hero } from "@/components/landing/Hero";
import { Navigation } from "@/components/landing/Navigation";
import { StatsSection } from "@/components/landing/StatsSection";
import { TradingPlatforms } from "@/components/landing/TradingPlatforms";
import { useStyles } from "@/components/landing/style";

function LandingPageView() {
  const { styles } = useStyles();

  return (
    <div className={styles.page}>
      <Navigation />
      <Hero />
      <Features />
      <StatsSection />
      <TradingPlatforms />
      <Footer />
    </div>
  );
}

export default function Home() {
  return <LandingPageView />;
}
