"use client";

import { useEffect } from "react";
import { Alert } from "antd";
import type { DashboardChartProps } from "./types";
import { ChartFooter } from "./chart-footer";
import { ChartHeader } from "./chart-header";
import { useDashboardChartCanvas } from "./use-dashboard-chart-canvas";
import { useDashboardChartController } from "./use-dashboard-chart-controller";
import { useStyles } from "../style";

export type { ChartTradeOverlay } from "./types";

export const DashboardChart = (props: DashboardChartProps) => {
  const { styles } = useStyles();
  const controller = useDashboardChartController(props);
  const {
    canvasRef,
    error,
    handleMouseDown,
    handleMouseLeave,
    handleMouseMove,
    handleMouseUp,
    handleWheel,
  } = controller;

  useDashboardChartCanvas(controller);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) {
      return;
    }

    const handleNativeWheel = (event: WheelEvent) => {
      handleWheel(event);
    };

    canvas.addEventListener("wheel", handleNativeWheel, { passive: false });

    return () => {
      canvas.removeEventListener("wheel", handleNativeWheel);
    };
  }, [canvasRef, handleWheel]);

  return (
    <div className={styles.terminal}>
      <ChartHeader controller={controller} {...props} />
      {error ? <div className={styles.errorWrap}><Alert type="warning" showIcon title={error} /></div> : null}
      <div className={styles.canvasWrap}>
        <canvas
          ref={canvasRef}
          className={styles.chartCanvas}
          aria-label="Interactive BTCUSDT chart"
          onMouseMove={handleMouseMove}
          onMouseDown={handleMouseDown}
          onMouseUp={handleMouseUp}
          onMouseLeave={handleMouseLeave}
        />
      </div>
      <ChartFooter />
    </div>
  );
};
