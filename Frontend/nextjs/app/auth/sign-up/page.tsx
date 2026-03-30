"use client";

import { Suspense } from "react";
import { AuthCard } from "@/components/auth/AuthCard";
import { AuthHighlights } from "@/components/auth/AuthHighlights";
import { useStyles } from "@/components/auth/style";

function SignUpPageView() {
  const { styles } = useStyles();

  return (
    <div className={styles.page}>
      <AuthHighlights />
      <Suspense fallback={null}>
        <AuthCard mode="sign-up" />
      </Suspense>
    </div>
  );
}

export default function SignUpPage() {
  return <SignUpPageView />;
}
