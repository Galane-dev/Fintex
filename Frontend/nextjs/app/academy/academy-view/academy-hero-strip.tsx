"use client";

import { Card, Tag, Typography } from "antd";
import { useStyles } from "./style";

interface AcademyHeroStripProps {
  currentStep: number;
}

const STEP_LABELS = [
  "Study the lessons",
  "Pass the intro quiz",
  "Enter trade academy",
];

export const AcademyHeroStrip = ({ currentStep }: AcademyHeroStripProps) => {
  const { styles, cx } = useStyles();

  return (
    <Card className={styles.heroCard}>
      <div className={styles.heroTopRow}>
        <div className={styles.badgeRow}>
          <Tag color="blue">Required onboarding</Tag>
          <Tag color="gold">Pass score 90%+</Tag>
          <Tag color="green">Trade academy unlock</Tag>
        </div>
        <Typography.Title level={3} className={styles.heroTitle}>
          Fintex Trading Foundations
        </Typography.Title>
      </div>

      <div className={styles.heroSteps}>
        {STEP_LABELS.map((label, index) => {
          const stepNumber = index + 1;
          const isActive = currentStep === index;
          const isComplete = currentStep > index;

          return (
            <div key={label} className={cx(styles.heroStep, isActive ? styles.heroStepActive : undefined)}>
              <div className={cx(styles.heroStepNumber, isActive || isComplete ? styles.heroStepNumberActive : undefined)}>
                {stepNumber}
              </div>
              <Typography.Text className={cx(styles.heroStepLabel, isActive || isComplete ? styles.heroStepLabelActive : undefined)}>
                {label}
              </Typography.Text>
            </div>
          );
        })}
      </div>
    </Card>
  );
};
