"use client";

import { Typography } from "antd";
import { useStyles } from "../style";

export const ChartFooter = () => {
  const { styles } = useStyles();

  return (
    <div className={styles.footerBar}>
      <div className={styles.legend}>
        <span className={styles.legendItem}><span className={styles.legendDotBull} />Bull candles</span>
        <span className={styles.legendItem}><span className={styles.legendDotBear} />EMA 9</span>
        <span className={styles.legendItem}><span className={styles.legendDotSignal} />SMA 20</span>
        <span className={styles.legendItem}><span className={styles.legendDotEntry} />Entry / SL / TP</span>
        <span className={styles.legendItem}><span className={styles.legendDotSpread} />Spread</span>
      </div>
      <Typography.Text type="secondary">Wheel to zoom, drag to pan, move to inspect price and candle detail.</Typography.Text>
    </div>
  );
};
