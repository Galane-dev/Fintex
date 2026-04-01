"use client";

import type { CSSProperties } from "react";
import { useFintexLoaderStyles } from "./style";

type FintexLoaderVariant = "fullscreen" | "panel" | "inline";

type FintexLoaderProps = {
  variant?: FintexLoaderVariant;
  label?: string;
  minHeight?: CSSProperties["minHeight"];
  size?: number;
  className?: string;
};

type FintexLoaderMarkProps = {
  size?: number;
  className?: string;
};

export function FintexLoader({
  variant = "inline",
  label = "Loading",
  minHeight,
  size = variant === "fullscreen" ? 80 : 72,
  className,
}: FintexLoaderProps) {
  const { styles, cx } = useFintexLoaderStyles();

  return (
    <div
      className={cx(styles.container, styles[variant], className)}
      style={{ minHeight }}
      role="status"
      aria-live="polite"
      aria-label={label}
    >
      <div
        className={styles.loader}
        style={{ ["--fintex-loader-size" as string]: `${size}px` }}
      >
        <FintexLoaderMark size={size} />
        <div className={styles.label}>{label}</div>
      </div>
    </div>
  );
}

export function FintexLoaderMark({
  size = 72,
  className,
}: FintexLoaderMarkProps) {
  const { styles, cx } = useFintexLoaderStyles();

  return (
    <span
      className={cx(styles.markWrap, className)}
      style={{ ["--fintex-loader-size" as string]: `${size}px` }}
      aria-hidden="true"
    >
      <svg
        viewBox="0 0 100 120"
        xmlns="http://www.w3.org/2000/svg"
        className={styles.markSvg}
      >
        <path
          className={cx(styles.markPath, styles.animatedPath)}
          d="M30 100 V20 H80 M30 55 H70"
        />
      </svg>
    </span>
  );
}

export function getFintexButtonLoading(isLoading: boolean) {
  if (!isLoading) {
    return false;
  }

  return {
    icon: <FintexLoaderMark size={14} />,
  };
}
