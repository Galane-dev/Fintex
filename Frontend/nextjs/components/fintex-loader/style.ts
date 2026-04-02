import { createStyles } from "antd-style";

const NEON_GREEN = "#39ff14";

export const useFintexLoaderStyles = createStyles(({ css }) => ({
  container: css`
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
  `,

  fullscreen: css`
    min-height: 100vh;
    background:
      radial-gradient(circle at top, rgba(57, 255, 20, 0.08), transparent 42%),
      #000000;
  `,

  panel: css`
    min-height: 220px;
    border-radius: 24px;
    border: 1px solid rgba(57, 255, 20, 0.16);
    background:
      radial-gradient(circle at top, rgba(57, 255, 20, 0.08), transparent 36%),
      linear-gradient(180deg, rgba(3, 8, 3, 0.96), rgba(0, 0, 0, 0.92));
  `,

  inline: css`
    min-height: 160px;
    padding: 12px 0;
  `,

  loader: css`
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 14px;
  `,

  markWrap: css`
    width: var(--fintex-loader-size, 80px);
    height: calc(var(--fintex-loader-size, 80px) * 1.25);
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
  `,

  markSvg: css`
    width: 100%;
    height: 100%;
    overflow: visible;
  `,

  markPath: css`
    fill: none;
    stroke: ${NEON_GREEN};
    stroke-width: 4;
    stroke-linecap: round;
    stroke-linejoin: round;
  `,

  animatedPath: css`
    stroke-dasharray: 300;
    stroke-dashoffset: 300;
    animation:
      fintex-loader-draw 0.7s ease-in-out infinite alternate,
      fintex-loader-glow 0.7s ease-in-out infinite alternate;

    @keyframes fintex-loader-draw {
      0% {
        stroke-dashoffset: 300;
      }

      100% {
        stroke-dashoffset: 0;
      }
    }

    @keyframes fintex-loader-glow {
      0% {
        filter: drop-shadow(0 0 2px ${NEON_GREEN}) drop-shadow(0 0 5px ${NEON_GREEN});
      }

      100% {
        filter: drop-shadow(0 0 10px ${NEON_GREEN}) drop-shadow(0 0 20px ${NEON_GREEN});
      }
    }
  `,

  label: css`
    color: ${NEON_GREEN};
    font-size: 12px;
    letter-spacing: 0.28em;
    text-transform: uppercase;
    text-align: center;
    text-shadow: 0 0 10px rgba(57, 255, 20, 0.4);
  `,

  buttonIcon: css`
    width: 16px;
    height: 16px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    line-height: 1;
  `,
}));
